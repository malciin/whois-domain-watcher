namespace DomainWatcher.Infrastructure.HttpServer.Contracts;

public interface IHttpServerInfo
{
    public int AssignedPort { get; }
}
