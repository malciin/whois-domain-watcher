using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Internal.Services;
using DomainWatcher.Infrastructure.HttpServer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Infrastructure.HttpServer.Internal.Middlewares;

internal class EndpointDispatcherMiddleware(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<EndpointDispatcherMiddleware> logger,
    EndpointsResolver endpoints) : IRequestMiddleware
{
    public async ValueTask<HttpResponse?> TryProcess(HttpRequest request)
    {
        if (!endpoints.TryResolve(request, out var endpointType))
        {
            return null;
        }

        logger.LogTrace("{Method} {Url} resolved to {Endpoint}", request.Method, request.RelativeUrl, endpointType!.FullName);

        using var serviceScope = serviceScopeFactory.CreateScope();
        var endpoint = (IHttpEndpoint)serviceScope.ServiceProvider.GetRequiredService(endpointType);

        return await endpoint.Handle(request);
    }
}
