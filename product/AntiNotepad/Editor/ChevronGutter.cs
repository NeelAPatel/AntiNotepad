using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Rendering;

namespace AntiNotepad.Editor;

/// <summary>
/// A quiet fold gutter: chevrons appear while the pointer is over the note (folded sections always show theirs),
/// replacing AvaloniaEdit's boxed tree markers. Click a chevron to fold or unfold.
/// </summary>
internal sealed class ChevronGutter : AbstractMargin
{
    private const double GutterWidth = 16, Arm = 4.2;
    private readonly FoldingManager _manager;
    private readonly PadView _pad;
    private bool _showAll;
    private Point? _hover;

    public ChevronGutter(FoldingManager manager, PadView pad)
    {
        _manager = manager; _pad = pad;
        Width = GutterWidth;
        Cursor = new Cursor(StandardCursorType.Hand);
    }

    /// <summary>Swaps the stock FoldingMargin that FoldingManager.Install added for this gutter.</summary>
    public static ChevronGutter Replace(TextArea area, FoldingManager manager, PadView pad)
    {
        var margins = area.LeftMargins;
        var at = 0;
        for (var i = margins.Count - 1; i >= 0; i--)
            if (margins[i] is FoldingMargin or ChevronGutter) { at = i; margins.RemoveAt(i); }
        var gutter = new ChevronGutter(manager, pad);
        margins.Insert(Math.Min(at, margins.Count), gutter);
        return gutter;
    }

    public bool ShowAll { set { if (_showAll != value) { _showAll = value; InvalidateVisual(); } } }

    protected override Size MeasureOverride(Size availableSize) => new(GutterWidth, 0);

    protected override void OnPointerMoved(PointerEventArgs e) { _hover = e.GetPosition(this); InvalidateVisual(); }
    protected override void OnPointerExited(PointerEventArgs e) { _hover = null; InvalidateVisual(); }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var target = Targets().Where(t => t.Bounds.Contains(e.GetPosition(this))).Select(t => t.Folding).FirstOrDefault();
        if (target is null) return;
        target.IsFolded = !target.IsFolded;
        e.Handled = true;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(Brushes.Transparent, null, new Rect(Bounds.Size)); // hit-testable everywhere
        var p = _pad.Palette;
        foreach (var (folding, bounds) in Targets())
        {
            var hot = _hover is { } h && bounds.Contains(h);
            if (!folding.IsFolded && !_showAll && !hot) continue;
            var color = hot ? p.Accent : folding.IsFolded ? p.Muted : p.Marker;
            var pen = new Pen(new SolidColorBrush(color), 1.6, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
            var c = bounds.Center;
            if (folding.IsFolded)
            {
                context.DrawLine(pen, new Point(c.X - Arm / 2, c.Y - Arm), new Point(c.X + Arm / 2, c.Y));
                context.DrawLine(pen, new Point(c.X + Arm / 2, c.Y), new Point(c.X - Arm / 2, c.Y + Arm));
            }
            else
            {
                context.DrawLine(pen, new Point(c.X - Arm, c.Y - Arm / 2), new Point(c.X, c.Y + Arm / 2));
                context.DrawLine(pen, new Point(c.X, c.Y + Arm / 2), new Point(c.X + Arm, c.Y - Arm / 2));
            }
        }
    }

    private IEnumerable<(FoldingSection Folding, Rect Bounds)> Targets()
    {
        if (TextView is not { VisualLinesValid: true } view || view.Document is null) yield break;
        foreach (var folding in _manager.AllFoldings)
        {
            var number = view.Document.GetLineByOffset(folding.StartOffset).LineNumber;
            var line = view.VisualLines.FirstOrDefault(l => l.FirstDocumentLine.LineNumber == number);
            if (line is null) continue;
            var top = line.VisualTop - view.VerticalOffset;
            yield return (folding, new Rect(0, top, GutterWidth, line.TextLines[0].Height));
        }
    }
}
