// Aligns wrapped continuation lines with the text after list markers or leading whitespace.
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace AntiNotepad.Editor;

/// <summary>Connects note line prefixes to AvaloniaEdit's visual wrap indentation.</summary>
internal static class WrapIndent
{
    /// <summary>Installs the note's text-column lookup on its text view.</summary>
    internal static void Configure(TextView view)
    {
        view.WrapIndentationProvider = GetTextOffset;
    }

    /// <summary>Returns the source offset where visible text begins on a document line.</summary>
    internal static int GetTextOffset(VisualLine visualLine)
    {
        var text = visualLine.Document.GetText(visualLine.FirstDocumentLine);
        return TextOffset(text);
    }

    /// <summary>Finds the first text column after a list marker or leading whitespace.</summary>
    internal static int TextOffset(string text)
    {
        var prefix = ListEditing.ListPrefix.Match(text);
        if (prefix.Success && (!prefix.Groups[3].Success || ListEditing.TryListNumber(prefix, out _)))
            return prefix.Length;

        return text.Length - text.TrimStart().Length;
    }
}
