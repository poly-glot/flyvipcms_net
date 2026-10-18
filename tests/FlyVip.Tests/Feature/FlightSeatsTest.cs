using System.Globalization;
using System.Net;
using FlyVip.Tests.Support;
using FlyVip.Web.Data.Entities;

namespace FlyVip.Tests.Feature;

public sealed class FlightSeatsTest(DatabaseFixture database) : DatabaseTestCase(database)
{
    [Fact]
    public async Task ReturnsSeatsAlreadyBookedOnTheFlight()
    {
        var (aircraft, route) = await Fleet();
        var owner = await Member();
        var viewer = await Member();
        await Reservations.Book(owner, route, aircraft, Tomorrow, ReservationKind.Partial, [1, 3]);
        using var client = await ActingAsMember(viewer);

        using var response = await client.GetAsync(TakenSeats(Text(aircraft), Iso(Tomorrow)), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("{\"taken\":[1,3]}", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task OtherFlightsAndDatesDoNotLeakSeats()
    {
        var (aircraft, route) = await Fleet();
        var owner = await Member();
        await Reservations.Book(owner, route, aircraft, Tomorrow, ReservationKind.Partial, [2]);
        using var client = await ActingAsMember(owner);

        using var response = await client.GetAsync(TakenSeats(Text(aircraft), Iso(Today.AddDays(9))), TestContext.Current.CancellationToken);

        Assert.Equal("{\"taken\":[]}", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RejectsMalformedQuery()
    {
        using var client = await ActingAsMember(await Member());

        using var response = await client.GetAsync(TakenSeats("x", "tomorrow"), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("{\"taken\":[]}", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AnonymousVisitorsAreSentToLogin()
    {
        using var client = Client();

        using var response = await client.GetAsync(TakenSeats("1", Iso(Tomorrow)), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private static string TakenSeats(string aircraftId, string flightDate) => $"/flights/taken-seats?aircraft_id={aircraftId}&flight_date={flightDate}";

    private static string Iso(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
