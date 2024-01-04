using DomainWatcher.Core.Values;

namespace DomainWatcher.Core.Repositories;

public interface IDomainsRepository
{
    IAsyncEnumerable<Domain> GetWatchedDomains();

    Task Watch(Domain domain);

    Task Unwatch(Domain domain);

    Task<bool> IsWatched(Domain domain);
}
