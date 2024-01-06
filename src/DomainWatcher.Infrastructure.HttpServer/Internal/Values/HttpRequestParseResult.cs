using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Infrastructure.HttpServer.Internal.Values;

internal class HttpRequestParseResult
{
    public required bool Success { get; init; }

    public HttpRequest? Result { get; init; }

    public string? ErrorMessage { get; init; }
}
