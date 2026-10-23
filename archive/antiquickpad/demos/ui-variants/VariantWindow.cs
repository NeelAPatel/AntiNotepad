using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Folding;

namespace UiVariants;

public sealed class VariantWindow : Window
{
    public const int W = 1100, H = 760;
    private const string Fixture = "# AntiQuickPad\n\n## A focused scratchpad\n\n### Plain, quick, yours\n\nVisible **bold**, *italic*, and ~~strike~~ markers remain editable.\n\n- [ ] Click the checkbox marker to toggle it\n- [x] Keep notes local\n\n```python\ndef quick_note(text):\n    # saved on every keystroke\n    return 'wrapped and highlighted'\n```\n\nThis is one deliberately long line of ordinary prose that keeps going past the edge of the card so that soft wrapping, the gutter and the line height can all be judged together in a single glance.";
    private const string Second = "# Shopping\n\n## This week\n\n- [x] Coffee beans\n- [ ] Oat milk\n- [ ] **Batteries** for the *keyboard*\n\n### Later\n\nCall the plumber about the ~~old~~ new boiler.";

    private readonly string _variant; private readonly string? _out; private readonly Look _look;
    private readonly Control _root;

    internal VariantWindow(string variant, string? outPath)
    {
        _variant = variant; _out = outPath;
        var carousel = variant == "carousel";
        _look = Look.All[carousel ? "ink" : variant];
        Title = "AntiQuickPad ui-variants"; Width = W; Height = H; CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowDecorations = WindowDecorations.None;

        var panelBody = carousel ? BuildCarousel() : (Control)BuildEditor(Fixture, caretLine: 7, fold: _look.Folding);
        var strip = BuildStrip(carousel);
        var dock = new DockPanel();
        DockPanel.SetDock(strip, Dock.Top); dock.Children.Add(strip); dock.Children.Add(panelBody);
        var panel = new Border
        {
            Margin = new Thickness(36), CornerRadius = new CornerRadius(12), ClipToBounds = true,
            Background = new SolidColorBrush(Color.Parse(_look.Panel)), BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse(_look.Border)), Child = dock,
            BoxShadow = BoxShadows.Parse("0 14 40 0 #80000000")
        };
        _root = new Grid { Background = _look.Backdrop, Children = { panel } };
        Content = _root;
        Opened += (_, _) => { if (_out != null) DispatcherTimer.RunOnce(RenderAndExit, TimeSpan.FromMilliseconds(900)); };
    }

    // The only "chrome": a thin custom grab area with a pill and a page indicator.
    private Control BuildStrip(bool carousel)
    {
        var dim = new SolidColorBrush(Color.Parse(_look.Dim));
        var g = new Grid { Height = 28 };
        g.Children.Add(new Border { Width = 36, Height = 4, CornerRadius = new CornerRadius(2), Background = dim, Opacity = .6, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
        g.Children.Add(new TextBlock { Text = carousel ? "1 / 3" : "1 / 1", FontSize = 11, Foreground = dim, Margin = new Thickness(0, 0, 16, 0), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, FontFamily = new FontFamily(_look.EditorFont) });
        return g;
    }

    private Control BuildCarousel()
    {
        const double cardW = 700, gap = 28, shift = -120;
        var canvas = new Canvas { ClipToBounds = true };
        var texts = new[] { Fixture, Second, "# Ideas\n\n- [ ] something" };
        for (var i = 0; i < 2; i++)
        {
            var ed = BuildEditor(texts[i], caretLine: 7, fold: false);
            ed.Width = cardW;
            var card = new Border
            {
                Width = cardW, Height = H - 72 - 28 - 20, CornerRadius = new CornerRadius(10), ClipToBounds = true, Child = ed,
                Background = new SolidColorBrush(Color.Parse("#14FFFFFF")), BorderThickness = new Thickness(1), BorderBrush = new SolidColorBrush(Color.Parse(_look.Border)),
                Opacity = i == 0 ? 1 : .72
            };
            Canvas.SetLeft(card, 150 + shift + i * (cardW + gap)); Canvas.SetTop(card, 4);
            canvas.Children.Add(card);
        }
        return canvas;
    }

    private TextEditor BuildEditor(string text, int caretLine, bool fold)
    {
        var L = _look;
        var ed = new TextEditor
        {
            FontFamily = new FontFamily(L.EditorFont), FontSize = L.FontSize, WordWrap = true, ShowLineNumbers = false,
            Background = Brushes.Transparent, Foreground = new SolidColorBrush(Color.Parse(L.Fg)),
            Padding = new Thickness(L.Pad, L.Pad / 2, L.Pad, L.Pad), Text = text, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Hidden
        };
        ed.Options.HighlightCurrentLine = L.HighlightCurrent;
        ed.Options.EnableHyperlinks = false; ed.Options.EnableEmailHyperlinks = false;
        var ta = ed.TextArea; var tv = ta.TextView;
        tv.LineTransformers.Add(new MarkdownColorizer(L));
        tv.BackgroundRenderers.Add(new CodePanelRenderer(L));
        if (L.CheckboxGraphics) tv.ElementGenerators.Add(new CheckboxGenerator(L));
        tv.CurrentLineBackground = new SolidColorBrush(Color.Parse(L.CurrentBg));
        tv.CurrentLineBorder = new Pen(Brushes.Transparent, 0);
        ta.Caret.CaretBrush = new SolidColorBrush(Color.Parse(L.Accent));
        ta.SelectionBrush = new SolidColorBrush(Color.Parse(L.Accent)) { Opacity = .3 };
        if (fold)
        {
            var fm = FoldingManager.Install(ta);
            fm.UpdateFoldings(Fence.Ranges(ed.Document).Select(r => new NewFolding(ed.Document.GetLineByNumber(r.Start).Offset, ed.Document.GetLineByNumber(r.End).EndOffset)).ToList(), -1);
            var fmargin = ta.LeftMargins.OfType<FoldingMargin>().FirstOrDefault();
            if (fmargin != null)
            {
                fmargin.SetValue(FoldingMargin.FoldingMarkerBrushProperty, new SolidColorBrush(Color.Parse(L.Accent)));
            }
        }
        if (L.LineNumbers) ta.LeftMargins.Insert(0, new GutterMargin(L));
        ed.TextArea.Caret.Line = caretLine;
        return ed;
    }

    private async void RenderAndExit()
    {
        try
        {
            Dispatcher.UIThread.RunJobs();
            using var bmp = new RenderTargetBitmap(new PixelSize(W, H), new Vector(96, 96));
            bmp.Render(_root);
            Directory.CreateDirectory(Path.GetDirectoryName(_out!)!);
            bmp.Save(_out!);
            Console.WriteLine($"rendered {_variant} -> {_out} ({W}x{H}) renderScaling={RenderScaling}");
        }
        catch (Exception ex) { Console.Error.WriteLine("render failed: " + ex); Environment.ExitCode = 1; }
        await Task.Yield();
        Close();
    }
}
