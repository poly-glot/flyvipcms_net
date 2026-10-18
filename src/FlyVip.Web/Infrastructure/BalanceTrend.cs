namespace FlyVip.Web.Infrastructure;

public static class BalanceTrend
{
    public const int Points = 30;

    public static IReadOnlyList<decimal> Of(IEnumerable<decimal> newestFirst, int take = Points) => [.. newestFirst.Take(take).Reverse()];
}
