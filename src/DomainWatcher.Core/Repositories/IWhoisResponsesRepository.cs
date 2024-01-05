using DomainWatcher.Core.Values;

namespace DomainWatcher.Core.Repositories;

public interface IWhoisResponsesRepository
{
    Task Add(WhoisResponse whoisResponse);

    Task<WhoisResponse?> GetLatestFor(Domain domain);

    /// <summary>
    /// Gets Ids for responses sorted by QueryTimestamp (ie. the last id is the id of the latest response)
    /// </summary>
    IAsyncEnumerable<long> GetWhoisResponsesIdsFor(Domain domain);
}
