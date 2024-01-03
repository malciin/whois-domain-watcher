using DomainWatcher.Core;
using DomainWatcher.Infrastructure.HttpServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

var startupBegining = DateTime.UtcNow;
var hostBuilder = Host.CreateDefaultBuilder(args);

hostBuilder
    .ConfigureLogging((_, logging) => logging.AddSerilog())
    .ConfigureServices(x => x
        .AddCore()
        .AddInternalHttpServer(x => x.Port = 8050)
        .UseEndpointsFromCurrentAssembly()
        .RegisterAsHostedService())
    .UseSerilog((_, configuration) => configuration
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContextName}] {Message:lj}{NewLine}{Exception}")
        .MinimumLevel.Override("Microsoft.Extensions.Hosting", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Warning)
        .MinimumLevel.Debug()
        .Enrich.FromLogContext()
        .Enrich.WithComputed("SourceContextName", "Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)"));

using var host = hostBuilder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogDebug("Startup took {Elapsed:0.0000}ms", (DateTime.UtcNow - startupBegining).TotalMilliseconds);
logger.LogInformation("Press CTRL+C to stop.");

await host.RunAsync();
