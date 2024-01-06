using Microsoft.Data.Sqlite;

namespace DomainWatcher.Infrastructure.Sqlite.Internal.Extensions;

internal static class SqliteDataReaderExtensions
{
    public static DateTime? GetNullableDateTime(this SqliteDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal)) return null;

        return reader.GetDateTime(ordinal);
    }
}
