namespace MyNetworkTime.Core.Platforms;

public interface IAppLifecycleSyncService
{
    void Start();

    ValueTask CheckNowAsync(CancellationToken cancellationToken = default);
}
