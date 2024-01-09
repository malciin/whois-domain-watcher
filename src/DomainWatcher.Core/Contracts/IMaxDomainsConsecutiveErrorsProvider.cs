namespace DomainWatcher.Core.Contracts;

public interface IMaxDomainsConsecutiveErrorsProvider
{
    int MaxDomainConsecutiveErrors { get; }
}
