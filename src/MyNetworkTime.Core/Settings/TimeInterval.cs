namespace MyNetworkTime.Core.Settings;

public readonly record struct TimeInterval(int Value, IntervalUnit Unit)
{
    public TimeSpan ToTimeSpan()
    {
        if (Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Value), "Interval value cannot be negative.");
        }

        return Unit switch
        {
            IntervalUnit.Second => TimeSpan.FromSeconds(Value),
            IntervalUnit.Minute => TimeSpan.FromMinutes(Value),
            IntervalUnit.Hour => TimeSpan.FromHours(Value),
            IntervalUnit.Day => TimeSpan.FromDays(Value),
            IntervalUnit.Week => TimeSpan.FromDays(Value * 7d),
            IntervalUnit.Year => TimeSpan.FromDays(Value * 365d),
            _ => throw new ArgumentOutOfRangeException(nameof(Unit), Unit, "Unsupported interval unit.")
        };
    }

    public override string ToString()
    {
        var unit = Unit switch
        {
            IntervalUnit.Second => "second",
            IntervalUnit.Minute => "minute",
            IntervalUnit.Hour => "hour",
            IntervalUnit.Day => "day",
            IntervalUnit.Week => "week",
            IntervalUnit.Year => "year",
            _ => "unit"
        };

        return $"{Value} {unit}{(Value == 1 ? string.Empty : "s")}";
    }
}
