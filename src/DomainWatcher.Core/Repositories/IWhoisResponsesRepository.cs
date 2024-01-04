using DomainWatcher.Core.Values;

namespace DomainWatcher.Core.Repositories;

public interface IWhoisResponsesRepository
{
    Task Add(WhoisResponse whoisResponse);

    Task<WhoisResponse?> GetLatestFor(Domain domain);
}
