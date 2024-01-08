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

    public Task<HttpResponse> Handle(HttpRequest request)
    {
        return responseFormatter.CreateResponse(domainsRepository.GetWatchedDomains());
    }
}
