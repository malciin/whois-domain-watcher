using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Cli.Endpoints;

public class FaviconEndpoint : IHttpEndpoint
{
    public static HttpMethod Method => HttpMethod.Get;

    public static string Path => "/favicon.ico";

    public Task<HttpResponse> Handle(HttpRequest request)
    {
        // to prevent that request to fall into WhoisDomainEndpoint
        // in future it could load favicon from file or something
        return HttpResponse.NotFound;
    }
}
