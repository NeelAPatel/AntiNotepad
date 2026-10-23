using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Antipad.Theme;

namespace Antipad.UI;

/// <summary>Segoe Fluent Icons code points used by the chrome.</summary>
internal static class Glyph
{
    public const string Search = "", Pin = "", PinFilled = "", Settings = "", Close = "",
        Split = "", Delete = "", Back = "", Forward = "", Grid = "",
        List = "", Restore = "", Hide = "";
    public static readonly FontFamily Font = new("Segoe Fluent Icons, Segoe MDL2 Assets");
    /// <summary>The bundled Fluent UI System Icons font (named icons; regular weight, drawn at 20 px).</summary>
    public static readonly FontFamily Fluent = new("avares://Antipad/Assets/Fonts#FluentSystemIcons-Regular");
}

/// <summary>
/// A Segoe Fluent Icons glyph drawn one weight heavier. The font ships a single light weight, so the glyph is
/// drawn with four extra copies nudged by <c>weight</c> px around it (faux bold); with grayscale antialiasing
/// that reads as a cleanly thicker stroke.
/// </summary>
internal sealed class GlyphIcon(string glyph, Color color, double size = 13, double weight = 0.15, FontFamily? font = null) : Control
{
    private readonly Typeface _face = new(font ?? Glyph.Font);

    private FormattedText Text(IBrush brush) =>
        new(glyph, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, _face, size, brush);

    protected override Size MeasureOverride(Size availableSize) => new(size + 2, size + 2);

    /// <summary>Dev (icon lab): place the glyph on whole device pixels, as Windows does, instead of wherever centring lands.</summary>
    public static bool Snap = true;

    public override void Render(DrawingContext context)
    {
        var t = Text(new SolidColorBrush(color));
        var origin = new Point((Bounds.Width - t.Width) / 2, (Bounds.Height - t.Height) / 2);
        if (Snap)
        {
            var scale = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
            origin = new Point(Math.Round(origin.X * scale) / scale, Math.Round(origin.Y * scale) / scale);
        }
        // Natural weight: one copy. (Five copies on the same spot stack their antialiased edges into a fuzzy outline.)
        if (weight <= 0) { context.DrawText(t, origin); return; }
        foreach (var (dx, dy) in new[] { (-weight, 0.0), (weight, 0.0), (0.0, -weight), (0.0, weight), (0.0, 0.0) })
            context.DrawText(t, new Point(origin.X + dx, origin.Y + dy));
    }
}

/// <summary>Small hand-built controls so the chrome follows the palette exactly (no Fluent theme chrome).</summary>
internal static class Kit
{
    public static Button IconButton(string glyph, string name, Palette p, Action click, double size = 32, bool filled = false, Color? color = null)
    {
        var ink = color ?? p.Ink;
        Control icon = new GlyphIcon(glyph, ink, 16, 0) { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        // Filled: a soft fill under the outline glyph, so the shape still reads (a solid fill alone is a blob).
        if (filled && glyph == Glyph.Pin)
            icon = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                Children = { new GlyphIcon(Glyph.PinFilled, Color.FromArgb(110, ink.R, ink.G, ink.B), 16, 0), icon },
            };
        var button = Plain(icon, p, size, size, radius: 4);
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

    /// <summary>Independent on/off pills (several can be on), same shape as Segmented.</summary>
    public static Control Toggles(Palette p, IReadOnlyList<(string Value, string Label)> options, Func<string, bool> isOn, Action<string> toggle)
    {
        var row = new WrapPanel { Orientation = Orientation.Horizontal };
        foreach (var (value, label) in options)
        {
            var on = isOn(value);
            var text = Text(label, on ? p.Ink : p.Faint, 12.5, on ? FontWeight.SemiBold : FontWeight.Normal);
            text.Margin = new Thickness(10, 4);
            var button = Plain(text, p, radius: 8);
            button.Margin = new Thickness(0, 0, 4, 4);
            if (on && button.Content is Border b)
            {
                b.Background = new SolidColorBrush(p.Accent, p.Dark ? .22 : .18);
                button.PointerExited += (_, _) => b.Background = new SolidColorBrush(p.Accent, p.Dark ? .22 : .18);
            }
            var chosen = value;
            button.Click += (_, _) => toggle(chosen);
            Avalonia.Automation.AutomationProperties.SetName(button, label + (on ? ", on" : ", off"));
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

    /// <summary>The window material as a transparency request, best first.</summary>
    public static WindowTransparencyLevel[] Material(string material) => material switch
    {
        "acrylic" => [WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Mica, WindowTransparencyLevel.None],
        "mica" => [WindowTransparencyLevel.Mica, WindowTransparencyLevel.None],
        _ => [WindowTransparencyLevel.None]
    };

    /// <summary>Fluent's text boxes go black/teal on focus and its buttons paint square fills; keep both on-palette.</summary>
    public static void InputStyles(Avalonia.Styling.Styles styles, Palette p)
    {
        styles.Clear();
        foreach (var state in new[] { "", ":pointerover", ":focus" })
        {
            var style = new Avalonia.Styling.Style(x => (state.Length == 0 ? x.OfType<TextBox>() : x.OfType<TextBox>().Class(state)).Template().OfType<Border>().Name("PART_BorderElement"));
            style.Setters.Add(new Avalonia.Styling.Setter(Border.BackgroundProperty, new SolidColorBrush(p.Surface)));
            style.Setters.Add(new Avalonia.Styling.Setter(Border.BorderBrushProperty, new SolidColorBrush(state == ":focus" ? p.Accent : p.Faint, state == ":focus" ? .9 : .5)));
            style.Setters.Add(new Avalonia.Styling.Setter(Border.BorderThicknessProperty, new Thickness(state == ":focus" ? 1.5 : 1)));
            styles.Add(style);
        }
        var fg = new Avalonia.Styling.Style(x => x.OfType<TextBox>().Class(":focus"));
        fg.Setters.Add(new Avalonia.Styling.Setter(TextBox.ForegroundProperty, new SolidColorBrush(p.Ink)));
        styles.Add(fg);
        // Plain buttons draw their own rounded hover; Fluent's square template fill must never show through.
        foreach (var state in new[] { ":pointerover", ":pressed", ":focus-visible", "" })
        {
            var plain = new Avalonia.Styling.Style(x => (state.Length == 0 ? x.OfType<Button>().Class("plain") : x.OfType<Button>().Class("plain").Class(state))
                .Template().OfType<Avalonia.Controls.Presenters.ContentPresenter>().Name("PART_ContentPresenter"));
            plain.Setters.Add(new Avalonia.Styling.Setter(Avalonia.Controls.Presenters.ContentPresenter.BackgroundProperty, Brushes.Transparent));
            plain.Setters.Add(new Avalonia.Styling.Setter(Avalonia.Controls.Presenters.ContentPresenter.BorderBrushProperty, Brushes.Transparent));
            styles.Add(plain);
        }
        var bar = new Avalonia.Styling.Style(x => x.OfType<Avalonia.Controls.Primitives.ScrollBar>());
        bar.Setters.Add(new Avalonia.Styling.Setter(Visual.OpacityProperty, .5));
        styles.Add(bar);
    }

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
