using DomainWatcher.Core.Whois.Contracts;
using DomainWatcher.Infrastructure.Sqlite.Abstract;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Infrastructure.Sqlite.Cache;

public class WhoisServerUrlResolverSqliteCache : SqliteCacheService, IWhoisServerUrlResolver
{
    private readonly IWhoisServerUrlResolver implementation;

    public WhoisServerUrlResolverSqliteCache(
        SqliteConnection connection,
        ILogger<WhoisServerUrlResolverSqliteCache> logger,
        IWhoisServerUrlResolver implementation) : base(logger, connection)
    {
        this.implementation = implementation;
    }

    public Task<string?> Resolve(string tld)
    {
        return GetFromCachedOrImpl(
            $"whoisServerUrl:{tld}",
            TimeSpan.FromDays(7),
            () => implementation.Resolve(tld),
            x => x != null);
    }
}
