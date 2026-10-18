using FlyVip.Tests.Support;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Tests.Service;

public sealed class PointsServiceTest(DatabaseFixture database) : DatabaseTestCase(database)
{
    [Fact]
    public async Task BalanceIsLastLedgerEntry()
    {
        var userId = await Member(activate: false);

        Assert.Equal(0.00m, await Points.Balance(userId));

        await Points.Credit(userId, 100m, LedgerKind.Adjustment, "seed");
        await Points.Debit(userId, 30.50m, LedgerKind.Booking, "spend");

        Assert.Equal(69.50m, await Points.Balance(userId));
    }

    [Fact]
    public async Task DebitBeyondBalanceIsRefusedAndLeavesNoEntry()
    {
        var userId = await Member(activate: false);
        await Points.Credit(userId, 10m, LedgerKind.Adjustment, "seed");

        await Assert.ThrowsAsync<DomainRuleException>(() => Points.Debit(userId, 10.01m, LedgerKind.Booking, "too much"));

        Assert.Equal(10.00m, await Points.Balance(userId));
        Assert.Equal(1, await Db.PointsLedger.CountAsync(e => e.UserId == userId, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ForcedDebitMayOverdraw()
    {
        var userId = await Member(activate: false);

        Assert.Equal(-5.00m, await Points.Debit(userId, 5m, LedgerKind.Booking, "forced", allowOverdraft: true));
    }

    [Fact]
    public async Task TransferMovesPointsBetweenMembers()
    {
        var sender = await Member();
        var receiver = await Member(activate: false);

        await Points.Transfer(sender, receiver, 2500m, await Admin());

        Assert.Equal(9500.00m, await Points.Balance(sender));
        Assert.Equal(2500.00m, await Points.Balance(receiver));
    }

    [Fact]
    public async Task TransferRejectsSameMemberAndOverdraft()
    {
        var sender = await Member();
        var admin = await Admin();

        await Assert.ThrowsAsync<DomainRuleException>(() => Points.Transfer(sender, sender, 1m, admin));
    }

    [Fact]
    public async Task TransferBeyondBalanceChangesNothing()
    {
        var sender = await Member();
        var receiver = await Member(activate: false);
        var admin = await Admin();

        await Assert.ThrowsAsync<DomainRuleException>(() => Points.Transfer(sender, receiver, 99999m, admin));

        Assert.Equal(12000.00m, await Points.Balance(sender));
        Assert.Equal(0.00m, await Points.Balance(receiver));
    }
}
