using System.Text.Json;
using DomainWatcher.Cli.Formatters;
using DomainWatcher.Cli.Json;
using DomainWatcher.Cli.Json.Responses;
using DomainWatcher.Core.Repositories;
using DomainWatcher.Infrastructure.HttpServer.Constants;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Cli.Endpoints;

public class IndexEndpoint(
    IDomainsRepository domainsRepository,
    IWhoisResponsesRepository whoisResponsesRepository,
    WatchedDomainsResponseFormatter responseFormatter) : IHttpEndpoint
{
    public static HttpMethod Method => HttpMethod.Get;

    public static string Path => "/";

    public async Task<HttpResponse> Handle(HttpRequest request)
    {
        if (request.Headers.TryGetValue("Accept", out var accept))
        {
            if (accept == MimeTypes.Json)
            {
                var data = await domainsRepository
                .GetWatchedDomains()
                .SelectAwait(async (domain) => (
                    Domain: domain,
                    LatestResponse: await whoisResponsesRepository.GetLatestFor(domain)))
                .OrderByDescending(x => x.LatestResponse == null)
                .ThenByDescending(x => x.LatestResponse?.IsAvailable)
                .ThenBy(x => x.LatestResponse?.Status)
                .ThenBy(x => x.LatestResponse?.Expiration)
                .Select(x => new WatchedDomainInfoJsonResponse
                {
                    Domain = x.Domain.FullName,
                    QueryStatus = x.LatestResponse?.Status,
                    QueryTimestamp = x.LatestResponse?.QueryTimestamp,
                    ExpirationTimestamp = x.LatestResponse?.Expiration,
                    RegistrationTimestamp = x.LatestResponse?.Registration
                })
                .ToListAsync();

                return HttpResponse.Json(JsonSerializer.Serialize(data, AppJsonSerializerContext.Default.ListWatchedDomainInfoJsonResponse));
            }
            if (accept.Contains(MimeTypes.Html))
            {
                try
                {
                    return HttpResponse.Html(File.ReadAllText("ui.html"));
                }
                catch (FileNotFoundException)
                {
                    return HttpResponse.Html("""
                        <!doctype html>
                        <html lang="en">
                          <head>
                            <meta charset="UTF-8" />
                            <link rel="icon" type="image/svg+xml" href="/vite.svg" />
                            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                            <title>domain-watcher-ui</title>
                          </head>
                          <body>
                            NO ui.html FILE FOUND
                          </body>
                        </html>
                        """);
                }
            }
        }

        return await responseFormatter.CreateResponse(domainsRepository.GetWatchedDomains());
    }
}
