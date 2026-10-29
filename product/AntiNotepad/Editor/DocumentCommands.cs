using AvaloniaEdit.Document;

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
