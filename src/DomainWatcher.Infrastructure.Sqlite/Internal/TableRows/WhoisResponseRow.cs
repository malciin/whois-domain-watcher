using DomainWatcher.Core.Enums;

namespace DomainWatcher.Infrastructure.Sqlite.Internal.TableRows;

internal class WhoisResponseRow
{
    public int DomainId { get; set; }

    public string SourceServer { get; set; }

    public WhoisResponseStatus Status { get; set; }

    public DateTime QueryTimestamp { get; set; }

    public DateTime Registration { get; set; }

    public DateTime Expiration { get; set; }

    public string RawResponse { get; set; }
}
