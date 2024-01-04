using System.Reflection;
using System.Text.RegularExpressions;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Infrastructure.HttpServer.Values;

public class EndpointsCollection
{
    internal static Dictionary<Type, Regex> UrlRegexByEndpoint = new();

    private readonly Dictionary<(HttpMethod, string), Type> endpoints;
    private readonly Dictionary<HttpMethod, List<Type>> regexPathEndpoints;

    public EndpointsCollection(IEnumerable<Type> endpointTypes)
    {
        var getEndpointParametersMethod = GetType()
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(x => x.Name.Contains(nameof(GetEndpointParameters)));

        endpoints = [];
        regexPathEndpoints = [];

        foreach (var endpointType in endpointTypes)
        {
            var (method, path) = ((HttpMethod, string))getEndpointParametersMethod
                .MakeGenericMethod(endpointType)
                .Invoke(null, null)!;

            if (path[0] == '/')
            {
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
        }
    }

    public bool TryGetFor(HttpRequest request, out Type? type)
    {
        if (endpoints.TryGetValue((request.Method, request.RelativeUrl), out type)) return true;

        type = regexPathEndpoints
            .GetValueOrDefault(request.Method)
            ?.FirstOrDefault(x => UrlRegexByEndpoint[x].IsMatch(request.RelativeUrl));

        return type != null;
    }

    private static (HttpMethod, string) GetEndpointParameters<T>() where T : IHttpEndpoint
    {
        return (T.Method, T.Path);
    }
}
