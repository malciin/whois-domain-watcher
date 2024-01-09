using DomainWatcher.Core.Contracts;
using DomainWatcher.Core.Utilities;
using Microsoft.Extensions.Configuration;

namespace DomainWatcher.Core.Settings;

internal class ProcessingQueueSettings : IMaxDomainsConsecutiveErrorsProvider
{
    public int MaxDomainConsecutiveErrors { get; }

    public DomainWhoisQueryIntervalsSettings WhoisQueryIntervals { get; }

    public ProcessingQueueSettings(IConfiguration configuration)
    {
        var processingQueueSection = configuration.GetSection("ProcessingQueue");
        var sectionWhoisQueryIntervalsSection = processingQueueSection.GetSection(nameof(WhoisQueryIntervals));

        MaxDomainConsecutiveErrors = int.Parse(processingQueueSection[nameof(MaxDomainConsecutiveErrors)]!);
        WhoisQueryIntervals = new DomainWhoisQueryIntervalsSettings
        {
            DomainTaken = JiraLikeDurationConverter.ToTimeSpan(
                sectionWhoisQueryIntervalsSection[nameof(DomainWhoisQueryIntervalsSettings.DomainTaken)]!),
            DomainTakenButExpirationHidden = JiraLikeDurationConverter.ToTimeSpan(
                sectionWhoisQueryIntervalsSection[nameof(DomainWhoisQueryIntervalsSettings.DomainTakenButExpirationHidden)]!),
            DomainFree = JiraLikeDurationConverter.ToTimeSpan(
                sectionWhoisQueryIntervalsSection[nameof(DomainWhoisQueryIntervalsSettings.DomainFree)]!),
            MissingParser = JiraLikeDurationConverter.ToTimeSpan(
                sectionWhoisQueryIntervalsSection[nameof(DomainWhoisQueryIntervalsSettings.MissingParser)]!),
            BaseErrorRetryDelay = JiraLikeDurationConverter.ToTimeSpan(
                sectionWhoisQueryIntervalsSection[nameof(DomainWhoisQueryIntervalsSettings.MissingParser)]!)
        };
    }
}
