namespace AntiQuickPad.Editor;

internal sealed class GuideState(MarkdownScanner scanner)
{
    internal static int Columns(string text)
    {
        var n = 0;
        foreach (var c in text) { if (c == ' ') n++; else if (c == '\t') n += 4 - n % 4; else break; }
        return n;
    }
    internal int Depth(int number)
    {
        var lines = scanner.Lines;
        var i = number - 1;
        if (!string.IsNullOrWhiteSpace(lines[i].Text)) return Columns(lines[i].Text) / 4;
        var before = i - 1; var after = i + 1;
        while (before >= 0 && string.IsNullOrWhiteSpace(lines[before].Text)) before--;
        while (after < lines.Count && string.IsNullOrWhiteSpace(lines[after].Text)) after++;
        return before >= 0 && after < lines.Count && lines[before].IsCode == lines[after].IsCode ? Math.Min(Columns(lines[before].Text), Columns(lines[after].Text)) / 4 : 0;
    }
    // The deepest existing guide on the caret line owns the contiguous block at that depth.
    // Fence markers are boundaries; body indentation is independent of the fence delimiter.
    internal (int Level, int First, int Last) ActiveBlock(int caretLine)
    {
        var lines = scanner.Lines;
        var info = scanner.GetLine(caretLine);
        var level = Columns(info.Text) / 4;
        if (level == 0 || info.IsFence) return default;
        bool Contains(int n) => !lines[n - 1].IsFence && lines[n - 1].IsCode == info.IsCode && Depth(n) >= level;
        var first = caretLine; var last = caretLine;
        while (first > 1 && Contains(first - 1)) first--;
        while (last < lines.Count && Contains(last + 1)) last++;
        return (level, first, last);
    }

    internal int CaretLine { get; set; } = 1;
}
