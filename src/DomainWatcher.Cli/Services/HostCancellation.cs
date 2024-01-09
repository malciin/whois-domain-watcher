namespace DomainWatcher.Cli.Services;

public class HostCancellation : IDisposable
{
    public CancellationToken Token => cts.Token;

    private readonly CancellationTokenSource cts;

    public HostCancellation()
    {
        cts = new CancellationTokenSource();
    }

    public void Cancel()
    {
        cts.Cancel();
    }
    
    public void Dispose()
    {
        cts.Dispose();
    }
}
