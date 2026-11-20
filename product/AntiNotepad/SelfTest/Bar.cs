// Capability checks for bottom-bar decisions and settings compatibility.
using System.Text.Json;
using AntiNotepad.Model;
using AntiNotepad.Theme;
using AntiNotepad.UI;

namespace AntiNotepad;

internal static partial class SelfTest
{
    private static bool BarChecks(out string report)
    {
        var stack = PadWindow.DecideBar(0, true, true, false, 2, false);
        var slot = PadWindow.DecideBar(3, true, true, true, 2, false);
        var blank = PadWindow.DecideBar(0, true, true, true, 1, true);
        var hidden = PadWindow.DecideBar(0, false, false, false, 2, false);
        var settings = JsonSerializer.Deserialize<Settings>("{}")!;
        settings.ShowDeleteButton = false;
        settings.SlotDots = "quiet";
        var round = JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(settings))!;
        var iconButton = Kit.IconButton(Glyph.Delete, "Delete", Palette.All[0], () => { }, hoverColor: Palette.All[0].Error);
        var face = (Avalonia.Controls.Border)iconButton.Content!;
        var enabledOpacity = face.Opacity * ((GlyphIcon)face.Child!).Opacity;
        iconButton.IsEnabled = false;
        var disabledOpacity = face.Opacity * ((GlyphIcon)face.Child!).Opacity;
        iconButton.IsEnabled = true;
        var restoredOpacity = face.Opacity * ((GlyphIcon)face.Child!).Opacity;
        var ok = enabledOpacity == 1 && disabledOpacity < enabledOpacity && restoredOpacity == enabledOpacity
            && !stack.Slots && stack.ShowNew && stack.NewEnabled && stack.DeleteEnabled
            && slot.Slots && !slot.ShowNew && !slot.DeleteEnabled && slot.DeleteName == "Clear slot 3"
            && !blank.DeleteEnabled && !blank.NewEnabled && !hidden.ShowDelete && !hidden.ShowNew
            && !round.ShowDeleteButton && round.ShowNewNoteButton && round.SlotDots == "quiet"
            && JsonSerializer.Deserialize<Settings>("{}")!.ShowDeleteButton
            && PadWindow.CanDeleteAt(1000, 1500) && !PadWindow.CanDeleteAt(1000, 1499)
            && PadWindow.SwapTarget(3, [false, true]) == 3 && PadWindow.SwapTarget(0, [false, true]) == 2
            && PadWindow.SwapTarget(0, [false, false]) == 1
            && PadWindow.SwapTarget(1, [true, true], 1) == 2
            && ChromeTokens.PagerCountButtonHeight == 32 && ChromeTokens.PagerDotButtonHeight == 32
            && ChromeTokens.BarCornerSize == ChromeTokens.PillHeight
            && ShortcutMap.Definitions.Any(d => d.Id == "swap-view" && d.DefaultKey == "");
        report = ok ? "bar decisions and settings" : "bar decisions or settings failed";
        return ok;
    }
}
