namespace DomainWatcher.Core.Utilities;

public static class TimeSpanMath
{
    public static TimeSpan Max(params TimeSpan[] timeSpans)
    {
        return TimeSpan.FromTicks(timeSpans.Select(x => x.Ticks).Max());
    }

    public static TimeSpan Min(params TimeSpan[] timeSpans)
    {
        return TimeSpan.FromTicks(timeSpans.Select(x => x.Ticks).Min());
    }
}
