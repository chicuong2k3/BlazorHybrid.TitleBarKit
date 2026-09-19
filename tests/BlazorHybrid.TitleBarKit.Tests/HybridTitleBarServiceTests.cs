using BlazorHybrid.TitleBarKit.Internal;

namespace BlazorHybrid.TitleBarKit.Tests;

public sealed class HybridTitleBarServiceTests
{
    [Fact]
    public void Toggle_maximize_uses_current_backend_state()
    {
        var backend = new FakeBackend { State = Attached(isMaximized: false) };
        using var service = new HybridTitleBarService(backend);

        service.ToggleMaximize();
        Assert.Equal(1, backend.MaximizeCalls);
        Assert.Equal(0, backend.RestoreCalls);

        backend.SetState(Attached(isMaximized: true));
        service.ToggleMaximize();
        Assert.Equal(1, backend.MaximizeCalls);
        Assert.Equal(1, backend.RestoreCalls);
    }

    [Fact]
    public void Commands_are_forwarded_once()
    {
        var backend = new FakeBackend { State = Attached(false) };
        using var service = new HybridTitleBarService(backend);

        service.BeginDrag();
        service.Minimize();
        service.Maximize();
        service.Restore();
        service.ShowSystemMenu();
        service.Close();

        Assert.Equal((1, 1, 1, 1, 1, 1),
            (backend.DragCalls, backend.MinimizeCalls, backend.MaximizeCalls,
             backend.RestoreCalls, backend.MenuCalls, backend.CloseCalls));
    }

    [Fact]
    public void State_change_is_republished_and_dispose_unsubscribes()
    {
        var backend = new FakeBackend();
        var service = new HybridTitleBarService(backend);
        var observed = new List<TitleBarState>();
        service.StateChanged += (_, state) => observed.Add(state);

        var maximized = Attached(true);
        backend.SetState(maximized);
        Assert.Equal([maximized], observed);

        service.Dispose();
        backend.SetState(Attached(false));
        Assert.Single(observed);
    }

    private static TitleBarState Attached(bool isMaximized) =>
        new(true, isMaximized, false, true, true, true, true);

    private sealed class FakeBackend : IWindowBackend
    {
        public event EventHandler<TitleBarState>? StateChanged;
        public TitleBarState State { get; set; } = TitleBarState.Detached;
        public int DragCalls { get; private set; }
        public int MinimizeCalls { get; private set; }
        public int MaximizeCalls { get; private set; }
        public int RestoreCalls { get; private set; }
        public int MenuCalls { get; private set; }
        public int CloseCalls { get; private set; }
        public void BeginDrag() => DragCalls++;
        public void Minimize() => MinimizeCalls++;
        public void Maximize() => MaximizeCalls++;
        public void Restore() => RestoreCalls++;
        public void ShowSystemMenu() => MenuCalls++;
        public void Close() => CloseCalls++;
        public void SetState(TitleBarState state)
        {
            State = state;
            StateChanged?.Invoke(this, state);
        }
    }
}
