using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Rendering;

namespace AntiQuickPad.Editor;

internal sealed class MarkdownColorizer(Look look, MarkdownScanner scanner) : DocumentColorizingTransformer
{
    private readonly IBrush _dim = new SolidColorBrush(look.MarkdownMarker), _keyword = new SolidColorBrush(look.Keyword),
        _string = new SolidColorBrush(look.String), _comment = new SolidColorBrush(look.Comment), _accent = new SolidColorBrush(look.Accent);
    private readonly IBrush[] _heading = look.HeadingColors.Select(c => (IBrush)new SolidColorBrush(c)).ToArray();
    private readonly Typeface _codeTypeface = new(new FontFamily(look.CodeFont));
    private static readonly Regex Strings = new("""(?<q>['"])(?:\\.|(?!\k<q>).)*?\k<q>""", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    private static readonly Regex Keywords = new(@"\b(def|class|return|if|elif|else|for|while|in|import|from|as|try|except|with|yield|async|await|True|False|None|pass|break|continue|lambda|and|or|not|is|raise|finally|global|nonlocal|del|assert)\b", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));

    protected override void ColorizeLine(DocumentLine line)
    {
        var info = scanner.GetLine(line.LineNumber); var text = info.Text; var offset = line.Offset;
        if (info.IsCode)
        {
            ChangeLinePart(offset, line.EndOffset, e => e.TextRunProperties.SetTypeface(_codeTypeface));
            if (info.IsFence) { Paint(offset, line.EndOffset, _dim); return; }
            if (!info.IsPython) return;
            if (info.PythonMarker) { Paint(offset, line.EndOffset, _dim); return; }
            var codeEnd = info.CommentOffset < 0 ? text.Length : info.CommentOffset;
            foreach (Match m in Keywords.Matches(text)) if (m.Index < codeEnd) Paint(offset + m.Index, offset + Math.Min(codeEnd, m.Index + m.Length), _keyword);
            foreach (Match m in Strings.Matches(text)) if (m.Index < codeEnd) Paint(offset + m.Index, offset + Math.Min(codeEnd, m.Index + m.Length), _string);
            if (info.CommentOffset >= 0) Paint(offset + info.CommentOffset, line.EndOffset, _comment);
            return;
        }
        if (info.HeadingLevel > 0)
        {
            var idx = Math.Clamp(info.HeadingLevel - 1, 0, _heading.Length - 1);
            Paint(offset, line.EndOffset, _heading[idx]);
            Paint(offset, offset + info.HeadingMarkerLength, _dim);
            ChangeLinePart(offset, line.EndOffset, e =>
            {
                var old = e.TextRunProperties.Typeface;
                e.TextRunProperties.SetTypeface(new Typeface(old.FontFamily, FontStyle.Normal, info.HeadingLevel <= 2 ? FontWeight.Bold : FontWeight.SemiBold, old.Stretch));
                e.TextRunProperties.SetFontRenderingEmSize(look.HeadingSizes[Math.Clamp(info.HeadingLevel - 1, 0, look.HeadingSizes.Length - 1)]);
            });
            return;
        }
        foreach (var span in info.Bold) { Markers(offset + span.Start, span.Length, span.MarkerLength); Format(offset + span.Start + span.MarkerLength, offset + span.Start + span.Length - span.MarkerLength, FontWeight.Bold, FontStyle.Normal, null); }
        foreach (var span in info.Italic) { Markers(offset + span.Start, span.Length, span.MarkerLength); Format(offset + span.Start + span.MarkerLength, offset + span.Start + span.Length - span.MarkerLength, FontWeight.Normal, FontStyle.Italic, null); }
        foreach (var span in info.Strike) Markers(offset + span.Start, span.Length, span.MarkerLength);
        if (info.BulletLength > 0) Paint(offset, offset + info.BulletLength, _accent);
        if (info.CheckboxOffset >= 0) Paint(offset + info.CheckboxOffset, offset + info.CheckboxOffset + 3, Brushes.Transparent);
    }

    private void Markers(int start, int length, int count) { Paint(start, start + count, _dim); Paint(start + length - count, start + length, _dim); }
    private void Paint(int start, int end, IBrush brush) { if (end > start) ChangeLinePart(start, end, e => e.TextRunProperties.SetForegroundBrush(brush)); }
    private void Format(int start, int end, FontWeight weight, FontStyle style, TextDecorationCollection? decorations)
    {
        if (end <= start) return;
        ChangeLinePart(start, end, e => { var old = e.TextRunProperties.Typeface; e.TextRunProperties.SetTypeface(new Typeface(old.FontFamily, style, weight, old.Stretch)); if (decorations is not null) e.TextRunProperties.SetTextDecorations(decorations); });
    }
}

internal sealed class CodePanelRenderer(Look look, MarkdownScanner scanner) : IBackgroundRenderer
{
    private readonly IBrush _fill = new SolidColorBrush(look.CodePanel);
    public KnownLayer Layer => KnownLayer.Background;
    public void Draw(TextView textView, DrawingContext dc)
    {
        if (!textView.VisualLinesValid || textView.Document is null) return;
        foreach (var group in scanner.Lines.Select((line, i) => (line, i)).Where(x => x.line.IsCode).GroupAdjacent())
        {
            double top = double.NaN, bottom = 0;
            foreach (var visual in textView.VisualLines)
            {
                var n = visual.FirstDocumentLine.LineNumber - 1;
                if (n < group.Start || n > group.End) continue;
                var y = visual.VisualTop - textView.VerticalOffset;
                if (double.IsNaN(top)) top = y;
                bottom = y + visual.Height;
            }
            if (!double.IsNaN(top)) dc.DrawRectangle(_fill, null, new Rect(look.CodePanelInset, top, Math.Max(0, textView.Bounds.Width - 2 * look.CodePanelInset), bottom - top), look.CodeRadius, look.CodeRadius);
        }
    }
}

internal static class AdjacentGrouping
{
    public static IEnumerable<(int Start, int End)> GroupAdjacent(this IEnumerable<(MarkdownScanner.LineInfo line, int i)> source)
    {
        var start = -1; var end = -1;
        foreach (var item in source)
        {
            if (start < 0) start = end = item.i;
            else if (item.i == end + 1) end = item.i;
            else { yield return (start, end); start = end = item.i; }
        }
        if (start >= 0) yield return (start, end);
    }
}

