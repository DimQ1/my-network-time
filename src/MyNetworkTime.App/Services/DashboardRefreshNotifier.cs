namespace MyNetworkTime.App.Services;

internal sealed class DashboardRefreshNotifier
{
    public event Func<ValueTask>? RefreshRequested;

    public async ValueTask NotifyAsync()
    {
        var handlers = RefreshRequested;
        if (handlers is null)
        {
            return;
        }

        foreach (var handler in handlers.GetInvocationList())
        {
            if (handler is Func<ValueTask> refreshRequested)
            {
                await refreshRequested();
            }
        }
    }
}
