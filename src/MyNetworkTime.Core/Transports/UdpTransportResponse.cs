namespace MyNetworkTime.Core.Transports;

public sealed record UdpTransportResponse(byte[] Payload, DateTimeOffset ReceivedAtUtc);
