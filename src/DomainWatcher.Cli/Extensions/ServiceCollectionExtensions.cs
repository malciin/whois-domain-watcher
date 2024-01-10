using DomainWatcher.Cli.Formatters;
using DomainWatcher.Cli.Logging.Sinks;
using DomainWatcher.Cli.Services;
using DomainWatcher.Cli.Settings;
using DomainWatcher.Infrastructure.HttpServer;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DomainWatcher.Cli.Extensions;

public static partial class ServiceCollectionExtensions
{
    // TODO: Check if sctuor will be AoT compatible (at the development time it was not compatible).
    //public static IServiceCollection AddCache<TInterface, TCache>(this IServiceCollection services)
    //    where TCache : TInterface
    //{
    //    services.Decorate(typeof(TInterface), typeof(TCache));

    //    return services;
    //}

    public static IServiceCollection AddCache<TInterface, TCache>(this IServiceCollection services, Func<IServiceProvider, TCache> factory)
        where TInterface : class
        where TCache : class, TInterface
    {
        services.AddScoped<TInterface, TCache>(factory);

        return services;
    }

    public static IServiceCollection AddCliServices(this IServiceCollection services)
    {
        services.AddScoped<WatchedDomainsResponseFormatter>();

        services.AddSingleton<HostCancellation>();
        services.AddSingleton<HostStoppingWhenFatalLogSink>();
        services.AddSingleton<CorsHeaderSettings>();

        return services;
    }

    public static IServiceCollection AddHttpServer(this IServiceCollection services)
    {
        return AddHttpServer(services, x => x.UseEndpoints());
    }

    public static IServiceCollection AddHttpServer(this IServiceCollection services, Action<IHttpPipelineBuilder> createHttpPipeline)
    {
        createHttpPipeline(services
            .AddInternalHttpServer((s, options) =>
            {
                options.Port = int.Parse(s.GetRequiredService<IConfiguration>()["Port"]!);
            })
            .AddSourceGeneratedEndpoints()
            .RegisterAsHostedService());

        return services;
    }

    private static partial HttpServerBuilder AddSourceGeneratedEndpoints(this HttpServerBuilder builder);
}
