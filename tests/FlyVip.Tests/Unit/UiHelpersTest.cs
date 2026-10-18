using FlyVip.Web.Infrastructure;

namespace FlyVip.Tests.Unit;

public sealed class UiHelpersTest
{
    [Fact]
    public void InitialsTakeTheFirstTwoNames()
    {
        Assert.Equal("AP", UiHelpers.UiInitials("ana", " personal ", "extra"));
        Assert.Equal("?", UiHelpers.UiInitials("", " "));
    }

    [Fact]
    public void FormatsPointsAndDates()
    {
        Assert.Equal("12,000.00", UiHelpers.UiPoints(12000m));
        Assert.Equal("−1,250.50", UiHelpers.UiSignedPoints(-1250.5m));
        Assert.Equal("+0.00", UiHelpers.UiSignedPoints(null));
        Assert.Equal("5 Apr 2026", UiHelpers.UiDate(new DateOnly(2026, 4, 5)));
        Assert.Equal("5 Apr 2026, 09:30", UiHelpers.UiDate(new DateTime(2026, 4, 5, 9, 30, 0, DateTimeKind.Local), "d MMM yyyy, HH:mm"));
        Assert.Equal("—", UiHelpers.UiDate((DateOnly?)null));
    }

    [Theory]
    [InlineData(1, "seat", null, "seat")]
    [InlineData(0, "seat", null, "seats")]
    [InlineData(3, "day", null, "days")]
    [InlineData(2, "category", "categories", "categories")]
    public void PluralisesByCount(int count, string singular, string? plural, string expected)
    {
        Assert.Equal(expected, UiHelpers.UiPlural(count, singular, plural));
    }

    [Fact]
    public void NavigationLabelsComeFromTheAdminNav()
    {
        Assert.Equal("Maintenance", UiHelpers.UiNavLabel("admin/maintenance", "Repairs"));
        Assert.Equal("Repairs", UiHelpers.UiNavLabel("admin/unknown", "Repairs"));
        Assert.Equal(3, UiHelpers.UiNav(false).Count);
    }
}
