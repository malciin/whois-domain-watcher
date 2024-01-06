namespace DomainWatcher.Infrastructure.HttpServer.Internal.Values;

internal class EndpointEntry
{
    public required HttpMethod Method { get; init; }

    public required string Path { get; init; }

    public required Type EndpointType { get; init; }
}
