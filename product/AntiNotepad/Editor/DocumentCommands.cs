using AvaloniaEdit.Document;
using System.Globalization;
using AntiNotepad.Model;

namespace AntiNotepad.Editor;

/// <summary>Applies pure list edits to a document line without depending on an editor control.</summary>
internal static class DocumentCommands
{
    internal static void ToggleCheckbox(TextDocument document, int lineNumber)
    {
        if (!TryGetLine(document, lineNumber, out var line)) return;
        Apply(document, line, ListEditing.ToggleCheckbox(document.GetText(line)));
    }

    internal static void Indent(TextDocument document, int lineNumber, bool deeper)
    {
        if (!TryGetLine(document, lineNumber, out var line)) return;
        Apply(document, line, ListEditing.Indent(document.GetText(line), deeper));
    }

    /// <summary>Changes each selected list line as one undoable edit and updates its numbering.</summary>
    internal static bool IndentListLines(TextDocument document, int firstLine, int lastLine, bool deeper)
    {
        var lines = Enumerable.Range(firstLine, lastLine - firstLine + 1)
            .Where(number => IsListLine(document, number)).ToArray();
        if (lines.Length == 0) return false;
        using (document.RunUpdate())
        {
            foreach (var number in lines) Indent(document, number, deeper);
            foreach (var number in lines) Renumber(document, number, fresh: deeper);
        }
        return true;
    }

    internal static bool IsListLine(TextDocument document, int lineNumber)
        => TryGetLine(document, lineNumber, out var line) && ListEditing.IsListLine(document.GetText(line));

    internal static bool ContinueList(TextDocument document, int lineNumber, NoteMode mode, Action<string> insertAtCaret)
    {
        if (!TryGetLine(document, lineNumber, out var line)) return false;

        var continuation = ListEditing.Continue(document.GetText(line), lineNumber, mode);
        switch (continuation.Kind)
        {
            case ListEditing.ContinuationKind.InsertAtCaret:
                insertAtCaret(continuation.InsertText!);
                break;
            case ListEditing.ContinuationKind.ClearLine:
                document.Replace(line.Offset, line.Length, "");
                break;
            case ListEditing.ContinuationKind.Outdent:
                document.Remove(line.Offset, 2);
                Renumber(document, lineNumber);
                break;
        }

        return continuation.IsContinued;
    }

    internal static void Renumber(TextDocument document, int lineNumber, bool fresh = false)
    {
        var edits = ListEditing.Renumber(document.LineCount,
            number => document.GetText(document.GetLineByNumber(number)), lineNumber, fresh);

        foreach (var edit in edits)
        {
            var line = document.GetLineByNumber(edit.LineNumber);
            var match = ListEditing.ListPrefix.Match(document.GetText(line));
            if (!match.Success || !ListEditing.TryListNumber(match, out _)) continue;

            var digits = match.Groups[3];
            document.Replace(line.Offset + digits.Index, digits.Length,
                edit.Number.ToString(CultureInfo.InvariantCulture));
        }
    }

    private static bool TryGetLine(TextDocument document, int lineNumber, out DocumentLine line)
    {
        if (lineNumber < 1 || lineNumber > document.LineCount)
        {
            line = null!;
            return false;
        }

        line = document.GetLineByNumber(lineNumber);
        return true;
    }

    private static void Apply(TextDocument document, DocumentLine line, ListEditing.LineEdit? edit)
    {
        if (edit is not { } change) return;

        var offset = line.Offset + change.Offset;
        if (change.RemovalLength == 0)
        {
            if (change.InsertText.Length > 0) document.Insert(offset, change.InsertText);
        }
        else if (change.InsertText.Length == 0)
        {
            document.Remove(offset, change.RemovalLength);
        }
        else
        {
            document.Replace(offset, change.RemovalLength, change.InsertText);
        }
    }
}
