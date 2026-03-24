using Microsoft.Extensions.DependencyInjection;
using MyNetworkTime.App.Services;
using MyNetworkTime.Core.Platforms;
using MyNetworkTime.Core.Protocols;
using MyNetworkTime.Core.Services;
using MyNetworkTime.Core.Storage;
using MyNetworkTime.Core.Sync;
using MyNetworkTime.Core.Transports;

namespace MyNetworkTime.App;

public static class AppServiceCollectionExtensions
{
    public static IServiceCollection AddMyNetworkTimeApp(this IServiceCollection services)
    {
        services.AddSingleton<TimeProvider>(TimeProvider.System);

        services.AddSingleton<ITimeAdjustmentService, PlatformTimeAdjustmentService>();
        services.AddSingleton<IPermissionGuidanceService, PlatformPermissionGuidanceService>();
        services.AddSingleton<IPlatformCapabilitiesProvider, DevicePlatformCapabilitiesProvider>();
        services.AddSingleton<IAppLifecycleSyncService, AppLifecycleSyncService>();
        services.AddSingleton<DashboardRefreshNotifier>();
        services.AddSingleton<ISettingsRepository>(_ => new JsonSettingsRepository(AppStoragePaths.SettingsFilePath));
        services.AddSingleton<ILogRepository>(_ => new JsonLogRepository(AppStoragePaths.LogsFilePath));
        services.AddSingleton<ISyncStateRepository>(_ => new JsonSyncStateRepository(AppStoragePaths.SyncStateFilePath));

        services.AddSingleton<IUdpTransport, SocketUdpTransport>();
        services.AddSingleton<ITcpTransport, SocketTcpTransport>();

        services.AddSingleton<INetworkTimeProtocolClient, SntpClient>();
        services.AddSingleton<INetworkTimeProtocolClient, Rfc868TcpClient>();
        services.AddSingleton<INetworkTimeProtocolClient, Rfc868UdpClient>();
        services.AddSingleton<NetworkTimeProtocolClientResolver>();

        services.AddSingleton<ServerSelectionPolicy>();
        services.AddSingleton<SyncCoordinator>();
        services.AddSingleton<INetworkTimeWorkspaceService, NetworkTimeWorkspaceService>();

        return services;
    }
}
