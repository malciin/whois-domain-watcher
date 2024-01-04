using DomainWatcher.Core.Contracts;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Repositories;
using DomainWatcher.Core.Values;
using DomainWatcher.Core.Whois;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Cli.Endpoints;

public class WatchDomainEndpoint(
    IWhoisClient client,
    IDomainsQueryQueue queryQueue,
    IDomainsRepository domainsRepository,
    IWhoisResponsesRepository whoisResponsesRepository,
    ILogger<WatchDomainEndpoint> logger) : IHttpEndpoint
{
    public static HttpMethod Method => HttpMethod.Post;

    public static string Path => "/";

    public async Task<HttpResponse> Handle(HttpRequest request)
    {
        var domainString = request.Body;
        var domain = new Domain(domainString);

        if (await domainsRepository.IsWatched(domain))
        {
            var latestResponse = await whoisResponsesRepository.GetLatestFor(domain);

            return HttpResponse.PlainText($"{domain} is already watched! " + GetStatusString(latestResponse));
        }

        var domainSupportedResult = await client.IsDomainSupported(domain);

        if (domainSupportedResult.IsSupported)
        {
            await domainsRepository.Watch(domain);
            var whoisResponse = await client.QueryAsync(domain);

            queryQueue.EnqueueNext(domain, whoisResponse);

            return HttpResponse.PlainText($"{domain} watched! " + GetStatusString(whoisResponse));
        }

        if (domainSupportedResult.WhoisServerUrl == null)
        {
            logger.LogInformation("Domain {DomainUrl} has invalid tld.", domain.FullName);

            return HttpResponse.BadRequestWithReason($"Invalid tld: {domain.Tld}");
        }
        
        logger.LogInformation("Domain {DomainUrl} with whois server url {WhoisServerUrl} is valid but not (yet) supported.", domain.FullName, domainSupportedResult.WhoisServerUrl);
        return HttpResponse.BadRequestWithReason($"Tld '{domain.Tld}' with whois server url '{domainSupportedResult.WhoisServerUrl}' is valid but not (yet) supported.");
    }

    private static string GetStatusString(WhoisResponse? response)
    {
        if (response == null)
        {
            return "Its was not queried yet however. Try after some time";
        }

        if (response.IsAvailable)
        {
            return "Its currently available";
        }

        var expiration = response.Expiration!.Value;
        var delay = expiration - DateTime.UtcNow;

        return $"Its currently taken and expiring at {delay.ToJiraDuration()} ({expiration:yyyy-MM-dd HH:mm})";
    }
}
