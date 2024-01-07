﻿using DomainWatcher.Cli.Formatters;
using DomainWatcher.Infrastructure.HttpServer;
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

        return services;
    }

    public static IServiceCollection AddHttpServer(this IServiceCollection services)
    {
        services
            .AddInternalHttpServer((s, options) =>
            {
                var port = s.GetRequiredService<IConfiguration>()["Port"];

                options.Port = port != null ? int.Parse(port) : 0;
            })
            .UseEndpointsSourceGen()
            .RegisterAsHostedService();

        return services;
    }

    private static partial HttpServerBuilder UseEndpointsSourceGen(this HttpServerBuilder builder);
}
