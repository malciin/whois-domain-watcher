using DomainWatcher.Core.Whois.Contracts;
using DomainWatcher.Infrastructure.Sqlite.Abstract;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Infrastructure.Sqlite.Cache;

public class WhoisServerUrlResolverSqliteCache : SqliteCacheService, IWhoisServerUrlResolver
{
    private readonly ILogger<WhoisServerUrlResolverSqliteCache> logger;
    private readonly IWhoisServerUrlResolver implementation;

    public WhoisServerUrlResolverSqliteCache(
        SqliteConnection connection,
        ILogger<WhoisServerUrlResolverSqliteCache> logger,
        IWhoisServerUrlResolver implementation) : base(connection)
    {
        this.logger = logger;
        this.implementation = implementation;
    }

    public async Task<string?> Resolve(string tld)
    {
        var cacheKey = $"whoisServerUrl:{tld}";
        var cached = await GetStringFromCacheOrNull(cacheKey);

        if (cached != null)
        {
            logger.LogTrace("Returning {Value} for {Tld} tld from cache.", cached, tld);

            return cached;
        }

        logger.LogTrace("Cache miss for {Tld} tld from cache.", tld);
        var resolvedUrl = await implementation.Resolve(tld);

        if (resolvedUrl != null)
        {
            await SetCache(cacheKey, resolvedUrl, TimeSpan.FromDays(7));
        }

        return resolvedUrl;
    }
}
