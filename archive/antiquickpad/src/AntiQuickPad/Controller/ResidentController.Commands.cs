using System.Windows.Input;
using AntiQuickPad.ViewModels;
using AntiQuickPad.Contract;

namespace AntiQuickPad.Controller;

internal sealed partial class ResidentController
{
    public ICommand Command(string id)
    {
        FunctionContract.RequireCommand(id);
        return id switch
        {
            "note.create" => _vm.CreateCommand, "note.delete" => _vm.DeleteCommand,
            "note.select" => _vm.SelectCommand,
            "note.previous" => _vm.PreviousCommand, "note.next" => _vm.NextCommand, "window.pin" => _vm.PinCommand,
            "app.hide" => new RelayCommand(HideResident), "app.show" => new RelayCommand(() => ShowResident("command")),
            "app.toggle" => new RelayCommand(() => Toggle()), "app.quit" => new RelayCommand(() => Quit()),
            "window.minimize" => new RelayCommand(Minimize),
            "settings.toggle" => new RelayCommand(ToggleSettings),
            "settings.material" => new ValueCommand<string>(SetMaterial), "settings.tint" => new ValueCommand<double>(SetTint),
            "settings.bars" => new ValueCommand<string>(SetBars), "settings.hotkey" => new ValueCommand<string>(g => SetHotkey(g)),
            "settings.hotkey-capture" => new ValueCommand<bool>(CaptureHotkey), "settings.startup" => new ValueCommand<bool>(SetStartup),
            "settings.font-size" => new ValueCommand<double>(SetFontSize), "settings.marker-dim" => new ValueCommand<double>(SetMarkerOpacity),
            "settings.strike" => new ValueCommand<double>(SetStrike),
            "theme.system" => new RelayCommand(() => SetTheme("system")), "theme.light" => new RelayCommand(() => SetTheme("light")),
            "theme.dark" => new RelayCommand(() => SetTheme("dark")),
            _ => throw new ArgumentException(id)
        };
    }
    // Settings is owned by the pad: minimising closes it rather than letting Windows re-show it on restore.
    private void Minimize() { CloseSettings(false); _surface.Minimize(); }
    public void SwitchShell(string id)
    {
        if (id != "quiet" || id == _surface.ShellId) return;
        if (!SaveNotes()) return;
        _surface.ApplyShell(id); _surface.SaveWindowSettings();
    }
    public void SetTheme(string preference) { _surface.ApplyTheme(preference); _surface.SaveWindowSettings(); }
}
