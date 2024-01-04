using DomainWatcher.Core.Contracts;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Repositories;
using DomainWatcher.Core.Values;
using DomainWatcher.Core.Whois;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Core;

internal class DomainWatcherHostedService : IHostedService
{
    private readonly ILogger<DomainWatcherHostedService> logger;
    private readonly IDomainsQueryQueue domainsQueryQueue;
    private readonly IServiceScopeFactory serviceScopeFactory;
    
    private CancellationTokenSource? loopTaskCts;
    private Task? loopTask;

    public DomainWatcherHostedService(
        ILogger<DomainWatcherHostedService> logger,
        IDomainsQueryQueue domainsQueryQueue,
        IServiceScopeFactory serviceScopeFactory)
    {
        this.logger = logger;
        this.domainsQueryQueue = domainsQueryQueue;
        this.serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        loopTaskCts = new CancellationTokenSource();
        loopTask = Task.Run(async () =>
        {
            try
            {
                await Loop(loopTaskCts.Token);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Domain watcher loop failure.");
            }
        }, CancellationToken.None);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        loopTaskCts!.Cancel();

        await loopTask!;

        logger.LogInformation("Service stopped");
    }

    private async Task Loop(CancellationToken token)
    {
        await FillQueue();

        while (!token.IsCancellationRequested)
        {
            Domain? domain;
            try
            {
                domain = await domainsQueryQueue.Dequeue(token);
            }
            catch (TaskCanceledException)
            {
                continue;
            }

            using var scope = serviceScopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            if (!await serviceProvider.GetRequiredService<IDomainsRepository>().IsWatched(domain))
            {
                logger.LogInformation("Skipping querying {DomainUrl} because it was marked as not watched externally", domain.FullName);

                continue;
            }

            WhoisResponse whoisResponse;

            try
            {
                whoisResponse = await serviceProvider.GetRequiredService<IWhoisClient>().QueryAsync(domain);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failure to get whois response for {Domain}", domain.FullName);

                domainsQueryQueue.EnqueueAfterError(domain);
                continue;
            }

            await serviceProvider.GetRequiredService<IWhoisResponsesRepository>().Add(whoisResponse);

            domainsQueryQueue.EnqueueNext(domain, whoisResponse);

            logger.LogInformation("Domain {Domain} succesfully queried.", domain.FullName);
            LogQueuePeek();
        }
    }

    private async Task FillQueue()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var domainsRepository = scope.ServiceProvider.GetRequiredService<IDomainsRepository>();
        var whoisResponsesRepository = scope.ServiceProvider.GetRequiredService<IWhoisResponsesRepository>();

        await foreach (var domain in scope.ServiceProvider.GetRequiredService<IDomainsRepository>().GetWatchedDomains())
        {
            var latestResponse = await whoisResponsesRepository.GetLatestFor(domain);

            domainsQueryQueue.EnqueueNext(domain, latestResponse);
        }
        
        logger.LogInformation(
            "Filled {QueueName} with {QueuedCount} items.",
            "DomainsQueryQueue",
            domainsQueryQueue.Count);

        LogQueuePeek();
    }

    private void LogQueuePeek()
    {
        if (!domainsQueryQueue.TryPeek(out var domain, out var fireAt)) return;

        logger.LogDebug(
            "Next to queue will be {Domain} after {Delay}.",
            domain!.FullName,
            (fireAt - DateTime.UtcNow).ToJiraDuration(2));
    }
}
