using MyNetworkTime.Core.Dashboard;
using MyNetworkTime.Core.Settings;

namespace MyNetworkTime.Core.Sync;

public sealed record ServerSyncState(
    ServerEndpointSettings Endpoint,
    int ConsecutiveFailures,
    ServerHealthState State,
    bool IsDemoted,
    DateTimeOffset? LastAttemptUtc,
    DateTimeOffset? LastSuccessUtc,
    TimeSpan? Offset,
    TimeSpan? Lag,
    string? LastError);
