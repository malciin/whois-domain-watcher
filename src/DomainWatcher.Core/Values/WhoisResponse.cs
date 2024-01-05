using DomainWatcher.Core.Enums;

namespace DomainWatcher.Core.Values;

public class WhoisResponse
{
    public long Id { get; init; }

    public required Domain Domain { get; init; }

    public required string SourceServer { get; init; }

    public required WhoisResponseStatus Status { get; init; }

    public required DateTime QueryTimestamp { get; init; }

    public required string RawResponse { get; init; }
 
    public DateTime? Registration { get; init; }

    public DateTime? Expiration { get; init; }

    public bool IsAvailable => Expiration == null && Status == WhoisResponseStatus.OK;
}
