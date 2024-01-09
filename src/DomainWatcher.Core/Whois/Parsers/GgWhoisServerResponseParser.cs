using System.Text.RegularExpressions;
using DomainWatcher.Core.Whois.Exceptions;
using DomainWatcher.Core.Whois.Values;

namespace DomainWatcher.Core.Whois.Parsers;

/// <summary>
/// Dedicated parser because whois.gg returns only Registration date.
/// Expiration is relative to registration date
/// </summary>
internal class GgWhoisServerResponseParser : WhoisServerResponseParser
{
    internal override IEnumerable<string> GetSupportedWhoisServers()
    {
        yield return "whois.gg";
    }

    internal override WhoisServerResponseParsed Parse(string whoisServerUrl, string rawResponse)
    {
        if (rawResponse.StartsWith("NOT FOUND")) return DomainAvailable;

        var dateString = GetDateString(rawResponse, "Registered on")
            ?? throw new UnexpectedWhoisResponseException($"Cannot find 'Registered on' in whois response", rawResponse);

        var removedSuffix = Regex.Replace(dateString, @"([\d]{1,2})((th)|(rd)|(st))", "$1");
        var registration = ParseDate(removedSuffix, "dd MMMM yyyy 'at' HH:mm:ss.fff");
        var expiration = new DateTime(
            DateTime.UtcNow.Year,
            registration.Month,
            registration.Day,
            registration.Hour,
            registration.Minute, 
            registration.Second,
            registration.Millisecond,
            DateTimeKind.Utc).ToUniversalTime();

        if (expiration < DateTime.UtcNow) expiration = expiration.AddYears(1);

        return new WhoisServerResponseParsed
        {
            Expiration = expiration,
            Registration = registration
        };
    }
}
