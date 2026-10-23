using System.Globalization;
using System.Text.RegularExpressions;

namespace AntiQuickPad.Core.Text;

public enum TextEditKind { ToggleCheckbox, ContinueList, ApplyTab }

public sealed record TextEditRequest(
    TextEditKind Kind,
    string Text,
    int CaretOffset,
    int? SelectionStart = null,
    int? SelectionEnd = null,
    bool Outdent = false,
    bool AllowList = true,
    int MarkerOffset = -1);

public readonly record struct TextEdit(int Offset, int Length, string Replacement);

public sealed record TextEditResult(
    IReadOnlyList<TextEdit> Edits,
    int CaretOffset,
    int? SelectionStart,
    int? SelectionEnd,
    bool Changed,
    string? CheckboxToken = null,
    int? ContinuationCaret = null);

/// <summary>Pure text operations that return small edits for a host editor to apply.</summary>
public static class MarkdownTextOperations
{
    public static TextEditResult Execute(TextEditRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var document = new PlainTextDocument(request.Text ?? string.Empty);
        var caret = Math.Clamp(request.CaretOffset, 0, document.TextLength);
        switch (request.Kind)
        {
            case TextEditKind.ToggleCheckbox:
            {
                var token = ToggleCheckbox(document, request.MarkerOffset);
                return new(document.Edits, Math.Clamp(caret, 0, document.TextLength), null, null, token is not null, token);
            }
            case TextEditKind.ContinueList:
            {
                var nextCaret = ContinueList(document, caret);
                return new(document.Edits, nextCaret ?? caret, null, null, nextCaret is not null, ContinuationCaret: nextCaret);
            }
            case TextEditKind.ApplyTab:
            {
                var result = ApplyTab(document, caret, request.SelectionStart, request.SelectionEnd, request.Outdent, request.AllowList);
                return new(document.Edits, result.CaretOffset, result.SelectionStart, result.SelectionEnd, result.Changed);
            }
            default: throw new ArgumentOutOfRangeException(nameof(request.Kind));
        }
    }

    private static string? ToggleCheckbox(PlainTextDocument document, int markerOffset)
    {
        if (markerOffset < 0 || markerOffset + 3 > document.TextLength) return null;
        var token = document.GetText(markerOffset, 3);
        var replacement = token == "[ ]" ? "[x]" : token.Equals("[x]", StringComparison.OrdinalIgnoreCase) ? "[ ]" : null;
        if (replacement is null) return null;
        document.Replace(markerOffset, 3, replacement);
        return replacement;
    }

    private static TabResult ApplyTab(PlainTextDocument document, int caretOffset, int? selectionStart, int? selectionEnd, bool outdent, bool allowList)
    {
        caretOffset = Math.Clamp(caretOffset, 0, document.TextLength);
        if (selectionStart is null || selectionEnd is null || selectionStart == selectionEnd)
        {
            var line = document.GetLineByOffset(caretOffset);
            var item = Regex.Match(document.GetText(line), @"^(?<indent>[ \t]*)(?<bullet>[-*+]|[0-9]+[.)])(?<tail>[ \t]+(?:\[[ xX]\][ \t]+)?)$");
            if (allowList && item.Success && caretOffset == line.EndOffset)
            {
                var indent = item.Groups["indent"].Value;
                var bullet = item.Groups["bullet"].Value;
                var removeIndent = indent.StartsWith('\t') ? 1 : Math.Min(4, indent.Length);
                if (outdent && removeIndent == 0) return new(caretOffset, null, null, false);
                var nextIndent = outdent ? indent[removeIndent..] : indent + "    ";
                if (char.IsDigit(bullet[0]))
                {
                    var number = 1L;
                    if (outdent)
                    {
                        for (var n = line.LineNumber - 1; n >= 1; n--)
                        {
                            var previousLine = document.GetLineByNumber(n);
                            var text = document.GetText(previousLine);
                            var previous = Regex.Match(text, @"^(?<indent>[ \t]*)(?<num>[0-9]+)[.)][ \t]+");
                            if (string.IsNullOrWhiteSpace(text)) continue;
                            var whitespace = text.TakeWhile(c => c is ' ' or '\t').Count();
                            if (whitespace < nextIndent.Length) break;
                            if (previous.Success && previous.Groups["indent"].Value == nextIndent && long.TryParse(previous.Groups["num"].Value, out var value) && value < long.MaxValue)
                            { number = value + 1; break; }
                        }
                    }
                    bullet = number.ToString(CultureInfo.InvariantCulture) + bullet[^1];
                }
                var replacement = nextIndent + bullet + item.Groups["tail"].Value;
                document.Replace(line.Offset, line.Length, replacement);
                return new(line.Offset + replacement.Length, null, null, true);
            }
            if (!outdent) { document.Insert(caretOffset, "    "); return new(caretOffset + 4, null, null, true); }
            var prefix = document.GetText(line.Offset, Math.Min(4, line.Length));
            var remove = prefix.StartsWith('\t') ? 1 : prefix.TakeWhile(c => c == ' ').Count();
            if (remove == 0) return new(caretOffset, null, null, false);
            document.Remove(line.Offset, remove);
            return new(MapRemove(caretOffset, line.Offset, remove), null, null, true);
        }

        var start = Math.Clamp(Math.Min(selectionStart.Value, selectionEnd.Value), 0, document.TextLength);
        var end = Math.Clamp(Math.Max(selectionStart.Value, selectionEnd.Value), 0, document.TextLength);
        var first = document.GetLineByOffset(start); var last = document.GetLineByOffset(Math.Max(start, end - 1));
        var lineNumbers = Enumerable.Range(first.LineNumber, last.LineNumber - first.LineNumber + 1);
        var caret = caretOffset; var a = selectionStart.Value; var b = selectionEnd.Value; var changed = false;
        foreach (var number in lineNumbers.Reverse())
        {
            var currentLine = document.GetLineByNumber(number);
            if (!outdent)
            {
                document.Insert(currentLine.Offset, "    "); caret = MapInsert(caret, currentLine.Offset, 4); a = MapInsert(a, currentLine.Offset, 4); b = MapInsert(b, currentLine.Offset, 4); changed = true;
            }
            else
            {
                var p = document.GetText(currentLine.Offset, Math.Min(4, currentLine.Length));
                var count = p.StartsWith('\t') ? 1 : p.TakeWhile(c => c == ' ').Count();
                if (count == 0) continue;
                document.Remove(currentLine.Offset, count); caret = MapRemove(caret, currentLine.Offset, count); a = MapRemove(a, currentLine.Offset, count); b = MapRemove(b, currentLine.Offset, count); changed = true;
            }
        }
        return new(caret, a, b, changed);
    }

    private static int? ContinueList(PlainTextDocument document, int caretOffset)
    {
        var line = document.GetLineByOffset(caretOffset);
        var prefix = document.GetText(line.Offset, caretOffset - line.Offset);
        var match = Regex.Match(prefix, @"^(?<indent>[ \t]*)(?<bullet>[-*+]|[0-9]+[.)])(?:[ \t]+(?<task>\[[ xX]\])(?:[ \t]+|$)|[ \t]+)(?<text>.*)$");
        if (!match.Success) return null;
        var indent = match.Groups["indent"].Value;
        var rest = document.GetText(caretOffset, line.EndOffset - caretOffset);
        string replacement;
        int start, length;
        if (string.IsNullOrWhiteSpace(match.Groups["text"].Value + rest))
        {
            start = line.Offset; length = line.Length; replacement = indent;
        }
        else
        {
            var bullet = match.Groups["bullet"].Value;
            if (char.IsDigit(bullet[0]))
            {
                if (!long.TryParse(bullet[..^1], out var number) || number == long.MaxValue) return null;
                bullet = (number + 1).ToString(CultureInfo.InvariantCulture) + bullet[^1];
            }
            var newline = line.Delimiter.Length > 0 ? line.Delimiter : document.Lines.FirstOrDefault(l => l.Delimiter.Length > 0)?.Delimiter ?? Environment.NewLine;
            start = caretOffset; length = 0;
            replacement = newline + indent + bullet + " " + (match.Groups["task"].Success ? "[ ] " : "");
        }
        document.Replace(start, length, replacement);
        return start + replacement.Length;
    }

    private static int MapInsert(int p, int at, int length) => p >= at ? p + length : p;
    private static int MapRemove(int p, int at, int length) => p >= at + length ? p - length : p >= at ? at : p;
    private readonly record struct TabResult(int CaretOffset, int? SelectionStart, int? SelectionEnd, bool Changed);

    private sealed class PlainTextDocument
    {
        private string _text;
        public List<TextEdit> Edits { get; } = [];
        public string Text => _text;
        public int TextLength => _text.Length;
        public IReadOnlyList<PlainLine> Lines => GetLines();
        public PlainTextDocument(string text) => _text = text;
        public PlainLine GetLineByOffset(int offset) => GetLines().First(l => offset >= l.Offset &&
            (l.DelimiterLength == 0 ? offset <= l.EndOffset : offset < l.EndOffset + l.DelimiterLength));
        public PlainLine GetLineByNumber(int number) => GetLines()[number - 1];
        public string GetText(PlainLine line) => GetText(line.Offset, line.Length);
        public string GetText(int offset, int length) => _text.Substring(offset, length);
        public void Insert(int offset, string text) => Replace(offset, 0, text);
        public void Remove(int offset, int length) => Replace(offset, length, string.Empty);
        public void Replace(int offset, int length, string replacement)
        {
            _text = _text.Remove(offset, length).Insert(offset, replacement);
            Edits.Add(new TextEdit(offset, length, replacement));
        }
        private IReadOnlyList<PlainLine> GetLines()
        {
            var lines = new List<PlainLine>(); var start = 0;
            for (var i = 0; i < _text.Length; i++)
            {
                if (_text[i] is not ('\r' or '\n')) continue;
                var size = _text[i] == '\r' && i + 1 < _text.Length && _text[i + 1] == '\n' ? 2 : 1;
                lines.Add(new PlainLine(start, i, size, _text.Substring(i, size), lines.Count + 1));
                i += size - 1; start = i + 1;
            }
            lines.Add(new PlainLine(start, _text.Length, 0, string.Empty, lines.Count + 1));
            return lines;
        }
    }

    private sealed record PlainLine(int Offset, int EndOffset, int DelimiterLength, string Delimiter, int LineNumber)
    {
        public int Length => EndOffset - Offset;
    }
}
