using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace AntiQuickPad.Editor;

// Selection is drawn above Text. No glyph run is cloned or decorated by this renderer.
internal sealed class StrikeRenderer(Look look, MarkdownScanner scanner) : IBackgroundRenderer
{
    internal List<Rect> StrokeBounds { get; } = [];
    public KnownLayer Layer => KnownLayer.Selection;
    public void Draw(TextView view, DrawingContext dc)
    {
        StrokeBounds.Clear();
        if (!view.VisualLinesValid || view.Document is null) return;
        foreach (var visual in view.VisualLines)
        foreach (var lineNumber in Enumerable.Range(visual.FirstDocumentLine.LineNumber,
                     visual.LastDocumentLine.LineNumber - visual.FirstDocumentLine.LineNumber + 1))
        {
            var info = scanner.GetLine(lineNumber);
            if (info.IsCode) continue;
            var line = view.Document.GetLineByNumber(lineNumber);
            foreach (var span in info.Strike)
            {
                var start = line.Offset + span.Start + span.MarkerLength;
                var end = line.Offset + span.Start + span.Length - span.MarkerLength;
                // Split at visual elements to use the actual foreground, font size and face.
                foreach (var element in visual.Elements)
                {
                    var a = Math.Max(start, visual.FirstDocumentLine.Offset + element.RelativeTextOffset);
                    var b = Math.Min(end, visual.FirstDocumentLine.Offset + element.RelativeTextOffset + element.DocumentLength);
                    if (b <= a) continue;
                    var props = element.TextRunProperties;
                    var glyph = props.Typeface.GlyphTypeface;
                    glyph.TryGetGlyphMetrics(glyph.CharacterToGlyphMap['x'], out var metrics);
                    // Glyph Height can be negative (y-up design units) or zero for some faces; fall back to ~0.5 em.
                    var xheight = Math.Abs(metrics.Height) * props.FontRenderingEmSize / glyph.Metrics.DesignEmHeight;
                    if (xheight < 0.3 * props.FontRenderingEmSize) xheight = 0.5 * props.FontRenderingEmSize;
                    foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(view, new SimpleSegment(a, b - a)))
                    {
                        var column = visual.GetVisualColumn(a - visual.FirstDocumentLine.Offset);
                        var textLine = visual.GetTextLine(column);
                        var y = rect.Top + textLine.Baseline - xheight / 2;
                        var thickness = Math.Max(1.5, look.StrikeThicknessEm * props.FontRenderingEmSize);
                        StrokeBounds.Add(new Rect(rect.Left, y - thickness / 2, rect.Width, thickness));
                        dc.DrawLine(new Pen(props.ForegroundBrush, thickness),
                            new Point(rect.Left, y), new Point(rect.Right, y));
                    }
                }
            }
        }
    }
}
