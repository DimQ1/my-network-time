namespace MyNetworkTime.Core.Settings;

public static class AppSettingsDefaults
{
    public static AppSettingsSnapshot Create()
    {
        return new AppSettingsSnapshot(
            Servers:
            [
                new ServerEndpointSettings("1.nettime.pool.ntp.org", ServerProtocol.Sntp, 123),
                new ServerEndpointSettings("192.168.31.1", ServerProtocol.Sntp, 123),
                new ServerEndpointSettings("2.nettime.pool.ntp.org", ServerProtocol.Sntp, 123),
                new ServerEndpointSettings("3.nettime.pool.ntp.org", ServerProtocol.Sntp, 123),
                new ServerEndpointSettings("0.openwrt.pool.ntp.org", ServerProtocol.Sntp, 123)
            ],
            UpdateInterval: new TimeInterval(12, IntervalUnit.Hour),
            RetryInterval: new TimeInterval(1, IntervalUnit.Minute),
            DemoteServers: true,
            DemoteAfterFailures: 4,
            AllowPeersToSync: false,
            AlwaysProvideTime: false,
            ShowTrayIconAtLogin: true,
            StartAtBoot: true,
            MaxFreeRun: new TimeInterval(24, IntervalUnit.Hour),
            AdjustmentThreshold: new TimeInterval(2, IntervalUnit.Minute),
            AdjustmentMode: TimeAdjustmentMode.AdjustSystemTime,
            AutoCheckEveryDays: 7,
            LoggingLevel: "Normal");
    }
}
