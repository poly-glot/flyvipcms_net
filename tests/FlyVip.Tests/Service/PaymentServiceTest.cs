using FlyVip.Tests.Support;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Tests.Service;

public sealed class PaymentServiceTest(DatabaseFixture database) : DatabaseTestCase(database)
{
    [Fact]
    public async Task UnpaidMemberMayOnlyPayJoiningFee()
    {
        var userId = await Member(activate: false);

        Assert.Equal([PaymentType.Joining], await Payments.AllowedTypes(userId));
    }

    [Fact]
    public async Task JoiningFeeActivatesMembershipAndSetsDueDate()
    {
        var userId = await Member(activate: false);

        await Payments.Record(userId, PaymentType.Joining, new DateOnly(2026, 11, 20), "wire", await Admin());
        var member = await Db.Members.SingleAsync(m => m.UserId == userId, TestContext.Current.CancellationToken);

        Assert.Equal(MemberStatus.Active, member.Status);
        Assert.Equal(new DateOnly(2027, 1, 5), member.NextDueOn);
        Assert.Equal(0.00m, await Points.Balance(userId));
    }

    [Fact]
    public async Task YearlyFeeCreditsWholeYearAndCreatesFourQuarters()
    {
        var userId = await Member(planId: 2, yearlyFee: 24000m);
        var memberId = await Db.Members.Where(m => m.UserId == userId).Select(m => m.Id).SingleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(24000.00m, await Points.Balance(userId));
        Assert.Equal(4, await Db.MembershipQuarters.CountAsync(q => q.Term.MemberId == memberId, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task YearlyIsRefusedWhileATermIsPartlyPaidByQuarters()
    {
        var userId = await Member(fund: false);
        await Payments.Record(userId, PaymentType.Quarterly, Today, "wire", await Admin());

        Assert.DoesNotContain(PaymentType.Yearly, await Payments.AllowedTypes(userId));

        var admin = await Admin();
        await Assert.ThrowsAsync<DomainRuleException>(() => Payments.Record(userId, PaymentType.Yearly, Today, "again", admin));
    }

    [Fact]
    public async Task SecondYearlyPrepaysTheTermStartingAfterThePreviousOne()
    {
        var userId = await Member();

        var first = await Db.MembershipTerms.OrderBy(t => t.Id).FirstAsync(TestContext.Current.CancellationToken);
        await Payments.Record(userId, PaymentType.Yearly, Today, "prepay", await Admin());

        var terms = await Db.MembershipTerms.OrderBy(t => t.Id).ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, terms.Count);
        Assert.Equal(first.EndsOn.AddDays(1), terms[1].StartsOn);
        Assert.Equal(24000.00m, await Points.Balance(userId));
    }

    [Fact]
    public async Task QuarterlyPaymentCreditsQuarterShareAndExtendsTerm()
    {
        var userId = await Member(yearlyFee: 12000m, fund: false);

        await Payments.Record(userId, PaymentType.Quarterly, Today, "wire", await Admin(), null, 2);

        Assert.Equal(6000.00m, await Points.Balance(userId));

        await Payments.Record(userId, PaymentType.Quarterly, Today, "wire", await Admin(), null, 2);

        Assert.Equal(12000.00m, await Points.Balance(userId));
        Assert.Equal(4, await Db.MembershipQuarters.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await Db.MembershipTerms.CountAsync(TestContext.Current.CancellationToken));
        Assert.Contains(PaymentType.Yearly, await Payments.AllowedTypes(userId));
    }

    [Fact]
    public async Task FixedAmountsIgnoreWhatTheClientSends()
    {
        var userId = await Member(activate: false);

        await Payments.Record(userId, PaymentType.Joining, Today, "wire", await Admin(), 1.00m);

        Assert.Equal(5000.00m, (await Db.Payments.OrderByDescending(p => p.Id).FirstAsync(p => p.UserId == userId, TestContext.Current.CancellationToken)).Amount);
    }

    [Fact]
    public async Task BonusRequiresActiveMembershipAndCreditsPoints()
    {
        var inactive = await Member(activate: false);

        Assert.DoesNotContain(PaymentType.Bonus, await Payments.AllowedTypes(inactive));

        var active = await Member(fund: false);
        await Payments.Record(active, PaymentType.Bonus, Today, "promo", await Admin(), 250m);

        Assert.Equal(250.00m, await Points.Balance(active));
    }

    [Fact]
    public async Task BuyPointsCreditsLedger()
    {
        var userId = await Member(fund: false);

        await Payments.Record(userId, PaymentType.Points, Today, "card", await Admin(), 800m);

        Assert.Equal(800.00m, await Points.Balance(userId));
    }

    [Fact]
    public async Task FreeAmountTypesRejectZero()
    {
        var userId = await Member();
        var admin = await Admin();

        await Assert.ThrowsAsync<DomainRuleException>(() => Payments.Record(userId, PaymentType.Points, Today, "card", admin, 0m));
    }

    [Fact]
    public async Task SubMembersCannotBePaidFor()
    {
        var parent = await Member(planId: 2);
        var childId = await Members.AddSubMember(parent, new Account("kid1", "kid1@example.test", Password), new Profile { FirstName = "Kid", LastName = "One" }, 3);
        var admin = await Admin();

        await Assert.ThrowsAsync<DomainRuleException>(() => Payments.Record(childId, PaymentType.Points, Today, "card", admin, 10m));
    }
}
