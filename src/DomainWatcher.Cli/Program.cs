using DomainWatcher.Cli.Extensions;
using DomainWatcher.Cli.Internal.LogEnrichers;
using DomainWatcher.Core;
using DomainWatcher.Core.Whois.Contracts;
using DomainWatcher.Core.Whois.Implementation;
using DomainWatcher.Infrastructure.Sqlite;
using DomainWatcher.Infrastructure.Sqlite.Cache;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

var hostBuilder = Host.CreateDefaultBuilder(args);

hostBuilder
    .ConfigureAppConfiguration(x => x.AddCommandLine(args))
    .ConfigureLogging((_, logging) => logging.AddSerilog())
    .ConfigureServices(x => x
        .AddCore()
        .AddSqlite()
        // Clunky way of adding decorator because Scrutor right now is not AOT compatible.
        .AddCache<IWhoisServerUrlResolver, WhoisServerUrlResolverSqliteCache>(ctx => new WhoisServerUrlResolverSqliteCache(
            ctx.GetRequiredService<SqliteConnection>(),
            ctx.GetRequiredService<ILogger<WhoisServerUrlResolverSqliteCache>>(),
            new WhoisServerUrlResolver(ctx.GetRequiredService<IWhoisRawResponseProvider>())))
        .AddHttpServer()
        .AddCliServices())
    .UseSerilog((_, configuration) => configuration
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContextName}] {Message:lj}{NewLine}{Exception}")
        .MinimumLevel.Override("Microsoft.Extensions.Hosting", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Warning)
        .MinimumLevel.Debug()
        .Enrich.FromLogContext()
        .Enrich.With<SourceContextNameEnricher>());

using var host = hostBuilder.Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

if (host.Services.GetRequiredService<IConfiguration>()["port"] == null)
{
    logger.LogWarning("No --port specified. Http server will start on port provided by the system. Try run specifyng port like --port 8080");
}

await host.Services.GetRequiredService<SqliteDbMigrator>().MigrateIfNecessary();

logger.LogInformation("Press CTRL+C to stop.");

await host.RunAsync();