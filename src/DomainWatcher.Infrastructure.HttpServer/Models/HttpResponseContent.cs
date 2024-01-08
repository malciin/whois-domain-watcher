namespace DomainWatcher.Infrastructure.HttpServer.Models;

public class HttpResponseContent
{
    public required string ContentType { get; init; }

    public required byte[] Body { get; init; }
}
