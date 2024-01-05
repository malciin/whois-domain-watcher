using DomainWatcher.Cli.Formatters;
using DomainWatcher.Core.Repositories;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Cli.Endpoints;

public class IndexEndpoint(
    IDomainsRepository domainsRepository,
    WatchedDomainsResponseFormatter responseFormatter) : IHttpEndpoint
{
    public static HttpMethod Method => HttpMethod.Get;

    public static string Path => "/";

    public async Task<HttpResponse> Handle(HttpRequest request)
    {
        var response = await responseFormatter.CreateResponse(domainsRepository.GetWatchedDomains());

        return HttpResponse.PlainText(response);
    }
}
