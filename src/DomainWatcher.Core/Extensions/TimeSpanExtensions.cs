using System.Runtime.InteropServices;

namespace DomainWatcher.Core.Extensions;

public static class TimeSpanExtensions
{
    public static string ToJiraDuration(this TimeSpan timeSpan)
    {
        return string.Join(" ", GetSegments(timeSpan));
    }

    public static string ToJiraDuration(this TimeSpan timeSpan, int maxSegments)
    {
        return string.Join(" ", GetSegments(timeSpan).Take(maxSegments));
    }

    private static IEnumerable<string> GetSegments(TimeSpan timeSpan)
    {
        var showMiliseconds = timeSpan.TotalSeconds < 1.0;
        var seconds = showMiliseconds
            ? (long)timeSpan.TotalMilliseconds
            : (long)timeSpan.TotalSeconds;

        var timeUnits = showMiliseconds
            ? TimeUnits.Select(x => (x.Unit, x.SecondsValue * 1000)).Append(("ms", 1))
            : TimeUnits;

        var anyReturned = false;

        foreach (var (suffix, step) in timeUnits)
        {
            if (seconds >= step)
            {
                var mod = seconds / step;
                seconds -= mod * step;

                anyReturned = true;
                yield return $"{mod}{suffix}";
            }
        }

        if (!anyReturned)
        {
            yield return "0ms";
        }
    }

    private static readonly IReadOnlyList<(string Unit, long SecondsValue)> TimeUnits = new[]
    {
        ("y", 60 * 60 * 24 * 365L),
        ("m", 60 * 60 * 24 * 30),
        ("d", 60 * 60 * 24),
        ("h", 60 * 60),
        ("m", 60),
        ("s", 1)
    };
}
