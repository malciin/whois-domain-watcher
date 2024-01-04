using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace DomainWatcher.Infrastructure.Sqlite.Internal.Migrations;

internal abstract class Migration
{
    internal abstract Task Migrate(SqliteConnection sqlite, IDbTransaction dbTransaction);
}

internal abstract class SqlCommandMigration : Migration
{
    public abstract string Command { get; }

    internal override Task Migrate(SqliteConnection sqlite, IDbTransaction dbTransaction)
    {
        return sqlite.ExecuteAsync(Command, transaction: dbTransaction);
    }
}
