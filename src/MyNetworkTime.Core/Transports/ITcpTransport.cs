namespace MyNetworkTime.Core.Transports;

public interface ITcpTransport
{
    ValueTask<TcpTransportResponse> SendAndReceiveAsync(
        string host,
        int port,
        ReadOnlyMemory<byte> payload,
        int expectedBytes,
        TimeSpan timeout,
        CancellationToken cancellationToken = default);
}
