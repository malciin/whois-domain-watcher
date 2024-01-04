using Dapper;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Infrastructure.Sqlite.Abstract;
using DomainWatcher.Infrastructure.Sqlite.Internal.Migrations;
using DomainWatcher.Infrastructure.Sqlite.Internal.Values;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Infrastructure.Sqlite;

public class SqliteDbMigrator(
    SqliteConnection connection,
    ILogger<SqliteDbMigrator> logger) : SqliteService(connection)
{
    public async Task MigrateIfNecessary()
    {
        logger.LogTrace("Migrating '{ConnString}'", Connection.ConnectionString);

        await Connection.OpenAsync();

        var version = await GetCurrentVersion();
        var migrationsToRun = GetMigrationsDetails().Where(x => x.Number > version);

        if (!migrationsToRun.Any())
        {
            logger.LogDebug("No migrations to run");

            return;
        }

        foreach (var migration in migrationsToRun)
        {
            using var transaction = await Connection.BeginTransactionAsync();

            await migration.Run(Connection, transaction);
            await Connection.ExecuteAsync("""
                CREATE TABLE IF NOT EXISTS __Migrations (
                    Version INTEGER PRIMARY KEY,
                    MigrationName TEXT NOT NULL,
                    AppliedOn TEXT NOT NULL)
                    WITHOUT ROWID;

                INSERT INTO __Migrations(Version, MigrationName, AppliedOn) VALUES (@version, @migrationName, @appliedOn)
                """,
                new { version = migration.Number, migrationName = migration.Name, appliedOn = DateTime.UtcNow },
                transaction: transaction);

            await transaction.CommitAsync();

            logger.LogInformation("{Number} {Name} migration applied.", migration.Number, migration.Name);
        }
    }

    public async Task<int> GetCurrentVersion()
    {
        var migrationsTableDoesNotExist = string.IsNullOrEmpty(await Connection.QuerySingleOrDefaultAsync<string>("""
            SELECT name FROM sqlite_master WHERE type='table' AND name='__Migrations'
            """));

        if (migrationsTableDoesNotExist) return 0;

        return await Connection.QuerySingleOrDefaultAsync<int>("""
            SELECT Version FROM __Migrations ORDER BY Version DESC LIMIT(1);
            """);
    }

    private IOrderedEnumerable<MigrationEntry> GetMigrationsDetails()
    {
        return typeof(Migration).Assembly
            .GetInstantiableTypesAssignableTo<Migration>()
            .Select(type => new MigrationEntry(type))
            .OrderBy(x => x.Number);
    }
}
