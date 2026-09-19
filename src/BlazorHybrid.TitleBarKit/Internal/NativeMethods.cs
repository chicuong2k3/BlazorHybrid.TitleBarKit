using System.Runtime.InteropServices;

namespace BlazorHybrid.TitleBarKit.Internal;

internal static partial class NativeMethods
{
    internal const uint WmNcLButtonDown = 0x00A1;
    internal const int HtCaption = 0x0002;
    internal const uint WmSysCommand = 0x0112;
    internal const uint TpmRightButton = 0x0002;
    internal const uint TpmReturnCommand = 0x0100;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Point
    {
        internal int X;
        internal int Y;
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool ReleaseCapture();

    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    internal static partial nint SendMessage(nint windowHandle, uint message, nint wParam, nint lParam);

    [LibraryImport("user32.dll")]
    internal static partial nint GetSystemMenu(nint windowHandle, [MarshalAs(UnmanagedType.Bool)] bool revert);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetCursorPos(out Point point);

    [LibraryImport("user32.dll")]
    internal static partial uint TrackPopupMenuEx(nint menu, uint flags, int x, int y, nint windowHandle, nint reserved);

    [LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool PostMessage(nint windowHandle, uint message, nint wParam, nint lParam);
}
