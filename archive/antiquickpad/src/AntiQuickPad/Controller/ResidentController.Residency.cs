using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using AntiQuickPad.Shell;

namespace AntiQuickPad.Controller;

internal sealed partial class ResidentController
{
    private readonly DispatcherTimer _cue = new() { Interval = TimeSpan.FromMilliseconds(130) };
    private bool _quitting;
    private long _showStarted;
    private string _quitSource = "tray", _showSource = "startup";
    public bool Quitting => _quitting;
    public TaskCompletionSource<double>? FocusCompletion { get; set; }
    public double LastShowToFocusMs { get; private set; }
    public void InitializeCue() => _cue.Tick += (_, _) => { _cue.Stop(); _surface.SetFocusCue(false); };
    public void StartInitialFocus() => _showStarted = Stopwatch.GetTimestamp();
    public void Toggle(long started = 0)
    {
        Measurement.Event("hotkey", started);
        if (_surface.IsVisible) HideResident(); else ShowResident("toggle", started);
    }

    public void ShowResident(string source, long started = 0)
    {
        _showStarted = started == 0 ? Stopwatch.GetTimestamp() : started; _showSource = source;
        _platform.Show(); _platform.Activate();
        _surface.RecordFrame();
        _surface.SetFocusCue(true); _cue.Stop(); _cue.Start();
        FocusEditor();
        Dispatcher.UIThread.Post(FocusEditor, DispatcherPriority.Input);
    }

    public void HideResident()
    {
        _surface.CancelMotion();
        if (!SaveNotes()) return;
        CloseSettings(false);
        _showStarted = 0; _platform.Hide(); Measurement.Event("hidden"); _surface.Log("hidden resident=true");
    }

    public void FocusEditor()
    {
        if (!_surface.IsVisible) return;
        _platform.Activate(); _surface.Editor.TextArea.Focus(); CompleteFocus();
    }

    public void CompleteFocus()
    {
        if (_showStarted == 0 || !_surface.IsVisible || !_surface.IsKeyboardFocusWithin) return;
        if (Measurement.IsChild && (!_surface.IsActive || !_surface.IsForeground)) return;
        Measurement.Event("focused");
        LastShowToFocusMs = Stopwatch.GetElapsedTime(_showStarted).TotalMilliseconds;
        _showStarted = 0;
        _surface.Log($"show-to-focus source={_showSource} ms={LastShowToFocusMs:F3} focused=true");
        FocusCompletion?.TrySetResult(LastShowToFocusMs);
    }

    public void Quit(int exitCode = 0, string source = "tray")
    {
        _quitSource = source;
        _surface.Log($"quit-start source={source}");
        _surface.CancelMotion();
        if (!SaveNotes())
        {
            _surface.Log($"quit-aborted save-failed reason={_lastSaveError}");
            if (source != "command") ShowResident("quit-save-failed");
            _surface.RefreshStatus();
            return;
        }
        CloseSettings(false);
        _quitting = true; _surface.Close();
        (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.Shutdown(exitCode);
    }

}
