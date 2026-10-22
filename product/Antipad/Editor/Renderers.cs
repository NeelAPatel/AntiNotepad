using System.Globalization;
using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Rendering;
using Antipad.Model;

namespace Antipad.Editor;

/// <summary>Paper texture drawn in text space, so lines and grid stay locked to text while scrolling and resizing.</summary>
internal sealed class PaperRenderer(PadView pad) : IBackgroundRenderer
{
    public KnownLayer Layer => KnownLayer.Background;

    public void Draw(TextView view, DrawingContext dc)
    {
        var kind = pad.Settings.Paper;
        if (kind == "blank") return;
        var alpha = pad.Settings.PaperStrength switch { 0 => 0.55, 2 => 1.9, _ => 1.0 };
        var color = pad.Palette.Paper;
        var brush = new SolidColorBrush(color, Math.Min(1, alpha));
        var pen = new Pen(brush, 1);
        var h = view.DefaultLineHeight;
        if (h <= 0) return;
        var width = view.Bounds.Width; var height = view.Bounds.Height;
        // Baseline-ish rule under each line, anchored to the document's first line top.
        var top = -(view.VerticalOffset % h);
        switch (kind)
        {
            case "lined":
                for (var y = top + h - 0.5; y < height; y += h) dc.DrawLine(pen, new Point(0, Math.Round(y) + .5), new Point(width, Math.Round(y) + .5));
                break;
            case "dotted":
                {
                    var dot = new SolidColorBrush(color, Math.Min(1, alpha * 2.2));
                    for (var y = top + h; y < height; y += h)
                        for (var x = h / 2; x < width; x += h) dc.DrawEllipse(dot, null, new Point(x, y), 1.1, 1.1);
                    break;
                }
            case "grid":
            case "biggrid":
                {
                    var step = kind == "grid" ? h : h * 2;
                    var gtop = -(view.VerticalOffset % step);
                    for (var y = gtop + step - 0.5; y < height; y += step) dc.DrawLine(pen, new Point(0, Math.Round(y) + .5), new Point(width, Math.Round(y) + .5));
                    for (var x = step / 2; x < width; x += step) dc.DrawLine(pen, new Point(Math.Round(x) + .5, 0), new Point(Math.Round(x) + .5, height));
                    break;
                }
        }
    }
}

/// <summary>Right-aligned result chips: math answers, inline `expr =` answers, timer countdowns. Chips are clickable.</summary>
internal sealed class ResultRenderer(PadView pad) : IBackgroundRenderer
{
    public readonly record struct Chip(Rect Bounds, int Line, string Kind, string Value);
    public List<Chip> Chips { get; } = [];
    public KnownLayer Layer => KnownLayer.Selection;

    public void Draw(TextView view, DrawingContext dc)
    {
        Chips.Clear();
        if (!view.VisualLinesValid || view.Document is null) return;
        var p = pad.Palette;
        var typeface = new Typeface(new FontFamily(pad.Settings.Font + ", Cascadia Mono, Consolas"), FontStyle.Normal, FontWeight.SemiBold);
        var size = pad.Settings.FontSize * 0.93;
        foreach (var visual in view.VisualLines)
        {
            var docLine = visual.LastDocumentLine;
            var n = docLine.LineNumber;
            var chip = pad.ChipFor(n);
            if (chip is null) continue;
            var (text, kind, color, value) = chip.Value;
            var ft = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, size, new SolidColorBrush(color));
            var lastRow = visual.TextLines[^1];
            var rowTop = visual.VisualTop + visual.Height - lastRow.Height - view.VerticalOffset;
            var textEnd = lastRow.WidthIncludingTrailingWhitespace;
            var x = view.Bounds.Width - ft.Width - 6;
            // Never draw over the text: when the line is long, sit just after it.
            if (x < textEnd + 18) x = textEnd + 18;
            var y = rowTop + (lastRow.Height - ft.Height) / 2;
            var rect = new Rect(x - 7, y - 1, ft.Width + 14, ft.Height + 2);
            dc.DrawRectangle(new SolidColorBrush(color, pad.Palette.Dark ? 0.14 : 0.10), null, rect, 7, 7);
            dc.DrawText(ft, new Point(x, y));
            Chips.Add(new Chip(rect, n, kind, value));
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
