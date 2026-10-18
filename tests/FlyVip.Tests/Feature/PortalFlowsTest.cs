using FlyVip.Tests.Support;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Tests.Feature;

public sealed class PortalFlowsTest(DatabaseFixture database) : DatabaseTestCase(database)
{
    [Fact]
    public async Task MemberSeesOwnBalanceOnDashboard()
    {
        using var member = await ActingAsMember(await Member());

        var html = await member.GetStringAsync("/portal", TestContext.Current.CancellationToken);

        Assert.Contains("12,000.00", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SubMemberSeesParentAccount()
    {
        var parent = await Member(planId: 2);
        var childId = await Members.AddSubMember(parent, new Account("kid3", "kid3@example.test", Password), new Profile { FirstName = "Kid", LastName = "Three" }, 3);
        using var child = await ActingAsMember(childId);

        var html = await child.GetStringAsync("/portal", TestContext.Current.CancellationToken);

        Assert.Contains("12,000.00", html, StringComparison.Ordinal);
        Assert.Contains("view-only access", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MemberUpdatesOwnProfileButNotOthers()
    {
        var mine = await Member();
        var other = await Member();
        using var client = await ActingAsMember(mine);

        using var response = await client.PostAsync("/portal/profile", Form(("city", "Valencia"), ("user_id", Text(other))), TestContext.Current.CancellationToken);

        Assert.Equal("/portal/profile", response.Headers.Location?.OriginalString);
        Assert.Equal("Valencia", await Db.Profiles.AsNoTracking().Where(p => p.UserId == mine).Select(p => p.City).SingleAsync(TestContext.Current.CancellationToken));
        Assert.NotEqual("Valencia", await Db.Profiles.AsNoTracking().Where(p => p.UserId == other).Select(p => p.City).SingleAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SubMemberCannotEditProfile()
    {
        var parent = await Member(planId: 2);
        var childId = await Members.AddSubMember(parent, new Account("kid4", "kid4@example.test", Password), new Profile { FirstName = "Kid", LastName = "Four" }, 3);
        using var child = await ActingAsMember(childId);

        using var response = await child.PostAsync("/portal/profile", Form(("city", "Valencia")), TestContext.Current.CancellationToken);

        Assert.Equal("/portal", response.Headers.Location?.OriginalString);
        Assert.Null(await Db.Profiles.AsNoTracking().Where(p => p.UserId == childId).Select(p => p.City).SingleAsync(TestContext.Current.CancellationToken));
    }
}
