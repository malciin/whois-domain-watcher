using System.Text.RegularExpressions;
using DomainWatcher.Infrastructure.HttpServer.Internal.Values;
using DomainWatcher.Infrastructure.HttpServer.Models;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Infrastructure.HttpServer.Internal.Services;

internal class EndpointsResolver
{
    internal static Dictionary<Type, Regex> UrlRegexByEndpoint = [];

    private readonly Dictionary<(HttpMethod, string), Type> endpoints;
    private readonly Dictionary<HttpMethod, List<Type>> regexPathEndpoints;

    internal EndpointsResolver(
        IEnumerable<EndpointEntry> endpointEntries,
        ILogger<EndpointsResolver> logger)
    {
        endpoints = [];
        regexPathEndpoints = [];

        foreach (var endpointEntry in endpointEntries)
        {
            var path = endpointEntry.Path;
            var method = endpointEntry.Method;
            var endpointType = endpointEntry.EndpointType;

            if (path[0] == '/')
            {
                logger.LogDebug("{Method} {Path} mapped to {Type}", method, path, endpointType.Name);
                endpoints.Add((method, path), endpointType);

                continue;
            }

            if (!path.StartsWith("^/"))
            {
                throw new InvalidOperationException($"Path '{path}' is invalid");
            }

            UrlRegexByEndpoint[endpointType] = new Regex(path[1..]);

            if (regexPathEndpoints.TryGetValue(method, out var regexEndpointsList))
            {
                regexEndpointsList.Add(endpointType);
            }
            else
            {
                regexPathEndpoints[method] = [endpointType];
            }

            logger.LogDebug("{Method} {Path} mapped to {Type}", method, path, endpointType.Name);
        }
    }

    public bool TryResolve(HttpRequest request, out Type? type)
    {
        if (endpoints.TryGetValue((request.Method, request.RelativeUrl), out type)) return true;

        type = regexPathEndpoints
            .GetValueOrDefault(request.Method)
            ?.FirstOrDefault(x => UrlRegexByEndpoint[x].IsMatch(request.RelativeUrl));

        return type != null;
    }
}
