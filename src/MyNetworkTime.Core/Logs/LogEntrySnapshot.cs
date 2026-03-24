namespace MyNetworkTime.Core.Logs;

public sealed record LogEntrySnapshot(
    DateTimeOffset Timestamp,
    string Message,
    string Context);
