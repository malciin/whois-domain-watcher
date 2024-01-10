using System.Text;
using DomainWatcher.Cli.Formatters.Values;
using DomainWatcher.Core.Contracts;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Repositories;
using DomainWatcher.Core.Settings;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Cli.Endpoints;

public class QueueStatusEndpoint(
    IDomainsQueryQueue queue,
    IWhoisResponsesRepository whoisResponsesRepository,
    DomainWhoisQueryIntervalsSettings domainWhoisQueryIntervals) : IHttpEndpoint
{
    public static HttpMethod Method => HttpMethod.Get;

    public static string Path => "/queue";

    public async Task<HttpResponse> Handle(HttpRequest request)
    {
        var utcNow = DateTime.UtcNow;
        var entries = queue.GetEntries();

        if (queue.Count == 0) return HttpResponse.PlainText("Nothing in queue.");

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{queue.Count} domains enqueued.");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("Query intervals:");

        var queryIntervalsSection = new TabularStringBuilder(new TabularColumnSpec { PaddingLeft = 4 });
        queryIntervalsSection[0, 0] = "Domain taken";
        queryIntervalsSection[0, 1] = domainWhoisQueryIntervals.DomainTaken.ToJiraDuration();
        queryIntervalsSection[1, 0] = "Domain taken but expiration hidden";
        queryIntervalsSection[1, 1] = domainWhoisQueryIntervals.DomainTakenButExpirationHidden.ToJiraDuration();
        queryIntervalsSection[2, 0] = "Domain free";
        queryIntervalsSection[2, 1] = domainWhoisQueryIntervals.DomainFree.ToJiraDuration();
        queryIntervalsSection[3, 0] = "Missing parser";
        queryIntervalsSection[3, 1] = domainWhoisQueryIntervals.MissingParser.ToJiraDuration();
        queryIntervalsSection[4, 0] = "Base errror retry delay";
        queryIntervalsSection[4, 1] = domainWhoisQueryIntervals.BaseErrorRetryDelay.ToJiraDuration();

        stringBuilder.AppendLine(queryIntervalsSection.ToString());
        stringBuilder.AppendLine();

        var queueStatusSection = new TabularStringBuilder(
            new TabularColumnSpec { Padding = 1, Header = "Domain" },
            new TabularColumnSpec { Padding = 1, Header = "Next query" },
            new TabularColumnSpec { Padding = 1, Header = "Last query" });

        foreach (var (domain, fireAt) in entries)
        {
            var latestResponse = await whoisResponsesRepository.GetLatestFor(domain);

            queueStatusSection[0] = domain.FullName;
            queueStatusSection[1] = (fireAt - utcNow).ToJiraDuration(2);
            queueStatusSection[2] = (utcNow - latestResponse!.QueryTimestamp).ToJiraDuration(2);

            queueStatusSection.CurrentRow++;
        }

        stringBuilder.Append(queueStatusSection.ToString());

        return HttpResponse.PlainText(stringBuilder.ToString());
    }
}
