using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using AvaloniaEdit.Rendering;
using AntiNotepad.Theme;

namespace AntiNotepad.Editor;

/// <summary>A run of replacement glyphs that stands for a span of document text (one caret stop).</summary>
internal class GlyphElement : FormattedTextElement
{
    public GlyphElement(TextLine line, int documentLength) : base(line, documentLength) { }
    public Action<PointerPressedEventArgs>? Pressed { get; init; }
    public bool Hand { get; init; }
    public bool IsLink { get; init; }

    protected override void OnQueryCursor(PointerEventArgs e)
    {
        if (!Hand || e.Source is not InputElement element) return;
        element.Cursor = new Cursor(StandardCursorType.Hand);
        e.Handled = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.Handled || Pressed is null || !e.GetCurrentPoint(null).Properties.IsLeftButtonPressed) return;
        Pressed(e);
    }

    public static TextLine Prepare(ITextRunConstructionContext context, string text, IBrush brush, double? size = null, FontWeight weight = Typography.Regular, TextDecorationCollection? decorations = null)
    {
        var props = context.GlobalTextRunProperties;
        var run = new GenericTextRunProperties(new Typeface(props.Typeface.FontFamily, FontStyle.Normal, weight), size ?? props.FontRenderingEmSize,
            decorations, brush);
        return PrepareText(TextFormatter.Current, text, run);
    }
}

/// <summary>Draws `[ ]` / `[x]` as a checkbox glyph; click toggles. `- [ ]` bullets fold into the box.</summary>
internal sealed class CheckboxGenerator(PadView pad) : VisualLineElementGenerator
{
    public override int GetFirstInterestedOffset(int startOffset)
    {
        var doc = CurrentContext.Document;
        var line = doc.GetLineByOffset(startOffset);
        if (startOffset != line.Offset && startOffset > line.Offset + IndentOf(doc, line)) return -1;
        var m = ListEditing.Checkbox.Match(doc.GetText(line));
        if (!m.Success) return -1;
        var at = line.Offset + m.Groups[1].Length;
        return at >= startOffset ? at : -1;
    }

    public override VisualLineElement? ConstructElement(int offset)
    {
        if (!pad.Decorate) return null;
        var doc = CurrentContext.Document;
        var line = doc.GetLineByOffset(offset);
        var m = ListEditing.Checkbox.Match(doc.GetText(line));
        if (!m.Success || line.Offset + m.Groups[1].Length != offset) return null;
        var length = m.Length - m.Groups[1].Length;
        var lineNumber = line.LineNumber;
        // Invisible text of the box's width keeps the line exactly as tall as its neighbours;
        // CheckboxRenderer paints the box itself on top.
        var blank = GlyphElement.Prepare(CurrentContext, "  ", Brushes.Transparent);
        return new GlyphElement(blank, length)
        {
            Hand = true,
            Pressed = e => { e.Handled = true; pad.ToggleCheckbox(lineNumber); }
        };
    }

    private static int IndentOf(TextDocument doc, DocumentLine line)
    {
        var text = doc.GetText(line);
        return text.Length - text.TrimStart().Length;
    }
}

/// <summary>`- item` and `* item` render with a round bullet; the dash stays in the text.</summary>
internal sealed class BulletGenerator(PadView pad) : VisualLineElementGenerator
{
    private static readonly Regex Bullet = new(@"^(\s*)[-*+](?=\s)(?!\s+\[[ xX]\])", RegexOptions.Compiled);

    public override int GetFirstInterestedOffset(int startOffset)
    {
        var doc = CurrentContext.Document;
        var line = doc.GetLineByOffset(startOffset);
        var m = Bullet.Match(doc.GetText(line));
        if (!m.Success) return -1;
        var at = line.Offset + m.Groups[1].Length;
        return at >= startOffset ? at : -1;
    }

    public override VisualLineElement? ConstructElement(int offset)
    {
        if (!pad.Decorate) return null;
        var text = GlyphElement.Prepare(CurrentContext, "•", new SolidColorBrush(pad.Palette.Muted));
        return new GlyphElement(text, 1);
    }
}

/// <summary>Long URLs display as `host/…/tail` (AntiNote's link shortening). The text and clipboard keep the full URL.</summary>
internal sealed class LinkGenerator(PadView pad) : VisualLineElementGenerator
{
    internal static readonly Regex Url = new(@"https?://[^\s<>()\[\]""']+[^\s<>()\[\]""'.,;:!?]", RegexOptions.Compiled);

    public override int GetFirstInterestedOffset(int startOffset)
    {
        var doc = CurrentContext.Document;
        var line = doc.GetLineByOffset(startOffset);
        var text = doc.GetText(startOffset, line.EndOffset - startOffset);
        var m = Url.Match(text);
        while (m.Success && !Wants(m)) m = m.NextMatch();
        return m.Success ? startOffset + m.Index : -1;
    }

    private bool Wants(Match m) => m.Length > 34 || !pad.Settings.ShortenLinks;

    public override VisualLineElement? ConstructElement(int offset)
    {
        var doc = CurrentContext.Document;
        var line = doc.GetLineByOffset(offset);
        var m = Url.Match(doc.GetText(offset, line.EndOffset - offset));
        if (!m.Success || m.Index != 0) return null;
        var url = m.Value;
        // Show the full URL while the caret is on its line, so it can be edited in place.
        var shorten = pad.Settings.ShortenLinks && url.Length > 34 && pad.CaretLine != line.LineNumber;
        var display = shorten ? Shorten(url) : url;
        if (!shorten && !pad.Settings.ShortenLinks) return null;
        if (!shorten) return null;
        var text = GlyphElement.Prepare(CurrentContext, display, new SolidColorBrush(pad.Palette.Link), decorations: TextDecorations.Underline);
        return new GlyphElement(text, url.Length)
        {
            Hand = true,
            IsLink = true,
            Pressed = e =>
            {
                if (e.KeyModifiers.HasFlag(KeyModifiers.Control)) { e.Handled = true; Platform.DesktopPlatform.OpenUrl(url); }
            }
        };
    }

    internal static string Shorten(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return url[..30] + "…";
        var host = uri.Host.StartsWith("www.") ? uri.Host[4..] : uri.Host;
        var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var tail = segments.Length == 0 ? "" : segments[^1];
        if (tail.Length > 18) tail = tail[..17] + "…";
        return segments.Length switch
        {
            0 => host + (uri.Query.Length > 0 ? "/?…" : ""),
            1 => $"{host}/{tail}",
            _ => $"{host}/…/{tail}"
        };
    }
}

/// <summary>
/// Clean markdown: `**`, `~~`, `++`, `*`, backticks and heading hashes vanish except on the line being edited,
/// so formatted notes read like a document but stay plain text underneath.
/// </summary>
internal sealed class MarkerGenerator(PadView pad) : VisualLineElementGenerator
{
    private int _line = -1;
    private List<(int Start, int Length)> _spans = [];

    private List<(int Start, int Length)> Spans(DocumentLine line)
    {
        // Cached per line for one render pass; the caret line and document version are part of the key.
        if (_line == line.LineNumber && _caret == pad.CaretLine && _cachedVersion == CurrentContext.Document.Version) return _spans;
        _line = line.LineNumber; _caret = pad.CaretLine; _cachedVersion = CurrentContext.Document.Version;
        _spans = [];
        if (!pad.Settings.CleanMarkdown || !pad.Decorate || pad.Mode == Model.NoteMode.Math || pad.InFence(line.LineNumber)) return _spans;
        if (line.LineNumber == pad.CaretLine) return _spans;
        var text = CurrentContext.Document.GetText(line);
        var trimmed = text.TrimStart();
        var heading = NoteColorizer.HeadingLevel(trimmed);
        if (heading > 0 && !(line.LineNumber == 1 && pad.ModeKeywordLength > 0)) _spans.Add((text.Length - trimmed.Length, heading + 1));
        if (NoteColorizer.Divider.IsMatch(text)) { _spans.Add((text.Length - trimmed.Length, trimmed.TrimEnd().Length)); return _spans; }
        var urls = LinkGenerator.Url.Matches(text).Select(m => (m.Index, End: m.Index + m.Length)).ToList();
        var code = NoteColorizer.InlineCode.Matches(text).Select(m => (m.Index, End: m.Index + m.Length)).ToList();
        bool Free(int at) => !urls.Any(u => at >= u.Index && at < u.End);
        bool OutsideCode(int at) => !code.Any(c => at > c.Index && at < c.End - 1);
        foreach (var (i, end) in code) { _spans.Add((i, 1)); _spans.Add((end - 1, 1)); }
        void Pair(Regex regex, int width)
        {
            foreach (Match m in regex.Matches(text))
            {
                if (!Free(m.Index) || !OutsideCode(m.Index)) continue;
                _spans.Add((m.Index, width)); _spans.Add((m.Index + m.Length - width, width));
            }
        }
        Pair(NoteColorizer.Bold, 2); Pair(NoteColorizer.Strike, 2); Pair(NoteColorizer.Underline, 2); Pair(NoteColorizer.Italic, 1);
        _spans.Sort();
        return _spans;
    }

    private ITextSourceVersion? _cachedVersion;
    private int _caret = -1;

    public override int GetFirstInterestedOffset(int startOffset)
    {
        var line = CurrentContext.Document.GetLineByOffset(startOffset);
        foreach (var (start, _) in Spans(line))
            if (line.Offset + start >= startOffset) return line.Offset + start;
        return -1;
    }

    public override VisualLineElement? ConstructElement(int offset)
    {
        var line = CurrentContext.Document.GetLineByOffset(offset);
        var span = Spans(line).FirstOrDefault(s => line.Offset + s.Start == offset);
        if (span.Length == 0) return null;
        var empty = GlyphElement.Prepare(CurrentContext, "​", Brushes.Transparent);
        return new GlyphElement(empty, span.Length);
    }
}
