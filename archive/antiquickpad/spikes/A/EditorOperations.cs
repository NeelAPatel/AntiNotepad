using AvaloniaEdit.Document;

namespace InputProbe;

internal static class EditorOperations
{
    internal readonly record struct TabResult(int CaretOffset, int? SelectionStart, int? SelectionEnd, bool Changed);

    public static string? ToggleCheckbox(TextDocument document, int markerOffset, ref int caretOffset)
    {
        if (markerOffset < 0 || markerOffset + 3 > document.TextLength) return null;
        var token = document.GetText(markerOffset, 3);
        var replacement = token.Equals("[ ]", StringComparison.Ordinal) ? "[x]" :
            token.Equals("[x]", StringComparison.OrdinalIgnoreCase) ? "[ ]" : null;
        if (replacement is null) return null;
        document.Replace(markerOffset, 3, replacement);
        caretOffset = Math.Clamp(caretOffset, 0, document.TextLength);
        return replacement;
    }

    // This is the shared handler path for keyboard input and smoke checks. A null
    // selection means a caret-only edit on the current line.
    public static TabResult ApplyTab(TextDocument document, int caretOffset, int? selectionStart, int? selectionEnd, bool outdent)
    {
        caretOffset = Math.Clamp(caretOffset, 0, document.TextLength);
        if (selectionStart is null || selectionEnd is null || selectionStart == selectionEnd)
        {
            var line = document.GetLineByOffset(caretOffset);
            var prefixLength = Math.Min(line.Length, 4);
            var prefix = document.GetText(line.Offset, prefixLength);
            if (outdent)
            {
                var remove = prefix.StartsWith('\t') ? 1 : prefix.TakeWhile(c => c == ' ').Count();
                remove = Math.Min(remove, 4);
                if (remove == 0) return new(caretOffset, null, null, false);
                document.Remove(line.Offset, remove);
                return new(MapRemove(caretOffset, line.Offset, remove), null, null, true);
            }

            document.Insert(caretOffset, "    ");
            return new(caretOffset + 4, null, null, true);
        }

        var start = Math.Clamp(Math.Min(selectionStart.Value, selectionEnd.Value), 0, document.TextLength);
        var end = Math.Clamp(Math.Max(selectionStart.Value, selectionEnd.Value), 0, document.TextLength);
        var first = document.GetLineByOffset(start);
        var last = document.GetLineByOffset(Math.Max(start, end - 1));
        var lineNumbers = new List<int>();
        for (var line = first; line.LineNumber <= last.LineNumber; line = line.NextLine!)
        {
            lineNumbers.Add(line.LineNumber);
            if (line == last || line.NextLine is null) break;
        }

        var caret = caretOffset;
        var a = selectionStart.Value;
        var b = selectionEnd.Value;
        var changed = false;
        document.BeginUpdate();
        try
        {
            foreach (var number in lineNumbers.Distinct().OrderByDescending(x => x))
            {
                var line = document.GetLineByNumber(number);
                if (!outdent)
                {
                    document.Insert(line.Offset, "    ");
                    caret = MapInsert(caret, line.Offset, 4); a = MapInsert(a, line.Offset, 4); b = MapInsert(b, line.Offset, 4);
                    changed = true;
                }
                else
                {
                    var prefix = document.GetText(line.Offset, Math.Min(4, line.Length));
                    var remove = prefix.StartsWith('\t') ? 1 : prefix.TakeWhile(c => c == ' ').Count();
                    if (remove > 0)
                    {
                        document.Remove(line.Offset, remove);
                        caret = MapRemove(caret, line.Offset, remove); a = MapRemove(a, line.Offset, remove); b = MapRemove(b, line.Offset, remove);
                        changed = true;
                    }
                }
            }
        }
        finally { document.EndUpdate(); }
        return new(caret, a, b, changed);
    }

    private static int MapInsert(int offset, int at, int length) => offset >= at ? offset + length : offset;
    private static int MapRemove(int offset, int at, int length) => offset >= at + length ? offset - length : offset >= at ? at : offset;
}
