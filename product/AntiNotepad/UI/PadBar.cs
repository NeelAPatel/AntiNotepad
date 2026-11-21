// Owns one pane's bottom bar, including its left, center (pager bar), and right containers.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AntiNotepad.Theme;

namespace AntiNotepad.UI;

/// <summary>Names the pane a bottom bar belongs to.</summary>
internal enum BarPane { Left, Right }

/// <summary>Names one of the bottom bar's left, center (pager bar), and right containers.</summary>
internal enum BarItemContainer { Left, Center, Right }

/// <summary>Names the content kind shown by a pane.</summary>
internal enum BarViewKind { Stack, Slot }

/// <summary>Names the placement style used by a bottom bar.</summary>
internal enum BarStyle { Docked, Overlay, Float }

/// <summary>Contains the display state for one permanent slot.</summary>
internal sealed record BarSlotContext(bool IsEmpty, Func<string> Title);

/// <summary>Describes the state and display labels needed to render one pane's bottom bar.</summary>
internal sealed record BarContext(
    BarPane Pane,
    BarViewKind ViewKind,
    int ViewSlot,
    int OtherPaneSlot,
    int StackCount,
    int CurrentIndex,
    bool IsBlank,
    bool IsTrailingBlank,
    IReadOnlyList<BarSlotContext> Slots,
    bool ShowDeleteButton,
    bool ShowNewNoteButton,
    string SlotDots,
    BarStyle Style,
    Palette Palette,
    Func<string, string> ShortcutLabel)
{
    /// <summary>Combines a tooltip description with its current shortcut label.</summary>
    internal string TooltipFor(string description, string actionId)
    {
        var keys = ShortcutLabel(actionId);
        return keys.Length == 0 ? description : $"{description} ({keys})";
    }
}

/// <summary>Registers an ordered control in one named bottom-bar container.</summary>
internal sealed record BarItem(
    string Id,
    BarItemContainer Container,
    int Order,
    Control Control,
    Func<BarContext, bool>? IsVisible = null,
    Func<BarContext, bool>? IsEnabled = null);

/// <summary>Supplies the window actions invoked by bottom-bar controls.</summary>
internal sealed record BarActions(
    Action Delete,
    Action NewNote,
    Action Previous,
    Action Next,
    Action OpenSearch,
    Action<int> SelectSlot);

/// <summary>Describes the content and enabled state selected for a bar context.</summary>
internal readonly record struct BarContent(bool Slots, bool ShowDelete, bool DeleteEnabled, bool ShowNew, bool NewEnabled, string DeleteName);

/// <summary>Owns one pane's three-container bottom bar and renders its registered items.</summary>
internal sealed class PadBar
{
    private readonly Border _root = new() { VerticalAlignment = VerticalAlignment.Bottom };
    private readonly Grid _groups = new();
    private readonly Border _barLeft = new() { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center };
    private readonly Border _barCenter = new() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
    private readonly Border _barRight = new() { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
    private readonly Border _pager = new() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0, 0, 0, ChromeTokens.PillBottomInset), CornerRadius = new CornerRadius(ChromeTokens.PillCornerRadius), Padding = new Thickness(ChromeTokens.PillInset) };
    private readonly List<BarItem> _items = [];
    private Button _deleteButton = null!;
    private Button _newButton = null!;
    private BarContext? _context;

    /// <summary>Creates the bottom-bar containers and registers its built-in controls.</summary>
    internal PadBar(BarPane pane, BarContext context, BarActions actions)
    {
        Pane = pane;
        _root.Child = _groups;
        _groups.Children.Add(_barLeft);
        _groups.Children.Add(_barCenter); Grid.SetColumn(_barCenter, 1);
        _groups.Children.Add(_barRight); Grid.SetColumn(_barRight, 2);
        _barCenter.Child = _pager;
        _pager.Margin = default;
        Rebuild(context, actions);
    }

    /// <summary>Identifies the pane this bar belongs to.</summary>
    internal BarPane Pane { get; }

    /// <summary>Returns the visual root placed by the window.</summary>
    internal Control Root => _root;

    /// <summary>Returns the three named containers for placement and capability checks.</summary>
    internal IReadOnlyList<Border> Containers => [_barLeft, _barCenter, _barRight];

    /// <summary>Returns the center container's pager bar.</summary>
    internal Border Pager => _pager;

    /// <summary>Returns the controls in registration order for selftests and layout checks.</summary>
    internal IReadOnlyList<BarItem> Items => _items.OrderBy(item => item.Container).ThenBy(item => item.Order).ToArray();

    /// <summary>Chooses bar content without reading window or note state.</summary>
    internal static BarContent DecideBar(BarContext context) => new(
        context.ViewKind == BarViewKind.Slot,
        context.ShowDeleteButton,
        !context.IsBlank || context.ViewKind == BarViewKind.Stack && context.StackCount > 1,
        context.ShowNewNoteButton && context.ViewKind == BarViewKind.Stack,
        context.ViewKind == BarViewKind.Stack && !context.IsTrailingBlank,
        context.ViewKind == BarViewKind.Slot ? $"Clear slot {context.ViewSlot}" : "Delete note");

    /// <summary>Recreates palette-bound controls while preserving any additional registered items.</summary>
    internal void Rebuild(BarContext context, BarActions actions)
    {
        var size = context.Style == BarStyle.Float ? ChromeTokens.ButtonSize : ChromeTokens.BarCornerSize;
        _deleteButton = Kit.IconButton(Icons.Delete, "Delete note", context.Palette, actions.Delete, size: size, hoverColor: context.Palette.Error);
        _newButton = Kit.IconButton(Icons.NewNote, context.TooltipFor("New note", "new-note"), context.Palette, actions.NewNote, size: size, hoverColor: context.Palette.Accent);
        ReplaceItem(new("delete", BarItemContainer.Left, 0, _deleteButton,
            static state => state.ShowDeleteButton,
            static state => !state.IsBlank || state.ViewKind == BarViewKind.Stack && state.StackCount > 1));
        ReplaceItem(new("pager", BarItemContainer.Center, 0, _pager));
        ReplaceItem(new("new-note", BarItemContainer.Right, 0, _newButton,
            static state => state.ShowNewNoteButton && state.ViewKind == BarViewKind.Stack,
            static state => state.ViewKind == BarViewKind.Stack && !state.IsTrailingBlank));
        LayoutItems();
        ConfigureBarStyle(context);
        Update(context, actions);
    }

    /// <summary>Updates bar decoration, item state, and pager content from the supplied snapshot.</summary>
    internal void Update(BarContext context, BarActions actions)
    {
        _context = context;
        ApplyItemState(context);

        var p = context.Palette;
        var state = DecideBar(context);
        var floating = context.Style == BarStyle.Float;
        _deleteButton.IsVisible = state.ShowDelete;
        _deleteButton.IsEnabled = state.DeleteEnabled;
        var deleteName = context.TooltipFor(state.DeleteName, "delete-note");
        ToolTip.SetTip(_deleteButton, deleteName);
        Avalonia.Automation.AutomationProperties.SetName(_deleteButton, state.DeleteName);
        _newButton.IsVisible = state.ShowNew;
        _newButton.IsEnabled = state.NewEnabled;
        _barLeft.Width = floating ? (state.ShowDelete ? ChromeTokens.ButtonSize : default) : ChromeTokens.BarCornerSize;
        _barRight.Width = floating ? (context.ShowNewNoteButton ? ChromeTokens.ButtonSize : default) : ChromeTokens.BarCornerSize;
        _pager.Background = floating || state.Slots && context.SlotDots == "quiet" ? Brushes.Transparent : new SolidColorBrush(p.Surface);
        _pager.BorderBrush = new SolidColorBrush(p.Faint, ChromeTokens.PillBorderAlpha);
        _pager.BorderThickness = new Thickness(floating || state.Slots && context.SlotDots == "quiet" ? default : ChromeTokens.PillBorderThickness);
        _pager.BoxShadow = floating || state.Slots && context.SlotDots == "quiet" ? default : PillShadow(p);
        _pager.Padding = new Thickness(floating ? default : ChromeTokens.PillInset);
        BuildPager(context, actions, state);
    }

    /// <summary>Adds another ordered control to one of the three named containers.</summary>
    internal void RegisterItem(BarItem item)
    {
        if (_items.Any(existing => existing.Id == item.Id)) throw new InvalidOperationException($"Bar item '{item.Id}' is already registered.");
        _items.Add(item);
        LayoutItems();
        if (_context is { } context) ApplyItemState(context);
    }

    /// <summary>Creates the shared palette shadow used by bottom-bar pills.</summary>
    internal static BoxShadows PillShadow(Palette p) => new(new BoxShadow
    {
        Blur = ChromeTokens.PillShadowBlur,
        OffsetY = ChromeTokens.PillShadowOffsetY,
        Color = Color.FromArgb(p.Dark ? ChromeTokens.PillShadowDarkAlpha : ChromeTokens.PillShadowLightAlpha, 0, 0, 0),
    });

    /// <summary>Applies placement and geometry from the supplied style snapshot.</summary>
    internal void ConfigureBarStyle(BarContext context)
    {
        var floating = context.Style == BarStyle.Float;
        _root.HorizontalAlignment = floating ? HorizontalAlignment.Center : HorizontalAlignment.Stretch;
        _root.Margin = new Thickness(floating ? default : ChromeTokens.BarEdgeInset, default, floating ? default : ChromeTokens.BarEdgeInset, floating ? default : ChromeTokens.PillBottomInset);
        _groups.ColumnDefinitions = new ColumnDefinitions(floating ? "Auto,Auto,Auto" : "*,Auto,*");
        _root.CornerRadius = new CornerRadius(ChromeTokens.PillCornerRadius);
        _root.Padding = floating ? new Thickness(ChromeTokens.PillInset) : default;
        _root.Background = floating ? new SolidColorBrush(context.Palette.Surface) : null;
        _root.BorderBrush = floating ? new SolidColorBrush(context.Palette.Faint, ChromeTokens.PillBorderAlpha) : null;
        _root.BorderThickness = new Thickness(floating ? ChromeTokens.PillBorderThickness : default);
        _root.BoxShadow = floating ? PillShadow(context.Palette) : default;
        if (_deleteButton is not null) _deleteButton.Width = _deleteButton.Height = floating ? ChromeTokens.ButtonSize : ChromeTokens.BarCornerSize;
        if (_newButton is not null) _newButton.Width = _newButton.Height = floating ? ChromeTokens.ButtonSize : ChromeTokens.BarCornerSize;
    }

    /// <summary>Rebuilds the center pager while keeping corner controls registered.</summary>
    private void BuildPager(BarContext context, BarActions actions, BarContent state)
    {
        var p = context.Palette;
        var floating = context.Style == BarStyle.Float;
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = ChromeTokens.PagerRowSpacing, VerticalAlignment = VerticalAlignment.Center };
        if (floating && state.ShowDelete) row.Children.Add(BarSeparator(p));
        if (!state.Slots)
        {
            row.Children.Add(Kit.IconButton(Icons.Back, context.TooltipFor("Previous note", "previous-note"), p, actions.Previous, radius: ChromeTokens.PagerArrowHoverRadius));
            var count = Kit.Text($"{context.CurrentIndex + 1} / {context.StackCount}", p.Muted, ChromeTokens.PagerCountFontSize, Typography.Strong);
            count.TextAlignment = TextAlignment.Center; count.HorizontalAlignment = HorizontalAlignment.Center; count.VerticalAlignment = VerticalAlignment.Center;
            count.FontFeatures = new FontFeatureCollection { new FontFeature { Tag = "tnum", Value = 1 } };
            var digits = Math.Max(context.StackCount.ToString().Length, 1);
            var countButton = Kit.Plain(count, p, ChromeTokens.PagerCountBaseWidth + digits * ChromeTokens.PagerCountDigitWidth, height: ChromeTokens.PagerCountButtonHeight, radius: ChromeTokens.PagerCountHoverRadius);
            countButton.Click += (_, _) => actions.OpenSearch();
            Avalonia.Automation.AutomationProperties.SetName(countButton, $"Note {context.CurrentIndex + 1} of {context.StackCount}");
            row.Children.Add(countButton);
            row.Children.Add(Kit.IconButton(Icons.Forward, context.TooltipFor("Next note · past the last one starts a new note", "next-note"), p, actions.Next, radius: ChromeTokens.PagerArrowHoverRadius));
        }
        else
        {
            var dots = new StackPanel { Orientation = Orientation.Horizontal };
            for (var i = 1; i <= 9; i++)
            {
                var target = i;
                var slot = context.Slots[i - 1];
                var filled = !slot.IsEmpty;
                var current = context.ViewSlot == i || context.OtherPaneSlot == i;
                var dot = new Border { Width = current ? ChromeTokens.PagerDotSelectedWidth : ChromeTokens.PagerDotWidth, Height = ChromeTokens.PagerDotHeight, CornerRadius = new CornerRadius(ChromeTokens.PagerDotRadius), Background = current ? new SolidColorBrush(p.Accent) : filled ? new SolidColorBrush(p.Muted) : Brushes.Transparent, BorderBrush = new SolidColorBrush(current ? p.Accent : p.Faint), BorderThickness = new Thickness(filled || current ? default : ChromeTokens.PagerDotBorderThickness), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                var button = Kit.Plain(dot, p, ChromeTokens.PagerDotTargetWidth, ChromeTokens.PagerDotButtonHeight, ChromeTokens.PagerDotHoverRadius);
                button.Click += (_, _) => actions.SelectSlot(target);
                ToolTip.SetTip(button, context.TooltipFor($"Slot {i} · {(filled ? slot.Title() : "empty")}", "open-slot"));
                Avalonia.Automation.AutomationProperties.SetName(button, $"Slot {i}");
                dots.Children.Add(button);
            }
            row.Children.Add(dots);
        }
        if (floating && context.ShowNewNoteButton) row.Children.Add(BarSeparator(p));
        _pager.Child = row;
    }

    /// <summary>Applies optional visibility and enabled predicates to registered controls.</summary>
    private void ApplyItemState(BarContext context)
    {
        foreach (var item in _items)
        {
            if (item.IsVisible is { } isVisible) item.Control.IsVisible = isVisible(context);
            if (item.IsEnabled is { } isEnabled) item.Control.IsEnabled = isEnabled(context);
        }
    }

    /// <summary>Places items in container and order sequence without adding a wrapper for a single item.</summary>
    private void LayoutItems()
    {
        foreach (var container in Containers)
        {
            var items = _items.Where(item => ReferenceEquals(ContainerFor(item.Container), container))
                .OrderBy(item => item.Container).ThenBy(item => item.Order).Select(item => item.Control).ToArray();
            container.Child = null;
            foreach (var control in items) DetachControl(control);
            container.Child = items.Length switch
            {
                0 => null,
                1 => items[0],
                _ => ContainerRow(items),
            };
        }
    }

    /// <summary>Creates a horizontal row when a named container has multiple registered controls.</summary>
    private static StackPanel ContainerRow(IEnumerable<Control> controls)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var control in controls) row.Children.Add(control);
        return row;
    }

    /// <summary>Removes a registered control from its current visual parent before ordering it again.</summary>
    private static void DetachControl(Control control)
    {
        if (control.Parent is Panel panel) panel.Children.Remove(control);
    }

    /// <summary>Returns the visual container named by a registered item.</summary>
    private Border ContainerFor(BarItemContainer container) => container switch
    {
        BarItemContainer.Left => _barLeft,
        BarItemContainer.Center => _barCenter,
        BarItemContainer.Right => _barRight,
        _ => throw new ArgumentOutOfRangeException(nameof(container)),
    };

    /// <summary>Replaces an existing built-in registration without disturbing added items.</summary>
    private void ReplaceItem(BarItem item)
    {
        var index = _items.FindIndex(existing => existing.Id == item.Id);
        if (index < 0) _items.Add(item);
        else _items[index] = item;
    }

    /// <summary>Creates a separator inside the floating center pill.</summary>
    private static Border BarSeparator(Palette p) => new()
    {
        Width = ChromeTokens.PagerSeparatorWidth,
        Height = ChromeTokens.PagerSeparatorHeight,
        Background = new SolidColorBrush(p.Faint, ChromeTokens.PagerSeparatorAlpha),
        Margin = new Thickness(ChromeTokens.PagerSeparatorMargin, default),
    };
}
