using BlazorHybrid.TitleBarKit.Internal;

namespace BlazorHybrid.TitleBarKit;

internal sealed class HybridTitleBarService : IHybridTitleBarService, IDisposable
{
    private readonly IWindowBackend _backend;

    public HybridTitleBarService(IWindowBackend backend)
    {
        _backend = backend;
        _backend.StateChanged += OnBackendStateChanged;
    }

    public event EventHandler<TitleBarState>? StateChanged;

    public TitleBarState State => _backend.State;

    public void BeginDrag() => _backend.BeginDrag();
    public void SetCaptionRegion(int x, int y, int width, int height) =>
        _backend.SetCaptionRegion(x, y, width, height);
    public void CancelPendingDrag() => _backend.CancelPendingDrag();
    public void Minimize() => _backend.Minimize();
    public void Maximize() => _backend.Maximize();
    public void Restore() => _backend.Restore();
    public void ShowSystemMenu() => _backend.ShowSystemMenu();
    public void Close() => _backend.Close();

    public void ToggleMaximize()
    {
        if (State.IsMaximized)
            Restore();
        else
            Maximize();
    }

    public void Dispose() => _backend.StateChanged -= OnBackendStateChanged;

    private void OnBackendStateChanged(object? sender, TitleBarState state) => StateChanged?.Invoke(this, state);
}
