using MyNetworkTime.Core.Logs;
using MyNetworkTime.Core.Settings;
using MyNetworkTime.Core.Storage;

namespace MyNetworkTime.Core.Tests;

public sealed class JsonRepositoryTests : IDisposable
{
    private readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), $"mynetworktime-tests-{Guid.NewGuid():N}");

    [Fact]
    public async Task SettingsRepository_CreatesDefaultsAndPersistsUpdates()
    {
        Directory.CreateDirectory(_tempDirectory);
        var filePath = Path.Combine(_tempDirectory, "settings.json");
        var repository = new JsonSettingsRepository(filePath);

        var settings = await repository.GetAsync();
        var updated = settings with
        {
            RetryInterval = new TimeInterval(2, IntervalUnit.Minute),
            LoggingLevel = "Verbose"
        };

        await repository.SaveAsync(updated);
        var reloaded = await repository.GetAsync();

        Assert.True(File.Exists(filePath));
        Assert.Equal(5, settings.Servers.Count);
        Assert.Equal("Verbose", reloaded.LoggingLevel);
        Assert.Equal(2, reloaded.RetryInterval.Value);
    }

    [Fact]
    public async Task LogRepository_ReturnsNewestEntriesFirstAndTrimsHistory()
    {
        Directory.CreateDirectory(_tempDirectory);
        var repository = new JsonLogRepository(Path.Combine(_tempDirectory, "logs.json"), maxEntries: 2);

        await repository.AppendAsync(
        [
            new LogEntrySnapshot(new DateTimeOffset(2026, 3, 24, 12, 0, 0, TimeSpan.Zero), "A", "ctx"),
            new LogEntrySnapshot(new DateTimeOffset(2026, 3, 24, 12, 1, 0, TimeSpan.Zero), "B", "ctx"),
            new LogEntrySnapshot(new DateTimeOffset(2026, 3, 24, 12, 2, 0, TimeSpan.Zero), "C", "ctx")
        ]);

        var logs = await repository.GetRecentAsync();

        Assert.Collection(
            logs,
            entry => Assert.Equal("C", entry.Message),
            entry => Assert.Equal("B", entry.Message));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
