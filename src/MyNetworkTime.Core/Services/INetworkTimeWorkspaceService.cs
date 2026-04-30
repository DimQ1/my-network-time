using MyNetworkTime.Core.Dashboard;
using MyNetworkTime.Core.Logs;
using MyNetworkTime.Core.Platforms;
using MyNetworkTime.Core.Settings;
using MyNetworkTime.Core.Sync;

namespace MyNetworkTime.Core.Services;

public interface INetworkTimeWorkspaceService
{
    ValueTask<DashboardSnapshot> GetDashboardAsync(CancellationToken cancellationToken = default);

    ValueTask<DashboardSnapshot> RefreshAsync(SyncTrigger trigger, CancellationToken cancellationToken = default);

    ValueTask<AppSettingsSnapshot> GetSettingsAsync(CancellationToken cancellationToken = default);

    ValueTask SaveSettingsAsync(AppSettingsSnapshot settings, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<LogEntrySnapshot>> GetLogsAsync(CancellationToken cancellationToken = default);

    ValueTask<TimeAdjustmentResult> AdjustSystemTimeAsync(CancellationToken cancellationToken = default);

    ValueTask<TimeAdjustmentResult> SetSystemTimeAsync(DateTimeOffset targetLocalTime, CancellationToken cancellationToken = default);

    ValueTask<PlatformActionResult> OpenSystemTimeSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests elevated privileges for time adjustment.
    /// On Windows, triggers UAC and restarts the app as administrator.
    /// </summary>
    ValueTask<ElevationRequestResult> RequestTimeAdjustmentElevationAsync(CancellationToken cancellationToken = default);
}
