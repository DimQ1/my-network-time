using MyNetworkTime.App.Services;
using MyNetworkTime.Core.Platforms;

namespace MyNetworkTime.App;

public partial class App : Application
{
    private readonly IAppLifecycleSyncService _lifecycleSyncService;

    public App(IAppLifecycleSyncService lifecycleSyncService)
    {
        _lifecycleSyncService = lifecycleSyncService;
        InitializeComponent();
        _lifecycleSyncService.Start();
        _ = _lifecycleSyncService.CheckNowAsync();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var trayService = activationState!.Context.Services.GetRequiredService<WindowsTrayBehaviorService>();
        var window = new Window(new MainPage()) { Title = "MyNetworkTime.App" };
        trayService.Attach(window);
        return window;
    }
}
