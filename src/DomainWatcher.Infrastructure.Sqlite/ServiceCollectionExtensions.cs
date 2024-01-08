using DomainWatcher.Core.Repositories;
using DomainWatcher.Infrastructure.Sqlite.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DomainWatcher.Infrastructure.Sqlite;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlite(this IServiceCollection services)
    {
        return AddSqlite(services, ctx => $"Data Source={ctx.GetRequiredService<IConfiguration>()["DbPath"]!}");
    }

    public static IServiceCollection AddSqlite(this IServiceCollection services, string connectionString)
    {
        return AddSqlite(services, _ => connectionString);
    }

    public static IServiceCollection AddSqlite(this IServiceCollection services, Func<IServiceProvider, string> connectionStringProvider)
    {
        services
            .AddScoped(ctx => new SqliteConnection(connectionStringProvider(ctx)))
            .AddScoped<SqliteDbMigrator>()
            .AddScoped<IDomainsRepository, SqliteDomainsRepository>()
            .AddScoped<IWhoisResponsesRepository, SqliteWhoisResponsesRepository>();

        return services;
    }
}
