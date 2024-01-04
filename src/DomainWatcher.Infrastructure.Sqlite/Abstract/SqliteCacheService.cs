using System.Text;
using Dapper;
using DomainWatcher.Infrastructure.Sqlite.Internal;
using DomainWatcher.Infrastructure.Sqlite.Internal.TableRows;
using Microsoft.Data.Sqlite;

namespace DomainWatcher.Infrastructure.Sqlite.Abstract;

public abstract class SqliteCacheService : SqliteService
{
    public SqliteCacheService(SqliteConnection connection) : base(connection)
    {
    }

    protected async Task<byte[]?> GetBytesFromCacheOrNull(string key)
    {
        var cacheEntry = await Connection.QuerySingleOrDefaultAsync<CacheRow>($"""
            SELECT *
            FROM {TableNames.Cache}
            WHERE {nameof(CacheRow.Key)} = @key
              AND {nameof(CacheRow.ExpirationTimestamp)} > @timestamp
            """,
            new { key = key, timestamp = DateTime.UtcNow });
     
        return cacheEntry?.Value;
    }

    protected async Task<string?> GetStringFromCacheOrNull(string key)
    {
        var bytes = await GetBytesFromCacheOrNull(key);

        return bytes == null
            ? null
            : Encoding.UTF8.GetString(bytes);
    }

    protected Task SetCache(string key, byte[] value, TimeSpan timeToLive)
    {
        var cacheTimestamp = DateTime.UtcNow;

        return Connection.ExecuteAsync($"""
            INSERT INTO {TableNames.Cache}
                (
                    {nameof(CacheRow.Key)},
                    {nameof(CacheRow.Value)},
                    {nameof(CacheRow.Timestamp)},
                    {nameof(CacheRow.ExpirationTimestamp)}
                )
                VALUES
                (@key, @value, @timestamp, @expirationTimestamp)
            ON CONFLICT({nameof(CacheRow.Key)}) DO UPDATE SET
                {nameof(CacheRow.Value)} = @value,
                {nameof(CacheRow.Timestamp)} = @timestamp,
                {nameof(CacheRow.ExpirationTimestamp)} = @expirationTimestamp;
            """,
            new
            {
                key = key,
                value = value,
                timestamp = cacheTimestamp,
                expirationTimestamp = cacheTimestamp + timeToLive
            });
    }

    protected Task SetCache(string key, string value, TimeSpan timeToLive)
    {
        return SetCache(key, Encoding.UTF8.GetBytes(value), timeToLive);
    }
}
