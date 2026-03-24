using MyNetworkTime.Core.Logs;

namespace MyNetworkTime.Core.Storage;

public interface ILogRepository
{
    ValueTask<IReadOnlyList<LogEntrySnapshot>> GetRecentAsync(CancellationToken cancellationToken = default);

    ValueTask AppendAsync(IEnumerable<LogEntrySnapshot> entries, CancellationToken cancellationToken = default);
}
