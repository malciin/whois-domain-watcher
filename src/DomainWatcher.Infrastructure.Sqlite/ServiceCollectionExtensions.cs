using System.Reflection;
using Dapper;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Repositories;
using DomainWatcher.Infrastructure.Sqlite.Abstract;
using DomainWatcher.Infrastructure.Sqlite.Internal.TypeHandlers;
using DomainWatcher.Infrastructure.Sqlite.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace DomainWatcher.Infrastructure.Sqlite;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlite(this IServiceCollection services)
    {
        return AddSqlite(services, "Data Source=sqlite.db");
    }

    public static IServiceCollection AddSqlite(this IServiceCollection services, string connectionString)
    {
        SqlMapper.AddTypeHandler(new DateTimeUtcTypeHandler());

        services
            .AddScoped(ctx => new SqliteConnection(connectionString))
            .AddScoped<SqliteDbMigrator>()
            .AddScoped<IDomainsRepository, SqliteDomainsRepository>()
            .AddScoped<IWhoisResponsesRepository, SqliteWhoisResponsesRepository>();

        return services;
    }

    public static IServiceCollection AddSqliteCacheFor<T>(this IServiceCollection services)
    {
        var requestedCacheFor = typeof(T);
        availableCacheServicesByInterface ??= typeof(SqliteCacheService).Assembly
            .GetInstantiableTypesAssignableTo<SqliteCacheService>()
            .SelectMany(x => x.GetInterfaces().Select(i => (InterfaceType: i, CacheImplementation: x)))
            .ToDictionary(x => x.InterfaceType, x => x.CacheImplementation);

        if (!availableCacheServicesByInterface.TryGetValue(requestedCacheFor, out var cacheImplementationType))
        {
            throw new NotImplementedException(
                $"Sqlite cache is not implemented for {requestedCacheFor}. There are implemented sqlite caches only for:"
                + Environment.NewLine
                + string.Join(Environment.NewLine, availableCacheServicesByInterface.Keys.Select(x => x.FullName)));
        }

        services.Decorate(requestedCacheFor, cacheImplementationType);

        return services;
    }

    private static Dictionary<Type, Type>? availableCacheServicesByInterface;
}
