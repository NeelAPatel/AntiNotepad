using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AntiQuickPad.Editor;
using AntiQuickPad.Views.Common;

namespace AntiQuickPad.Design;

internal sealed class VariantRenderWindow : Window
{
    private readonly VariantRenderSpec _spec;
    private readonly bool _dark;
    private readonly bool _hover;
    private readonly Grid _root = new() { Background = Brushes.Transparent, ClipToBounds = true };
    private readonly Border _tint = new() { IsHitTestVisible = false };
    private readonly Panel _topBar;
    private readonly Panel _bottomBar;
    private readonly TextBlock _idlePosition;
    private readonly TextEditor _editor;
    private readonly ShellPalette _palette;
    private bool _closed;

    internal VariantRenderWindow(VariantRenderSpec spec, bool dark, bool hover, int tintPercent = 90)
    {
        _spec = spec; _dark = dark; _hover = hover;
        _palette = ShellPalette.Get("quiet", dark);
        var look = _palette.Look with { ModernEditor = true, FontSize = spec.TextSize, EditorPadding = 0, ShowGutter = false };
        RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
        Title = $"AntiQuickPad / {spec.Id} / {(dark ? "dark" : "light")} / {(hover ? "hover" : "idle")}";
        Width = spec.Width; Height = spec.Height;
        MinWidth = spec.Width; MinHeight = spec.Height; MaxWidth = spec.Width; MaxHeight = spec.Height;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowDecorations = WindowDecorations.BorderOnly;
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaTitleBarHeightHint = spec.TopBarHeight;
        ShowActivated = false; ShowInTaskbar = false; Topmost = true;
        CanResize = false; Background = Brushes.Transparent;
        TransparencyBackgroundFallback = new SolidColorBrush(_palette.Canvas);
        TransparencyLevelHint = new[] { WindowTransparencyLevel.AcrylicBlur };

        _tint.Background = TintBrush(_palette.Canvas, tintPercent);
        _root.Children.Add(_tint);
        if (spec.PaperGrid) _root.Children.Add(new PaperGridSurface(new SolidColorBrush(_palette.Look.Muted, dark ? .12 : .15)));
        _editor = EditorFactory.Create(look, null, new TextDocument(DesignWindow.Fixture));
        _editor.FontSize = spec.TextSize;
        _editor.Margin = new Thickness(spec.Insets, spec.TopBarHeight + 12, spec.Insets, spec.BottomBarHeight + 10);
        _editor.Background = Brushes.Transparent;
        _editor.TextArea.Background = Brushes.Transparent;
        _editor.TextArea.Caret.Hide();
        _editor.CaretOffset = Math.Min(_editor.Document.TextLength, _editor.Document.GetLineByNumber(13).Offset + 20);
        _root.Children.Add(_editor);

        _topBar = BuildTopBar(look);
        _bottomBar = BuildBottomBar(look);
        _root.Children.Add(_topBar); _root.Children.Add(_bottomBar);
        _idlePosition = new TextBlock { Text = "3 / 8", FontSize = 11, Foreground = new SolidColorBrush(_palette.Look.Muted, .65),
            HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 14, 12), IsHitTestVisible = false };
        if (spec.Bare) _root.Children.Add(_idlePosition);
        Content = _root;
        ApplyBarState();
        Loaded += (_, _) => ClearEditorBackgrounds();
        Opened += (_, _) =>
        {
            ClearEditorBackgrounds();
            Dispatcher.UIThread.Post(ClearEditorBackgrounds, DispatcherPriority.Loaded);
        };
        Closed += (_, _) => { _closed = true; EditorFactory.Release(_editor); };
        AddHandler(KeyDownEvent, (_, e) => { if (e.Key == Key.Escape) { e.Handled = true; Close(); } }, RoutingStrategies.Tunnel, true);
    }

    internal VariantRenderSpec Spec => _spec;
    internal string StateId => $"{(_dark ? "dark" : "light")}-{(_hover ? "hover" : "idle")}";
    internal bool Dark => _dark;
    internal bool Hover => _hover;
    internal string ActualTransparency => ActualTransparencyLevel.ToString();
    internal TextEditor Editor => _editor;

    internal void AssertAcrylic()
    {
        if (ActualTransparencyLevel != WindowTransparencyLevel.AcrylicBlur)
            throw new InvalidOperationException($"{_spec.Id}/{StateId}: expected AcrylicBlur, actual {ActualTransparencyLevel}.");
    }

    internal void PrepareForCapture()
    {
        if (_closed) throw new InvalidOperationException("Variant window closed before capture.");
        UpdateLayout();
        _editor.UpdateLayout();
        _editor.TextArea.TextView.EnsureVisualLines();
        _editor.TextArea.Caret.Hide();
        ClearEditorBackgrounds();
    }

    private Panel BuildTopBar(Look look)
    {
        if (_spec.EdgeStrips)
        {
            var strip = new Border { Height = _spec.TopBarHeight, Background = Brushes.Transparent,
                BorderBrush = new SolidColorBrush(_palette.Rule, .9), BorderThickness = new Thickness(0, 0, 0, 1),
                HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top };
            var title = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(_spec.Insets, 0, 0, 0), Children = { Text("AntiQuickPad", 13, look.Foreground, FontWeight.SemiBold), Text("TODAY · 03 OCT", 10, look.Muted) } };
            var actions = ActionRow(look, includeDelete: false);
            var grid = new Grid(); grid.Children.Add(title); Grid.SetColumn(actions, 1); grid.Children.Add(actions);
            grid.ColumnDefinitions = new ColumnDefinitions("*,Auto"); strip.Child = grid;
            var edgeHost = new PanelHost(strip) { Height = _spec.TopBarHeight, HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top };
            return edgeHost;
        }

        var row = ActionRow(look, includeDelete: false);
        var left = new TextBlock { Text = _spec.Bare || _spec.Id is "V1-compact-pill" or "V3-paper-grid" ? "" : "AntiQuickPad", FontSize = 12, Foreground = new SolidColorBrush(look.Foreground),
            FontWeight = FontWeight.SemiBold, VerticalAlignment = VerticalAlignment.Center };
        var gridRoot = new Grid { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center };
        gridRoot.ColumnDefinitions = new ColumnDefinitions("*,Auto");
        gridRoot.Children.Add(left); Grid.SetColumn(row, 1); gridRoot.Children.Add(row);
        var host = new PanelHost(gridRoot);
        host.Height = _spec.TopBarHeight;
        host.HorizontalAlignment = HorizontalAlignment.Stretch;
        host.VerticalAlignment = VerticalAlignment.Top;
        host.Margin = new Thickness(_spec.Insets, 10, _spec.Insets, 0);
        return host;
    }

    private Panel BuildBottomBar(Look look)
    {
        var position = new TextBlock { Text = _spec.Bare ? "3 / 8" : "3 of 8", FontSize = 11, Foreground = new SolidColorBrush(look.Muted),
            VerticalAlignment = VerticalAlignment.Center };
        var dots = Dots(look);
        var nav = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, VerticalAlignment = VerticalAlignment.Center,
            Children = { IconButton("\uE76B", "Previous", look), position, dots, IconButton("\uE76C", "Next", look) } };
        var save = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, VerticalAlignment = VerticalAlignment.Center,
            Children = { Text("\uE73E", 11, look.Accent, FontWeight.Normal, fluent: true), Text(_spec.EdgeStrips ? "Saved · 03 Oct" : "All changes saved", 11, look.Muted) } };
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto,*") };
        var host = new PanelHost(grid);
        grid.Children.Add(nav); Grid.SetColumn(save, 1); grid.Children.Add(save);
        var trailing = new TextBlock { Text = _spec.EdgeStrips ? "3 / 8" : "Esc to hide", FontSize = 11, Foreground = new SolidColorBrush(look.Muted),
            HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(trailing, 2); grid.Children.Add(trailing);
        host.Height = _spec.BottomBarHeight;
        host.HorizontalAlignment = HorizontalAlignment.Stretch;
        host.VerticalAlignment = VerticalAlignment.Bottom;
        host.Margin = new Thickness(_spec.Insets, 0, _spec.Insets, 10);
        return host;
    }

    private StackPanel ActionRow(Look look, bool includeDelete)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        row.Children.Add(IconButton("\uE710", "New note", look));
        row.Children.Add(IconButton("\uE712", "More menu", look));
        if (includeDelete) row.Children.Add(IconButton("\uE74D", "Delete note", look));
        row.Children.Add(IconButton("\uE718", "Pin", look));
        row.Children.Add(IconButton("\uE8BB", "Close / hide", look));
        return row;
    }

    private StackPanel Dots(Look look)
    {
        var dots = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, VerticalAlignment = VerticalAlignment.Center };
        for (var i = 0; i < 8; i++)
        {
            var dot = new Button { Width = i == 2 ? 14 : 5, Height = 5, Padding = new Thickness(0),
                Background = i == 2 ? new SolidColorBrush(look.Accent) : new SolidColorBrush(look.Muted, .35),
                BorderThickness = new Thickness(0), CornerRadius = new CornerRadius(3), Content = "" };
            ToolTip.SetTip(dot, $"Note {i + 1} of 8"); dots.Children.Add(dot);
        }
        return dots;
    }

    private Button IconButton(string glyph, string label, Look look)
    {
        var text = Text(glyph, 14, look.Foreground, fluent: true);
        var button = new Button { Content = text, Width = 28, Height = 28, Padding = new Thickness(0),
            Background = Brushes.Transparent, BorderBrush = Brushes.Transparent, BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(6), HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center };
        ToolTip.SetTip(button, label);
        button.Click += (_, _) => { };
        return button;
    }

    private TextBlock Text(string value, double size, Color color, FontWeight? weight = null, bool fluent = false)
        => new() { Text = value, FontSize = size, Foreground = new SolidColorBrush(color), FontWeight = weight ?? FontWeight.Normal,
            FontFamily = fluent ? new FontFamily("Segoe Fluent Icons") : FontFamily, VerticalAlignment = VerticalAlignment.Center };

    private void ApplyBarState()
    {
        var visible = _hover;
        _topBar.IsVisible = visible;
        _bottomBar.IsVisible = visible;
        if (_spec.Bare) _idlePosition.IsVisible = true;
    }

    private void ClearEditorBackgrounds()
    {
        if (_closed) return;
        _editor.Background = Brushes.Transparent;
        _editor.TextArea.Background = Brushes.Transparent;
        foreach (var visual in _editor.GetVisualDescendants())
        {
            if (visual is Panel panel) panel.Background = Brushes.Transparent;
            if (visual is Border border) border.Background = Brushes.Transparent;
            if (visual is TemplatedControl control) control.Background = Brushes.Transparent;
            if (visual is ContentPresenter presenter) presenter.Background = Brushes.Transparent;
        }
    }

    private static IBrush TintBrush(Color canvas, int percent) =>
        new SolidColorBrush(Color.FromArgb((byte)Math.Round(percent * 255d / 100), canvas.R, canvas.G, canvas.B));

    private sealed class PanelHost : Panel
    {
        internal PanelHost(Control child) => Children.Add(child);
        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (var child in Children) child.Measure(availableSize);
            return availableSize;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var child in Children) child.Arrange(new Rect(finalSize));
            return finalSize;
        }
    }
}

internal sealed class PaperGridSurface : Control
{
    private readonly IBrush _dotBrush;
    public PaperGridSurface(IBrush dotBrush) { _dotBrush = dotBrush; IsHitTestVisible = false; }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        for (var y = 18d; y < Bounds.Height; y += 18)
            for (var x = 18d; x < Bounds.Width; x += 18)
                context.DrawEllipse(_dotBrush, null, new Point(x, y), 0.7, 0.7);
    }
}
