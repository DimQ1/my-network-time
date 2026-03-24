using MyNetworkTime.Core.Settings;
using MyNetworkTime.Core.Sync;
using MyNetworkTime.Core.Transports;

namespace MyNetworkTime.Core.Protocols;

public sealed class Rfc868UdpClient(IUdpTransport transport, TimeProvider timeProvider) : INetworkTimeProtocolClient
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(2);

    public ServerProtocol Protocol => ServerProtocol.Rfc868Udp;

    public async ValueTask<NetworkTimeSample> QueryAsync(ServerEndpointSettings endpoint, CancellationToken cancellationToken = default)
    {
        var startedAtUtc = timeProvider.GetUtcNow();
        var response = await transport.SendAndReceiveAsync(endpoint.Host, endpoint.Port, new byte[] { 0x00 }, DefaultTimeout, cancellationToken);

        return Rfc868TcpClient.CreateSample(startedAtUtc, response.Payload, response.ReceivedAtUtc);
    }
}
