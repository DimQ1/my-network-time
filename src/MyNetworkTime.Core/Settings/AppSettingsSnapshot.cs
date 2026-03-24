namespace MyNetworkTime.Core.Settings;

public sealed record AppSettingsSnapshot(
    IReadOnlyList<ServerEndpointSettings> Servers,
    TimeInterval UpdateInterval,
    TimeInterval RetryInterval,
    bool DemoteServers,
    int DemoteAfterFailures,
    bool AllowPeersToSync,
    bool AlwaysProvideTime,
    bool ShowTrayIconAtLogin,
    bool StartAtBoot,
    TimeInterval MaxFreeRun,
    TimeInterval AdjustmentThreshold,
    TimeAdjustmentMode AdjustmentMode,
    int AutoCheckEveryDays,
    string LoggingLevel);
