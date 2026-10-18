using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlyVip.Web.Infrastructure;

public sealed record NavItem(string Path, string Label, string Icon);

public sealed record NavGroup(string Label, IReadOnlyList<NavItem> Items);

public static class UiHelpers
{
    private static readonly IReadOnlyList<NavGroup> MemberNav =
    [
        new("Overview", [new("portal", "Overview", "home")]),
        new("Flying", [new("portal/reservations", "Reservations", "ticket"), new("portal/points", "Points", "star")]),
        new("Account", [new("portal/profile", "Profile", "user"), new("account/password", "Password", "lock")]),
    ];

    private static readonly IReadOnlyList<NavGroup> AdminNav =
    [
        new("Overview", [new("admin", "Dashboard", "home")]),
        new("Club", [new("admin/members", "Members", "users"), new("admin/payments", "Payments", "card"), new("admin/points", "Points", "star"), new("admin/reservations", "Reservations", "ticket")]),
        new("Fleet", [new("admin/aircrafts", "Aircraft", "plane"), new("admin/aircraft-types", "Types", "layers"), new("admin/maintenance", "Maintenance", "wrench"), new("admin/airports", "Airports", "pin"), new("admin/routes", "Routes", "route")]),
        new("Crew", [new("admin/pilots", "Pilots", "user"), new("admin/pilot-documents", "Documents", "file"), new("admin/pilot-certifications", "Certifications", "award"), new("admin/flight-schedule", "Schedule", "clock")]),
    ];

    private static readonly HashSet<string> NavRoots = ["admin", "portal"];

    public static string UiInitials(params string[] names)
    {
        var letters = names
            .Select(name => name.Trim())
            .Where(name => name.Length > 0)
            .Take(2)
            .Select(name => char.ToUpperInvariant(name[0]));

        var initials = string.Concat(letters);

        return initials.Length == 0 ? "?" : initials;
    }

    public static string UiDate(DateOnly? value) =>
        value?.ToString("d MMM yyyy", CultureInfo.InvariantCulture) ?? "—";

    public static string UiDate(DateTime? value, string format) =>
        value?.ToString(format, CultureInfo.InvariantCulture) ?? "—";

    public static string UiPlural(int count, string singular, string? plural = null) => count == 1 ? singular : plural ?? singular + "s";

    public static string UiPoints(decimal? value) => (value ?? 0).ToString("N2", CultureInfo.InvariantCulture);

    public static string UiSignedPoints(decimal? value)
    {
        var amount = value ?? 0;

        return (amount < 0 ? "−" : "+") + UiPoints(Math.Abs(amount));
    }

    public static IReadOnlyList<NavGroup> UiNav(bool admin) => admin ? AdminNav : MemberNav;

    public static bool UiIsCurrent(this IHtmlHelper html, string path)
    {
        var current = html.ViewContext.HttpContext.Request.Path.Value?.Trim('/') ?? "";

        if (NavRoots.Contains(path))
        {
            return current == path;
        }

        return current == path || current.StartsWith(path + "/", StringComparison.Ordinal);
    }

    public static string UiNavLabel(string path, string fallback) =>
        AdminNav.SelectMany(group => group.Items).FirstOrDefault(item => item.Path == path)?.Label ?? fallback;
}
