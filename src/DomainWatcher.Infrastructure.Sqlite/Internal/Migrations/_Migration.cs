using System.Data;
using DomainWatcher.Infrastructure.Sqlite.Extensions;
using Microsoft.Data.Sqlite;

namespace DomainWatcher.Infrastructure.Sqlite.Internal.Migrations;

internal abstract class Migration
{
    internal abstract Task Migrate(SqliteConnection sqlite, SqliteTransaction dbTransaction);
}

internal abstract class SqlCommandMigration : Migration
{
    public abstract string Command { get; }

    internal override Task Migrate(SqliteConnection sqlite, SqliteTransaction dbTransaction)
    {
        return sqlite.ExecuteAsync(Command, transaction: dbTransaction);
    }
}
