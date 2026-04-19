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
    ITimeAdjustmentService timeAdjustmentService,
    IPermissionGuidanceService permissionGuidanceService,
    WindowsTrayBehaviorService windowsTrayBehaviorService,
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

    public async ValueTask SaveSettingsAsync(AppSettingsSnapshot settings, CancellationToken cancellationToken = default)
    {
        await settingsRepository.SaveAsync(settings, cancellationToken);
        await windowsTrayBehaviorService.RefreshAsync(cancellationToken);
    }

    public ValueTask<IReadOnlyList<LogEntrySnapshot>> GetLogsAsync(CancellationToken cancellationToken = default) =>
        logRepository.GetRecentAsync(cancellationToken);

    public async ValueTask<TimeAdjustmentResult> AdjustSystemTimeAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsRepository.GetAsync(cancellationToken);
        if (settings.AdjustmentMode == TimeAdjustmentMode.DoNotUpdateTime)
        {
            return TimeAdjustmentResult.Failure("Time adjustment is disabled in Settings.");
        }

        var state = await syncStateRepository.GetAsync(cancellationToken);
        if (state?.LastSyncOffset is null)
        {
            return TimeAdjustmentResult.Failure("No successful sync sample is available yet. Run Update Now first.");
        }

        var availability = timeAdjustmentService.GetAvailability();
        if (!availability.CanAdjustNow)
        {
            return TimeAdjustmentResult.Failure(availability.Guidance);
        }

        var targetUtc = timeProvider.GetUtcNow() + state.LastSyncOffset.Value;
        var result = await timeAdjustmentService.TryAdjustAsync(targetUtc, cancellationToken);
        var loggedAtUtc = timeProvider.GetUtcNow();

        await logRepository.AppendAsync(
        [
            new LogEntrySnapshot(
                Timestamp: loggedAtUtc,
                Message: result.Succeeded
                    ? "System clock adjusted from the latest stored sync sample."
                    : $"System clock adjustment failed: {result.Message}",
                Context: "Manual Adjustment")
        ], cancellationToken);

        if (!result.Succeeded)
        {
            return result;
        }

        await syncStateRepository.SaveAsync(
            state with
            {
                LastSyncOffset = TimeSpan.Zero,
                SummaryStatus = "System clock adjusted from the latest network sample.",
                LastError = null
            },
            cancellationToken);

        return result;
    }

    public async ValueTask<TimeAdjustmentResult> SetSystemTimeAsync(DateTimeOffset targetLocalTime, CancellationToken cancellationToken = default)
    {
        var availability = timeAdjustmentService.GetAvailability();
        if (!availability.CanAdjustNow)
        {
            return TimeAdjustmentResult.Failure(availability.Guidance);
        }

        var result = await timeAdjustmentService.TryAdjustAsync(targetLocalTime, cancellationToken);
        var loggedAtUtc = timeProvider.GetUtcNow();

        await logRepository.AppendAsync(
        [
            new LogEntrySnapshot(
                Timestamp: loggedAtUtc,
                Message: result.Succeeded
                    ? $"System clock set manually to {targetLocalTime.ToLocalTime():M/d/yyyy h:mm:ss tt}."
                    : $"Manual system clock update failed: {result.Message}",
                Context: "Manual Time Set")
        ], cancellationToken);

        if (!result.Succeeded)
        {
            return result;
        }

        var state = await syncStateRepository.GetAsync(cancellationToken);
        if (state is not null)
        {
            await syncStateRepository.SaveAsync(
                state with
                {
                    LastSyncOffset = TimeSpan.Zero,
                    SummaryStatus = "System clock set manually from dashboard.",
                    LastError = null
                },
                cancellationToken);
        }

        return result;
    }

    public ValueTask<PlatformActionResult> OpenSystemTimeSettingsAsync(CancellationToken cancellationToken = default) =>
        permissionGuidanceService.OpenSystemTimeSettingsAsync(cancellationToken);

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

        var decision = state.LastSyncOffset is { } offset
            ? TimeAdjustmentPolicy.Evaluate(settings, offset, timeAdjustmentService.GetAvailability())
            : null;

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
            CanAdjustSystemTimeFromDashboard: decision?.Kind == TimeAdjustmentDecisionKind.AwaitingUserConfirmation,
            AdjustActionLabel: BuildAdjustActionLabel(settings, capabilities),
            Servers: servers);
    }

    private static string BuildModeLabel(AppSettingsSnapshot settings, PlatformCapabilities capabilities)
    {
        return settings.AdjustmentMode switch
        {
            TimeAdjustmentMode.AdjustSystemTime when capabilities.CanAdjustDirectlyNow => "Automatic system clock sync",
            TimeAdjustmentMode.AdjustSystemTime when capabilities.SupportsDirectTimeAdjustment => "Automatic sync (needs elevation)",
            TimeAdjustmentMode.AdjustSystemTime => "Guided sync mode",
            TimeAdjustmentMode.DoNotUpdateTime => "Read-only monitoring",
            TimeAdjustmentMode.AskUser when capabilities.CanAdjustDirectlyNow => "Ask before adjusting",
            TimeAdjustmentMode.AskUser => "Ask with guided fallback",
            _ => "Monitor mode"
        };
    }

    private static string BuildAdjustActionLabel(AppSettingsSnapshot settings, PlatformCapabilities capabilities)
    {
        return settings.AdjustmentMode switch
        {
            TimeAdjustmentMode.AskUser when capabilities.CanAdjustDirectlyNow => "Adjust System Time",
            TimeAdjustmentMode.AskUser => "Adjustment unavailable",
            TimeAdjustmentMode.AdjustSystemTime when capabilities.CanAdjustDirectlyNow => "Auto-adjust active",
            TimeAdjustmentMode.AdjustSystemTime => "Run elevated to adjust",
            _ => "Adjustment disabled"
        };
    }
}
