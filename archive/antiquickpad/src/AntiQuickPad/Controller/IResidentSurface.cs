using AvaloniaEdit;
using AntiQuickPad.Shell;

namespace AntiQuickPad.Controller;

internal interface IResidentSurface
{
    // The current editor follows appearance rebuilds.
    TextEditor Editor { get; }
    // Create a hotkey bound to the native host without exposing the window.
    IHotkeyService CreateHotkey(Action<long> pressed, string? gesture);
    // Attach the existing interactive measurement harness to the host.
    void AttachMeasurement(bool registered);
    // Run the host's retained capability smoke suite.
    Task RunSelfTest();
    // Release the shell's theme subscription on close.
    void DisposeAppearance();
    // Dispose the log after the final quit line.
    void DisposeLog();
    // Reflect pin state in the shell control.
    void RefreshPin(bool pinned);
    // Apply the native minimized window state.
    void Minimize();
    // Identify the currently rendered shell.
    string ShellId { get; }
    // Rebuild the visual shell while preserving the editor document.
    void ApplyShell(string id);
    // Apply the theme through the shell's presentation service.
    void ApplyTheme(string preference);
    // Inspect a toolbar, settings or visual gesture route without exposing controls.
    bool HasPresentationRoute(string kind, string label, string commandId);
    // Visibility guards resident focus and hide transitions.
    bool IsVisible { get; }
    // Activation is required by interactive measurement.
    bool IsActive { get; }
    // Report keyboard focus inside the editor text area.
    bool IsKeyboardFocusWithin { get; }
    // Check the native foreground window for interactive measurement.
    bool IsForeground { get; }
    // Start the existing window frame measurement callback.
    void RecordFrame();
    // Apply the shell's active or resting focus-cue brush.
    void SetFocusCue(bool active);
    // Close the native host only after a successful quit save.
    void Close();
    // Stop any current carousel movement.
    void CancelMotion();
    // Recompute empty-editor hint visibility.
    void RefreshHint();
    // Refresh the shell's status projection.
    void RefreshStatus();
    // Capture and persist current window settings.
    void SaveWindowSettings();
    // M11a-settings-function: the settings window is owned by the pad and closes with it.
    bool IsSettingsOpen { get; }
    void OpenSettings();
    void CloseSettings();
    // Re-apply preferences (material, tint, bars, fonts) to the presentation.
    void ApplyPreferences();
    // Append the resident diagnostic line.
    void Log(string message);
}
