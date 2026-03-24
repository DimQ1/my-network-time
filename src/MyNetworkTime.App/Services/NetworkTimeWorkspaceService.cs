using MyNetworkTime.Core.Dashboard;
using MyNetworkTime.Core.Logs;
using MyNetworkTime.Core.Platforms;
using MyNetworkTime.Core.Services;
using MyNetworkTime.Core.Settings;
using MyNetworkTime.Core.Storage;
using MyNetworkTime.Core.Sync;

namespace MyNetworkTime.App.Services;

internal sealed class NetworkTimeWorkspaceService(
    ISettingsRepository settingsRepository,
    ILogRepository logRepository,
    ISyncStateRepository syncStateRepository,
    SyncCoordinator syncCoordinator,
    IPlatformCapabilitiesProvider platformCapabilitiesProvider,
    TimeProvider timeProvider) : INetworkTimeWorkspaceService
{
    public async ValueTask<DashboardSnapshot> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsRepository.GetAsync(cancellationToken);
        var state = await syncStateRepository.GetAsync(cancellationToken) ?? SyncStateSnapshot.CreateInitial(settings.Servers);

        return BuildDashboardSnapshot(settings, state);
    }

    public async ValueTask<DashboardSnapshot> RefreshAsync(SyncTrigger trigger, CancellationToken cancellationToken = default)
    {
        var settings = await settingsRepository.GetAsync(cancellationToken);
        var state = await syncCoordinator.RefreshAsync(trigger, cancellationToken);

        return BuildDashboardSnapshot(settings, state);
    }

    public ValueTask<AppSettingsSnapshot> GetSettingsAsync(CancellationToken cancellationToken = default) =>
        settingsRepository.GetAsync(cancellationToken);

    public ValueTask SaveSettingsAsync(AppSettingsSnapshot settings, CancellationToken cancellationToken = default) =>
        settingsRepository.SaveAsync(settings, cancellationToken);

    public ValueTask<IReadOnlyList<LogEntrySnapshot>> GetLogsAsync(CancellationToken cancellationToken = default) =>
        logRepository.GetRecentAsync(cancellationToken);

    private DashboardSnapshot BuildDashboardSnapshot(AppSettingsSnapshot settings, SyncStateSnapshot state)
    {
        var capabilities = platformCapabilitiesProvider.GetCurrentCapabilities();
        var now = timeProvider.GetLocalNow();
        var nextAttemptIn = state.NextAttemptUtc is { } nextAttemptUtc
            ? nextAttemptUtc - now.ToUniversalTime()
            : settings.UpdateInterval.ToTimeSpan();

        if (nextAttemptIn < TimeSpan.Zero)
        {
            nextAttemptIn = TimeSpan.Zero;
        }

        var statesByServer = state.Servers.ToDictionary(server => server.Endpoint);
        var servers = settings.Servers
            .Select(server =>
            {
                statesByServer.TryGetValue(server, out var runtime);

                return new ServerStatusSnapshot(
                    Name: server.Host,
                    Protocol: server.Protocol,
                    State: runtime?.State ?? ServerHealthState.NotUsed,
                    Offset: runtime?.Offset,
                    Lag: runtime?.Lag,
                    LastError: runtime?.LastError);
            })
            .ToList();

        return new DashboardSnapshot(
            CurrentTime: now,
            LastAttempt: state.LastAttemptUtc?.ToLocalTime(),
            LastSync: state.LastSyncUtc?.ToLocalTime(),
            LastSyncOffset: state.LastSyncOffset,
            NextAttemptIn: nextAttemptIn,
            SummaryStatus: state.SummaryStatus,
            ModeLabel: BuildModeLabel(settings, capabilities),
            LastError: state.LastError,
            Platform: capabilities,
            Servers: servers);
    }

    private static string BuildModeLabel(AppSettingsSnapshot settings, PlatformCapabilities capabilities)
    {
        return settings.AdjustmentMode switch
        {
            TimeAdjustmentMode.AdjustSystemTime when capabilities.SupportsDirectTimeAdjustment => "System clock sync",
            TimeAdjustmentMode.AdjustSystemTime => "Monitor mode",
            TimeAdjustmentMode.DoNotUpdateTime => "Read-only monitoring",
            TimeAdjustmentMode.AskUser => "Ask before adjusting",
            _ => "Monitor mode"
        };
    }
}
