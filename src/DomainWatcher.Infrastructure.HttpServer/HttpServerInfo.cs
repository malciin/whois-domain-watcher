using DomainWatcher.Infrastructure.HttpServer.Contracts;

namespace DomainWatcher.Infrastructure.HttpServer;

public class HttpServerInfo : IHttpServerInfo
{
    public int AssignedPort { get; internal set; }
}
