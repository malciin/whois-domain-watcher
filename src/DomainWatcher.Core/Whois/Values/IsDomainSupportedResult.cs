namespace DomainWatcher.Core.Whois.Values;

public class IsDomainSupportedResult
{
    public bool IsSupported { get; init; }

    public string? WhoisServerUrl { get; init; }
}
