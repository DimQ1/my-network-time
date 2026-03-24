using MyNetworkTime.Core.Dashboard;
using MyNetworkTime.Core.Settings;
using MyNetworkTime.Core.Sync;

namespace MyNetworkTime.Core.Tests;

public sealed class ServerSelectionPolicyTests
{
    [Fact]
    public void OrderServers_PushesDemotedServersToTheEnd()
    {
        var settings = new AppSettingsSnapshot(
            Servers:
            [
                new ServerEndpointSettings("primary", ServerProtocol.Sntp, 123),
                new ServerEndpointSettings("unstable", ServerProtocol.Sntp, 123),
                new ServerEndpointSettings("backup", ServerProtocol.Sntp, 123)
            ],
            UpdateInterval: new TimeInterval(12, IntervalUnit.Hour),
            RetryInterval: new TimeInterval(1, IntervalUnit.Minute),
            DemoteServers: true,
            DemoteAfterFailures: 3,
            AllowPeersToSync: false,
            AlwaysProvideTime: false,
            ShowTrayIconAtLogin: true,
            StartAtBoot: true,
            MaxFreeRun: new TimeInterval(24, IntervalUnit.Hour),
            AdjustmentThreshold: new TimeInterval(2, IntervalUnit.Minute),
            AdjustmentMode: TimeAdjustmentMode.AdjustSystemTime,
            AutoCheckEveryDays: 7,
            LoggingLevel: "Normal");

        var previousState = new SyncStateSnapshot(
            LastAttemptUtc: null,
            LastSyncUtc: null,
            LastSyncOffset: null,
            NextAttemptUtc: null,
            SummaryStatus: "Ready",
            LastError: null,
            Servers:
            [
                new ServerSyncState(settings.Servers[0], 0, ServerHealthState.NotUsed, false, null, null, null, null, null),
                new ServerSyncState(settings.Servers[1], 3, ServerHealthState.Warning, true, null, null, null, null, "demoted"),
                new ServerSyncState(settings.Servers[2], 1, ServerHealthState.Error, false, null, null, null, null, "timeout")
            ]);

        var policy = new ServerSelectionPolicy();

        var ordered = policy.OrderServers(settings, previousState);

        Assert.Equal("primary", ordered[0].Host);
        Assert.Equal("backup", ordered[1].Host);
        Assert.Equal("unstable", ordered[2].Host);
    }

    [Fact]
    public void OrderServers_PreservesConfiguredOrderWhenDemotionIsDisabled()
    {
        var settings = AppSettingsDefaults.Create() with
        {
            DemoteServers = false
        };

        var previousState = new SyncStateSnapshot(
            LastAttemptUtc: null,
            LastSyncUtc: null,
            LastSyncOffset: null,
            NextAttemptUtc: null,
            SummaryStatus: "Ready",
            LastError: null,
            Servers: settings.Servers
                .Select(server => new ServerSyncState(server, 10, ServerHealthState.Warning, true, null, null, null, null, null))
                .ToList());

        var policy = new ServerSelectionPolicy();

        var ordered = policy.OrderServers(settings, previousState);

        Assert.Equal(settings.Servers.Select(server => server.Host), ordered.Select(server => server.Host));
    }
}
