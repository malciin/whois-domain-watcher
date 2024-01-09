using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Infrastructure.HttpServer;

public class HttpServer(
    HttpServerOptions options,
    HttpServerInfo httpServerInfo,
    IEnumerable<IRequestMiddleware> requestMiddlewares,
    ILogger<HttpServer> logger)
{
    private readonly IReadOnlyList<IRequestMiddleware> requestMiddlewares = requestMiddlewares.ToList();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var tcpListener = new TcpListener(IPAddress.Any, options.Port);
        tcpListener.Start();

        httpServerInfo.AssignedPort = ((IPEndPoint)tcpListener.LocalEndpoint).Port;

        logger.LogInformation("Listening on {Port}", httpServerInfo.AssignedPort);

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

        var httpRequest = parseResult.Result!;
        var stopwatch = Stopwatch.StartNew();

        var response = await ProcessRequest(httpRequest);

        stopwatch.Stop();
        logger.LogDebug("[{ResponseCode}] {Time} {Method} {Url}",
            (int)response.Code,
            $"{stopwatch.Elapsed.TotalMilliseconds:0.0}ms",
            httpRequest.Method,
            httpRequest.RelativeUrl);

        await response.WriteResponse(networkStream);
    }

    private async ValueTask<HttpResponse> ProcessRequest(HttpRequest request, int index = 0)
    {
        if (index < requestMiddlewares.Count)
        {
            return await requestMiddlewares[index].TryProcess(request, () => ProcessRequest(request, index + 1));
        }
        
        logger.LogTrace(
            "Request {Url} not handled by any middleware. Returning {Status} status.",
            request.RelativeUrl,
            HttpResponse.NotFound.Code);

        return HttpResponse.NotFound;
    }
}
