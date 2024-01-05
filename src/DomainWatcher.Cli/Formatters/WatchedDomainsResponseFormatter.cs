using System.Text;
using DomainWatcher.Core.Enums;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Repositories;
using DomainWatcher.Core.Values;

namespace DomainWatcher.Cli.Formatters;

public class WatchedDomainsResponseFormatter(
    IWhoisResponsesRepository whoisResponsesRepository)
{
    public async Task<string> CreateResponse(IAsyncEnumerable<Domain> watchedDomains)
    {
        var domains = await watchedDomains
            .SelectAwait(async (domain) => (Domain: domain, LatestResponse: await whoisResponsesRepository.GetLatestFor(domain)))
            .OrderByDescending(x => x.LatestResponse == null)
            .ThenByDescending(x => x.LatestResponse?.IsAvailable)
            .ThenBy(x => x.LatestResponse?.Status)
            .ThenBy(x => x.LatestResponse?.Expiration)
            .ToListAsync();

        if (domains.Count == 0)
        {
            return "No domains watched.";
        }

        var stringBuilder = new StringBuilder();
        var length = domains.Select(x => x.Domain.FullName.Length).Max();

        foreach (var (domain, latestResponse) in domains)
        {
            stringBuilder.AppendLine($"{domain.FullName.PadRight(length)} {GetDescriptiveStatus(latestResponse)}");

            if (latestResponse != null)
            {
                stringBuilder.AppendLine($"{' '.ToString().PadRight(length)} Queried {(DateTime.UtcNow - latestResponse.QueryTimestamp).ToJiraDuration(2)} ago");
            }
        }

        return stringBuilder.ToString().TrimEnd();
    }

    public static string GetDescriptiveStatus(WhoisResponse? latestResponse)
    {
        if (latestResponse == null) return "Not yet queried";
        if (latestResponse.IsAvailable) return "Available";
        if (latestResponse.Status == WhoisResponseStatus.OK) return $"Taken for {(latestResponse.Expiration!.Value - DateTime.UtcNow).ToJiraDuration(2)} ({latestResponse.Expiration!.Value:yyyy-MM-dd HH:mm})";
        if (latestResponse.Status == WhoisResponseStatus.ParserMissing) return $"Missing parser for {latestResponse.SourceServer}";
        if (latestResponse.Status == WhoisResponseStatus.TakenButTimestampsHidden) return $"Taken for HIDDEN ({latestResponse.SourceServer} hides that information)";

        throw new Exception("Unsupported case");
    }
}
