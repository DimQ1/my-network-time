using MyNetworkTime.Core.Sync;

namespace MyNetworkTime.Core.Storage;

public sealed class JsonSyncStateRepository(string filePath) : ISyncStateRepository
{
    private readonly JsonFileStore<SyncStateSnapshot> _store = new(filePath);

    public ValueTask<SyncStateSnapshot?> GetAsync(CancellationToken cancellationToken = default) =>
        _store.ReadAsync(cancellationToken);

    public ValueTask SaveAsync(SyncStateSnapshot state, CancellationToken cancellationToken = default) =>
        _store.WriteAsync(state, cancellationToken);
}
