using System.Collections.Concurrent;
using DomainWatcher.Core.Contracts;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Settings;
using DomainWatcher.Core.Values;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Core.Services;

public class DomainsQueryQueue(
    ILogger<DomainsQueryQueue> logger,
    IDomainQueryDelayProvider domainQueryDelayProvider,
    DomainWhoisQueryIntervalsSettings queryIntervals,
    IMaxDomainsConsecutiveErrorsProvider maxDomainsConsecutiveErrorsProvider) : IDomainsQueryQueue
{
    private readonly TimedSequence<Domain> domainsTimedSequence = new();
    private readonly ConcurrentDictionary<Domain, int> domainInvalidResponsesCounter = new();

    public int Count => domainInvalidResponsesCounter.Count;

    public bool TryPeek(out Domain? domain, out DateTime fireAt)
    {
        return domainsTimedSequence.TryPeek(out fireAt, out domain);
    }

    public IReadOnlyList<(Domain Domain, DateTime FireAt)> GetEntries()
    {
        return domainsTimedSequence.GetEntries().Select(x => (x.Value, x.Key)).ToList();
    }

    public void EnqueueNext(Domain domain, WhoisResponse? latestResponse)
    {
        domainInvalidResponsesCounter.AddOrUpdate(domain, _ => 0, (_, __) => 0);
        Enqueue(domain, domainQueryDelayProvider.GetDelay(domain, latestResponse));
    }

    public void EnqueueAfterError(Domain domain)
    {
        var invalidCounterForDomain = domainInvalidResponsesCounter.AddOrUpdate(domain, _ => 1, (_, x) => x + 1);

        if (invalidCounterForDomain > maxDomainsConsecutiveErrorsProvider.MaxDomainConsecutiveErrors)
        {
            logger.LogWarning(
                "Removing {Domain} from queue after {Errors} errors",
                domain.FullName,
                maxDomainsConsecutiveErrorsProvider.MaxDomainConsecutiveErrors);
            return;
        }

        var delay = queryIntervals.BaseErrorRetryDelay * Math.Pow(2, invalidCounterForDomain);

        logger.LogWarning(
            "{Attempt}/{MaxAttempts} Domain enqueued to retry after {DelayDuration}. Error counter for this domain: {InvalidCounter}.",
            invalidCounterForDomain,
            maxDomainsConsecutiveErrorsProvider.MaxDomainConsecutiveErrors,
            delay.ToJiraDuration(),
            invalidCounterForDomain);

        Enqueue(domain, delay);
    }

    public Task<Domain> Dequeue(CancellationToken token)
    {
        return domainsTimedSequence.GetNext(token);
    }

    private void Enqueue(Domain domain, TimeSpan delay)
    {
        lock (domainsTimedSequence) domainsTimedSequence.Add(domain, delay);
    }
}
