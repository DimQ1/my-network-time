using MyNetworkTime.Core.Sync;

namespace MyNetworkTime.Core.Storage;

public interface ISyncStateRepository
{
    ValueTask<SyncStateSnapshot?> GetAsync(CancellationToken cancellationToken = default);

    ValueTask SaveAsync(SyncStateSnapshot state, CancellationToken cancellationToken = default);
}
