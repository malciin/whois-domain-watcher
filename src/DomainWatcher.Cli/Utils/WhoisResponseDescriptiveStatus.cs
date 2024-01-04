using DomainWatcher.Core.Enums;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Values;

namespace DomainWatcher.Cli.Utils;

public static class WhoisResponseDescriptiveStatus
{
    public static string Get(WhoisResponse? latestResponse)
    {
        if (latestResponse == null) return "Not yet queried";
        if (latestResponse.IsAvailable) return "Available";
        if (latestResponse.Status == WhoisResponseStatus.OK) return $"Taken for {(latestResponse.Expiration!.Value - DateTime.UtcNow).ToJiraDuration(2)} ({latestResponse.Expiration!.Value:yyyy-MM-dd HH:mm})";
        if (latestResponse.Status == WhoisResponseStatus.ParserMissing) return $"Missing parser for {latestResponse.SourceServer}";
        if (latestResponse.Status == WhoisResponseStatus.TakenButTimestampsHidden) return $"Taken for HIDDEN ({latestResponse.SourceServer} hides that information)";

        throw new Exception("Unsupported case");
    }
}
