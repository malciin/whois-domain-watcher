using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Infrastructure.Sqlite.IntegrationTests.TestInfrastructure;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class SqliteIntegrationTestFixture
{
    public SqliteConnection SqliteConnection { get; private set; }

    private ServiceProvider rootServiceProvider;
    private IServiceScope testScope;

    [SetUp]
    public void SetUp()
    {
        rootServiceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddConsole();
            })
            .AddSqlite("Data Source=:memory:")
            .BuildServiceProvider();

        testScope = rootServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

        SqliteConnection = ResolveService<SqliteConnection>();
        ResolveService<ILogger<SqliteIntegrationTestFixture>>().LogInformation($"Using SqliteConnection with '{SqliteConnection.ConnectionString}' connection string.");

        AdditionalSetupSteps();
    }

    [TearDown]
    public void TearDown()
    {
        testScope.Dispose();
        rootServiceProvider.Dispose();
    }

    protected virtual void AdditionalSetupSteps()
    {
    }

    protected T ResolveService<T>() where T : notnull => testScope.ServiceProvider.GetRequiredService<T>();
}
