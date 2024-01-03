using Microsoft.Extensions.DependencyInjection;

namespace DomainWatcher.Infrastructure.HttpServer;

public static class ServiceCollectionExtensions
{
    public static HttpServerBuilder AddInternalHttpServer(
        this IServiceCollection serviceCollection,
        Action<HttpServerOptions> mutator)
    {
        var httpServerOptions = new HttpServerOptions();

        mutator(httpServerOptions);

        return new HttpServerBuilder(serviceCollection, _ => httpServerOptions);
    }
}
