using System.Net;
using System.Net.Sockets;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Infrastructure.HttpServer;

public class HttpServer(
    HttpServerOptions options,
    IEnumerable<IRequestMiddleware> requestMiddlewares,
    ILogger<HttpServer> logger)
{
    public int AssignedPort { get; private set; }


    private readonly IReadOnlyCollection<IRequestMiddleware> requestMiddlewares = requestMiddlewares.ToList();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var tcpListener = new TcpListener(IPAddress.Any, options.Port);
        tcpListener.Start();

        AssignedPort = ((IPEndPoint)tcpListener.LocalEndpoint).Port;

        logger.LogInformation("Listening on {Port}", AssignedPort);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = await tcpListener.AcceptTcpClientAsync(cancellationToken);

                _ = TryHandleClient(client, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogDebug("Cancelling http server");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Unhandled exception durning http server loop. Stopping server.");
                break;
            }
        }

        tcpListener.Stop();

        logger.LogDebug("Http server gracefully stopped.");
    }

    private async Task TryHandleClient(TcpClient client, CancellationToken cancellationToken)
    {
        using (client)
        {
            try
            {
                await HandleClient(client, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogTrace("HTTP Cancelled connection with {RemoteIpAddress} because server is stopping", client.Client.RemoteEndPoint!.ToString());
            }
            catch (IOException ex)
            {
                logger.LogTrace(ex, "HTTP IO exception durning write to {RemoteIpAddress}", client.Client.RemoteEndPoint!.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "HTTP Error durning processing request");
            }
        }
    }

    private async Task HandleClient(TcpClient client, CancellationToken cancellationToken)
    {
        using var networkStream = client.GetStream();
        var parseResult = await HttpRequest.TryParseAsync(networkStream, cancellationToken);

        if (!parseResult.Success)
        {
            logger.LogDebug(
                "HTTP {RemoteIpAddress} Failure to parse request: {Reason}",
                client.Client.RemoteEndPoint!.ToString(),
                parseResult.ErrorMessage);

            return;
        }

        var response = await GetResponse(parseResult.Result!);

        await response.WriteResponse(networkStream);
    }

    private async Task<HttpResponse> GetResponse(HttpRequest request)
    {
        foreach (var middleware in requestMiddlewares)
        {
            var response = await middleware.TryProcess(request);

            if (response != null)
            {
                return response;
            }
        }

        logger.LogTrace("Request {Url} not handled by any middleware. Returning {Status} status.", request.RelativeUrl, HttpResponse.NotFound.Code);

        return HttpResponse.NotFound;
    }
}
