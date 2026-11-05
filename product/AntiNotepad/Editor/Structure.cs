using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Rendering;
using AntiNotepad.Model;
using AntiNotepad.Theme;

namespace AntiNotepad.Editor;

/// <summary>A note's block structure, computed once per edit: fenced code blocks, indent levels and fold ranges.</summary>
internal sealed class NoteStructure
{
    public sealed record Fence(int Open, int Close, string? Language, bool Closed = true);   // 1-based; an unclosed fence runs to the last line

    private readonly record struct LineSignature(bool IsBlank, int RawIndent, int HeadingLevel, bool IsFence, string? FenceLanguage);
    private readonly record struct FoldRange(int StartLine, int EndLine);                    // inclusive, 1-based lines

    public List<Fence> Fences { get; } = [];
    public int[] Indent { get; private set; } = [];                       // per line (1-based), in spaces; blank lines inherit
    public bool[] InFence { get; private set; } = [];
    private LineSignature[] _lineSignatures = [];
    private List<FoldRange> _textNoteFolds = [];
    private List<FoldRange> _codeNoteFolds = [];

    /// <summary>Number of line signatures computed by the most recent Build or Update call.</summary>
    public int LastAnalysedLineCount { get; private set; }

    public static NoteStructure Build(string[] lines)
    {
        return BuildCore(lines, null, 0, -1, 0);
    }

    /// <summary>
    /// Reuses this structure when every changed line keeps the same structural signature. Bounds are zero-based and inclusive.
    /// A line-count change or a structural change falls back to a full rebuild.
    /// </summary>
    public NoteStructure Update(string[] lines, int firstChanged, int lastChanged)
    {
        if (lines.Length != _lineSignatures.Length) return Build(lines);
        if (firstChanged > lastChanged)
        {
            if (lines.Length == 0 && firstChanged == 0 && lastChanged == -1)
            {
                LastAnalysedLineCount = 0;
                return this;
            }
            return Build(lines);
        }
        if (firstChanged < 0 || lastChanged >= lines.Length) return Build(lines);

        var count = lastChanged - firstChanged + 1;
        var changedSignatures = new LineSignature[count];
        var matches = true;
        for (var i = firstChanged; i <= lastChanged; i++)
        {
            var signature = GetSignature(lines[i]);
            changedSignatures[i - firstChanged] = signature;
            if (signature != _lineSignatures[i]) matches = false;
        }

        if (matches)
        {
            LastAnalysedLineCount = count;
            return this;
        }

        // Keep the signatures already computed for the changed region; BuildCore computes only the remaining lines.
        return BuildCore(lines, changedSignatures, firstChanged, lastChanged, count);
    }

    private static NoteStructure BuildCore(string[] lines, LineSignature[]? knownSignatures, int knownFrom, int knownTo, int alreadyAnalysed)
    {
        var signatures = new LineSignature[lines.Length];
        var analysed = alreadyAnalysed;
        for (var i = 0; i < lines.Length; i++)
        {
            if (knownSignatures is not null && i >= knownFrom && i <= knownTo)
            {
                signatures[i] = knownSignatures[i - knownFrom];
            }
            else
            {
                signatures[i] = GetSignature(lines[i]);
                analysed++;
            }
        }

        var s = new NoteStructure
        {
            Indent = new int[lines.Length + 2],
            InFence = new bool[lines.Length + 2],
            _lineSignatures = signatures,
            LastAnalysedLineCount = analysed
        };

        int openLine = 0;
        string? lang = null;
        for (var i = 0; i < signatures.Length; i++)
        {
            var signature = signatures[i];
            if (signature.IsFence)
            {
                if (openLine == 0) { openLine = i + 1; lang = signature.FenceLanguage; }
                else { s.Fences.Add(new Fence(openLine, i + 1, lang)); openLine = 0; lang = null; }
            }
            s.InFence[i + 1] = openLine != 0 || signature.IsFence;
        }
        if (openLine != 0) s.Fences.Add(new Fence(openLine, lines.Length, lang, Closed: false));

        // Blank lines inherit the smaller of their nearest nonblank neighbours. Two passes keep this linear.
        for (int i = 0, previous = 0; i < signatures.Length; i++)
        {
            if (signatures[i].RawIndent >= 0)
            {
                previous = signatures[i].RawIndent;
                s.Indent[i + 1] = previous;
            }
            else s.Indent[i + 1] = previous;
        }
        for (int i = signatures.Length - 1, next = 0; i >= 0; i--)
        {
            if (signatures[i].RawIndent >= 0) next = signatures[i].RawIndent;
            else s.Indent[i + 1] = Math.Min(s.Indent[i + 1], next);
        }

        s.CacheFoldRanges(signatures);
        return s;
    }

    private static LineSignature GetSignature(string line)
    {
        var trimmedStart = line.TrimStart();
        var isFence = trimmedStart.StartsWith("```");
        string? language = null;
        if (isFence)
        {
            var value = trimmedStart[3..].Trim().ToLowerInvariant();
            if (value.Length != 0) language = value;
        }
        var isBlank = line.Trim().Length == 0;
        var rawIndent = isBlank
            ? -1
            : line.Length - line.TrimStart(' ', '\t').Length + line.TakeWhile(c => c is ' ' or '\t').Count(c => c == '\t') * 3;
        return new LineSignature(isBlank, rawIndent, NoteColorizer.HeadingLevel(trimmedStart), isFence, language);
    }

    private void CacheFoldRanges(LineSignature[] signatures)
    {
        var count = signatures.Length;
        var blank = new bool[count];
        var fenceOpen = new bool[count];
        var insideCode = new bool[count];
        for (var i = 0; i < count; i++) blank[i] = signatures[i].IsBlank;
        foreach (var fence in Fences)
        {
            fenceOpen[fence.Open - 1] = true;
            for (var line = fence.Open + 1; line < fence.Close; line++) insideCode[line - 1] = true;
        }

        var headingFolds = new List<FoldRange>();
        var lastContentLine = new int[count + 1];
        for (var i = 0; i < count; i++) lastContentLine[i + 1] = blank[i] ? lastContentLine[i] : i + 1;

        // The nearest qualifying blank-pair boundary at or after each line.
        var nextGap = new int[count + 1];
        var nearestGap = count;
        for (var i = count - 1; i >= 0; i--)
        {
            if (i > 0 && blank[i] && blank[i - 1] && !InFence[i + 1]) nearestGap = i;
            nextGap[i] = nearestGap;
        }
        nextGap[count] = count;

        // At most six heading levels exist, so a reverse pass finds every next same-or-higher heading in linear time.
        var latestHeadingAtLevel = new int[7];
        Array.Fill(latestHeadingAtLevel, count);
        for (var i = count - 1; i >= 0; i--)
        {
            var level = signatures[i].HeadingLevel;
            if (level > 0 && !InFence[i + 1])
            {
                var boundary = count;
                for (var candidateLevel = 1; candidateLevel <= level; candidateLevel++)
                    boundary = Math.Min(boundary, latestHeadingAtLevel[candidateLevel]);
                var gapFrom = Math.Min(i + 2, count);
                boundary = Math.Min(boundary, nextGap[gapFrom]);
                var endLine = Math.Max(i + 1, lastContentLine[boundary]);
                if (endLine > i + 1) headingFolds.Add(new FoldRange(i + 1, endLine));
            }
            if (level > 0 && !InFence[i + 1]) latestHeadingAtLevel[level] = i;
        }
        headingFolds.Reverse();

        var fenceFolds = new List<FoldRange>(Fences.Count);
        foreach (var fence in Fences)
            if (fence.Close > fence.Open) fenceFolds.Add(new FoldRange(fence.Open, fence.Close));

        // For each nonblank line, find the next nonblank line with indentation no deeper than its own.
        var nextAtOrLess = new int[count];
        Array.Fill(nextAtOrLess, count);
        var stack = new int[count];
        var top = -1;
        for (var i = count - 1; i >= 0; i--)
        {
            if (blank[i]) continue;
            while (top >= 0 && Indent[stack[top] + 1] > Indent[i + 1]) top--;
            if (top >= 0) nextAtOrLess[i] = stack[top];
            stack[++top] = i;
        }
        var previousNonblankIndex = new int[count + 1];
        Array.Fill(previousNonblankIndex, -1);
        for (var i = 0; i < count; i++) previousNonblankIndex[i + 1] = blank[i] ? previousNonblankIndex[i] : i;

        var textIndentFolds = new List<FoldRange>();
        var codeIndentFolds = new List<FoldRange>();
        for (var i = 0; i < count; i++)
        {
            if (blank[i] || fenceOpen[i]) continue;
            var next = nextAtOrLess[i];
            var endLine = previousNonblankIndex[next] + 1;
            if (endLine <= i + 1) continue;

            var range = new FoldRange(i + 1, endLine);
            codeIndentFolds.Add(range);
            if (!InFence[i + 1] || insideCode[i]) textIndentFolds.Add(range);
        }

        _textNoteFolds = [.. headingFolds, .. fenceFolds, .. textIndentFolds];
        _codeNoteFolds = [.. fenceFolds, .. codeIndentFolds];
    }

    /// <summary>
    /// Fold ranges: headings to the next heading of the same or higher level or the first gap of two blank lines
    /// (whichever comes first), fenced blocks, and indented children.
    /// </summary>
    public IEnumerable<NewFolding> Folds(TextDocument doc, string[] lines, bool codeNote)
    {
        var ranges = codeNote ? _codeNoteFolds : _textNoteFolds;
        var folds = new List<NewFolding>(ranges.Count);
        foreach (var range in ranges)
        {
            var start = doc.GetLineByNumber(range.StartLine).EndOffset;
            var end = doc.GetLineByNumber(range.EndLine).EndOffset;
            if (end > start) folds.Add(new NewFolding(start, end) { Name = " … " });
        }
        return folds.OrderBy(f => f.StartOffset);
    }
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

/// <summary>Per-opening-line fence text and highlighter cache, pruned to the current structure.</summary>
internal sealed class FenceCache<T> where T : class
{
    private sealed class Entry
    {
        public required ITextSourceVersion Version;
        public required int BodyEndLine;
        public required bool Closed;
        public required string Text;
        public required string? Language;
        public required Palette Palette;
        public required T? Value;
    }

    private readonly Dictionary<int, Entry> _entries = [];
    public int Count => _entries.Count;

    public bool TryGet(int openLine, int bodyEndLine, bool closed, ITextSourceVersion version,
        string? language, Palette palette, out T? value)
    {
        if (_entries.TryGetValue(openLine, out var entry) && ReferenceEquals(entry.Version, version)
            && entry.BodyEndLine == bodyEndLine && entry.Closed == closed
            && entry.Language == language && ReferenceEquals(entry.Palette, palette))
        {
            value = entry.Value;
            return true;
        }
        value = null;
        return false;
    }

    public T? Get(int openLine, int bodyEndLine, bool closed, ITextSourceVersion version,
        string? language, Palette palette,
        Func<string> copyText, Func<string, T?> create)
    {
        _entries.TryGetValue(openLine, out var entry);
        var text = entry is not null && ReferenceEquals(entry.Version, version)
            && entry.BodyEndLine == bodyEndLine && entry.Closed == closed ? entry.Text : copyText();
        if (entry is not null && entry.Text == text && entry.Language == language && ReferenceEquals(entry.Palette, palette))
        {
            entry.Version = version;
            entry.BodyEndLine = bodyEndLine;
            entry.Closed = closed;
            return entry.Value;
        }

        var value = create(text);
        if (entry?.Value is IDisposable old) old.Dispose();
        _entries[openLine] = new Entry
        {
            Version = version, BodyEndLine = bodyEndLine, Closed = closed,
            Text = text, Language = language, Palette = palette, Value = value
        };
        return value;
    }

    public void Prune(IEnumerable<int> currentOpenLines)
    {
        var current = currentOpenLines.ToHashSet();
        foreach (var open in _entries.Keys.Where(open => !current.Contains(open)).ToArray()) Remove(open);
    }

    public void Clear()
    {
        foreach (var open in _entries.Keys.ToArray()) Remove(open);
    }

    private void Remove(int openLine)
    {
        if (_entries.Remove(openLine, out var entry) && entry.Value is IDisposable disposable) disposable.Dispose();
    }
}

/// <summary>Real syntax colours inside ``` fences: each fence's body gets its own small document and highlighter.</summary>
internal sealed class FenceColorizer(PadView pad) : DocumentColorizingTransformer
{
    private sealed class CodeHighlight : IDisposable
    {
        public required TextDocument Doc;
        public required DocumentHighlighter Highlighter;
        public void Dispose() { if (Highlighter is IDisposable disposable) disposable.Dispose(); }
    }
    private readonly FenceCache<CodeHighlight> _cache = new();   // by current fence open line

    public void Reset() => _cache.Clear();

    public void UpdateStructure(NoteStructure structure) => _cache.Prune(structure.Fences.Select(f => f.Open));

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

    private CodeHighlight? Get(NoteStructure.Fence fence)
    {
        var doc = CurrentContext.Document;
        // The body ends before the closing fence, or at the last line while the fence is still open.
        var last = Math.Min(fence.Closed ? fence.Close - 1 : fence.Close, doc.LineCount);
        if (_cache.TryGet(fence.Open, last, fence.Closed, doc.Version, fence.Language, pad.Palette, out var current)) return current;
        if (last <= fence.Open) return null;
        var from = doc.GetLineByNumber(fence.Open + 1).Offset;
        var to = doc.GetLineByNumber(last).EndOffset;
        return _cache.Get(fence.Open, last, fence.Closed, doc.Version, fence.Language, pad.Palette,
            () => doc.GetText(from, to - from), text =>
            {
                var definition = CodeColors.Definition(fence.Language, pad.Palette);
                if (definition is null) return null;
                var inner = new TextDocument(text);
                return new CodeHighlight { Doc = inner, Highlighter = new DocumentHighlighter(inner, definition) };
            });
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
        var faint = new Pen(new SolidColorBrush(p.Marker, Paint.MarkerFaint), 1);
        var caretLine = pad.CaretLine;
        var caretLevel = caretLine < s.Indent.Length ? s.Indent[caretLine] / step : 0;
        var strong = new Pen(new SolidColorBrush(p.Marker, Paint.MarkerStrong), 1);
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
