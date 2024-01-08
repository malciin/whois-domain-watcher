using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Infrastructure.HttpServer.Contracts;

public interface IRequestMiddleware
{
    ValueTask<HttpResponse> TryProcess(HttpRequest request, Func<ValueTask<HttpResponse>> nextMiddleware);
}
