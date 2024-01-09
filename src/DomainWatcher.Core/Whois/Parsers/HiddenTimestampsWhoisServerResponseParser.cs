using DomainWatcher.Core.Enums;
using DomainWatcher.Core.Whois.Values;

namespace DomainWatcher.Core.Whois.Parsers;

/// <summary>
/// Parser for whois servers which hides expiration/creation timestamps
/// </summary>
internal class HiddenTimestampsWhoisServerResponseParser : WhoisServerResponseParser
{
    private static readonly IEnumerable<Handler> Handlers = new[]
    {
        new Handler { Url = "whois.eu", IsFree = x => x.Contains("Status: AVAILABLE") },
        new Handler { Url = "whois.denic.de", IsFree = x => x.Contains("Status: free") },
        new Handler { Url = "whois.tonic.to", IsFree = x => x.Contains("No match for") },
    };

    internal override IEnumerable<string> GetSupportedWhoisServers()
    {
        return Handlers.Select(x => x.Url);
    }

    internal override WhoisServerResponseParsed Parse(string whoisServerUrl, string rawResponse)
    {
        var handler = Handlers.Single(x => x.Url == whoisServerUrl);

        if (handler.IsFree(rawResponse))
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

    private class Handler
    {
        required public string Url { get; init; }

        required public Func<string, bool> IsFree { get; init; }
    }
}
