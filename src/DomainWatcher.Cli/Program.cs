using DomainWatcher.Cli.Extensions;
using DomainWatcher.Core;
using DomainWatcher.Infrastructure.Cache.Memory;
using DomainWatcher.Infrastructure.HttpServer;
using DomainWatcher.Infrastructure.Sqlite;
using DomainWatcher.Infrastructure.Sqlite.Cache;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

var hostBuilder = Host.CreateDefaultBuilder(args);

hostBuilder
    .ConfigureLogging((_, logging) => logging.AddSerilog())
    .ConfigureServices(x => x
        .AddCore()
        .AddSqlite()
        .AddCache<WhoisServerUrlResolverSqliteCache>() // longer persisted cache
        .AddCache<WhoisServerUrlResolverMemoryCache>() // shortlived memcache
        .AddCli()
        .AddInternalHttpServer(x => x.Port = 8051)
        .UseEndpointsFromCurrentAssembly()
        .RegisterAsHostedService())
    .UseSerilog((_, configuration) => configuration
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContextName}] {Message:lj}{NewLine}{Exception}")
        .MinimumLevel.Override("Microsoft.Extensions.Hosting", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Warning)
        .MinimumLevel.Debug()
        .Enrich.FromLogContext()
        .Enrich.WithComputed("SourceContextName", "Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)"));

using var host = hostBuilder.Build();

await host.Services.GetRequiredService<SqliteDbMigrator>().MigrateIfNecessary();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Press CTRL+C to stop.");

await host.RunAsync();
