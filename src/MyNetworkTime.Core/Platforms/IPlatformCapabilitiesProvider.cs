namespace MyNetworkTime.Core.Platforms;

public interface IPlatformCapabilitiesProvider
{
    PlatformCapabilities GetCurrentCapabilities();
}
