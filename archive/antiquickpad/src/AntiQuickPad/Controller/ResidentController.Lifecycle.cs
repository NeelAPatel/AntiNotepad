using System.ComponentModel;
using Avalonia.Input;
using Avalonia.Threading;
using AntiQuickPad.Shell;
using AntiQuickPad.ViewModels;

namespace AntiQuickPad.Controller;

internal sealed partial class ResidentController
{
    private ITrayService? _tray;
    private IHotkeyService? _hotkey;
    private bool _initialized;
    public bool Initialized => _initialized;
    public void InitializeViewModel()
    {
        _vm.OperationFailed += (_, ex) => { _surface.RefreshStatus(); _surface.Log($"note-operation-error {ex}"); };
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.Pinned)) SetPinned(_vm.Pinned);
            // Projections always change alongside a source change that already refreshes; skipping them keeps one refresh per change.
            if (MainViewModel.ProjectionNames.Contains(e.PropertyName)) return;
            _surface.RefreshStatus();
        };
        _vm.Status.PropertyChanged += (_, _) => _surface.RefreshStatus();
    }
    public void AttachEditor()
    {
        var editor = _surface.Editor;
        editor.TextChanged += (_, _) => { _surface.RefreshHint(); ScheduleSave(); };
        editor.TextArea.Caret.PositionChanged += (_, _) => ScheduleSave();
        editor.TextArea.GotFocus += (_, _) => CompleteFocus();
    }
    public void OnActivated() => Dispatcher.UIThread.Post(FocusEditor, DispatcherPriority.Input);
    public void OnClosing(CancelEventArgs e) { if (!_quitting) { e.Cancel = true; HideResident(); } }
    public void OnDeactivated() => _surface.CancelMotion();
    public void OnClosed()
    {
        _surface.CancelMotion(); _autosave.Stop(); _cue.Stop(); CloseSettings(false); _hotkey?.Dispose(); _tray?.Dispose(); _surface.DisposeAppearance();
        if (_quitting) _surface.Log($"quit-finish window-closed=true source={_quitSource}");
        _surface.DisposeLog();
    }
    public void OnOpened(bool rendering)
    {
        if (_initialized) return;
        _initialized = true;
        // The selftest does not claim the real hotkey or affect an existing resident app.
        if (!Program.SelfTest && !Program.QuitResidentTest && !rendering)
        {
            _hotkeysEnabled = true;
            InstallHotkey();
            SetupTray();
        }
        SyncStartup();
        StartInitialFocus();
        if (Measurement.IsChild) _surface.AttachMeasurement(_hotkey?.IsRegistered == true);
        Dispatcher.UIThread.Post(FocusEditor, DispatcherPriority.Input);
        if (Program.QuitResidentTest)
        {
            _surface.Editor.Text = "command quit saves this pending edit";
            _platform.Hide();
            File.WriteAllText(Path.Combine(Program.Paths.Root, "ready"), "ready");
        }
        if (Program.SelfTest) Dispatcher.UIThread.Post(async () => await _surface.RunSelfTest(), DispatcherPriority.Background);
    }

    private void InstallHotkey()
    {
        try
        {
            _hotkey = _surface.CreateHotkey(timestamp => Toggle(timestamp), EffectiveGesture);
            _surface.Log($"hotkey gesture={_hotkey.GestureLabel} registered={_hotkey.IsRegistered} error={_hotkey.RegistrationError} {_hotkey.RegistrationErrorMessage}");
        }
        catch (FormatException ex) { _surface.Log($"hotkey-configuration-error {ex.Message}"); }
    }

    private void SetPinned(bool pinned)
    {
        _platform.Pinned = pinned; _surface.RefreshPin(pinned);
        try { _surface.SaveWindowSettings(); }
        catch (Exception ex) { _surface.Log($"window-settings-save-error {ex}"); }
    }

    public void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (_surface.IsSettingsOpen) CloseSettings();
            else HideResident();
            e.Handled = true;
        }
        else if (e.Key == Key.N && e.KeyModifiers == KeyModifiers.Control) { _vm.CreateNote(); e.Handled = true; }
        else if (e.Key == Key.Delete && e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift)) { _vm.DeleteCurrent(); e.Handled = true; }
    }

    private void SetupTray()
    {
        _tray = _services.CreateTray();
        var hotkeyStatus = _hotkey is { IsRegistered: true } ? _hotkey.GestureLabel : "hotkey unavailable; see log";
        _tray.Initialize(() => Toggle(), () => Quit(), () => ShowResident("tray"), hotkeyStatus);
    }

}
