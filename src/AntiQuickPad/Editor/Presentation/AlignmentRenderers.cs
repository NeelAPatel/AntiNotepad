using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace AntiQuickPad.Editor;

// Overlay keeps the marker's normal text advance and baseline: no inline object changes row metrics.
internal sealed class CheckboxRenderer(Look look, MarkdownScanner scanner) : IBackgroundRenderer
{
    internal List<(int Offset, Rect Bounds)> Boxes { get; } = [];
    public KnownLayer Layer => KnownLayer.Selection;
    public void Draw(TextView view, DrawingContext dc)
    {
        Boxes.Clear();
        if (!view.VisualLinesValid || view.Document is null) return;
        foreach (var visual in view.VisualLines)
        {
            var line = visual.FirstDocumentLine;
            var info = scanner.GetLine(line.LineNumber);
            if (info.IsCode || info.CheckboxOffset < 0) continue;
            var offset = line.Offset + info.CheckboxOffset;
            var column = visual.GetVisualColumn(info.CheckboxOffset);
            var textLine = visual.GetTextLine(column);
            var props = visual.Elements.First(e => e.RelativeTextOffset <= info.CheckboxOffset && e.RelativeTextOffset + e.DocumentLength > info.CheckboxOffset).TextRunProperties;
            var glyph = props.Typeface.GlyphTypeface;
            glyph.TryGetGlyphMetrics(glyph.CharacterToGlyphMap['x'], out var metrics);
            var height = Math.Abs(metrics.Height) * props.FontRenderingEmSize / glyph.Metrics.DesignEmHeight;
            if (height < .3 * props.FontRenderingEmSize) height = .5 * props.FontRenderingEmSize;
            var rect = BackgroundGeometryBuilder.GetRectsForSegment(view, new SimpleSegment(offset, 3)).First();
            var center = new Point(rect.Center.X, rect.Top + textLine.Baseline - height / 2);
            var box = new Rect(center.X - look.CheckboxSize / 2, center.Y - look.CheckboxSize / 2, look.CheckboxSize, look.CheckboxSize);
            Boxes.Add((offset, box));
            var accent = new SolidColorBrush(look.Accent);
            dc.DrawRectangle(info.CheckboxChecked ? accent : null,
                new Pen(info.CheckboxChecked ? accent : new SolidColorBrush(look.Muted), look.CheckboxStroke), box, look.CheckboxRadius, look.CheckboxRadius);
            if (!info.CheckboxChecked) continue;
            // The path's ink bounds are symmetric around the box centre, including round caps.
            var geometry = new StreamGeometry();
            using (var g = geometry.Open())
            {
                g.BeginFigure(new Point(center.X - 4.75, center.Y + .25), false);
                g.LineTo(new Point(center.X - 1.25, center.Y + 3.75));
                g.LineTo(new Point(center.X + 4.75, center.Y - 3.75)); g.EndFigure(false);
            }
            dc.DrawGeometry(null, new Pen(new SolidColorBrush(look.CheckboxCheck), look.CheckboxCheckStroke, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round), geometry);
        }
    }
}

internal sealed class IndentGuideRenderer(Look look, MarkdownScanner scanner) : IBackgroundRenderer
{
    private double EditorSpace { get; } = SpaceWidth(look.EditorFont, look.FontSize);
    private double CodeSpace { get; } = SpaceWidth(look.CodeFont, look.FontSize);
    private static double SpaceWidth(string font, double size) => new FormattedText(" ",
        System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
        new Typeface(new FontFamily(font)), size, Brushes.White).WidthIncludingTrailingWhitespace;
    public KnownLayer Layer => KnownLayer.Background;
    private readonly GuideState _state = new(scanner);
    internal static int Columns(string text) => GuideState.Columns(text);
    internal int Depth(int number) => _state.Depth(number);
    internal (int Level, int First, int Last) ActiveBlock(int caretLine) => _state.ActiveBlock(caretLine);
    internal int CaretLine { get => _state.CaretLine; set => _state.CaretLine = value; }
    public void Draw(TextView view, DrawingContext dc)
    {
        if (!look.ShowIndentGuides || !view.VisualLinesValid) return;
        var pen = new Pen(new SolidColorBrush(look.IndentGuide), look.IndentGuideThickness);
        var activePen = new Pen(new SolidColorBrush(look.IndentGuideActive), look.IndentGuideThickness);
        var active = ActiveBlock(CaretLine);
        foreach (var visual in view.VisualLines)
        {
            var number = visual.FirstDocumentLine.LineNumber;
            var top = visual.VisualTop - view.VerticalOffset;
            for (var level = 1; level <= Depth(visual.FirstDocumentLine.LineNumber); level++)
            {
                var column = level * 4;
                var x = column * (scanner.GetLine(visual.FirstDocumentLine.LineNumber).IsCode ? CodeSpace : EditorSpace) - view.HorizontalOffset;
                x = Math.Floor(x) + .5;
                dc.DrawLine(level == active.Level && number >= active.First && number <= active.Last ? activePen : pen, new Point(x, top), new Point(x, top + visual.Height));
            }
        }
    }
}
