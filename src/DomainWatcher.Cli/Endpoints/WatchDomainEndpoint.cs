using DomainWatcher.Cli.Utils;
using DomainWatcher.Core.Contracts;
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
            return HttpResponse.PlainText($"{domain} is already watched! Status: " + WhoisResponseDescriptiveStatus.Get(latestAvailableWhoisResponse));
        }

        var domainSupportedResult = await client.IsDomainSupported(domain);

        if (!domainSupportedResult.IsSupported)
        {
            logger.LogInformation("Domain {DomainUrl} unsupported. Reason: {Reason}.", domain.FullName, domainSupportedResult.Reason);

            return HttpResponse.BadRequestWithReason($"Domain not supported. Reason: {domainSupportedResult.Reason}");
        }

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

        return HttpResponse.PlainText($"{domain} watched! Status: " + WhoisResponseDescriptiveStatus.Get(latestAvailableWhoisResponse));
    }
}
