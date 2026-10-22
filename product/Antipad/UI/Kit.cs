using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Antipad.Theme;

namespace Antipad.UI;

/// <summary>
/// Stroke icons on a 24-unit grid, drawn with a 1.7 px pen (a weight heavier than Segoe Fluent Icons).
/// Paths follow Lucide (lucide.dev, ISC licence) where noted; the rest are simple geometry.
/// </summary>
internal static class Glyph
{
    public const string
        Search = "M11 4a7 7 0 1 0 0 14a7 7 0 1 0 0-14 M20 20l-4-4",
        Split = "M5 4h14a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2z M12 4v16",
        // Lucide "pin"
        Pin = "M12 17v5 M9 10.76a2 2 0 0 1-1.11 1.79l-1.78.9A2 2 0 0 0 5 15.24V16a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1v-.76a2 2 0 0 0-1.11-1.79l-1.78-.9A2 2 0 0 1 15 10.76V7a1 1 0 0 1 1-1a2 2 0 0 0 0-4H8a2 2 0 0 0 0 4a1 1 0 0 1 1 1z",
        Pinned = Pin,
        // Lucide "sliders-horizontal"
        Settings = "M21 4h-7 M10 4H3 M21 12h-9 M8 12H3 M21 20h-5 M12 20H3 M14 2v4 M8 10v4 M16 18v4",
        Close = "M18 6L6 18 M6 6l12 12",
        Hide = "M5 12h14",
        Back = "M15 18l-6-6l6-6",
        Forward = "M9 18l6-6l-6-6",
        Grid = "M3 3h7v7H3z M14 3h7v7h-7z M14 14h7v7h-7z M3 14h7v7H3z",
        List = "M8 6h13 M8 12h13 M8 18h13 M3 6h1 M3 12h1 M3 18h1",
        // Lucide "trash-2" (simplified)
        Delete = "M3 6h18 M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6 M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2 M10 11v6 M14 11v6",
        Restore = "M3 12a9 9 0 1 0 3-6.7L3 8 M3 3v5h5";

    /// <summary>The 24-unit icon as a path control, centred in a 16 px box.</summary>
    public static Control Make(string data, Color stroke, bool filled = false, double size = 16)
    {
        var path = new Avalonia.Controls.Shapes.Path
        {
            Data = Geometry.Parse(data), Stretch = Stretch.Uniform, Width = size, Height = size,
            Stroke = new SolidColorBrush(stroke), StrokeThickness = 1.7, StrokeLineCap = PenLineCap.Round, StrokeJoin = PenLineJoin.Round,
            Fill = filled ? new SolidColorBrush(stroke) : null,
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
        };
        return path;
    }
}

/// <summary>Small hand-built controls so the chrome follows the palette exactly (no Fluent theme chrome).</summary>
internal static class Kit
{
    public static Button IconButton(string glyph, string name, Palette p, Action click, double size = 30, bool filled = false, Color? color = null)
    {
        var icon = Glyph.Make(glyph, color ?? p.Muted, filled, size >= 28 ? 16 : 14);
        var button = Plain(icon, p, size, size, radius: size / 2.6);
        button.Click += (_, _) => click();
        Avalonia.Automation.AutomationProperties.SetName(button, name);
        ToolTip.SetTip(button, name);
        ToolTip.SetShowDelay(button, 450);
        return button;
    }

    /// <summary>A borderless button with a soft rounded hover fill.</summary>
    public static Button Plain(Control content, Palette p, double width = double.NaN, double height = double.NaN, double radius = 7)
    {
        var hover = new SolidColorBrush(p.Ink, p.Dark ? 0.09 : 0.06);
        var press = new SolidColorBrush(p.Ink, p.Dark ? 0.15 : 0.11);
        var border = new Border { CornerRadius = new CornerRadius(radius), Background = Brushes.Transparent, Child = content, Padding = new Thickness(0) };
        var button = new Button
        {
            Content = border, Padding = new Thickness(0), Background = Brushes.Transparent, BorderThickness = new Thickness(0),
            Width = width, Height = height, Cursor = new Cursor(StandardCursorType.Hand), Focusable = true,
            HorizontalContentAlignment = HorizontalAlignment.Stretch, VerticalContentAlignment = VerticalAlignment.Stretch,
        };
        button.Classes.Add("plain");
        button.PointerEntered += (_, _) => border.Background = hover;
        button.PointerExited += (_, _) => border.Background = Brushes.Transparent;
        button.AddHandler(InputElement.PointerPressedEvent, (_, _) => border.Background = press, Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        button.AddHandler(InputElement.PointerReleasedEvent, (_, _) => border.Background = hover, Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        return button;
    }

    public static TextBlock Text(string text, Color color, double size = 13, FontWeight weight = FontWeight.Normal) =>
        new() { Text = text, Foreground = new SolidColorBrush(color), FontSize = size, FontWeight = weight, VerticalAlignment = VerticalAlignment.Center };

    /// <summary>A row of mutually exclusive pills. Calls <paramref name="pick"/> with the chosen value.</summary>
    public static Control Segmented(Palette p, IReadOnlyList<(string Value, string Label)> options, string current, Action<string> pick)
    {
        var row = new WrapPanel { Orientation = Orientation.Horizontal };
        foreach (var (value, label) in options)
        {
            var selected = value == current;
            var text = Text(label, selected ? p.Ink : p.Muted, 12.5, selected ? FontWeight.SemiBold : FontWeight.Normal);
            text.Margin = new Thickness(10, 4);
            var button = Plain(text, p, radius: 8);
            button.Margin = new Thickness(0, 0, 4, 4);
            if (selected && button.Content is Border b)
            {
                b.Background = new SolidColorBrush(p.Accent, p.Dark ? .22 : .18);
                button.PointerExited += (_, _) => b.Background = new SolidColorBrush(p.Accent, p.Dark ? .22 : .18);
            }
            var chosen = value;
            button.Click += (_, _) => pick(chosen);
            Avalonia.Automation.AutomationProperties.SetName(button, label);
            row.Children.Add(button);
        }
        return row;
    }

    public static Control Switch(Palette p, string label, bool on, Action<bool> set, string? hint = null)
    {
        var knob = new Border { Width = 14, Height = 14, CornerRadius = new CornerRadius(7), Background = new SolidColorBrush(on ? p.Surface : p.Muted), HorizontalAlignment = on ? HorizontalAlignment.Right : HorizontalAlignment.Left, Margin = new Thickness(3, 0) };
        var track = new Border { Width = 36, Height = 20, CornerRadius = new CornerRadius(10), Background = on ? new SolidColorBrush(p.Accent) : new SolidColorBrush(p.Ink, .12), Child = knob, VerticalAlignment = VerticalAlignment.Center };
        var labels = new StackPanel { Spacing = 1, VerticalAlignment = VerticalAlignment.Center };
        labels.Children.Add(Text(label, p.Ink, 13));
        if (hint is not null) labels.Children.Add(Text(hint, p.Muted, 11.5));
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,12,Auto"), Margin = new Thickness(8, 5) };
        grid.Children.Add(labels);
        Grid.SetColumn(track, 2); grid.Children.Add(track);
        var button = Plain(grid, p, radius: 8);
        button.HorizontalAlignment = HorizontalAlignment.Stretch;
        button.Click += (_, _) => set(!on);
        Avalonia.Automation.AutomationProperties.SetName(button, label);
        return button;
    }

    /// <summary>Panel title with a close button at the right (Esc does the same).</summary>
    public static Control PanelHeader(string title, Palette p, Action close)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(8, 0, 0, 0) };
        var text = Text(title, p.Ink, 18, FontWeight.SemiBold);
        grid.Children.Add(text);
        var x = IconButton(Glyph.Close, "Close (Esc)", p, close, 32);
        Grid.SetColumn(x, 1);
        grid.Children.Add(x);
        return grid;
    }

    public static Control Section(Palette p, string title) =>
        new TextBlock { Text = title.ToUpperInvariant(), Foreground = new SolidColorBrush(p.Faint), FontSize = 11, FontWeight = FontWeight.SemiBold, LetterSpacing = 0.8, Margin = new Thickness(8, 16, 0, 6) };

    public static string Ago(DateTime when)
    {
        var d = DateTime.Now - when;
        if (d.TotalMinutes < 1) return "just now";
        if (d.TotalMinutes < 60) return $"{(int)d.TotalMinutes}m ago";
        if (d.TotalHours < 24 && when.Date == DateTime.Today) return when.ToString("t");
        if (when.Date == DateTime.Today.AddDays(-1)) return "yesterday";
        if (d.TotalDays < 7) return when.ToString("ddd");
        return when.ToString("MMM d");
    }
}
