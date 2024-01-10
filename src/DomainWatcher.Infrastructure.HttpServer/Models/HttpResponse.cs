using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using DomainWatcher.Infrastructure.HttpServer.Constants;

namespace DomainWatcher.Infrastructure.HttpServer.Models;

public sealed class HttpResponse
{
    public required HttpStatusCode Code { get; init; }

    public HttpResponseContent? Content { get; init; }

    public IDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

    public HttpResponse WithHeader(string key, string value)
    {
        Headers.Add(key, value);

        return this;
    }

    public HttpResponse WithHeaderIfNotPresent(string key, string value)
    {
        Headers.TryAdd(key, value);

        return this;
    }

    public async Task WriteResponse(NetworkStream networkStream)
    {
        using var streamWriter = new StreamWriter(networkStream, leaveOpen: true);
        streamWriter.NewLine = HttpServerConstants.NewLineString;
        await streamWriter.WriteLineAsync($"""
            HTTP/1.1 {(int)Code} {StatusCodeToDescriptiveText[Code]}
            Date: {DateTime.UtcNow}
            Connection: Closed
            """);

        foreach (var header in Headers)
        {
            await streamWriter.WriteLineAsync($"{header.Key}: {header.Value}");
        }

        if (Content == null)
        {
            await streamWriter.WriteLineAsync();
            await streamWriter.WriteLineAsync();
            return;
        }

        await streamWriter.WriteLineAsync($"""
            Content-Length: {Content.Body.Length}
            Content-Type: {Content.ContentType}
            """);

        await streamWriter.WriteLineAsync();
        await streamWriter.FlushAsync();
        await networkStream.WriteAsync(Content.Body);
    }

    public static implicit operator Task<HttpResponse>(HttpResponse value)
    {
        return Task.FromResult(value);
    }

    public static implicit operator ValueTask<HttpResponse>(HttpResponse value)
    {
        return ValueTask.FromResult(value);
    }

    #region CommonResponseFactories
    public static HttpResponse Ok => new() { Code = HttpStatusCode.OK };

    public static HttpResponse NoContent => new() { Code = HttpStatusCode.NoContent };

    public static HttpResponse BadRequest => new() { Code = HttpStatusCode.BadRequest };

    public static HttpResponse NotFound => new() { Code = HttpStatusCode.NotFound };

    public static HttpResponse InternalServerError => new() { Code = HttpStatusCode.InternalServerError };

    public static HttpResponse PlainText(string text)
    {
        return new HttpResponse
        {
            Code = HttpStatusCode.OK,
            Content = new HttpResponseContent
            {
                ContentType = MimeTypes.PlainText,
                Body = ToUtf8Bytes(text),
            }
        };
    }

    public static HttpResponse Html(string html)
    {
        return new HttpResponse
        {
            Code = HttpStatusCode.OK,
            Content = new HttpResponseContent
            {
                ContentType = MimeTypes.Html,
                Body = ToUtf8Bytes(html),
            }
        };
    }

    public static HttpResponse Json(string json)
    {
        return new HttpResponse
        {
            Code = HttpStatusCode.OK,
            Content = new HttpResponseContent
            {
                ContentType = MimeTypes.Json,
                Body = ToUtf8Bytes(json),
            }
        };
    }

    public static HttpResponse ServeFile(string filePath, string? overrideContentType = null)
    {
        return new HttpResponse
        {
            Code = HttpStatusCode.OK,
            Content = new HttpResponseContent
            {
                ContentType = overrideContentType != null
                    ? overrideContentType
                    : TryGetContentTypeFromFilePathOrDefaultContentType(filePath, MimeTypes.Binary),
                Body = File.ReadAllBytes(filePath)
            }
        };
    }

    public static HttpResponse BadRequestWithReason(string reason)
    {
        return new HttpResponse
        {
            Code = HttpStatusCode.BadRequest,
            Content = new HttpResponseContent
            {
                ContentType = MimeTypes.PlainText,
                Body = ToUtf8Bytes(reason)
            }
        };
    }
    #endregion

    private static byte[] ToUtf8Bytes(string text)
    {
        return Encoding.UTF8.GetBytes(text);
    }

    private static string TryGetContentTypeFromFilePathOrDefaultContentType(string filePath, string defaultContentType)
    {
        var extension = Path.GetExtension(filePath);

        if (CommonExtensionsToContentType.TryGetValue(extension, out var contentType))
        {
            return contentType;
        }
        else
        {
            return defaultContentType;
        }
    }

    private readonly static IReadOnlyDictionary<HttpStatusCode, string> StatusCodeToDescriptiveText = Enum
        .GetValues<HttpStatusCode>()
        // Distinct because HttpStatusCode contains "Ambiguous = 300" and "MultipleChoices = 300"
        .Distinct()
        .ToDictionary(
            x => x,
            // https://stackoverflow.com/a/4489031
            v => string.Join(" ", Regex.Split(v.ToString(), @"(?<!^)(?=[A-Z][a-z])")));

    private readonly static IReadOnlyDictionary<string, string> CommonExtensionsToContentType = new Dictionary<string, string>
    {
        [".html"] = MimeTypes.Html,
        [".htm"] = MimeTypes.Html,
        [".css"] = MimeTypes.Css,
        [".ico"] = MimeTypes.Ico,
        [".js"] = MimeTypes.JavaScript,
        [".json"] = MimeTypes.Json,
        [".png"] = MimeTypes.Png,
        [".svg"] = MimeTypes.Svg,
        [".jpg"] = MimeTypes.Jpeg,
        [".jpeg"] = MimeTypes.Jpeg,
    };
}
