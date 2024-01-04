using DomainWatcher.Core.Enums;
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

        if (whoisServerUrl != null) return new IsDomainSupportedResult { IsSupported = true };

        return new IsDomainSupportedResult
        {
            IsSupported = false,
            Reason = $"Invalid TLD: {domain.Tld}"
        };
    }

    public async Task<WhoisResponse> QueryAsync(Domain domain)
    {
        var whoisServerUrl = await whoisServerUrlResolver.Resolve(domain.Tld);

        if (whoisServerUrl == null)
        {
            throw new InvalidOperationException($"Invalid tld: {domain.Tld}");
        }

        var queryTimestamp = DateTime.UtcNow;
        var whoisResponse = await rawClient.GetResponse(whoisServerUrl, domain.FullName);

        if (string.IsNullOrWhiteSpace(whoisResponse))
        {
            throw new UnexpectedWhoisResponseException(
                $"Empty whois response from whois server url: {whoisServerUrl}", whoisResponse);
        }

        if (!whoisResponseParser.DoesSupport(whoisServerUrl))
        {
            return new WhoisResponse
            {
                Domain = domain,
                QueryTimestamp = queryTimestamp,
                RawResponse = whoisResponse,
                Status = WhoisResponseStatus.ParserMissing,
                SourceServer = whoisServerUrl
            };
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

        if (parsed.Status == WhoisResponseStatus.ParserMissing)
        {
            throw new InvalidOperationException($"Unexpected {WhoisResponseStatus.ParserMissing} status after parsing.");
        }

        return new WhoisResponse
        {
            SourceServer = whoisServerUrl,
            Status = parsed.Status,
            Domain = domain,
            RawResponse = whoisResponse,
            QueryTimestamp = queryTimestamp,
            Expiration = parsed.Expiration,
            Registration = parsed.Registration
        };
    }
}
