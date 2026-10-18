namespace FlyVip.Web.Infrastructure;

public static class TimeProviderExtensions
{
    public static DateOnly Today(this TimeProvider clock) => DateOnly.FromDateTime(clock.GetLocalNow().DateTime);
}
