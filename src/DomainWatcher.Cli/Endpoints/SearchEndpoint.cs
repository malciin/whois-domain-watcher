using System.Text.RegularExpressions;
using DomainWatcher.Cli.Formatters;
using DomainWatcher.Core.Repositories;
using DomainWatcher.Core.Values;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Cli.Endpoints;

public class SearchEndpoint(
    ILogger<SearchEndpoint> logger,
    IDomainsRepository domainsRepository,
    WatchedDomainsResponseFormatter responseFormatter) : IHttpEndpoint
{
    public static HttpMethod Method => HttpMethod.Get;

    public static string Path => @"^/s/(?<Filter>[\*\.\w\+]+)$";

    public async Task<HttpResponse> Handle(HttpRequest request)
    {
        var filter = this.ExtractRegexGroup(request, "Filter");
        IAsyncEnumerable<Domain> domains;

        if (!filter.Contains('*') && !filter.Contains('+'))
        {
            domains = domainsRepository.GetWatchedDomains().Where(x => x.FullName.Contains(filter));
        }
        else
        {
            var filterRegexString = filter.Replace(".", @"\.").Replace("+", ".").Replace("*", ".+");
            var filterRegex = new Regex($"^{filterRegexString}$");

            logger.LogDebug("Searching regex: {Regex}", filterRegex.ToString());

            domains = domainsRepository.GetWatchedDomains().Where(x => filterRegex.IsMatch(x.FullName));
        }

        if (!await domains.AnyAsync())
        {
            return HttpResponse.PlainText("No watched domain found");
        }

        return await responseFormatter.CreateResponse(domains);
    }
}
