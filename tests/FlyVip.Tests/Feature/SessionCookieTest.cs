using System.Net;
using FlyVip.Tests.Support;
using FlyVip.Web.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace FlyVip.Tests.Feature;

public sealed class SessionCookieTest(DatabaseFixture database) : DatabaseTestCase(database)
{
    private const int SessionCookieSizeGuard = 3000;

    [Fact]
    public async Task SignInPortalAndSignOutNeedOnlyTheSessionCookie()
    {
        var email = await MemberEmail();
        using var browser = new Browser(Client(handleCookies: false));

        var loginPage = await browser.Get("/login");
        var signIn = await browser.Post("/login", ("__RequestVerificationToken", Token(loginPage)), ("email", email), ("password", Password));
        var portal = await browser.Get("/portal");
        var signOut = await browser.Post("/logout", ("__RequestVerificationToken", Token(portal)));
        var signedOutSession = browser.Session;
        var loggedOutPage = await browser.Get("/login");
        var portalAfterSignOut = await browser.Get("/portal");

        Assert.Equal(HttpStatusCode.OK, loginPage.Status);
        Assert.Contains(".AspNetCore.Antiforgery.", Assert.Single(loginPage.SetCookies), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.Redirect, signIn.Status);
        Assert.Contains(".FlyVip.Auth=", Assert.Single(signIn.SetCookies), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, portal.Status);
        Assert.Equal("/login", signOut.Location);
        Assert.DoesNotContain(".FlyVip.Auth=", signedOutSession, StringComparison.Ordinal);
        Assert.Contains(".AspNetCore.Mvc.CookieTempDataProvider=", signedOutSession, StringComparison.Ordinal);
        Assert.Contains("You have successfully logged out.", loggedOutPage.Body, StringComparison.Ordinal);
        Assert.Equal("/login", portalAfterSignOut.Location);
    }

    [Fact]
    public async Task AReturnUrlIsRememberedAcrossSignIn()
    {
        var email = await MemberEmail();
        using var browser = new Browser(Client(handleCookies: false));

        var bounced = await browser.Get("/portal/points");
        var loginPage = await browser.Get("/login");
        var signIn = await browser.Post("/login", ("__RequestVerificationToken", Token(loginPage)), ("email", email), ("password", Password));

        Assert.Equal("/login", bounced.Location);
        Assert.Equal("/portal/points", signIn.Location);
    }

    [Fact]
    public async Task AnOverlongReturnUrlIsForgottenSoTheSessionCookieStaysSmall()
    {
        var email = await MemberEmail();
        using var browser = new Browser(Client(handleCookies: false));

        var bounced = await browser.Get("/portal/points?x=" + new string('a', SessionCookieSizeGuard));
        var loginPage = await browser.Get("/login");
        var signIn = await browser.Post("/login", ("__RequestVerificationToken", Token(loginPage)), ("email", email), ("password", Password));

        Assert.Equal("/login", bounced.Location);
        Assert.Equal("/", signIn.Location);
    }

    [Fact]
    public async Task AnUnchangedJarWritesNoCookie()
    {
        using var browser = await SignedInMember();
        var session = browser.Session;

        var portal = await browser.Get("/portal");

        Assert.Equal(HttpStatusCode.OK, portal.Status);
        Assert.Empty(portal.SetCookies);
        Assert.Equal(session, browser.Session);
    }

    [Fact]
    public async Task CookiesOutsideTheSessionCookieAreDropped()
    {
        using var browser = await SignedInMember();
        var authCookie = QueryHelpers.ParseQuery(browser.Session)[".FlyVip.Auth"];

        var throughSession = await browser.Get("/portal");
        var asBareCookie = await browser.Get("/portal", $".FlyVip.Auth={authCookie}");

        Assert.Equal(HttpStatusCode.OK, throughSession.Status);
        Assert.Equal("/login", asBareCookie.Location);
    }

    private static string Token(Reply page)
    {
        const string marker = "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"";

        Assert.Contains(marker, page.Body, StringComparison.Ordinal);

        var start = page.Body.IndexOf(marker, StringComparison.Ordinal) + marker.Length;

        return page.Body[start..page.Body.IndexOf('"', start)];
    }

    private async Task<string> MemberEmail()
    {
        var userId = await Member();

        return await Db.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => u.Email).SingleAsync(TestContext.Current.CancellationToken);
    }

    private async Task<Browser> SignedInMember()
    {
        var email = await MemberEmail();
        var browser = new Browser(Client(handleCookies: false));
        var loginPage = await browser.Get("/login");
        var signIn = await browser.Post("/login", ("__RequestVerificationToken", Token(loginPage)), ("email", email), ("password", Password));

        Assert.Equal(HttpStatusCode.Redirect, signIn.Status);

        return browser;
    }

    private sealed record Reply(HttpStatusCode Status, string? Location, string Body, IReadOnlyList<string> SetCookies);

    private sealed class Browser(HttpClient client) : IDisposable
    {
        public string Session { get; private set; } = "";

        public Task<Reply> Get(string path, string? cookie = null) => Send(HttpMethod.Get, path, null, cookie);

        public Task<Reply> Post(string path, params (string Key, string Value)[] fields) =>
            Send(HttpMethod.Post, path, new FormUrlEncodedContent(fields.Select(field => KeyValuePair.Create(field.Key, field.Value))), null);

        public void Dispose() => client.Dispose();

        private async Task<Reply> Send(HttpMethod method, string path, HttpContent? content, string? cookie)
        {
            using var request = new HttpRequestMessage(method, path) { Content = content };

            var header = cookie ?? (Session.Length > 0 ? $"{SessionCookieJar.CookieName}={Session}" : null);

            if (header is not null)
            {
                request.Headers.Add(HeaderNames.Cookie, header);
            }

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
            var setCookies = response.Headers.TryGetValues(HeaderNames.SetCookie, out var values) ? values.ToList() : [];

            Remember(setCookies);

            return new Reply(response.StatusCode, response.Headers.Location?.OriginalString, await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), setCookies);
        }

        private void Remember(List<string> setCookies)
        {
            Assert.All(setCookies, setCookie => Assert.StartsWith(SessionCookieJar.CookieName + "=", setCookie, StringComparison.Ordinal));

            if (setCookies.Count == 0)
            {
                return;
            }

            var session = SetCookieHeaderValue.Parse(Assert.Single(setCookies));

            Assert.InRange(session.Value.Length, 0, SessionCookieSizeGuard - 1);

            Session = session.Expires <= DateTimeOffset.UtcNow ? "" : session.Value.ToString();
        }
    }
}
