using System.Collections.Concurrent;
using DomainWatcher.Core.Contracts;
using DomainWatcher.Core.Enums;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Utilities;
using DomainWatcher.Core.Values;
using DomainWatcher.Core.Whois.Contracts;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Core.Services;

public class DomainsQueryQueue : IDomainsQueryQueue
{
    private readonly ILogger<DomainsQueryQueue> logger;
    private readonly TimedSequence<Domain> domainsTimedSequence;
    private readonly ConcurrentDictionary<Domain, int> domainInvalidResponsesCounter;
    private readonly IWhoisResponseParser whoisResponseParser;

    public int Count => domainInvalidResponsesCounter.Count;

    public DomainsQueryQueue(
        ILogger<DomainsQueryQueue> logger,
        IWhoisResponseParser whoisResponseParser)
    {
        this.logger = logger;
        this.whoisResponseParser = whoisResponseParser;
        domainsTimedSequence = new TimedSequence<Domain>();
        domainInvalidResponsesCounter = new ConcurrentDictionary<Domain, int>();
    }

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

        if (latestResponse == null)
        {
            Enqueue(domain, TimeSpan.Zero);
            return;
        }

        var queriedAgo = DateTime.UtcNow - latestResponse.QueryTimestamp;

        if (latestResponse.IsAvailable)
        {
            Enqueue(domain, TimeSpan.FromHours(12) - queriedAgo);
        }
        else if (latestResponse.Status == WhoisResponseStatus.TakenButTimestampsHidden)
        {
            Enqueue(domain, TimeSpan.FromDays(1) - queriedAgo);
        }
        else if (latestResponse.Status == WhoisResponseStatus.ParserMissing)
        {
            Enqueue(domain, whoisResponseParser.DoesSupport(latestResponse.SourceServer) ? TimeSpan.Zero : TimeSpan.FromDays(7) - queriedAgo);
        }
        else
        {
            Enqueue(domain, TimeSpanMath.Min(latestResponse.Expiration!.Value - DateTime.UtcNow, TimeSpan.FromDays(7) - queriedAgo));
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
