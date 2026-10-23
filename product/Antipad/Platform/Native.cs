using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Microsoft.Win32;

namespace Antipad.Platform;

/// <summary>Small Win32 helpers: clipboard listener, attention cues, run-at-login.</summary>
internal static class Native
{
    private const uint WmClipboardUpdate = 0x031D;
    private const uint DwmwaWindowCornerPreference = 33;
    private const uint DwmwaBorderColor = 34;
    private const uint DwmwcpDonotround = 1;
    private const uint DwmwaColorNone = 0xFFFFFFFE;

    /// <summary>Raises <paramref name="changed"/> on every clipboard update while the window handle lives.</summary>
    public static IDisposable ListenClipboard(Window window, Action changed)
    {
        var hwnd = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (hwnd == IntPtr.Zero) return new Disposer(() => { });
        Win32Properties.CustomWndProcHookCallback hook = (IntPtr h, uint msg, IntPtr w, IntPtr l, ref bool handled) =>
        {
            if (msg == WmClipboardUpdate) changed();
            return IntPtr.Zero;
        };
        Win32Properties.AddWndProcHookCallback(window, hook);
        AddClipboardFormatListener(hwnd);
        return new Disposer(() => { RemoveClipboardFormatListener(hwnd); Win32Properties.RemoveWndProcHookCallback(window, hook); });
    }

    public static Avalonia.PixelPoint CursorPosition() => GetCursorPos(out var p) ? new Avalonia.PixelPoint(p.X, p.Y) : default;

    /// <summary>True when the clipboard owner asked monitors/history to skip this content (password managers do).</summary>
    public static bool ClipboardIsPrivate()
    {
        try
        {
            if (IsClipboardFormatAvailable(RegisterClipboardFormat("ExcludeClipboardContentFromMonitorProcessing"))) return true;
            if (IsClipboardFormatAvailable(RegisterClipboardFormat("Clipboard Viewer Ignore"))) return true;
            var history = RegisterClipboardFormat("CanIncludeInClipboardHistory");
            if (!IsClipboardFormatAvailable(history) || !OpenClipboard(IntPtr.Zero)) return false;
            try
            {
                var handle = GetClipboardData(history);
                var ptr = handle == IntPtr.Zero ? IntPtr.Zero : GlobalLock(handle);
                if (ptr == IntPtr.Zero) return true;
                try { return Marshal.ReadInt32(ptr) == 0; } finally { GlobalUnlock(handle); }
            }
            finally { CloseClipboard(); }
        }
        catch { return true; }
    }

    /// <summary>Hands this process's unused pages back to Windows (they fault back in on demand).</summary>
    public static void TrimWorkingSet()
    {
        try { using var self = Process.GetCurrentProcess(); SetProcessWorkingSetSize(self.Handle, -1, -1); } catch { }
    }

    public static void Chime() { try { MessageBeep(0x40); } catch { } }

    /// <summary>Removes Windows 11's automatic popup rounding and border from a transparent popup host.</summary>
    public static void ConfigurePopupWindow(TopLevel popup)
    {
        var hwnd = popup.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (hwnd == IntPtr.Zero) return;
        try
        {
            SetDwmWindowAttribute(hwnd, DwmwaWindowCornerPreference, DwmwcpDonotround);
            SetDwmWindowAttribute(hwnd, DwmwaBorderColor, DwmwaColorNone);
        }
        catch
        {
            // DWM attributes are cosmetic; unsupported or unavailable DWM must not affect the menu.
        }
    }

    public static void Flash(Window window)
    {
        var hwnd = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (hwnd == IntPtr.Zero) return;
        var info = new FlashInfo { Size = (uint)Marshal.SizeOf<FlashInfo>(), Hwnd = hwnd, Flags = 3 | 0xC, Count = 3 };
        FlashWindowEx(ref info);
    }

    public static void OpenUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")) return;
        Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
    }

    public static void StartScreenSnip()
    {
        try { Process.Start(new ProcessStartInfo("ms-screenclip:") { UseShellExecute = true }); } catch { }
    }

    private sealed class Disposer(Action dispose) : IDisposable
    {
        private Action? _dispose = dispose;
        public void Dispose() { _dispose?.Invoke(); _dispose = null; }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FlashInfo { public uint Size; public IntPtr Hwnd; public uint Flags; public uint Count; public uint Timeout; }
    [StructLayout(LayoutKind.Sequential)] private struct NativePoint { public int X, Y; }
    [DllImport("user32.dll")] private static extern bool GetCursorPos(out NativePoint point);
    [DllImport("user32.dll")] private static extern bool AddClipboardFormatListener(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern bool MessageBeep(uint type);
    [DllImport("kernel32.dll")] private static extern bool SetProcessWorkingSetSize(IntPtr process, nint min, nint max);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern uint RegisterClipboardFormat(string name);
    [DllImport("user32.dll")] private static extern bool IsClipboardFormatAvailable(uint format);
    [DllImport("user32.dll")] private static extern bool OpenClipboard(IntPtr owner);
    [DllImport("user32.dll")] private static extern bool CloseClipboard();
    [DllImport("user32.dll")] private static extern IntPtr GetClipboardData(uint format);
    [DllImport("kernel32.dll")] private static extern IntPtr GlobalLock(IntPtr handle);
    [DllImport("kernel32.dll")] private static extern bool GlobalUnlock(IntPtr handle);
    [DllImport("user32.dll")] private static extern bool FlashWindowEx(ref FlashInfo info);
    [DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr hwnd, uint attribute, ref uint value, int valueSize);

    private static void SetDwmWindowAttribute(IntPtr hwnd, uint attribute, uint value) =>
        _ = DwmSetWindowAttribute(hwnd, attribute, ref value, sizeof(uint));
}

/// <summary>HKCU Run entry for Antipad only (never the AntiQuickPad value).</summary>
internal static class Startup
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "Antipad";
    private static string Command => "\"" + Path.Combine(AppContext.BaseDirectory, "Antipad.exe") + "\" --hidden";

    public static bool IsEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey);
            return key?.GetValue(ValueName) is string value && string.Equals(value, Command, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKey);
        if (enabled) key.SetValue(ValueName, Command);
        else if (key.GetValue(ValueName) is not null) key.DeleteValue(ValueName);
    }
}
