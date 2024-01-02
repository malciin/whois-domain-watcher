using System.Globalization;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Whois.Values;

namespace DomainWatcher.Core.Whois.Parsers;

internal abstract class WhoisResponseParser
{
    protected static readonly WhoisResponseParsed DomainAvailable = new();

    internal abstract IEnumerable<string> GetSupportedWhoisServers();

    internal abstract WhoisResponseParsed Parse(string rawResponse);

    protected DateTime? ParseDate(string rawResponse, string lineWithDate, string dateFormat)
    {
        var line = rawResponse.GetLineThatContains(lineWithDate);

        if (line == null) return null;

        var dateString = line[line.IndexOfAny(['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'])..];

        return DateTime
            .ParseExact(dateString, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
            .ToUniversalTime();
    }
}
