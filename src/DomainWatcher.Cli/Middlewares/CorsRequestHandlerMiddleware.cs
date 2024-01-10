using DomainWatcher.Cli.Settings;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Cli.Middlewares;

public class CorsRequestHandlerMiddleware(
    CorsHeaderSettings corsHeaderSettings) : IRequestMiddleware
{
    public string methodsHeaderValue = string.Join(", ", corsHeaderSettings.Headers.Split(' '));

    public async ValueTask<HttpResponse> TryProcess(HttpRequest request, Func<ValueTask<HttpResponse>> nextMiddleware)
    {
        var response = request.Method != HttpMethod.Options
            ? await nextMiddleware()
            : HttpResponse.NoContent;

        return response
            .WithHeaderIfNotPresent("Access-Control-Allow-Methods", methodsHeaderValue)
            .WithHeaderIfNotPresent("Access-Control-Allow-Origin", corsHeaderSettings.Origin)
            .WithHeaderIfNotPresent("Access-Control-Allow-Headers", corsHeaderSettings.Headers);
    }
}
