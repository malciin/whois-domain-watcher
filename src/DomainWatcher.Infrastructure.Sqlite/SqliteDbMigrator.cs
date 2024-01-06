using System.Data;
using DomainWatcher.Infrastructure.Sqlite.Abstract;
using DomainWatcher.Infrastructure.Sqlite.Extensions;
using DomainWatcher.Infrastructure.Sqlite.Internal.Values;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Infrastructure.Sqlite;

public partial class SqliteDbMigrator(
    SqliteConnection connection,
    ILogger<SqliteDbMigrator> logger) : SqliteService(connection)
{
    public async Task MigrateIfNecessary()
    {
        logger.LogTrace("Migrating '{ConnString}'", Connection.ConnectionString);

        await Connection.OpenAsync();

        var version = await GetCurrentVersion();
        var migrationsToRun = GetMigrations().Where(x => x.Number > version);

        if (!migrationsToRun.Any())
        {
            logger.LogDebug("No migrations to run");

            return;
        }

        foreach (var migration in migrationsToRun)
        {
            using var transaction = Connection.BeginTransaction();

            await migration.Run(Connection, transaction);
            await Connection.ExecuteAsync("""
                CREATE TABLE IF NOT EXISTS __Migrations (
                    Version INTEGER PRIMARY KEY,
                    MigrationName TEXT NOT NULL,
                    AppliedOn TEXT NOT NULL)
                    WITHOUT ROWID;

                INSERT INTO __Migrations(Version, MigrationName, AppliedOn) VALUES (@version, @migrationName, @appliedOn)
                """,
                new Dictionary<string, (DbType, object?)>
                {
                    ["version"] = (DbType.Int32, migration.Number),
                    ["migrationName"] = (DbType.String, migration.Name),
                    ["appliedOn"] = (DbType.DateTime, DateTime.UtcNow)
                },
                transaction: transaction);

            await transaction.CommitAsync();

            logger.LogInformation("{Number} {Name} migration applied.", migration.Number, migration.Name);
        }
    }

    public async Task<int> GetCurrentVersion()
    {
        var migrationsTableDoesExists = await Connection.QuerySingleOrDefaultAsync("""
            SELECT name FROM sqlite_master WHERE type='table' AND name='__Migrations'
            """,
            x => true,
            false);

        if (!migrationsTableDoesExists) return 0;

        return await Connection.QuerySingleOrDefaultAsync("""
            SELECT Version FROM __Migrations ORDER BY Version DESC LIMIT(1);
            """,
            x => x.GetInt32(0),
            0);
    }

    private static partial IEnumerable<MigrationEntry> GetMigrations();
}
