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
        var latestAvailableWhoisResponse = await whoisResponsesRepository.GetLatestFor(domain);

        if (await domainsRepository.IsWatched(domain))
        {
            return HttpResponse.PlainText($"{domain} is already watched! " + GetStatusString(latestAvailableWhoisResponse!));
        }

        var domainSupportedResult = await client.IsDomainSupported(domain);

        if (domainSupportedResult.IsSupported)
        {
            if (latestAvailableWhoisResponse != null && latestAvailableWhoisResponse.QueryTimestamp < DateTime.UtcNow.AddDays(7))
            {
                // if we've got previous whois response that is not too old we'll just use that.
                await domainsRepository.Watch(domain);
            }
            else
            {
                // if we haven't got any whois response or we've got an old
                // one then we just get fresh whois response and store it 
                latestAvailableWhoisResponse = await client.QueryAsync(domain);
                await domainsRepository.Watch(domain);
                await whoisResponsesRepository.Add(latestAvailableWhoisResponse);
            }

            queryQueue.EnqueueNext(domain, latestAvailableWhoisResponse);
            logger.LogInformation("{DomainUrl} watched.", domain.FullName);

            return HttpResponse.PlainText($"{domain} watched! " + GetStatusString(latestAvailableWhoisResponse));
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

    private static string GetStatusString(WhoisResponse response)
    {
        if (response.IsAvailable)
        {
            return "Its currently available";
        }

        var expiration = response.Expiration!.Value;
        var delay = expiration - DateTime.UtcNow;

        return $"Its currently taken and expiring at {delay.ToJiraDuration()} ({expiration:yyyy-MM-dd HH:mm})";
    }
}
