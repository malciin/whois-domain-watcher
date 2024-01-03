using DomainWatcher.Core.Whois.Values;

namespace DomainWatcher.Core.Whois.Parsers;

internal class VerisignGrsWhoisServerResponseParser : WhoisServerResponseParser
{
    public const string DateFormat = "yyyy-MM-dd\\THH:mm:ss\\Z";

    private static readonly IEnumerable<string> DomainNotFoundBeginnings = new[]
    {
        "No match for \"",
        "No match for domain \"",
        "Domain not found."
    };

    internal override IEnumerable<string> GetSupportedWhoisServers()
    {
        yield return "whois.verisign-grs.com";
        yield return "tvwhois.verisign-grs.com";
        yield return "whois.nic.io";
    }

    internal override WhoisServerResponseParsed Parse(string rawResponse)
    {
        if (DomainNotFoundBeginnings.Any(rawResponse.StartsWith))
        {
            return DomainAvailable;
        }

        return new WhoisServerResponseParsed
        {
            Registration = ParseDate(rawResponse, "Creation Date:", DateFormat)!,
            Expiration = ParseDate(rawResponse, "Registry Expiry Date:", DateFormat)!
        };
    }
}
