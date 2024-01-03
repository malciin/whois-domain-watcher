using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Infrastructure.HttpServer;

public class HttpServerHostedService(
    ILogger<HttpServerHostedService> logger,
    HttpServer httpServer) : IHostedService
{
    private CancellationTokenSource? cancellationTokenSource;
    private Task? httpTask;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationTokenSource = new CancellationTokenSource();
        httpTask = httpServer.StartAsync(cancellationTokenSource.Token);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationTokenSource!.Cancel();
        await httpTask!;

        logger.LogInformation($"Gracefully stopped.");
    }
}
