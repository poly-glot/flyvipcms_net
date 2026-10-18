using System.Net;
using FlyVip.Tests.Support;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Tests.Service;

public sealed class MemberServiceTest(DatabaseFixture database) : DatabaseTestCase(database)
{
    private static int subSequence;

    [Fact]
    public async Task PrimaryMemberGetsPlanPrefixedCode()
    {
        var userId = await Member(planId: 2, activate: false);
        var member = await Db.Members.SingleAsync(m => m.UserId == userId, TestContext.Current.CancellationToken);

        Assert.Equal($"FO-{userId:D3}", member.MemberCode);
        Assert.Equal($"F-{userId:D3}", member.MembershipName);
        Assert.Equal(MemberStatus.Inactive, member.Status);
    }

    [Fact]
    public async Task PersonalPlanRefusesSubMembers()
    {
        var parent = await Member(planId: 1, activate: false);

        await Assert.ThrowsAsync<DomainRuleException>(() => AddSub(parent));
    }

    [Fact]
    public async Task FamilyPlanAcceptsSubMemberWithRelationshipSuffix()
    {
        var parent = await Member(planId: 2, activate: false);
        var childId = await AddSub(parent, contactType: 2);

        var child = await Db.Members.SingleAsync(m => m.UserId == childId, TestContext.Current.CancellationToken);

        Assert.Equal($"FA-{childId:D3}", child.MemberCode);
        Assert.Equal(parent, child.ParentUserId);
        Assert.Null(child.PlanId);
    }

    [Fact]
    public async Task SubMemberLoginsAreInSubmemberGroup()
    {
        var parent = await Member(planId: 3, activate: false);
        var childId = await AddSub(parent);

        var user = await Db.Users.SingleAsync(u => u.Id == childId, TestContext.Current.CancellationToken);

        Assert.Equal(UserRole.Submember, user.Role);
    }

    [Fact]
    public async Task RemovingParentRemovesChildrenAndBansLogins()
    {
        var parent = await Member(planId: 2, activate: false);
        var childId = await AddSub(parent);

        await Members.Remove(parent);

        Assert.Equal(0, await Db.Members.CountAsync(m => m.UserId == parent || m.UserId == childId, TestContext.Current.CancellationToken));
        Assert.Contains("Member removed", (await AttemptLogin(childId)).Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BanningAndRestoringStatus()
    {
        var userId = await Member();

        await Members.SetStatus(userId, MemberStatus.Banned);
        Assert.Contains("Banned by admin", (await AttemptLogin(userId)).Body, StringComparison.Ordinal);

        await Members.SetStatus(userId, MemberStatus.Active);
        Assert.Equal(HttpStatusCode.Redirect, (await AttemptLogin(userId)).Status);
    }

    [Fact]
    public async Task PrimaryMembersExcludeSubMembersAndFilterByActiveStatus()
    {
        var parent = await Member(planId: 2, activate: false);
        var active = await Member();
        await AddSub(parent, contactType: 2);

        var all = (await Members.PrimaryMembers()).Select(m => m.UserId);
        var activeOnly = (await Members.PrimaryMembers(true)).Select(m => m.UserId);

        Assert.Equivalent(new[] { parent, active }, all, strict: true);
        Assert.Equal([active], activeOnly);
    }

    [Fact]
    public async Task PrimaryMembersCarryTheirBalanceWhenAskedWithoutPerMemberQueries()
    {
        var funded = await Member();
        var unfunded = await Member(fund: false);

        var withBalances = await Members.PrimaryMembers(withBalances: true);
        var without = await Members.PrimaryMembers();

        Assert.Equal(await Points.Balance(funded), withBalances.Single(m => m.UserId == funded).Balance);
        Assert.Equal(await Points.Balance(unfunded), withBalances.Single(m => m.UserId == unfunded).Balance);
        Assert.All(without, m => Assert.Equal(0m, m.Balance));
    }

    private Task<int> AddSub(int parent, int contactType = 1)
    {
        var n = Interlocked.Increment(ref subSequence);

        return Members.AddSubMember(parent, new Account($"sub{n}x", $"sub{n}x@example.test", Password), new Profile { FirstName = "Sub", LastName = "Member" }, contactType);
    }

    private async Task<(HttpStatusCode Status, string Body)> AttemptLogin(int userId)
    {
        var email = await Db.Users.Where(u => u.Id == userId).Select(u => u.Email).SingleAsync();
        using var client = Client();
        using var response = await client.PostAsync("/login", Form(("email", email), ("password", Password)));

        return (response.StatusCode, await response.Content.ReadAsStringAsync());
    }
}
