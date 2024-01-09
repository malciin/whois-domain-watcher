using DomainWatcher.Cli.Formatters.Values;
using DomainWatcher.Core.Enums;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Repositories;
using DomainWatcher.Core.Values;
using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Cli.Formatters;

public class WatchedDomainsResponseFormatter(
    IWhoisResponsesRepository whoisResponsesRepository)
{
    public async Task<HttpResponse> CreateResponse(IAsyncEnumerable<Domain> watchedDomains)
    {
        var domains = await watchedDomains
            .SelectAwait(async (domain) => (
                Domain: domain,
                LatestResponse: await whoisResponsesRepository.GetLatestFor(domain)))
            .OrderByDescending(x => x.LatestResponse == null)
            .ThenByDescending(x => x.LatestResponse?.IsAvailable)
            .ThenBy(x => x.LatestResponse?.Status)
            .ThenBy(x => x.LatestResponse?.Expiration)
            .ToListAsync();

        if (domains.Count == 0)
        {
            return HttpResponse.PlainText("No domains watched.");
        }

        var table = new TabularStringBuilder(
            new TabularColumnSpec { Padding = 1, Header = "Domain" },
            new TabularColumnSpec { Padding = 1, Header = "Status" },
            new TabularColumnSpec { Padding = 1, Header = "Expiration" },
            new TabularColumnSpec { Padding = 1, Header = "Queried" });

        foreach (var (domain, latestResponse) in domains)
        {
            table[0] = domain.FullName;
            table[1] = GetStatus(latestResponse);
            table[2] = GetExpirationString(latestResponse);
            table[3] = (DateTime.UtcNow - latestResponse!.QueryTimestamp).ToJiraDuration(2) + " ago";

            table.CurrentRow++;
        }

        return HttpResponse.PlainText(table.ToString());
    }

    public static string GetDescriptiveStatus(WhoisResponse? latestResponse)
    {
        return latestResponse switch
        {
            null => "Not yet queried",
            { IsAvailable: true } => "Available",
            { Status: WhoisResponseStatus.OK } =>
                $"Taken for {(latestResponse.Expiration!.Value - DateTime.UtcNow).ToJiraDuration(2)} " +
                $"({latestResponse.Expiration!.Value:yyyy-MM-dd HH:mm})",
            { Status: WhoisResponseStatus.ParserMissing } => $"Missing parser for {latestResponse.SourceServer}",
            { Status: WhoisResponseStatus.TakenButTimestampsHidden } => $"Taken for HIDDEN ({latestResponse.SourceServer} hides that information)",
            _ => throw new Exception("Unsupported case")
        };
    }

    private static string GetStatus(WhoisResponse? latestResponse)
    {
        return latestResponse switch
        {
            null => "Not yet queried",
            { IsAvailable: true } => "Available",
            { Status: WhoisResponseStatus.ParserMissing } => "No parser",
            _ => "Taken"
        };
    }

    private static string GetExpirationString(WhoisResponse? latestResponse)
    {
        return latestResponse switch
        {
            null => string.Empty,
            { IsAvailable: true } => string.Empty,
            { Status: WhoisResponseStatus.TakenButTimestampsHidden } => "HIDDEN",
            { Status: WhoisResponseStatus.ParserMissing } => "NO PARSER",
            _ => (latestResponse!.Expiration!.Value - DateTime.UtcNow).ToJiraDuration(2)
        };
    }
}
