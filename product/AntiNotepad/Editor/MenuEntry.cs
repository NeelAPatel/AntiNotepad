namespace AntiNotepad.Editor;

/// <summary>One right-click menu line. A null header is a separator; a null icon leaves the icon column empty.</summary>
internal sealed record MenuEntry(string? Header, string? Keys = null, Icon? Icon = null, Action? Click = null, bool Enabled = true, IReadOnlyList<MenuEntry>? Children = null)
{
    public static readonly MenuEntry Separator = new((string?)null);
}
