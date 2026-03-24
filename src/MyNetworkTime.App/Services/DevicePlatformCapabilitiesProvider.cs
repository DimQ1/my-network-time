using Microsoft.Maui.Devices;
using MyNetworkTime.Core.Platforms;

namespace MyNetworkTime.App.Services;

internal sealed class DevicePlatformCapabilitiesProvider(ITimeAdjustmentService timeAdjustmentService) : IPlatformCapabilitiesProvider
{
    public PlatformCapabilities GetCurrentCapabilities()
    {
        var adjustment = timeAdjustmentService.GetAvailability();

        return DetectPlatform() switch
        {
            AppPlatform.Windows => new PlatformCapabilities(
                Platform: AppPlatform.Windows,
                SupportsDirectTimeAdjustment: adjustment.IsSupported,
                CanAdjustDirectlyNow: adjustment.CanAdjustNow,
                RequiresElevation: adjustment.RequiresElevation,
                SupportsBackgroundRefresh: true,
                CanOpenSystemTimeSettings: true,
                SupportLevel: adjustment.CanAdjustNow ? "Full sync target" : "Full sync with elevation",
                Summary: adjustment.Summary,
                Guidance: adjustment.Guidance,
                BackgroundRefreshMode: "Foreground scheduler with due-time refresh",
                SettingsActionLabel: "Open Date & Time Settings"),
            AppPlatform.Android => new PlatformCapabilities(
                Platform: AppPlatform.Android,
                SupportsDirectTimeAdjustment: false,
                CanAdjustDirectlyNow: false,
                RequiresElevation: false,
                SupportsBackgroundRefresh: true,
                CanOpenSystemTimeSettings: true,
                SupportLevel: "Monitor with guidance",
                Summary: "Android can query network time and keep a foreground schedule while the app is active, but direct clock changes stay reserved for privileged apps.",
                Guidance: "Use Update Now to measure drift, then open Android Date & Time settings if a manual correction is needed.",
                BackgroundRefreshMode: "Foreground scheduler while the app is open",
                SettingsActionLabel: "Open Date & Time Settings"),
            AppPlatform.Ios => new PlatformCapabilities(
                Platform: AppPlatform.Ios,
                SupportsDirectTimeAdjustment: false,
                CanAdjustDirectlyNow: false,
                RequiresElevation: false,
                SupportsBackgroundRefresh: true,
                CanOpenSystemTimeSettings: false,
                SupportLevel: "Monitor with guidance",
                Summary: "iOS can present drift and sync diagnostics and keep checks on a foreground schedule, but public APIs do not allow direct system clock changes.",
                Guidance: "Open Settings > General > Date & Time manually if the sampled drift suggests the device clock is wrong.",
                BackgroundRefreshMode: "Foreground scheduler while the app is open",
                SettingsActionLabel: "Review manual steps"),
            _ => new PlatformCapabilities(
                Platform: AppPlatform.Unknown,
                SupportsDirectTimeAdjustment: false,
                CanAdjustDirectlyNow: false,
                RequiresElevation: false,
                SupportsBackgroundRefresh: false,
                CanOpenSystemTimeSettings: false,
                SupportLevel: "Unknown platform",
                Summary: "Platform-specific capability checks are not available yet.",
                Guidance: "Platform-specific guidance is unavailable for this target.",
                BackgroundRefreshMode: "Unknown",
                SettingsActionLabel: "Open Settings")
        };
    }

    private static AppPlatform DetectPlatform()
    {
        var platform = DeviceInfo.Current.Platform;

        if (platform == DevicePlatform.WinUI)
        {
            return AppPlatform.Windows;
        }

        if (platform == DevicePlatform.Android)
        {
            return AppPlatform.Android;
        }

        if (platform == DevicePlatform.iOS)
        {
            return AppPlatform.Ios;
        }

        return AppPlatform.Unknown;
    }
}
