using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Infrastructure.HttpServer.IntegrationTests.TestInfrastructure.Endpoints;

/// <summary>
/// Endpoint for testing where it always respond with request.Body as plaintext
/// </summary>
internal class EchoEndpoint : IHttpEndpoint
{
    public static HttpMethod Method => HttpMethod.Post;

    public static string Path => "/echo";

    public Task<HttpResponse> Handle(HttpRequest request)
    {
        return HttpResponse.PlainText(request.Body);
    }
}
