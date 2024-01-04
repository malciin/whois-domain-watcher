using DomainWatcher.Core.Values;
using DomainWatcher.Core.Whois.Contracts;
using DomainWatcher.Core.Whois.Exceptions;
using DomainWatcher.Core.Whois.Values;

namespace DomainWatcher.Core.Whois.Implementation;

public class WhoisClient(
    IWhoisServerUrlResolver whoisServerUrlResolver,
    IWhoisRawResponseProvider rawClient,
    IWhoisResponseParser whoisResponseParser) : IWhoisClient
{
    public async Task<IsDomainSupportedResult> IsDomainSupported(Domain domain)
    {
        var whoisServerUrl = await whoisServerUrlResolver.Resolve(domain.Tld);

        return new IsDomainSupportedResult
        {
            WhoisServerUrl = whoisServerUrl,
            IsSupported = whoisServerUrl != null && whoisResponseParser.DoesSupport(whoisServerUrl)
        };
    }

    public async Task<WhoisResponse> QueryAsync(Domain domain)
    {
        var whoisServerUrl = await whoisServerUrlResolver.Resolve(domain.Tld);

        if (whoisServerUrl == null)
        {
            throw new InvalidOperationException($"Invalid tld: {domain.Tld}");
        }

        if (!whoisResponseParser.DoesSupport(whoisServerUrl))
        {
            throw new NoParserForWhoisServerException(whoisServerUrl);
        }

        var queryTimestamp = DateTime.UtcNow;
        var whoisResponse = await rawClient.GetResponse(whoisServerUrl, domain.FullName);

        if (string.IsNullOrWhiteSpace(whoisResponse))
        {
            throw new UnexpectedWhoisResponseException(
                $"Empty whois response from whois server url: {whoisServerUrl}", whoisResponse);
        }

        WhoisServerResponseParsed parsed;

        try
        {
            parsed = whoisResponseParser.Parse(whoisServerUrl, whoisResponse);
        }
        catch (Exception ex)
        {
            throw new UnexpectedWhoisResponseException(
                $"Exception durning parsing whois response for '{domain}' with whois server url: {whoisServerUrl}",
                whoisResponse,
                ex);
        }

        return new WhoisResponse
        {
            Domain = domain,
            RawResponse = whoisResponse,
            QueryTimestamp = queryTimestamp,
            Expiration = parsed.Expiration,
            Registration = parsed.Registration
        };
    }
}
