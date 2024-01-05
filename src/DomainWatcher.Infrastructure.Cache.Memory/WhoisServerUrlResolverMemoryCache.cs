using DomainWatcher.Core.Whois.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace DomainWatcher.Infrastructure.Cache.Memory;

public class WhoisServerUrlResolverMemoryCache(IWhoisServerUrlResolver implementation) : IWhoisServerUrlResolver
{
    private static readonly MemoryCache Cache = new(new MemoryCacheOptions { });

    public async Task<string?> Resolve(string tld)
    {
        if (Cache.TryGetValue<string>(tld, out var address))
        {
            return address;
        }

        address = await implementation.Resolve(tld);
    
        if (address != null)
        {
            Cache.Set(tld, address, DateTimeOffset.UtcNow.AddHours(12));
        }

        return address;
    }
}
