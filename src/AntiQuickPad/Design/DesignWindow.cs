using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.Rendering;
using AntiQuickPad.Editor;

namespace AntiQuickPad.Design;

// Real Avalonia visual tree, real app editor factory. Fixture-only: no user notes or shell services.
internal sealed class DesignWindow : Window, Views.IEditorHost
{
    internal const string Fixture = "# A calmer workspace\n\nA few ideas for the next version. Keep the useful things close,\nand leave enough room for the thought that comes next.\n\n## Make space for the essentials\n- One shortcut. A fresh page. Back to work.\n- Small details that make the whole thing feel effortless.\n\n- [x] Keep the first interaction immediate\n- [ ] Refine the writing surface\n    - [x] Align the little things\n    - [ ] Give every control a clear purpose\n\n## A small experiment\n```python\ndef make_room(thoughts):\n    # Keep only what matters\n    return [idea for idea in thoughts if idea.useful]\n```\n\n~~Add another row of buttons~~\nLess chrome. More room to think.\n";
    private const string NextFixture = "# Make it feel effortless\n\nThe best tools get out of the way.\n\n## Next time\n- [ ] Try the shortcut without looking\n- [ ] Read a long note at 150% scaling\n    - [ ] Check the details\n\n## Remember\nSmall, deliberate decisions add up.\n\n```python\nfor detail in workspace:\n    detail.refine()\n```\n";
    private readonly DesignSpec _s;
    private readonly Grid _root = new() { Width = 960, Height = 800 };
    private readonly Viewbox _capture = new() { Stretch = Stretch.Fill };
    private readonly Grid _viewport = new() { ClipToBounds = true };
    private readonly Grid _bars = new() { IsHitTestVisible = true };
    private readonly Border _menu;
    private readonly List<TextEditor> _editors = [];
    private readonly List<Control> _pages = [];
    private readonly Border _placeholder;
    private readonly TextBlock _position;
    private readonly TextBlock _save;
    private readonly StackPanel _dots;
    private Button? _moreButton;
    private TextBlock? _lineInfo, _wordInfo;
    public TextEditor Editor => _editors[0];
    private bool IsCard => _s.Id == "card";
    private bool IsPro => _s.Id == "pro";
    private static IBrush B(string hex) => new SolidColorBrush(Color.Parse(hex));
    private IBrush Ink => new SolidColorBrush(_s.Look.Foreground);
    private IBrush Muted => new SolidColorBrush(_s.Look.Muted);

    public DesignWindow(string direction)
    {
        _s = DesignSpec.Get(direction);
        Width = 960; Height = 800; CanResize = false;
        Title = "AntiQuickPad / " + _s.Title;
        WindowDecorations = WindowDecorations.Full;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        RequestedThemeVariant = IsPro ? ThemeVariant.Dark : ThemeVariant.Light;
        Background = B(_s.Canvas); Foreground = Ink;
        FontFamily = new FontFamily("Segoe UI Variable Text");
        _root.Background = Background; _capture.Child = _root; Content = _capture;
        _root.Children.Add(_viewport);
        var style = new Style(x => x.OfType<ScrollBar>());
        style.Setters.Add(new Setter(ScrollBar.WidthProperty, 6d));
        style.Setters.Add(new Setter(ScrollBar.OpacityProperty, .55));
        style.Setters.Add(new Setter(ScrollBar.BackgroundProperty, Brushes.Transparent));
        Styles.Add(style);
        var thumb = new Style(x => x.OfType<ScrollBar>().Descendant().OfType<Thumb>());
        thumb.Setters.Add(new Setter(Thumb.BackgroundProperty, Muted));
        thumb.Setters.Add(new Setter(Thumb.MinHeightProperty, 28d));
        Styles.Add(thumb);
        if (IsCard) BuildCardPages(); else BuildFlatPages();
        _placeholder = new Border
        {
            Margin = IsCard ? new Thickness(146, 150, 80, 0) : IsPro ? new Thickness(106, 132, 50, 0) : new Thickness(112, 145, 80, 0),
            VerticalAlignment = VerticalAlignment.Top, IsHitTestVisible = false,
            Child = new StackPanel { Spacing = 16, Children = {
                Text("A fresh thought.", IsPro ? 27 : 34, Ink, FontWeight.SemiBold),
                Text("Start typing. This space is yours.", 17, Muted),
                Text(IsPro ? "Markdown  /  Tab indent  /  Esc hide" : "Markdown welcome. Everything saves as you go.", 13, Muted)
            }}
        };
        _root.Children.Add(_placeholder); _placeholder.IsVisible = false;
        _root.Children.Add(_bars);
        _position = Text(IsPro ? "03 / 08" : "3 of 8", 12, Muted);
        _save = Text("All changes saved", 12, Muted);
        _dots = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 7, VerticalAlignment = VerticalAlignment.Center };
        for (var i = 0; i < 8; i++) _dots.Children.Add(new Border { Width = i == 2 ? 20 : 5, Height = 5, CornerRadius = new CornerRadius(3), Background = i == 2 ? new SolidColorBrush(_s.Look.Accent) : B("#A4B1C9"), Opacity = i == 2 ? 1 : .5 });
        if (IsCard) BuildCardBars(); else if (IsPro) BuildProBars(); else BuildQuietBars();
        _menu = MakeMenu(); _menu.IsVisible = false; _root.Children.Add(_menu);
    }

    private TextBlock Text(string value, double size, IBrush? brush = null, FontWeight? weight = null) => new()
    { Text = value, FontSize = size, Foreground = brush ?? Ink, FontWeight = weight ?? FontWeight.Normal, VerticalAlignment = VerticalAlignment.Center };
    private Button IconButton(string glyph, string tip, bool highlighted = false)
    {
        var button = new Button
        {
            Content = new TextBlock { Text = glyph, FontFamily = new FontFamily("Segoe Fluent Icons"), FontSize = 15, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
            Width = 32, Height = 30, Padding = new Thickness(0), CornerRadius = new CornerRadius(6),
            Background = highlighted ? B(IsPro ? "#354254" : IsCard ? "#E4E9F7" : "#E8ECE6") : Brushes.Transparent,
            Foreground = Ink, BorderThickness = new Thickness(0), VerticalAlignment = VerticalAlignment.Center
        };
        ToolTip.SetTip(button, tip);
        if (tip == "More") _moreButton = button;
        if (tip == "More") button.Click += (_, _) => _menu.IsVisible = !_menu.IsVisible;
        if (tip == "Hide") button.Click += (_, _) => Hide();
        if (tip == "Minimize") button.Click += (_, _) => WindowState = WindowState.Minimized;
        if (tip == "Pin") button.Click += (_, _) => Topmost = !Topmost;
        return button;
    }
    private StackPanel Row(params Control[] controls)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, VerticalAlignment = VerticalAlignment.Center };
        foreach (var c in controls) row.Children.Add(c); return row;
    }
    private Border Rule() => new() { Width = 1, Height = 16, Background = B(_s.Rule), Margin = new Thickness(7, 0) };
    private void Place(Control control, HorizontalAlignment horizontal, VerticalAlignment vertical, Thickness margin)
    { control.HorizontalAlignment = horizontal; control.VerticalAlignment = vertical; control.Margin = margin; _bars.Children.Add(control); }

    private TextEditor MakeEditor(string text)
    {
        var editor = EditorFactory.Create(_s.Look); editor.FontWeight = FontWeight.Normal; editor.Text = text;
        editor.TextArea.TextView.LineTransformers.Insert(0, new BaseTypeface(_s.Look.EditorFont));
        ChevronMargin.Install(editor, Muted);
        editor.TextArea.TextView.BackgroundRenderers.Insert(0, new ActiveLine(editor, B(IsPro ? "#303B49" : IsCard ? "#E9EDF9" : "#EAF0E9")));
        editor.CaretOffset = editor.Document.GetLineByNumber(Math.Min(13, editor.Document.LineCount)).Offset + Math.Min(20, editor.Document.GetLineByNumber(Math.Min(13, editor.Document.LineCount)).Length);
        _editors.Add(editor); return editor;
    }
    private void BuildFlatPages()
    {
        _viewport.Margin = IsPro ? new Thickness(56, 88, 16, 70) : new Thickness(78, 105, 78, 60);
        for (var i = 0; i < 2; i++)
        {
            var editor = MakeEditor(i == 0 ? Fixture : NextFixture);
            editor.Margin = IsPro ? new Thickness(12, 10, 20, 8) : new Thickness(12, 0, 12, 0);
            var page = new Border { Background = B(_s.Surface), Child = editor };
            _viewport.Children.Add(page); _pages.Add(page); if (i == 1) page.IsVisible = false;
        }
    }
    private void BuildCardPages()
    {
        _viewport.Margin = new Thickness(0, 82, 0, 62);
        foreach (var (left, title) in new[] { (-794d, "A small collection"), (890d, "Make it feel effortless") })
        {
            var peek = new Border { Width = 820, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(left, 18, 0, 18), CornerRadius = new CornerRadius(18),
                Background = B("#EDF1F8"), BorderBrush = B("#C8D2E3"), BorderThickness = new Thickness(1),
                Child = new Border { Margin = new Thickness(42, 28, 0, 0), VerticalAlignment = VerticalAlignment.Top, Child = Text(title, 20, Muted) } };
            _viewport.Children.Add(peek);
        }
        for (var i = 0; i < 2; i++)
        {
            var editor = MakeEditor(i == 0 ? Fixture : NextFixture);
            var page = new Border { Width = 816, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(72, 0, 0, 0),
                Padding = new Thickness(44, 28, 38, 16), CornerRadius = new CornerRadius(16), Background = B(_s.Surface),
                BorderBrush = B("#CED7E9"), BorderThickness = new Thickness(1), BoxShadow = new BoxShadows(new BoxShadow { OffsetY = 8, Blur = 22, Spread = -5, Color = Color.Parse("#2533486C") }), Child = editor };
            _viewport.Children.Add(page); _pages.Add(page); if (i == 1) page.IsVisible = false;
        }
    }
    private void BuildQuietBars()
    {
        Place(Row(Text("AntiQuickPad", 13, Ink, FontWeight.SemiBold)), HorizontalAlignment.Left, VerticalAlignment.Top, new Thickness(28, 20, 0, 0));
        Place(Row(IconButton("\uE710", "New note"), IconButton("\uE712", "More"), Rule(), IconButton("\uE718", "Pin"), IconButton("\uE921", "Minimize"), IconButton("\uE8BB", "Hide")), HorizontalAlignment.Right, VerticalAlignment.Top, new Thickness(0, 14, 20, 0));
        Place(Text("PERSONAL  /  TODAY", 10, Muted), HorizontalAlignment.Left, VerticalAlignment.Top, new Thickness(112, 72, 0, 0));
        Place(Row(IconButton("\uE76B", "Previous"), _position, IconButton("\uE76C", "Next")), HorizontalAlignment.Left, VerticalAlignment.Bottom, new Thickness(28, 0, 0, 17));
        Place(Row(Text("\uE73E", 11, new SolidColorBrush(_s.Look.Accent)), _save), HorizontalAlignment.Center, VerticalAlignment.Bottom, new Thickness(0, 0, 0, 23));
        Place(Row(Text("Esc to hide", 11, Muted), IconButton("\uE74D", "Delete note")), HorizontalAlignment.Right, VerticalAlignment.Bottom, new Thickness(0, 0, 24, 17));
    }
    private Border Floating(Control child) => new() { Padding = new Thickness(9, 5), CornerRadius = new CornerRadius(12), Background = B("#F8FAFF"), BorderBrush = B("#CCD6E7"), BorderThickness = new Thickness(1), BoxShadow = new BoxShadows(new BoxShadow { OffsetY = 3, Blur = 12, Color = Color.Parse("#1233486C") }), Child = child };
    private void BuildCardBars()
    {
        Place(Row(Text("AntiQuickPad", 14, Ink, FontWeight.SemiBold), Text("  /  Notes", 13, Muted)), HorizontalAlignment.Left, VerticalAlignment.Top, new Thickness(30, 28, 0, 0));
        Place(Floating(Row(IconButton("\uE710", "New note", true), IconButton("\uE74D", "Delete note"), IconButton("\uE712", "More"), Rule(), IconButton("\uE718", "Pin"), IconButton("\uE921", "Minimize"), IconButton("\uE8BB", "Hide"))), HorizontalAlignment.Right, VerticalAlignment.Top, new Thickness(0, 17, 27, 0));
        Place(Floating(Row(IconButton("\uE76B", "Previous"), _dots, IconButton("\uE76C", "Next"))), HorizontalAlignment.Center, VerticalAlignment.Bottom, new Thickness(0, 0, 0, 12));
        Place(_position, HorizontalAlignment.Left, VerticalAlignment.Bottom, new Thickness(33, 0, 0, 28));
        Place(_save, HorizontalAlignment.Right, VerticalAlignment.Bottom, new Thickness(0, 0, 31, 28));
    }
    private Border Chip(string text) => new() { CornerRadius = new CornerRadius(4), BorderBrush = B(_s.Rule), BorderThickness = new Thickness(1), Padding = new Thickness(7, 3), Child = Text(text, 10, Muted) };
    private void BuildProBars()
    {
        Place(new Border { Height = 48, Background = B("#252C35"), BorderBrush = B(_s.Rule), BorderThickness = new Thickness(0, 0, 0, 1) }, HorizontalAlignment.Stretch, VerticalAlignment.Top, new Thickness(0));
        Place(Row(IconButton("\uE8A5", "Notes"), Text("AntiQuickPad", 12, Ink, FontWeight.SemiBold), Rule(), Text("workspace.md", 12, Muted)), HorizontalAlignment.Left, VerticalAlignment.Top, new Thickness(12, 9, 0, 0));
        Place(Row(IconButton("\uE710", "New note"), IconButton("\uE74D", "Delete note"), IconButton("\uE712", "More"), Rule(), IconButton("\uE718", "Pin"), IconButton("\uE921", "Minimize"), IconButton("\uE8BB", "Hide")), HorizontalAlignment.Right, VerticalAlignment.Top, new Thickness(0, 9, 12, 0));
        Place(Row(IconButton("\uE76B", "Previous"), _position, IconButton("\uE76C", "Next"), Rule(), Text("PERSONAL  /  A CALMER WORKSPACE", 10, Muted)), HorizontalAlignment.Left, VerticalAlignment.Top, new Thickness(64, 54, 0, 0));
        Place(new Border { Width = 48, Background = B("#1D2229"), BorderBrush = B(_s.Rule), BorderThickness = new Thickness(0, 0, 1, 0), Child = new StackPanel { Spacing = 16, Margin = new Thickness(7, 16, 7, 0), Children = { IconButton("\uE8A5", "Notes", true), IconButton("\uE721", "Search") } } }, HorizontalAlignment.Left, VerticalAlignment.Stretch, new Thickness(0, 48, 0, 55));
        Place(Row(Chip("Tab  indent"), Chip("Ctrl+N  new note"), Chip("Esc  hide")), HorizontalAlignment.Left, VerticalAlignment.Bottom, new Thickness(76, 0, 0, 41));
        var status = new Border { Height = 30, Background = B("#191E25"), BorderBrush = B(_s.Rule), BorderThickness = new Thickness(0, 1, 0, 0) };
        Place(status, HorizontalAlignment.Stretch, VerticalAlignment.Bottom, new Thickness(0));
        Place(Row(_lineInfo = Text("Ln 13, Col 21", 11, Muted), Rule(), _wordInfo = Text($"{Fixture.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length} words", 11, Muted)), HorizontalAlignment.Left, VerticalAlignment.Bottom, new Thickness(18, 0, 0, 8));
        Place(Row(Text("Markdown", 11, Muted), Rule(), Text("UTF-8", 11, Muted), Rule(), _save), HorizontalAlignment.Right, VerticalAlignment.Bottom, new Thickness(0, 0, 18, 8));
    }
    private Border MakeMenu()
    {
        var list = new StackPanel { Spacing = 2 };
        void Item(string glyph, string label, string shortcut)
        {
            var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("28,*,Auto") };
            var icon = Text(glyph, 14, Muted); icon.FontFamily = new FontFamily("Segoe Fluent Icons");
            grid.Children.Add(icon);
            var name = Text(label, 13); Grid.SetColumn(name, 1); grid.Children.Add(name);
            var key = Text(shortcut, 11, Muted); Grid.SetColumn(key, 2); grid.Children.Add(key);
            list.Children.Add(new Button { Content = grid, HorizontalContentAlignment = HorizontalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, Height = 38,
                Padding = new Thickness(10, 0), Background = Brushes.Transparent, BorderThickness = new Thickness(0), CornerRadius = new CornerRadius(5) });
        }
        Item("\uE710", "New note", "Ctrl+N"); Item("\uE8B7", "Open notes folder", "");
        list.Children.Add(new Border { Height = 1, Background = B(_s.Rule), Margin = new Thickness(9, 6) });
        Item("\uE713", "Settings", ""); Item("\uE765", "Keyboard shortcuts", "");
        list.Children.Add(new Border { Height = 1, Background = B(_s.Rule), Margin = new Thickness(9, 6) });
        Item("\uE7E8", "Quit AntiQuickPad", "");
        return new Border { Width = 286, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, IsCard ? 66 : 52, 24, 0),
            Padding = new Thickness(5), CornerRadius = new CornerRadius(10), BorderBrush = B(_s.Rule), BorderThickness = new Thickness(1), Background = B(_s.Surface),
            BoxShadow = new BoxShadows(new BoxShadow { OffsetY = 10, Blur = 30, Color = Color.Parse("#30000000") }), Child = list };
    }

    public async Task RenderStates(string output)
    {
        Directory.CreateDirectory(output);
        foreach (var state in new[] { "main", "bars", "menu", "carousel", "empty" })
        {
            _menu.IsVisible = state == "menu";
            _bars.Opacity = state == "main" ? .65 : 1;
            _placeholder.IsVisible = state == "empty";
            if (_moreButton is not null) _moreButton.Background = state is "bars" or "menu" ? B(IsPro ? "#354254" : IsCard ? "#E4E9F7" : "#E8ECE6") : Brushes.Transparent;
            if (_lineInfo is not null) _lineInfo.Text = state == "empty" ? "Ln 1, Col 1" : "Ln 13, Col 21";
            if (_wordInfo is not null) _wordInfo.Text = state == "empty" ? "0 words" : $"{Fixture.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length} words";
            for (var i = 0; i < _dots.Children.Count; i++) { _dots.Children[i].Width = i == (state == "empty" ? 7 : 2) ? 20 : 5; ((Border)_dots.Children[i]).Background = i == (state == "empty" ? 7 : 2) ? new SolidColorBrush(_s.Look.Accent) : B("#A4B1C9"); }
            Editor.Text = state == "empty" ? "" : Fixture;
            Editor.CaretOffset = state == "empty" ? 0 : Editor.Document.GetLineByNumber(13).Offset + 20;
            _position.Text = state == "empty" ? (IsPro ? "09 / 09" : "9 of 9") : (IsPro ? "03 / 08" : "3 of 8");
            _save.Text = state == "empty" ? "Ready to write" : "All changes saved";
            _pages[1].IsVisible = state == "carousel";
            // Deterministic mid-slide pose: same two real editor visual trees as the runtime carousel.
            _pages[0].RenderTransform = new TranslateTransform(state == "carousel" ? (IsCard ? -340 : -350) : 0, 0);
            _pages[1].RenderTransform = new TranslateTransform(IsCard ? 500 : 530, 0);
            await Task.Delay(100);
            UpdateLayout(); Editor.TextArea.Focus(); Editor.TextArea.TextView.EnsureVisualLines();
            foreach (var scale in new[] { 1d, 1.5d })
            {
                if (state == "empty") Editor.TextArea.Caret.Hide(); else Editor.TextArea.Caret.Show();
                await Task.Delay(25); // Flush caret invalidation before recording either scale.
                using var bitmap = new RenderTargetBitmap(new PixelSize((int)(960 * scale), (int)(800 * scale)), new Vector(96, 96));
                _capture.Measure(new Size(960 * scale, 800 * scale));
                _capture.Arrange(new Rect(0, 0, 960 * scale, 800 * scale));
                bitmap.Render(_capture);
                var path = Path.Combine(output, state + (scale == 1 ? "@1x.png" : "@1.5x.png"));
                bitmap.Save(path, PngBitmapEncoderOptions.Default);
                Console.WriteLine(path);
            }
        }
    }

    private sealed class BaseTypeface(string font) : DocumentColorizingTransformer
    {
        protected override void ColorizeLine(AvaloniaEdit.Document.DocumentLine line)
        {
            if (line.Length > 0) ChangeLinePart(line.Offset, line.EndOffset,
                e => e.TextRunProperties.SetTypeface(new Typeface(new FontFamily(font), FontStyle.Normal, FontWeight.Normal)));
        }
    }

    private sealed class ActiveLine(TextEditor editor, IBrush brush) : IBackgroundRenderer
    {
        public KnownLayer Layer => KnownLayer.Background;
        public void Draw(TextView view, DrawingContext dc)
        {
            if (!view.VisualLinesValid || view.Document == null || view.Document.TextLength == 0) return;
            var visual = view.VisualLines.FirstOrDefault(v => v.FirstDocumentLine.LineNumber <= editor.TextArea.Caret.Line && v.LastDocumentLine.LineNumber >= editor.TextArea.Caret.Line);
            if (visual == null) return;
            dc.DrawRectangle(brush, null, new Rect(0, visual.VisualTop - view.VerticalOffset, view.Bounds.Width, visual.Height), 3, 3);
        }
    }
}





