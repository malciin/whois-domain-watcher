namespace DomainWatcher.Infrastructure.Sqlite.Internal.Migrations;

internal class _002_AddStatusAndSourceServerToWhoisResponsesTable : SqlCommandMigration
{
    public override string Command => """
        DROP INDEX IX_WhoisResponses_DomainId;
        DROP TABLE WhoisResponses;

        CREATE TABLE WhoisResponses(
            DomainId INTEGER NOT NULL,
            SourceServer TEXT NOT NULL,
            Status INTEGER NOT NULL,
            QueryTimestamp INTEGER NOT NULL,
            Registration INTEGER NULL,
            Expiration INTEGER NULL,
            RawResponse TEXT NOT NULL,
            FOREIGN KEY(DomainId) REFERENCES Domains(Id));

        CREATE INDEX IX_WhoisResponses_DomainId ON WhoisResponses(DomainId);
        """;
}
