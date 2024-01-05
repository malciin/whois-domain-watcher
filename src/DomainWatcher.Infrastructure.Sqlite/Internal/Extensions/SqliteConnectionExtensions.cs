using System.Data.Common;
using Dapper;
using Microsoft.Data.Sqlite;

namespace DomainWatcher.Infrastructure.Sqlite.Internal.Extensions;

internal static class SqliteConnectionExtensions
{
    internal static async IAsyncEnumerable<T> AsyncRead<T>(
        this SqliteConnection sqliteConnection,
        string query,
        Func<DbDataReader, T> mapFunc)
    {
        using var reader = await sqliteConnection.ExecuteReaderAsync(query);

        while (await reader.ReadAsync())
        {
            yield return mapFunc(reader);
        }
    }

    internal static async IAsyncEnumerable<T> AsyncRead<T>(
        this SqliteConnection sqliteConnection,
        string query,
        object param,
        Func<DbDataReader, T> mapFunc)
    {
        using var reader = await sqliteConnection.ExecuteReaderAsync(query, param: param);

        while (await reader.ReadAsync())
        {
            yield return mapFunc(reader);
        }
    }
}
