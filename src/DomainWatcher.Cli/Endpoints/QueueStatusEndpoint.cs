using System.Text;
using DomainWatcher.Core.Contracts;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Cli.Endpoints;

internal class QueueStatusEndpoint(IDomainsQueryQueue queue) : IHttpEndpoint
{
    public static HttpMethod Method => HttpMethod.Get;

    public static string Path => "/queue";

    public Task<HttpResponse> Handle(HttpRequest request)
    {
        var utcNow = DateTime.UtcNow;
        var entries = queue.GetEntries();

        if (entries.Count == 0)
        {
            return HttpResponse.PlainText("Nothing in queue.");
        }

        var maxDomainNameLength = entries.Select(x => x.Domain.FullName.Length).Max();
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{entries.Count} domains enqueued.");
        stringBuilder.AppendLine();

        foreach (var (domain, fireAt) in entries)
        {
            stringBuilder.Append($"{domain.FullName.PadRight(maxDomainNameLength)} will be queried after {(fireAt - utcNow).ToJiraDuration(2)}.");

            if (domain != entries.Last().Domain) stringBuilder.AppendLine();
        }

        return HttpResponse.PlainText(stringBuilder.ToString());
    }
}
