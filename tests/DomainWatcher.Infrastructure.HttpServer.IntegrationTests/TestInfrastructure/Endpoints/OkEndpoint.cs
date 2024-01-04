using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Infrastructure.HttpServer.IntegrationTests.TestInfrastructure.Endpoints;

internal class OkEndpoint : IHttpEndpoint
{
    public static HttpMethod Method => HttpMethod.Get;

    public static string Path => "/ok";

    public const string StringResponse = "Response from OK endpoint";

    public Task<HttpResponse> Handle(HttpRequest request)
    {
        return HttpResponse.PlainText(StringResponse);
    }
}
