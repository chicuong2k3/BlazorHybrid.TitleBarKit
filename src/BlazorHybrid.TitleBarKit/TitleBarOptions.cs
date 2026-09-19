namespace BlazorHybrid.TitleBarKit;

/// <summary>Configures native window behavior for the hybrid title bar.</summary>
public sealed class TitleBarOptions
{
    /// <summary>Gets or sets whether the native title bar is hidden while the resize border is retained.</summary>
    public bool HideNativeTitleBar { get; set; } = true;

    /// <summary>Gets or sets whether the window may be minimized.</summary>
    public bool IsMinimizable { get; set; } = true;

    /// <summary>Gets or sets whether the window may be maximized.</summary>
    public bool IsMaximizable { get; set; } = true;

    /// <summary>Gets or sets whether the window may be resized.</summary>
    public bool IsResizable { get; set; } = true;

    /// <summary>Gets or sets whether the window may be closed.</summary>
    public bool IsClosable { get; set; } = true;
}
