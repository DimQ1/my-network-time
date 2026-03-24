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
        return new Window(new MainPage()) { Title = "MyNetworkTime.App" };
    }
}
