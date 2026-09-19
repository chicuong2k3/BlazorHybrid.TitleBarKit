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
    /// <summary>The state used before a native window has been attached.</summary>
    public static TitleBarState Detached { get; } = new(false, false, false, false, false, false, false);
}
