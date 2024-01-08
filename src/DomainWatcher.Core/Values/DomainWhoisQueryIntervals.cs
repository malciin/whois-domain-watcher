namespace DomainWatcher.Core.Values;

public class DomainWhoisQueryIntervals
{
    public required TimeSpan DomainTaken { get; init; }

    public required TimeSpan DomainTakenButExpirationHidden { get; init; }

    public required TimeSpan DomainFree { get; init; }

    public required TimeSpan MissingParser { get; init; }
}
