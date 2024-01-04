namespace DomainWatcher.Infrastructure.Sqlite.Internal.Migrations;

internal class _001_InitialMigration : SqlCommandMigration
{
    public override string Command => $"""
        CREATE TABLE Domains (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Domain TEXT UNIQUE NOT NULL,
            IsWatched INTEGER NOT NULL);

        CREATE TABLE WhoisResponses (
            DomainId INTEGER NOT NULL,
            QueryTimestamp INTEGER NOT NULL,
            Registration INTEGER NULL,
            Expiration INTEGER NULL,
            RawResponse TEXT NOT NULL,
            FOREIGN KEY(DomainId) REFERENCES Domains(Id));

        CREATE INDEX IX_WhoisResponses_DomainId ON WhoisResponses(DomainId);

        CREATE TABLE __Cache (
            Key TEXT PRIMARY KEY NOT NULL,
            Value BLOB NOT NULL,
            Timestamp INTEGER NOT NULL,
            ExpirationTimestamp INTEGER NOT NULL)
            WITHOUT ROWID;
        """;
}
