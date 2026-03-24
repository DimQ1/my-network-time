using MyNetworkTime.Core.Settings;
using MyNetworkTime.Core.Sync;
using MyNetworkTime.Core.Transports;

namespace MyNetworkTime.Core.Protocols;

public sealed class SntpClient(IUdpTransport transport, TimeProvider timeProvider) : INetworkTimeProtocolClient
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(2);

    public ServerProtocol Protocol => ServerProtocol.Sntp;

    public async ValueTask<NetworkTimeSample> QueryAsync(ServerEndpointSettings endpoint, CancellationToken cancellationToken = default)
    {
        var sentAtUtc = timeProvider.GetUtcNow();
        var request = BuildRequest(sentAtUtc);
        var response = await transport.SendAndReceiveAsync(endpoint.Host, endpoint.Port, request, DefaultTimeout, cancellationToken);

        if (response.Payload.Length < 48)
        {
            throw new InvalidOperationException("SNTP response must contain at least 48 bytes.");
        }

        var mode = response.Payload[0] & 0b111;
        if (mode is not 4 and not 5)
        {
            throw new InvalidOperationException($"SNTP response mode '{mode}' is not supported.");
        }

        var serverReceiveTimeUtc = NtpTimestampConverter.ReadTimestamp(response.Payload, 32);
        var serverTransmitTimeUtc = NtpTimestampConverter.ReadTimestamp(response.Payload, 40);

        var roundTripDelay = (response.ReceivedAtUtc - sentAtUtc) - (serverTransmitTimeUtc - serverReceiveTimeUtc);
        if (roundTripDelay < TimeSpan.Zero)
        {
            roundTripDelay = TimeSpan.Zero;
        }

        var offset = TimeSpan.FromTicks(((serverReceiveTimeUtc - sentAtUtc).Ticks + (serverTransmitTimeUtc - response.ReceivedAtUtc).Ticks) / 2);

        return new NetworkTimeSample(
            ServerTimeUtc: serverTransmitTimeUtc,
            ObservedAtUtc: response.ReceivedAtUtc,
            Offset: offset,
            RoundTripDelay: roundTripDelay);
    }

    internal static byte[] BuildRequest(DateTimeOffset sentAtUtc)
    {
        var buffer = new byte[48];
        buffer[0] = 0x1B;
        NtpTimestampConverter.WriteTimestamp(buffer, 40, sentAtUtc);
        return buffer;
    }
}
