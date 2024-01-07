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
            return HttpResponse.PlainText(latestAvailableWhoisResponse.RawResponse.TrimEnd());
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

        return HttpResponse.PlainText(latestAvailableWhoisResponse.RawResponse.TrimEnd());
    }
}
