using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace FlyVip.Web.Infrastructure;

public sealed class SessionCookieJar(RequestDelegate next, TimeProvider clock)
{
    public const string CookieName = "__session";

    public static Dictionary<string, StringValues> Unpack(StringValues cookieHeader)
    {
        var session = CookieHeaderValue.ParseList(cookieHeader).FirstOrDefault(cookie => cookie.Name == CookieName);

        return session is null ? [] : QueryHelpers.ParseQuery(session.Value.ToString());
    }

    public static Dictionary<string, StringValues> Apply(Dictionary<string, StringValues> jar, IEnumerable<SetCookieHeaderValue> setCookies, DateTimeOffset now)
    {
        var applied = new Dictionary<string, StringValues>(jar, jar.Comparer);

        foreach (var cookie in setCookies)
        {
            var name = cookie.Name.ToString();

            if (IsExpired(cookie, now))
            {
                applied.Remove(name);
            }
            else
            {
                applied[name] = cookie.Value.ToString();
            }
        }

        return applied;
    }

    public static string Pack(Dictionary<string, StringValues> jar) =>
        string.Join('&', jar
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .SelectMany(pair => pair.Value.Select(value => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(value ?? "")}")));

    public async Task InvokeAsync(HttpContext context)
    {
        var requested = Unpack(context.Request.Headers.Cookie);

        context.Response.OnStarting(() => Commit(context, requested));

        if (requested.Count == 0)
        {
            context.Request.Headers.Remove(HeaderNames.Cookie);
        }
        else
        {
            context.Request.Headers.Cookie = ToCookieHeader(requested);
        }

        await next(context);
    }

    private Task Commit(HttpContext context, Dictionary<string, StringValues> requested)
    {
        var headers = context.Response.Headers;
        var current = Apply(requested, SetCookieHeaderValue.ParseList(headers.SetCookie), clock.GetUtcNow());
        var packed = Pack(current);

        headers.Remove(HeaderNames.SetCookie);

        if (packed != Pack(requested))
        {
            headers.SetCookie = Issue(packed, current.Count == 0, context.Request.IsHttps);
        }

        return Task.CompletedTask;
    }

    private static bool IsExpired(SetCookieHeaderValue cookie, DateTimeOffset now) =>
        cookie.MaxAge <= TimeSpan.Zero || cookie.Expires <= now;

    private static string ToCookieHeader(Dictionary<string, StringValues> jar) =>
        string.Join("; ", jar.Select(pair => $"{pair.Key}={pair.Value}"));

    private static string Issue(string value, bool expired, bool secure) =>
        new SetCookieHeaderValue(CookieName, value)
        {
            Expires = expired ? DateTimeOffset.UnixEpoch : null,
            HttpOnly = true,
            Path = "/",
            SameSite = Microsoft.Net.Http.Headers.SameSiteMode.Lax,
            Secure = secure,
        }.ToString();
}
