using DomainWatcher.Core.Contracts;
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
    IDomainQueryDelayProvider delayProvider,
    ILogger<WhoisEndpoint> logger) : IHttpEndpoint
{
    public static HttpMethod Method => HttpMethod.Get;

    public static string Path => @"^/(?<Domain>[\w\.]+\.[\w]+)$";

    public async Task<HttpResponse> Handle(HttpRequest request)
    {
        if (!Domain.TryParse(this.ExtractRegexGroup(request, "Domain"), out var domain))
        {
            return HttpResponse.BadRequestWithReason($"Cannot parse '{this.ExtractRegexGroup(request, "Domain")}' domain");
        }

        var latestAvailableWhoisResponse = await whoisResponsesRepository.GetLatestFor(domain!);

        if (latestAvailableWhoisResponse != null
            && delayProvider.GetDelay(domain, latestAvailableWhoisResponse).TotalSeconds > 0)
        {
            // if we've got previous whois response that is not too old we'll just use that.
            return CreateResponse(latestAvailableWhoisResponse);
        }

        var domainSupportedResult = await client.IsDomainSupported(domain);

        if (!domainSupportedResult.IsSupported)
        {
            logger.LogInformation("Domain {DomainUrl} unsupported. Reason: {Reason}.", domain.FullName, domainSupportedResult.Reason);

            return HttpResponse.BadRequestWithReason($"Domain not supported. Reason: {domainSupportedResult.Reason}");
        }

        latestAvailableWhoisResponse = await client.QueryAsync(domain);
        await domainsRepository.Store(domain);
        await whoisResponsesRepository.Add(latestAvailableWhoisResponse);

        return CreateResponse(latestAvailableWhoisResponse);
    }

    private static HttpResponse CreateResponse(WhoisResponse response)
    {
        return HttpResponse.PlainText(response.RawResponse.TrimEnd());
    }
}
