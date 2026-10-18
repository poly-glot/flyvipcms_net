using System.Net;
using FlyVip.Tests.Support;
using FlyVip.Web.Data.Entities;

namespace FlyVip.Tests.Feature;

public sealed class AccessTest(DatabaseFixture database) : DatabaseTestCase(database)
{
    [Fact]
    public async Task AnonymousVisitorsAreSentToLogin()
    {
        using var client = Client();

        using var admin = await client.GetAsync("/admin", TestContext.Current.CancellationToken);
        using var portal = await client.GetAsync("/portal", TestContext.Current.CancellationToken);
        using var home = await client.GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal("/login", admin.Headers.Location?.OriginalString);
        Assert.Equal("/login", portal.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.OK, home.StatusCode);
    }

    [Fact]
    public async Task AdminReachesAdminAreaButNotThePortal()
    {
        using var admin = await ActingAs(await Login(UserRole.Admin));

        using var dashboard = await admin.GetAsync("/admin", TestContext.Current.CancellationToken);
        using var members = await admin.GetAsync("/admin/members", TestContext.Current.CancellationToken);
        using var portal = await admin.GetAsync("/portal", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);
        Assert.Equal(HttpStatusCode.OK, members.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, portal.StatusCode);
        Assert.Equal("/", portal.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(UserRole.Member)]
    [InlineData(UserRole.Submember)]
    public async Task MembersAndSubMembersCannotReachAdmin(UserRole role)
    {
        using var client = await ActingAs(await Login(role));

        using var dashboard = await client.GetAsync("/admin", TestContext.Current.CancellationToken);
        using var payments = await client.GetAsync("/admin/payments", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, dashboard.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, payments.StatusCode);
    }
}
