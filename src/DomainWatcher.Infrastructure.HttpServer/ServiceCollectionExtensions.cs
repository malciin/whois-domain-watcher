using Microsoft.Extensions.DependencyInjection;

namespace DomainWatcher.Infrastructure.HttpServer;

public static class ServiceCollectionExtensions
{
    public static HttpServerBuilder AddInternalHttpServer(
        this IServiceCollection serviceCollection,
        Action<HttpServerOptions> mutator)
    {
        return AddInternalHttpServer(serviceCollection, (_, x) => mutator(x));
    }

    public static HttpServerBuilder AddInternalHttpServer(
        this IServiceCollection serviceCollection,
        Action<IServiceProvider, HttpServerOptions> mutator)
    {
        var options = new HttpServerOptions();

        return new HttpServerBuilder(serviceCollection, s =>
        {
            mutator(s, options);

            return options;
        });
    }
}
