using System.Text.RegularExpressions;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Antipad.Calc;
using Antipad.Model;

namespace Antipad.Editor;

/// <summary>Plain-note styling: headings, emphasis, inline code, fences, comments, quotes, done tasks, mode keyword.</summary>
internal sealed class NoteColorizer(PadView pad) : DocumentColorizingTransformer
{
    internal static readonly Regex Bold = new(@"(\*\*|__)(?=\S)(.+?)(?<=\S)\1", RegexOptions.Compiled);
    internal static readonly Regex Italic = new(@"(?<![*\w])\*(?=\S)([^*]+?)(?<=\S)\*(?![*\w])|(?<![_\w])_(?=\S)([^_]+?)(?<=\S)_(?![_\w])", RegexOptions.Compiled);
    internal static readonly Regex Underline = new(@"(?<!\+)\+\+(?=\S)(.+?)(?<=\S)\+\+", RegexOptions.Compiled);
    internal static readonly Regex Strike = new(@"~~(?=\S)(.+?)(?<=\S)~~", RegexOptions.Compiled);
    internal static readonly Regex InlineCode = new(@"`[^`\n]+`", RegexOptions.Compiled);
    private static readonly Regex Tag = new(@"(?<=^|\s)#[A-Za-z][\w-]*", RegexOptions.Compiled);
    private static readonly Regex Number = new(@"(?<![\w.])\d+(?:[.,:]\d+)*(?![\w])", RegexOptions.Compiled);

    protected override void ColorizeLine(DocumentLine line)
    {
        var p = pad.Palette;
        var text = CurrentContext.Document.GetText(line);
        var o = line.Offset;
        if (line.LineNumber == 1 && pad.ModeKeywordLength > 0)
        {
            Paint(o, o + Math.Min(pad.ModeKeywordLength, line.Length), p.Keyword, FontWeight.SemiBold);
            return;
        }
        if (pad.Mode is NoteMode.Code or NoteMode.Math) return; // their own colourizers own these notes
        if (pad.InFence(line.LineNumber))
        {
            // Fence markers recede; the body is ink, and FenceColorizer adds syntax colours when the language is known.
            Mono(o, line.EndOffset, text.TrimStart().StartsWith("```") ? p.Faint : p.Ink);
            return;
        }
        var trimmed = text.TrimStart();
        if (trimmed.StartsWith("//")) { Paint(o, line.EndOffset, p.Comment, style: FontStyle.Italic); return; }
        if (trimmed.StartsWith('>')) Paint(o, line.EndOffset, p.Muted, style: FontStyle.Italic);
        var heading = HeadingLevel(trimmed);
        if (heading > 0)
        {
            var lead = text.Length - trimmed.Length;
            // Hierarchy by colour and size: H1 accent and largest, H2 keyword colour, H3 identifier colour, deeper levels muted.
            var (color, size, weight) = heading switch
            {
                1 => (p.Accent, 1.32, FontWeight.Bold),
                2 => (p.Keyword, 1.16, FontWeight.Bold),
                3 => (p.Identifier, 1.06, FontWeight.SemiBold),
                _ => (p.Muted, 1.0, FontWeight.SemiBold),
            };
            Paint(o, line.EndOffset, color, weight);
            Paint(o + lead, o + lead + heading, p.Faint);
            if (size > 1) ChangeLinePart(o, line.EndOffset, e => e.TextRunProperties.SetFontRenderingEmSize(e.TextRunProperties.FontRenderingEmSize * size));
            return;
        }
        var box = CheckboxGenerator.Box.Match(text);
        if (box.Success && box.Groups[2].Value != " ")
        {
            ChangeLinePart(o + box.Length, line.EndOffset, e =>
            {
                e.TextRunProperties.SetForegroundBrush(new SolidColorBrush(p.Done));
                e.TextRunProperties.SetTextDecorations(TextDecorations.Strikethrough);
            });
            return;
        }
        if (pad.Mode == NoteMode.Timer) foreach (Match m in Number.Matches(text)) Paint(o + m.Index, o + m.Index + m.Length, p.Number);
        foreach (Match m in Bold.Matches(text)) { Markers(o + m.Index, m.Length, 2); Weight(o + m.Index + 2, o + m.Index + m.Length - 2, FontWeight.Bold); }
        foreach (Match m in Italic.Matches(text)) { Markers(o + m.Index, m.Length, 1); Style(o + m.Index + 1, o + m.Index + m.Length - 1); }
        foreach (Match m in Underline.Matches(text)) { Markers(o + m.Index, m.Length, 2); Decorate(o + m.Index + 2, o + m.Index + m.Length - 2, TextDecorations.Underline); }
        foreach (Match m in Strike.Matches(text)) { Markers(o + m.Index, m.Length, 2); Decorate(o + m.Index + 2, o + m.Index + m.Length - 2, TextDecorations.Strikethrough); }
        foreach (Match m in InlineCode.Matches(text)) Mono(o + m.Index, o + m.Index + m.Length, p.String);
        foreach (Match m in Tag.Matches(text)) Paint(o + m.Index, o + m.Index + m.Length, p.Identifier);
        foreach (Match m in LinkGenerator.Url.Matches(text)) ChangeLinePart(o + m.Index, o + m.Index + m.Length, e =>
        {
            e.TextRunProperties.SetForegroundBrush(new SolidColorBrush(p.Link));
            e.TextRunProperties.SetTextDecorations(TextDecorations.Underline);
        });
        var comment = text.IndexOf(" //", StringComparison.Ordinal);
        if (comment >= 0 && !text.Contains("://")) Paint(o + comment + 1, line.EndOffset, p.Comment, style: FontStyle.Italic);
        if (pad.InlineResult(line.LineNumber) is not null)
        {
            var eq = text.LastIndexOf('=');
            if (eq >= 0) Paint(o + eq, o + eq + 1, p.Result);
        }
    }

    internal static int HeadingLevel(string trimmed)
    {
        var n = 0;
        while (n < trimmed.Length && n < 6 && trimmed[n] == '#') n++;
        return n > 0 && n < trimmed.Length && trimmed[n] == ' ' ? n : 0;
    }

    private void Markers(int start, int length, int count)
    {
        Paint(start, start + count, pad.Palette.Faint);
        Paint(start + length - count, start + length, pad.Palette.Faint);
    }

    private void Paint(int start, int end, Color color, FontWeight? weight = null, FontStyle? style = null)
    {
        if (end <= start) return;
        ChangeLinePart(start, end, e =>
        {
            e.TextRunProperties.SetForegroundBrush(new SolidColorBrush(color));
            if (weight is not null || style is not null)
            {
                var t = e.TextRunProperties.Typeface;
                e.TextRunProperties.SetTypeface(new Typeface(t.FontFamily, style ?? t.Style, weight ?? t.Weight, t.Stretch));
            }
        });
    }

    private void Weight(int start, int end, FontWeight weight)
    {
        if (end <= start) return;
        ChangeLinePart(start, end, e => { var t = e.TextRunProperties.Typeface; e.TextRunProperties.SetTypeface(new Typeface(t.FontFamily, t.Style, weight, t.Stretch)); });
    }

    private void Style(int start, int end)
    {
        if (end <= start) return;
        ChangeLinePart(start, end, e => { var t = e.TextRunProperties.Typeface; e.TextRunProperties.SetTypeface(new Typeface(t.FontFamily, FontStyle.Italic, t.Weight, t.Stretch)); });
    }

    private void Decorate(int start, int end, TextDecorationCollection decorations)
    {
        if (end <= start) return;
        ChangeLinePart(start, end, e => e.TextRunProperties.SetTextDecorations(decorations));
    }

    private void Mono(int start, int end, Color color)
    {
        if (end <= start) return;
        ChangeLinePart(start, end, e => e.TextRunProperties.SetForegroundBrush(new SolidColorBrush(color)));
    }
}

/// <summary>Math-note colouring from the calculator's own tokens, so colours always agree with evaluation.</summary>
internal sealed class MathColorizer(PadView pad) : DocumentColorizingTransformer
{
    protected override void ColorizeLine(DocumentLine line)
    {
        if (pad.Mode != NoteMode.Math || line.LineNumber == 1) return;
        var eval = pad.MathLine(line.LineNumber);
        if (eval is null) return;
        var p = pad.Palette;
        foreach (var token in eval.Tokens)
        {
            var start = line.Offset + token.Start;
            var end = Math.Min(line.EndOffset, start + token.Length);
            if (end <= start) continue;
            var (color, italic) = token.Kind switch
            {
                TokenKind.Number => (p.Number, false),
                TokenKind.Identifier => (p.Identifier, false),
                TokenKind.Unit => (p.Muted, false),
                TokenKind.Keyword => (p.Keyword, false),
                TokenKind.Function => (p.Keyword, false),
                TokenKind.Comment => (p.Comment, true),
                _ => (p.Muted, false)
            };
            ChangeLinePart(start, end, e =>
            {
                e.TextRunProperties.SetForegroundBrush(new SolidColorBrush(color));
                if (italic) { var t = e.TextRunProperties.Typeface; e.TextRunProperties.SetTypeface(new Typeface(t.FontFamily, FontStyle.Italic, t.Weight, t.Stretch)); }
            });
        }
    }
}
