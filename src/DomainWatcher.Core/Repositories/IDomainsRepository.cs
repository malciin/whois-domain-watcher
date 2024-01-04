using DomainWatcher.Core.Values;

namespace DomainWatcher.Core.Repositories;

public interface IDomainsRepository
{
    IAsyncEnumerable<Domain> GetWatchedDomains();

    Task Store(Domain domain);

    Task Watch(Domain domain);

    Task Unwatch(Domain domain);

    Task<bool> IsWatched(Domain domain);
}
