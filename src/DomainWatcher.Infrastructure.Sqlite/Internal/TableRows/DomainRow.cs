namespace DomainWatcher.Infrastructure.Sqlite.Internal.TableRows;

internal class DomainRow
{
    public long Id { get; set; }

    public string Domain { get; set; }

    public bool IsWatched { get; set; }
}
