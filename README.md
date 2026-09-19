# BlazorHybrid.TitleBarKit

An accessible, customizable Windows title bar built specifically for **.NET MAUI Blazor Hybrid** applications.

The title-bar UI is a Razor component rendered inside `BlazorWebView`; window movement and commands are delegated to the native Windows window manager. The library does not support browser-only Blazor, Blazor Server, WPF, or WinForms hosts.

## Features

- Native drag, minimize, maximize, restore, close, and Windows system-menu behavior
- Double-click the drag region to maximize or restore
- Keep the native resize border while replacing the native caption
- Live maximize/restore icon updates
- Semantic buttons, keyboard focus indicators, high-contrast support, and reduced-motion support
- Custom icon, title, title content, actions, and CSS variables
- No JavaScript bridge and no static window-handle cache
- Reattaches safely when a MAUI window is recreated

## Requirements

- Windows 10 version 1809 or newer
- .NET 10
- .NET MAUI Blazor Hybrid (`BlazorWebView`)

## Installation

Reference the project directly, install the package when one is published, or add the repository as a submodule:

```bash
git submodule add https://github.com/chicuong2k3/BlazorHybrid.TitleBarKit.git vendor/BlazorHybrid.TitleBarKit
```

```xml
<ProjectReference Include="..\..\vendor\BlazorHybrid.TitleBarKit\src\BlazorHybrid.TitleBarKit\BlazorHybrid.TitleBarKit.csproj" />
```

## Setup

Register the library in `MauiProgram.cs`:

```csharp
using BlazorHybrid.TitleBarKit;

builder
    .UseMauiApp<App>()
    .UseBlazorHybridTitleBar(options =>
    {
        options.HideNativeTitleBar = true;
        options.IsMinimizable = true;
        options.IsMaximizable = true;
        options.IsResizable = true;
        options.IsClosable = true;
    });
```

Load the component stylesheet in the Blazor host page:

```html
<link rel="stylesheet" href="_content/BlazorHybrid.TitleBarKit/titlebar.css" />
```

Import the component namespace:

```razor
@using BlazorHybrid.TitleBarKit
```

Use the frame at the root of the rendered application:

```razor
<HybridWindowFrame Title="My application">
    <Icon>
        <img src="app-icon.svg" alt="" />
    </Icon>
    <Actions>
        <button type="button">Help</button>
    </Actions>

    <Router AppAssembly="typeof(MauiProgram).Assembly">
        ...
    </Router>
</HybridWindowFrame>
```

You can render `HybridTitleBar` directly when the application already owns its root layout:

```razor
<HybridTitleBar Title="My application" />
<main class="application-content">...</main>
```

## Styling

Override the semantic CSS variables instead of replacing internal selectors:

```css
:root {
    --bh-titlebar-height: 2.25rem;
    --bh-titlebar-background: #ffffff;
    --bh-titlebar-foreground: #211a14;
    --bh-titlebar-border: #ded7ce;
    --bh-titlebar-hover: #f4efe9;
    --bh-titlebar-active: #e9e0d6;
}
```

The `Class` parameter is available on both `HybridTitleBar` and `HybridWindowFrame` for application-level layout integration.

## Direct service access

The Razor component uses `IHybridTitleBarService`. Applications can inject the same service for custom controls:

```razor
@inject IHybridTitleBarService TitleBar

<button @onclick="TitleBar.Minimize">Minimize</button>
<button @onclick="TitleBar.ToggleMaximize">Maximize or restore</button>
<button @onclick="TitleBar.Close">Close</button>
```

`StateChanged` reports attachment and presenter-state changes. Commands are ignored safely before the native window is attached.

## Design notes

This project was inspired by [JiuLing.TitleBarKit](https://github.com/JiuLing-zhang/JiuLing.TitleBarKit), an MIT-licensed title-bar command bridge for WPF, WinForms, and hybrid hosts. This implementation is independently structured around .NET MAUI Blazor Hybrid, WinUI `AppWindow`, explicit lifecycle attachment, live state notifications, reusable Razor UI, accessibility, and testable service boundaries.

It intentionally remains Windows-only because custom title-bar APIs and behavior differ substantially across desktop platforms.

## Build and test

```bash
dotnet build BlazorHybrid.TitleBarKit.slnx -c Release
dotnet test BlazorHybrid.TitleBarKit.slnx -c Release
```

## License

MIT. See [LICENSE](LICENSE).
