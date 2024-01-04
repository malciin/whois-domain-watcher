using DomainWatcher.Infrastructure.HttpServer.Values;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;

namespace DomainWatcher.Infrastructure.HttpServer.Models;

public partial class HttpRequest
{
    public required HttpMethod Method { get; init; }

    public required string RelativeUrl { get; init; }

    public required IReadOnlyDictionary<string, string> Headers { get; init; }

    public required string Body { get; init; }

    public string? UserAgent => Headers.TryGetValue("User-Agent", out var userAgent)
        ? userAgent
        : null;

    [GeneratedRegex(@"(?<method>GET|POST|PATCH|DELETE|OPTIONS) (?<path>[/\-a-zA-Z0-9\.\?\&]+) HTTP/1.1")]
    private static partial Regex GetHttp11HeaderGeneratedRegex();

    [GeneratedRegex("(?<key>.+)?: (?<value>.*)")]
    private static partial Regex GetHttpRequestHeadersGeneratedRegex();

    private static readonly Regex http11HeaderRegex = GetHttp11HeaderGeneratedRegex();
    private static readonly Regex httpRequestHeadersRegex = GetHttpRequestHeadersGeneratedRegex();

    public static async Task<HttpRequestParseResult> TryParseAsync(
        NetworkStream networkStream,
        CancellationToken cancellationToken)
    {
        using var streamReader = new StreamReader(networkStream, leaveOpen: true);
        var requestLine = await streamReader.ReadLineAsync(cancellationToken);

        if (string.IsNullOrEmpty(requestLine))
        {
            return new HttpRequestParseResult
            {
                Success = false,
                ErrorMessage = $"No HTTP/1.1 header. Received empty line."
            };
        }

        var match = http11HeaderRegex.Match(requestLine);

        if (!match.Success)
        {
            return new HttpRequestParseResult
            {
                Success = false,
                ErrorMessage = $"Cannot parse HTTP/1.1 header - {nameof(http11HeaderRegex)} results in no match. Received: {requestLine}"
            };
        }

        var headers = await ParseHeaders(streamReader, cancellationToken);
        var body = await GetBody(streamReader, headers, cancellationToken);

        var request = new HttpRequest
        {
            Method = new HttpMethod(match.Groups["method"].Value),
            RelativeUrl = match.Groups["path"].Value,
            Headers = headers,
            Body = body
        };

        return new HttpRequestParseResult { Success = true, Result = request };
    }

    private static async Task<IReadOnlyDictionary<string, string>> ParseHeaders(
        StreamReader streamReader,
        CancellationToken cancellationToken)
    {
        var dictionary = new Dictionary<string, string>();

        while (true)
        {
            var requestLine = await streamReader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrEmpty(requestLine)) break;

            var match = httpRequestHeadersRegex.Match(requestLine);
            if (!match.Success) break;

            dictionary.Add(match.Groups["key"].Value, match.Groups["value"].Value);
        }

        return dictionary;
    }

    private static async ValueTask<string> GetBody(
        StreamReader streamReader,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken cancellationToken)
    {
        var stringBuilder = new StringBuilder();

        if (!headers.TryGetValue("Content-Length", out var contentLengthRaw))
        {
            return string.Empty;
        }

        var contentLength = int.Parse(contentLengthRaw);
        var buffer = new char[1024];
        var totalReadedBytes = 0;

        while (totalReadedBytes < contentLength)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var readedBytes = await streamReader.ReadAsync(buffer, 0, buffer.Length);
            stringBuilder.Append(buffer, 0, readedBytes);

            totalReadedBytes += readedBytes;
        }

        return stringBuilder.ToString();
    }
}
