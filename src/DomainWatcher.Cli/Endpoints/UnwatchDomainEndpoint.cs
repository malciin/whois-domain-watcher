using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Cli.Endpoints;

public class UnwatchDomainEndpoint : IHttpEndpoint
{
    public static HttpMethod Method => HttpMethod.Delete;

    public static string Path => "/";

    public Task<HttpResponse> Handle(HttpRequest request)
    {
        return HttpResponse.PlainText("Implement unwatch");
    }
}
