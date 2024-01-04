using DomainWatcher.Core.Enums;

namespace DomainWatcher.Core.Whois.Values;

public class WhoisServerResponseParsed
{
    public WhoisResponseStatus Status { get; init; } = WhoisResponseStatus.OK;

    public required DateTime? Registration { get; init; }

    public required DateTime? Expiration { get; init; }
}
