using System.Runtime.CompilerServices;
using DomainWatcher.Core.Extensions;

namespace DomainWatcher.Core.Values;

public class TimedSequence<T>
{
    public int Count => sortedJobSequence.Count;

    private readonly SortedDictionary<DateTime, T> sortedJobSequence;
    private readonly object readerCtsLock;

    private ReaderCancellationTokenSource? readerCts;

    public TimedSequence()
    {
        sortedJobSequence = [];
        readerCtsLock = new();
    }

    public bool TryPeek(out DateTime fireAt, out T? item)
    {
        fireAt = DateTime.MinValue;
        item = default;

        lock (sortedJobSequence)
        {
            if (Count == 0) return false;

            (fireAt, item) = sortedJobSequence.FirstOrDefault();
        }

        return true;
    }

    public bool TryRemove(T item)
    {
        lock (sortedJobSequence)
        {
            try
            {
                var (fireAt, dequeued) = sortedJobSequence.First(x => x.Value?.Equals(item) == true);
                sortedJobSequence.Remove(fireAt);

                lock (readerCtsLock) readerCts?.QueueModified();

                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
    }

    public IReadOnlyList<KeyValuePair<DateTime, T>> GetEntries()
    {
        lock (sortedJobSequence) return sortedJobSequence.ToList();
    }

    public void Add(T item, TimeSpan delay) => Add(item, DateTime.UtcNow + delay);

    public void Add(T item, DateTime fireAt)
    {
        lock (sortedJobSequence)
        {
            // Unlikely to happend but for sake of peace of mind
            // if jobKeyByPlannedFireUp already exists then to avoid conflict increment datetime by 1 tick
            while (sortedJobSequence.ContainsKey(fireAt)) fireAt = new DateTime(fireAt.Ticks + 1, DateTimeKind.Utc);

            sortedJobSequence.Add(fireAt, item);
        }

        lock (readerCtsLock)
        {
            readerCts?.QueueModified();
        }
    }

    public IAsyncEnumerable<T> EnumerateAsync() => EnumerateAsync(CancellationToken.None);

    public async IAsyncEnumerable<T> EnumerateAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            yield return await GetNext(cancellationToken);
        }
    }

    public Task<T> GetNext() => GetNext(CancellationToken.None);

    public async Task<T> GetNext(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using var readerCts = CreateReaderTokenSourceLinkedTo(cancellationToken);

            if (!TryPeek(out var fireAt, out var item))
            {
                await Task.Delay(-1, readerCts.NewItemPresentToken).CaptureCancellation();
                continue;
            }

            var delay = fireAt - DateTime.UtcNow;

            if (delay.TotalMilliseconds > 0)
            {
                try
                {
                    await Task.Delay(delay, readerCts.NewItemPresentToken);
                }
                catch (TaskCanceledException)
                {
                    continue;
                }
            }

            lock (sortedJobSequence) sortedJobSequence.Remove(fireAt);

            return item!;
        }

        throw new TaskCanceledException("Read cancelled");
    }

    private ReaderCancellationTokenSource CreateReaderTokenSourceLinkedTo(CancellationToken token)
    {
        lock (readerCtsLock)
        {
            if (readerCts != null)
            {
                throw new InvalidOperationException("Previous reader was not disposed. This indicates that sequence is readed from multiple readers - thats not supported");
            }

            var reader = new ReaderCancellationTokenSource(ClearReaderTokenSource, token);

            readerCts = reader;

            return reader;
        }
    }

    private void ClearReaderTokenSource(ReaderCancellationTokenSource cts)
    {
        lock (readerCtsLock)
        {
            if (readerCts != cts)
            {
                throw new InvalidOperationException("Does not expect that to happend");
            }

            readerCts = null;
        }
    }

    private class ReaderCancellationTokenSource(
        Action<ReaderCancellationTokenSource> cleanUp,
        CancellationToken linkedToken) : IDisposable
    {
        public CancellationToken NewItemPresentToken => cts.Token;

        private readonly CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(linkedToken);

        public void QueueModified() => cts.Cancel();
        
        public void Dispose() => cleanUp(this);
    }
}
