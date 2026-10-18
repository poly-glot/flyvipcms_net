using FlyVip.Web.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace FlyVip.Tests.Unit;

public sealed class SessionCookieJarTest
{
    private static readonly DateTimeOffset Now = new(2026, 10, 18, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void PackedCookiesUnpackUnchanged()
    {
        var jar = new Dictionary<string, StringValues>
        {
            [".FlyVip.Auth"] = "CfDJ8-_%2Bx%3D",
            [".AspNetCore.Antiforgery.wV3PD0Cx6Pc"] = "CfDJ8_abc",
            [".AspNetCore.Mvc.CookieTempDataProvider"] = "",
        };

        var unpacked = SessionCookieJar.Unpack($"bare=1; {SessionCookieJar.CookieName}={SessionCookieJar.Pack(jar)}");

        Assert.Equal(jar, unpacked);
    }

    [Theory]
    [InlineData("a=1; max-age=0", false)]
    [InlineData("a=1; max-age=-5", false)]
    [InlineData("a=1; expires=Thu, 01 Jan 1970 00:00:00 GMT", false)]
    [InlineData("a=1; max-age=60", true)]
    [InlineData("a=1; expires=Fri, 01 Jan 2100 00:00:00 GMT", true)]
    [InlineData("a=1; path=/; httponly", true)]
    public void OnlyUnexpiredCookiesStayInTheJar(string setCookie, bool kept)
    {
        var jar = new Dictionary<string, StringValues> { ["a"] = "0" };

        var applied = SessionCookieJar.Apply(jar, SetCookieHeaderValue.ParseList([setCookie]), Now);

        Assert.Equal(kept ? "1" : null, applied.TryGetValue("a", out var value) ? value.ToString() : null);
    }

    [Fact]
    public void CookiesApplyInHeaderOrderWithoutTouchingTheInput()
    {
        var jar = new Dictionary<string, StringValues> { ["kept"] = "k", ["gone"] = "g", ["swapped"] = "old" };
        var setCookies = SetCookieHeaderValue.ParseList(["gone=; max-age=0", "swapped=new; path=/", "fresh=1", "fresh=2"]);

        var applied = SessionCookieJar.Apply(jar, setCookies, Now);

        Assert.Equal(new Dictionary<string, StringValues> { ["kept"] = "k", ["swapped"] = "new", ["fresh"] = "2" }, applied);
        Assert.Equal(new Dictionary<string, StringValues> { ["kept"] = "k", ["gone"] = "g", ["swapped"] = "old" }, jar);
    }

    [Theory]
    [InlineData("bare=1; other=2", "")]
    [InlineData("bare=1; __session=; other=2", "")]
    [InlineData("bare=1; __session=a=1&b=2; other=2", "a=1; b=2")]
    public async Task RequestsSeeOnlyTheCookiesInsideTheSessionCookie(string cookieHeader, string expected)
    {
        using var host = await Serve(context => context.Response.WriteAsync(context.Request.Headers.Cookie.ToString()));
        using var client = host.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
        request.Headers.Add(HeaderNames.Cookie, cookieHeader);

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(expected, await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ResponsesCarryOneSecureSessionCookie()
    {
        using var host = await Serve(context =>
        {
            context.Response.Cookies.Append("second", "two words");
            context.Response.Cookies.Append("first", "1");

            return Task.CompletedTask;
        });
        using var client = host.GetTestClient();

        using var response = await client.GetAsync(new Uri("https://localhost/"), TestContext.Current.CancellationToken);

        Assert.Equal("__session=first=1&second=two%2520words; path=/; secure; samesite=lax; httponly", Assert.Single(response.Headers.GetValues(HeaderNames.SetCookie)));
    }

    [Fact]
    public async Task ACraftedSessionCookieStillGetsAValidCookieBack()
    {
        using var host = await Serve(context =>
        {
            context.Response.Cookies.Append("fresh", "1");

            return Task.CompletedTask;
        });
        using var client = host.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
        request.Headers.Add(HeaderNames.Cookie, "__session=x%3D%3B%20.FlyVip.Auth%3Dforged");

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal("__session=fresh=1&x%3D%3B%20.FlyVip.Auth%3Dforged=; path=/; samesite=lax; httponly", Assert.Single(response.Headers.GetValues(HeaderNames.SetCookie)));
    }

    [Fact]
    public async Task CookiesAppendedWhenTheResponseStartsAreCollected()
    {
        using var host = await Serve(context =>
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Cookies.Append("late", "1");

                return Task.CompletedTask;
            });

            return context.Response.WriteAsync("body");
        });
        using var client = host.GetTestClient();

        using var response = await client.GetAsync(new Uri("http://localhost/"), TestContext.Current.CancellationToken);

        Assert.Equal("__session=late=1; path=/; samesite=lax; httponly", Assert.Single(response.Headers.GetValues(HeaderNames.SetCookie)));
    }

    [Fact]
    public async Task AJarThatBecomesEmptyExpiresTheSessionCookie()
    {
        using var host = await Serve(context =>
        {
            context.Response.Cookies.Delete("only");

            return Task.CompletedTask;
        });
        using var client = host.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
        request.Headers.Add(HeaderNames.Cookie, "__session=only=1");

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal("__session=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; samesite=lax; httponly", Assert.Single(response.Headers.GetValues(HeaderNames.SetCookie)));
    }

    [Fact]
    public async Task AnUnchangedJarWritesNoSetCookie()
    {
        using var host = await Serve(context =>
        {
            context.Response.Cookies.Append("only", "1");
            context.Response.Cookies.Delete("absent");

            return Task.CompletedTask;
        });
        using var client = host.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
        request.Headers.Add(HeaderNames.Cookie, "__session=only=1");

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.False(response.Headers.Contains(HeaderNames.SetCookie));
    }

    private static async Task<IHost> Serve(RequestDelegate handler) =>
        await new HostBuilder()
            .ConfigureWebHost(web => web
                .UseTestServer()
                .ConfigureServices(services => services.AddSingleton(TimeProvider.System))
                .Configure(app => app.UseMiddleware<SessionCookieJar>().Run(handler)))
            .StartAsync(TestContext.Current.CancellationToken);
}
