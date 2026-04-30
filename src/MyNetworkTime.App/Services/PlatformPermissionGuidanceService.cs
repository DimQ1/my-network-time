using Microsoft.Maui.ApplicationModel;
using MyNetworkTime.Core.Platforms;

namespace MyNetworkTime.App.Services;

internal sealed class PlatformPermissionGuidanceService(ITimeAdjustmentService timeAdjustmentService) : IPermissionGuidanceService
{
    public async ValueTask<PlatformActionResult> OpenSystemTimeSettingsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
#if WINDOWS
            var opened = await Launcher.Default.OpenAsync(new Uri("ms-settings:dateandtime"));
            return opened
                ? PlatformActionResult.Success("Opened Windows Date & Time settings.")
                : PlatformActionResult.Failure("Windows Date & Time settings did not open.");
#elif ANDROID
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                var intent = new global::Android.Content.Intent(global::Android.Provider.Settings.ActionDateSettings);
                intent.AddFlags(global::Android.Content.ActivityFlags.NewTask);
                global::Android.App.Application.Context.StartActivity(intent);
            });

            return PlatformActionResult.Success("Opened Android Date & Time settings.");
#elif IOS
            return PlatformActionResult.Failure("iOS does not expose a public deep link to Date & Time settings. Open Settings > General > Date & Time manually.");
#else
            return PlatformActionResult.Failure("System Date & Time settings are not available on this platform.");
#endif
        }
        catch (Exception exception)
        {
            return PlatformActionResult.Failure($"Unable to open system settings: {exception.Message}");
        }
    }

    public ValueTask<ElevationRequestResult> RequestTimeAdjustmentElevationAsync(CancellationToken cancellationToken = default)
    {
        return timeAdjustmentService.RequestElevationAsync(cancellationToken);
    }
}
