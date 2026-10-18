using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace FlyVip.Web.Data;

public static class Menus
{
    public static async Task<IReadOnlyList<KeyValuePair<string, string>>> AircraftTypeMenu(this AppDbContext db) =>
        await db.AircraftTypes.OrderBy(t => t.Name).Select(t => Option(t.Id, t.Name)).ToListAsync();

    public static async Task<IReadOnlyList<KeyValuePair<string, string>>> AircraftMenu(this AppDbContext db) =>
        await db.Aircrafts.OrderBy(a => a.Name).Select(a => Option(a.Id, a.Name)).ToListAsync();

    public static async Task<IReadOnlyList<KeyValuePair<string, string>>> AirportMenu(this AppDbContext db) =>
        await db.Airports.OrderBy(a => a.Name).Select(a => Option(a.Id, a.Name)).ToListAsync();

    public static async Task<IReadOnlyList<KeyValuePair<string, string>>> PilotMenu(this AppDbContext db) =>
        await db.Pilots.OrderBy(p => p.LastName).Select(p => Option(p.Id, p.FirstName + " " + p.LastName)).ToListAsync();

    public static async Task<IReadOnlyList<KeyValuePair<string, string>>> RouteMenu(this AppDbContext db)
    {
        var airports = await db.Airports.ToDictionaryAsync(a => a.Id, a => a.Name);
        var routes = await db.AirRoutes.OrderBy(r => r.Id).Select(r => new { r.Id, r.FromAirportId, r.ToAirportId }).ToListAsync();

        return [.. routes.Select(r => Option(r.Id, $"{airports.GetValueOrDefault(r.FromAirportId, "?")} → {airports.GetValueOrDefault(r.ToAirportId, "?")}"))];
    }

    public static KeyValuePair<string, string> Option(int id, string label) =>
        KeyValuePair.Create(id.ToString(CultureInfo.InvariantCulture), label);
}
