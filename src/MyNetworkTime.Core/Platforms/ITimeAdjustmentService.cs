namespace MyNetworkTime.Core.Platforms;

public interface ITimeAdjustmentService
{
    TimeAdjustmentAvailability GetAvailability();

    ValueTask<TimeAdjustmentResult> TryAdjustAsync(DateTimeOffset targetUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to request elevated privileges for time adjustment on platforms that support it.
    /// Returns true if elevation was requested and the app should restart.
    /// </summary>
    ValueTask<ElevationRequestResult> RequestElevationAsync(CancellationToken cancellationToken = default);
}

public sealed record ElevationRequestResult(
    bool ElevationRequested,
    bool RestartRequired,
    string Message)
{
    public static ElevationRequestResult NotSupported(string message) =>
        new(ElevationRequested: false, RestartRequired: false, Message: message);

    public static ElevationRequestResult RequestedAndRestart(string message) =>
        new(ElevationRequested: true, RestartRequired: true, Message: message);

    public static ElevationRequestResult AlreadyElevated(string message) =>
        new(ElevationRequested: false, RestartRequired: false, Message: message);
}
