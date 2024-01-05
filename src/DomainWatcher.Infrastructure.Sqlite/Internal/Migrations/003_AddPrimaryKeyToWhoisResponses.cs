namespace DomainWatcher.Infrastructure.Sqlite.Internal.Migrations;

internal class _003_AddPrimaryKeyToWhoisResponses : SqlCommandMigration
{
    // Why creating new tmp table: https://stackoverflow.com/a/4007086
    public override string Command => """
        CREATE TABLE WhoisResponsesTemp (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            DomainId INTEGER NOT NULL,
            SourceServer TEXT NOT NULL,
            Status INTEGER NOT NULL,
            QueryTimestamp INTEGER NOT NULL,
            Registration INTEGER NULL,
            Expiration INTEGER NULL,
            RawResponse TEXT NOT NULL,
            FOREIGN KEY(DomainId) REFERENCES Domains(Id));

        INSERT INTO WhoisResponsesTemp (
            DomainId,
            SourceServer,
            Status,
            QueryTimestamp,
            Registration,
            Expiration,
            RawResponse)
            SELECT
                DomainId,
                SourceServer,
                Status,
                QueryTimestamp,
                Registration,
                Expiration,
                RawResponse
            FROM WhoisResponses;

        DROP INDEX IX_WhoisResponses_DomainId;
        DROP TABLE WhoisResponses;

        ALTER TABLE WhoisResponsesTemp RENAME TO WhoisResponses;

        CREATE INDEX IX_WhoisResponses_DomainId ON WhoisResponses(DomainId);
        """;
}
