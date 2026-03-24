namespace MyNetworkTime.Core.Transports;

public sealed record TcpTransportResponse(byte[] Payload, DateTimeOffset ReceivedAtUtc);
