using System.Data;
using DomainWatcher.Infrastructure.Sqlite.Internal.Migrations;
using Microsoft.Data.Sqlite;

namespace DomainWatcher.Infrastructure.Sqlite.Internal.Values;

internal class MigrationEntry
{
    public required int Number { get; init; }
    
    public required string Name { get; init; }
    
    public required Func<Migration> Factory { get; init; }

    public Task Run(SqliteConnection connection, IDbTransaction transaction)
    {
        return Factory().Migrate(connection, transaction);
    }
}
