using System.Text.Json.Serialization;
using DomainWatcher.Cli.Json.Responses;

namespace DomainWatcher.Cli.Json;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(List<WatchedDomainInfoJsonResponse>))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
