using DomainWatcher.Core.Utilities;

namespace DomainWatcher.Core.UnitTests.ExtensionsTests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class JiraLikeDurationConverterTests
{
    private const long SecondFactor = 1000;
    private const long MinuteFactor = SecondFactor * 60;
    private const long HourFactor = MinuteFactor * 60;
    private const long DayFactor = HourFactor * 24;
    private const long MonthFactor = DayFactor * 30;
    private const long YearFactor = DayFactor * 365;

    [TestCase(0, ExpectedResult = "0ms")]
    [TestCase(200, ExpectedResult = "200ms")]
    [TestCase(999, ExpectedResult = "999ms")]
    [TestCase(1000, ExpectedResult = "1s")]
    [TestCase(1001, ExpectedResult = "1s")]
    [TestCase(1999, ExpectedResult = "1s")]
    [TestCase(2999, ExpectedResult = "2s")]
    [TestCase(3000, ExpectedResult = "3s")]
    [TestCase(SecondFactor * 59, ExpectedResult = "59s")]
    [TestCase(SecondFactor * 60, ExpectedResult = "1m")]
    [TestCase(SecondFactor * 61, ExpectedResult = "1m 1s")]
    [TestCase(HourFactor * 1, ExpectedResult = "1h")]
    [TestCase(HourFactor * 1 + SecondFactor * 1, ExpectedResult = "1h 1s")]
    [TestCase(HourFactor * 1 + MinuteFactor * 1 + SecondFactor * 1, ExpectedResult = "1h 1m 1s")]
    [TestCase(DayFactor * 1 + HourFactor * 1 + MinuteFactor * 1 + SecondFactor * 1, ExpectedResult = "1d 1h 1m 1s")]
    [TestCase(DayFactor * 2 + HourFactor * 1 + MinuteFactor * 1 + SecondFactor * 1, ExpectedResult = "2d 1h 1m 1s")]
    [TestCase(YearFactor * 10, ExpectedResult = "10y")]
    [TestCase(YearFactor * 10 + MonthFactor * 1, ExpectedResult = "10y 1mo")]
    [TestCase(YearFactor * 10 + MonthFactor * 2, ExpectedResult = "10y 2mo")]
    [TestCase(YearFactor * 10 + 1000 * 1, ExpectedResult = "10y 1s")]
    public static string WhenGettingJiraDuration_ReturnsCorrectString(long miliseconds)
    {
        var timeSpan = TimeSpan.FromMilliseconds(miliseconds);

        return JiraLikeDurationConverter.ToString(timeSpan);
    }

    [TestCase(1, 0, ExpectedResult = "0ms")]
    [TestCase(1, SecondFactor * 1, ExpectedResult = "1s")]
    [TestCase(1, SecondFactor * 61, ExpectedResult = "1m")]
    [TestCase(2, SecondFactor * 61, ExpectedResult = "1m 1s")]
    [TestCase(1, HourFactor * 1 + SecondFactor * 61, ExpectedResult = "1h")]
    [TestCase(2, HourFactor * 1 + SecondFactor * 61, ExpectedResult = "1h 1m")]
    [TestCase(3, HourFactor * 1 + SecondFactor * 61, ExpectedResult = "1h 1m 1s")]
    [TestCase(4, HourFactor * 1 + SecondFactor * 61, ExpectedResult = "1h 1m 1s")]
    [TestCase(2, YearFactor * 10 + SecondFactor * 1, ExpectedResult = "10y 1s")]
    public static string WhenGettingJiraDurationWithMaxSegments_ReturnsCorrectString(int maxSegments, long miliseconds)
    {
        var timeSpan = TimeSpan.FromMilliseconds(miliseconds);

        return JiraLikeDurationConverter.ToString(timeSpan, maxSegments);
    }

    [TestCase("0ms", ExpectedResult = 0)]
    [TestCase("200ms", ExpectedResult = 200)]
    [TestCase("999ms", ExpectedResult = 999)]
    [TestCase("1s", ExpectedResult = 1000)]
    [TestCase("1s 1ms", ExpectedResult = 1001)]
    [TestCase("1s 999ms", ExpectedResult = 1999)]
    [TestCase("2s 999ms", ExpectedResult = 2999)]
    [TestCase("3s", ExpectedResult = 3000)]
    [TestCase("59s", ExpectedResult = SecondFactor * 59)]
    [TestCase("1m", ExpectedResult = SecondFactor * 60)]
    [TestCase("1m 1s", ExpectedResult = SecondFactor * 61)]
    [TestCase("1h", ExpectedResult = HourFactor * 1)]
    [TestCase("1h 1s", ExpectedResult = HourFactor * 1 + SecondFactor * 1)]
    [TestCase("1h 1m 1s", ExpectedResult = HourFactor * 1 + SecondFactor * 61)]
    [TestCase("1d 1h 1m 1s", ExpectedResult = DayFactor * 1 + HourFactor * 1 + SecondFactor * 61)]
    [TestCase("10y", ExpectedResult = YearFactor * 10)]
    [TestCase("10y 1mo", ExpectedResult = YearFactor * 10 + MonthFactor * 1)]
    [TestCase("10y 1s", ExpectedResult = YearFactor * 10 + SecondFactor * 1)]
    public static long WhenParsingJiraDurationString_ReturnsTimestampWithCorrectNumberOfMiliseconds(string jiraDurationString)
    {
        var timeSpan = JiraLikeDurationConverter.ToTimeSpan(jiraDurationString);

        return (long)timeSpan.TotalMilliseconds;
    }

    [TestCase("-7ms")]
    [TestCase("-1s")]
    [TestCase("1q")]
    [TestCase("1 light year")]
    [TestCase("1lightyear")]
    [TestCase("1q2q")]
    public static void WhenParsingInvalidDurationString_ThrowsArgumentException(string invalidDurationString)
    {
        Assert.Throws<ArgumentException>(() => JiraLikeDurationConverter.ToTimeSpan(invalidDurationString));
    }
}
