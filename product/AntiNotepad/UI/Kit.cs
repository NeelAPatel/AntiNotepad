// Shared controls and glyphs for palette-aware application chrome.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using AntiNotepad;
using AntiNotepad.Theme;

namespace AntiNotepad.UI;

/// <summary>Segoe Fluent Icons code points used by the chrome.</summary>
internal static class Glyph
{
    public const string Search = "", Pin = "", PinFilled = "", Settings = "", Close = "",
        Split = "", Delete = "", Back = "", Forward = "", Grid = "",
        List = "", Add = "", Restore = "", Hide = "";
    public static readonly FontFamily Font = new("Segoe Fluent Icons, Segoe MDL2 Assets");
    /// <summary>The bundled Fluent UI System Icons font (named icons; regular weight, drawn at 20 px).</summary>
    public static readonly FontFamily Fluent = new($"{AppInfo.ResourceUri}Assets/Fonts#FluentSystemIcons-Regular");
}

/// <summary>
/// A Segoe Fluent Icons glyph. Weight 0 draws it once; positive weights add four offset copies for faux bold.
/// </summary>
internal sealed class GlyphIcon(string glyph, Color color, double size = Typography.IconBody, double weight = 0.15, FontFamily? font = null) : Control
{
    private readonly Typeface _face = new(font ?? Glyph.Font);
    private readonly IBrush _brush = new ImmutableSolidColorBrush(color);
    private Geometry? _geometry;
    private Rect _inkBounds;
    private double _textScale = double.NaN;

    private Geometry Geometry(double scale)
    {
        if (_geometry is not null && _textScale.Equals(scale)) return _geometry;
        var text = new FormattedText(glyph, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            _face, size, new ImmutableSolidColorBrush(color));
        _geometry = text.BuildGeometry(default)!;
        _inkBounds = _geometry.Bounds;
        _textScale = scale;
        return _geometry;
    }

    protected override Size MeasureOverride(Size availableSize) => new(size + ChromeTokens.GlyphIconBoxExtra, size + ChromeTokens.GlyphIconBoxExtra);

    public override void Render(DrawingContext context)
    {
        var scale = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
        var geometry = Geometry(scale);
        var localCenter = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var inkCenter = DevicePixelCenter(localCenter);
        var origin = inkCenter - _inkBounds.Center;

        // Natural weight: one copy. (Five copies on the same spot stack their antialiased edges into a fuzzy outline.)
        if (weight <= 0) { Draw(context, geometry, origin); return; }
        Draw(context, geometry, new Point(origin.X - weight, origin.Y));
        Draw(context, geometry, new Point(origin.X + weight, origin.Y));
        Draw(context, geometry, new Point(origin.X, origin.Y - weight));
        Draw(context, geometry, new Point(origin.X, origin.Y + weight));
        Draw(context, geometry, origin);
    }

    private Point DevicePixelCenter(Point localCenter)
    {
        if (TopLevel.GetTopLevel(this) is not { } top) return localCenter;

        // Snap the translated ink center, including the actual client origin. Rounding a local origin is wrong when
        // this 18 px icon is inset 7 DIP into a 32 px button at 125% scaling.
        if (this.TranslatePoint(localCenter, top) is not { } rootCenter) return localCenter;
        var screenOrigin = top.PointToScreen(default);
        var screenCenter = new PixelPoint(
            (int)Math.Round(screenOrigin.X + rootCenter.X * top.RenderScaling, MidpointRounding.AwayFromZero),
            (int)Math.Round(screenOrigin.Y + rootCenter.Y * top.RenderScaling, MidpointRounding.AwayFromZero));
        var snappedRootCenter = top.PointToClient(screenCenter);
        return top.TranslatePoint(snappedRootCenter, this) ?? localCenter;
    }

    private void Draw(DrawingContext context, Geometry geometry, Point origin)
    {
        using (context.PushTransform(Matrix.CreateTranslation(origin.X, origin.Y)))
            context.DrawGeometry(_brush, null, geometry);
    }
}

/// <summary>Small hand-built controls so the chrome follows the palette exactly (no Fluent theme chrome).</summary>
internal static class Kit
{
    public static Button IconButton(string glyph, string name, Palette p, Action click, double size = ChromeTokens.ButtonSize, bool filled = false, Color? color = null, double radius = ChromeTokens.TopIconHoverRadius, Color? hoverColor = null)
    {
        var ink = color ?? p.Ink;
        Control icon = new GlyphIcon(glyph, ink, ChromeTokens.IconGlyphSize, 0) { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        // Filled: a soft fill under the outline glyph, so the shape still reads (a solid fill alone is a blob).
        if (filled && glyph == Glyph.Pin)
            icon = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children = { new GlyphIcon(Glyph.PinFilled, Color.FromArgb(Paint.PinnedGlyphAlpha, ink.R, ink.G, ink.B), ChromeTokens.IconGlyphSize, 0), icon },
            };
        var button = Plain(icon, p, size, size, radius: radius);
        if (hoverColor is { } highlight && button.Content is Border face)
        {
            var normal = icon;
            var highlighted = new GlyphIcon(glyph, highlight, ChromeTokens.IconGlyphSize, ChromeTokens.PlainIconWeight) { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            void Refresh() => face.Child = button.IsEnabled && (button.IsPointerOver || button.IsKeyboardFocusWithin) ? highlighted : normal;
            button.PropertyChanged += (_, e) => { if (e.Property == InputElement.IsEnabledProperty) Refresh(); };
            button.PointerEntered += (_, _) => Refresh();
            button.PointerExited += (_, _) => Refresh();
            button.GotFocus += (_, _) => Refresh();
            button.LostFocus += (_, _) => Refresh();
        }
        if (button.Content is Border iconFace)
            button.PropertyChanged += (_, e) =>
            {
                if (e.Property != InputElement.IsEnabledProperty) return;
                iconFace.Opacity = button.IsEnabled ? 1 : Paint.DisabledIcon;
                if (!button.IsEnabled) iconFace.Background = Brushes.Transparent;
            };
        button.Click += (_, _) => click();
        Avalonia.Automation.AutomationProperties.SetName(button, name);
        ToolTip.SetTip(button, name);
        ToolTip.SetShowDelay(button, 450);
        return button;
    }

    /// <summary>A borderless button with a soft rounded hover fill.</summary>
    public static Button Plain(Control content, Palette p, double width = double.NaN, double height = double.NaN, double radius = 4)
    {
        var hover = new SolidColorBrush(p.Ink, Paint.Hover(p.Dark));
        var press = new SolidColorBrush(p.Ink, Paint.Press(p.Dark));
        var border = new Border { CornerRadius = new CornerRadius(radius), Background = Brushes.Transparent, Child = content, Padding = new Thickness(0) };
        var button = new Button
        {
            Content = border, Padding = new Thickness(0), Background = Brushes.Transparent, BorderThickness = new Thickness(0),
            Width = width, Height = height, Cursor = new Cursor(StandardCursorType.Hand), Focusable = true,
            HorizontalContentAlignment = HorizontalAlignment.Stretch, VerticalContentAlignment = VerticalAlignment.Stretch,
        };
        button.Classes.Add("plain");
        button.PointerEntered += (_, _) => { if (button.IsEnabled) border.Background = hover; };
        button.PointerExited += (_, _) => border.Background = Brushes.Transparent;
        button.AddHandler(InputElement.PointerPressedEvent, (_, _) => { if (button.IsEnabled) border.Background = press; }, Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        button.AddHandler(InputElement.PointerReleasedEvent, (_, _) => { if (button.IsEnabled) border.Background = hover; }, Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        return button;
    }

    public static TextBlock Text(string text, Color color, double size = Typography.Body, FontWeight weight = Typography.Regular) =>
        new() { Text = text, Foreground = new SolidColorBrush(color), FontSize = size, FontWeight = weight, VerticalAlignment = VerticalAlignment.Center };

    /// <summary>Independent on/off pills (several can be on).</summary>
    public static Control Toggles(Palette p, IReadOnlyList<(string Value, string Label)> options, Func<string, bool> isOn, Action<string> toggle)
    {
        var row = new WrapPanel { Orientation = Orientation.Horizontal };
        foreach (var (value, label) in options)
        {
            var on = isOn(value);
            var text = Text(label, on ? p.Ink : p.Faint, Typography.Body, on ? Typography.Strong : Typography.Regular);
            text.Margin = new Thickness(8, 4);
            var button = Plain(text, p, radius: 4);
            button.Margin = new Thickness(0, 0, 4, 4);
            if (on && button.Content is Border b)
            {
                b.Background = new SolidColorBrush(p.Accent, Paint.Selected(p.Dark));
                button.PointerExited += (_, _) => b.Background = new SolidColorBrush(p.Accent, Paint.Selected(p.Dark));
            }
            var chosen = value;
            button.Click += (_, _) => toggle(chosen);
            Avalonia.Automation.AutomationProperties.SetName(button, label + (on ? ", on" : ", off"));
            row.Children.Add(button);
        }
        return row;
    }

    /// <summary>Panel title with a close button at the right (Esc does the same).</summary>
    public static Control PanelHeader(string title, Palette p, Action close)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(8, 0, 0, 0) };
        var text = Text(title, p.Ink, Typography.PanelTitle, Typography.Strong);
        grid.Children.Add(text);
        var x = IconButton(Glyph.Close, "Close (Esc)", p, close);
        x.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(x, 1);
        grid.Children.Add(x);
        return grid;
    }

    public static Control Section(Palette p, string title) =>
        new TextBlock { Text = title.ToUpperInvariant(), Foreground = new SolidColorBrush(p.Faint), FontSize = Typography.Micro, FontWeight = Typography.Strong, LetterSpacing = 0.8, Margin = new Thickness(8, 24, 0, 2) };

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
            style.Setters.Add(new Avalonia.Styling.Setter(Border.BorderBrushProperty, new SolidColorBrush(state == ":focus" ? p.Accent : p.Faint, state == ":focus" ? Paint.FocusBorder : Paint.RestBorder)));
            style.Setters.Add(new Avalonia.Styling.Setter(Border.BorderThicknessProperty, new Thickness(state == ":focus" ? 2 : 1)));
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
