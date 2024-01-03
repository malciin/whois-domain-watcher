namespace DomainWatcher.Infrastructure.HttpServer.Values;

public class ContentResponse
{
    public required string ContentType { get; init; }

    public required byte[] Body { get; init; }
}
