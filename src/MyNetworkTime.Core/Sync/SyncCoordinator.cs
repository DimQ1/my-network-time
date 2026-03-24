using Microsoft.Extensions.Logging;
using MyNetworkTime.Core.Dashboard;
using MyNetworkTime.Core.Logs;
using MyNetworkTime.Core.Protocols;
using MyNetworkTime.Core.Settings;
using MyNetworkTime.Core.Storage;

namespace MyNetworkTime.Core.Sync;

public sealed class SyncCoordinator(
    ISettingsRepository settingsRepository,
    ISyncStateRepository syncStateRepository,
    ILogRepository logRepository,
    NetworkTimeProtocolClientResolver protocolResolver,
    ServerSelectionPolicy selectionPolicy,
    TimeProvider timeProvider,
    ILogger<SyncCoordinator> logger)
{
    public async ValueTask<SyncStateSnapshot> RefreshAsync(SyncTrigger trigger, CancellationToken cancellationToken = default)
    {
        var settings = await settingsRepository.GetAsync(cancellationToken);
        var previousState = await syncStateRepository.GetAsync(cancellationToken) ?? SyncStateSnapshot.CreateInitial(settings.Servers);
        var orderedServers = selectionPolicy.OrderServers(settings, previousState);

        var attemptResults = new Dictionary<ServerEndpointSettings, ServerAttemptResult>();
        var successfulSamples = new List<(ServerEndpointSettings Endpoint, NetworkTimeSample Sample)>();
        var attempts = 0;
        var startedAtUtc = timeProvider.GetUtcNow();

        foreach (var server in orderedServers)
        {
            if (ShouldStopAttempting(attempts, successfulSamples.Count))
            {
                break;
            }

            attempts++;

            try
            {
                var client = protocolResolver.Resolve(server.Protocol);
                var sample = await client.QueryAsync(server, cancellationToken);
                successfulSamples.Add((server, sample));
                attemptResults[server] = ServerAttemptResult.Success(server, sample);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Failed to query time server {Host}:{Port} via {Protocol}.", server.Host, server.Port, server.Protocol);
                attemptResults[server] = ServerAttemptResult.Failure(server, startedAtUtc, exception.Message);
            }
        }

        foreach (var server in settings.Servers)
        {
            if (!attemptResults.ContainsKey(server))
            {
                attemptResults[server] = ServerAttemptResult.NotUsed(server);
            }
        }

        var nextAttemptUtc = startedAtUtc + GetNextDelay(settings, successfulSamples.Count > 0);
        var updatedServers = BuildServerStates(settings, previousState, attemptResults);
        var logs = BuildLogs(trigger, successfulSamples, attemptResults, startedAtUtc);

        SyncStateSnapshot state;
        if (successfulSamples.Count > 0)
        {
            var selected = successfulSamples
                .OrderBy(result => result.Sample.RoundTripDelay)
                .First();

            state = new SyncStateSnapshot(
                LastAttemptUtc: startedAtUtc,
                LastSyncUtc: selected.Sample.ObservedAtUtc,
                LastSyncOffset: selected.Sample.Offset,
                NextAttemptUtc: nextAttemptUtc,
                SummaryStatus: "Time is synchronized.",
                LastError: attemptResults.Values.FirstOrDefault(result => result.State == ServerHealthState.Error)?.Error,
                Servers: updatedServers);
        }
        else
        {
            var error = attemptResults.Values
                .Where(result => result.State == ServerHealthState.Error)
                .Select(result => result.Error)
                .FirstOrDefault() ?? "All configured time servers failed.";

            state = new SyncStateSnapshot(
                LastAttemptUtc: startedAtUtc,
                LastSyncUtc: previousState.LastSyncUtc,
                LastSyncOffset: previousState.LastSyncOffset,
                NextAttemptUtc: nextAttemptUtc,
                SummaryStatus: "Time sync failed.",
                LastError: error,
                Servers: updatedServers);
        }

        await syncStateRepository.SaveAsync(state, cancellationToken);
        await logRepository.AppendAsync(logs, cancellationToken);

        return state;
    }

    private static bool ShouldStopAttempting(int attempts, int successfulSamples)
    {
        if (successfulSamples >= 2)
        {
            return true;
        }

        return successfulSamples >= 1 && attempts >= 2;
    }

    private static TimeSpan GetNextDelay(AppSettingsSnapshot settings, bool succeeded)
    {
        return succeeded
            ? settings.UpdateInterval.ToTimeSpan()
            : settings.RetryInterval.ToTimeSpan();
    }

    private static IReadOnlyList<ServerSyncState> BuildServerStates(
        AppSettingsSnapshot settings,
        SyncStateSnapshot previousState,
        IReadOnlyDictionary<ServerEndpointSettings, ServerAttemptResult> attemptResults)
    {
        var previousByServer = previousState.Servers.ToDictionary(server => server.Endpoint);

        return settings.Servers
            .Select(server =>
            {
                previousByServer.TryGetValue(server, out var previous);
                var result = attemptResults[server];

                if (result.Sample is not null)
                {
                    return new ServerSyncState(
                        Endpoint: server,
                        ConsecutiveFailures: 0,
                        State: ServerHealthState.Good,
                        IsDemoted: false,
                        LastAttemptUtc: result.Sample.ObservedAtUtc,
                        LastSuccessUtc: result.Sample.ObservedAtUtc,
                        Offset: result.Sample.Offset,
                        Lag: result.Sample.RoundTripDelay,
                        LastError: null);
                }

                if (result.State == ServerHealthState.Error)
                {
                    var failures = (previous?.ConsecutiveFailures ?? 0) + 1;
                    var demoted = settings.DemoteServers && failures >= settings.DemoteAfterFailures;
                    var error = demoted
                        ? $"{result.Error} (demoted after {failures} failures)"
                        : result.Error;

                    return new ServerSyncState(
                        Endpoint: server,
                        ConsecutiveFailures: failures,
                        State: demoted ? ServerHealthState.Warning : ServerHealthState.Error,
                        IsDemoted: demoted,
                        LastAttemptUtc: result.AttemptedAtUtc,
                        LastSuccessUtc: previous?.LastSuccessUtc,
                        Offset: previous?.Offset,
                        Lag: previous?.Lag,
                        LastError: error);
                }

                if (previous is not null)
                {
                    return previous with
                    {
                        State = previous.IsDemoted ? ServerHealthState.Warning : ServerHealthState.NotUsed
                    };
                }

                return new ServerSyncState(
                    Endpoint: server,
                    ConsecutiveFailures: 0,
                    State: ServerHealthState.NotUsed,
                    IsDemoted: false,
                    LastAttemptUtc: null,
                    LastSuccessUtc: null,
                    Offset: null,
                    Lag: null,
                    LastError: null);
            })
            .ToList();
    }

    private static IReadOnlyList<LogEntrySnapshot> BuildLogs(
        SyncTrigger trigger,
        IReadOnlyList<(ServerEndpointSettings Endpoint, NetworkTimeSample Sample)> successfulSamples,
        IReadOnlyDictionary<ServerEndpointSettings, ServerAttemptResult> attemptResults,
        DateTimeOffset startedAtUtc)
    {
        var context = trigger switch
        {
            SyncTrigger.InitialStartup => "Initial Startup",
            SyncTrigger.BackgroundRefresh => "Background Refresh",
            SyncTrigger.Retry => "Retry",
            _ => "Manual Update"
        };

        var logs = new List<LogEntrySnapshot>();

        if (successfulSamples.Count > 0)
        {
            var selected = successfulSamples
                .OrderBy(result => result.Sample.RoundTripDelay)
                .First();

            logs.Add(new LogEntrySnapshot(
                Timestamp: startedAtUtc,
                Message: $"Time Updated: {selected.Sample.Offset.TotalMilliseconds:+0.##;-0.##;0}ms",
                Context: context));
        }
        else
        {
            logs.Add(new LogEntrySnapshot(
                Timestamp: startedAtUtc,
                Message: "Time Sync Failed!",
                Context: context));

            var firstError = attemptResults.Values
                .Where(result => result.State == ServerHealthState.Error)
                .Select(result => result.Error)
                .FirstOrDefault() ?? "All configured time servers failed.";

            logs.Add(new LogEntrySnapshot(
                Timestamp: startedAtUtc,
                Message: $"Failure Reason: {firstError}",
                Context: context));
        }

        return logs;
    }

    private sealed record ServerAttemptResult(
        ServerEndpointSettings Endpoint,
        ServerHealthState State,
        DateTimeOffset AttemptedAtUtc,
        NetworkTimeSample? Sample,
        string? Error)
    {
        public static ServerAttemptResult Success(ServerEndpointSettings endpoint, NetworkTimeSample sample) =>
            new(endpoint, ServerHealthState.Good, sample.ObservedAtUtc, sample, null);

        public static ServerAttemptResult Failure(ServerEndpointSettings endpoint, DateTimeOffset attemptedAtUtc, string error) =>
            new(endpoint, ServerHealthState.Error, attemptedAtUtc, null, error);

        public static ServerAttemptResult NotUsed(ServerEndpointSettings endpoint) =>
            new(endpoint, ServerHealthState.NotUsed, DateTimeOffset.MinValue, null, null);
    }
}
