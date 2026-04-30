namespace MyNetworkTime.Core.Platforms;

public interface IPermissionGuidanceService
{
    ValueTask<PlatformActionResult> OpenSystemTimeSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests elevated privileges for system time adjustment and restarts the app if granted.
    /// </summary>
    ValueTask<ElevationRequestResult> RequestTimeAdjustmentElevationAsync(CancellationToken cancellationToken = default);
}
