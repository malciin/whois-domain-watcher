using System.Diagnostics.CodeAnalysis;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Internal.Middlewares;
using DomainWatcher.Infrastructure.HttpServer.Internal.Services;
using DomainWatcher.Infrastructure.HttpServer.Internal.Values;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Infrastructure.HttpServer;

public class HttpServerBuilder : IHttpPipelineBuilder
{
    private readonly IServiceCollection serviceCollection;
    private readonly List<EndpointEntry> endpointEntries;

    public HttpServerBuilder(
        IServiceCollection serviceCollection,
        Func<IServiceProvider, HttpServerOptions> httpServerOptionsFunc)
    {
        this.serviceCollection = serviceCollection;
        this.endpointEntries = [];

        serviceCollection.AddSingleton(httpServerOptionsFunc);
        serviceCollection.AddSingleton(ctx => new EndpointsResolver(
            endpointEntries,
            ctx.GetRequiredService<ILogger<EndpointsResolver>>()));
        serviceCollection.AddSingleton<HttpServer>();
        serviceCollection.AddSingleton<HttpServerInfo>();
        serviceCollection.AddSingleton<IHttpServerInfo>(ctx => ctx.GetRequiredService<HttpServerInfo>());
    }

    public HttpServerBuilder AddEndpoint<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
        where T : class, IHttpEndpoint
    {
        endpointEntries.Add(new EndpointEntry
        {
            Path = T.Path,
            Method = T.Method,
            EndpointType = typeof(T)
        });
        serviceCollection.AddScoped<T>();

        return this;
    }

    public IHttpPipelineBuilder Use<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
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
}
