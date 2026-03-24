using System.Runtime.InteropServices;
using MyNetworkTime.Core.Storage;

namespace MyNetworkTime.App.Services;

internal sealed class WindowsTrayBehaviorService(ISettingsRepository settingsRepository) : IDisposable
{
#if WINDOWS
    private global::Microsoft.UI.Xaml.Window? _nativeWindow;
    private global::Microsoft.UI.Windowing.AppWindow? _appWindow;
    private volatile bool _collapseToTrayOnMinimize;
    private bool _isAttached;
    private bool _trayIconVisible;
    private IntPtr _hwnd;
    private SUBCLASSPROC? _subclassProc;

    // Shell_NotifyIcon constants
    private const uint NIM_ADD = 0x00000000;
    private const uint NIM_DELETE = 0x00000002;
    private const uint NIF_MESSAGE = 0x00000001;
    private const uint NIF_ICON = 0x00000002;
    private const uint NIF_TIP = 0x00000004;
    private const uint WM_TRAYICON = 0x8001; // WM_APP + 1
    private const uint WM_LBUTTONUP = 0x0202;
    private const int IDI_APPLICATION = 32512;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATAW
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szTip;
    }

    private delegate IntPtr SUBCLASSPROC(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, nuint uIdSubclass, nuint dwRefData);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool Shell_NotifyIconW(uint dwMessage, ref NOTIFYICONDATAW lpData);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern bool SetWindowSubclass(IntPtr hwnd, SUBCLASSPROC pfnSubclass, nuint uIdSubclass, nuint dwRefData);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern bool RemoveWindowSubclass(IntPtr hwnd, SUBCLASSPROC pfnSubclass, nuint uIdSubclass);

    [DllImport("comctl32.dll")]
    private static extern IntPtr DefSubclassProc(IntPtr hwnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    private const uint WM_GETICON = 0x007F;
    private const int ICON_SMALL = 0;
    private const int ICON_SMALL2 = 2;
    private const int GCLP_HICON = -14;
    private const int GCLP_HICONSM = -34;

    private IntPtr GetAppIconHandle()
    {
        // Prefer the small icon the window already has (set by MAUI from appicon.svg)
        var hIcon = SendMessage(_hwnd, WM_GETICON, new IntPtr(ICON_SMALL2), IntPtr.Zero);
        if (hIcon == IntPtr.Zero)
            hIcon = SendMessage(_hwnd, WM_GETICON, new IntPtr(ICON_SMALL), IntPtr.Zero);
        if (hIcon == IntPtr.Zero)
            hIcon = GetClassLongPtr(_hwnd, GCLP_HICONSM);
        if (hIcon == IntPtr.Zero)
            hIcon = GetClassLongPtr(_hwnd, GCLP_HICON);
        if (hIcon == IntPtr.Zero)
            hIcon = LoadIcon(IntPtr.Zero, new IntPtr(IDI_APPLICATION));
        return hIcon;
    }
#endif

    internal void Attach(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

#if WINDOWS
        if (_isAttached)
        {
            return;
        }

        _isAttached = true;
        _ = ConfigureAsync(window);
#endif
    }

    /// <summary>
    /// Re-reads the collapse-to-tray setting from persisted storage.
    /// Call after the user saves settings so the behavior takes effect immediately.
    /// </summary>
    internal async ValueTask RefreshAsync(CancellationToken cancellationToken = default)
    {
#if WINDOWS
        var settings = await settingsRepository.GetAsync(cancellationToken);
        _collapseToTrayOnMinimize = settings.CollapseToTrayOnMinimize;
#else
        await ValueTask.CompletedTask;
#endif
    }

    public void Dispose()
    {
#if WINDOWS
        if (_appWindow is not null)
        {
            _appWindow.Changed -= OnAppWindowChanged;
        }

        RemoveTrayIcon();

        if (_hwnd != IntPtr.Zero && _subclassProc is not null)
        {
            RemoveWindowSubclass(_hwnd, _subclassProc, 1);
            _subclassProc = null;
        }
#endif
    }

#if WINDOWS
    private async Task ConfigureAsync(Window window)
    {
        var settings = await settingsRepository.GetAsync();
        _collapseToTrayOnMinimize = settings.CollapseToTrayOnMinimize;

        window.HandlerChanged += OnWindowHandlerChanged;

        if (window.Handler is not null)
        {
            AttachNativeWindow(window);
        }
    }

    private void OnWindowHandlerChanged(object? sender, EventArgs eventArgs)
    {
        if (sender is Window window)
        {
            AttachNativeWindow(window);
        }
    }

    private void AttachNativeWindow(Window window)
    {
        if (_nativeWindow is not null)
        {
            return;
        }

        if (window.Handler?.PlatformView is not global::Microsoft.UI.Xaml.Window nativeWindow)
        {
            return;
        }

        _nativeWindow = nativeWindow;
        _appWindow = _nativeWindow.AppWindow;
        _hwnd = global::WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);

        _subclassProc = SubclassWindowProc;
        SetWindowSubclass(_hwnd, _subclassProc, 1, 0);

        _appWindow.Changed += OnAppWindowChanged;
    }

    private void OnAppWindowChanged(global::Microsoft.UI.Windowing.AppWindow sender, global::Microsoft.UI.Windowing.AppWindowChangedEventArgs args)
    {
        if (!_collapseToTrayOnMinimize || _trayIconVisible)
        {
            return;
        }

        if (sender.Presenter is global::Microsoft.UI.Windowing.OverlappedPresenter
            {
                State: global::Microsoft.UI.Windowing.OverlappedPresenterState.Minimized
            })
        {
            ShowTrayIcon();
            sender.Hide();
        }
    }

    private IntPtr SubclassWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, nuint uIdSubclass, nuint dwRefData)
    {
        if (uMsg == WM_TRAYICON && (uint)lParam == WM_LBUTTONUP)
        {
            RestoreFromTray();
        }

        return DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }

    private void RestoreFromTray()
    {
        RemoveTrayIcon();
        _appWindow?.Show();

        if (_appWindow?.Presenter is global::Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.Restore();
        }
    }

    private void ShowTrayIcon()
    {
        if (_trayIconVisible)
        {
            return;
        }

        var hIcon = GetAppIconHandle();
        var nid = new NOTIFYICONDATAW
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>(),
            hWnd = _hwnd,
            uID = 1,
            uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
            uCallbackMessage = WM_TRAYICON,
            hIcon = hIcon,
            szTip = "My Network Time"
        };

        _trayIconVisible = Shell_NotifyIconW(NIM_ADD, ref nid);
    }

    private void RemoveTrayIcon()
    {
        if (!_trayIconVisible)
        {
            return;
        }

        var nid = new NOTIFYICONDATAW
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>(),
            hWnd = _hwnd,
            uID = 1,
            szTip = string.Empty
        };

        Shell_NotifyIconW(NIM_DELETE, ref nid);
        _trayIconVisible = false;
    }
#endif
}
