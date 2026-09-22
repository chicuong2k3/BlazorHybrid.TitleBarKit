using BlazorHybrid.TitleBarKit.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace BlazorHybrid.TitleBarKit;

/// <summary>Registration helpers for .NET MAUI Blazor Hybrid hosts.</summary>
public static class TitleBarKitExtensions
{
    /// <summary>Adds the title-bar service and attaches it to each Windows MAUI window.</summary>
    public static MauiAppBuilder UseBlazorHybridTitleBar(
        this MauiAppBuilder builder,
        Action<TitleBarOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new TitleBarOptions();
        configure?.Invoke(options);

        // MAUI builds its own XAML title row after OnWindowCreated. Reserve zero
        // space for it; HybridTitleBar owns the visible title row inside the WebView.
        Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping("TitleBarKit", (handler, window) =>
        {
            if (options.HideNativeTitleBar && window is Microsoft.Maui.Controls.Window mauiWindow)
                mauiWindow.TitleBar = new Microsoft.Maui.Controls.TitleBar
                {
                    HeightRequest = 0,
                    MinimumHeightRequest = 0,
                    IsVisible = false,
                };
        });

        var backend = new WindowsWindowBackend(options);
        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton(backend);
        builder.Services.AddSingleton<IWindowBackend>(backend);
        builder.Services.AddSingleton<IHybridTitleBarService, HybridTitleBarService>();
        builder.ConfigureLifecycleEvents(events => events.AddWindows(windows =>
            windows.OnWindowCreated(backend.Attach)));

        return builder;
    }
}
