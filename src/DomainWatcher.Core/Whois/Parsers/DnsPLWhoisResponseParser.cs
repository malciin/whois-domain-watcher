using System.Globalization;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Whois.Values;

namespace DomainWatcher.Core.Whois.Parsers;

internal class DnsPLWhoisResponseParser : WhoisResponseParser
{
    private const string DateFormat = "yyyy.MM.dd HH:mm:ss";

    internal override IEnumerable<string> GetSupportedWhoisServers()
    {
        yield return "whois.dns.pl";
    }

    internal override WhoisResponseParsed Parse(string rawResponse)
    {
        if (rawResponse.Contains("No information available about domain name", StringComparison.InvariantCulture))
        {
            return DomainAvailable;
        }

        return new WhoisResponseParsed
        {
            Registration = ParseDate(rawResponse, "created:", DateFormat)!,
            Expiration = ParseDate(rawResponse, "option expiration date:", DateFormat)
                ?? ParseDate(rawResponse, "renewal date:", DateFormat)!
        };
    }

    private DateTime? ParseDate(string rawResponse, string lineWithDate)
    {
        var line = rawResponse.GetLineThatContains(lineWithDate);

        if (line == null) return null;

        var dateString = line[line.IndexOfAny(['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'])..];

        return DateTime
            .ParseExact(dateString, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
            .ToUniversalTime();
    }
}
