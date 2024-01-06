using DomainWatcher.Infrastructure.Sqlite.Extensions;
using DomainWatcher.Infrastructure.Sqlite.IntegrationTests.TestInfrastructure;
using Microsoft.Data.Sqlite;

namespace DomainWatcher.Infrastructure.Sqlite.IntegrationTests;

[Parallelizable(ParallelScope.All)]
public class SqliteDbMigratorTests : SqliteIntegrationTestFixture
{
    private const int ExpectedVersion = 3;
    private SqliteDbMigrator migrator;

    protected override void AdditionalSetupSteps()
    {
        migrator = ResolveService<SqliteDbMigrator>();
    }

    [Test]
    public async Task GivenEmptyDatabase_GettingDbVersionReturns0AndDoesNotMutateDb()
    {
        var currentVersion = await migrator.GetCurrentVersion();

        Assert.That(currentVersion, Is.Zero);
        Assert.That(await GetTableNames(), Is.Empty);
    }

    [Test]
    public async Task CorrectlyMigratesDb()
    {
        await migrator.MigrateIfNecessary();

        Assert.That(await migrator.GetCurrentVersion(), Is.EqualTo(ExpectedVersion));
        Assert.That(await GetTableNames(), Is.EquivalentTo(new[]
        {
            "__Migrations",
            "__Cache",
            "Domains",
            "WhoisResponses",
            "sqlite_sequence"
        }));
    }

    private async Task<IEnumerable<string>> GetTableNames()
    {
        return await SqliteConnection
            .AsyncRead("SELECT name FROM sqlite_master WHERE type='table';", x => x.GetString(0))
            .ToListAsync();
    }
}
