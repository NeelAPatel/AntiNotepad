using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using AvaloniaEdit.Rendering;
using Antipad.Model;

namespace Antipad.Editor;

/// <summary>
/// Paper texture behind the whole pane, with equal margins left and right. Columns are anchored at the text's
/// starting edge and rows at line boundaries, so letters sit on the pattern; it scrolls with the text.
/// </summary>
internal sealed class PaperLayer(PadView pad) : Avalonia.Controls.Control
{
    /// <summary>
    /// Fenced blocks get a rounded panel reaching 20 px past the text on both sides. It lives here, behind the
    /// editor, because the text view clips at the text's own left edge.
    /// </summary>
    private void DrawCodePanels(DrawingContext dc, TextView view, Point origin)
    {
        if (pad.Structure is not { } s || s.Fences.Count == 0 || !view.VisualLinesValid || pad.Mode == Model.NoteMode.Code) return;
        var p = pad.Palette;
        var fill = new SolidColorBrush(p.Ink, p.Dark ? 0.085 : 0.075);
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
            dc.DrawRectangle(fill, null, new Rect(left, top - 2, Bounds.Width - 2 * left, bottom - top + 4), 9, 9);
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
        var pen = new Pen(new SolidColorBrush(color, Math.Min(1, alpha)), 1);
        // Symmetric frame: the text's left margin, mirrored on the right. Columns start exactly at the text edge;
        // the last one may sit up to half a step past the mirror, so both margins come out within a few pixels.
        var left = origin.X; var right = Bounds.Width - origin.X;
        var top = origin.Y; var bottom = Bounds.Height - 8;
        if (right <= left) return;
        var step = kind == "biggrid" ? h * 2 : h;
        // Row phase follows scrolling; the first row boundary is the bottom of the first visible line.
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
                    var dot = new SolidColorBrush(color, Math.Min(1, alpha * 2.2));
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
    public List<Chip> Chips { get; } = [];
    public KnownLayer Layer => KnownLayer.Selection;
    private (object?, int, int, double) _key;
    private double _column;

    public void Draw(TextView view, DrawingContext dc)
    {
        Chips.Clear();
        if (!view.VisualLinesValid || view.Document is null) return;
        var typeface = new Typeface(new FontFamily(pad.Settings.Font + ", Cascadia Mono, Consolas"), FontStyle.Normal, FontWeight.SemiBold);
        var size = pad.Settings.FontSize * 0.93;
        // Pass 1: every visible chip with its line geometry.
        var items = new List<(int Line, string Text, string Kind, Color Color, string Value, FormattedText Ft, double TextEnd, double Baseline)>();
        foreach (var visual in view.VisualLines)
        {
            if (visual.FirstDocumentLine != visual.LastDocumentLine) continue; // folded: its answers are hidden with it
            var n = visual.LastDocumentLine.LineNumber;
            if (pad.ChipFor(n) is not { } chip) continue;
            var ft = new FormattedText(chip.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, size, new SolidColorBrush(chip.Color));
            var row = visual.TextLines[^1];
            // The editor's own baseline for the line's last row: line spacing makes row arithmetic drift.
            var baseline = visual.GetVisualPosition(visual.VisualLength, VisualYPosition.Baseline).Y - view.VerticalOffset;
            items.Add((n, chip.Text, chip.Kind, chip.Color, chip.Value, ft, row.WidthIncludingTrailingWhitespace, baseline));
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
        foreach (var item in items) DrawChip(view, dc, item, edge ? double.PositiveInfinity : column);
    }

    private void DrawChip(TextView view, DrawingContext dc, (int Line, string Text, string Kind, Color Color, string Value, FormattedText Ft, double TextEnd, double Baseline) c, double column)
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
        dc.DrawRectangle(new SolidColorBrush(c.Color, pad.Palette.Dark ? 0.14 : 0.10), null, rect, 7, 7);
        if (icon is not null) DrawTimerIcon(dc, icon, new Rect(x, y, ft.Height * 0.62, ft.Height), c.Color);
        dc.DrawText(ft, new Point(x + iconWidth, y));
        Chips.Add(new Chip(rect, c.Line, c.Kind, c.Value));
    }

    /// <summary>Play, pause, running dot and done check, drawn so they match the text weight in any font.</summary>
    private static void DrawTimerIcon(DrawingContext dc, string icon, Rect box, Color color)
    {
        var brush = new SolidColorBrush(color);
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
                    var g = new StreamGeometry();
                    using (var c = g.Open())
                    {
                        c.BeginFigure(new Point(cx - s * 0.3, cy - s * 0.4), true);
                        c.LineTo(new Point(cx + s * 0.4, cy));
                        c.LineTo(new Point(cx - s * 0.3, cy + s * 0.4));
                        c.EndFigure(true);
                    }
                    dc.DrawGeometry(brush, new Pen(brush, 1, lineJoin: PenLineJoin.Round), g);
                    break;
                }
            case "running":
                dc.DrawEllipse(brush, null, new Point(cx, cy), s * 0.28, s * 0.28);
                break;
            case "done":
                {
                    var pen = new Pen(brush, 1.6, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
                    var g = new StreamGeometry();
                    using (var c = g.Open())
                    {
                        c.BeginFigure(new Point(cx - s * 0.38, cy), false);
                        c.LineTo(new Point(cx - s * 0.08, cy + s * 0.3));
                        c.LineTo(new Point(cx + s * 0.42, cy - s * 0.32));
                    }
                    dc.DrawGeometry(null, pen, g);
                    break;
                }
        }
    }
}

/// <summary>A `---` line becomes a thin rule across the note (the dashes stay faint, or hide with clean markdown).</summary>
internal sealed class DividerRenderer(PadView pad) : IBackgroundRenderer
{
    public KnownLayer Layer => KnownLayer.Background;

    public void Draw(TextView view, DrawingContext dc)
    {
        if (!view.VisualLinesValid || view.Document is null || !pad.Decorate || pad.Mode is NoteMode.Code or NoteMode.Math) return;
        Pen? pen = null;
        foreach (var visual in view.VisualLines)
        {
            var line = visual.FirstDocumentLine;
            if (pad.InFence(line.LineNumber) || !NoteColorizer.Divider.IsMatch(view.Document.GetText(line))) continue;
            pen ??= new Pen(new SolidColorBrush(pad.Palette.Marker, .55), 1);
            var y = Math.Round(visual.VisualTop + visual.Height / 2 - view.VerticalOffset) + 0.5;
            dc.DrawLine(pen, new Point(0, y), new Point(view.Bounds.Width, y));
        }
    }
}

/// <summary>Draws rounded checkboxes over the blank runs CheckboxGenerator leaves, so they read the same in every font.</summary>
internal sealed class CheckboxRenderer(PadView pad) : IBackgroundRenderer
{
    public KnownLayer Layer => KnownLayer.Text;

    public void Draw(TextView view, DrawingContext dc)
    {
        if (!view.VisualLinesValid || view.Document is null || !pad.Decorate) return;
        var p = pad.Palette;
        var size = Math.Round(pad.Settings.FontSize * 0.86);
        var stroke = new Pen(new SolidColorBrush(p.Accent, .9), 1.6);
        var check = new Pen(new SolidColorBrush(p.Canvas), 1.9, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        var fill = new SolidColorBrush(p.Done);
        foreach (var visual in view.VisualLines)
        {
            var line = visual.FirstDocumentLine;
            var text = view.Document.GetText(line);
            var m = CheckboxGenerator.Box.Match(text);
            if (!m.Success) continue;
            var column = visual.GetVisualColumn(m.Groups[1].Length);
            var left = visual.GetVisualPosition(column, VisualYPosition.TextMiddle);
            var right = visual.GetVisualPosition(column + 1, VisualYPosition.TextMiddle);
            var cx = (left.X + right.X) / 2 - view.HorizontalOffset;
            var cy = left.Y - view.VerticalOffset;
            var rect = new Rect(cx - size / 2, cy - size / 2 + 0.5, size, size);
            if (m.Groups[2].Value == " ") dc.DrawRectangle(null, stroke, rect.Deflate(0.8), size * .28, size * .28);
            else
            {
                dc.DrawRectangle(fill, null, rect, size * .28, size * .28);
                var g = new StreamGeometry();
                using (var c = g.Open())
                {
                    c.BeginFigure(new Point(rect.X + size * .24, rect.Y + size * .52), false);
                    c.LineTo(new Point(rect.X + size * .43, rect.Y + size * .71));
                    c.LineTo(new Point(rect.X + size * .77, rect.Y + size * .3));
                }
                dc.DrawGeometry(null, check, g);
            }
        }
    }
}
