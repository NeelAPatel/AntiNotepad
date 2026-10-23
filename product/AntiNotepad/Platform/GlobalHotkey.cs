using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using AntiNotepad;

namespace AntiNotepad.Platform;

/// <summary>A system-wide hotkey registered against an Avalonia window handle.</summary>
public sealed class GlobalHotkey : IDisposable
{
    private readonly int _id;               // one per hotkey on the same window
    private const uint WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001, ModControl = 0x0002, ModShift = 0x0004, ModWin = 0x0008, ModNoRepeat = 0x4000;

    // The default is defined here once. Override with the AppInfo-derived hotkey environment variable, e.g. Ctrl+Alt+Shift+A.
    public static HotkeyGesture DefaultGesture { get; } = new("Ctrl+Alt+Shift+S", ModControl | ModAlt | ModShift, 0x53);

    private readonly Window _window;
    private readonly HotkeyGesture _gesture;
    private readonly Action<long> _onPressed;
    private IntPtr _hwnd;
    private bool _hookAdded;
    private bool _disposed;

    public bool IsRegistered { get; private set; }
    public int RegistrationError { get; private set; }
    public string? RegistrationErrorMessage => !OperatingSystem.IsWindows()
        ? "Global hotkeys are supported only on Windows."
        : RegistrationError == 0 ? null : new System.ComponentModel.Win32Exception(RegistrationError).Message;
    public string GestureLabel => _gesture.Label;

    /// <param name="id">Distinct per hotkey registered on this window.</param>
    /// <param name="fromEnvironment">Only the show/hide hotkey honours the AppInfo-derived override (dev runs).</param>
    public GlobalHotkey(Window window, Action<long> onPressed, string? gesture = null, int id = 0x4150, bool fromEnvironment = true)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _onPressed = onPressed ?? throw new ArgumentNullException(nameof(onPressed));
        _id = id;
        var configured = gesture ?? (fromEnvironment ? Environment.GetEnvironmentVariable(AppInfo.Env("HOTKEY")) : null);
        _gesture = string.IsNullOrWhiteSpace(configured) ? DefaultGesture : HotkeyGesture.Parse(configured);
        _window.Opened += OnOpened;
        _window.Closed += OnClosed;
        Register();
    }

    private void OnOpened(object? sender, EventArgs e) => Register();

    private void Register()
    {
        if (_disposed || !OperatingSystem.IsWindows() || IsRegistered) return;
        _hwnd = _window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (_hwnd == IntPtr.Zero)
        {
            RegistrationError = 1400; // ERROR_INVALID_WINDOW_HANDLE
            return;
        }

        Win32Properties.AddWndProcHookCallback(_window, WndProc);
        _hookAdded = true;
        IsRegistered = RegisterHotKey(_hwnd, _id, _gesture.Modifiers | ModNoRepeat, _gesture.VirtualKey);
        RegistrationError = IsRegistered ? 0 : Marshal.GetLastWin32Error();
        if (!IsRegistered)
        {
            Win32Properties.RemoveWndProcHookCallback(_window, WndProc);
            _hookAdded = false;
        }
    }

    private IntPtr WndProc(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == WmHotkey && wParam.ToInt32() == _id)
        {
            handled = true;
            _onPressed(Stopwatch.GetTimestamp());
        }
        return IntPtr.Zero;
    }

    private void OnClosed(object? sender, EventArgs e) => Dispose();

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _window.Opened -= OnOpened;
        _window.Closed -= OnClosed;
        if (OperatingSystem.IsWindows())
        {
            if (IsRegistered) UnregisterHotKey(_hwnd, _id);
            if (_hookAdded) Win32Properties.RemoveWndProcHookCallback(_window, WndProc);
        }
        IsRegistered = false;
    }

    public readonly record struct HotkeyGesture(string Label, uint Modifiers, int VirtualKey)
    {
        public static HotkeyGesture Parse(string value)
        {
            var parts = value.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) throw new FormatException("Hotkey must include a modifier and a key, for example Ctrl+Alt+Shift+A.");
            uint modifiers = 0;
            int key = 0;
            foreach (var part in parts)
            {
                switch (part.ToLowerInvariant())
                {
                    case "ctrl": case "control": modifiers |= ModControl; break;
                    case "alt": modifiers |= ModAlt; break;
                    case "shift": modifiers |= ModShift; break;
                    case "win": case "windows": modifiers |= ModWin; break;
                    default:
                        if (key != 0) throw new FormatException("Hotkey must contain exactly one key.");
                        key = ParseVirtualKey(part);
                        break;
                }
            }
            if (modifiers == 0 || key == 0) throw new FormatException("Hotkey must include modifiers and one supported key.");
            return new HotkeyGesture(string.Join('+', parts), modifiers, key);
        }

        private static int ParseVirtualKey(string key)
        {
            if (key.Length == 1 && char.IsAsciiLetterOrDigit(key[0])) return char.ToUpperInvariant(key[0]);
            if (key.Length >= 2 && (key[0] is 'F' or 'f') && int.TryParse(key.AsSpan(1), out var function) && function is >= 1 and <= 24)
                return 0x70 + function - 1;
            return key.ToLowerInvariant() switch
            {
                "space" => 0x20, "escape" or "esc" => 0x1B, "tab" => 0x09,
                "enter" => 0x0D, "insert" => 0x2D, "delete" => 0x2E,
                _ => throw new FormatException($"Unsupported hotkey key '{key}'. Use A-Z, 0-9, F1-F24, Space, Escape, Tab, Enter, Insert, or Delete.")
            };
        }
    }

    [DllImport("user32.dll", SetLastError = true)] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint modifiers, int key);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
