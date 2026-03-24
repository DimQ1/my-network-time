using MyNetworkTime.Core.Settings;

namespace MyNetworkTime.Core.Protocols;

public sealed class NetworkTimeProtocolClientResolver(IEnumerable<INetworkTimeProtocolClient> clients)
{
    private readonly IReadOnlyDictionary<ServerProtocol, INetworkTimeProtocolClient> _clients =
        clients.ToDictionary(client => client.Protocol);

    public INetworkTimeProtocolClient Resolve(ServerProtocol protocol)
    {
        if (_clients.TryGetValue(protocol, out var client))
        {
            return client;
        }

        throw new InvalidOperationException($"No network time client is registered for protocol '{protocol}'.");
    }
}
