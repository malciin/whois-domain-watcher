using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Cli.Endpoints;

public class WatchDomainEndpoint : IHttpEndpoint
{
    public static HttpMethod Method => HttpMethod.Post;

    public static string Path => "/";

    public Task<HttpResponse> Handle(HttpRequest request)
    {
        return HttpResponse.PlainText("OK - Watch");
    }
}
