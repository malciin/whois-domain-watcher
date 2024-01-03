using System.Reflection;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Middlewares;
using DomainWatcher.Infrastructure.HttpServer.Values;
using Microsoft.Extensions.DependencyInjection;

namespace DomainWatcher.Infrastructure.HttpServer;

public class HttpServerBuilder
{
    private readonly IServiceCollection serviceCollection;

    private bool endpointsFeatureEnabled = false;

    public HttpServerBuilder(
        IServiceCollection serviceCollection,
        Func<IServiceProvider, HttpServerOptions> httpServerOptionsFunc)
    {
        this.serviceCollection = serviceCollection;

        serviceCollection.AddSingleton(httpServerOptionsFunc);
        serviceCollection.AddSingleton<HttpServer>();
    }

    public HttpServerBuilder UseEndpointsFromCurrentAssembly()
    {
        return UseEnpointsFromAssembly(Assembly.GetCallingAssembly());
    }

    public HttpServerBuilder UseEnpointsFromAssembly(Assembly assembly)
    {
        return UseEndpoints(assembly.GetInstantiableTypesAssignableTo(typeof(IHttpEndpoint)).ToArray());
    }

    public HttpServerBuilder UseEndpoints(params Type[] endpointTypes)
    {
        endpointTypes.ForEach(x => serviceCollection.AddScoped(x));

        EnableEndpointsFeature();

        return this;
    }

    public HttpServerBuilder UseMiddleware<T>()
        where T : class, IRequestMiddleware => UseMiddleware(typeof(T));

    public HttpServerBuilder UseMiddleware(Type middlewareType)
    {
        serviceCollection.AddSingleton(typeof(IRequestMiddleware), middlewareType);

        return this;
    }

    public HttpServerBuilder RegisterAsHostedService()
    {
        serviceCollection.AddHostedService<HttpServerHostedService>();

        return this;
    }

    private void EnableEndpointsFeature()
    {
        if (endpointsFeatureEnabled)
        {
            return;
        }

        serviceCollection.AddSingleton(_ => new EndpointsCollection(serviceCollection
            .Where(x => x.ImplementationType != null && x.ImplementationType.IsAssignableTo(typeof(IHttpEndpoint)))
            .Select(x => x.ImplementationType!)));

        UseMiddleware<EndpointDispatcherMiddleware>();

        endpointsFeatureEnabled = true;
    }
}
