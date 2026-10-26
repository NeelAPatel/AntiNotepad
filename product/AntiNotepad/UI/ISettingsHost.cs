using Avalonia.Controls;
using AntiNotepad.Theme;

namespace AntiNotepad.UI;

internal interface ISettingsHost
{
    Palette CurrentPalette { get; }
    WindowIcon? Icon { get; }
    string HotkeyStatus { get; }
    string NewNoteHotkeyStatus { get; }
    bool RegisterHotkey(string? gesture);
    bool RegisterNewNoteHotkey(string? gesture);
    void Toast(string text, string? action = null, Action? onAction = null, bool brief = false);
    void ShowResident();
    void OpenHelp();
    void ResetSettings();
    bool Save();
    void SettingsLostFocus();
}
