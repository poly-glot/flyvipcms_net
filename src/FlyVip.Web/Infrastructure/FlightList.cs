using FlyVip.Web.Services;

namespace FlyVip.Web.Infrastructure;

public sealed record FlightAction(string Label, string Url, string? Title = null, string? Confirm = null);

public sealed record FlightCardModel(ReservationListing Listing, string Href, IReadOnlyList<FlightAction> Actions)
{
    public static FlightCardModel Of(ReservationListing listing, string href = "") => new(listing, href, []);
}

public sealed record FlightList(IReadOnlyList<FlightCardModel> Upcoming, IReadOnlyList<FlightCardModel> Earlier)
{
    public int Count => Upcoming.Count + Earlier.Count;

    public static FlightList Of(
        IEnumerable<ReservationListing> listings,
        DateOnly today,
        Func<ReservationListing, string>? href = null,
        Func<ReservationListing, IReadOnlyList<FlightAction>>? actions = null)
    {
        var cards = listings
            .Select(listing => new FlightCardModel(listing, href?.Invoke(listing) ?? "", actions?.Invoke(listing) ?? []))
            .ToList();

        var upcoming = cards.Where(card => card.Listing.IsUpcoming(today)).Reverse().ToList();
        var earlier = cards.Where(card => !card.Listing.IsUpcoming(today)).ToList();

        return new FlightList(upcoming, earlier);
    }
}
