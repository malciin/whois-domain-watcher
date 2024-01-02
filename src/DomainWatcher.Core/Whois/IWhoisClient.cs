using DomainWatcher.Core.Values;

namespace DomainWatcher.Core.Whois;

public interface IWhoisClient
{
    Task<bool> IsDomainSupported(Domain domain);

    Task<WhoisResponse> QueryAsync(Domain domain);
}
