using System.Runtime.InteropServices;

namespace ThemeTrayApp.Services;

internal static class NativeMethods
{
    internal const uint WM_HOTKEY = 0x0312;
    internal const uint WM_THEMECHANGED = 0x031A;
    internal const uint WM_SETTINGCHANGE = 0x001A;

    internal static readonly IntPtr HwndBroadcast = new(0xFFFF);

    [Flags]
    internal enum SendMessageTimeoutFlags : uint
    {
        Normal = 0x0000,
        Block = 0x0001,
        AbortIfHung = 0x0002,
        NoTimeoutIfNotHung = 0x0008
    }

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern IntPtr SendMessageTimeout(
        IntPtr hWnd,
        uint msg,
        UIntPtr wParam,
        string? lParam,
        SendMessageTimeoutFlags flags,
        uint timeout,
        out UIntPtr lpdwResult);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool DestroyIcon(IntPtr hIcon);
}