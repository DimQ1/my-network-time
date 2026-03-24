namespace MyNetworkTime.Core.Sync;

public enum SyncTrigger
{
    InitialStartup = 0,
    ManualUpdate = 1,
    BackgroundRefresh = 2,
    Retry = 3
}
