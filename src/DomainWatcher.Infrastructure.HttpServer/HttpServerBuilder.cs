using System.Diagnostics.CodeAnalysis;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Internal.Middlewares;
using DomainWatcher.Infrastructure.HttpServer.Internal.Services;
using DomainWatcher.Infrastructure.HttpServer.Internal.Values;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Infrastructure.HttpServer;

public class HttpServerBuilder
{
    private readonly IServiceCollection serviceCollection;
    private readonly List<EndpointEntry> endpointEntries;

    private bool endpointsFeatureEnabled = false;

    public HttpServerBuilder(
        IServiceCollection serviceCollection,
        Func<IServiceProvider, HttpServerOptions> httpServerOptionsFunc)
    {
        this.serviceCollection = serviceCollection;
        this.endpointEntries = [];

        serviceCollection.AddSingleton(httpServerOptionsFunc);
        serviceCollection.AddSingleton<HttpServer>();
        serviceCollection.AddSingleton<HttpServerInfo>();
        serviceCollection.AddSingleton<IHttpServerInfo>(ctx => ctx.GetRequiredService<HttpServerInfo>());
    }

    public HttpServerBuilder UseEndpoint<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
        where T : class, IHttpEndpoint
    {
        EnableEndpointsFeature();

        endpointEntries.Add(new EndpointEntry
        {
            Path = T.Path,
            Method = T.Method,
            EndpointType = typeof(T)
        });
        serviceCollection.AddScoped<T>();

        return this;
    }

    public HttpServerBuilder UseMiddleware<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
        where T : class, IRequestMiddleware
    {
        serviceCollection.AddSingleton<IRequestMiddleware, T>();

        return this;
    }

    public HttpServerBuilder RegisterAsHostedService()
    {
        serviceCollection.AddHostedService<HttpServerHostedService>();

        return this;
    }

    private void EnableEndpointsFeature()
    {
        if (endpointsFeatureEnabled)
        {
            return;
        }

        serviceCollection.AddSingleton(ctx => new EndpointsResolver(
            endpointEntries,
            ctx.GetRequiredService<ILogger<EndpointsResolver>>()));

        UseMiddleware<EndpointDispatcherMiddleware>();

        endpointsFeatureEnabled = true;
    }
}
