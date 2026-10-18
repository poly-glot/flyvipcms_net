using FlyVip.Web.Data.Entities;
using FlyVip.Web.Infrastructure;
using FlyVip.Web.Services;

namespace FlyVip.Tests.Unit;

public sealed class FlightListTest
{
    private static readonly DateOnly Today = new(2026, 10, 4);

    [Fact]
    public void UpcomingFlightsRunOldestFirstAndEarlierKeepsListingOrder()
    {
        var listings = new[]
        {
            Listing(1, Today.AddDays(9), ReservationStatus.Confirmed),
            Listing(2, Today.AddDays(2), ReservationStatus.Pending),
            Listing(3, Today.AddDays(-1), ReservationStatus.Confirmed),
            Listing(4, Today.AddDays(5), ReservationStatus.Cancelled),
        };

        var flights = FlightList.Of(listings, Today);

        Assert.Equal([2, 1], flights.Upcoming.Select(card => card.Listing.Id));
        Assert.Equal([3, 4], flights.Earlier.Select(card => card.Listing.Id));
        Assert.Equal(4, flights.Count);
    }

    [Fact]
    public void TodayCountsAsUpcomingUnlessCancelled()
    {
        Assert.True(Listing(1, Today, ReservationStatus.Confirmed).IsUpcoming(Today));
        Assert.False(Listing(1, Today, ReservationStatus.Cancelled).IsUpcoming(Today));
        Assert.False(Listing(1, Today.AddDays(-1), ReservationStatus.Confirmed).IsUpcoming(Today));
    }

    [Fact]
    public void LinksAndActionsAreResolvedPerListing()
    {
        var flights = FlightList.Of(
            [Listing(7, Today, ReservationStatus.Confirmed)],
            Today,
            listing => $"/flights/{listing.Id}",
            listing => [new FlightAction("Cancel", $"/flights/{listing.Id}/cancel")]);

        var card = Assert.Single(flights.Upcoming);

        Assert.Equal("/flights/7", card.Href);
        Assert.Equal("/flights/7/cancel", Assert.Single(card.Actions).Url);
    }

    [Fact]
    public void WholeAircraftHoldsEverySeatAndSharedHoldsPassengers()
    {
        Assert.Equal(6, Listing(1, Today, ReservationStatus.Confirmed, ReservationKind.Complete).SeatsHeld);
        Assert.Equal(2, Listing(1, Today, ReservationStatus.Confirmed, ReservationKind.Partial).SeatsHeld);
    }

    private static ReservationListing Listing(int id, DateOnly date, ReservationStatus status, ReservationKind kind = ReservationKind.Partial) =>
        new(id, 1, null, date, status, kind, 2, 100m, "Jet", 6, "A", "B", null, null, null);
}
