namespace BlazorHybrid.TitleBarKit.Internal;

internal interface IWindowBackend
{
    event EventHandler<TitleBarState>? StateChanged;
    TitleBarState State { get; }
    void BeginDrag();
    void Minimize();
    void Maximize();
    void Restore();
    void ShowSystemMenu();
    void Close();
}
