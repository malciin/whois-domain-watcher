using DomainWatcher.Core.Utilities;

namespace DomainWatcher.Core.Extensions;

public static class TimeSpanExtensions
{
    public static string ToJiraDuration(this TimeSpan timeSpan, int? maxSegments = null)
    {
        return JiraLikeDurationConverter.ToString(timeSpan, maxSegments);
    }
}
