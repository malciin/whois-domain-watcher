namespace DomainWatcher.Core.Whois.Values;

public class IsDomainSupportedResult
{
    public bool IsSupported { get; init; }

    public string? Reason { get; init; }
}
