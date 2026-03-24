namespace MyNetworkTime.Core.Settings;

public sealed record ServerEndpointSettings(
    string Host,
    ServerProtocol Protocol,
    int Port);
