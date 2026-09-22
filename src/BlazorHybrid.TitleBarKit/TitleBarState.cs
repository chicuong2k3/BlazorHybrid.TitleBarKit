namespace BlazorHybrid.TitleBarKit;

/// <summary>Describes the current state of the attached native window.</summary>
public readonly record struct TitleBarState(
    bool IsAttached,
    bool IsMaximized,
    bool IsMinimized,
    bool CanMaximize,
    bool CanMinimize,
    bool CanResize,
    bool CanClose)
{
    /// <summary>Whether Windows draws the caption buttons (including Snap Layouts).</summary>
    public bool UsesNativeControls { get; init; }

    /// <summary>Space reserved for native caption buttons, in logical pixels.</summary>
    public double CaptionButtonsWidth { get; init; }

    /// <summary>The state used before a native window has been attached.</summary>
    public static TitleBarState Detached { get; } = new(false, false, false, false, false, false, false);
}
