using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Infrastructure.HttpServer.Contracts;

public interface IHttpEndpoint
{
    static abstract HttpMethod Method { get; }

    static abstract string Path { get; }

    Task<HttpResponse> Handle(HttpRequest request);
}
