using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Values;

namespace DomainWatcher.Core.UnitTests.ValuesTests;

public class TimedSequenceTests
{
    [Test]
    public async Task ReadsInExpectedOrder()
    {
        var timedSequence = new TimedSequence<string>();

        _ = Task.Run(async () =>
        {
            await Task.Delay(100);
            timedSequence.Add("3rd", TimeSpan.FromMilliseconds(300));
            await Task.Delay(100);
            timedSequence.Add("2nd", TimeSpan.FromMilliseconds(150));
            await Task.Delay(100);
            timedSequence.Add("1st", TimeSpan.FromMilliseconds(-1000));
        });

        Assert.That(
            new[] { "1st", "2nd", "3rd" },
            Is.EqualTo(await timedSequence.EnumerateAsync().Take(3).ToListAsync()));
    }

    [Test]
    public async Task GivenRandomDelays_ReadsInOrder()
    {
        var timedSequence = new TimedSequence<int>();
        var random = new Random();
        var delay = TimeSpan.Zero;
        var itemsWithDelay = new (int Value, TimeSpan Delay)[10];

        for (var i = 0; i < itemsWithDelay.Length; i++)
        {
            delay += TimeSpan.FromMilliseconds(random.Next(10, 20));
            itemsWithDelay[i] = (i, delay);
        }

        random.Shuffle(itemsWithDelay);
        itemsWithDelay.ForEach(x => timedSequence.Add(x.Value, x.Delay));

        Assert.That(
            await timedSequence.EnumerateAsync().Take(itemsWithDelay.Length).ToListAsync(),
            Is.Ordered);
    }

    [Test]
    public async Task MultipleReaders_ThrowsInvalidOne()
    {
        var timedSequence = new TimedSequence<int>();

        var validReadTask = timedSequence.GetNext();
        await Task.Delay(10);
        var readTaskToBeInvalidOperation = timedSequence.GetNext();
        await Task.Delay(10);
        timedSequence.Add(1, TimeSpan.Zero);

        Assert.ThrowsAsync<InvalidOperationException>(async () => await readTaskToBeInvalidOperation);
        Assert.That(await validReadTask, Is.EqualTo(1));
    }
}
