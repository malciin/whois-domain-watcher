using DomainWatcher.Infrastructure.HttpServer.Models;
using DomainWatcher.Infrastructure.HttpServer.Values;

namespace DomainWatcher.Infrastructure.HttpServer.Contracts;

public interface IHttpEndpoint
{
    static abstract HttpMethod Method { get; }

    /// <summary>
    /// If Path begins with ^ its treated as Regex path.
    /// </summary>
    static abstract string Path { get; }

    Task<HttpResponse> Handle(HttpRequest request);
}

public static class IHttpEndpointExtensions
{
    public static string ExtractRegexGroup<T>(this T endpoint, HttpRequest request, string groupName)
        where T : IHttpEndpoint
    {
        return ExtractRegexGroup(endpoint, request.RelativeUrl, groupName);
    }

    public static string ExtractRegexGroup<T>(this T endpoint, string url, string groupName)
        where T : IHttpEndpoint
    {
        if (EndpointsCollection.UrlRegexByEndpoint.TryGetValue(typeof(T), out var regex))
        {
            return regex.Match(url).Groups[groupName].Value!;
        }

        throw new InvalidOperationException(
            $"{nameof(ExtractRegexGroup)} could be only used for RegexEndpoints " +
            $"(the ones whose path begins with '^/') " +
            $"but was used on {typeof(T)}");
    } 
}
