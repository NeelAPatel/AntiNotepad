// Draws the editor paper and decorations, including result chips, dividers, and checkboxes.
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.TextFormatting;
using AvaloniaEdit.Rendering;
using AntiNotepad.Model;

using AntiNotepad.Theme;

namespace AntiNotepad.Editor;

/// <summary>Small per-renderer resource caches use these immutable drawing resources.</summary>
internal static class RenderPaint
{
    internal static ImmutableSolidColorBrush Brush(Color color, double opacity = 1) =>
        new(color, opacity);

    internal static ImmutablePen Pen(
        ImmutableSolidColorBrush brush,
        double thickness,
        PenLineCap cap = PenLineCap.Flat,
        PenLineJoin join = PenLineJoin.Miter) =>
        new(brush, thickness, null, cap, join, 10);
}

/// <summary>Insertion-ordered bounded cache. Each cache is owned by one renderer and never grows past its limit.</summary>
internal sealed class BoundedCache<TKey, TValue>(int capacity) where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _values = [];
    private readonly Queue<TKey> _order = [];

    public bool TryGet(TKey key, out TValue value) => _values.TryGetValue(key, out value!);

    public void Add(TKey key, TValue value)
    {
        if (_values.ContainsKey(key)) return;
        if (_values.Count >= capacity) _values.Remove(_order.Dequeue());
        _values.Add(key, value);
        _order.Enqueue(key);
    }
}

/// <summary>
/// Paper texture behind the whole pane, with equal margins left and right. Columns are anchored at the text's
/// starting edge and rows at line boundaries, so letters sit on the pattern; it scrolls with the text.
/// </summary>
internal sealed class PaperLayer(PadView pad) : Avalonia.Controls.Control
{
    private readonly BoundedCache<BrushKey, ImmutableSolidColorBrush> _brushes = new(8);
    private readonly BoundedCache<PenKey, ImmutablePen> _pens = new(8);

    private readonly record struct BrushKey(Color Color, double Opacity);
    private readonly record struct PenKey(Color Color, double Opacity, double Thickness, double Scale);

    private double RenderScale => TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;

    private ImmutableSolidColorBrush Brush(Color color, double opacity = 1)
    {
        var key = new BrushKey(color, opacity);
        if (_brushes.TryGet(key, out var brush)) return brush;
        brush = RenderPaint.Brush(color, opacity);
        _brushes.Add(key, brush);
        return brush;
    }

    private ImmutablePen Pen(Color color, double opacity, double thickness)
    {
        var key = new PenKey(color, opacity, thickness, RenderScale);
        if (_pens.TryGet(key, out var pen)) return pen;
        pen = RenderPaint.Pen(Brush(color, opacity), thickness);
        _pens.Add(key, pen);
        return pen;
    }

    /// <summary>
    /// Fenced blocks get a rounded panel reaching 20 px past the text on both sides. It lives here, behind the
    /// editor, because the text view clips at the text's own left edge.
    /// </summary>
    private void DrawCodePanels(DrawingContext dc, TextView view, Point origin)
    {
        if (pad.Structure is not { } s || s.Fences.Count == 0 || !view.VisualLinesValid || pad.Mode == Model.NoteMode.Code) return;
        var p = pad.Palette;
        var fill = Brush(p.Ink, Paint.CompletionFill(p.Dark));
        using var clip = dc.PushClip(new Rect(0, origin.Y, Bounds.Width, view.Bounds.Height));
        foreach (var fence in s.Fences)
        {
            double top = double.NaN, bottom = 0;
            foreach (var v in view.VisualLines)
            {
                var n = v.FirstDocumentLine.LineNumber;
                if (n < fence.Open || n > fence.Close) continue;
                var y = origin.Y + v.VisualTop - view.VerticalOffset;
                if (double.IsNaN(top)) top = y;
                bottom = y + v.Height;
            }
            if (double.IsNaN(top)) continue;
            // Wide enough to hold the fold chevrons, which sit in the 16 px just left of the text.
            var left = origin.X - 20;
            dc.DrawRectangle(fill, null, new Rect(left, top - 2, Bounds.Width - 2 * left, bottom - top + 4), Shape.Popup, Shape.Popup);
        }
    }

    private static double LastColumn(double left, double right, double step) => left + Math.Floor((right + step / 2 - left) / step) * step;

    public PaperLayer Init() { IsHitTestVisible = false; return this; }

    public override void Render(DrawingContext dc)
    {
        var kind = pad.Settings.Paper;
        var view = pad.Editor.TextArea.TextView;
        var h = view.DefaultLineHeight;
        if (h <= 0 || view.TranslatePoint(new Point(0, 0), this) is not { } origin) return;
        DrawCodePanels(dc, view, origin);
        if (kind == "blank") return;
        var alpha = pad.Settings.PaperStrength switch { 0 => 0.55, 2 => 1.9, _ => 1.0 };
        var color = pad.Palette.Paper;
        var pen = Pen(color, Math.Min(1, alpha), 1);
        // Symmetric frame: the text's left margin, mirrored on the right. Columns start exactly at the text edge;
        // the last one may sit up to half a step past the mirror, so both margins come out within a few pixels.
        var left = origin.X; var right = Bounds.Width - origin.X;
        var top = origin.Y;
        var bottom = !pad.Private && pad.Settings.Bars == "docked" ? pad.Editor.Bounds.Bottom : Bounds.Height - 8;
        if (right <= left) return;
        var step = kind == "biggrid" ? h * 2 : h;
        // The inset adapter moves the TextView origin while the top whitespace remains; its own offset stays document-local.
        // Using both coordinates keeps paper rows attached to line boundaries without counting the inset twice.
        var rowPhase = origin.Y - view.VerticalOffset % step;
        double Snap(double v) => Math.Round(v) + 0.5;
        switch (kind)
        {
            case "lined":
                for (var y = rowPhase + h; y < bottom; y += h)
                    if (y > top) dc.DrawLine(pen, new Point(left, Snap(y)), new Point(LastColumn(left, right, h), Snap(y)));
                break;
            case "dotted":
                {
                    var dot = Brush(color, Math.Min(1, alpha * 2.2));
                    for (var y = rowPhase + h; y < bottom; y += h)
                    {
                        if (y <= top) continue;
                        for (var x = left; x <= right + h / 2; x += h) dc.DrawEllipse(dot, null, new Point(x, y), 1.1, 1.1);
                    }
                    break;
                }
            case "grid":
            case "biggrid":
                for (var y = rowPhase + step; y < bottom; y += step)
                    if (y > top) dc.DrawLine(pen, new Point(left, Snap(y)), new Point(LastColumn(left, right, step), Snap(y)));
                for (var x = left; x <= right + step / 2; x += step)
                    dc.DrawLine(pen, new Point(Snap(x), top), new Point(Snap(x), bottom));
                break;
        }
    }
}

/// <summary>Right-aligned result chips: math answers, inline `expr =` answers, timer countdowns. Chips are clickable.</summary>
internal sealed class ResultRenderer(PadView pad) : IBackgroundRenderer
{
    public readonly record struct Chip(Rect Bounds, int Line, string Kind, string Value);

    private readonly record struct TextKey(string Text, string Font, string Culture, double Size, double Scale, Color Color);
    private readonly record struct BrushKey(Color Color, double Opacity);
    private readonly record struct TimerPaintKey(Color Color, double Scale);
    private readonly record struct TimerGeometryKey(string Icon, double Width, double Height, double Scale);
    private readonly record struct TimerPaint(ImmutableSolidColorBrush Brush, ImmutablePen ReadyPen, ImmutablePen DonePen);

    private readonly BoundedCache<TextKey, FormattedText> _formatted = new(128);
    private readonly BoundedCache<BrushKey, ImmutableSolidColorBrush> _brushes = new(16);
    private readonly BoundedCache<TimerPaintKey, TimerPaint> _timerPaints = new(8);
    private readonly BoundedCache<TimerGeometryKey, StreamGeometry> _timerGeometries = new(16);
    private string? _typefaceFont;
    private Typeface? _typeface;

    public List<Chip> Chips { get; } = [];
    public KnownLayer Layer => KnownLayer.Selection;
    private (object?, int, int, double) _key;
    private double _column;

    private Typeface GetTypeface(string font)
    {
        if (_typeface is { } cached && StringComparer.Ordinal.Equals(_typefaceFont, font)) return cached;
        _typefaceFont = font;
        var typeface = new Typeface(new FontFamily(font + ", " + Typography.MonoFamily), FontStyle.Normal, Typography.Strong);
        _typeface = typeface;
        return typeface;
    }

    private ImmutableSolidColorBrush Brush(Color color, double opacity = 1)
    {
        var key = new BrushKey(color, opacity);
        if (_brushes.TryGet(key, out var brush)) return brush;
        brush = RenderPaint.Brush(color, opacity);
        _brushes.Add(key, brush);
        return brush;
    }

    private FormattedText FormatChip(string text, Color color, string font, CultureInfo culture, Typeface typeface, double size, double scale)
    {
        var key = new TextKey(text, font, culture.Name, size, scale, color);
        if (_formatted.TryGet(key, out var formatted)) return formatted;
        formatted = new FormattedText(text, culture, FlowDirection.LeftToRight, typeface, size, Brush(color));
        _formatted.Add(key, formatted);
        return formatted;
    }

    private TimerPaint TimerPaintFor(Color color, double scale)
    {
        var key = new TimerPaintKey(color, scale);
        if (_timerPaints.TryGet(key, out var paint)) return paint;
        var brush = Brush(color);
        paint = new TimerPaint(
            brush,
            RenderPaint.Pen(brush, Shape.Thin, join: PenLineJoin.Round),
            RenderPaint.Pen(brush, 1.6, PenLineCap.Round, PenLineJoin.Round));
        _timerPaints.Add(key, paint);
        return paint;
    }

    private StreamGeometry TimerGeometry(string icon, double width, double height, double scale)
    {
        var key = new TimerGeometryKey(icon, width, height, scale);
        if (_timerGeometries.TryGet(key, out var geometry)) return geometry;
        var s = width;
        var cx = s / 2; var cy = height / 2;
        geometry = new StreamGeometry();
        using (var c = geometry.Open())
        {
            if (icon == "ready")
            {
                c.BeginFigure(new Point(cx - s * 0.3, cy - s * 0.4), true);
                c.LineTo(new Point(cx + s * 0.4, cy));
                c.LineTo(new Point(cx - s * 0.3, cy + s * 0.4));
                c.EndFigure(true);
            }
            else
            {
                c.BeginFigure(new Point(cx - s * 0.38, cy), false);
                c.LineTo(new Point(cx - s * 0.08, cy + s * 0.3));
                c.LineTo(new Point(cx + s * 0.42, cy - s * 0.32));
            }
        }
        _timerGeometries.Add(key, geometry);
        return geometry;
    }

    public void Draw(TextView view, DrawingContext dc)
    {
        Chips.Clear();
        if (!view.VisualLinesValid || view.Document is null) return;
        var font = pad.Settings.Font;
        var typeface = GetTypeface(font);
        var size = pad.Settings.FontSize * Typography.EditorChipScale;
        var culture = CultureInfo.CurrentCulture;
        var scale = TopLevel.GetTopLevel(view)?.RenderScaling ?? 1;
        // Pass 1: every visible chip with its line geometry.
        var items = new List<(int Line, string Text, string Kind, Color Color, string Value, FormattedText Ft, double TextEnd, double Baseline)>();
        foreach (var visual in view.VisualLines)
        {
            if (visual.FirstDocumentLine != visual.LastDocumentLine) continue; // folded: its answers are hidden with it
            var n = visual.LastDocumentLine.LineNumber;
            if (pad.ChipFor(n) is not { } chip) continue;
            var ft = FormatChip(chip.Text, chip.Color, font, culture, typeface, size, scale);
            // The editor's own baseline for the line's last row: line spacing makes row arithmetic drift.
            var baseline = visual.GetVisualPosition(visual.VisualLength, VisualYPosition.Baseline).Y - view.VerticalOffset;
            items.Add((n, chip.Text, chip.Kind, chip.Color, chip.Value, ft, TextEnd(visual), baseline));
        }
        // Pass 2: every answer shares one column, just past the longest line that still leaves room for its chip.
        // A line too long for that falls back to sitting right after its own text.
        if (items.Count == 0) return;
        var fits = items.Where(x => x.TextEnd + 14 + x.Ft.Width + 30 <= view.Bounds.Width).ToList();
        var column = (fits.Count > 0 ? fits.Max(x => x.TextEnd) : 0) + 14;
        // Scrolling never pulls the column left; an edit or a resize lets it settle again.
        var key = (view.Document, view.Document.TextLength, view.Document.LineCount, view.Bounds.Width);
        if (key == _key) column = Math.Max(column, _column);
        (_key, _column) = (key, column);
        // "edge": every bubble right-aligned to the text's right edge instead.
        var edge = pad.Settings.Bubbles == "edge";
        foreach (var item in items) DrawChip(view, dc, item, edge ? double.PositiveInfinity : column, scale);
    }

    /// <summary>Returns the visual end of the last row, including its continuation indent.</summary>
    internal static double TextEnd(VisualLine visual) => visual.GetTextLineVisualXPosition(visual.TextLines[^1], visual.VisualLength);

    private void DrawChip(TextView view, DrawingContext dc, (int Line, string Text, string Kind, Color Color, string Value, FormattedText Ft, double TextEnd, double Baseline) c, double column, double scale)
    {
        var ft = c.Ft;
        var icon = c.Kind.StartsWith("timer:") ? c.Kind[6..] : null;
        var iconWidth = icon is null ? 0 : ft.Height * 0.62 + 6;
        var width = ft.Width + iconWidth;
        // The shared column, pulled left only if the window is too narrow, never onto the text.
        var x = Math.Max(c.TextEnd + 10, Math.Min(column, view.Bounds.Width - width - 8));
        // Baseline-aligned with the line's text.
        var y = c.Baseline - ft.Baseline;
        var rect = new Rect(x - 7, y - 2, width + 14, ft.Height + 4);
        dc.DrawRectangle(Brush(c.Color, Paint.BadgeFill(pad.Palette.Dark)), null, rect, 4, 4);
        if (icon is not null) DrawTimerIcon(dc, icon, new Rect(x, y, ft.Height * 0.62, ft.Height), c.Color, scale);
        dc.DrawText(ft, new Point(x + iconWidth, y));
        Chips.Add(new Chip(rect, c.Line, c.Kind, c.Value));
    }

    /// <summary>Play, pause, running dot and done check, drawn so they match the text weight in any font.</summary>
    private void DrawTimerIcon(DrawingContext dc, string icon, Rect box, Color color, double scale)
    {
        var paint = TimerPaintFor(color, scale);
        var brush = paint.Brush;
        var s = box.Width;
        var cx = box.X + s / 2; var cy = box.Y + box.Height / 2;
        switch (icon)
        {
            case "paused":
                {
                    var bar = s * 0.24; var h = s * 0.78;
                    dc.DrawRectangle(brush, null, new Rect(cx - s * 0.3, cy - h / 2, bar, h), 1, 1);
                    dc.DrawRectangle(brush, null, new Rect(cx + s * 0.06, cy - h / 2, bar, h), 1, 1);
                    break;
                }
            case "ready":
                {
                    using (dc.PushTransform(Matrix.CreateTranslation(box.X, box.Y)))
                        dc.DrawGeometry(brush, paint.ReadyPen, TimerGeometry(icon, box.Width, box.Height, scale));
                    break;
                }
            case "running":
                dc.DrawEllipse(brush, null, new Point(cx, cy), s * 0.28, s * 0.28);
                break;
            case "done":
                {
                    using (dc.PushTransform(Matrix.CreateTranslation(box.X, box.Y)))
                        dc.DrawGeometry(null, paint.DonePen, TimerGeometry(icon, box.Width, box.Height, scale));
                    break;
                }
        }
    }
}

/// <summary>A `---` line becomes a thin rule across the note (the dashes stay faint, or hide with clean markdown).</summary>
internal sealed class DividerRenderer(PadView pad) : IBackgroundRenderer
{
    private readonly record struct PenKey(Color Color, double Scale);
    private readonly BoundedCache<PenKey, ImmutablePen> _pens = new(4);

    public KnownLayer Layer => KnownLayer.Background;

    public void Draw(TextView view, DrawingContext dc)
    {
        if (!view.VisualLinesValid || view.Document is null || !pad.Decorate || pad.Mode is NoteMode.Code or NoteMode.Math) return;
        ImmutablePen? pen = null;
        foreach (var visual in view.VisualLines)
        {
            var line = visual.FirstDocumentLine;
            if (pad.InFence(line.LineNumber) || !NoteColorizer.Divider.IsMatch(view.Document.GetText(line))) continue;
            if (pen is null)
            {
                var key = new PenKey(pad.Palette.Marker, TopLevel.GetTopLevel(view)?.RenderScaling ?? 1);
                if (!_pens.TryGet(key, out pen))
                {
                    pen = RenderPaint.Pen(RenderPaint.Brush(key.Color, Paint.KeyPen), Shape.Thin);
                    _pens.Add(key, pen);
                }
            }
            var y = Math.Round(visual.VisualTop + visual.Height / 2 - view.VerticalOffset) + 0.5;
            dc.DrawLine(pen, new Point(0, y), new Point(view.Bounds.Width, y));
        }
    }
}

/// <summary>Draws rounded checkboxes over the blank runs CheckboxGenerator leaves, so they read the same in every font.</summary>
internal sealed class CheckboxRenderer(PadView pad) : IBackgroundRenderer
{
    private readonly record struct PaintKey(Color Accent, Color Canvas, Color Done, double Scale);
    private readonly record struct CheckKey(double Size, double Scale);
    private readonly record struct Paint(ImmutablePen Stroke, ImmutablePen Check, ImmutableSolidColorBrush Fill);
    private readonly BoundedCache<PaintKey, Paint> _paints = new(8);
    private readonly BoundedCache<CheckKey, StreamGeometry> _checkMarks = new(8);

    public KnownLayer Layer => KnownLayer.Text;

    private StreamGeometry CheckMark(double size, double scale)
    {
        var key = new CheckKey(size, scale);
        if (_checkMarks.TryGet(key, out var geometry)) return geometry;
        geometry = new StreamGeometry();
        using (var c = geometry.Open())
        {
            c.BeginFigure(new Point(size * .24, size * .52), false);
            c.LineTo(new Point(size * .43, size * .71));
            c.LineTo(new Point(size * .77, size * .3));
        }
        _checkMarks.Add(key, geometry);
        return geometry;
    }

    public void Draw(TextView view, DrawingContext dc)
    {
        if (!view.VisualLinesValid || view.Document is null || !pad.Decorate) return;
        var p = pad.Palette;
        var size = Math.Round(pad.Settings.FontSize * Shape.CheckboxSizeScale);
        var key = new PaintKey(p.Accent, p.Canvas, p.Done, TopLevel.GetTopLevel(view)?.RenderScaling ?? 1);
        if (!_paints.TryGet(key, out var paint))
        {
            paint = new Paint(
                RenderPaint.Pen(RenderPaint.Brush(p.Accent, AntiNotepad.Theme.Paint.AccentPen), Shape.CheckboxAccentPen),
                RenderPaint.Pen(RenderPaint.Brush(p.Canvas), Shape.CheckboxCheckPen, PenLineCap.Round, PenLineJoin.Round),
                RenderPaint.Brush(p.Done));
            _paints.Add(key, paint);
        }
        foreach (var visual in view.VisualLines)
        {
            var line = visual.FirstDocumentLine;
            var text = view.Document.GetText(line);
            var m = ListEditing.Checkbox.Match(text);
            if (!m.Success) continue;
            var column = visual.GetVisualColumn(m.Groups[1].Length);
            var left = visual.GetVisualPosition(column, VisualYPosition.TextMiddle);
            var right = visual.GetVisualPosition(column + 1, VisualYPosition.TextMiddle);
            var cx = (left.X + right.X) / 2 - view.HorizontalOffset;
            var cy = left.Y - view.VerticalOffset;
            var rect = new Rect(cx - size / 2, cy - size / 2 + 0.5, size, size);
            if (m.Groups[2].Value == " ") dc.DrawRectangle(null, paint.Stroke, rect.Deflate(0.8), size * Shape.CheckboxRadiusScale, size * Shape.CheckboxRadiusScale);
            else
            {
                dc.DrawRectangle(paint.Fill, null, rect, size * Shape.CheckboxRadiusScale, size * Shape.CheckboxRadiusScale);
                using (dc.PushTransform(Matrix.CreateTranslation(rect.X, rect.Y)))
                    dc.DrawGeometry(null, paint.Check, CheckMark(size, key.Scale));
            }
        }
    }
}
