using System.Text;
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
            .ThenByDescending(x => x.LatestResponse?.Expiration)
            .ToListAsync();

        if (domains.Count == 0)
        {
            return HttpResponse.PlainText("No domains watched.");
        }

        var stringBuilder = new StringBuilder();
        var length = domains.Select(x => x.Domain.FullName.Length).Max();

        foreach (var (domain, latestResponse) in domains)
        {
            stringBuilder.Append($"{domain.FullName.PadRight(length)} ");

            if (latestResponse == null) stringBuilder.Append("Not yet queried");
            else if (latestResponse.IsAvailable) stringBuilder.Append("Available");
            else
            {
                stringBuilder.AppendLine($"Taken for {(latestResponse.Expiration!.Value - DateTime.UtcNow).ToJiraDuration(2)} ({latestResponse.Expiration!.Value:yyyy-MM-dd HH:mm})");
                stringBuilder.Append($"{' '.ToString().PadRight(length)} Queried {(DateTime.UtcNow - latestResponse.QueryTimestamp).ToJiraDuration(2)} ago");
            }

            if (domain != domains.Last().Domain) stringBuilder.AppendLine();
        }

        return HttpResponse.PlainText(stringBuilder.ToString());
    }
}
