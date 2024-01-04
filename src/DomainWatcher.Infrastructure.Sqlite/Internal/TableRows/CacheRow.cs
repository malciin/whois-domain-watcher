namespace DomainWatcher.Infrastructure.Sqlite.Internal.TableRows;

internal class CacheRow
{
    public string Key { get; set; }
    
    public byte[] Value { get; set; }
    
    public DateTime Timestamp { get; set; }

    public DateTime ExpirationTimestamp { get; set; }
}
