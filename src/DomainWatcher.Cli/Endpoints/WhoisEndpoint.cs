using DomainWatcher.Core.Repositories;
using DomainWatcher.Core.Values;
using DomainWatcher.Core.Whois;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Cli.Endpoints;

public class WhoisEndpoint(
    IWhoisClient client,
    IDomainsRepository domainsRepository,
    IWhoisResponsesRepository whoisResponsesRepository,
    ILogger<WhoisEndpoint> logger) : IHttpEndpoint
{
    public static HttpMethod Method => HttpMethod.Get;

    public static string Path => @"^/(?<Domain>[\w\.]+)$";

    public async Task<HttpResponse> Handle(HttpRequest request)
    {
        var domainString = this.ExtractRegexGroup(request, "Domain");
        var domain = new Domain(domainString);
        var latestAvailableWhoisResponse = await whoisResponsesRepository.GetLatestFor(domain);

        if (latestAvailableWhoisResponse != null && latestAvailableWhoisResponse.QueryTimestamp < DateTime.UtcNow.AddDays(7))
        {
            // if we've got previous whois response that is not too old we'll just use that.
            return HttpResponse.PlainText(latestAvailableWhoisResponse.RawResponse.Trim());
        }

        var domainSupportedResult = await client.IsDomainSupported(domain);

        if (domainSupportedResult.IsSupported)
        {
            latestAvailableWhoisResponse = await client.QueryAsync(domain);
            await domainsRepository.Store(domain);
            await whoisResponsesRepository.Add(latestAvailableWhoisResponse);

            return HttpResponse.PlainText(latestAvailableWhoisResponse.RawResponse.Trim());
        }

        if (domainSupportedResult.WhoisServerUrl == null)
        {
            logger.LogInformation("Domain {DomainUrl} has invalid tld.", domain.FullName);

            return HttpResponse.BadRequestWithReason($"Invalid tld: {domain.Tld}");
        }

        logger.LogWarning(
            "Domain {DomainUrl} with whois server url {WhoisServerUrl} is valid but not (yet) supported.",
            domain.FullName,
            domainSupportedResult.WhoisServerUrl);

        return HttpResponse.BadRequestWithReason($"Tld '{domain.Tld}' with whois server url '{domainSupportedResult.WhoisServerUrl}' is valid but not (yet) supported.");
    }
}
