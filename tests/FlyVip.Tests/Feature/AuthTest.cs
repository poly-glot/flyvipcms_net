using System.Net;
using FlyVip.Tests.Support;
using FlyVip.Web.Data.Entities;

namespace FlyVip.Tests.Feature;

public sealed class AuthTest(DatabaseFixture database) : DatabaseTestCase(database)
{
    [Fact]
    public async Task LoginPageRenders()
    {
        using var client = Client();
        var html = await client.GetStringAsync("/login", TestContext.Current.CancellationToken);

        Assert.Contains("<h1>Sign in</h1>", html, StringComparison.Ordinal);
        Assert.Contains("<form class=\"form\" method=\"post\" action=\"/login\">", html, StringComparison.Ordinal);
        Assert.Contains("<input id=\"f_email\" type=\"email\" name=\"email\" value=\"\" autocomplete=\"email\" autofocus required>", html, StringComparison.Ordinal);
        Assert.Contains("<input id=\"f_password\" type=\"password\" name=\"password\" autocomplete=\"current-password\" required>", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WrongPasswordReRendersWithError()
    {
        var user = await Login(UserRole.Member);
        using var client = Client();

        using var response = await client.PostAsync("/login", Form(("email", user.Email), ("password", "not-my-password")), TestContext.Current.CancellationToken);
        var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Unable to log you in. Please check your password.", html, StringComparison.Ordinal);
        Assert.Contains($"value=\"{user.Email}\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnknownEmailIsRefused()
    {
        using var client = Client();

        using var response = await client.PostAsync("/login", Form(("email", "nobody@example.test"), ("password", "whatever-it-is")), TestContext.Current.CancellationToken);

        Assert.Contains("Unable to log you in. Please check your credentials.", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvalidLoginFormShowsInlineErrors()
    {
        using var client = Client();

        using var response = await client.PostAsync("/login", Form(("email", "not-an-email"), ("password", "")), TestContext.Current.CancellationToken);
        var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("2 things need your attention", html, StringComparison.Ordinal);
        Assert.Contains("The Email Address field must contain a valid email address.", html, StringComparison.Ordinal);
        Assert.Contains("The Password field is required.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BannedMemberIsRefused()
    {
        var user = await Login(UserRole.Member);
        Db.Members.Add(new Member { UserId = user.Id, MemberCode = "PO-001", MembershipName = "PO-001", Status = MemberStatus.Banned });
        await Db.SaveChangesAsync(TestContext.Current.CancellationToken);
        using var client = Client();

        using var response = await client.PostAsync("/login", Form(("email", user.Email), ("password", Password)), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Banned by admin", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), StringComparison.Ordinal);
    }

    [Fact]
    public async Task HomeRedirectsSignedInUsersByGroup()
    {
        using var admin = await ActingAs(await Login(UserRole.Admin));
        using var member = await ActingAs(await Login(UserRole.Member));

        using var adminHome = await admin.GetAsync("/", TestContext.Current.CancellationToken);
        using var memberHome = await member.GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal("/admin", adminHome.Headers.Location?.OriginalString);
        Assert.Equal("/portal", memberHome.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task RegistrationIsDisabled()
    {
        using var client = Client();

        using var response = await client.GetAsync("/register", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task LogoutIsRoutedAsPost()
    {
        using var client = await ActingAs(await Login(UserRole.Member));

        using var response = await client.PostAsync("/logout", Form(), TestContext.Current.CancellationToken);
        using var account = await client.GetAsync("/account/password", TestContext.Current.CancellationToken);

        Assert.Equal("/login", response.Headers.Location?.OriginalString);
        Assert.Equal("/login", account.Headers.Location?.OriginalString);
    }
}
