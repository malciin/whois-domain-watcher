using Dapper;
using DomainWatcher.Core.Repositories;
using DomainWatcher.Core.Values;
using DomainWatcher.Infrastructure.Sqlite.Abstract;
using DomainWatcher.Infrastructure.Sqlite.Internal;
using DomainWatcher.Infrastructure.Sqlite.Internal.TableRows;
using Microsoft.Data.Sqlite;

namespace DomainWatcher.Infrastructure.Sqlite.Repositories;

public class SqliteWhoisResponsesRepository(SqliteConnection connection) : SqliteService(connection), IWhoisResponsesRepository
{
    public Task Add(WhoisResponse whoisResponse)
    {
        return Connection.ExecuteAsync($"""
            INSERT INTO {TableNames.WhoisResponses}
                (
                    {nameof(WhoisResponseRow.DomainId)},
                    {nameof(WhoisResponseRow.QueryTimestamp)},
                    {nameof(WhoisResponseRow.Registration)},
                    {nameof(WhoisResponseRow.Expiration)},
                    {nameof(WhoisResponseRow.RawResponse)}
                )
                VALUES
                (
                    ( SELECT {nameof(DomainRow.Id)} FROM {TableNames.Domains} WHERE {nameof(DomainRow.Domain)} = @domainName ),
                    @queryTimestamp,
                    @registration,
                    @expiration,
                    @rawResponse
                )
            """,
            new
            {
                domainName = whoisResponse.Domain.FullName,
                queryTimestamp = whoisResponse.QueryTimestamp,
                registration = whoisResponse.Registration,
                expiration = whoisResponse.Expiration,
                rawResponse = whoisResponse.RawResponse
            });
    }

    public async Task<WhoisResponse?> GetLatestFor(Domain domain)
    {
        var row = await Connection.QuerySingleOrDefaultAsync<WhoisResponseReadEntry>($"""
            SELECT
                D.{nameof(DomainRow.Domain)},
                W.{nameof(WhoisResponseRow.QueryTimestamp)},
                W.{nameof(WhoisResponseRow.Registration)},
                W.{nameof(WhoisResponseRow.Expiration)},
                W.{nameof(WhoisResponseRow.RawResponse)}
            FROM {TableNames.WhoisResponses} W
            LEFT JOIN {TableNames.Domains} D ON W.{nameof(WhoisResponseRow.DomainId)} = D.{nameof(DomainRow.Id)}
            WHERE D.{nameof(DomainRow.Domain)} = @domainName
            ORDER BY W.{nameof(WhoisResponseRow.QueryTimestamp)} DESC
            LIMIT 1
            """,
            new { domainName = domain.FullName });

        if (row == null) return null;

        return new WhoisResponse
        {
            Domain = new Domain(row.Domain),
            Expiration = row.Expiration,
            QueryTimestamp = row.QueryTimestamp,
            RawResponse = row.RawResponse,
            Registration = row.Registration
        };
    }

    private class WhoisResponseReadEntry
    {
        public string Domain { get; set; }

        public DateTime QueryTimestamp { get; set; }

        public DateTime? Registration { get; set; }

        public DateTime? Expiration { get; set; }

        public string RawResponse { get; set; }
    }
}
