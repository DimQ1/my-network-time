namespace MyNetworkTime.Core.Platforms;

public sealed record PlatformCapabilities(
    AppPlatform Platform,
    bool SupportsDirectTimeAdjustment,
    bool SupportsBackgroundRefresh,
    string SupportLevel,
    string Summary,
    string Guidance);
