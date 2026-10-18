using System.Text.RegularExpressions;
using FlyVip.Tests.Support;
using FlyVip.Web.Data.Entities;

namespace FlyVip.Tests.Feature;

public sealed partial class NavigationTest(DatabaseFixture database) : DatabaseTestCase(database)
{
    [Fact]
    public async Task OnlyTheCurrentSectionIsMarkedAsCurrent()
    {
        using var admin = await ActingAs(await Login(UserRole.Admin));

        var current = CurrentLinks().Matches(await admin.GetStringAsync("/admin/aircrafts", TestContext.Current.CancellationToken));

        Assert.Single(current);
        Assert.Contains("title=\"Aircraft\"", current[0].Value, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DashboardIsCurrentOnlyOnTheDashboard()
    {
        using var admin = await ActingAs(await Login(UserRole.Admin));

        var current = CurrentLinks().Matches(await admin.GetStringAsync("/admin", TestContext.Current.CancellationToken));

        Assert.Single(current);
        Assert.Contains("title=\"Dashboard\"", current[0].Value, StringComparison.Ordinal);
    }

    [GeneratedRegex("<a[^>]*aria-current=\"page\"[^>]*>")]
    private static partial Regex CurrentLinks();
}
