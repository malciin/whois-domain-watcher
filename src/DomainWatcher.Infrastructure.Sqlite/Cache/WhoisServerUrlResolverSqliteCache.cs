using DomainWatcher.Core.Whois.Contracts;

namespace DomainWatcher.Infrastructure.Sqlite.Cache;

public class WhoisServerUrlResolverSqliteCache : IWhoisServerUrlResolver
{
    private readonly IWhoisServerUrlResolver implementation;

    public WhoisServerUrlResolverSqliteCache(IWhoisServerUrlResolver implementation)
    {
        this.implementation = implementation;
    }

    public Task<string?> Resolve(string tld)
    {
        throw new NotImplementedException();
    }
}
