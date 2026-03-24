using Microsoft.Extensions.DependencyInjection;
using MyNetworkTime.App.Services;
using MyNetworkTime.Core.Services;

namespace MyNetworkTime.App;

public static class AppServiceCollectionExtensions
{
    public static IServiceCollection AddMyNetworkTimeApp(this IServiceCollection services)
    {
        services.AddSingleton<INetworkTimeWorkspaceService, DesignNetworkTimeWorkspaceService>();

        return services;
    }
}
