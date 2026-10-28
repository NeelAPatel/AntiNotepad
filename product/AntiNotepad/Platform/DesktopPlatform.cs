using Avalonia.Controls;

namespace AntiNotepad.Platform;

internal static partial class DesktopPlatform
{
    internal static Avalonia.PixelPoint CursorPosition() => Native.CursorPosition();
    internal static IDisposable ListenClipboard(Window window, Action changed) => Native.ListenClipboard(window, changed);
    internal static bool ClipboardIsPrivate() => Native.ClipboardIsPrivate();
    internal static void Chime() => Native.Chime();
    internal static void TrimWorkingSet() => Native.TrimWorkingSet();
    internal static void ConfigurePopupWindow(TopLevel popup) => Native.ConfigurePopupWindow(popup);
    internal static void Flash(Window window) => Native.Flash(window);
    internal static void OpenUrl(string url) => Native.OpenUrl(url);
    internal static void OpenFolder(string path) => Native.OpenFolder(path);
    internal static void StartScreenSnip() => Native.StartScreenSnip();
}
