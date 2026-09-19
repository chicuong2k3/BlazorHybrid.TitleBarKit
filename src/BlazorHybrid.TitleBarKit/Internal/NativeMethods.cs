using System.Runtime.InteropServices;

namespace BlazorHybrid.TitleBarKit.Internal;

internal static partial class NativeMethods
{
    internal const uint WmNcLButtonDown = 0x00A1;
    internal const int HtCaption = 0x0002;
    internal const uint WmSysCommand = 0x0112;
    internal const uint TpmRightButton = 0x0002;
    internal const uint TpmReturnCommand = 0x0100;
    internal const int GwlStyle = -16;
    internal const long WsThickFrame = 0x00040000L;
    internal const long WsSysMenu = 0x00080000L;
    internal const long WsMinimizeBox = 0x00020000L;
    internal const long WsMaximizeBox = 0x00010000L;
    internal const uint SwpNoSize = 0x0001;
    internal const uint SwpNoMove = 0x0002;
    internal const uint SwpNoZOrder = 0x0004;
    internal const uint SwpNoActivate = 0x0010;
    internal const uint SwpFrameChanged = 0x0020;

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

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    internal static partial nint GetWindowLongPtr(nint windowHandle, int index);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    internal static partial nint SetWindowLongPtr(nint windowHandle, int index, nint value);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetWindowPos(
        nint windowHandle,
        nint insertAfter,
        int x,
        int y,
        int width,
        int height,
        uint flags);
}
