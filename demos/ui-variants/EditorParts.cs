using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;

namespace UiVariants;

internal static class Fence
{
    // line index (1-based) -> true when the line is a fence line or inside a fence
    public static (int Start, int End)[] Ranges(TextDocument doc)
    {
        var res = new List<(int, int)>(); var start = -1;
        foreach (var l in doc.Lines)
            if (doc.GetText(l).TrimStart().StartsWith("```")) { if (start < 0) start = l.LineNumber; else { res.Add((start, l.LineNumber)); start = -1; } }
        return res.ToArray();
    }
}

// Palette-driven markdown + python colorizer (DocumentColorizingTransformer, same mechanism as spike A).
internal sealed class MarkdownColorizer(Look look) : DocumentColorizingTransformer
{
    private readonly IBrush[] _h = look.Headings.Select(c => (IBrush)new SolidColorBrush(Color.Parse(c))).ToArray();
    private readonly IBrush _dim = new SolidColorBrush(Color.Parse(look.Dim)), _kw = new SolidColorBrush(Color.Parse(look.Keyword)),
        _str = new SolidColorBrush(Color.Parse(look.Str)), _com = new SolidColorBrush(Color.Parse(look.Comment)), _acc = new SolidColorBrush(Color.Parse(look.Accent));
    private readonly Typeface _code = new(new FontFamily(look.CodeFont));
    private static readonly Regex Kw = new(@"\b(def|class|return|if|elif|else|for|while|in|import|from|as|try|except|with|yield|async|await|True|False|None|pass|break|continue|lambda)\b", RegexOptions.Compiled);
    private static readonly Regex Str = new("(?<q>['\\\"])(?:\\\\.|(?!\\k<q>).)*?\\k<q>", RegexOptions.Compiled);
    private static readonly Regex Bold = new(@"\*\*.+?\*\*", RegexOptions.Compiled);
    private static readonly Regex Italic = new(@"(?<!\*)\*(?!\*).+?(?<!\*)\*(?!\*)", RegexOptions.Compiled);
    private static readonly Regex Strike = new(@"~~.+?~~", RegexOptions.Compiled);

    protected override void ColorizeLine(DocumentLine line)
    {
        var doc = CurrentContext.Document; var text = doc.GetText(line); var o = line.Offset;
        var ranges = Fence.Ranges(doc); var n = line.LineNumber;
        var fence = ranges.FirstOrDefault(r => n >= r.Start && n <= r.End);
        if (fence.Start != 0)
        {
            ChangeLinePart(o, line.EndOffset, e => e.TextRunProperties.SetTypeface(_code));
            if (n == fence.Start || n == fence.End) { Paint(o, line.EndOffset, _dim); return; }
            var cm = text.IndexOf('#');
            var codeEnd = cm >= 0 ? cm : text.Length;
            foreach (Match m in Str.Matches(text)) if (m.Index < codeEnd) Paint(o + m.Index, o + m.Index + m.Length, _str);
            foreach (Match m in Kw.Matches(text)) if (m.Index < codeEnd) Paint(o + m.Index, o + m.Index + m.Length, _kw);
            if (cm >= 0) Paint(o + cm, line.EndOffset, _com);
            return;
        }
        var pre = text.TakeWhile(c => c == ' ').Count();
        var hashes = text.Skip(pre).TakeWhile(c => c == '#').Count();
        if (hashes is > 0 and <= 6 && text.Skip(pre + hashes).FirstOrDefault() is ' ')
        {
            Paint(o, line.EndOffset, _h[hashes - 1]);
            Paint(o + pre, o + pre + hashes, _dim);                       // dim the # markers
            ChangeLinePart(o, line.EndOffset, e => e.TextRunProperties.SetTypeface(new Typeface(e.TextRunProperties.Typeface.FontFamily, FontStyle.Normal, hashes == 1 ? FontWeight.Bold : FontWeight.SemiBold)));
            return;
        }
        foreach (Match m in Bold.Matches(text)) { Marks(o + m.Index, m.Length, 2); Fmt(o + m.Index + 2, o + m.Index + m.Length - 2, FontWeight.Bold, FontStyle.Normal, null); }
        foreach (Match m in Italic.Matches(text)) { Marks(o + m.Index, m.Length, 1); Fmt(o + m.Index + 1, o + m.Index + m.Length - 1, FontWeight.Normal, FontStyle.Italic, null); }
        foreach (Match m in Strike.Matches(text)) { Marks(o + m.Index, m.Length, 2); Fmt(o + m.Index + 2, o + m.Index + m.Length - 2, FontWeight.Normal, FontStyle.Normal, TextDecorations.Strikethrough); }
        foreach (Match m in Regex.Matches(text, @"\[[ xX]\]")) Paint(o + m.Index, o + m.Index + m.Length, _acc);
        var lm = Regex.Match(text, @"^\s*[-*] ");
        if (lm.Success) Paint(o, o + lm.Length, _acc);
    }

    private void Marks(int start, int len, int mark) { Paint(start, start + mark, _dim); Paint(start + len - mark, start + len, _dim); }
    private void Paint(int s, int e, IBrush b) { if (e > s) ChangeLinePart(s, e, el => el.TextRunProperties.SetForegroundBrush(b)); }
    private void Fmt(int s, int e, FontWeight w, FontStyle st, TextDecorationCollection? d)
    {
        if (e <= s) return;
        ChangeLinePart(s, e, el =>
        {
            var old = el.TextRunProperties.Typeface;
            el.TextRunProperties.SetTypeface(new Typeface(old.FontFamily, st, w, old.Stretch));
            if (d is not null) el.TextRunProperties.SetTextDecorations(d);
        });
    }
}

// Full-width rounded panel behind fenced code, drawn on the Background layer.
internal sealed class CodePanelRenderer(Look look) : IBackgroundRenderer
{
    private readonly IBrush _fill = new SolidColorBrush(Color.Parse(look.CodePanel));
    private readonly IPen _edge = new Pen(new SolidColorBrush(Color.Parse(look.Border)), 1);
    public KnownLayer Layer => KnownLayer.Background;
    public void Draw(TextView textView, DrawingContext dc)
    {
        if (!textView.VisualLinesValid || textView.Document is null) return;
        var ranges = Fence.Ranges(textView.Document);
        foreach (var r in ranges)
        {
            double top = double.NaN, bottom = 0;
            foreach (var vl in textView.VisualLines)
            {
                var n = vl.FirstDocumentLine.LineNumber;
                if (n < r.Start || n > r.End) continue;
                var y = vl.VisualTop - textView.VerticalOffset;
                if (double.IsNaN(top)) top = y;
                bottom = y + vl.Height;
            }
            if (double.IsNaN(top)) continue;
            var rect = new Rect(-textView.HorizontalOffset - 4, top, Math.Max(textView.Bounds.Width, textView.DocumentHeight) + 4, bottom - top);
            rect = new Rect(0, top, textView.Bounds.Width, bottom - top).Deflate(new Thickness(2, 0));
            dc.DrawRectangle(_fill, null, rect, 8, 8);
        }
    }
}

// Replaces "[ ]" / "[x]" in list items with a real rounded-square control (InlineObjectElement).
internal sealed class CheckboxGenerator(Look look) : VisualLineElementGenerator
{
    private static readonly Regex Item = new(@"^\s*[-*] (\[[ xX]\])", RegexOptions.Compiled);
    public override int GetFirstInterestedOffset(int startOffset)
    {
        var line = CurrentContext.Document.GetLineByOffset(startOffset);
        var m = Item.Match(CurrentContext.Document.GetText(line));
        if (!m.Success) return -1;
        var off = line.Offset + m.Groups[1].Index;
        return off >= startOffset ? off : -1;
    }

    public override VisualLineElement? ConstructElement(int offset)
    {
        var ch = CurrentContext.Document.GetCharAt(offset + 1);
        var on = ch is 'x' or 'X';
        var accent = new SolidColorBrush(Color.Parse(look.Accent));
        var box = new Border
        {
            Width = 17, Height = 17, CornerRadius = new CornerRadius(5), BorderThickness = new Thickness(1.6),
            BorderBrush = on ? accent : new SolidColorBrush(Color.Parse(look.Dim)), Background = on ? accent : Brushes.Transparent,
            Margin = new Thickness(1, 0, 3, 1), VerticalAlignment = VerticalAlignment.Bottom,
            Child = on ? new Avalonia.Controls.Shapes.Path { Data = Geometry.Parse("M3.5,8.5 L7,12 L13,4.5"), Stroke = Brushes.White, StrokeThickness = 2, StrokeLineCap = PenLineCap.Round, StrokeJoin = PenLineJoin.Round } : null
        };
        return new InlineObjectElement(3, box);
    }
}

// Custom line-number gutter: small, dimmed, right aligned, optional hairline separator and current-line emphasis.
internal sealed class GutterMargin : AbstractMargin
{
    private readonly Look _look; private readonly IBrush _dim, _cur, _rowBg; private readonly IPen _sep;
    private readonly Typeface _tf;
    public GutterMargin(Look look)
    {
        _look = look; _tf = new Typeface(new FontFamily(look.EditorFont.Split(',')[0].Contains("Segoe") ? look.CodeFont : look.EditorFont));
        _dim = new SolidColorBrush(Color.Parse(look.LnBrush)); _cur = new SolidColorBrush(Color.Parse(look.LnCurrent));
        _rowBg = new SolidColorBrush(Color.Parse(look.CurrentBg));
        _sep = new Pen(new SolidColorBrush(Color.Parse(look.Border)), 1);
    }
    protected override Size MeasureOverride(Size s) => new(Math.Ceiling(_look.LnSize * 0.62 * 3) + 22, 0);
    protected override void OnTextViewChanged(TextView? oldTextView, TextView? newTextView)
    {
        if (oldTextView != null) oldTextView.VisualLinesChanged -= Inv;
        base.OnTextViewChanged(oldTextView, newTextView);
        if (newTextView != null) newTextView.VisualLinesChanged += Inv;
    }
    private void Inv(object? s, EventArgs e) => InvalidateVisual();
    public override void Render(DrawingContext dc)
    {
        var tv = TextView; if (tv is null || !tv.VisualLinesValid) return;
        var caretLine = TextArea?.Caret.Line ?? -1;
        foreach (var vl in tv.VisualLines)
        {
            var n = vl.FirstDocumentLine.LineNumber; var cur = n == caretLine;
            var y = vl.VisualTop - tv.VerticalOffset;
            var first = vl.TextLines[0];
            if (cur && _look.HighlightCurrent) dc.DrawRectangle(_rowBg, null, new Rect(0, y, Bounds.Width, vl.Height));
            var ft = new FormattedText(n.ToString(), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, _tf, _look.LnSize, cur ? _cur : _dim);
            dc.DrawText(ft, new Point(Bounds.Width - 10 - ft.Width, y + first.Baseline - ft.Baseline));
        }
        if (_look.LnSeparator) dc.DrawLine(_sep, new Point(Bounds.Width - .5, 0), new Point(Bounds.Width - .5, Bounds.Height));
    }
}
