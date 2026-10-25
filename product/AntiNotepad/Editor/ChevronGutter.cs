using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Rendering;
using AntiNotepad.Theme;

namespace AntiNotepad.Editor;

/// <summary>One-entry owner cache for the gutter's three immutable stroke resources.</summary>
internal sealed class ChevronPens
{
    private readonly record struct Key(Palette Palette, double Scale);
    private Key? _key;
    private ChevronPenSet? _current;

    internal ChevronPenSet Get(double scale, Palette palette)
    {
        if (!double.IsFinite(scale) || scale <= 0) scale = 1;
        var key = new Key(palette, scale);
        if (_current is not null && _key == key) return _current;
        _key = key;
        return _current = new ChevronPenSet(palette);
    }
}

internal sealed class ChevronPenSet
{
    public ImmutablePen Accent { get; }
    public ImmutablePen Muted { get; }
    public ImmutablePen Marker { get; }

    internal ChevronPenSet(Palette palette)
    {
        Accent = Create(palette.Accent);
        Muted = Create(palette.Muted);
        Marker = Create(palette.Marker);
    }

    private static ImmutablePen Create(Color color) =>
        new(new ImmutableSolidColorBrush(color), 1.6, null, PenLineCap.Round, PenLineJoin.Round, 10);
}

/// <summary>
/// A quiet fold gutter: chevrons appear while the pointer is over the note (folded sections always show theirs),
/// replacing AvaloniaEdit's boxed tree markers. Click a chevron to fold or unfold.
/// </summary>
internal sealed class ChevronGutter : AbstractMargin
{
    private const double GutterWidth = 16, Arm = 4.2;
    private readonly FoldingManager _manager;
    private readonly PadView _pad;
    private readonly ChevronPens _pens = new();
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
        var scale = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
        var pens = _pens.Get(scale, p);
        foreach (var (folding, bounds) in Targets())
        {
            var hot = _hover is { } h && bounds.Contains(h);
            if (!folding.IsFolded && !_showAll && !hot) continue;
            var pen = hot ? pens.Accent : folding.IsFolded ? pens.Muted : pens.Marker;
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
