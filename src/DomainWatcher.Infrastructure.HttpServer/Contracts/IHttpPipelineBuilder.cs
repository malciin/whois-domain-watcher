using System.Diagnostics.CodeAnalysis;
using DomainWatcher.Infrastructure.HttpServer.Internal.Middlewares;

namespace DomainWatcher.Infrastructure.HttpServer.Contracts;

public interface IHttpPipelineBuilder
{
    IHttpPipelineBuilder Use<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
        where T : class, IRequestMiddleware;
}

public static class IHttpPipelineBuilderExtensions
{
    public static IHttpPipelineBuilder UseEndpoints(this IHttpPipelineBuilder pipelineBuilder)
    {
        return pipelineBuilder.Use<EndpointDispatcherMiddleware>();
    }
}
