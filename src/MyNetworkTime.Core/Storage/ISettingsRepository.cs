using MyNetworkTime.Core.Settings;

namespace MyNetworkTime.Core.Storage;

public interface ISettingsRepository
{
    ValueTask<AppSettingsSnapshot> GetAsync(CancellationToken cancellationToken = default);

    ValueTask SaveAsync(AppSettingsSnapshot settings, CancellationToken cancellationToken = default);
}
