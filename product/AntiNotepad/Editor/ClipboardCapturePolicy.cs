namespace AntiNotepad.Editor;

internal sealed class ClipboardCapturePolicy
{
    internal enum TextCheck
    {
        Blank,
        OwnCopy,
        LastCaptured,
        Eligible
    }

    private string? _ownClipboard;
    private string? _lastCaptured;

    internal void RecordOwnCopy(string text) => _ownClipboard = text;

    internal TextCheck CheckText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return TextCheck.Blank;
        if (text == _ownClipboard) return TextCheck.OwnCopy;
        if (text == _lastCaptured) return TextCheck.LastCaptured;
        return TextCheck.Eligible;
    }

    internal bool Decide(string text, string? existingText)
    {
        if (existingText?.Contains(text.Trim()) == true) return false;
        _lastCaptured = text;
        return true;
    }

    internal static string FormatAppend(string text, bool endsWithLf) =>
        (endsWithLf ? "" : "\n") + "- " + text.Trim().Replace("\r\n", "\n").Replace("\n", "\n  ");
}
