using DomainWatcher.Core.Repositories;
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
        services
            .AddScoped(ctx => new SqliteConnection(connectionString))
            .AddScoped<SqliteDbMigrator>()
            .AddScoped<IDomainsRepository, SqliteDomainsRepository>()
            .AddScoped<IWhoisResponsesRepository, SqliteWhoisResponsesRepository>();

        return services;
    }

    public static IServiceCollection AddSqliteCache(this IServiceCollection services)
    {
        return services;
    }
}
