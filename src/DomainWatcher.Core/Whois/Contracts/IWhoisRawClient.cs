using DomainWatcher.Core.Values;

namespace DomainWatcher.Core.Whois.Contracts;

public interface IWhoisRawClient
{
    Task<string> QueryAsync(string whoisServerUrl, Domain domain);
}
