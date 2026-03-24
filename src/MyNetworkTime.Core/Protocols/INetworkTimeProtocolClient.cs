using MyNetworkTime.Core.Settings;
using MyNetworkTime.Core.Sync;

namespace MyNetworkTime.Core.Protocols;

public interface INetworkTimeProtocolClient
{
    ServerProtocol Protocol { get; }

    ValueTask<NetworkTimeSample> QueryAsync(ServerEndpointSettings endpoint, CancellationToken cancellationToken = default);
}
