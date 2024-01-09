using System.Text;
using DomainWatcher.Infrastructure.HttpServer.Constants;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Cli.Middlewares;

/// <summary>
/// Convenience middleware to add newline to every plain text responses for curl clients.
/// Without it to prevent response to same line user would need to add '-w "\n"' to every request:
/// https://stackoverflow.com/questions/12849584/automatically-add-newline-at-end-of-curl-response-body
/// </summary>
public class CurlNewLineAdderForPlainTextMiddleware : IRequestMiddleware
{
    public async ValueTask<HttpResponse> TryProcess(HttpRequest request, Func<ValueTask<HttpResponse>> nextMiddleware)
    {
        var response = await nextMiddleware();

        if (response.Content == null
            || response.Content.ContentType != MimeTypes.PlainText
            || request.UserAgent == null
            || !request.UserAgent.Contains("curl", StringComparison.InvariantCultureIgnoreCase))
        {
            return response;
        }

        return new HttpResponse
        {
            Code = response.Code,
            Headers = response.Headers,
            Content = new HttpResponseContent
            {
                ContentType = response.Content.ContentType,
                Body = [
                    ..response.Content.Body,
                    ..Encoding.UTF8.GetBytes(HttpServerConstants.NewLineString)
                ]
            }
        };
    }
}
