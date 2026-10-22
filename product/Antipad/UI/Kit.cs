using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Antipad.Theme;

namespace Antipad.UI;

/// <summary>Segoe Fluent Icons code points used by the chrome.</summary>
internal static class Glyph
{
    public const string Search = "", Pin = "", PinFilled = "", Settings = "", Close = "",
        Split = "", Delete = "", Back = "", Forward = "", Grid = "",
        List = "", Restore = "", Hide = "";
    public static readonly FontFamily Font = new("Segoe Fluent Icons, Segoe MDL2 Assets");
}

/// <summary>
/// A Segoe Fluent Icons glyph drawn one weight heavier. The font ships a single light weight, so the glyph is
/// drawn with four extra copies nudged by <c>weight</c> px around it (faux bold); with grayscale antialiasing
/// that reads as a cleanly thicker stroke.
/// </summary>
internal sealed class GlyphIcon(string glyph, Color color, double size = 13, double weight = 0.3) : Control
{
    private static readonly Typeface Face = new(Glyph.Font);

    private FormattedText Text(IBrush brush) =>
        new(glyph, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Face, size, brush);

    protected override Size MeasureOverride(Size availableSize) => new(size + 2, size + 2);

    public override void Render(DrawingContext context)
    {
        var t = Text(new SolidColorBrush(color));
        var origin = new Point((Bounds.Width - t.Width) / 2, (Bounds.Height - t.Height) / 2);
        foreach (var (dx, dy) in new[] { (-weight, 0.0), (weight, 0.0), (0.0, -weight), (0.0, weight), (0.0, 0.0) })
            context.DrawText(t, new Point(origin.X + dx, origin.Y + dy));
    }
}

/// <summary>Small hand-built controls so the chrome follows the palette exactly (no Fluent theme chrome).</summary>
internal static class Kit
{
    public static Button IconButton(string glyph, string name, Palette p, Action click, double size = 28, bool filled = false, Color? color = null)
    {
        // A filled pin is its own glyph in Segoe Fluent Icons.
        var icon = new GlyphIcon(filled && glyph == Glyph.Pin ? Glyph.PinFilled : glyph, color ?? p.Muted)
        {
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
        };
        var button = Plain(icon, p, size, size, radius: 7);
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
        var x = IconButton(Glyph.Close, "Close (Esc)", p, close);
        x.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(x, 1);
        grid.Children.Add(x);
        return grid;
    }

    public static Control Section(Palette p, string title) =>
        new TextBlock { Text = title.ToUpperInvariant(), Foreground = new SolidColorBrush(p.Faint), FontSize = 11, FontWeight = FontWeight.SemiBold, LetterSpacing = 0.8, Margin = new Thickness(8, 22, 0, 2) };

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
