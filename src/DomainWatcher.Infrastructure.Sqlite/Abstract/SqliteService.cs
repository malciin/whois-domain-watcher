using Microsoft.Data.Sqlite;

namespace DomainWatcher.Infrastructure.Sqlite.Abstract;

public abstract class SqliteService
{
    public SqliteService(SqliteConnection connection)
    {
        Connection = connection;
    }

    protected SqliteConnection Connection { get; }
}
