using Microsoft.Extensions.Logging;
using MyNetworkTime.Core.Platforms;
using MyNetworkTime.Core.Storage;
using MyNetworkTime.Core.Sync;

namespace MyNetworkTime.App.Services;

internal sealed class AppLifecycleSyncService(
    ISyncStateRepository syncStateRepository,
    SyncCoordinator syncCoordinator,
    DashboardRefreshNotifier dashboardRefreshNotifier,
    TimeProvider timeProvider,
    ILogger<AppLifecycleSyncService> logger) : IAppLifecycleSyncService
{
    private readonly SemaphoreSlim _refreshGate = new(1, 1);
    private readonly Lock _startGate = new();
    private Task? _loopTask;
    private CancellationTokenSource? _loopCancellationTokenSource;

    public void Start()
    {
        lock (_startGate)
        {
            if (_loopTask is not null)
            {
                return;
            }

            _loopCancellationTokenSource = new CancellationTokenSource();
            _loopTask = RunLoopAsync(_loopCancellationTokenSource.Token);
        }

        _ = CheckNowAsync();
    }

    public ValueTask CheckNowAsync(CancellationToken cancellationToken = default) => new(RunScheduledRefreshAsync(cancellationToken));

    private async Task RunLoopAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await RunScheduledRefreshAsync(cancellationToken);
        }
    }

    private async Task RunScheduledRefreshAsync(CancellationToken cancellationToken)
    {
        if (!await _refreshGate.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            var refreshed = false;
            var state = await syncStateRepository.GetAsync(cancellationToken);
            if (state is null || state.LastAttemptUtc is null)
            {
                await syncCoordinator.RefreshAsync(SyncTrigger.InitialStartup, cancellationToken);
                refreshed = true;
            }
            else
            {
                var now = timeProvider.GetUtcNow();
                if (state.NextAttemptUtc is null || state.NextAttemptUtc <= now)
                {
                    var trigger = state.SummaryStatus.Contains("failed", StringComparison.OrdinalIgnoreCase)
                        ? SyncTrigger.Retry
                        : SyncTrigger.BackgroundRefresh;

                    await syncCoordinator.RefreshAsync(trigger, cancellationToken);
                    refreshed = true;
                }
            }

            if (refreshed)
            {
                await dashboardRefreshNotifier.NotifyAsync();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Scheduled sync loop failed.");
        }
        finally
        {
            _refreshGate.Release();
        }
    }
}
