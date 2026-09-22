using Bunit;
using Microsoft.Extensions.DependencyInjection;
using PointerEventArgs = Microsoft.AspNetCore.Components.Web.PointerEventArgs;

namespace BlazorHybrid.TitleBarKit.Tests;

public sealed class HybridTitleBarComponentTests
{
    [Fact]
    public void Renders_accessible_window_controls_and_forwards_clicks()
    {
        using var context = new BunitContext();
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
    public void Drag_starts_only_after_the_pointer_moves_while_held()
    {
        using var context = new BunitContext();
        var service = new FakeTitleBarService();
        context.Services.AddSingleton<IHybridTitleBarService>(service);
        var cut = context.Render<HybridTitleBar>();
        var brand = cut.Find(".bh-titlebar__brand");

        brand.TriggerEvent("onpointerdown", new PointerEventArgs { Button = 0, Buttons = 1, ClientX = 10, ClientY = 8 });
        brand.TriggerEvent("onpointermove", new PointerEventArgs { Buttons = 1, ClientX = 12, ClientY = 9 });
        Assert.Equal(0, service.DragCalls);

        brand.TriggerEvent("onpointerup", new PointerEventArgs { Button = 0, ClientX = 12, ClientY = 9 });
        brand.TriggerEvent("onpointermove", new PointerEventArgs { Buttons = 1, ClientX = 40, ClientY = 8 });
        Assert.Equal(0, service.DragCalls);

        brand.TriggerEvent("onpointerdown", new PointerEventArgs { Button = 0, Buttons = 1, ClientX = 10, ClientY = 8 });
        brand.TriggerEvent("onpointermove", new PointerEventArgs { Buttons = 1, ClientX = 20, ClientY = 8 });
        Assert.Equal(1, service.DragCalls);
    }

    [Fact]
    public void Reflects_maximized_state_and_disabled_capabilities()
    {
        using var context = new BunitContext();
        var service = new FakeTitleBarService { State = new TitleBarState(true, true, false, false, false, false, true) };
        context.Services.AddSingleton<IHybridTitleBarService>(service);

        var cut = context.Render<HybridTitleBar>();

        Assert.True(cut.Find("button[aria-label='Minimize window']").HasAttribute("disabled"));
        Assert.True(cut.Find("button[aria-label='Restore window']").HasAttribute("disabled"));
        Assert.False(cut.Find("button[aria-label='Close window']").HasAttribute("disabled"));
    }

    private sealed class FakeTitleBarService : IHybridTitleBarService
    {
        public event EventHandler<TitleBarState>? StateChanged;
        public TitleBarState State { get; set; } = TitleBarState.Detached;
        public int MinimizeCalls { get; private set; }
        public int ToggleCalls { get; private set; }
        public int CloseCalls { get; private set; }
        public int DragCalls { get; private set; }
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
