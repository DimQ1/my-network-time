using MyNetworkTime.Core.Protocols;
using MyNetworkTime.Core.Settings;
using MyNetworkTime.Core.Tests.Support;
using MyNetworkTime.Core.Transports;

namespace MyNetworkTime.Core.Tests;

public sealed class Rfc868ClientTests
{
    [Fact]
    public async Task TcpClient_ParsesOffsetFromResponse()
    {
        var startedAt = new DateTimeOffset(2026, 3, 24, 12, 0, 0, TimeSpan.Zero);
        var receivedAt = startedAt.AddMilliseconds(40);
        var serverTime = startedAt.AddSeconds(1);

        var transport = new FakeTcpTransport((_, _, payload, expectedBytes, _) =>
        {
            Assert.Empty(payload);
            Assert.Equal(4, expectedBytes);
            return new TcpTransportResponse(ToRfc868Bytes(serverTime), receivedAt);
        });

        var client = new Rfc868TcpClient(transport, new ManualTimeProvider(startedAt));
        var result = await client.QueryAsync(new ServerEndpointSettings("time.example", ServerProtocol.Rfc868Tcp, 37));

        Assert.Equal(980d, result.Offset.TotalMilliseconds, 6);
        Assert.Equal(40d, result.RoundTripDelay.TotalMilliseconds, 6);
    }

    [Fact]
    public async Task UdpClient_SendsProbeByteAndParsesOffset()
    {
        var startedAt = new DateTimeOffset(2026, 3, 24, 12, 0, 0, TimeSpan.Zero);
        var receivedAt = startedAt.AddMilliseconds(60);
        var serverTime = startedAt.AddSeconds(1);

        var transport = new FakeUdpTransport((_, _, payload, _) =>
        {
            Assert.Single(payload);
            return new UdpTransportResponse(ToRfc868Bytes(serverTime), receivedAt);
        });

        var client = new Rfc868UdpClient(transport, new ManualTimeProvider(startedAt));
        var result = await client.QueryAsync(new ServerEndpointSettings("time.example", ServerProtocol.Rfc868Udp, 37));

        Assert.Equal(970d, result.Offset.TotalMilliseconds, 6);
        Assert.Equal(60d, result.RoundTripDelay.TotalMilliseconds, 6);
    }

    private static byte[] ToRfc868Bytes(DateTimeOffset timestamp)
    {
        var epoch = new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var seconds = (uint)(timestamp - epoch).TotalSeconds;

        return
        [
            (byte)(seconds >> 24),
            (byte)(seconds >> 16),
            (byte)(seconds >> 8),
            (byte)seconds
        ];
    }
}
