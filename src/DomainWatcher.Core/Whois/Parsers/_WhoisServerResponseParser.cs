using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DomainWatcher.Core.Enums;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Whois.Values;

namespace DomainWatcher.Core.Whois.Parsers;

internal abstract class WhoisServerResponseParser
{
    protected static readonly WhoisServerResponseParsed DomainAvailable = new()
    {
        Status = WhoisResponseStatus.OK,
        Expiration = null,
        Registration = null
    };

    internal abstract IEnumerable<string> GetSupportedWhoisServers();

    internal abstract WhoisServerResponseParsed Parse(string whoisServerUrl, string rawResponse);

    protected static string? GetDateString(string rawResponse, string lineWithDate)
    {
        var line = rawResponse.GetLineThatContains(lineWithDate);

        if (line == null) return null;

        var dateString = line.Substring(line.IndexOf(lineWithDate) + lineWithDate.Length).Trim();

        return dateString;
    }

    protected static DateTime ParseDate(
        string rawResponse,
        string lineWithDate,
        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string dateFormat)
    {
        var dateTime = ParseDateOrNull(rawResponse, lineWithDate, dateFormat);

        if (dateTime == null) throw new NullReferenceException("Nullable date received but not nullable is expected!");

        return dateTime.Value;
    }

    protected static DateTime ParseDate(
        string dateString,
        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string dateFormat)
    {
        var dateTime = ParseDateOrNull(dateString, dateFormat);

        if (dateTime == null) throw new NullReferenceException("Nullable date received but not nullable is expected!");

        return dateTime.Value;
    }

    protected static DateTime? ParseDateOrNull(
        string rawResponse,
        string lineWithDate,
        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string dateFormat)
    {
        var dateString = GetDateString(rawResponse, lineWithDate);

        return ParseDateOrNull(dateString, dateFormat);
    }

    protected static DateTime? ParseDateOrNull(
        string? dateString,
        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string dateFormat)
    {
        if (dateString == null) return null;

        try
        {
            return DateTime
                .ParseExact(dateString, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
                .ToUniversalTime();
        }
        catch (FormatException)
        {
            throw new FormatException($"String '{dateString}' cannot be parsed given format: '{dateFormat}'");
        }
    }
}
