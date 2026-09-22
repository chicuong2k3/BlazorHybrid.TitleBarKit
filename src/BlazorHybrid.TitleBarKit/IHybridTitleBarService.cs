namespace BlazorHybrid.TitleBarKit;

/// <summary>Controls the native window that hosts the Blazor Hybrid WebView.</summary>
public interface IHybridTitleBarService
{
    /// <summary>Raised when attachment or native window state changes.</summary>
    event EventHandler<TitleBarState>? StateChanged;

    /// <summary>Gets the latest native window state.</summary>
    TitleBarState State { get; }

    /// <summary>Legacy compatibility no-op. Use HybridTitleBar's native caption region instead of synthetic drag events.</summary>
    void BeginDrag();

    /// <summary>Sets the native caption hit-test rectangle in client-area physical pixels.</summary>
    void SetCaptionRegion(int x, int y, int width, int height);

    /// <summary>Legacy compatibility no-op. Windows now owns caption mouse capture and release.</summary>
    void CancelPendingDrag();

    /// <summary>Minimizes the attached window.</summary>
    void Minimize();

    /// <summary>Maximizes the attached window.</summary>
    void Maximize();

    /// <summary>Restores the attached window.</summary>
    void Restore();

    /// <summary>Toggles between maximized and restored states.</summary>
    void ToggleMaximize();

    /// <summary>Opens the standard Windows system menu at the current pointer position.</summary>
    void ShowSystemMenu();

    /// <summary>Closes the attached window.</summary>
    void Close();
}
