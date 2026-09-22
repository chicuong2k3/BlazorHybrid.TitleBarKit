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

    // Kept for source compatibility. Native caption regions now own the entire gesture;
    // entering a nested SendMessage move loop from a WebView callback can deadlock it.
    public void BeginDrag() { }

    public void CancelPendingDrag() { }

    public void SetCaptionRegion(int x, int y, int width, int height)
    {
        var window = _window;
        if (window is null) return;
        void Update()
        {
            if (_window != window || _appWindow is null) return;
            // MAUI's initial template layout can reset PreferredHeightOption after
            // OnWindowCreated. Reassert our mode once the Razor title row is laid out.
            if (_options.HideNativeTitleBar)
                _appWindow.TitleBar.PreferredHeightOption = _options.UseNativeWindowControls
                    ? TitleBarHeightOption.Standard : TitleBarHeightOption.Collapsed;
            var regions = width > 0 && height > 0
                ? new[] { new Windows.Graphics.RectInt32(x, y, width, height) }
                : [];
            if (_options.UseNativeWindowControls)
                _appWindow.TitleBar.SetDragRectangles(regions);
            else
                Microsoft.UI.Input.InputNonClientPointerSource.GetForWindowId(_appWindow.Id)
                    .SetRegionRects(Microsoft.UI.Input.NonClientRegionKind.Caption, regions);
        }
        if (window.DispatcherQueue.HasThreadAccess) Update();
        else window.DispatcherQueue.TryEnqueue(Update);
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
            // Extend content into the OS caption rather than removing WS_CAPTION.
            // Keeping native non-client behavior preserves snap and drag-to-restore.
            if (_options.HideNativeTitleBar)
            {
                window.ExtendsContentIntoTitleBar = true;
                window.SetTitleBar(null);
                window.AppWindow.TitleBar.PreferredHeightOption = _options.UseNativeWindowControls
                    ? TitleBarHeightOption.Standard
                    : TitleBarHeightOption.Collapsed;
            }

            presenter.IsMinimizable = _options.IsMinimizable;
            presenter.IsMaximizable = _options.IsMaximizable;
            presenter.IsResizable = _options.IsResizable;
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
        // Preserve WS_CAPTION: extending content hides its background, while the OS
        // still supplies native non-client gestures and caption buttons. Removing it
        // breaks native dragging even when the title bar has a drag rectangle.
        style |= NativeMethods.WsCaption;
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
        // Reconfiguring OverlappedPresenter during activation is unsafe on Windows App
        // SDK 1.7 and used to crash as soon as the custom title bar was clicked.
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
                _options.IsClosable)
            {
                UsesNativeControls = _options.HideNativeTitleBar && _options.UseNativeWindowControls,
                CaptionButtonsWidth = Math.Max(138, (_appWindow?.TitleBar.RightInset ?? 0) /
                    (_window?.Content?.XamlRoot?.RasterizationScale ?? 1)),
            };

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
