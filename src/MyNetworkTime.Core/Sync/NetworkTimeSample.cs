namespace MyNetworkTime.Core.Sync;

public sealed record NetworkTimeSample(
    DateTimeOffset ServerTimeUtc,
    DateTimeOffset ObservedAtUtc,
    TimeSpan Offset,
    TimeSpan RoundTripDelay);
