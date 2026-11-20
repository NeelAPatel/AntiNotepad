// Builds bar snapshots and supplies the window actions used by each PadBar.
using Avalonia.Controls;
using Avalonia.Media;
using AntiNotepad.Theme;

namespace AntiNotepad.UI;

internal sealed partial class PadWindow
{
    private Button? _swapButton;
    private int _swapIconSlot = -1;
    private long _lastBarDelete = long.MinValue;
    private int _lastSlot;

    /// <summary>Builds one immutable bar snapshot from the active pane and current settings.</summary>
    private BarContext BarContextFor(BarPane pane)
    {
        var activeSlot = _active.Note?.Slot ?? 0;
        var otherPaneSlot = !Settings.Split ? 0 : ReferenceEquals(_active, _left) ? Settings.SplitSlot : Data.ViewSlot;
        var isBlank = IsBlankDocument(_active.Editor.Document);
        return new BarContext(
            pane,
            activeSlot > 0 ? BarViewKind.Slot : BarViewKind.Stack,
            activeSlot,
            otherPaneSlot,
            Data.Stack.Count,
            Data.Current,
            isBlank,
            Data.Current == Data.Stack.Count - 1 && isBlank,
            Data.Slots.Select(note => new BarSlotContext(note.IsEmpty, note.Title)).ToArray(),
            Settings.ShowDeleteButton,
            Settings.ShowNewNoteButton,
            Settings.SlotDots,
            Settings.Bars switch
            {
                "overlay" => BarStyle.Overlay,
                "float" => BarStyle.Float,
                _ => BarStyle.Docked,
            },
            _palette,
            _shortcuts.GetLabel);
    }

    /// <summary>Connects bar controls to the existing window commands and their guards.</summary>
    private BarActions BarActionsFor(BarPane pane) => new(
        DeleteFromBar,
        NewNote,
        () => Navigate(-1),
        () => Navigate(1),
        () => OpenSearch(),
        slot => SelectBarSlot(pane, slot));

    /// <summary>Builds the persistent top-row button that switches the left pane's view.</summary>
    private void BuildSwapButton()
    {
        _swapButton = Kit.IconButton(Glyph.List, "Show slots", _palette, () => SwapView(BarPane.Left));
        _swapIconSlot = -1;
    }

    /// <summary>Refreshes the swap button's glyph and destination label for the left pane.</summary>
    private void UpdateSwapButton()
    {
        if (_swapButton?.Content is not Border face || _swapIconSlot == Data.ViewSlot) return;
        _swapIconSlot = Data.ViewSlot;
        var glyph = Data.ViewSlot == 0 ? Glyph.List : Glyph.Grid;
        face.Child = new GlyphIcon(glyph, _palette.Ink, ChromeTokens.IconGlyphSize, ChromeTokens.PlainIconWeight)
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        var destination = Data.ViewSlot == 0 ? "Show slots" : "Show stack";
        ToolTip.SetTip(_swapButton, ShortcutTooltip(destination, _shortcuts.GetLabel("swap-view")));
        Avalonia.Automation.AutomationProperties.SetName(_swapButton, destination);
    }

    /// <summary>Selects a slot while preserving the other split pane.</summary>
    private void SelectBarSlot(BarPane pane, int slot)
    {
        if (pane == BarPane.Left && Settings.Split && _active == _right)
        {
            if (slot == Data.ViewSlot) return;
            CycleSplitSlot(0, slot);
        }
        else OpenSlot(slot);
    }

    /// <summary>Deletes from the active pane with a repeat-click guard.</summary>
    private void DeleteFromBar()
    {
        var now = Environment.TickCount64;
        if (!CanDeleteAt(_lastBarDelete, now)) return;
        _lastBarDelete = now;
        DeleteCurrent();
        UpdateChrome();
        FocusActive();
    }

    /// <summary>Checks whether enough time has passed since the previous bar delete.</summary>
    internal static bool CanDeleteAt(long previous, long now) => previous == long.MinValue || now - previous >= ChromeTokens.DeleteDebounceMs;

    /// <summary>Chooses an available slot when switching views.</summary>
    internal static int SwapTarget(int lastSlot, bool[] filled, int occupiedSlot = 0)
    {
        if (lastSlot > 0 && lastSlot != occupiedSlot) return lastSlot;
        for (var i = 0; i < filled.Length; i++)
            if (i + 1 != occupiedSlot && filled[i]) return i + 1;
        return occupiedSlot == 1 ? 2 : 1;
    }

    /// <summary>Switches the requested pane between stack and slot views.</summary>
    private void SwapView(BarPane pane)
    {
        if (pane != BarPane.Left) return;
        if (Data.ViewSlot > 0) { _lastSlot = Data.ViewSlot; ShowStack(); return; }
        var slot = SwapTarget(_lastSlot, Data.Slots.Select(note => !note.IsEmpty).ToArray(), Settings.Split ? Settings.SplitSlot : 0);
        OpenSlot(slot);
    }
}
