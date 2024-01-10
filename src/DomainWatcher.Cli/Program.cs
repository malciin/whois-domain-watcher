using DomainWatcher.Cli;
using DomainWatcher.Cli.Extensions;
using DomainWatcher.Cli.Logging.Enrichers;
using DomainWatcher.Cli.Logging.Sinks;
using DomainWatcher.Cli.Middlewares;
using DomainWatcher.Cli.Services;
using DomainWatcher.Core;
using DomainWatcher.Core.Whois.Contracts;
using DomainWatcher.Core.Whois.Implementation;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
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
    .ConfigureAppConfiguration(x => x
        .AddYamlString(ReferenceSettings.Yaml)
        .AddYamlFile("settings.yaml", optional: true)
        .AddCommandLine(args))
    .ConfigureLogging((_, logging) => logging.AddSerilog())
    .ConfigureServices(x => x
        .AddCore()
        .AddSqlite()
        .AddSerilog((services, configuration) => configuration
            .ReadFrom.Configuration(services.GetRequiredService<IConfiguration>())
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContextName}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Sink(services.GetRequiredService<HostStoppingWhenFatalLogSink>(), LogEventLevel.Fatal)
            .Enrich.FromLogContext()
            .Enrich.With<SourceContextNameEnricher>())
        // Clunky way of adding decorator because Scrutor right now is not AOT compatible.
        .AddCache<IWhoisServerUrlResolver, WhoisServerUrlResolverSqliteCache>(ctx => new WhoisServerUrlResolverSqliteCache(
            ctx.GetRequiredService<SqliteConnection>(),
            ctx.GetRequiredService<ILogger<WhoisServerUrlResolverSqliteCache>>(),
            new WhoisServerUrlResolver(ctx.GetRequiredService<IWhoisRawResponseProvider>())))
        .AddHttpServer(pipeline => pipeline
            .Use<CorsRequestHandlerMiddleware>()
            .Use<CurlNewLineAdderForPlainTextMiddleware>()
            .UseEndpoints())
        .AddCliServices());

using var host = hostBuilder.Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

if (host.Services.GetRequiredService<IConfiguration>()["Port"] == "0")
{
    logger.LogWarning("""
        Http server will use 0 port which means it will use port provided by the system.
        If you need static port try run --port 8080 or specify port in settings.yaml
        """);
}

await host.Services.GetRequiredService<SqliteDbMigrator>().MigrateIfNecessary();

logger.LogInformation("Press CTRL+C to stop.");

var hostCancellation = host.Services.GetRequiredService<HostCancellation>();
hostCancellation.Token.Register(() => logger.LogWarning("Stopping daemon because fatal error happend"));
await host.RunAsync(hostCancellation.Token);
