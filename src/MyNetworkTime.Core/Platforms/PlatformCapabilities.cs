namespace MyNetworkTime.Core.Platforms;

public sealed record PlatformCapabilities(
    AppPlatform Platform,
    bool SupportsDirectTimeAdjustment,
    bool CanAdjustDirectlyNow,
    bool RequiresElevation,
    bool SupportsBackgroundRefresh,
    bool CanOpenSystemTimeSettings,
    string SupportLevel,
    string Summary,
    string Guidance,
    string BackgroundRefreshMode,
    string SettingsActionLabel);
