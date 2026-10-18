using System.Globalization;
using System.Text.Json;
using FlyVip.Web.Data;
using FlyVip.Web.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Services;

public sealed record BookingChoices(
    IReadOnlyList<KeyValuePair<string, string>> Routes,
    IReadOnlyList<KeyValuePair<string, string>> Aircrafts,
    string Capacities,
    string EarliestFlightDate);

public sealed class BookingQueries(AppDbContext db, TimeProvider clock)
{
    public async Task<BookingChoices> Choices() => new(
        await db.RouteMenu(),
        await db.AircraftMenu(),
        JsonSerializer.Serialize(await db.Aircrafts.OrderBy(a => a.Id).ToDictionaryAsync(a => a.Id, a => a.PassengerCapacity)),
        clock.Today().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}
