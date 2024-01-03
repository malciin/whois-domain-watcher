using System.Reflection;
using DomainWatcher.Infrastructure.HttpServer.Contracts;
using DomainWatcher.Infrastructure.HttpServer.Models;

namespace DomainWatcher.Infrastructure.HttpServer.Values;

public class EndpointsCollection
{
    private readonly Dictionary<(HttpMethod, string), Type> endpoints;

    public EndpointsCollection(IEnumerable<Type> endpointTypes)
    {
        var getEndpointParametersMethod = GetType()
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(x => x.Name.Contains(nameof(GetEndpointParameters)));

        endpoints = [];

        foreach (var endpointType in endpointTypes)
        {
            var (method, path) = ((HttpMethod, string))getEndpointParametersMethod
                .MakeGenericMethod(endpointType)
                .Invoke(null, null)!;

            endpoints.Add((method, path), endpointType);
        }
    }

    public bool TryGetFor(HttpRequest request, out Type? type)
    {
        return endpoints.TryGetValue((request.Method, request.Url), out type);
    }

    private static (HttpMethod, string) GetEndpointParameters<T>() where T : IHttpEndpoint
    {
        return (T.Method, T.Path);
    }
}
