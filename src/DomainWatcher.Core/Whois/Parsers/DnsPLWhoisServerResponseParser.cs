using DomainWatcher.Core.Whois.Values;

namespace DomainWatcher.Core.Whois.Parsers;

/// <summary>
/// Separate parser because Expiration could be named
/// "option expiration date:" or "renewal date:"
/// </summary>
internal class DnsPLWhoisServerResponseParser : WhoisServerResponseParser
{
    private const string DateFormat = "yyyy.MM.dd HH:mm:ss";

    internal override IEnumerable<string> GetSupportedWhoisServers()
    {
        yield return "whois.dns.pl";
    }

    internal override WhoisServerResponseParsed Parse(string whoisServerUrl, string rawResponse)
    {
        if (rawResponse.Contains("No information available about domain name", StringComparison.InvariantCulture))
        {
            return DomainAvailable;
        }

        return new WhoisServerResponseParsed
        {
            Registration = ParseDate(rawResponse, "created:", DateFormat),
            Expiration = ParseDateOrNull(rawResponse, "option expiration date:", DateFormat)
                ?? ParseDate(rawResponse, "renewal date:", DateFormat)
        };
    }
}
