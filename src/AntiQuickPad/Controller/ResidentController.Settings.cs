using AntiQuickPad.Core;
using AntiQuickPad.Shell;

namespace AntiQuickPad.Controller;

// M11a-settings-function: settings window lifecycle and preference commands. The window itself is a view.
internal sealed partial class ResidentController
{
    private bool _hotkeysEnabled;
    private string? _hotkeyError;
    public string? HotkeyError => _hotkeyError;
    public string HotkeyLabel => _hotkey?.GestureLabel ?? EffectiveGesture ?? GlobalHotkey.DefaultGesture.Label;
    public bool HotkeyFromEnvironment => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ANTIQUICKPAD_HOTKEY"));
    // ANTIQUICKPAD_HOTKEY (the measure child relies on it) always wins over the stored preference.
    private string? EffectiveGesture => HotkeyFromEnvironment ? Environment.GetEnvironmentVariable("ANTIQUICKPAD_HOTKEY") : _vm.Preferences.Hotkey;

    public void ToggleSettings()
    {
        if (_surface.IsSettingsOpen) CloseSettings(); else _surface.OpenSettings();
    }
    public void CloseSettings(bool refocus = true)
    {
        if (!_surface.IsSettingsOpen) return;
        if (_hotkeyCapture) CaptureHotkey(false);
        _surface.CloseSettings();
        if (refocus) FocusEditor();
    }

    private void SetPreference(Func<Preferences, Preferences> change)
    {
        var before = _vm.Preferences;
        _vm.UpdatePreferences(change);
        if (_vm.Preferences == before) return;
        _surface.ApplyPreferences();
        try { _surface.SaveWindowSettings(); }
        catch (Exception ex) { _surface.Log($"window-settings-save-error {ex}"); }
    }
    public void SetMaterial(string material) => SetPreference(p => p with { Material = material });
    public void SetTint(double tint) => SetPreference(p => p with { Tint = tint });
    public void SetBars(string mode) => SetPreference(p => p with { Bars = mode });
    public void SetFontSize(double size) => SetPreference(p => p with { FontSize = size });
    public void SetMarkerOpacity(double opacity) => SetPreference(p => p with { MarkerOpacity = opacity });
    public void SetStrike(double em) => SetPreference(p => p with { StrikeEm = em });

    public void SetStartup(bool enabled)
    {
        try { _services.Startup.SetEnabled(enabled); }
        catch (Exception ex) { _surface.Log($"startup-entry-error {ex.Message}"); }
        // The stored value mirrors the real entry, never just the request.
        var actual = _services.Startup.IsEnabled;
        SetPreference(p => p with { StartWithWindows = actual });
    }
    private void SyncStartup()
    {
        try { var actual = _services.Startup.IsEnabled; if (actual != _vm.Preferences.StartWithWindows) SetPreference(p => p with { StartWithWindows = actual }); }
        catch (Exception ex) { _surface.Log($"startup-entry-error {ex.Message}"); }
    }

    // Suspend the global hotkey while the settings capture box listens, so the combination reaches the box.
    private bool _hotkeyCapture;
    public void CaptureHotkey(bool capturing)
    {
        if (capturing == _hotkeyCapture) return;
        _hotkeyCapture = capturing;
        if (!_hotkeysEnabled) return;
        if (capturing) { _hotkey?.Dispose(); _hotkey = null; }
        else if (_hotkey is null) InstallHotkey();
    }

    /// <summary>Validate, register and store a new gesture; on failure the previous gesture stays active.</summary>
    public bool SetHotkey(string gesture)
    {
        _hotkeyError = null;
        string label;
        try { label = GlobalHotkey.HotkeyGesture.Parse(gesture).Label; }
        catch (FormatException ex) { _hotkeyError = ex.Message; _surface.RefreshStatus(); return false; }
        if (HotkeyFromEnvironment) { _hotkeyError = "ANTIQUICKPAD_HOTKEY is set; it overrides this setting."; _surface.RefreshStatus(); return false; }
        var previous = _vm.Preferences.Hotkey;
        _hotkeyCapture = false;
        if (_hotkeysEnabled)
        {
            _hotkey?.Dispose(); _hotkey = null;
            var candidate = _surface.CreateHotkey(timestamp => Toggle(timestamp), label);
            if (!candidate.IsRegistered)
            {
                _hotkeyError = $"{label} is not available: {candidate.RegistrationErrorMessage}";
                candidate.Dispose(); InstallHotkey();
                _surface.Log($"hotkey-change-rejected gesture={label} kept={HotkeyLabel}");
                _surface.RefreshStatus();
                return false;
            }
            _hotkey = candidate;
        }
        SetPreference(p => p with { Hotkey = label });
        _tray?.SetHotkeyStatus(label);
        _surface.Log($"hotkey-changed from={previous ?? GlobalHotkey.DefaultGesture.Label} to={label}");
        _surface.RefreshStatus();
        return true;
    }

    // Selftest only: route hotkeys through the services (memory hotkey in hidden mode).
    internal void EnableHotkeysForSmoke() { _hotkeysEnabled = true; InstallHotkey(); }
    internal bool HotkeyRegistered => _hotkey?.IsRegistered == true;
}
