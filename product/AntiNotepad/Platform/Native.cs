using System.Diagnostics;
using System.Runtime.InteropServices;
using AntiNotepad;
using Avalonia.Controls;
using Microsoft.Win32;

namespace AntiNotepad.Platform;

/// <summary>Small Win32 helpers: clipboard listener, attention cues, run-at-login.</summary>
internal static class Native
{
    private const uint WmClipboardUpdate = 0x031D;
    private const uint DwmwaWindowCornerPreference = 33;
    private const uint DwmwaBorderColor = 34;
    private const uint DwmwaTransitionsForcedDisabled = 3;
    private const uint DwmwcpDonotround = 1;
    private const uint DwmwaColorNone = 0xFFFFFFFE;
    private const uint SpiGetClientAreaAnimation = 0x1042;
    private const int SwHide = 0;
    private const int SwShowNoActivate = 4;
    private const int SwShowMinNoActive = 7;
    private const uint WsCaption = 0x00C00000;
    private const uint WsSysMenu = 0x00080000;
    private const uint WsMinimizeBox = 0x00020000;
    private const uint WsMaximizeBox = 0x00010000;
    internal const uint NativeTransitionStyleBits = WsCaption | WsSysMenu | WsMinimizeBox | WsMaximizeBox;
    private static readonly object DwmTransitionLock = new();
    private static readonly Dictionary<IntPtr, TransitionSuppressionState> DwmTransitionSuppressions = new();

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

    /// <summary>
    /// Keeps the native top-level caption style bits through Avalonia's frameless style rewrites.
    /// Avalonia's extended-client-area path suppresses the actual non-client title bar and keeps resize-border sizing.
    /// </summary>
    public static IDisposable ConfigureNativeTransitions(TopLevel window)
    {
        Win32Properties.CustomWindowStylesCallback styles = (style, exStyle) =>
            (AddNativeTransitionStyles(style), exStyle);
        Win32Properties.AddWindowStylesCallback(window, styles);
        return new Disposer(() => Win32Properties.RemoveWindowStylesCallback(window, styles));
    }

    /// <summary>Adds caption, system-menu, minimize, and maximize style bits without dropping existing styles.</summary>
    internal static uint AddNativeTransitionStyles(uint style) => style | NativeTransitionStyleBits;

    /// <summary>Temporarily suppresses DWM transitions for one window. Nested scopes restore the original value once.</summary>
    public static IDisposable SuppressDwmTransitions(TopLevel window)
    {
        var hwnd = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (hwnd == IntPtr.Zero) return new Disposer(() => { });

        lock (DwmTransitionLock)
        {
            if (DwmTransitionSuppressions.TryGetValue(hwnd, out var existing))
            {
                existing.Depth++;
                return new TransitionSuppression(hwnd);
            }

            // This DWM attribute is documented as set-only. Read it when the OS supports that query; otherwise the
            // new HWND's default is zero and A32 owns all subsequent changes to it.
            var previous = TryGetDwmWindowAttribute(hwnd, DwmwaTransitionsForcedDisabled, out var current) ? current : 0;
            if (!TrySetDwmWindowAttribute(hwnd, DwmwaTransitionsForcedDisabled, 1))
                return new Disposer(() => { });

            DwmTransitionSuppressions.Add(hwnd, new TransitionSuppressionState(previous));
            return new TransitionSuppression(hwnd);
        }
    }

    /// <summary>Reads the Windows client-area animation preference; a failed query conservatively disables app motion.</summary>
    public static bool AnimationsEnabled
    {
        get
        {
            try { return SystemParametersInfo(SpiGetClientAreaAnimation, 0, out var enabled, 0) && enabled != 0; }
            catch { return false; }
        }
    }

    /// <summary>Registers a system-wide message such as TaskbarCreated for a native window hook.</summary>
    public static uint RegisterWindowMessage(string name)
    {
        try { return RegisterWindowMessageNative(name); }
        catch { return 0; }
    }

    /// <summary>Shows a native taskbar stand-in minimized without activating it.</summary>
    public static void ShowMinimizedNoActivate(TopLevel window) => ShowNativeWindow(window, SwShowMinNoActive);

    /// <summary>Restores a native window without activating it.</summary>
    public static void RestoreNoActivate(TopLevel window) => ShowNativeWindow(window, SwShowNoActivate);

    /// <summary>Hides a native taskbar stand-in without changing Avalonia's logical window state.</summary>
    public static void HideNativeWindow(TopLevel window) => ShowNativeWindow(window, SwHide);

    private static void ShowNativeWindow(TopLevel window, int command)
    {
        var hwnd = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (hwnd == IntPtr.Zero) return;
        try { ShowWindowNative(hwnd, command); } catch { }
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

    private sealed class TransitionSuppression(IntPtr hwnd) : IDisposable
    {
        private IntPtr _hwnd = hwnd;

        public void Dispose()
        {
            var target = _hwnd;
            if (target == IntPtr.Zero) return;
            _hwnd = IntPtr.Zero;

            lock (DwmTransitionLock)
            {
                if (!DwmTransitionSuppressions.TryGetValue(target, out var state)) return;
                if (--state.Depth == 0)
                {
                    DwmTransitionSuppressions.Remove(target);
                    // Keep restoration serialized with a new suppression scope for the same HWND.
                    TrySetDwmWindowAttribute(target, DwmwaTransitionsForcedDisabled, state.Previous);
                }
            }
        }
    }

    private sealed class TransitionSuppressionState(uint previous)
    {
        public uint Previous { get; } = previous;
        public int Depth { get; set; } = 1;
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
    [DllImport("dwmapi.dll")] private static extern int DwmGetWindowAttribute(IntPtr hwnd, uint attribute, out uint value, int valueSize);
    [DllImport("user32.dll", EntryPoint = "SystemParametersInfoW")] private static extern bool SystemParametersInfo(uint action, uint param, out int value, uint flags);
    [DllImport("user32.dll", EntryPoint = "RegisterWindowMessageW", CharSet = CharSet.Unicode)] private static extern uint RegisterWindowMessageNative(string name);
    [DllImport("user32.dll", EntryPoint = "ShowWindow")] private static extern bool ShowWindowNative(IntPtr hwnd, int command);

    private static void SetDwmWindowAttribute(IntPtr hwnd, uint attribute, uint value) =>
        _ = DwmSetWindowAttribute(hwnd, attribute, ref value, sizeof(uint));

    private static bool TryGetDwmWindowAttribute(IntPtr hwnd, uint attribute, out uint value)
    {
        try { return DwmGetWindowAttribute(hwnd, attribute, out value, sizeof(uint)) == 0; }
        catch { value = 0; return false; }
    }

    private static bool TrySetDwmWindowAttribute(IntPtr hwnd, uint attribute, uint value)
    {
        try { return DwmSetWindowAttribute(hwnd, attribute, ref value, sizeof(uint)) == 0; }
        catch { return false; }
    }
}

/// <summary>HKCU Run entry for this product, with a one-time migration from the legacy value name.</summary>
internal static class Startup
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    // This old value name is retained only to migrate the previous product identity.
    private const string LegacyValueName = "Antipad";
    // Settings always target the canonical installation, even when opened in the dev copy.
    private static string Command => "\"" + Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Programs", AppInfo.Name, $"{AppInfo.Name}.exe") + "\" --hidden";

    public static bool IsEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey);
            return key?.GetValue(AppInfo.RunValueName) is string value && string.Equals(value, Command, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKey);
        if (enabled) key.SetValue(AppInfo.RunValueName, Command);
        else if (key.GetValue(AppInfo.RunValueName) is not null) key.DeleteValue(AppInfo.RunValueName);
    }

    /// <summary>Called only by the primary instance on normal startup, never by --selftest or a custom-data dev run.</summary>
    public static void MigrateLegacy(Action<string>? logError = null)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
            if (key?.GetValue(LegacyValueName) is null) return;

            // Write the new value first. If this fails, the old value remains intact.
            key.SetValue(AppInfo.RunValueName, Command, RegistryValueKind.String);
            key.DeleteValue(LegacyValueName, throwOnMissingValue: false);
        }
        catch (Exception ex)
        {
            logError?.Invoke($"Could not migrate the legacy Run-at-login entry: {ex.Message}");
        }
    }
}
