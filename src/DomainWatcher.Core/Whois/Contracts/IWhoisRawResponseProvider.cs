namespace DomainWatcher.Core.Whois.Contracts;

public interface IWhoisRawResponseProvider
{
    Task<string?> GetResponse(string whoisServerUrl, string query);
}
