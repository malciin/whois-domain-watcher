using DomainWatcher.Core.Values;

namespace DomainWatcher.Core.Contracts;

public interface IDomainsQueryQueue
{
    int Count { get; }

    bool TryPeek(out Domain? domain, out DateTime fireAt);

    void EnqueueNext(Domain domain, WhoisResponse? latestResponse);

    void EnqueueAfterError(Domain domain);

    Task<Domain> Dequeue(CancellationToken token);
}
