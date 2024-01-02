using DomainWatcher.Core.Whois.Values;

namespace DomainWatcher.Core.Whois.Parsers;

internal class GoogleWhoisResponseParser : WhoisResponseParser
{
    public const string DateFormat = "yyyy-MM-dd\\THH:mm:ss\\Z";

    internal override IEnumerable<string> GetSupportedWhoisServers()
    {
        yield return "whois.nic.google";
    }

    internal override WhoisResponseParsed Parse(string rawResponse)
    {
        if (rawResponse.StartsWith("Domain not found."))
        {
            return DomainAvailable;
        }

        return new WhoisResponseParsed
        {
            Registration = ParseDate(rawResponse, "Creation Date:", DateFormat)!,
            Expiration = ParseDate(rawResponse, "Registry Expiry Date:", DateFormat)!
        };
    }
}
