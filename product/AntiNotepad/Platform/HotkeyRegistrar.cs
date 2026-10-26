using AntiNotepad;
using Avalonia.Controls;

namespace AntiNotepad.Platform;

internal interface IHotkeyRegistration : IDisposable
{
    string GestureLabel { get; }
    bool IsRegistered { get; }
    string? RegistrationErrorMessage { get; }
}

/// <summary>Adapts the window-bound native registration to the registrar's test seam.</summary>
internal sealed class GlobalHotkeyRegistration : IHotkeyRegistration
{
    private readonly GlobalHotkey _hotkey;

    internal GlobalHotkeyRegistration(Window window, Action<long> onPressed, string? gesture = null,
        int id = 0x4150, bool fromEnvironment = true)
    {
        _hotkey = new GlobalHotkey(window, onPressed, gesture, id, fromEnvironment);
    }

    public string GestureLabel => _hotkey.GestureLabel;
    public bool IsRegistered => _hotkey.IsRegistered;
    public string? RegistrationErrorMessage => _hotkey.RegistrationErrorMessage;

    public void Dispose() => _hotkey.Dispose();
}

/// <summary>Owns both global hotkey registrations and the rollback/status policy.</summary>
internal sealed class HotkeyRegistrar : IDisposable
{
    private readonly Func<string, Action<long>, IHotkeyRegistration> _makeHotkey;
    private readonly Func<string, Action<long>, IHotkeyRegistration> _makeNewNoteHotkey;
    private readonly Action<long> _toggleAction;
    private readonly Action _newNoteAction;
    private readonly Action<string> _toast;
    private IHotkeyRegistration? _hotkey;
    private IHotkeyRegistration? _newNoteHotkey;
    private bool _disposed;

    internal HotkeyRegistrar(
        Func<string, Action<long>, IHotkeyRegistration> makeHotkey,
        Func<string, Action<long>, IHotkeyRegistration> makeNewNoteHotkey,
        Action<long> toggleAction,
        Action newNoteAction,
        Action<string> toast)
    {
        _makeHotkey = makeHotkey;
        _makeNewNoteHotkey = makeNewNoteHotkey;
        _toggleAction = toggleAction;
        _newNoteAction = newNoteAction;
        _toast = toast;
    }

    internal string HotkeyStatus { get; private set; } = "";
    internal string NewNoteHotkeyStatus => _newNoteHotkey?.GestureLabel ?? "off";
    internal event Action<string>? HotkeyStatusChanged;

    internal bool RegisterHotkey(string? gesture)
    {
        try
        {
            // No gesture: the dev override, else the default.
            var wanted = gesture ?? (Environment.GetEnvironmentVariable(AppInfo.Env("HOTKEY")) is { Length: > 0 } env ? env : GlobalHotkey.DefaultGesture.Label);
            var label = GlobalHotkey.HotkeyGesture.Parse(wanted).Label;
            var ok = SwapHotkey(ref _hotkey, wanted, label,
                g => _makeHotkey(g, _toggleAction), out var error);
            HotkeyStatus = _hotkey?.GestureLabel ?? $"{label} unavailable: {error}";
            HotkeyStatusChanged?.Invoke(HotkeyStatus);
            return ok;
        }
        catch (FormatException ex) { _toast(ex.Message); return false; }
    }

    /// <summary>
    /// Replaces a registered hotkey. Windows keeps two registrations with the same id side by side and may release either,
    /// so the old key is released first, and put back if Windows refuses the new one. The same key again changes nothing.
    /// </summary>
    private static bool SwapHotkey(ref IHotkeyRegistration? slot, string wanted, string label,
        Func<string, IHotkeyRegistration> make, out string? error)
    {
        error = null;
        if (slot is { IsRegistered: true } && string.Equals(slot.GestureLabel, label, StringComparison.OrdinalIgnoreCase)) return true;
        var previous = slot?.GestureLabel;
        slot?.Dispose(); slot = null;
        var next = make(wanted);
        if (next.IsRegistered) { slot = next; return true; }
        error = next.RegistrationErrorMessage;
        next.Dispose();
        if (previous is not null)
        {
            var back = make(previous);
            if (back.IsRegistered) slot = back; else back.Dispose();
        }
        return false;
    }

    /// <summary>A second global hotkey that shows the pad on a fresh note. Empty turns it off. Returns false if Windows refused it.</summary>
    internal bool RegisterNewNoteHotkey(string? gesture)
    {
        if (string.IsNullOrWhiteSpace(gesture)) { _newNoteHotkey?.Dispose(); _newNoteHotkey = null; return true; }
        try
        {
            var label = GlobalHotkey.HotkeyGesture.Parse(gesture).Label;
            return SwapHotkey(ref _newNoteHotkey, gesture, label,
                g => _makeNewNoteHotkey(g, _ => _newNoteAction()), out _);
        }
        catch (FormatException ex) { _toast(ex.Message); return false; }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _hotkey?.Dispose(); _hotkey = null;
        _newNoteHotkey?.Dispose(); _newNoteHotkey = null;
    }
}
