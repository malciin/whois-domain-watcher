﻿using System.Data;
using DomainWatcher.Core.Repositories;
using DomainWatcher.Core.Values;
using DomainWatcher.Infrastructure.Sqlite.Abstract;
using DomainWatcher.Infrastructure.Sqlite.Extensions;
using DomainWatcher.Infrastructure.Sqlite.Internal;
using DomainWatcher.Infrastructure.Sqlite.Internal.Extensions;
using DomainWatcher.Infrastructure.Sqlite.Internal.TableRows;
using Microsoft.Data.Sqlite;

namespace DomainWatcher.Infrastructure.Sqlite.Repositories;

public class SqliteDomainsRepository(SqliteConnection connection) : SqliteService(connection), IDomainsRepository
{
    public Task Store(Domain domain)
    {
        return Connection.ExecuteAsync($"""
            INSERT OR IGNORE INTO {TableNames.Domains} ({nameof(DomainRow.Domain)}, {nameof(DomainRow.IsWatched)}) VALUES (@domain, False)
            """,
            new Dictionary<string, (DbType, object?)>{ ["domain"] = (DbType.String, domain.FullName) });
    }

    public Task Watch(Domain domain)
    {
        return Connection.ExecuteAsync($"""
            INSERT INTO {TableNames.Domains} ({nameof(DomainRow.Domain)}, {nameof(DomainRow.IsWatched)}) VALUES (@domain, True)
            ON CONFLICT({nameof(DomainRow.Domain)}) DO UPDATE SET {nameof(DomainRow.IsWatched)} = True
            """,
            new Dictionary<string, (DbType, object?)> { ["domain"] = (DbType.String, domain.FullName) });
    }

    public Task Unwatch(Domain domain)
    {
        return Connection.ExecuteAsync($"""
            UPDATE {TableNames.Domains} SET {nameof(DomainRow.IsWatched)} = False WHERE {nameof(DomainRow.Domain)} = @domain
            """,
            new Dictionary<string, (DbType, object?)> { ["domain"] = (DbType.String, domain.FullName) });
    }

    public Task<bool> IsWatched(Domain domain)
    {
        return Connection.QuerySingleOrDefaultAsync($"""
            SELECT {(nameof(DomainRow.IsWatched))} FROM {TableNames.Domains} WHERE {nameof(DomainRow.Domain)} = @domain
            """,
            new Dictionary<string, (DbType, object?)> { ["domain"] = (DbType.String, domain.FullName) },
            x => x.GetBoolean(0),
            false);
    }

    public IAsyncEnumerable<Domain> GetWatchedDomains()
    {
        return Connection.AsyncRead($"""
            SELECT {nameof(DomainRow.Domain)} FROM {TableNames.Domains} WHERE {nameof(DomainRow.IsWatched)} = True
            """,
            (SqliteDataReader x) => new Domain(x.GetString(0)));
    }
}
