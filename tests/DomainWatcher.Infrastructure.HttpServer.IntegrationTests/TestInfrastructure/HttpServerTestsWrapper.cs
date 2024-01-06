using DomainWatcher.Core.Extensions;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Infrastructure.HttpServer.IntegrationTests.TestInfrastructure;

public class HttpServerTestsWrapper : IAsyncDisposable
{
    private readonly ServiceProvider serviceProvider;
    private readonly HttpServer httpServer;
    private readonly IHttpServerInfo httpServerInfo;
    private readonly CancellationTokenSource httpServerListenTaskCts;
    private readonly Task httpServerListenTask;

    public string Url => $"http://localhost:{httpServerInfo.AssignedPort}";

    public HttpServerTestsWrapper(IEnumerable<Type>? endpoints)
    {
        var serviceCollection = new ServiceCollection()
           .AddLogging(builder =>
           {
               builder.SetMinimumLevel(LogLevel.Trace);
               builder.AddConsole();
           });

        var serverBuilder = serviceCollection
            .AddInternalHttpServer(x => x.Port = 0);

        if (endpoints?.Any() == true)
        {
            var method = serverBuilder
                .GetType()
                .GetMethod(nameof(HttpServerBuilder.UseEndpoint))!;

            endpoints.ForEach(x => method.MakeGenericMethod(x).Invoke(serverBuilder, null));
        }

        serviceProvider = serviceCollection.BuildServiceProvider();
        httpServer = serviceProvider.GetRequiredService<HttpServer>();
        httpServerInfo = serviceProvider.GetRequiredService<IHttpServerInfo>();
        httpServerListenTaskCts = new CancellationTokenSource();
        httpServerListenTask = httpServer.StartAsync(httpServerListenTaskCts.Token);
    }

    public async ValueTask DisposeAsync()
    {
        httpServerListenTaskCts.Cancel();
        await httpServerListenTask;
        serviceProvider.Dispose();
    }
}
