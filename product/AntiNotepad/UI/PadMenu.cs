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
            if (TopLevel.GetTopLevel(content) is { } host)
            {
                host.TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
                host.Background = Brushes.Transparent;
                DesktopPlatform.ConfigurePopupWindow(host);
            }
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
        Grid.SetIsSharedSizeScope(content, true);
        if (settings.MenuActionRow)
        {
            content.Children.Add(BuildActionRow(entries, p, settings.MenuColorIcons, close));
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
            content.Children.Add(BuildMenuRow(entry, p, settings.MenuColorIcons, close));
            lastWasSeparator = false;
        }

        return new Border
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
    }

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
            if (entry.Enabled) hit.Click += (_, _) => { entry.Click?.Invoke(); close(); };
            Grid.SetColumn(hit, i);
            row.Children.Add(hit);
        }
        return new Border { Height = 52, Child = row };
    }

    private static Control BuildMenuRow(MenuEntry entry, Palette p, bool colored, Action close)
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
            Text = entry.Keys is null ? null : Display(entry.Keys),
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
        if (entry.Enabled) hit.Click += (_, _) => { entry.Click?.Invoke(); close(); };
        Avalonia.Automation.AutomationProperties.SetName(hit, entry.Header!);
        return hit;
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

    private static Control Icon(string? glyph, Palette p, bool colored)
    {
        if (glyph is null) return new Border { Width = 18, Height = 18, VerticalAlignment = VerticalAlignment.Center };
        var color = colored ? glyph switch
        {
            MenuGlyphs.NewNote or MenuGlyphs.Pin => p.Accent,
            MenuGlyphs.Delete => p.Error,
            _ => p.Ink,
        } : p.Ink;
        return new GlyphIcon(glyph, color, Typography.IconMedium, 0)
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
