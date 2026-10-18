using FlyVip.Tests.Support;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Tests.Service;

public sealed class ReservationServiceTest(DatabaseFixture database) : DatabaseTestCase(database)
{
    [Fact]
    public async Task CompleteCharterChargesWholeAircraftAndTakesEverySeat()
    {
        var fleet = await Fleet();
        var userId = await Member();

        var id = await Reservations.Book(userId, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Complete);

        Assert.Equal(8100.00m, await Points.Balance(userId));
        Assert.Equal(6, await Db.ReservationSeats.CountAsync(s => s.ReservationId == id, TestContext.Current.CancellationToken));
        Assert.Equal(ReservationStatus.Confirmed, (await Reservation(id)).Status);
    }

    [Fact]
    public async Task PartialOwnerPaysForFullAircraftUpFront()
    {
        var fleet = await Fleet();
        var owner = await Member();

        await Reservations.Book(owner, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Partial, [1, 2]);

        Assert.Equal(8100.00m, await Points.Balance(owner));
    }

    [Fact]
    public async Task JoinerPaysShareOwnerIsRefundedAndRequestIsPending()
    {
        var fleet = await Fleet();
        var owner = await Member();
        var joiner = await Member();

        var ownerId = await Reservations.Book(owner, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Partial, [1, 2]);
        var joinId = await Reservations.Book(joiner, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Partial, [3, 4]);

        Assert.Equal(10700.00m, await Points.Balance(joiner));
        Assert.Equal(9400.00m, await Points.Balance(owner));
        Assert.Equal(ReservationStatus.Pending, (await Reservation(joinId)).Status);
        Assert.Equal(2600.00m, (await Reservation(ownerId)).Amount);
    }

    [Fact]
    public async Task OnlyOwnerOrAdminConfirmsRequests()
    {
        var fleet = await Fleet();
        var owner = await Member();
        var joiner = await Member();
        var stranger = await Member();

        await Reservations.Book(owner, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Partial, [1]);
        var joinId = await Reservations.Book(joiner, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Partial, [2]);

        await Assert.ThrowsAsync<DomainRuleException>(() => Reservations.Confirm(joinId, stranger, false));

        await Reservations.Confirm(joinId, owner, false);

        Assert.Equal(ReservationStatus.Confirmed, (await Reservation(joinId)).Status);
    }

    [Fact]
    public async Task SeatCannotBeBookedTwice()
    {
        var fleet = await Fleet();
        var owner = await Member();
        var joiner = await Member();

        await Reservations.Book(owner, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Partial, [1, 2]);

        await Assert.ThrowsAsync<DomainRuleException>(() => Reservations.Book(joiner, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Partial, [2]));
    }

    [Fact]
    public async Task CompleteIsRefusedOnceAircraftHasBookings()
    {
        var fleet = await Fleet();
        var owner = await Member();
        var other = await Member();

        await Reservations.Book(owner, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Partial, [1]);

        await Assert.ThrowsAsync<DomainRuleException>(() => Reservations.Book(other, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Complete));
    }

    [Fact]
    public async Task OwnerCannotJoinOwnFlight()
    {
        var fleet = await Fleet();
        var owner = await Member();

        await Reservations.Book(owner, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Partial, [1]);

        await Assert.ThrowsAsync<DomainRuleException>(() => Reservations.Book(owner, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Partial, [2]));
    }

    [Fact]
    public async Task InsufficientPointsBlocksBookingWithoutSideEffects()
    {
        var fleet = await Fleet(routeCost: 5000m);
        var userId = await Member();

        await Assert.ThrowsAsync<DomainRuleException>(() => Reservations.Book(userId, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Complete));

        Assert.Equal(12000.00m, await Points.Balance(userId));
        Assert.Equal(0, await Db.Reservations.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, await Db.ReservationSeats.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task OwnerCancellationRefundsOwnerAndEveryJoiner()
    {
        var fleet = await Fleet();
        var owner = await Member();
        var joiner = await Member();

        var ownerId = await Reservations.Book(owner, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Partial, [1, 2]);
        await Reservations.Book(joiner, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Partial, [3, 4]);

        await Reservations.Cancel(ownerId, owner, false);

        Assert.Equal(12000.00m, await Points.Balance(owner));
        Assert.Equal(12000.00m, await Points.Balance(joiner));
        Assert.Equal(0, await Db.ReservationSeats.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, await Db.Reservations.CountAsync(r => r.Status != ReservationStatus.Cancelled, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task JoinerCancellationRefundsJoinerAndRechargesOwner()
    {
        var fleet = await Fleet();
        var owner = await Member();
        var joiner = await Member();

        var ownerId = await Reservations.Book(owner, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Partial, [1, 2]);
        var joinId = await Reservations.Book(joiner, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Partial, [3, 4]);

        await Reservations.Cancel(joinId, joiner, false);

        Assert.Equal(12000.00m, await Points.Balance(joiner));
        Assert.Equal(8100.00m, await Points.Balance(owner));
        Assert.Equal(3900.00m, (await Reservation(ownerId)).Amount);
        Assert.Equal(2, await Db.ReservationSeats.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task MemberCannotCancelAnotherMembersReservation()
    {
        var fleet = await Fleet();
        var owner = await Member();
        var other = await Member();
        var id = await Reservations.Book(owner, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Complete);

        await Assert.ThrowsAsync<DomainRuleException>(() => Reservations.Cancel(id, other, false));
    }

    [Fact]
    public async Task CancelledReservationCannotBeCancelledAgain()
    {
        var fleet = await Fleet();
        var owner = await Member();
        var id = await Reservations.Book(owner, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Complete);
        await Reservations.Cancel(id, owner, false);

        await Assert.ThrowsAsync<DomainRuleException>(() => Reservations.Cancel(id, owner, false));
    }

    [Fact]
    public async Task InactiveMemberPastDatesAndSubMembersCannotBook()
    {
        var fleet = await Fleet();
        var inactive = await Member(activate: false);
        var active = await Member();
        var parent = await Member(planId: 2);
        var childId = await Members.AddSubMember(parent, new Account("kid2", "kid2@example.test", Password), new Profile { FirstName = "Kid", LastName = "Two" }, 3);

        foreach (var (userId, date) in new[] { (inactive, Tomorrow), (active, Today.AddDays(-1)), (childId, Tomorrow) })
        {
            await Assert.ThrowsAsync<DomainRuleException>(() => Reservations.Book(userId, fleet.Route, fleet.Aircraft, date, ReservationKind.Complete));
        }

        Assert.Equal(0, await Db.Reservations.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RouteMustMatchAircraft()
    {
        var fleet = await Fleet();
        var other = await Fleet();
        var userId = await Member();

        await Assert.ThrowsAsync<DomainRuleException>(() => Reservations.Book(userId, fleet.Route, other.Aircraft, Tomorrow, ReservationKind.Complete));
    }

    [Fact]
    public async Task SeatNumbersMustFitAircraft()
    {
        var fleet = await Fleet(capacity: 4);
        var userId = await Member();

        await Assert.ThrowsAsync<DomainRuleException>(() => Reservations.Book(userId, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Partial, [5]));
    }

    [Fact]
    public async Task ListingJoinsFlightDetailsAndOptionallyTheMember()
    {
        var fleet = await Fleet(capacity: 4);
        var userId = await Member();
        var id = await Reservations.Book(userId, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Complete);

        var queries = new ReservationQueries(Db);
        var plain = (await queries.ForAccount(userId)).Single(l => l.Id == id);
        var withMember = (await queries.Find(id))!;

        Assert.Equal(4, plain.Capacity);
        Assert.Null(plain.MemberCode);
        Assert.NotNull(withMember.MemberCode);
        Assert.Equal(plain.FromName, withMember.FromName);
    }

    [Fact]
    public async Task ListingKeepsFlightsOfSoftDeletedFleet()
    {
        var fleet = await Fleet();
        var userId = await Member();
        var id = await Reservations.Book(userId, fleet.Route, fleet.Aircraft, Tomorrow, ReservationKind.Complete);

        await Db.Aircrafts.Where(a => a.Id == fleet.Aircraft).ExecuteUpdateAsync(a => a.SetProperty(x => x.DeletedAt, DateTime.Now), TestContext.Current.CancellationToken);
        var listed = await new ReservationQueries(Db).All();

        Assert.Equal([id], listed.Select(l => l.Id));
    }

    private Task<Reservation> Reservation(int id) => Db.Reservations.SingleAsync(r => r.Id == id);
}
