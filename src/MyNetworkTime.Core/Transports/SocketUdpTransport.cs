using System.Net.Sockets;

namespace MyNetworkTime.Core.Transports;

public sealed class SocketUdpTransport(TimeProvider timeProvider) : IUdpTransport
{
    public async ValueTask<UdpTransportResponse> SendAndReceiveAsync(
        string host,
        int port,
        ReadOnlyMemory<byte> payload,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCts.CancelAfter(timeout);

        using var client = new UdpClient();
        client.Connect(host, port);

        await client.SendAsync(payload, linkedCts.Token);
        var response = await client.ReceiveAsync(linkedCts.Token);

        return new UdpTransportResponse(response.Buffer, timeProvider.GetUtcNow());
    }
}
