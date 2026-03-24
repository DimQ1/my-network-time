using MyNetworkTime.Core.Dashboard;
using MyNetworkTime.Core.Logs;
using MyNetworkTime.Core.Settings;

namespace MyNetworkTime.Core.Services;

public interface INetworkTimeWorkspaceService
{
    ValueTask<DashboardSnapshot> GetDashboardAsync(CancellationToken cancellationToken = default);

    ValueTask<AppSettingsSnapshot> GetSettingsAsync(CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<LogEntrySnapshot>> GetLogsAsync(CancellationToken cancellationToken = default);
}
