namespace DomainWatcher.Infrastructure.HttpServer.Internal.Values;

internal class ContentResponse
{
    public required string ContentType { get; init; }

    public required byte[] Body { get; init; }
}
