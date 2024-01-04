using System.Text;
using DomainWatcher.Cli.Utils;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Repositories;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Cli.Endpoints;

public class IndexEndpoint(
    IDomainsRepository domainsRepository,
    IWhoisResponsesRepository whoisResponsesRepository) : IHttpEndpoint
{
    public static HttpMethod Method => HttpMethod.Get;

    public static string Path => "/";

    public async Task<HttpResponse> Handle(HttpRequest request)
    {
        var domains = await domainsRepository
            .GetWatchedDomains()
            .SelectAwait(async (domain) => (Domain: domain, LatestResponse: await whoisResponsesRepository.GetLatestFor(domain)))
            .OrderByDescending(x => x.LatestResponse == null)
            .ThenByDescending(x => x.LatestResponse?.IsAvailable)
            .ThenBy(x => x.LatestResponse?.Status)
            .ThenBy(x => x.LatestResponse?.Expiration)
            .ToListAsync();

        if (domains.Count == 0)
        {
            return HttpResponse.PlainText("No domains watched.");
        }

        var stringBuilder = new StringBuilder();
        var length = domains.Select(x => x.Domain.FullName.Length).Max();

        foreach (var (domain, latestResponse) in domains)
        {
            stringBuilder.AppendLine($"{domain.FullName.PadRight(length)} {WhoisResponseDescriptiveStatus.Get(latestResponse)}");

            if (latestResponse != null)
            {
                stringBuilder.AppendLine($"{' '.ToString().PadRight(length)} Queried {(DateTime.UtcNow - latestResponse.QueryTimestamp).ToJiraDuration(2)} ago");
            }
        }

        return HttpResponse.PlainText(stringBuilder.ToString().TrimEnd());
    }
}
