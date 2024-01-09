using System.Data;
using System.Text;
using DomainWatcher.Infrastructure.Sqlite.Extensions;
using DomainWatcher.Infrastructure.Sqlite.Internal;
using DomainWatcher.Infrastructure.Sqlite.Internal.TableRows;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Infrastructure.Sqlite.Abstract;

public abstract class SqliteCacheService : SqliteService
{
    private readonly ILogger logger;

    public SqliteCacheService(ILogger logger, SqliteConnection connection) : base(connection)
    {
        this.logger = logger;
    }

    protected Task<string?> GetFromCachedOrImpl(
        string cacheKey,
        TimeSpan cacheTimeToLive,
        Func<Task<string?>> valueFactory,
        Func<string?, bool> cacheInsertFilter)
    {
        return GetFromCachedOrImpl(
            cacheKey,
            cacheTimeToLive,
            valueFactory,
            cacheInsertFilter,
            x => x == null ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(x),
            Encoding.UTF8.GetString);
    }

    protected async Task<T?> GetFromCachedOrImpl<T>(
        string cacheKey,
        TimeSpan cacheTimeToLive,
        Func<Task<T>> valueFactory,
        Func<T, bool> cacheInsertFilter,
        Func<T, byte[]> toCacheSerializer,
        Func<byte[], T> fromCacheDeserializer)
    {
        var cached = await GetBytesFromCacheOrNull(cacheKey);

        if (cached != null)
        {
            logger.LogTrace("Cache hit for {CacheKey}", cacheKey);

            return fromCacheDeserializer(cached);
        }

        logger.LogTrace("Cache miss for {CacheKey}.", cacheKey);
        var resolveValue = await valueFactory();

        if (cacheInsertFilter(resolveValue))
        {
            await SetCache(cacheKey, toCacheSerializer(resolveValue), cacheTimeToLive);
        }

        return resolveValue;
    }

    private async Task<byte[]?> GetBytesFromCacheOrNull(string key)
    {
        var cacheEntry = await Connection.QuerySingleOrDefaultAsync($"""
            SELECT
                {nameof(CacheRow.Key)},
                {nameof(CacheRow.Value)}
            FROM {TableNames.Cache}
            WHERE {nameof(CacheRow.Key)} = @key
              AND {nameof(CacheRow.ExpirationTimestamp)} > @timestamp
            """,
            new Dictionary<string, (DbType, object?)>
            {
                ["key"] = (DbType.String, key),
                ["timestamp"] = (DbType.DateTime, DateTime.UtcNow)
            },
            x => new CacheRow
            {
                Key = x.GetString(0),
                Value = x.GetFieldValue<byte[]>(1)
            },
            null);
     
        return cacheEntry?.Value;
    }

    private Task SetCache(string key, byte[] value, TimeSpan timeToLive)
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
            new Dictionary<string, (DbType, object?)>
            {
                ["key"] = (DbType.String, key),
                ["value"] = (DbType.Binary, value),
                ["timestamp"] = (DbType.DateTime2, cacheTimestamp),
                ["expirationTimestamp"] = (DbType.DateTime2, cacheTimestamp + timeToLive)
            });
    }
}
