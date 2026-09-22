namespace BlazorHybrid.TitleBarKit;

/// <summary>Controls the native window that hosts the Blazor Hybrid WebView.</summary>
public interface IHybridTitleBarService
{
    /// <summary>Raised when attachment or native window state changes.</summary>
    event EventHandler<TitleBarState>? StateChanged;

    /// <summary>Gets the latest native window state.</summary>
    TitleBarState State { get; }

    /// <summary>Begins the operating system's native window-drag operation. The caller starts this only after the pointer has moved while the button is still held.</summary>
    void BeginDrag();

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
