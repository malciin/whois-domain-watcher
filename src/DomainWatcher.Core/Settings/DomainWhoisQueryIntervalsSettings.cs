namespace DomainWatcher.Core.Settings;

public class DomainWhoisQueryIntervalsSettings
{
    public required TimeSpan DomainTaken { get; init; }

    public required TimeSpan DomainTakenButExpirationHidden { get; init; }

    public required TimeSpan DomainFree { get; init; }

    public required TimeSpan MissingParser { get; init; }

    public required TimeSpan BaseErrorRetryDelay { get; init; }
}
