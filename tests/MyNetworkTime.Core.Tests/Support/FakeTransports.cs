using MyNetworkTime.Core.Transports;

namespace MyNetworkTime.Core.Tests.Support;

internal sealed class FakeUdpTransport(Func<string, int, byte[], TimeSpan, UdpTransportResponse> handler) : IUdpTransport
{
    public byte[]? LastPayload { get; private set; }

    public ValueTask<UdpTransportResponse> SendAndReceiveAsync(
        string host,
        int port,
        ReadOnlyMemory<byte> payload,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        LastPayload = payload.ToArray();
        return ValueTask.FromResult(handler(host, port, LastPayload, timeout));
    }
}

internal sealed class FakeTcpTransport(Func<string, int, byte[], int, TimeSpan, TcpTransportResponse> handler) : ITcpTransport
{
    public byte[]? LastPayload { get; private set; }

    public ValueTask<TcpTransportResponse> SendAndReceiveAsync(
        string host,
        int port,
        ReadOnlyMemory<byte> payload,
        int expectedBytes,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        LastPayload = payload.ToArray();
        return ValueTask.FromResult(handler(host, port, LastPayload, expectedBytes, timeout));
    }
}
