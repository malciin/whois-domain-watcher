using DomainWatcher.Core.Enums;

namespace DomainWatcher.Cli.Json.Responses;

public class WatchedDomainInfoJsonResponse
{
    public required string Domain { get; set; }

    public required WhoisResponseStatus? QueryStatus { get; set; }

    public required DateTime? QueryTimestamp { get; set; }

    public required DateTime? ExpirationTimestamp { get; set; }

    public required DateTime? RegistrationTimestamp { get; set; }
}
