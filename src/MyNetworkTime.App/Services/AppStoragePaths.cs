using Microsoft.Maui.Storage;

namespace MyNetworkTime.App.Services;

internal static class AppStoragePaths
{
    private static string RootDirectory => Path.Combine(FileSystem.AppDataDirectory, "MyNetworkTime");

    public static string SettingsFilePath => Path.Combine(RootDirectory, "settings.json");

    public static string LogsFilePath => Path.Combine(RootDirectory, "logs.json");

    public static string SyncStateFilePath => Path.Combine(RootDirectory, "sync-state.json");
}
