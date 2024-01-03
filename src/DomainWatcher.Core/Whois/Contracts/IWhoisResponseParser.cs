using DomainWatcher.Core.Whois.Values;

namespace DomainWatcher.Core.Whois.Contracts;

public interface IWhoisResponseParser
{
    IEnumerable<string> SupportedWhoisServers { get; }

    bool DoesSupport(string whoisServerUrl);

    WhoisServerResponseParsed Parse(string whoisServerUrl, string whoisResponse);
}
