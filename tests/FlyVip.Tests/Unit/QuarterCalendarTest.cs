using System.Globalization;
using FlyVip.Web.Domain;

namespace FlyVip.Tests.Unit;

public sealed class QuarterCalendarTest
{
    [Theory]
    [InlineData("2026-02-14", "2026-04-05")]
    [InlineData("2026-05-01", "2026-07-05")]
    [InlineData("2026-09-30", "2026-10-05")]
    [InlineData("2026-11-20", "2027-01-05")]
    public void FirstDueAfterJoiningIsFifthOfNextQuarter(string paidOn, string due)
    {
        Assert.Equal(DateOnly.Parse(due, CultureInfo.InvariantCulture), QuarterCalendar.FirstDueAfterJoining(DateOnly.Parse(paidOn, CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void QuarterStartAndNext()
    {
        var date = new DateOnly(2026, 8, 17);

        Assert.Equal(new DateOnly(2026, 7, 1), QuarterCalendar.QuarterStart(date));
        Assert.Equal(new DateOnly(2026, 10, 1), QuarterCalendar.NextQuarterStart(date));
    }

    [Fact]
    public void TermAndQuarterWindows()
    {
        var start = new DateOnly(2026, 10, 1);

        Assert.Equal(new DateOnly(2027, 9, 30), QuarterCalendar.TermEnd(start));
        Assert.Equal(new DateOnly(2027, 4, 1), QuarterCalendar.QuarterStartInTerm(start, 3));
        Assert.Equal(new DateOnly(2026, 12, 31), QuarterCalendar.QuarterEnd(start));
        Assert.Equal(new DateOnly(2027, 10, 5), QuarterCalendar.DueAfter(new DateOnly(2027, 9, 30)));
    }
}
