namespace DomainWatcher.Core.Whois.Contracts;

public interface IWhoisServerUrlResolver
{
    Task<string> Resolve(string tld);
}
