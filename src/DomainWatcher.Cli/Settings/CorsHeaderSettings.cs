using Microsoft.Extensions.Configuration;

namespace DomainWatcher.Cli.Settings;

public class CorsHeaderSettings
{
    public required string Methods { get; init; }

    public required string Origin { get; init; }

    public required string Headers { get; init; }

    public CorsHeaderSettings(IConfiguration configuration)
    {
        var section = configuration.GetSection("Cors");

        Methods = section[nameof(Methods)]!;
        Origin = section[nameof(Origin)]!;
        Headers = section[nameof(Headers)]!;
    }
}
