using MyNetworkTime.Core.Dashboard;
using MyNetworkTime.Core.Settings;

namespace MyNetworkTime.Core.Sync;

public sealed record SyncStateSnapshot(
    DateTimeOffset? LastAttemptUtc,
    DateTimeOffset? LastSyncUtc,
    TimeSpan? LastSyncOffset,
    DateTimeOffset? NextAttemptUtc,
    string SummaryStatus,
    string? LastError,
    IReadOnlyList<ServerSyncState> Servers)
{
    public static SyncStateSnapshot CreateInitial(IReadOnlyList<ServerEndpointSettings> servers)
    {
        return new SyncStateSnapshot(
            LastAttemptUtc: null,
            LastSyncUtc: null,
            LastSyncOffset: null,
            NextAttemptUtc: null,
            SummaryStatus: "Ready to query configured time servers.",
            LastError: null,
            Servers: servers
                .Select(server => new ServerSyncState(
                    Endpoint: server,
                    ConsecutiveFailures: 0,
                    State: ServerHealthState.NotUsed,
                    IsDemoted: false,
                    LastAttemptUtc: null,
                    LastSuccessUtc: null,
                    Offset: null,
                    Lag: null,
                    LastError: null))
                .ToList());
    }
}
