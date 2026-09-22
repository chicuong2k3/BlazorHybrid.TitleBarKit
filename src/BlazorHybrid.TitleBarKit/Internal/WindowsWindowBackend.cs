using Microsoft.UI.Windowing;
using WinUiWindow = Microsoft.UI.Xaml.Window;
using WinRT.Interop;

namespace BlazorHybrid.TitleBarKit.Internal;

internal sealed class WindowsWindowBackend : IWindowBackend, IDisposable
{
    private readonly TitleBarOptions _options;
    private AppWindow? _appWindow;
    private WinUiWindow? _window;
    private nint _windowHandle;

    internal WindowsWindowBackend(TitleBarOptions options) => _options = options;

    public event EventHandler<TitleBarState>? StateChanged;

    public TitleBarState State { get; private set; } = TitleBarState.Detached;

    internal void Attach(WinUiWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);
        Detach();

        _window = window;
        _appWindow = window.AppWindow;
        _windowHandle = WindowNative.GetWindowHandle(window);
        ConfigurePresenter();
        _window.Activated += OnWindowActivated;
        _appWindow.Changed += OnAppWindowChanged;
        PublishState();
    }

    public void BeginDrag()
    {
        var window = _window;
        var windowHandle = _windowHandle;
        if (window is null || windowHandle == 0) return;

        // Do not synchronously enter Windows' modal move loop from a WebView callback.
        // Queue it on the native dispatcher so the callback can return before SendMessage blocks
        // for the duration of the drag operation.
        window.DispatcherQueue.TryEnqueue(() =>
        {
            if (_windowHandle != windowHandle || !NativeMethods.GetCursorPos(out var point)) return;

            // A click that has already been released must not enter the move loop.
            // Otherwise the window sticks to the cursor until the next click.
            if ((NativeMethods.GetAsyncKeyState(NativeMethods.VkLButton) & 0x8000) == 0) return;

            NativeMethods.ReleaseCapture();
            var coordinates = unchecked((nint)((point.X & 0xFFFF) | ((point.Y & 0xFFFF) << 16)));
            NativeMethods.SendMessage(
                windowHandle,
                NativeMethods.WmNcLButtonDown,
                NativeMethods.HtCaption,
                coordinates);
        });
    }

    public void Minimize()
    {
        if (_options.IsMinimizable && Presenter is { } presenter) presenter.Minimize();
    }

    public void Maximize()
    {
        if (_options.IsMaximizable && Presenter is { } presenter) presenter.Maximize();
    }

    public void Restore()
    {
        if (Presenter is { } presenter) presenter.Restore();
    }

    public void ShowSystemMenu()
    {
        if (_windowHandle == 0 || !NativeMethods.GetCursorPos(out var point)) return;
        var menu = NativeMethods.GetSystemMenu(_windowHandle, false);
        if (menu == 0) return;

        var command = NativeMethods.TrackPopupMenuEx(
            menu,
            NativeMethods.TpmRightButton | NativeMethods.TpmReturnCommand,
            point.X,
            point.Y,
            _windowHandle,
            0);
        if (command != 0)
            NativeMethods.PostMessage(_windowHandle, NativeMethods.WmSysCommand, (nint)command, 0);
    }

    public void Close()
    {
        if (_options.IsClosable) _window?.Close();
    }

    public void Dispose() => Detach();

    private OverlappedPresenter? Presenter => _appWindow?.Presenter as OverlappedPresenter;

    private void ConfigurePresenter()
    {
        var window = _window;
        if (window is null || Presenter is not { } presenter) return;

        var queue = window.DispatcherQueue;
        if (!queue.HasThreadAccess)
        {
            queue.TryEnqueue(ConfigurePresenter);
            return;
        }

        try
        {
            // MAUI can put the native caption back after OnWindowCreated, so this
            // runs again on every activation. SetBorderAndTitleBar throws 0x800710DD
            // when the dispatcher queue is not ready, and WinUI turns that into a
            // process-ending stowed exception.
            if (_options.HideNativeTitleBar)
            {
                window.ExtendsContentIntoTitleBar = false;
                window.SetTitleBar(null);
            }

            presenter.IsMinimizable = _options.IsMinimizable;
            presenter.IsMaximizable = _options.IsMaximizable;
            presenter.IsResizable = _options.IsResizable;
            presenter.SetBorderAndTitleBar(
                hasBorder: !_options.HideNativeTitleBar,
                hasTitleBar: !_options.HideNativeTitleBar);

            if (_options.HideNativeTitleBar)
                ApplyNativeWindowStyles();
        }
        catch (Exception)
        {
            // Keep the native caption for this attempt. The next activation retries.
        }
    }

    private void ApplyNativeWindowStyles()
    {
        if (_windowHandle == 0) return;

        var style = NativeMethods.GetWindowLongPtr(_windowHandle, NativeMethods.GwlStyle).ToInt64();
        style |= NativeMethods.WsSysMenu;
        style = _options.IsResizable
            ? style | NativeMethods.WsThickFrame
            : style & ~NativeMethods.WsThickFrame;
        style = _options.IsMinimizable
            ? style | NativeMethods.WsMinimizeBox
            : style & ~NativeMethods.WsMinimizeBox;
        style = _options.IsMaximizable
            ? style | NativeMethods.WsMaximizeBox
            : style & ~NativeMethods.WsMaximizeBox;

        NativeMethods.SetWindowLongPtr(_windowHandle, NativeMethods.GwlStyle, (nint)style);
        NativeMethods.SetWindowPos(
            _windowHandle,
            0,
            0,
            0,
            0,
            0,
            NativeMethods.SwpNoMove |
            NativeMethods.SwpNoSize |
            NativeMethods.SwpNoZOrder |
            NativeMethods.SwpNoActivate |
            NativeMethods.SwpFrameChanged);
    }

    private void OnWindowActivated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
    {
        ConfigurePresenter();
        PublishState();
    }

    private void OnAppWindowChanged(AppWindow sender, AppWindowChangedEventArgs args) => PublishState();

    private void PublishState()
    {
        var presenter = Presenter;
        var nextState = presenter is null
            ? TitleBarState.Detached
            : new TitleBarState(
                true,
                presenter.State == OverlappedPresenterState.Maximized,
                presenter.State == OverlappedPresenterState.Minimized,
                presenter.IsMaximizable,
                presenter.IsMinimizable,
                presenter.IsResizable,
                _options.IsClosable);

        // AppWindow raises Changed continuously while moving or resizing. Re-rendering
        // the WebView title bar for every position update overwhelms its input loop.
        if (nextState == State) return;

        State = nextState;
        StateChanged?.Invoke(this, State);
    }

    private void Detach()
    {
        if (_window is not null) _window.Activated -= OnWindowActivated;
        if (_appWindow is not null) _appWindow.Changed -= OnAppWindowChanged;
        _appWindow = null;
        _window = null;
        _windowHandle = 0;
        State = TitleBarState.Detached;
    }
}
