using MyNetworkTime.Core.Settings;

namespace MyNetworkTime.Core.Tests;

public sealed class TimeIntervalTests
{
    [Theory]
    [InlineData(5, IntervalUnit.Second, 5)]
    [InlineData(3, IntervalUnit.Minute, 180)]
    [InlineData(2, IntervalUnit.Hour, 7200)]
    [InlineData(1, IntervalUnit.Day, 86400)]
    [InlineData(2, IntervalUnit.Week, 1209600)]
    public void ToTimeSpan_ConvertsKnownUnits(int value, IntervalUnit unit, double expectedSeconds)
    {
        var interval = new TimeInterval(value, unit);

        var result = interval.ToTimeSpan();

        Assert.Equal(expectedSeconds, result.TotalSeconds);
    }

    [Fact]
    public void ToString_UsesReadablePluralization()
    {
        var interval = new TimeInterval(7, IntervalUnit.Day);

        var result = interval.ToString();

        Assert.Equal("7 days", result);
    }

    [Fact]
    public void ToTimeSpan_RejectsNegativeValues()
    {
        var interval = new TimeInterval(-1, IntervalUnit.Minute);

        Assert.Throws<ArgumentOutOfRangeException>(() => interval.ToTimeSpan());
    }
}
