using Serilog.Core;
using Serilog.Events;

namespace DomainWatcher.Cli.Internal.LogEnrichers;

internal class SourceContextNameEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!logEvent.Properties.TryGetValue("SourceContext", out var value))
        {
            return;
        }

        var sourceContext = value.ToString();
        var sourceContextNameProperty = propertyFactory.CreateProperty(
            "SourceContextName",
            sourceContext.Substring(sourceContext.LastIndexOf('.') + 1).Trim('"'));

        logEvent.AddOrUpdateProperty(sourceContextNameProperty);
    }
}
