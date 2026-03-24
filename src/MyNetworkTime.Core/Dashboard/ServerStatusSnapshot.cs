using MyNetworkTime.Core.Settings;

namespace MyNetworkTime.Core.Dashboard;

public sealed record ServerStatusSnapshot(
    string Name,
    ServerProtocol Protocol,
    ServerHealthState State,
    TimeSpan? Offset,
    TimeSpan? Lag,
    string? LastError);
