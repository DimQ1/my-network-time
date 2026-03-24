using MyNetworkTime.Core.Platforms;

namespace MyNetworkTime.Core.Dashboard;

public sealed record DashboardSnapshot(
    DateTimeOffset CurrentTime,
    DateTimeOffset? LastAttempt,
    DateTimeOffset? LastSync,
    TimeSpan? LastSyncOffset,
    TimeSpan NextAttemptIn,
    string SummaryStatus,
    string ModeLabel,
    string? LastError,
    PlatformCapabilities Platform,
    bool CanAdjustSystemTimeFromDashboard,
    string AdjustActionLabel,
    IReadOnlyList<ServerStatusSnapshot> Servers);
