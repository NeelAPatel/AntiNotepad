using System.Text.RegularExpressions;

namespace AntiQuickPad.Core.Text;

/// <summary>Markdown metadata used by editor renderers, without any editor framework types.</summary>
public static class MarkdownScanner
{
    private static readonly Regex Heading = new(@"^(?<indent> {0,3})(?<hash>#{1,6})(?:[ \t]+|$)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    private static readonly Regex Task = new(@"^\s*[-*+]\s+\[(?<mark>[ xX])\]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    private static readonly Regex Bullet = new(@"^\s*(?:[-*+]|\d+[.)])\s+", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    private static readonly Regex FenceOpen = new(@"^\s*(`{3,}|~{3,})\s*(?<lang>[^\s`]*)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    private static readonly Regex BoldPattern = new(@"\*\*[^*\r\n].*?\*\*|__[^_\r\n].*?__", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    private static readonly Regex ItalicPattern = new(@"(?<!\*)\*(?!\*)[^*\r\n].*?\*(?!\*)|(?<!_)_(?!_)[^_\r\n].*?_(?!_)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    private static readonly Regex StrikePattern = new(@"~~[^~\r\n].*?~~", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));

    public static MarkdownScanResult Scan(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var sourceLines = SplitLines(text);
        var lines = new MarkdownLineInfo[sourceLines.Count];
        for (var i = 0; i < lines.Length; i++)
        {
            var line = sourceLines[i];
            var heading = Heading.Match(line.Text);
            var task = Task.Match(line.Text);
            var fence = FenceOpen.Match(line.Text);
            var info = new MarkdownLineInfo(line.Text)
            {
                HeadingLevel = heading.Success ? heading.Groups["hash"].Length : 0,
                HeadingMarkerLength = heading.Success ? heading.Groups["indent"].Length + heading.Groups["hash"].Length : 0,
                CheckboxOffset = task.Success ? task.Groups["mark"].Index - 1 : -1,
                CheckboxChecked = task.Success && task.Groups["mark"].Value is "x" or "X",
                BulletLength = Bullet.Match(line.Text) is { Success: true } bullet ? bullet.Length : 0,
                FenceMarker = fence.Success ? fence.Groups[1].Value : null,
                FenceLanguage = fence.Success ? fence.Groups["lang"].Value : string.Empty
            };
            info.Bold.AddRange(BoldPattern.Matches(line.Text).Select(m => new MarkdownSpan(m.Index, m.Length, 2)));
            info.Italic.AddRange(ItalicPattern.Matches(line.Text).Select(m => new MarkdownSpan(m.Index, m.Length, 1)));
            info.Strike.AddRange(StrikePattern.Matches(line.Text).Select(m => new MarkdownSpan(m.Index, m.Length, 2)));
            lines[i] = info;
        }

        var folds = new List<MarkdownFoldSpan>();
        var openFence = -1; var fenceChar = '\0'; var fenceLen = 0; var pythonFence = false;
        var firstPython = lines.Length > 0 && lines[0].Text.TrimStart().StartsWith("python:", StringComparison.OrdinalIgnoreCase);
        for (var i = 0; i < lines.Length; i++)
        {
            var info = lines[i];
            if (firstPython && i == 0) { info.PythonMarker = true; info.IsCode = true; info.IsPython = true; continue; }
            if (info.FenceMarker is string marker)
            {
                if (openFence < 0)
                {
                    openFence = i; fenceChar = marker[0]; fenceLen = marker.Length;
                    pythonFence = info.FenceLanguage.Equals("python", StringComparison.OrdinalIgnoreCase) || info.FenceLanguage.Equals("py", StringComparison.OrdinalIgnoreCase);
                    info.IsFence = true; info.IsCode = true; info.IsPython = pythonFence;
                }
                else if (marker[0] == fenceChar && marker.Length >= fenceLen)
                {
                    info.IsFence = true; info.IsCode = true; info.IsPython = pythonFence;
                    if (i > openFence + 1) folds.Add(Fold(sourceLines, openFence, i - 1));
                    openFence = -1; pythonFence = false;
                }
                else { info.IsCode = openFence >= 0; info.IsPython = pythonFence; }
                continue;
            }
            if (openFence >= 0 && i == openFence + 1 && lines[openFence].FenceLanguage.Length == 0 &&
                info.Text.Trim().Equals("python:", StringComparison.OrdinalIgnoreCase))
            { pythonFence = true; info.PythonMarker = true; }
            info.IsCode = openFence >= 0;
            info.IsPython = pythonFence;
            if (firstPython && i > 0) { info.IsCode = true; info.IsPython = true; }
        }

        for (var i = 0; i < lines.Length; i++)
            if (lines[i].IsPython && !lines[i].IsFence && !lines[i].PythonMarker)
                lines[i].CommentOffset = FindComment(lines[i].Text);
        if (openFence >= 0)
        {
            for (var i = openFence + 1; i < lines.Length; i++) { lines[i].IsCode = true; lines[i].IsPython = pythonFence; }
            if (lines.Length > openFence + 1) folds.Add(Fold(sourceLines, openFence, lines.Length - 1));
        }

        var headings = Enumerable.Range(0, lines.Length).Where(i => lines[i].HeadingLevel > 0 && !lines[i].IsCode).ToArray();
        for (var h = 0; h < headings.Length; h++)
        {
            var start = headings[h]; var end = lines.Length - 1;
            for (var j = h + 1; j < headings.Length; j++) if (lines[headings[j]].HeadingLevel <= lines[start].HeadingLevel) { end = headings[j] - 1; break; }
            for (var j = start + 1; j < end; j++)
                if (!lines[j].IsCode && !lines[j + 1].IsCode && string.IsNullOrWhiteSpace(lines[j].Text) && string.IsNullOrWhiteSpace(lines[j + 1].Text))
                { end = j - 1; break; }
            if (end > start) folds.Add(Fold(sourceLines, start, end));
        }
        if (firstPython && lines.Length > 1) folds.Add(Fold(sourceLines, 0, lines.Length - 1));
        return new MarkdownScanResult(lines, folds.OrderBy(f => f.StartOffset).ToArray());
    }

    private static List<SourceLine> SplitLines(string text)
    {
        var result = new List<SourceLine>();
        var start = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] is not ('\r' or '\n')) continue;
            var delimiter = text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n' ? 2 : 1;
            result.Add(new SourceLine(text[start..i], start, i, delimiter, text.Substring(i, delimiter)));
            i += delimiter - 1; start = i + 1;
        }
        result.Add(new SourceLine(text[start..], start, text.Length, 0, string.Empty));
        return result;
    }

    private static MarkdownFoldSpan Fold(IReadOnlyList<SourceLine> lines, int startLine, int endLine) =>
        new(lines[startLine].EndOffset, lines[endLine].EndOffset);

    private static int FindComment(string text)
    {
        char quote = '\0'; var escaped = false;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (escaped) { escaped = false; continue; }
            if (c == '\\' && quote != '\0') { escaped = true; continue; }
            if (quote != '\0') { if (c == quote) quote = '\0'; continue; }
            if (c is '\'' or '"') { quote = c; continue; }
            if (c == '#') return i;
        }
        return -1;
    }

    private sealed record SourceLine(string Text, int Offset, int EndOffset, int DelimiterLength, string Delimiter);
}

public sealed record MarkdownScanResult(IReadOnlyList<MarkdownLineInfo> Lines, IReadOnlyList<MarkdownFoldSpan> Foldings);
public sealed record MarkdownFoldSpan(int StartOffset, int EndOffset);
public readonly record struct MarkdownSpan(int Start, int Length, int MarkerLength);

public sealed class MarkdownLineInfo(string text)
{
    public string Text { get; } = text;
    public int HeadingLevel { get; init; }
    public int HeadingMarkerLength { get; init; }
    public int CheckboxOffset { get; init; } = -1;
    public bool CheckboxChecked { get; init; }
    public int BulletLength { get; init; }
    public string? FenceMarker { get; init; }
    public string FenceLanguage { get; init; } = string.Empty;
    public bool IsFence { get; set; }
    public bool IsCode { get; set; }
    public bool IsPython { get; set; }
    public bool PythonMarker { get; set; }
    public int CommentOffset { get; set; } = -1;
    public List<MarkdownSpan> Bold { get; } = [];
    public List<MarkdownSpan> Italic { get; } = [];
    public List<MarkdownSpan> Strike { get; } = [];
}
