// Builds the bottom bar and its left, center (pager bar), and right containers.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AntiNotepad.Theme;

namespace AntiNotepad.UI;

internal sealed partial class PadWindow
{
    private readonly Border _bar = new() { VerticalAlignment = VerticalAlignment.Bottom };
    private readonly Grid _barGroups = new();
    private readonly Border _barLeft = new() { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center };
    private readonly Border _barCenter = new() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
    private readonly Border _barRight = new() { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
    private Button? _deleteButton, _newButton, _swapButton;
    private int _swapIconSlot = -1;
    private long _lastBarDelete = long.MinValue;
    private int _lastSlot;

    /// <summary>Describes the controls and enabled states for the active pane.</summary>
    internal readonly record struct BarContent(bool Slots, bool ShowDelete, bool DeleteEnabled, bool ShowNew, bool NewEnabled, string DeleteName);

    /// <summary>Describes the active pane's bar without constructing controls.</summary>
    internal static BarContent DecideBar(int slot, bool showDelete, bool showNew, bool empty, int stackCount, bool trailingBlank) =>
        new(slot > 0, showDelete, !empty || slot == 0 && stackCount > 1, showNew && slot == 0,
            slot == 0 && !trailingBlank, slot > 0 ? $"Clear slot {slot}" : "Delete note");

    /// <summary>Connects the bottom bar containers and pager.</summary>
    private void InitializeBar()
    {
        _bar.Child = _barGroups;
        _barGroups.Children.Add(_barLeft);
        _barGroups.Children.Add(_barCenter); Grid.SetColumn(_barCenter, 1);
        _barGroups.Children.Add(_barRight); Grid.SetColumn(_barRight, 2);
        _barCenter.Child = _pill;
        _pill.Margin = default;
    }

    /// <summary>Creates the persistent bottom bar and view swap buttons.</summary>
    private void BuildBarButtons()
    {
        var size = Settings.Bars == "float" ? ChromeTokens.ButtonSize : ChromeTokens.BarCornerSize;
        _deleteButton = Kit.IconButton(Glyph.Delete, "Delete note", _palette, DeleteFromBar, size: size, hoverColor: _palette.Error);
        _newButton = Kit.IconButton(Glyph.Add, ShortcutTooltip("New note", _shortcuts.GetLabel("new-note")), _palette, NewNote, size: size, hoverColor: _palette.Accent);
        _barLeft.Child = _deleteButton;
        _barRight.Child = _newButton;
        _swapButton = Kit.IconButton(Glyph.List, "Show slots", _palette, SwapView);
        _swapIconSlot = -1;
    }

    /// <summary>Creates the shared palette shadow for floating chrome.</summary>
    private static BoxShadows PillShadow(Palette p) => new(new BoxShadow
    {
        Blur = ChromeTokens.PillShadowBlur,
        OffsetY = ChromeTokens.PillShadowOffsetY,
        Color = Color.FromArgb(p.Dark ? ChromeTokens.PillShadowDarkAlpha : ChromeTokens.PillShadowLightAlpha, 0, 0, 0),
    });

    /// <summary>Applies the current bar placement and decoration.</summary>
    private void ConfigureBarStyle()
    {
        var floating = Settings.Bars == "float";
        _bar.HorizontalAlignment = floating ? HorizontalAlignment.Center : HorizontalAlignment.Stretch;
        _bar.Margin = new Thickness(floating ? default : ChromeTokens.BarEdgeInset, default, floating ? default : ChromeTokens.BarEdgeInset, floating ? default : ChromeTokens.PillBottomInset);
        _barGroups.ColumnDefinitions = new ColumnDefinitions(floating ? "Auto,Auto,Auto" : "*,Auto,*");
        _bar.CornerRadius = new CornerRadius(ChromeTokens.PillCornerRadius);
        _bar.Padding = floating ? new Thickness(ChromeTokens.PillInset) : default;
        _bar.Background = floating ? new SolidColorBrush(_palette.Surface) : null;
        _bar.BorderBrush = floating ? new SolidColorBrush(_palette.Faint, ChromeTokens.PillBorderAlpha) : null;
        _bar.BorderThickness = new Thickness(floating ? ChromeTokens.PillBorderThickness : default);
        _bar.BoxShadow = floating ? PillShadow(_palette) : default;
        if (_deleteButton is not null) _deleteButton.Width = _deleteButton.Height = floating ? ChromeTokens.ButtonSize : ChromeTokens.BarCornerSize;
        if (_newButton is not null) _newButton.Width = _newButton.Height = floating ? ChromeTokens.ButtonSize : ChromeTokens.BarCornerSize;
    }

    /// <summary>Refreshes pager content and actions for the active pane.</summary>
    private void BuildPill()
    {
        var p = _palette;
        var slot = _active.Note?.Slot ?? 0;
        var empty = IsBlankDocument(_active.Editor.Document);
        var state = DecideBar(slot, Settings.ShowDeleteButton, Settings.ShowNewNoteButton, empty,
            Data.Stack.Count, Data.Current == Data.Stack.Count - 1 && empty);
        var floating = Settings.Bars == "float";
        if (_deleteButton is not null)
        {
            _deleteButton.IsVisible = state.ShowDelete;
            _deleteButton.IsEnabled = state.DeleteEnabled;
            var name = ShortcutTooltip(state.DeleteName, _shortcuts.GetLabel("delete-note"));
            ToolTip.SetTip(_deleteButton, name);
            Avalonia.Automation.AutomationProperties.SetName(_deleteButton, state.DeleteName);
        }
        if (_newButton is not null) { _newButton.IsVisible = state.ShowNew; _newButton.IsEnabled = state.NewEnabled; }
        _barLeft.Width = floating ? (state.ShowDelete ? ChromeTokens.ButtonSize : default) : ChromeTokens.BarCornerSize;
        _barRight.Width = floating ? (Settings.ShowNewNoteButton ? ChromeTokens.ButtonSize : default) : ChromeTokens.BarCornerSize;
        _pill.Background = floating || state.Slots && Settings.SlotDots == "quiet" ? Brushes.Transparent : new SolidColorBrush(p.Surface);
        _pill.BorderThickness = new Thickness(floating || state.Slots && Settings.SlotDots == "quiet" ? default : ChromeTokens.PillBorderThickness);
        _pill.BoxShadow = floating || state.Slots && Settings.SlotDots == "quiet" ? default : PillShadow(p);
        _pill.Padding = new Thickness(floating ? default : ChromeTokens.PillInset);
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = ChromeTokens.PagerRowSpacing, VerticalAlignment = VerticalAlignment.Center };
        if (floating && state.ShowDelete) row.Children.Add(BarSeparator(p));
        if (!state.Slots)
        {
            row.Children.Add(Kit.IconButton(Glyph.Back, ShortcutTooltip("Previous note", _shortcuts.GetLabel("previous-note")), p, () => Navigate(-1), radius: ChromeTokens.PagerArrowHoverRadius));
            var count = Kit.Text($"{Data.Current + 1} / {Data.Stack.Count}", p.Muted, ChromeTokens.PagerCountFontSize, Typography.Strong);
            count.TextAlignment = TextAlignment.Center; count.HorizontalAlignment = HorizontalAlignment.Center; count.VerticalAlignment = VerticalAlignment.Center;
            count.FontFeatures = new FontFeatureCollection { new FontFeature { Tag = "tnum", Value = 1 } };
            var digits = Math.Max(Data.Stack.Count.ToString().Length, 1);
            var countButton = Kit.Plain(count, p, width: ChromeTokens.PagerCountBaseWidth + digits * ChromeTokens.PagerCountDigitWidth, height: ChromeTokens.PagerCountButtonHeight, radius: ChromeTokens.PagerCountHoverRadius);
            countButton.Click += (_, _) => OpenSearch();
            Avalonia.Automation.AutomationProperties.SetName(countButton, $"Note {Data.Current + 1} of {Data.Stack.Count}");
            row.Children.Add(countButton);
            row.Children.Add(Kit.IconButton(Glyph.Forward, ShortcutTooltip("Next note · past the last one starts a new note", _shortcuts.GetLabel("next-note")), p, () => Navigate(1), radius: ChromeTokens.PagerArrowHoverRadius));
        }
        else
        {
            for (var i = 1; i <= 9; i++)
            {
                var target = i;
                var filled = !Data.Slots[i - 1].IsEmpty;
                var current = Data.ViewSlot == i || (Settings.Split && Settings.SplitSlot == i);
                var dot = new Border { Width = current ? ChromeTokens.PagerDotSelectedWidth : ChromeTokens.PagerDotWidth, Height = ChromeTokens.PagerDotHeight, CornerRadius = new CornerRadius(ChromeTokens.PagerDotRadius), Background = current ? new SolidColorBrush(p.Accent) : filled ? new SolidColorBrush(p.Muted) : Brushes.Transparent, BorderBrush = new SolidColorBrush(current ? p.Accent : p.Faint), BorderThickness = new Thickness(filled || current ? default : ChromeTokens.PagerDotBorderThickness), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                var button = Kit.Plain(dot, p, current ? ChromeTokens.PagerDotSelectedButtonWidth : ChromeTokens.PagerDotButtonWidth, ChromeTokens.PagerDotButtonHeight, ChromeTokens.PagerDotHoverRadius);
                button.Click += (_, _) => SelectBarSlot(target);
                ToolTip.SetTip(button, ShortcutTooltip($"Slot {i} · {(filled ? Data.Slots[i - 1].Title : "empty")}", _shortcuts.GetLabel("open-slot")));
                Avalonia.Automation.AutomationProperties.SetName(button, $"Slot {i}");
                row.Children.Add(button);
            }
        }
        if (floating && Settings.ShowNewNoteButton) row.Children.Add(BarSeparator(p));
        _pill.Child = row;
        if (_swapButton?.Content is Border face && _swapIconSlot != Data.ViewSlot)
        {
            _swapIconSlot = Data.ViewSlot;
            var glyph = Data.ViewSlot == 0 ? Glyph.List : Glyph.Grid;
            face.Child = new GlyphIcon(glyph, p.Ink, ChromeTokens.IconGlyphSize, ChromeTokens.PlainIconWeight) { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            var destination = Data.ViewSlot == 0 ? "Show slots" : "Show stack";
            ToolTip.SetTip(_swapButton, ShortcutTooltip(destination, _shortcuts.GetLabel("swap-view")));
            Avalonia.Automation.AutomationProperties.SetName(_swapButton, destination);
        }
    }

    private static Border BarSeparator(AntiNotepad.Theme.Palette p) => new()
    {
        Width = ChromeTokens.PagerSeparatorWidth,
        Height = ChromeTokens.PagerSeparatorHeight,
        Background = new SolidColorBrush(p.Faint, ChromeTokens.PagerSeparatorAlpha),
        Margin = new Thickness(ChromeTokens.PagerSeparatorMargin, default),
    };

    /// <summary>Selects a slot while preserving the other split pane.</summary>
    private void SelectBarSlot(int slot)
    {
        if (Settings.Split && _active == _right)
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

    internal static bool CanDeleteAt(long previous, long now) => previous == long.MinValue || now - previous >= ChromeTokens.DeleteDebounceMs;

    /// <summary>Chooses an available slot when switching views.</summary>
    internal static int SwapTarget(int lastSlot, bool[] filled, int occupiedSlot = 0)
    {
        if (lastSlot > 0 && lastSlot != occupiedSlot) return lastSlot;
        for (var i = 0; i < filled.Length; i++)
            if (i + 1 != occupiedSlot && filled[i]) return i + 1;
        return occupiedSlot == 1 ? 2 : 1;
    }

    /// <summary>Switches the left pane between stack and slot views.</summary>
    private void SwapView()
    {
        if (Data.ViewSlot > 0) { _lastSlot = Data.ViewSlot; ShowStack(); return; }
        var slot = SwapTarget(_lastSlot, Data.Slots.Select(note => !note.IsEmpty).ToArray(), Settings.Split ? Settings.SplitSlot : 0);
        OpenSlot(slot);
    }
}
