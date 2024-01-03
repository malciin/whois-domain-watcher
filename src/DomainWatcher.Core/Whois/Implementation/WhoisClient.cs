using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Values;
using DomainWatcher.Core.Whois.Contracts;
using DomainWatcher.Core.Whois.Exceptions;
using DomainWatcher.Core.Whois.Parsers;

namespace DomainWatcher.Core.Whois.Implementation;

public class WhoisClient(
    IWhoisServerUrlResolver whoisServerUrlResolver,
    IWhoisRawClient rawClient) : IWhoisClient
{
    private static readonly IReadOnlyDictionary<string, WhoisResponseParser> ParsersByWhoisServerUrl = typeof(WhoisResponseParser)
        .Assembly
        .GetInstantiableTypesAssignableTo<WhoisResponseParser>()
        .Select(Activator.CreateInstance)
        .Cast<WhoisResponseParser>()
        .SelectMany(x => x.GetSupportedWhoisServers().Select(tld => KeyValuePair.Create(tld, x)))
        .ToDictionary(x => x.Key, v => v.Value);

    public async Task<bool> IsDomainSupported(Domain domain)
    {
        var whoisServerUrl = await whoisServerUrlResolver.Resolve(domain.Tld);

        return IsWhoisServerSupported(whoisServerUrl);
    }

    public async Task<WhoisResponse> QueryAsync(Domain domain)
    {
        var whoisServerUrl = await whoisServerUrlResolver.Resolve(domain.Tld);

        if (!IsWhoisServerSupported(whoisServerUrl))
        {
            throw new NoParserForWhoisServerException(whoisServerUrl);
        }

        var queryTimestamp = DateTime.UtcNow;
        var whoisResponse = await rawClient.QueryAsync(whoisServerUrl, domain);
        var parsedResponse = ParsersByWhoisServerUrl[whoisServerUrl].Parse(whoisResponse);

        return new WhoisResponse
        {
            Domain = domain,
            RawResponse = whoisResponse,
            QueryTimestamp = queryTimestamp,
            Expiration = parsedResponse.Expiration,
            Registration = parsedResponse.Registration
        };
    }

    private bool IsWhoisServerSupported(string serverUrl)
    {
        return ParsersByWhoisServerUrl.ContainsKey(serverUrl);
    }
}
