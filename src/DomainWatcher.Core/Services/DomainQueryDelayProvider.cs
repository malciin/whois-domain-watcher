using DomainWatcher.Core.Contracts;
using DomainWatcher.Core.Enums;
using DomainWatcher.Core.Settings;
using DomainWatcher.Core.Utilities;
using DomainWatcher.Core.Values;

namespace DomainWatcher.Core.Services;

internal class DomainQueryDelayProvider(DomainWhoisQueryIntervalsSettings queryIntervals) : IDomainQueryDelayProvider
{
    public TimeSpan GetDelay(Domain domain, WhoisResponse? latestResponse)
    {
        if (latestResponse == null)
        {
            return TimeSpan.Zero;
        }

        var queriedAgo = DateTime.UtcNow - latestResponse.QueryTimestamp;

        return latestResponse switch
        {
            { IsAvailable: true } => queryIntervals.DomainFree - queriedAgo,
            { Status: WhoisResponseStatus.TakenButTimestampsHidden } => queryIntervals.DomainTakenButExpirationHidden - queriedAgo,
            { Status: WhoisResponseStatus.ParserMissing } => queryIntervals.MissingParser - queriedAgo,
            _ => TimeSpanMath.Min(
                latestResponse.Expiration!.Value - DateTime.UtcNow,
                queryIntervals.DomainTaken - queriedAgo)
        };
    }
}
