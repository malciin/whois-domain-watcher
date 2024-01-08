using DomainWatcher.Core.Contracts;
using DomainWatcher.Core.Services;
using DomainWatcher.Core.Utilities;
using DomainWatcher.Core.Values;
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
        serviceCollection.AddSingleton(ctx =>
        {
            var section = ctx
                .GetRequiredService<IConfiguration>()
                .GetSection("DomainWhoisQueryIntervals");

            return new DomainWhoisQueryIntervals
            {
                DomainTaken = JiraLikeDurationConverter
                    .ToTimeSpan(section[nameof(DomainWhoisQueryIntervals.DomainTaken)]!),
                DomainTakenButExpirationHidden = JiraLikeDurationConverter
                    .ToTimeSpan(section[nameof(DomainWhoisQueryIntervals.DomainTakenButExpirationHidden)]!),
                DomainFree = JiraLikeDurationConverter
                    .ToTimeSpan(section[nameof(DomainWhoisQueryIntervals.DomainFree)]!),
                MissingParser = JiraLikeDurationConverter
                    .ToTimeSpan(section[nameof(DomainWhoisQueryIntervals.MissingParser)]!),
            };
        });

        serviceCollection.AddHostedService<DomainWatcherHostedService>();

        return serviceCollection;
    }
}
