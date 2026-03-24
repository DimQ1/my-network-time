namespace MyNetworkTime.Core.Platforms;

public interface IPermissionGuidanceService
{
    ValueTask<PlatformActionResult> OpenSystemTimeSettingsAsync(CancellationToken cancellationToken = default);
}
