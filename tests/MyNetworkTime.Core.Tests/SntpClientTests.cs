using MyNetworkTime.Core.Protocols;
using MyNetworkTime.Core.Settings;
using MyNetworkTime.Core.Tests.Support;
using MyNetworkTime.Core.Transports;

namespace MyNetworkTime.Core.Tests;

public sealed class SntpClientTests
{
    [Fact]
    public async Task QueryAsync_ParsesOffsetAndRoundTripFromSntpPacket()
    {
        var sentAt = new DateTimeOffset(2026, 3, 24, 12, 0, 0, TimeSpan.Zero);
        var receivedAt = sentAt.AddMilliseconds(25);
        var serverReceive = sentAt.AddMilliseconds(60);
        var serverTransmit = sentAt.AddMilliseconds(65);

        var timeProvider = new ManualTimeProvider(sentAt);
        var transport = new FakeUdpTransport((_, _, _, _) =>
        {
            var packet = new byte[48];
            packet[0] = 0x24;
            WriteNtpTimestamp(packet, 32, serverReceive);
            WriteNtpTimestamp(packet, 40, serverTransmit);

            return new UdpTransportResponse(packet, receivedAt);
        });

        var client = new SntpClient(transport, timeProvider);
        var endpoint = new ServerEndpointSettings("time.example", ServerProtocol.Sntp, 123);

        var result = await client.QueryAsync(endpoint);

        Assert.NotNull(transport.LastPayload);
        Assert.Equal(48, transport.LastPayload!.Length);
        Assert.Equal((byte)0x1B, transport.LastPayload[0]);
        Assert.InRange(result.Offset.TotalMilliseconds, 48d, 50d);
        Assert.InRange(result.RoundTripDelay.TotalMilliseconds, 20d, 23d);
    }

    private static void WriteNtpTimestamp(byte[] buffer, int offset, DateTimeOffset timestamp)
    {
        var epoch = new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var totalSeconds = (timestamp - epoch).TotalSeconds;
        var seconds = (uint)Math.Floor(totalSeconds);
        var fraction = (uint)Math.Round((totalSeconds - seconds) * 0x1_0000_0000d);

        buffer[offset] = (byte)(seconds >> 24);
        buffer[offset + 1] = (byte)(seconds >> 16);
        buffer[offset + 2] = (byte)(seconds >> 8);
        buffer[offset + 3] = (byte)seconds;
        buffer[offset + 4] = (byte)(fraction >> 24);
        buffer[offset + 5] = (byte)(fraction >> 16);
        buffer[offset + 6] = (byte)(fraction >> 8);
        buffer[offset + 7] = (byte)fraction;
    }
}
