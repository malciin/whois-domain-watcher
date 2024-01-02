namespace DomainWatcher.Core.Values;

public class WhoisResponse
{
    public required Domain Domain { get; init; }

    public DateTime QueryTimestamp { get; init; }

    public DateTime? Registration { get; init; }

    public DateTime? Expiration { get; init; }

    public bool IsAvailable => Expiration == null;

    public string RawResponse { get; init; } = string.Empty;
}
