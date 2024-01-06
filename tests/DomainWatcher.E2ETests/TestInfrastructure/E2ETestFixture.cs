using DomainWatcher.Cli.Extensions;
using DomainWatcher.Core;
using DomainWatcher.Core.Whois.Contracts;
using DomainWatcher.Infrastructure.Cache.Memory;
using DomainWatcher.Infrastructure.HttpServer;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.Sqlite;
using DomainWatcher.Infrastructure.Sqlite.Cache;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace DomainWatcher.E2ETests.TestInfrastructure;

[TestFixture]
public abstract class E2ETestFixture
{
    protected HttpClient HttpClient { get; private set; }

    private string dbName;
    private IHost host;
    private IServiceScope testCaseScope;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        HttpClient = new HttpClient();

        dbName = $"e2e-db-artifacts/{GetType().Name}_{DateTime.UtcNow.Ticks}.db";

        Directory.CreateDirectory(Path.GetDirectoryName(dbName)!);
        
        host = Host.CreateDefaultBuilder()
            .ConfigureLogging((_, logging) => logging.AddSerilog())
            .ConfigureServices(x => x
                .AddCore()
                .AddSqlite($"Data Source={dbName}")
                .AddCache<IWhoisServerUrlResolver, WhoisServerUrlResolverSqliteCache>() // longer persisted cache
                .AddCache<IWhoisServerUrlResolver, WhoisServerUrlResolverMemoryCache>() // shortlived memcache
                .AddHttpServer()
                .AddCliServices())
            .UseSerilog((_, configuration) => configuration
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContextName}] {Message:lj}{NewLine}{Exception}")
                .MinimumLevel.Override("Microsoft.Extensions.Hosting", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Warning)
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .Enrich.WithComputed("SourceContextName", "Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)"))
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<E2ETestFixture>>();

        logger.LogInformation("Host built");
        logger.LogInformation("Database used for E2E {TestsClassName} will be stored as {DbName}", GetType().Name, dbName);

        await host.Services.GetRequiredService<SqliteDbMigrator>().MigrateIfNecessary();

        _ = host.StartAsync();

        await WaitForHttpServerToStart(waitLimit: TimeSpan.FromSeconds(1));
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await host.StopAsync();

        HttpClient.Dispose();
        host.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        testCaseScope = host.Services.CreateScope();
    }

    [TearDown]
    public void TearDown()
    {
        testCaseScope.Dispose();
    }

    protected T Resolve<T>() where T : notnull
    {
        return testCaseScope.ServiceProvider.GetRequiredService<T>();
    }

    private async Task WaitForHttpServerToStart(TimeSpan waitLimit)
    {
        var startTimestamp = DateTime.UtcNow;
        var serverInfo = host.Services.GetRequiredService<IHttpServerInfo>();

        while (DateTime.UtcNow - startTimestamp < waitLimit)
        {
            if (serverInfo.AssignedPort != 0) return;

            await Task.Delay(10);
        }

        throw new Exception($"Http server not started within {waitLimit.TotalSeconds}s. Giving up.");
    }
}
