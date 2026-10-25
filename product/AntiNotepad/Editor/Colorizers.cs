using System.Text.RegularExpressions;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using AntiNotepad.Calc;
using AntiNotepad.Model;

namespace AntiNotepad.Editor;

/// <summary>Per-colorizer immutable span brushes, capped so palette changes cannot grow a view's cache indefinitely.</summary>
internal sealed class ColorizerBrushCache
{
    private const int Capacity = 24;
    private readonly Dictionary<Color, ImmutableSolidColorBrush> _brushes = [];

    internal ImmutableSolidColorBrush Get(Color color)
    {
        if (_brushes.TryGetValue(color, out var brush)) return brush;
        if (_brushes.Count >= Capacity) _brushes.Clear();
        brush = new ImmutableSolidColorBrush(color);
        _brushes.Add(color, brush);
        return brush;
    }
}

/// <summary>Plain-note styling: headings, emphasis, inline code, fences, comments, quotes, done tasks, mode keyword.</summary>
internal sealed class NoteColorizer(PadView pad) : DocumentColorizingTransformer
{
    private readonly ColorizerBrushCache _brushes = new();
    internal static readonly Regex Bold = new(@"\*\*(?=\S)(.+?)(?<=\S)\*\*", RegexOptions.Compiled);
    internal static readonly Regex Italic = new(@"(?<![*\w])\*(?=\S)([^*]+?)(?<=\S)\*(?![*\w])|(?<![_\w])_(?=\S)([^_]+?)(?<=\S)_(?![_\w])", RegexOptions.Compiled);
    internal static readonly Regex Underline = new(@"(?<![+_])(\+\+|__)(?=\S)(.+?)(?<=\S)\1", RegexOptions.Compiled);
    /// <summary>A line of three or more -, * or _ and nothing else: drawn as a rule across the note.</summary>
    internal static readonly Regex Divider = new(@"^\s*(?:-{3,}|\*{3,}|_{3,})\s*$", RegexOptions.Compiled);
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
            // Fence markers recede (muted, still readable on the darker panel); the body is ink, and FenceColorizer adds
            // syntax colours when the language is known.
            Mono(o, line.EndOffset, text.TrimStart().StartsWith("```") ? p.Muted : p.Ink);
            return;
        }
        var trimmed = text.TrimStart();
        if (trimmed.StartsWith("//")) { Paint(o, line.EndOffset, p.Comment, style: FontStyle.Italic); return; }
        if (trimmed.StartsWith('>')) Paint(o, line.EndOffset, p.Muted, style: FontStyle.Italic);
        if (Divider.IsMatch(text)) { Paint(o, line.EndOffset, p.Faint); return; }
        var heading = HeadingLevel(trimmed);
        if (heading > 0)
        {
            var lead = text.Length - trimmed.Length;
            // Hierarchy by colour (a rainbow, H1 red to H6 violet) and by size for the top three.
            var (size, weight) = heading switch
            {
                1 => (1.32, FontWeight.Bold),
                2 => (1.16, FontWeight.Bold),
                3 => (1.06, FontWeight.SemiBold),
                _ => (1.0, FontWeight.SemiBold),
            };
            Paint(o, line.EndOffset, HeadingColor(p, heading), weight);
            Paint(o + lead, o + lead + heading, p.Faint);
            if (size > 1) ChangeLinePart(o, line.EndOffset, e => e.TextRunProperties.SetFontRenderingEmSize(e.TextRunProperties.FontRenderingEmSize * size));
            return;
        }
        var box = CheckboxGenerator.Box.Match(text);
        if (box.Success && box.Groups[2].Value != " ")
        {
            ChangeLinePart(o + box.Length, line.EndOffset, e =>
            {
                e.TextRunProperties.SetForegroundBrush(_brushes.Get(p.Done));
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
            e.TextRunProperties.SetForegroundBrush(_brushes.Get(p.Link));
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

    // Rainbow hues per level, tuned to read on light paper and on dark glass.
    private static readonly Color[] RainbowLight = [C(0xC6, 0x3A, 0x33), C(0xC4, 0x66, 0x14), C(0x9C, 0x7C, 0x0C), C(0x2F, 0x8A, 0x4A), C(0x2A, 0x6C, 0xB8), C(0x80, 0x4C, 0xB8)];
    private static readonly Color[] RainbowDark = [C(0xF0, 0x7A, 0x70), C(0xF0, 0xA0, 0x5A), C(0xE6, 0xCC, 0x60), C(0x7C, 0xCF, 0x8C), C(0x76, 0xB0, 0xF0), C(0xC2, 0x96, 0xF0)];
    private static Color C(byte r, byte g, byte b) => Color.FromRgb(r, g, b);

    internal static Color HeadingColor(Theme.Palette p, int level) => (p.Dark ? RainbowDark : RainbowLight)[Math.Clamp(level, 1, 6) - 1];

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
            e.TextRunProperties.SetForegroundBrush(_brushes.Get(color));
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
        // Adds to what is there, so __~~both~~__ is underlined and struck through.
        ChangeLinePart(start, end, e =>
        {
            var merged = new TextDecorationCollection();
            if (e.TextRunProperties.TextDecorations is { } existing) foreach (var d in existing) merged.Add(d);
            foreach (var d in decorations) merged.Add(d);
            e.TextRunProperties.SetTextDecorations(merged);
        });
    }

    private void Mono(int start, int end, Color color)
    {
        if (end <= start) return;
        ChangeLinePart(start, end, e => e.TextRunProperties.SetForegroundBrush(_brushes.Get(color)));
    }
}

/// <summary>Math-note colouring from the calculator's own tokens, so colours always agree with evaluation.</summary>
internal sealed class MathColorizer(PadView pad) : DocumentColorizingTransformer
{
    private readonly ColorizerBrushCache _brushes = new();

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
                e.TextRunProperties.SetForegroundBrush(_brushes.Get(color));
                if (italic) { var t = e.TextRunProperties.Typeface; e.TextRunProperties.SetTypeface(new Typeface(t.FontFamily, FontStyle.Italic, t.Weight, t.Stretch)); }
            });
        }
    }
}
