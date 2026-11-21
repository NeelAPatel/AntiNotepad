// Shared controls for palette-aware application chrome.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using AntiNotepad;
using AntiNotepad.Theme;
using AntiNotepad.Motion;

namespace AntiNotepad.UI;

/// <summary>Draws a registered icon with its declared font and palette color.</summary>
internal sealed class GlyphIcon(Icon icon, Color color, double size = Typography.IconBody) : Control
{
    private readonly string _glyph = icon.Character;
    private readonly Typeface _face = new(Icons.FontFamily(icon.Font));
    private readonly IBrush _brush = new ImmutableSolidColorBrush(color);
    private Geometry? _geometry;
    private Rect _inkBounds;
    private double _textScale = double.NaN;
    private double _highlightOpacity;
    public Color? Highlight { get; set; }
    public double HighlightOpacity
    {
        get => _highlightOpacity;
        set { _highlightOpacity = value; InvalidateVisual(); }
    }

    /// <summary>Caches the icon outline for the current scale so repeated draws can reuse it.</summary>
    private Geometry Geometry(double scale)
    {
        if (_geometry is not null && _textScale.Equals(scale)) return _geometry;
        var text = new FormattedText(_glyph, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
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

        Draw(context, geometry, origin);
        if (Highlight is { } highlight && HighlightOpacity > 0)
        {
            using (context.PushOpacity(HighlightOpacity))
            using (context.PushTransform(Matrix.CreateTranslation(origin.X, origin.Y)))
                context.DrawGeometry(new ImmutableSolidColorBrush(highlight), null, geometry);
        }
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
    public static Button IconButton(Icon icon, string name, Palette p, Action click, double size = ChromeTokens.ButtonSize, bool filled = false, Color? color = null, double radius = ChromeTokens.TopIconHoverRadius, Color? hoverColor = null)
    {
        var ink = color ?? p.Ink;
        var glyphIcon = new GlyphIcon(icon, ink, ChromeTokens.IconGlyphSize) { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Highlight = hoverColor };
        Control content = glyphIcon;
        // Filled: a soft fill under the outline glyph, so the shape still reads (a solid fill alone is a blob).
        if (filled && ReferenceEquals(icon, Icons.KeepOnTop))
            content = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children = { new GlyphIcon(Icons.KeepOnTopFilled, Color.FromArgb(Paint.PinnedGlyphAlpha, ink.R, ink.G, ink.B), ChromeTokens.IconGlyphSize), glyphIcon },
            };
        var button = Plain(content, p, size, size, radius: radius);
        if (hoverColor is not null)
        {
            Anim.Handle? highlightFade = null;
            void Refresh()
            {
                highlightFade?.Cancel();
                var on = button.IsEnabled && (button.IsPointerOver || button.IsKeyboardFocusWithin);
                var from = glyphIcon.HighlightOpacity;
                var to = on ? 1.0 : 0.0;
                highlightFade = Anim.Run(button, on ? MotionTokens.Win11Fast : MotionTokens.Win11Faster,
                    t => glyphIcon.HighlightOpacity = from + (to - from) * t, easing: on ? MotionTokens.Win11Enter : MotionTokens.Win11Exit);
            }
            button.PropertyChanged += (_, e) => { if (e.Property == InputElement.IsEnabledProperty) Refresh(); };
            button.PointerEntered += (_, _) => Refresh();
            button.PointerExited += (_, _) => Refresh();
            button.GotFocus += (_, _) => Refresh();
            button.LostFocus += (_, _) => Refresh();
        }
        Anim.Handle? enabledFade = null;
        if (button.Content is Border iconFace)
            button.PropertyChanged += (_, e) =>
            {
                if (e.Property != InputElement.IsEnabledProperty) return;
                enabledFade?.Cancel();
                var from = iconFace.Opacity;
                var to = button.IsEnabled ? 1 : Paint.DisabledIcon;
                if (!button.IsEffectivelyVisible) iconFace.Opacity = to;
                else enabledFade = Anim.Run(iconFace, MotionTokens.PillFade, t => iconFace.Opacity = from + (to - from) * t);
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
        var border = new Border { CornerRadius = new CornerRadius(radius), Background = Brushes.Transparent, Child = content, Padding = new Thickness(0) };
        var button = new Button
        {
            Content = border,
            Padding = new Thickness(0),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Width = width,
            Height = height,
            Cursor = new Cursor(StandardCursorType.Hand),
            Focusable = true,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            FocusAdorner = new FuncTemplate<Control>(() => new Border
            {
                Margin = new Thickness(Shape.Thin),
                CornerRadius = new CornerRadius(radius),
                BorderThickness = new Thickness(Shape.Thick),
                BorderBrush = new SolidColorBrush(p.Ink, Paint.FocusBorder),
                IsHitTestVisible = false,
            }),
        };
        button.Classes.Add("plain");
        var wash = new SolidColorBrush(p.Ink, 0);
        Anim.Handle? fade = null;
        void SetWash(double target, bool immediate = false)
        {
            fade?.Cancel();
            var from = ReferenceEquals(border.Background, wash) ? wash.Opacity : 0;
            wash.Opacity = from;
            border.Background = wash;
            void Step(double t)
            {
                // A caller may replace the face background to supply its own feedback.
                if (ReferenceEquals(border.Background, wash)) wash.Opacity = from + (target - from) * t;
            }
            void Done() { if (target == 0 && ReferenceEquals(border.Background, wash)) border.Background = Brushes.Transparent; }
            if (immediate) { Step(1); Done(); }
            else fade = Anim.Run(button, target > from ? MotionTokens.Win11Fast : MotionTokens.Win11Faster,
                Step, Done, easing: target > from ? MotionTokens.Win11Enter : MotionTokens.Win11Exit);
        }
        button.PointerEntered += (_, _) => { if (button.IsEnabled) SetWash(Paint.Hover(p.Dark)); };
        button.PointerExited += (_, _) => SetWash(0);
        button.AddHandler(InputElement.PointerPressedEvent, (_, _) => { if (button.IsEnabled) SetWash(Paint.Press(p.Dark), true); }, Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        button.AddHandler(InputElement.PointerReleasedEvent, (_, _) => { if (button.IsEnabled) SetWash(button.IsPointerOver ? Paint.Hover(p.Dark) : 0); }, Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        button.PropertyChanged += (_, e) => { if (e.Property == InputElement.IsEnabledProperty && !button.IsEnabled) SetWash(0, true); };
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
            text.Margin = new Thickness(Space.S, Space.Xs);
            var button = Plain(text, p, height: ChromeTokens.ButtonSize, radius: Shape.Control);
            button.Margin = new Thickness(0, 0, Space.Xs, Space.Xs);
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
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(Space.S, 0, 0, 0) };
        var text = Text(title, p.Ink, Typography.PanelTitle, Typography.Strong);
        grid.Children.Add(text);
        var x = IconButton(Icons.Close, "Close (Esc)", p, close);
        x.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(x, 1);
        grid.Children.Add(x);
        return grid;
    }

    public static Control Section(Palette p, string title) =>
        new TextBlock { Text = title.ToUpperInvariant(), Foreground = new SolidColorBrush(p.Faint), FontSize = Typography.Micro, FontWeight = Typography.Strong, LetterSpacing = Typography.CapsLetterSpacing, Margin = new Thickness(Space.S, Space.Xl, 0, Space.Nudge) };

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
            style.Setters.Add(new Avalonia.Styling.Setter(Border.BorderThicknessProperty, new Thickness(state == ":focus" ? Shape.Thick : Shape.Thin)));
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
