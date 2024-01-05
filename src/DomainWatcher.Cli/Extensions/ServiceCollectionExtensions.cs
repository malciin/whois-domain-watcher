using DomainWatcher.Cli.Formatters;
using Microsoft.Extensions.DependencyInjection;

namespace DomainWatcher.Cli.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCache<T>(this IServiceCollection services)
    {
        services.Decorate(typeof(T).GetInterfaces().Single(), typeof(T));

        return services;
    }

    public static IServiceCollection AddCli(this IServiceCollection services)
    {
        services.AddScoped<WatchedDomainsResponseFormatter>();

        return services;
    }
}
