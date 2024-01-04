using Microsoft.Data.Sqlite;

namespace DomainWatcher.Infrastructure.Sqlite.Abstract;

public class SqliteService
{
    public SqliteService(SqliteConnection connection)
    {
        Connection = connection;
    }

    protected SqliteConnection Connection { get; }
}
