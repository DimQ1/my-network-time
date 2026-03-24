using Microsoft.Maui.Devices;
using MyNetworkTime.Core.Dashboard;
using MyNetworkTime.Core.Logs;
using MyNetworkTime.Core.Platforms;
using MyNetworkTime.Core.Services;
using MyNetworkTime.Core.Settings;

namespace MyNetworkTime.App.Services;

internal sealed class DesignNetworkTimeWorkspaceService : INetworkTimeWorkspaceService
{
    private static readonly ServerEndpointSettings[] ServerSettings =
    [
        new("1.nettime.pool.ntp.org", ServerProtocol.Sntp, 123),
        new("192.168.31.1", ServerProtocol.Sntp, 123),
        new("2.nettime.pool.ntp.org", ServerProtocol.Sntp, 123),
        new("3.nettime.pool.ntp.org", ServerProtocol.Sntp, 123),
        new("0.openwrt.pool.ntp.org", ServerProtocol.Sntp, 123)
    ];

    public ValueTask<DashboardSnapshot> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.Now;
        var capabilities = BuildPlatformCapabilities();

        var snapshot = new DashboardSnapshot(
            CurrentTime: now,
            LastAttempt: now.AddMinutes(-49),
            LastSync: now.AddMinutes(-4),
            LastSyncOffset: TimeSpan.FromMilliseconds(45.39),
            NextAttemptIn: TimeSpan.FromHours(11) + TimeSpan.FromMinutes(55) + TimeSpan.FromSeconds(56),
            SummaryStatus: "Time is synchronized.",
            ModeLabel: capabilities.SupportsDirectTimeAdjustment ? "Windows time service" : "Monitor mode",
            LastError: capabilities.SupportsDirectTimeAdjustment ? "Network Down on Mar 23, 2026 10:29 PM" : "Direct system time changes are limited on this platform.",
            Platform: capabilities,
            Servers:
            [
                new("1.nettime.pool.ntp.org", ServerProtocol.Sntp, ServerHealthState.Good, TimeSpan.FromMilliseconds(45.39), TimeSpan.FromMilliseconds(21), null),
                new("192.168.31.1", ServerProtocol.Sntp, ServerHealthState.Good, TimeSpan.FromMilliseconds(45.39), TimeSpan.FromMilliseconds(2), null),
                new("2.nettime.pool.ntp.org", ServerProtocol.Sntp, ServerHealthState.NotUsed, null, null, null),
                new("3.nettime.pool.ntp.org", ServerProtocol.Sntp, ServerHealthState.NotUsed, null, null, null),
                new("0.openwrt.pool.ntp.org", ServerProtocol.Sntp, ServerHealthState.NotUsed, null, null, null)
            ]);

        return ValueTask.FromResult(snapshot);
    }

    public ValueTask<AppSettingsSnapshot> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = new AppSettingsSnapshot(
            Servers: ServerSettings,
            UpdateInterval: new TimeInterval(12, IntervalUnit.Hour),
            RetryInterval: new TimeInterval(1, IntervalUnit.Minute),
            DemoteServers: true,
            DemoteAfterFailures: 4,
            AllowPeersToSync: false,
            AlwaysProvideTime: false,
            ShowTrayIconAtLogin: true,
            StartAtBoot: true,
            MaxFreeRun: new TimeInterval(24, IntervalUnit.Hour),
            AdjustmentThreshold: new TimeInterval(2, IntervalUnit.Minute),
            AdjustmentMode: TimeAdjustmentMode.AdjustSystemTime,
            AutoCheckEveryDays: 7,
            LoggingLevel: "Normal");

        return ValueTask.FromResult(settings);
    }

    public ValueTask<IReadOnlyList<LogEntrySnapshot>> GetLogsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.Now;

        IReadOnlyList<LogEntrySnapshot> logs =
        [
            new(now.AddMinutes(-4), "Time Updated: +45.39ms", "Manual Update"),
            new(now.AddHours(-5), "Time Updated: +3ms", "Manual Update"),
            new(now.AddHours(-5).AddMinutes(-1), "Time Updated: +5ms", "Manual Update"),
            new(now.AddHours(-5).AddDays(-1), "Time Sync Failed!", "Initial Startup"),
            new(now.AddHours(-5).AddDays(-1).AddSeconds(1), "Failure Reason: Network Down", "Initial Startup"),
            new(now.AddDays(-1), "NetTime bootstrap shell started", "Stage 1 Bootstrap")
        ];

        return ValueTask.FromResult(logs);
    }

    private static PlatformCapabilities BuildPlatformCapabilities()
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
