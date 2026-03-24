using MyNetworkTime.Core.Settings;

namespace MyNetworkTime.Core.Storage;

public sealed class JsonSettingsRepository(string filePath) : ISettingsRepository
{
    private readonly JsonFileStore<AppSettingsSnapshot> _store = new(filePath);

    public async ValueTask<AppSettingsSnapshot> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _store.ReadAsync(cancellationToken);
        if (settings is not null)
        {
            return settings;
        }

        settings = AppSettingsDefaults.Create();
        await _store.WriteAsync(settings, cancellationToken);

        return settings;
    }

    public ValueTask SaveAsync(AppSettingsSnapshot settings, CancellationToken cancellationToken = default) =>
        _store.WriteAsync(settings, cancellationToken);
}
