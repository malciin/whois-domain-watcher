using System.Data;
using DomainWatcher.Core.Enums;
using DomainWatcher.Core.Repositories;
using DomainWatcher.Core.Values;
using DomainWatcher.Infrastructure.Sqlite.Abstract;
using DomainWatcher.Infrastructure.Sqlite.Extensions;
using DomainWatcher.Infrastructure.Sqlite.Internal;
using DomainWatcher.Infrastructure.Sqlite.Internal.Extensions;
using DomainWatcher.Infrastructure.Sqlite.Internal.TableRows;
using Microsoft.Data.Sqlite;

namespace DomainWatcher.Infrastructure.Sqlite.Repositories;

public class SqliteWhoisResponsesRepository(SqliteConnection connection) : SqliteService(connection), IWhoisResponsesRepository
{
    public Task Add(WhoisResponse whoisResponse)
    {
        if (whoisResponse.Id != 0)
        {
            throw new ArgumentException($"Cannot add {nameof(WhoisResponse)} with explicit {nameof(WhoisResponse.Id)} set to {whoisResponse.Id}");
        }

        return Connection.ExecuteAsync($"""
            INSERT INTO {TableNames.WhoisResponses}
                (
                    {nameof(WhoisResponseRow.DomainId)},
                    {nameof(WhoisResponseRow.SourceServer)},
                    {nameof(WhoisResponseRow.Status)},
                    {nameof(WhoisResponseRow.QueryTimestamp)},
                    {nameof(WhoisResponseRow.Registration)},
                    {nameof(WhoisResponseRow.Expiration)},
                    {nameof(WhoisResponseRow.RawResponse)}
                )
                VALUES
                (
                    ( SELECT {nameof(DomainRow.Id)} FROM {TableNames.Domains} WHERE {nameof(DomainRow.Domain)} = @domainName ),
                    @sourceServer,
                    @status,
                    @queryTimestamp,
                    @registration,
                    @expiration,
                    @rawResponse
                )
            """,
            new Dictionary<string, (DbType, object?)>
            {
                ["sourceServer"] = (DbType.String, whoisResponse.SourceServer),
                ["status"] = (DbType.Int32, whoisResponse.Status),
                ["domainName"] = (DbType.String, whoisResponse.Domain.FullName),
                ["queryTimestamp"] = (DbType.DateTime, whoisResponse.QueryTimestamp),
                ["registration"] = (DbType.DateTime, whoisResponse.Registration),
                ["expiration"] = (DbType.DateTime, whoisResponse.Expiration),
                ["rawResponse"] = (DbType.String, whoisResponse.RawResponse)
            });
    }

    public IAsyncEnumerable<long> GetWhoisResponsesIdsFor(Domain domain)
    {
        return Connection.AsyncRead($"""
            SELECT W.{nameof(WhoisResponseRow.Id)}
            FROM {TableNames.WhoisResponses} W
            LEFT JOIN {TableNames.Domains} D ON W.{nameof(WhoisResponseRow.DomainId)} = D.{nameof(DomainRow.Id)}
            WHERE D.{nameof(DomainRow.Domain)} = @domainName
            ORDER BY W.{nameof(WhoisResponseRow.QueryTimestamp)}
            """,
            new Dictionary<string, (DbType, object?)> { ["domainName"] = (DbType.String, domain.FullName) },
            x => x.GetInt64(0));
    }

    public Task<WhoisResponse?> GetLatestFor(Domain domain)
    {
        return Connection.QuerySingleOrDefaultAsync($"""
            SELECT
                W.{nameof(WhoisResponseRow.Id)},
                D.{nameof(DomainRow.Domain)},
                W.{nameof(WhoisResponseRow.SourceServer)},
                W.{nameof(WhoisResponseRow.Status)},
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
            new Dictionary<string, (DbType, object?)> { ["domainName"] = (DbType.String, domain.FullName) },
            parser => new WhoisResponse
            {
                Id = parser.GetInt64(0),
                Domain = new Domain(parser.GetString(1)),
                SourceServer = parser.GetString(2),
                Status = (WhoisResponseStatus)parser.GetInt32(3),
                QueryTimestamp = parser.GetDateTime(4),
                Registration = parser.GetNullableDateTime(5),
                Expiration = parser.GetNullableDateTime(6),
                RawResponse = parser.GetString(7)
            },
            null);
    }
}
