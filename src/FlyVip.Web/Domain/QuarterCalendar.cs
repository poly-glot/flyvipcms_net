namespace FlyVip.Web.Domain;

public static class QuarterCalendar
{
    public const int GraceDays = 5;

    public static DateOnly QuarterStart(DateOnly date) => new(date.Year, ((date.Month - 1) / 3 * 3) + 1, 1);

    public static DateOnly NextQuarterStart(DateOnly date) => QuarterStart(date).AddMonths(3);

    public static DateOnly TermEnd(DateOnly start) => start.AddYears(1).AddDays(-1);

    public static DateOnly QuarterStartInTerm(DateOnly termStart, int quarter) => termStart.AddMonths((quarter - 1) * 3);

    public static DateOnly QuarterEnd(DateOnly quarterStart) => quarterStart.AddMonths(3).AddDays(-1);

    public static DateOnly DueAfter(DateOnly end) => end.AddDays(GraceDays);

    public static DateOnly FirstDueAfterJoining(DateOnly paidOn) => DueAfter(QuarterEnd(QuarterStart(paidOn)));
}
