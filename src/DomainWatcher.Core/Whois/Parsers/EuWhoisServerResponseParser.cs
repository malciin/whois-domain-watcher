using DomainWatcher.Core.Enums;
using DomainWatcher.Core.Whois.Values;

namespace DomainWatcher.Core.Whois.Parsers;

internal class EuWhoisServerResponseParser : WhoisServerResponseParser
{
    internal override IEnumerable<string> GetSupportedWhoisServers()
    {
        yield return "whois.eu";
    }

    internal override WhoisServerResponseParsed Parse(string rawResponse)
    {
        if (rawResponse.Contains("Status: AVAILABLE", StringComparison.InvariantCulture))
        {
            return DomainAvailable;
        }

        return new WhoisServerResponseParsed
        {
            Status = WhoisResponseStatus.TakenButTimestampsHidden,
            Expiration = null,
            Registration = null
        };
    }
}
