using DomainWatcher.Core.Extensions;

namespace DomainWatcher.Core.UnitTests.ExtensionsTests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class TimeSpanExtensionsTests
{
    [TestCase(0, ExpectedResult = "0ms")]
    [TestCase(200, ExpectedResult = "200ms")]
    [TestCase(999, ExpectedResult = "999ms")]
    [TestCase(1000, ExpectedResult = "1s")]
    [TestCase(1001, ExpectedResult = "1s")]
    [TestCase(1999, ExpectedResult = "1s")]
    [TestCase(2999, ExpectedResult = "2s")]
    [TestCase(1000L * 59, ExpectedResult = "59s")]
    [TestCase(1000L * 60, ExpectedResult = "1m")]
    [TestCase(1000L * 61, ExpectedResult = "1m 1s")]
    [TestCase(1000L * 60 * 60, ExpectedResult = "1h")]
    [TestCase(1000L * 60 * 60 + 1000 * 60 + 1000 * 1, ExpectedResult = "1h 1m 1s")]
    [TestCase(1000L * 60 * 60 * 24 + 1000 * 60 * 60 + 1000 * 60 + 1000 * 1, ExpectedResult = "1d 1h 1m 1s")]
    [TestCase(1000L * 60 * 60 * 24 * 365 * 10, ExpectedResult = "10y")]
    public static string WhenGettingJiraDuration_ReturnsCorrectString(long miliseconds)
    {
        var timeSpan = TimeSpan.FromMilliseconds(miliseconds);

        return timeSpan.ToJiraDuration();
    }

    [TestCase(1, 0, ExpectedResult = "0ms")]
    [TestCase(1, 1000, ExpectedResult = "1s")]
    [TestCase(1, 1000L * 61, ExpectedResult = "1m")]
    [TestCase(2, 1000L * 61, ExpectedResult = "1m 1s")]
    [TestCase(1, 1000L * 60 * 60 + 1000 * 60 + 1000 * 1, ExpectedResult = "1h")]
    [TestCase(2, 1000L * 60 * 60 + 1000 * 60 + 1000 * 1, ExpectedResult = "1h 1m")]
    [TestCase(3, 1000L * 60 * 60 + 1000 * 60 + 1000 * 1, ExpectedResult = "1h 1m 1s")]
    [TestCase(4, 1000L * 60 * 60 + 1000 * 60 + 1000 * 1, ExpectedResult = "1h 1m 1s")]
    public static string WhenGettingJiraDurationWithMaxSegments_ReturnsCorrectString(int maxSegments, long miliseconds)
    {
        var timeSpan = TimeSpan.FromMilliseconds(miliseconds);

        return timeSpan.ToJiraDuration(maxSegments);
    }
}
