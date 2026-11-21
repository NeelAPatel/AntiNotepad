using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using AntiNotepad.Editor;
using AntiNotepad.Model;
using AntiNotepad.Platform;
using AntiNotepad.Theme;

namespace AntiNotepad.UI;

/// <summary>The right-click menu: a Windows-style popup with Fluent icons and dimmed shortcuts.</summary>
internal static class PadMenu
{
    private static readonly FontFamily MenuFont = new(Typography.UiFamily);
    private static readonly string[] ActionLabels = ["Cut", "Copy", "Paste", "Delete"];

    public static void Show(Control anchor, IReadOnlyList<MenuEntry> entries, Palette p, Settings settings)
    {
        Popup? popup = null;
        var content = BuildMenu(entries, p, settings, () => { if (popup is not null) popup.IsOpen = false; });
        TextOptions.SetTextRenderingMode(content, TextRenderingMode.Antialias);
        popup = new Popup
        {
            PlacementTarget = anchor,
            Placement = PlacementMode.Pointer,
            IsLightDismissEnabled = true,
            WindowManagerAddShadowHint = false,
            Child = content,
        };
        TextOptions.SetTextRenderingMode(popup, TextRenderingMode.Antialias);
        popup.Opened += (_, _) =>
        {
            ConfigurePopup(content);
            content.Focus();
        };
        content.KeyDown += (_, e) =>
        {
            if (e.Key != Key.Escape) return;
            popup.IsOpen = false;
            e.Handled = true;
        };
        popup.IsOpen = true;
    }

    private static Border BuildMenu(IReadOnlyList<MenuEntry> entries, Palette p, Settings settings, Action close)
    {
        var content = new StackPanel();
        var submenus = new Dictionary<Control, Popup?>();
        Popup? openSubmenu = null;
        void Activate(Popup? submenu)
        {
            if (openSubmenu != submenu && openSubmenu is not null) openSubmenu.IsOpen = false;
            openSubmenu = submenu;
        }
        Grid.SetIsSharedSizeScope(content, true);
        if (settings.MenuActionRow)
        {
            var actionRow = BuildActionRow(entries, p, settings.MenuColorIcons, close);
            submenus.Add(actionRow, null);
            content.Children.Add(actionRow);
            content.Children.Add(Separator(p));
        }

        var lastWasSeparator = true;
        foreach (var entry in entries)
        {
            if (settings.MenuActionRow && entry.Header is not null && ActionLabels.Contains(entry.Header))
                continue;
            if (entry.Header is null)
            {
                if (!lastWasSeparator) content.Children.Add(Separator(p));
                lastWasSeparator = true;
                continue;
            }
            var row = BuildMenuRow(entry, p, settings.MenuColorIcons, close, Activate, out var submenu);
            submenus.Add(row, submenu);
            content.Children.Add(row);
            lastWasSeparator = false;
        }

        TrackHoveredRow(content, submenus, Activate);
        return MenuBorder(content, p);
    }

    /// <summary>Gives each menu popup the same transparent native window.</summary>
    private static void ConfigurePopup(Control content)
    {
        if (TopLevel.GetTopLevel(content) is not { } host) return;
        host.TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
        host.Background = Brushes.Transparent;
        DesktopPlatform.ConfigurePopupWindow(host);
    }

    /// <summary>Wraps menu rows in the shared popup surface and shadow.</summary>
    private static Border MenuBorder(Control content, Palette p) => new()
    {
        HorizontalAlignment = HorizontalAlignment.Left,
        Padding = new Thickness(4),
        CornerRadius = new CornerRadius(Shape.Popup),
        Background = new SolidColorBrush(p.Surface),
        BorderBrush = new SolidColorBrush(p.Faint, Paint.PopupBorder),
        BorderThickness = new Thickness(1),
        BoxShadow = new BoxShadows(new BoxShadow
        {
            Blur = Shape.MenuShadowBlur,
            OffsetY = Shape.PopupShadowOffsetY,
            Color = Color.FromArgb(p.Dark ? Shape.PopupShadowDarkAlpha : Shape.PopupShadowLightAlpha, 0, 0, 0),
        }),
        Child = content,
        Focusable = true,
    };

    private static Control BuildActionRow(IReadOnlyList<MenuEntry> entries, Palette p, bool colored, Action close)
    {
        var actions = ActionLabels.Select(label => entries.FirstOrDefault(e => e.Header == label)).Where(e => e is not null).Cast<MenuEntry>().ToArray();
        var row = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*,*,*"), MinWidth = 224 };
        for (var i = 0; i < actions.Length; i++)
        {
            var entry = actions[i];
            var face = new StackPanel
            {
                Spacing = 2,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            face.Children.Add(Icon(entry.Icon, p, colored));
            face.Children.Add(new TextBlock
            {
                Text = entry.Header,
                FontFamily = MenuFont,
                FontSize = Typography.Large,
                Foreground = new SolidColorBrush(p.Ink),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
            });
            var hit = MenuButton(face, p, entry.Enabled, 52, 4,
                new Thickness(i == 0 ? 0 : 2, 0, i == actions.Length - 1 ? 0 : 2, 0));
            ToolTip.SetTip(hit, entry.Header + (string.IsNullOrEmpty(entry.Keys) ? "" : " (" + Display(entry.Keys) + ")"));
            if (entry.Enabled) hit.Click += (_, _) => { entry.Click?.Invoke(); close(); };
            Grid.SetColumn(hit, i);
            row.Children.Add(hit);
        }
        return new Border { Height = 52, Child = row };
    }

    private static Control BuildMenuRow(MenuEntry entry, Palette p, bool colored, Action close, Action<Popup?> activate, out Popup? rowSubmenu)
    {
        var row = new Grid { ColumnDefinitions = new ColumnDefinitions("28,*,Auto"), Height = 32 };
        row.ColumnDefinitions[2].SharedSizeGroup = "MenuShortcuts";
        row.Children.Add(Icon(entry.Icon, p, colored));

        var label = new TextBlock
        {
            Text = entry.Header,
            FontFamily = MenuFont,
            FontSize = Typography.Large,
            Foreground = new SolidColorBrush(p.Ink),
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(label, 1);
        row.Children.Add(label);

        var keys = new TextBlock
        {
            Text = entry.Children is not null ? "›" : entry.Keys is null ? null : Display(entry.Keys),
            FontFamily = MenuFont,
            FontSize = Typography.Body,
            Foreground = new SolidColorBrush(p.Muted, Paint.MenuCaption),
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(24, 0, 8, 0),
        };
        Grid.SetColumn(keys, 2);
        row.Children.Add(keys);

        var hit = MenuButton(row, p, entry.Enabled, 32, 4);
        rowSubmenu = null;
        if (entry.Enabled && entry.Children is { Count: > 0 } children)
        {
            var panel = new StackPanel();
            var submenu = new Popup
            {
                PlacementTarget = hit,
                Placement = PlacementMode.Right,
                WindowManagerAddShadowHint = false,
                Child = MenuBorder(panel, p),
            };
            submenu.Opened += (_, _) => ConfigurePopup(panel);
            rowSubmenu = submenu;
            Popup? openChild = null;
            void ActivateChild(Popup? child)
            {
                if (openChild != child && openChild is not null) openChild.IsOpen = false;
                openChild = child;
            }
            var childSubmenus = new Dictionary<Control, Popup?>();
            foreach (var child in children)
            {
                var childRow = BuildMenuRow(child, p, colored, () => { submenu.IsOpen = false; close(); }, ActivateChild, out var childSubmenu);
                childSubmenus.Add(childRow, childSubmenu);
                panel.Children.Add(childRow);
            }
            TrackHoveredRow(panel, childSubmenus, ActivateChild);
            hit.DetachedFromVisualTree += (_, _) => submenu.IsOpen = false;
            void Open() { activate(submenu); submenu.IsOpen = true; }
            hit.PointerEntered += (_, _) => Open();
            hit.Click += (_, _) => Open();
            hit.KeyDown += (_, e) =>
            {
                if (e.Key != Key.Right) return;
                Open();
                if (panel.Children.FirstOrDefault() is Control first) first.Focus();
                e.Handled = true;
            };
            panel.KeyDown += (_, e) =>
            {
                if (e.Key is not (Key.Left or Key.Escape)) return;
                submenu.IsOpen = false;
                hit.Focus();
                e.Handled = true;
            };
        }
        else if (entry.Enabled) hit.Click += (_, _) => { entry.Click?.Invoke(); close(); };
        Avalonia.Automation.AutomationProperties.SetName(hit, entry.Header!);
        return hit;
    }

    /// <summary>Closes a submenu when the pointer moves onto another row in its parent menu.</summary>
    private static void TrackHoveredRow(StackPanel panel, Dictionary<Control, Popup?> submenus, Action<Popup?> activate)
    {
        panel.PointerMoved += (_, e) =>
        {
            var position = e.GetPosition(panel);
            foreach (var (row, submenu) in submenus)
                if (row.Bounds.Contains(position)) { activate(submenu); break; }
        };
    }

    private static Button MenuButton(Control content, Palette p, bool enabled, double height, double radius, Thickness? margin = null)
    {
        var opacity = enabled ? 1 : .42;
        content.Opacity = opacity;
        var border = new Border
        {
            Height = height,
            Margin = margin ?? default,
            Padding = height == 32 ? new Thickness(8, 0) : new Thickness(0),
            CornerRadius = new CornerRadius(radius),
            Background = Brushes.Transparent,
            Child = content,
        };
        AddHover(border, p);
        var button = new Button
        {
            Content = border,
            Padding = new Thickness(0),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            IsEnabled = enabled,
            Focusable = enabled,
            Cursor = new Cursor(enabled ? StandardCursorType.Hand : StandardCursorType.Arrow),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
        };
        button.Classes.Add("plain");
        return button;
    }

    private static Control Icon(global::AntiNotepad.Icon? icon, Palette p, bool colored)
    {
        if (icon is null) return new Border { Width = 18, Height = 18, VerticalAlignment = VerticalAlignment.Center };
        var color = colored ? icon switch
        {
            { } value when ReferenceEquals(value, Icons.NewNote) || ReferenceEquals(value, Icons.KeepOnTop) => p.Accent,
            { } value when ReferenceEquals(value, Icons.Delete) => p.Error,
            _ => p.Ink,
        } : p.Ink;
        return new GlyphIcon(icon, color, Typography.IconMedium)
        {
            Width = 28,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    private static void AddHover(Border border, Palette p)
    {
        var hover = new SolidColorBrush(p.Ink, Paint.Hover(p.Dark));
        var pressed = new SolidColorBrush(p.Ink, Paint.Press(p.Dark));
        border.PointerEntered += (_, _) => border.Background = hover;
        border.PointerExited += (_, _) => border.Background = Brushes.Transparent;
        border.AddHandler(InputElement.PointerPressedEvent, (_, _) => border.Background = pressed,
            Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        border.AddHandler(InputElement.PointerReleasedEvent, (_, _) => border.Background = hover,
            Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
    }

    private static Border Separator(Palette p) => new()
    {
        Height = 1,
        Margin = new Thickness(0, 4),
        Background = new SolidColorBrush(p.Faint, Paint.MenuSeparator),
    };

    private static string Display(string keys) => keys.Replace("OemBackslash", "\\").Replace("OemComma", ",").Replace("Escape", "Esc").Replace("Delete", "Del");
}
