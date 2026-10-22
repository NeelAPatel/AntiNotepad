namespace Antipad.Model;

public enum NoteMode { Plain, Math, Code, List, Timer, Clipboard }

/// <summary>A first-line keyword that switches a note into a mode (AntiNote's "contextual note modes").</summary>
public sealed record ModeKeyword(string Word, NoteMode Mode, string Summary, string Example);

public static class Modes
{
    public static readonly ModeKeyword[] Keywords =
    [
        new("math", NoteMode.Math, "Live calculator: variables, units, %, totals", "rent = 1200\nrent / 3"),
        new("list", NoteMode.List, "Checklist: every new line gets a checkbox", "[ ] milk"),
        new("todo", NoteMode.List, "Same as list", "[ ] call back"),
        new("timer", NoteMode.Timer, "Countdowns: one per line, Enter starts", "25m focus"),
        new("code", NoteMode.Code, "Syntax colouring: code py, code js, code cs …", "code py"),
        new("clipboard", NoteMode.Clipboard, "Collects everything you copy while it exists", "(copy something)"),
    ];

    public static readonly string[] CodeLanguages = ["py", "js", "ts", "cs", "json", "html", "xml", "css", "sql", "ps1", "sh", "java", "cpp", "c", "rs", "go", "md"];

    /// <summary>Mode of a note from its first line, plus the keyword's length and an optional argument (code language).</summary>
    public static (NoteMode Mode, int KeywordLength, string? Argument) Detect(string text)
    {
        var end = text.IndexOf('\n');
        var first = (end < 0 ? text : text[..end]).TrimEnd('\r');
        var trimmed = first.Trim().TrimEnd(':');
        if (trimmed.Length == 0) return (NoteMode.Plain, 0, null);
        var lead = first.Length - first.TrimStart().Length;
        foreach (var k in Keywords)
        {
            if (string.Equals(trimmed, k.Word, StringComparison.OrdinalIgnoreCase))
                return (k.Mode, lead + k.Word.Length, null);
        }
        // code: py / code py / ```py
        var parts = trimmed.Split([' ', ':'], 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 2 && parts[0].Equals("code", StringComparison.OrdinalIgnoreCase) && !parts[1].Contains(' '))
            return (NoteMode.Code, first.TrimEnd().Length, parts[1].ToLowerInvariant());
        return (NoteMode.Plain, 0, null);
    }

    public static string Label(NoteMode mode, string? argument) => mode switch
    {
        NoteMode.Math => "math",
        NoteMode.Code => argument is null ? "code" : $"code · {argument}",
        NoteMode.List => "list",
        NoteMode.Timer => "timer",
        NoteMode.Clipboard => "clipboard",
        _ => ""
    };
}
