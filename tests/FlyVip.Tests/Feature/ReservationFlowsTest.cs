using System.Globalization;
using System.Net;
using FlyVip.Tests.Support;
using FlyVip.Web.Data.Entities;
using FlyVip.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Tests.Feature;

public sealed class ReservationFlowsTest(DatabaseFixture database) : DatabaseTestCase(database)
{
    [Fact]
    public async Task AdminBooksReservationForMember()
    {
        var (aircraft, route) = await Fleet();
        var userId = await Member();
        using var admin = await ActingAs(await Login(UserRole.Admin));

        using var response = await admin.PostAsync("/admin/reservations", Booking(route, aircraft, "complete", ("user_id", Text(userId))), TestContext.Current.CancellationToken);

        Assert.Equal(1, await Db.Reservations.CountAsync(r => r.UserId == userId, TestContext.Current.CancellationToken));
        Assert.Equal(8100m, await Points.Balance(userId));
    }

    [Fact]
    public async Task MemberBooksAndCancelsFromPortal()
    {
        var (aircraft, route) = await Fleet();
        var userId = await Member();
        using var member = await ActingAsMember(userId);

        using var booked = await member.PostAsync("/portal/reservations", Booking(route, aircraft, "complete"), TestContext.Current.CancellationToken);
        var id = await Db.Reservations.Where(r => r.UserId == userId).Select(r => r.Id).SingleAsync(TestContext.Current.CancellationToken);
        var charged = await Points.Balance(userId);

        using var cancelled = await member.PostAsync($"/portal/reservations/{id}/cancel", Form(), TestContext.Current.CancellationToken);

        Assert.Equal(8100m, charged);
        Assert.Equal(12000m, await Points.Balance(userId));
    }

    [Fact]
    public async Task SubMemberCannotBook()
    {
        var (aircraft, route) = await Fleet();
        var parent = await Member(planId: 2);
        var childId = await Members.AddSubMember(parent, new Account("kid3", "kid3@example.test", Password), new Profile { FirstName = "Kid", LastName = "Three" }, 3);
        using var child = await ActingAsMember(childId);

        using var response = await child.PostAsync("/portal/reservations", Booking(route, aircraft, "complete"), TestContext.Current.CancellationToken);

        Assert.Equal(0, await Db.Reservations.CountAsync(TestContext.Current.CancellationToken));
        Assert.Contains("Only primary members can book flights.", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), StringComparison.Ordinal);
    }

    [Fact]
    public async Task JoinerRequestsFreeSeatsAndOwnerConfirms()
    {
        var (aircraft, route) = await Fleet();
        var owner = await Member();
        var joiner = await Member();
        var flight = await Reservations.Book(owner, route, aircraft, Tomorrow, ReservationKind.Partial, [1, 2]);
        using var joinerClient = await ActingAsMember(joiner);
        using var ownerClient = await ActingAsMember(owner);

        using var conflict = await joinerClient.PostAsync("/portal/reservations", Booking(route, aircraft, "partial", ("seats[]", "2"), ("seats[]", "3")), TestContext.Current.CancellationToken);
        using var joined = await joinerClient.PostAsync("/portal/reservations", Booking(route, aircraft, "partial", ("seats[]", "3"), ("seats[]", "4")), TestContext.Current.CancellationToken);
        var request = await Db.Reservations.AsNoTracking().SingleAsync(r => r.ParentReservationId == flight, TestContext.Current.CancellationToken);
        var page = await ownerClient.GetStringAsync("/portal/reservations", TestContext.Current.CancellationToken);

        using var confirmed = await ownerClient.PostAsync($"/portal/reservations/{request.Id}/confirm", Form(), TestContext.Current.CancellationToken);
        var status = await Db.Reservations.AsNoTracking().Where(r => r.Id == request.Id).Select(r => r.Status).SingleAsync(TestContext.Current.CancellationToken);

        Assert.Contains("Seat(s) 2 already booked.", await conflict.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), StringComparison.Ordinal);
        Assert.Equal("/portal/reservations", joined.Headers.Location?.OriginalString);
        Assert.Contains($"action=\"/portal/reservations/{request.Id}/confirm\"", page, StringComparison.Ordinal);
        Assert.Contains($"action=\"/portal/reservations/{flight}/cancel\" data-confirm=\"Your points are refunded and the seats are released.\"", page, StringComparison.Ordinal);
        Assert.Equal(ReservationStatus.Confirmed, status);
    }

    [Fact]
    public async Task AdminSeesFlightDetailAndCancelsIt()
    {
        var (aircraft, route) = await Fleet();
        var owner = await Member();
        var flight = await Reservations.Book(owner, route, aircraft, Tomorrow, ReservationKind.Partial, [1, 2]);
        await Reservations.Book(await Member(), route, aircraft, Tomorrow, ReservationKind.Partial, [5]);
        using var admin = await ActingAs(await Login(UserRole.Admin));

        var index = await admin.GetStringAsync("/admin/reservations", TestContext.Current.CancellationToken);
        var detail = await admin.GetStringAsync($"/admin/reservations/{flight}", TestContext.Current.CancellationToken);
        var form = await admin.GetStringAsync("/admin/reservations/new", TestContext.Current.CancellationToken);
        using var cancelled = await admin.PostAsync($"/admin/reservations/{flight}/cancel", Form(), TestContext.Current.CancellationToken);

        Assert.Contains("2 on record", index, StringComparison.Ordinal);
        Assert.Contains("<h2>Seat requests</h2>", detail, StringComparison.Ordinal);
        Assert.Contains("data-capacity=\"6\" data-selected=\"1,2\"", detail, StringComparison.Ordinal);
        Assert.Contains("Seats held: 1, 2", detail, StringComparison.Ordinal);
        Assert.Contains($"data-capacities=\"{{&quot;{aircraft}&quot;:6}}\"", form, StringComparison.Ordinal);
        Assert.Contains("data-seatmap data-url=\"/flights/taken-seats\"", form, StringComparison.Ordinal);
        Assert.Equal($"/admin/reservations/{flight}", cancelled.Headers.Location?.OriginalString);
        Assert.Equal(12000m, await Points.Balance(owner));
    }

    [Fact]
    public async Task UnknownReservationIsNotFound()
    {
        using var admin = await ActingAs(await Login(UserRole.Admin));

        using var response = await admin.GetAsync("/admin/reservations/999", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static FormUrlEncodedContent Booking(int route, int aircraft, string kind, params (string Key, string Value)[] extra) =>
        Form([.. extra, ("route_id", Text(route)), ("aircraft_id", Text(aircraft)), ("flight_date", Tomorrow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)), ("kind", kind)]);
}
