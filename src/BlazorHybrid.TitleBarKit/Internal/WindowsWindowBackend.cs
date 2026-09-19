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
        _appWindow.Changed += OnAppWindowChanged;
        PublishState();
    }

    public void BeginDrag()
    {
        if (_windowHandle == 0) return;
        NativeMethods.ReleaseCapture();
        NativeMethods.SendMessage(_windowHandle, NativeMethods.WmNcLButtonDown, NativeMethods.HtCaption, 0);
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
        if (Presenter is not { } presenter) return;
        presenter.IsMinimizable = _options.IsMinimizable;
        presenter.IsMaximizable = _options.IsMaximizable;
        presenter.IsResizable = _options.IsResizable;
        presenter.SetBorderAndTitleBar(hasBorder: true, hasTitleBar: !_options.HideNativeTitleBar);
    }

    private void OnAppWindowChanged(AppWindow sender, AppWindowChangedEventArgs args) => PublishState();

    private void PublishState()
    {
        var presenter = Presenter;
        State = presenter is null
            ? TitleBarState.Detached
            : new TitleBarState(
                true,
                presenter.State == OverlappedPresenterState.Maximized,
                presenter.State == OverlappedPresenterState.Minimized,
                presenter.IsMaximizable,
                presenter.IsMinimizable,
                presenter.IsResizable,
                _options.IsClosable);
        StateChanged?.Invoke(this, State);
    }

    private void Detach()
    {
        if (_appWindow is not null) _appWindow.Changed -= OnAppWindowChanged;
        _appWindow = null;
        _window = null;
        _windowHandle = 0;
        State = TitleBarState.Detached;
    }
}
