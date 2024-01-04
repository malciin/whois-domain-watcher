namespace DomainWatcher.Core.Utilities;

public static class TimeSpanMath
{
    public static TimeSpan Max(params TimeSpan[] timeSpans)
    {
        var max = timeSpans[0];

        for (var i = 1; i < timeSpans.Length; i++)
        {
            if (max < timeSpans[i])
            {
                max = timeSpans[i];
            }
        }

        return max;
    }

    public static TimeSpan Min(params TimeSpan[] timeSpans)
    {
        var min = timeSpans[0];

        for (var i = 1; i < timeSpans.Length; i++)
        {
            if (min > timeSpans[i])
            {
                min = timeSpans[i];
            }
        }

        return min;
    }
}
