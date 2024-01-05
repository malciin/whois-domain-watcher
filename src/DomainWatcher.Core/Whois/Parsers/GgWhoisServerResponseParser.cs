using System.Text.RegularExpressions;
using DomainWatcher.Core.Whois.Exceptions;
using DomainWatcher.Core.Whois.Values;

namespace DomainWatcher.Core.Whois.Parsers;

internal class GgWhoisServerResponseParser : WhoisServerResponseParser
{
    private static readonly Regex RegistrationDateRegex = new(
        @"Registered on (?<Day>\d+)\w{1,2} (?<Month>\w+) (?<Year>\d{4}) at (?<Hour>\d+):(?<Minute>\d+):(?<Second>\d+)\.(?<Milisecond>\d+)",
        RegexOptions.Multiline);

    internal override IEnumerable<string> GetSupportedWhoisServers()
    {
        yield return "whois.gg";
    }

    internal override WhoisServerResponseParsed Parse(string rawResponse)
    {
        if (rawResponse == "NOT FOUND") return DomainAvailable;

        var match = RegistrationDateRegex.Match(rawResponse);
        
        if (!match.Success)
        {
            throw new UnexpectedWhoisResponseException($"Cannot find {RegistrationDateRegex} in whois response", rawResponse);
        }

        var groups = match.Groups;
        var day = int.Parse(groups["Day"].Value);
        var month = groups["Month"].Value switch
        {
            "January" => 1,
            "February" => 2,
            "March" => 3,
            "April" => 4,
            "May" => 5,
            "June" => 6,
            "July" => 7,
            "August" => 8,
            "September" => 9,
            "October" => 10,
            "November" => 11,
            "December" => 12,
            _ => throw new UnexpectedWhoisResponseException("Invalid month name: " + groups["Month"].Value, rawResponse)
        };
        var year = int.Parse(groups["Year"].Value);
        var hour = int.Parse(groups["Hour"].Value);
        var minute = int.Parse(groups["Minute"].Value);
        var second = int.Parse(groups["Second"].Value);
        var milisecond = int.Parse(groups["Milisecond"].Value);

        var registration = new DateTime(year, month, day, hour, minute, second, milisecond, DateTimeKind.Utc).ToUniversalTime();
        var expiration = new DateTime(DateTime.UtcNow.Year, month, day, hour, minute, second, milisecond, DateTimeKind.Utc).ToUniversalTime();

        if (expiration < DateTime.UtcNow) expiration = expiration.AddYears(1);

        return new WhoisServerResponseParsed
        {
            Expiration = expiration,
            Registration = registration
        };
    }
}
