namespace DomainWatcher.Infrastructure.Sqlite.Internal.TableRows;

internal class DomainRow
{
    public int Id { get; set; }

    public string Domain { get; set; }

    public bool IsWatched { get; set; }
}
