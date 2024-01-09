using DomainWatcher.Cli.Services;
using Serilog.Core;
using Serilog.Events;

namespace DomainWatcher.Cli.Logging.Sinks;

public class HostStoppingWhenFatalLogSink(HostCancellation hostCancellation) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        hostCancellation.Cancel();
    }
}
