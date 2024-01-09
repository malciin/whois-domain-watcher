using DomainWatcher.Core.Whois.Values;

namespace DomainWatcher.Core.Whois.Contracts;

public interface IWhoisResponseParser
{
    IEnumerable<string> SupportedWhoisServers { get; }

    bool DoesSupport(string whoisServerUrl);

    WhoisServerResponseParsed Parse(string whoisServerUrl, string whoisResponse);

    /// <summary>
    /// If WhoisResponseParser does not have support for WhoisServer checked by .DoesSupport()
    /// its possible to try to parse it by generic parser that just try to enumerate on all subparsers
    /// hoping some parser will manage to parse it.
    /// </summary>
    /// <param name="whoisResponse">raw whois response</param>
    /// <param name="parsedBy">whois url for which parser managed to parse response</param>
    /// <returns>Parsed result or null if no subparser manage to parse it</returns>
    WhoisServerResponseParsed? ParseFallback(string whoisResponse, out string? parsedBy);
}
