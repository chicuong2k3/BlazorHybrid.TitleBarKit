using Bunit;
using Microsoft.Extensions.DependencyInjection;


namespace BlazorHybrid.TitleBarKit.Tests;

public sealed class HybridTitleBarComponentTests
{
    [Fact]
    public void Renders_accessible_window_controls_and_forwards_clicks()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var service = new FakeTitleBarService
        {
            State = new TitleBarState(true, false, false, true, true, true, true)
        };
        context.Services.AddSingleton<IHybridTitleBarService>(service);

        var cut = context.Render<HybridTitleBar>(parameters => parameters.Add(component => component.Title, "Test application"));

        Assert.Equal("Application title bar", cut.Find("header").GetAttribute("aria-label"));
        Assert.Equal("Test application", cut.Find(".bh-titlebar__title").TextContent.Trim());
        Assert.Equal(3, cut.FindAll("button").Count);
        Assert.All(cut.FindAll("button"), button => Assert.False(button.HasAttribute("disabled")));

        cut.Find("button[aria-label='Minimize window']").Click();
        cut.Find("button[aria-label='Maximize window']").Click();
        cut.Find("button[aria-label='Close window']").Click();

        Assert.Equal((1, 1, 1), (service.MinimizeCalls, service.ToggleCalls, service.CloseCalls));
    }

    [Fact]
    public void Geometry_is_forwarded_without_synthetic_drag_events()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var service = new FakeTitleBarService();
        context.Services.AddSingleton<IHybridTitleBarService>(service);
        var cut = context.Render<HybridTitleBar>();
        cut.Instance.UpdateCaptionRegion(10, 20, 500, 40);
        Assert.Equal((10, 20, 500, 40), service.CaptionRegion);
        Assert.Equal(0, service.DragCalls);
        Assert.DoesNotContain("onpointermove", cut.Markup);
    }

    [Fact]
    public void Reflects_maximized_state_and_disabled_capabilities()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var service = new FakeTitleBarService { State = new TitleBarState(true, true, false, false, false, false, true) };
        context.Services.AddSingleton<IHybridTitleBarService>(service);

        var cut = context.Render<HybridTitleBar>();

        Assert.True(cut.Find("button[aria-label='Minimize window']").HasAttribute("disabled"));
        Assert.True(cut.Find("button[aria-label='Restore window']").HasAttribute("disabled"));
        Assert.False(cut.Find("button[aria-label='Close window']").HasAttribute("disabled"));
    }

    [Fact]
    public void Custom_controls_receive_state_and_replace_default_buttons()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var service = new FakeTitleBarService
        {
            State = new TitleBarState(true, true, false, true, true, true, true)
        };
        context.Services.AddSingleton<IHybridTitleBarService>(service);
        var cut = context.Render<HybridTitleBar>(parameters => parameters
            .Add(p => p.WindowControls, state => builder =>
            {
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "id", "custom-restore");
                builder.AddContent(2, state.IsMaximized ? "Restore custom" : "Maximize custom");
                builder.CloseElement();
            }));
        Assert.Equal("Restore custom", cut.Find("#custom-restore").TextContent);
        Assert.Single(cut.FindAll("button"));
    }

    [Fact]
    public void Native_controls_reserve_space_without_duplicate_html_buttons()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddSingleton<IHybridTitleBarService>(new FakeTitleBarService
        {
            State = new TitleBarState(true, false, false, true, true, true, true)
            {
                UsesNativeControls = true,
                CaptionButtonsWidth = 150
            }
        });
        var cut = context.Render<HybridTitleBar>();
        Assert.Empty(cut.FindAll("button"));
        Assert.Contains("150px", cut.Markup);
    }

    private sealed class FakeTitleBarService : IHybridTitleBarService
    {
        public event EventHandler<TitleBarState>? StateChanged;
        public TitleBarState State { get; set; } = TitleBarState.Detached;
        public int MinimizeCalls { get; private set; }
        public int ToggleCalls { get; private set; }
        public int CloseCalls { get; private set; }
        public int DragCalls { get; private set; }
        public (int, int, int, int) CaptionRegion { get; private set; }
        public void SetCaptionRegion(int x, int y, int width, int height) => CaptionRegion = (x, y, width, height);
        public void BeginDrag() => DragCalls++;
        public void CancelPendingDrag() { }
        public void Minimize() => MinimizeCalls++;
        public void Maximize() { }
        public void Restore() { }
        public void ToggleMaximize() => ToggleCalls++;
        public void ShowSystemMenu() { }
        public void Close() => CloseCalls++;
        public void Publish(TitleBarState state)
        {
            State = state;
            StateChanged?.Invoke(this, state);
        }
    }
}
