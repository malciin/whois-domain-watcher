using DomainWatcher.Core.Values;
using DomainWatcher.Core.Whois.Values;

namespace DomainWatcher.Core.Whois;

public interface IWhoisClient
{
    Task<IsDomainSupportedResult> IsDomainSupported(Domain domain);

    Task<WhoisResponse> QueryAsync(Domain domain);
}
