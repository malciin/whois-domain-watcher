using System.Data;
using System.Text.RegularExpressions;
using DomainWatcher.Infrastructure.Sqlite.Internal.Migrations;
using Microsoft.Data.Sqlite;

namespace DomainWatcher.Infrastructure.Sqlite.Internal.Values;

internal class MigrationEntry
{
    public int Number { get; }
    
    public string Name { get; }
    
    private readonly Type migrationType;

    public MigrationEntry(Type migrationType)
    {
        var match = MigrationNameRegex.Match(migrationType.Name);

        if (!match.Success)
        {
            throw new Exception("Migration classes should follow up following name convention: _MigrationNumber_MigrationName");
        }

        this.migrationType = migrationType;
        Number = int.Parse(match.Groups[1].Value);
        Name = match.Groups[2].Value;
    }

    public Task Run(SqliteConnection connection, IDbTransaction transaction)
    {
        var migrationInstance = (Migration)Activator.CreateInstance(migrationType)!;

        return migrationInstance.Migrate(connection, transaction);
    }

    private static readonly Regex MigrationNameRegex = new(@"_(\d+)_(\w+)");
}
