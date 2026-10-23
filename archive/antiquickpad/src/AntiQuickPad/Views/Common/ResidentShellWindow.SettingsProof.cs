using Avalonia.Controls;
using Avalonia.Media;
using AntiQuickPad.Controller;

namespace AntiQuickPad.Views.Common;

public sealed partial class ResidentShellWindow
{
    // M11-settings-window: lifecycle, preference persistence (never wiped by window saves), hotkey rollback, startup mirror.
    private async Task SettingsSmoke()
    {
        var surface = (IResidentSurface)this;
        var toggle = Command("settings.toggle");
        toggle.Execute(null); await Task.Delay(50);
        Check(surface.IsSettingsOpen && _settings!.Owner == this, "Settings did not open as an owned window");
        Shell.HiddenWindows.Check(_settings!);
        toggle.Execute(null); Check(!surface.IsSettingsOpen, "Settings toggle did not close");
        toggle.Execute(null); _controller.HideResident();
        Check(!surface.IsSettingsOpen, "Hiding the pad left settings open");
        _controller.ShowResident("settings-smoke"); await Task.Delay(50);
        toggle.Execute(null); _controller.CloseSettings();
        Check(!surface.IsSettingsOpen, "Esc/close path left settings open");

        var defaults = _vm.Preferences;
        Command("settings.tint").Execute(.7);
        Check(_layout.Tint?.Background is ISolidColorBrush { Opacity: .7 } && CurrentLook.TintAlpha == .7, "Tint did not apply in place");
        var editorBefore = _editor;
        Check(ReferenceEquals(editorBefore, _editor), "Tint change rebuilt the editor");
        Command("settings.material").Execute("mica");
        Check(TransparencyLevelHint[0] == WindowTransparencyLevel.Mica, "Material did not apply");
        Command("settings.bars").Execute("always");
        Check(_barReveal!.Always && _barReveal.Revealed, "Always-visible bars did not apply");
        SaveWindowSnapshot(); // A window save must keep preferences.
        var reloaded = new Core.SettingsStore(Program.Paths.Settings).Load()!.Preferences!;
        Check(reloaded.Tint == .7 && reloaded.Material == "mica" && reloaded.Bars == "always", "Preferences lost on a window-settings save");
        Command("settings.font-size").Execute(16d);
        Check(_editor.FontSize == 16 && !ReferenceEquals(editorBefore, _editor), "Font size did not rebuild the editor");
        Command("settings.marker-dim").Execute(.5); Command("settings.strike").Execute(.2);
        Check(CurrentLook.MarkdownMarker.A < 255 && CurrentLook.StrikeThicknessEm == .2, "Marker/strike preferences did not apply");

        Command("settings.startup").Execute(true);
        Check(_vm.Preferences.StartWithWindows && _services.Startup.IsEnabled, "Start with Windows did not mirror the entry");
        Command("settings.startup").Execute(false);
        Check(!_vm.Preferences.StartWithWindows && !_services.Startup.IsEnabled, "Start with Windows off failed");

        _controller.EnableHotkeysForSmoke();
        Check(_controller.HotkeyRegistered && _controller.HotkeyLabel == Shell.GlobalHotkey.DefaultGesture.Label, "Memory hotkey not installed");
        Check(!_controller.SetHotkey(Shell.MemoryHotkeyService.Conflict) && _controller.HotkeyRegistered &&
            _controller.HotkeyLabel == Shell.GlobalHotkey.DefaultGesture.Label && _controller.HotkeyError is not null && _vm.Preferences.Hotkey is null,
            "Conflicting hotkey did not roll back");
        Check(!_controller.SetHotkey("Shift") && _controller.HotkeyRegistered, "Invalid hotkey accepted");
        Check(_controller.SetHotkey("Ctrl+Alt+Shift+K") && _controller.HotkeyLabel == "Ctrl+Alt+Shift+K" && _vm.Preferences.Hotkey == "Ctrl+Alt+Shift+K", "Hotkey change failed");
        Command("settings.hotkey-capture").Execute(true); Check(!_controller.HotkeyRegistered, "Capture did not suspend the hotkey");
        Command("settings.hotkey-capture").Execute(false); Check(_controller.HotkeyRegistered && _controller.HotkeyLabel == "Ctrl+Alt+Shift+K", "Capture end did not restore the hotkey");

        var legacy = Path.Combine(Program.Paths.Root, "settings-legacy-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(legacy, """{"Width":480,"Height":560,"X":1,"Y":2,"Pinned":false,"ShellId":"quiet","LookId":"system","LayoutVersion":1}""");
        Check(new Core.SettingsStore(legacy).Load()?.Preferences == new Core.Preferences(), "Legacy settings did not load default preferences");
        File.Delete(legacy);

        _vm.UpdatePreferences(_ => defaults); surface.ApplyPreferences(); SaveWindowSnapshot();
        Check(_vm.Preferences == defaults && _editor.FontSize == T.BodySize, "Preference reset failed");
        Console.WriteLine("M11-settings-window SETTINGS PASS: owned window closes with toggle/hide/Esc; tint/material/bars in place; font/marker/strike rebuild; preferences survive window saves; startup mirrors entry; hotkey change, rollback and capture suspend; legacy defaults.");
    }
}
