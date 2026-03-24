namespace MyNetworkTime.Core.Transports;

public interface IUdpTransport
{
    ValueTask<UdpTransportResponse> SendAndReceiveAsync(
        string host,
        int port,
        ReadOnlyMemory<byte> payload,
        TimeSpan timeout,
        CancellationToken cancellationToken = default);
}
