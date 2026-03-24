using MyNetworkTime.Core.Settings;
using MyNetworkTime.Core.Sync;
using MyNetworkTime.Core.Transports;

namespace MyNetworkTime.Core.Protocols;

public sealed class Rfc868TcpClient(ITcpTransport transport, TimeProvider timeProvider) : INetworkTimeProtocolClient
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(2);

    public ServerProtocol Protocol => ServerProtocol.Rfc868Tcp;

    public async ValueTask<NetworkTimeSample> QueryAsync(ServerEndpointSettings endpoint, CancellationToken cancellationToken = default)
    {
        var startedAtUtc = timeProvider.GetUtcNow();
        var response = await transport.SendAndReceiveAsync(endpoint.Host, endpoint.Port, Array.Empty<byte>(), 4, DefaultTimeout, cancellationToken);

        return CreateSample(startedAtUtc, response.Payload, response.ReceivedAtUtc);
    }

    internal static NetworkTimeSample CreateSample(DateTimeOffset startedAtUtc, byte[] payload, DateTimeOffset receivedAtUtc)
    {
        var serverTimeUtc = Rfc868TimeConverter.ReadTimestamp(payload);
        var roundTrip = receivedAtUtc - startedAtUtc;
        if (roundTrip < TimeSpan.Zero)
        {
            roundTrip = TimeSpan.Zero;
        }

        var midpoint = startedAtUtc + TimeSpan.FromTicks(roundTrip.Ticks / 2);
        var offset = serverTimeUtc - midpoint;

        return new NetworkTimeSample(
            ServerTimeUtc: serverTimeUtc,
            ObservedAtUtc: receivedAtUtc,
            Offset: offset,
            RoundTripDelay: roundTrip);
    }
}
