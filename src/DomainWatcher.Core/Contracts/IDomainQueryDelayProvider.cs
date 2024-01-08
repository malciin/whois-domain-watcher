using DomainWatcher.Core.Values;

namespace DomainWatcher.Core.Contracts;

public interface IDomainQueryDelayProvider
{
    TimeSpan GetDelay(Domain domain, WhoisResponse? latestResponse);
}
