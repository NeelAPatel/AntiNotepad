namespace AntiNotepad.Editor;

/// <summary>One right-click menu line. A null header is a separator; a null icon leaves the icon column empty.</summary>
internal sealed record MenuEntry(string? Header, string? Keys = null, string? Icon = null, Action? Click = null, bool Enabled = true)
{
    public static readonly MenuEntry Separator = new((string?)null);
}

/// <summary>Segoe Fluent Icons used by the pad and editor menus.</summary>
internal static class MenuGlyphs
{
    public const string Undo = "", Redo = "", Cut = "", Copy = "", Paste = "", SelectAll = "\uE8B3",
        Delete = "", NewNote = "", Search = "", Split = "", Pin = "", Settings = "";
}
