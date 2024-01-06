using System.Data;
using Microsoft.Data.Sqlite;

namespace DomainWatcher.Infrastructure.Sqlite.Extensions;

public static class SqliteConnectionExtensions
{
    public static async Task ExecuteAsync(
        this SqliteConnection sqliteConnection,
        string sql,
        Dictionary<string, (DbType, object?)>? args = null,
        SqliteTransaction? transaction = null)
    {
        using var command = await CreateCommand(sqliteConnection, sql, args, transaction);
        await command.ExecuteNonQueryAsync();
    }

    public static Task<T> QuerySingleOrDefaultAsync<T>(
        this SqliteConnection sqliteConnection,
        string sql,
        Func<SqliteDataReader, T> parse,
        T @default)
    {
        return sqliteConnection.QuerySingleOrDefaultAsync(sql, null, parse, @default);
    }

    public static async Task<T> QuerySingleOrDefaultAsync<T>(
        this SqliteConnection sqliteConnection,
        string sql,
        Dictionary<string, (DbType, object?)>? args,
        Func<SqliteDataReader, T> parse,
        T @default)
    {
        var value = @default;
        var receivedItems = 0;

        await foreach (var row in AsyncRead(sqliteConnection, sql, args, parse))
        {
            value = row;
            receivedItems++;

            if (receivedItems > 1)
            {
                throw new ArgumentException("Multiple results returned");
            }
        }

        return value;
    }

    public static IAsyncEnumerable<T> AsyncRead<T>(
        this SqliteConnection sqliteConnection,
        string sql,
        Func<SqliteDataReader, T> parse)
    {
        return sqliteConnection.AsyncRead(sql, null, parse);
    }

    public static async IAsyncEnumerable<T> AsyncRead<T>(
        this SqliteConnection sqliteConnection,
        string sql,
        Dictionary<string, (DbType, object?)>? args,
        Func<SqliteDataReader, T> parse)
    {
        using var command = await CreateCommand(sqliteConnection, sql, args);
        using var reader = await command.ExecuteReaderAsync();

        while (reader.HasRows)
        {
            while (await reader.ReadAsync())
            {
                yield return parse(reader);
            }

            await reader.NextResultAsync();
        }
    }

    private static async Task<SqliteCommand> CreateCommand(
        SqliteConnection sqliteConnection,
        string sql,
        Dictionary<string, (DbType, object?)>? args = null,
        SqliteTransaction? transaction = null)
    {
        var command = sqliteConnection.CreateCommand();

        try
        {
            if (!sqliteConnection.State.HasFlag(ConnectionState.Open))
            {
                await sqliteConnection.OpenAsync();
            }

            command.CommandText = sql;
            command.Transaction = transaction;

            if (args == null)
            {
                return command;
            }

            foreach (var arg in args)
            {
                var name = arg.Key;
                var (type, value) = arg.Value;

                var parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.DbType = type;
                parameter.Value = value ?? DBNull.Value;

                command.Parameters.Add(parameter);
            }
        }
        catch (Exception)
        {
            command.Dispose();

            throw;
        }

        return command;
    }
}
