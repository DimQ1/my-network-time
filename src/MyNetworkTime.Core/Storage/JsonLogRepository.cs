using MyNetworkTime.Core.Logs;

namespace MyNetworkTime.Core.Storage;

public sealed class JsonLogRepository(string filePath, int maxEntries = 500) : ILogRepository
{
    private readonly JsonFileStore<List<LogEntrySnapshot>> _store = new(filePath);

    public async ValueTask<IReadOnlyList<LogEntrySnapshot>> GetRecentAsync(CancellationToken cancellationToken = default)
    {
        var logs = await _store.ReadAsync(cancellationToken) ?? [];
        return logs
            .OrderByDescending(entry => entry.Timestamp)
            .ToList();
    }

    public async ValueTask AppendAsync(IEnumerable<LogEntrySnapshot> entries, CancellationToken cancellationToken = default)
    {
        var stored = await _store.ReadAsync(cancellationToken) ?? [];
        stored.AddRange(entries);

        var trimmed = stored
            .OrderByDescending(entry => entry.Timestamp)
            .Take(maxEntries)
            .ToList();

        await _store.WriteAsync(trimmed, cancellationToken);
    }
}
