using System.Globalization;
using System.Text.RegularExpressions;
using AntiNotepad.Model;

namespace AntiNotepad.Editor;

internal static class ListEditing
{
    internal static readonly Regex ListPrefix = new(@"^(\s*)((?:[-*+]\s+)?\[[ xX]\]\s?|[-*+]\s|([0-9]+)([.)])\s)", RegexOptions.Compiled);
    internal static readonly Regex Checkbox = new(@"^(\s*)(?:[-*+]\s+)?\[( |x|X)\]", RegexOptions.Compiled);

    internal readonly record struct LineEdit(int Offset, int RemovalLength, string InsertText);

    internal enum ContinuationKind { Decline, InsertAtCaret, ClearLine, Outdent }

    internal readonly record struct Continuation(ContinuationKind Kind, string? InsertText = null)
    {
        internal bool IsContinued => Kind != ContinuationKind.Decline;
    }

    internal readonly record struct NumberEdit(int LineNumber, int Number);

    internal enum TabAction { Indent, Outdent, InsertSpaces, Swallow, Default }

    /// <summary>Identifies positions through the end of a valid list marker, using zero-based columns.</summary>
    internal static bool IsAtTextStart(string text, int caretColumn)
    {
        var match = ListPrefix.Match(text);
        return IsListLine(text) && caretColumn >= 0 && caretColumn <= match.Length;
    }

    /// <summary>Chooses the list Tab behavior for the caret and modifier keys.</summary>
    internal static TabAction TabDecision(string text, int caretColumn, bool shift, bool ctrl, bool alt)
    {
        if (!IsListLine(text) || alt || (ctrl && shift)) return TabAction.Default;
        if (ctrl) return TabAction.InsertSpaces;
        if (IsAtTextStart(text, caretColumn)) return shift ? TabAction.Outdent : TabAction.Indent;
        return shift ? TabAction.Swallow : TabAction.InsertSpaces;
    }

    internal static LineEdit? ToggleCheckbox(string text)
    {
        var match = Checkbox.Match(text);
        if (match.Success)
            return new LineEdit(match.Groups[2].Index, 1, match.Groups[2].Value == " " ? "x" : " ");

        var indent = text.Length - text.TrimStart().Length;
        var bullet = Regex.Match(text, @"^\s*[-*+]\s+");
        return bullet.Success
            ? new LineEdit(indent, bullet.Length - indent, "[ ] ")
            : new LineEdit(indent, 0, "[ ] ");
    }

    internal static LineEdit? Indent(string text, bool deeper)
    {
        if (deeper) return new LineEdit(0, 0, "  ");

        var leadingSpaces = text.Length - text.TrimStart(' ').Length;
        return leadingSpaces > 0 ? new LineEdit(0, Math.Min(2, leadingSpaces), "") : null;
    }

    internal static bool TryListNumber(string line, out int number) => TryListNumber(ListPrefix.Match(line), out number);

    internal static bool TryListNumber(Match match, out int number)
    {
        number = 0;
        var digits = match.Groups[3];
        return digits.Success && digits.Length <= 9
            && int.TryParse(digits.Value, NumberStyles.None, CultureInfo.InvariantCulture, out number);
    }

    internal static bool IsListLine(string text)
    {
        var match = ListPrefix.Match(text);
        return match.Success && (!match.Groups[3].Success || TryListNumber(match, out _));
    }

    internal static Continuation Continue(string text, int lineNumber, NoteMode mode)
    {
        if (mode == NoteMode.Code) return new(ContinuationKind.Decline);

        var match = ListPrefix.Match(text);
        if (!match.Success)
        {
            return mode == NoteMode.List && (lineNumber == 1 || text.Trim().Length > 0)
                ? new(ContinuationKind.InsertAtCaret, "\n[ ] ")
                : new(ContinuationKind.Decline);
        }

        if (match.Groups[3].Success
            && (!TryListNumber(match, out var number) || number == 999_999_999))
            return new(ContinuationKind.Decline);

        var indent = match.Groups[1].Value;
        var prefix = match.Groups[2].Value;
        if (text.Trim() == prefix.Trim())
            return indent.Length >= 2
                ? new(ContinuationKind.Outdent)
                : new(ContinuationKind.ClearLine);

        string next;
        if (match.Groups[3].Success)
        {
            TryListNumber(match, out var nextNumber);
            next = $"{nextNumber + 1}{match.Groups[4].Value} ";
        }
        else if (prefix.Contains('[')) next = Regex.Replace(prefix, @"\[[ xX]\]", "[ ]");
        else next = prefix;

        if (!next.EndsWith(' ')) next += " ";
        return new(ContinuationKind.InsertAtCaret, "\n" + indent + next);
    }

    /// <summary>Returns the numbered markers that should change for the block containing a 1-based line number.</summary>
    internal static NumberEdit[] Renumber(int lineCount, Func<int, string> getLine, int lineNumber, bool fresh = false)
        => RenumberBlock(lineCount, getLine, lineNumber, fresh ? new HashSet<int> { lineNumber } : null, out _);

    /// <summary>Renumbers one block, restarting moved numbered sequences where needed.</summary>
    internal static NumberEdit[] RenumberBlock(int lineCount, Func<int, string> getLine, int lineNumber,
        HashSet<int>? freshLines, out int last)
    {
        bool InBlock(int number)
        {
            var text = getLine(number);
            return text.Trim().Length > 0 && (ListPrefix.IsMatch(text) || text.StartsWith(' '));
        }

        last = lineNumber;
        if (lineNumber < 1 || lineNumber > lineCount || !InBlock(lineNumber)) return [];

        var first = lineNumber;
        while (first > 1 && InBlock(first - 1)) first--;
        while (last < lineCount && InBlock(last + 1)) last++;

        var edits = new List<NumberEdit>();
        var counts = new Dictionary<int, int>();
        for (var number = first; number <= last; number++)
        {
            var match = ListPrefix.Match(getLine(number));
            if (!match.Success) continue;

            var indent = match.Groups[1].Length;
            foreach (var deeper in counts.Keys.Where(key => key > indent).ToList()) counts.Remove(deeper);
            if (!match.Groups[3].Success) { counts.Remove(indent); continue; }
            if (!TryListNumber(match, out var own)) { counts.Remove(indent); continue; }
            if (counts.TryGetValue(indent, out var previous) && previous == 999_999_999)
            { counts.Remove(indent); continue; }

            var want = counts.TryGetValue(indent, out var before) ? before + 1 : freshLines?.Contains(number) == true ? 1 : own;
            counts[indent] = want;
            if (want != own) edits.Add(new(number, want));
        }

        return edits.ToArray();
    }
}
