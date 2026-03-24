using MyNetworkTime.Core.Settings;

namespace MyNetworkTime.Core.Sync;

public sealed class ServerSelectionPolicy
{
    public IReadOnlyList<ServerEndpointSettings> OrderServers(AppSettingsSnapshot settings, SyncStateSnapshot previousState)
    {
        var stateByServer = previousState.Servers.ToDictionary(server => server.Endpoint);

        return settings.Servers
            .Select((server, index) =>
            {
                stateByServer.TryGetValue(server, out var state);
                var failures = state?.ConsecutiveFailures ?? 0;
                var demoted = settings.DemoteServers && failures >= settings.DemoteAfterFailures;

                return new OrderedServer(server, index, failures, demoted);
            })
            .OrderBy(candidate => candidate.Demoted)
            .ThenBy(candidate => candidate.Failures)
            .ThenBy(candidate => candidate.Index)
            .Select(candidate => candidate.Server)
            .ToList();
    }

    private sealed record OrderedServer(ServerEndpointSettings Server, int Index, int Failures, bool Demoted);
}
