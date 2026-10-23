using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Folding;

namespace AntiQuickPad.Editor;

/// <summary>Compact fold controls drawn as chevrons in place of AvaloniaEdit's boxed markers.</summary>
internal sealed class ChevronMargin : AbstractMargin
{
    private const double MarginWidth = 24;
    private const double ChevronSize = 5;
    private const double HitRadius = 12;

    private readonly FoldingManager _foldingManager;
    private readonly Pen _pen;
    private Point? _hover;
    private Action? _releaseInput;
    internal void ReleaseInput() { _releaseInput?.Invoke(); _releaseInput = null; }
    internal IReadOnlyList<Rect> HitTargets => Targets().Select(t => t.Bounds).ToArray();
    internal IEnumerable<FoldingSection> Foldings => _foldingManager.AllFoldings;
    internal void HoverAt(Point? point) { _hover = point; InvalidateVisual(); }

    private ChevronMargin(FoldingManager foldingManager, IBrush brush)
    {
        _foldingManager = foldingManager;
        _pen = new Pen(brush, 1.5, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        Width = MarginWidth;
        _releaseInput = EditorBehaviour.AttachChevron(this);
        Cursor = new Cursor(StandardCursorType.Hand);

    }

    /// <summary>Replaces the stock folding margin while preserving its installed folding manager.</summary>
    public static void Install(TextEditor editor, IBrush brush)
    {
        ArgumentNullException.ThrowIfNull(editor);
        ArgumentNullException.ThrowIfNull(brush);

        var margins = editor.TextArea.LeftMargins;
        var existingChevron = margins.OfType<ChevronMargin>().FirstOrDefault();
        if (existingChevron is not null)
        {
            existingChevron.ReleaseInput();
            var index = margins.IndexOf(existingChevron);
            margins[index] = new ChevronMargin(existingChevron._foldingManager, brush);
            return;
        }

        var firstStockIndex = -1;
        FoldingManager? foldingManager = null;
        for (var i = 0; i < margins.Count; i++)
        {
            if (margins[i] is not FoldingMargin stock) continue;
            if (firstStockIndex < 0) firstStockIndex = i;
            foldingManager ??= stock.FoldingManager;
        }

        // Install is idempotent only through the stock margin. If the caller has not
        // installed folding yet, create it here and then replace the generated margin.
        if (foldingManager is null)
        {
            foldingManager = FoldingManager.Install(editor.TextArea);
            firstStockIndex = margins.IndexOf(margins.OfType<FoldingMargin>().First());
        }

        for (var i = margins.Count - 1; i >= 0; i--)
            if (margins[i] is FoldingMargin) margins.RemoveAt(i);

        margins.Insert(Math.Clamp(firstStockIndex, 0, margins.Count), new ChevronMargin(foldingManager, brush));
    }

    protected override Size MeasureOverride(Size availableSize) => new(MarginWidth, 0);

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.DrawRectangle(Brushes.Transparent, null, new Rect(Bounds.Size));
        if (TextView is not { VisualLinesValid: true } view || view.Document is null) return;

        foreach (var folding in _foldingManager.AllFoldings)
        {
            var lineNumber = view.Document.GetLineByOffset(folding.StartOffset).LineNumber;
            var line = GetVisibleLine(view, lineNumber);
            if (line is null) continue;

            var center = new Point(Bounds.Width / 2, line.VisualTop - view.VerticalOffset + line.Height / 2);
            if (center.Y < 0 || center.Y > Bounds.Height) continue;
            var hit = new Rect(0, center.Y - Math.Max(24, line.Height) / 2, MarginWidth, Math.Max(24, line.Height));
            if (_hover is { } point && hit.Contains(point))
                context.DrawRectangle(new SolidColorBrush(Color.FromArgb(35, 128, 144, 160)), null, hit, 4, 4);
            DrawChevron(context, center, folding.IsFolded);
        }
    }

    private void DrawChevron(DrawingContext context, Point center, bool folded)
    {
        if (folded)
        {
            context.DrawLine(_pen, new Point(center.X - ChevronSize / 2, center.Y - ChevronSize),
                new Point(center.X + ChevronSize / 2, center.Y));
            context.DrawLine(_pen, new Point(center.X + ChevronSize / 2, center.Y),
                new Point(center.X - ChevronSize / 2, center.Y + ChevronSize));
        }
        else
        {
            context.DrawLine(_pen, new Point(center.X - ChevronSize, center.Y - ChevronSize / 2),
                new Point(center.X, center.Y + ChevronSize / 2));
            context.DrawLine(_pen, new Point(center.X, center.Y + ChevronSize / 2),
                new Point(center.X + ChevronSize, center.Y - ChevronSize / 2));
        }
    }

    internal FoldingSection? FoldingAt(Point point) => Targets().Where(t => t.Bounds.Contains(point))
        .OrderBy(t => Math.Abs(t.Bounds.Center.Y - point.Y)).Select(t => t.Folding).FirstOrDefault();

    private IEnumerable<(FoldingSection Folding, Rect Bounds)> Targets()
    {
        if (TextView is not { VisualLinesValid: true } view || view.Document is null) yield break;
        foreach (var folding in _foldingManager.AllFoldings)
        {
            var line = GetVisibleLine(view, view.Document.GetLineByOffset(folding.StartOffset).LineNumber);
            if (line is null) continue;
            var height = Math.Max(HitRadius * 2, line.Height);
            yield return (folding, new Rect(0, line.VisualTop - view.VerticalOffset + line.Height / 2 - height / 2, MarginWidth, height));
        }
    }

    // Only a fold starting on a visual line's first document line is visible; folds hidden inside a collapsed section are not.
    private static AvaloniaEdit.Rendering.VisualLine? GetVisibleLine(AvaloniaEdit.Rendering.TextView view, int lineNumber) =>
        view.VisualLines.FirstOrDefault(line => line.FirstDocumentLine.LineNumber == lineNumber);
}

