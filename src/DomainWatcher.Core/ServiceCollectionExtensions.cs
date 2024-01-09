using DomainWatcher.Core.Contracts;
using DomainWatcher.Core.Services;
using DomainWatcher.Core.Settings;
using DomainWatcher.Core.Utilities;
using DomainWatcher.Core.Whois;
using DomainWatcher.Core.Whois.Contracts;
using DomainWatcher.Core.Whois.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DomainWatcher.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IWhoisClient, WhoisClient>();
        serviceCollection.AddScoped<IWhoisServerUrlResolver, WhoisServerUrlResolver>();
        serviceCollection.AddScoped<IWhoisRawResponseProvider, TcpWhoisRawResponseProvider>();

        serviceCollection.AddSingleton<IWhoisResponseParser, WhoisResponseParser>();
        serviceCollection.AddSingleton<IDomainQueryDelayProvider, DomainQueryDelayProvider>();
        serviceCollection.AddSingleton<IDomainsQueryQueue, DomainsQueryQueue>();

        serviceCollection.AddSingleton<ProcessingQueueSettings>();
        serviceCollection.AddSingleton<IMaxDomainsConsecutiveErrorsProvider>(ctx => ctx.GetRequiredService<ProcessingQueueSettings>());
        serviceCollection.AddSingleton<DomainWhoisQueryIntervalsSettings>(ctx => ctx.GetRequiredService<ProcessingQueueSettings>().WhoisQueryIntervals);

        serviceCollection.AddHostedService<DomainWatcherHostedService>();

        return serviceCollection;
    }
}
