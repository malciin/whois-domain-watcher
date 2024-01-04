using System.Collections.Concurrent;
using DomainWatcher.Core.Contracts;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Utilities;
using DomainWatcher.Core.Values;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Core.Services;

public class DomainsQueryQueue : IDomainsQueryQueue
{
    private readonly ILogger<DomainsQueryQueue> logger;
    private readonly TimedSequence<Domain> domainsTimedSequence;
    private readonly ConcurrentDictionary<Domain, int> domainInvalidResponsesCounter;

    public int Count => domainInvalidResponsesCounter.Count;

    public DomainsQueryQueue(ILogger<DomainsQueryQueue> logger)
    {
        this.logger = logger;
        domainsTimedSequence = new TimedSequence<Domain>();
        domainInvalidResponsesCounter = new ConcurrentDictionary<Domain, int>();
    }

    public bool TryPeek(out Domain? domain, out DateTime fireAt)
    {
        return domainsTimedSequence.TryPeek(out fireAt, out domain);
    }

    public void EnqueueNext(Domain domain, WhoisResponse? latestResponse)
    {
        domainInvalidResponsesCounter.AddOrUpdate(domain, _ => 0, (_, __) => 0);

        if (latestResponse == null)
        {
            Enqueue(domain, TimeSpan.Zero);
        }
        else if (latestResponse.IsAvailable)
        {
            Enqueue(domain, TimeSpan.FromHours(12));
        }
        else
        {
            Enqueue(domain, TimeSpanMath.Min(latestResponse.Expiration!.Value - DateTime.UtcNow, TimeSpan.FromDays(7)));
        }
    }

    public void EnqueueAfterError(Domain domain)
    {
        var invalidCounterForDomain = domainInvalidResponsesCounter.AddOrUpdate(domain, _ => 1, (_, x) => x + 1);
        var delay = TimeSpan.FromMinutes(Math.Pow(2, invalidCounterForDomain));

        logger.LogWarning(
            "Domain enqueued to retry after {DelayDuration}. Error counter for this domain: {InvalidCounter}.",
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
