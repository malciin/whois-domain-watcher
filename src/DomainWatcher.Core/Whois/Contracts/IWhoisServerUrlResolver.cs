using DomainWatcher.Core.Values;

namespace DomainWatcher.Core.Whois.Contracts;

public interface IWhoisServerUrlResolver
{
    Task<string?> Resolve(string tld);
}


public static class IWhoisRawResponseProviderExtensions
{
    public static Task<string?> ResolveFor(this IWhoisServerUrlResolver resolver, Domain domain)
    {
        return resolver.Resolve(domain.Tld);
    }
}
