namespace DomainWatcher.Core.Extensions;

public static class TaskExtensions
{
    public static async Task CaptureCancellation(this Task task)
    {
        try
        {
            await task;
        }
        catch (TaskCanceledException) { }
    }
}
