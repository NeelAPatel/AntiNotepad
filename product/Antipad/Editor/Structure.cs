using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Rendering;
using Antipad.Model;
using Antipad.Theme;

namespace Antipad.Editor;

/// <summary>A note's block structure, computed once per edit: fenced code blocks, indent levels and fold ranges.</summary>
internal sealed class NoteStructure
{
    public sealed record Fence(int Open, int Close, string? Language, bool Closed = true);   // 1-based; an unclosed fence runs to the last line

    public List<Fence> Fences { get; } = [];
    public int[] Indent { get; private set; } = [];                       // per line (1-based), in spaces; blank lines inherit
    public bool[] InFence { get; private set; } = [];

    public static NoteStructure Build(string[] lines)
    {
        var s = new NoteStructure { Indent = new int[lines.Length + 2], InFence = new bool[lines.Length + 2] };
        Fence? open = null;
        int openLine = 0; string? lang = null;
        for (var i = 0; i < lines.Length; i++)
        {
            var t = lines[i].TrimStart();
            if (t.StartsWith("```"))
            {
                if (open is null && openLine == 0) { openLine = i + 1; lang = t[3..].Trim().ToLowerInvariant(); if (lang.Length == 0) lang = null; }
                else { s.Fences.Add(new Fence(openLine, i + 1, lang)); openLine = 0; lang = null; }
            }
            s.InFence[i + 1] = openLine != 0 || t.StartsWith("```");
        }
        if (openLine != 0) s.Fences.Add(new Fence(openLine, lines.Length, lang, Closed: false));
        // Indent: real for text lines; blank lines take the smaller of their neighbours so guides run through gaps.
        var raw = lines.Select(l => l.Trim().Length == 0 ? -1 : l.Length - l.TrimStart(' ', '\t').Length + l.TakeWhile(c => c is ' ' or '\t').Count(c => c == '\t') * 3).ToArray();
        for (var i = 0; i < raw.Length; i++)
        {
            if (raw[i] >= 0) { s.Indent[i + 1] = raw[i]; continue; }
            int prev = 0, next = 0;
            for (var j = i - 1; j >= 0; j--) if (raw[j] >= 0) { prev = raw[j]; break; }
            for (var j = i + 1; j < raw.Length; j++) if (raw[j] >= 0) { next = raw[j]; break; }
            s.Indent[i + 1] = Math.Min(prev, next);
        }
        return s;
    }

    /// <summary>Fold ranges: headings to the next heading of the same or higher level, fenced blocks, and indented children.</summary>
    public IEnumerable<NewFolding> Folds(TextDocument doc, string[] lines, bool codeNote)
    {
        var folds = new List<NewFolding>();
        int EndOf(int line) => doc.GetLineByNumber(line).EndOffset;
        int LastContent(int from, int to) { while (to > from && lines[to - 1].Trim().Length == 0) to--; return to; }
        // Headings (not in code notes, not inside fences).
        if (!codeNote)
        {
            for (var i = 0; i < lines.Length; i++)
            {
                if (InFence[i + 1]) continue;
                var level = NoteColorizer.HeadingLevel(lines[i].TrimStart());
                if (level == 0) continue;
                var end = lines.Length;
                for (var j = i + 1; j < lines.Length; j++)
                {
                    if (InFence[j + 1]) continue;
                    var l = NoteColorizer.HeadingLevel(lines[j].TrimStart());
                    if (l > 0 && l <= level) { end = j; break; }
                }
                end = LastContent(i + 1, end);
                if (end > i + 1) folds.Add(new NewFolding(EndOf(i + 1), EndOf(end)) { Name = " … " });
            }
        }
        foreach (var f in Fences)
            if (f.Close > f.Open) folds.Add(new NewFolding(EndOf(f.Open), EndOf(f.Close)) { Name = " … " });
        // Indented children: a line followed by deeper lines (lists anywhere, any indentation in code).
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].Trim().Length == 0 || (!codeNote && InFence[i + 1] && !Fences.Any(f => f.Open == i + 1) && !InsideCode(i + 1))) continue;
            var me = Indent[i + 1];
            var end = i + 1;
            for (var j = i + 1; j < lines.Length; j++)
            {
                if (lines[j].Trim().Length == 0) continue;
                if (Indent[j + 1] <= me) break;
                end = j + 1;
            }
            if (end > i + 1 && !Fences.Any(f => f.Open == i + 1)) folds.Add(new NewFolding(EndOf(i + 1), EndOf(end)) { Name = " … " });
        }
        return folds.Where(f => f.EndOffset > f.StartOffset).OrderBy(f => f.StartOffset);
    }

    private bool InsideCode(int line) => Fences.Any(f => line > f.Open && line < f.Close);
}

/// <summary>Palette-mapped AvaloniaEdit highlighting definitions, shared by code notes and fenced blocks.</summary>
internal static class CodeColors
{
    /// <summary>The languages AvaloniaEdit ships definitions for, with the words that select them.</summary>
    public static readonly (string Id, string Name, string Extension, string[] Aliases)[] Languages =
    [
        ("python", "Python", ".py", ["py", "python"]),
        ("javascript", "JavaScript", ".js", ["js", "javascript", "ts", "typescript", "jsx", "tsx"]),
        ("csharp", "C#", ".cs", ["cs", "csharp", "c#"]),
        ("json", "JSON", ".json", ["json"]),
        ("html", "HTML", ".html", ["html", "htm"]),
        ("xml", "XML", ".xml", ["xml", "xaml", "axaml"]),
        ("css", "CSS", ".css", ["css"]),
        ("sql", "SQL", ".sql", ["sql", "tsql"]),
        ("powershell", "PowerShell", ".ps1", ["ps1", "powershell", "pwsh"]),
        ("java", "Java", ".java", ["java"]),
        ("cpp", "C / C++", ".cpp", ["cpp", "c", "c++", "h", "hpp"]),
        ("markdown", "Markdown", ".md", ["md", "markdown"]),
        ("php", "PHP", ".php", ["php"]),
        ("vb", "Visual Basic", ".vb", ["vb"]),
    ];

    /// <summary>Ids switched off in Settings: never highlighted, never offered, never loaded.</summary>
    public static HashSet<string> Disabled { get; set; } = [];

    public static (string Id, string Name, string Extension, string[] Aliases)? Find(string? word) =>
        word is null ? null : Languages.Cast<(string Id, string Name, string Extension, string[] Aliases)?>()
            .FirstOrDefault(l => l!.Value.Aliases.Contains(word) || l.Value.Id == word);

    public static string Extension(string? language) => Find(language) is { } l && !Disabled.Contains(l.Id) ? l.Extension : "";

    private static Palette? _mappedFor;

    public static IHighlightingDefinition? Definition(string? language, Palette palette)
    {
        var ext = Extension(language);
        if (ext.Length == 0) return null;
        var definition = HighlightingManager.Instance.GetDefinitionByExtension(ext);
        if (definition is null) return null;
        if (!ReferenceEquals(_mappedFor, palette)) { _mappedFor = palette; }
        Map(definition, palette);
        return definition;
    }

    // The shipped definitions use light-theme colours; every token type is re-pointed at a palette role.
    private static void Map(IHighlightingDefinition definition, Palette p)
    {
        foreach (var color in definition.NamedHighlightingColors)
        {
            var n = color.Name.ToLowerInvariant();
            Color? c = n.Contains("comment") ? p.Comment
                : n.Contains("string") || n.Contains("char") ? p.String
                : n.Contains("number") || n.Contains("digit") ? p.Number
                : n.Contains("method") || n.Contains("function") ? p.Identifier
                : n.Contains("keyword") || n.Contains("modifier") || n.Contains("type") || n.Contains("visibility") || n.Contains("namespace")
                  || n.Contains("statement") || n.Contains("access") || n.Contains("reference") || n.Contains("operator") || n.Contains("tag") ? p.Keyword
                : n.Contains("attribute") || n.Contains("property") || n.Contains("field") || n.Contains("key") || n.Contains("name") ? p.Identifier
                : n.Contains("punctuation") || n.Contains("bracket") || n.Contains("separator") ? p.Muted
                : n.Contains("bool") || n.Contains("null") || n.Contains("constant") ? p.Keyword
                : null;
            if (c is { } value) color.Foreground = new SimpleHighlightingBrush(value);
            color.Background = null;
            color.FontWeight = null;
            if (!n.Contains("comment")) color.FontStyle = null;   // only comments stay italic
        }
    }
}

/// <summary>
/// Real syntax colours inside ``` fences: each fence's body gets its own small document and highlighter,
/// cached until the fence's text changes.
/// </summary>
internal sealed class FenceColorizer(PadView pad) : DocumentColorizingTransformer
{
    private sealed class Cache { public required string Text; public required string? Language; public required TextDocument Doc; public required DocumentHighlighter Highlighter; public required Palette Palette; }
    private readonly Dictionary<int, Cache> _cache = [];   // by fence open line

    public void Reset() => _cache.Clear();

    protected override void ColorizeLine(DocumentLine line)
    {
        if (pad.Mode is NoteMode.Code or NoteMode.Math || pad.Structure is not { } s) return;
        var fence = s.Fences.FirstOrDefault(f => line.LineNumber > f.Open && line.LineNumber < f.Close
                                                 || line.LineNumber > f.Open && f.Close == line.LineNumber && !CurrentContext.Document.GetText(line).TrimStart().StartsWith("```"));
        if (fence is null || fence.Language is null) return;
        var cache = Get(fence);
        if (cache is null) return;
        var inner = line.LineNumber - fence.Open;
        if (inner < 1 || inner > cache.Doc.LineCount) return;
        var highlighted = cache.Highlighter.HighlightLine(inner);
        var innerLine = cache.Doc.GetLineByNumber(inner);
        foreach (var section in highlighted.Sections)
        {
            if (section.Color?.Foreground?.GetBrush(null) is not { } brush) continue;
            var start = line.Offset + (section.Offset - innerLine.Offset);
            var end = Math.Min(line.EndOffset, start + section.Length);
            if (end > start && start >= line.Offset) ChangeLinePart(start, end, e => e.TextRunProperties.SetForegroundBrush(brush));
        }
    }

    private Cache? Get(NoteStructure.Fence fence)
    {
        var doc = CurrentContext.Document;
        // The body ends before the closing fence, or at the last line while the fence is still open.
        var last = Math.Min(fence.Closed ? fence.Close - 1 : fence.Close, doc.LineCount);
        if (last <= fence.Open) return null;
        var from = doc.GetLineByNumber(fence.Open + 1).Offset;
        var to = doc.GetLineByNumber(last).EndOffset;
        var text = doc.GetText(from, to - from);
        if (_cache.TryGetValue(fence.Open, out var c) && c.Text == text && c.Language == fence.Language && ReferenceEquals(c.Palette, pad.Palette)) return c;
        var definition = CodeColors.Definition(fence.Language, pad.Palette);
        if (definition is null) return null;
        var inner = new TextDocument(text);
        var cache = new Cache { Text = text, Language = fence.Language, Doc = inner, Highlighter = new DocumentHighlighter(inner, definition), Palette = pad.Palette };
        _cache[fence.Open] = cache;
        return cache;
    }
}

/// <summary>Faint vertical guides at each indent level, for lists and code; the caret's own level is drawn a little stronger.</summary>
internal sealed class IndentGuideRenderer(PadView pad) : IBackgroundRenderer
{
    public KnownLayer Layer => KnownLayer.Background;

    public void Draw(TextView view, DrawingContext dc)
    {
        if (pad.Structure is not { } s || !view.VisualLinesValid || !pad.Settings.IndentGuides) return;
        var p = pad.Palette;
        var step = pad.Mode == NoteMode.Code ? 4 : 2;
        var space = view.WideSpaceWidth;
        var faint = new Pen(new SolidColorBrush(p.Marker, 0.35), 1);
        var caretLine = pad.CaretLine;
        var caretLevel = caretLine < s.Indent.Length ? s.Indent[caretLine] / step : 0;
        var strong = new Pen(new SolidColorBrush(p.Marker, 0.8), 1);
        foreach (var v in view.VisualLines)
        {
            var n = v.FirstDocumentLine.LineNumber;
            if (n >= s.Indent.Length) continue;
            var levels = s.Indent[n] / step;
            if (levels == 0) continue;
            var top = v.VisualTop - view.VerticalOffset;
            for (var k = 0; k < levels; k++)
            {
                var x = Math.Round(k * step * space + space * 0.5) + 0.5 - view.HorizontalOffset;
                var pen = k == caretLevel - 1 && Math.Abs(n - caretLine) < 40 ? strong : faint;
                dc.DrawLine(pen, new Point(x, top), new Point(x, top + v.Height));
            }
        }
    }
}
