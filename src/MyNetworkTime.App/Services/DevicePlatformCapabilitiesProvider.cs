using Microsoft.Maui.Devices;
using MyNetworkTime.Core.Platforms;

namespace MyNetworkTime.App.Services;

internal sealed class DevicePlatformCapabilitiesProvider : IPlatformCapabilitiesProvider
{
    public PlatformCapabilities GetCurrentCapabilities()
    {
        return DetectPlatform() switch
        {
            AppPlatform.Windows => new PlatformCapabilities(
                Platform: AppPlatform.Windows,
                SupportsDirectTimeAdjustment: true,
                SupportsBackgroundRefresh: true,
                SupportLevel: "Full sync target",
                Summary: "Windows is the first platform where we aim for real system time synchronization.",
                Guidance: "Stage 4 will wire elevated clock adjustment and scheduled refresh."),
            AppPlatform.Android => new PlatformCapabilities(
                Platform: AppPlatform.Android,
                SupportsDirectTimeAdjustment: false,
                SupportsBackgroundRefresh: true,
                SupportLevel: "Monitor with guidance",
                Summary: "Android can query network time and surface drift, but direct clock changes are typically reserved for privileged apps.",
                Guidance: "The app will focus on monitoring, retries, and shortcuts into system settings."),
            AppPlatform.Ios => new PlatformCapabilities(
                Platform: AppPlatform.Ios,
                SupportsDirectTimeAdjustment: false,
                SupportsBackgroundRefresh: true,
                SupportLevel: "Monitor with guidance",
                Summary: "iOS can present drift and sync diagnostics, but public APIs do not normally allow direct system clock changes.",
                Guidance: "The shared UI stays the same while iOS falls back to guidance flows."),
            _ => new PlatformCapabilities(
                Platform: AppPlatform.Unknown,
                SupportsDirectTimeAdjustment: false,
                SupportsBackgroundRefresh: false,
                SupportLevel: "Unknown platform",
                Summary: "Platform-specific capability checks are not available yet.",
                Guidance: "Stage 4 will tighten platform detection and native actions.")
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
