using DomainWatcher.Core.Extensions;

namespace DomainWatcher.Core.Utilities;

public static class JiraLikeDurationConverter
{
    public static TimeSpan ToTimeSpan(string jiraLikeStringDuration)
    {
        var result = TimeSpan.Zero;
        var durationSegments = jiraLikeStringDuration.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var durationSegment in durationSegments)
        {
            var notDigitCharIdx = durationSegment.FindIndex(x => !char.IsDigit(x));

            if (notDigitCharIdx == -1)
            {
                throw new ArgumentException($"Cannot parse {jiraLikeStringDuration}. Invalid segment: {durationSegment}");
            }

            var unitValueString = durationSegment[..notDigitCharIdx];
            var unit = durationSegment[notDigitCharIdx..];

            if (!int.TryParse(unitValueString, out var unitValue))
            {
                throw new ArgumentException($"Cannot parse {jiraLikeStringDuration}. Cannot parse segment: {durationSegment} because of invalid number: {unitValueString}");
            }

            if (!MsTimeByUnit.TryGetValue(unit, out var milisecondsFactor))
            {
                throw new ArgumentException($"Cannot parse {jiraLikeStringDuration}. Cannot parse segment: {durationSegment} because of invalid unit: {unit}");
            }

            result = result.Add(TimeSpan.FromMilliseconds(unitValue * milisecondsFactor));
        }

        return result;
    }

    public static string ToString(this TimeSpan timeSpan, int? maxSegments = null)
    {
        var segments = GetSegments(timeSpan);

        if (maxSegments.HasValue) segments = segments.Take(maxSegments.Value);

        return string.Join(" ", segments);
    }

    private static IEnumerable<string> GetSegments(TimeSpan timeSpan)
    {
        var showMiliseconds = timeSpan.TotalSeconds < 1.0;

        if (showMiliseconds)
        {
            yield return $"{(int)timeSpan.TotalMilliseconds}ms";
            yield break;
        }

        var seconds = (long)timeSpan.TotalSeconds;

        foreach (var (suffix, step) in TimeSecondUnits)
        {
            if (seconds >= step)
            {
                var mod = seconds / step;
                seconds -= mod * step;

                yield return $"{mod}{suffix}";
            }
        }
    }

    private static readonly IReadOnlyList<(string Unit, long Seconds)> TimeSecondUnits = new[]
    {
        (Unit: "y", Seconds: 60 * 60 * 24 * 365L),
        (Unit: "mo", Seconds: 60 * 60 * 24 * 30),
        (Unit: "d", Seconds: 60 * 60 * 24),
        (Unit: "h", Seconds: 60 * 60),
        (Unit: "m", Seconds: 60),
        (Unit: "s", Seconds: 1)
    };

    private static readonly IReadOnlyDictionary<string, long> MsTimeByUnit = TimeSecondUnits
        .Select(x => (x.Unit, Miliseconds: x.Seconds * 1000L))
        .Append((Unit: "ms", Miliseconds: 1))
        .ToDictionary(x => x.Unit, x => x.Miliseconds);
}
