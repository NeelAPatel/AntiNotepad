using System.Globalization;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AntiNotepad.Model;

using AntiNotepad.Theme;

namespace AntiNotepad.Editor;

/// <summary>Caret-side completion: mode keywords on line 1, and `/commands` anywhere (AntiNote's extensions, built in).</summary>
internal static class Completion
{
    private static readonly JsonSerializerOptions PrettyJsonOptions = new() { WriteIndented = true };

    public sealed record Command(string Name, string Summary, Func<PadView, string?, string?> Run);

    public static event Action<PadView>? OcrRequested;

    public static readonly Command[] Commands =
    [
        new("date", "Insert today's date", (_, _) => DateTime.Now.ToString("MMM d, yyyy", CultureInfo.CurrentCulture)),
        new("time", "Insert the time", (_, _) => DateTime.Now.ToString("t", CultureInfo.CurrentCulture)),
        new("now", "Insert date and time", (_, _) => DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)),
        new("week", "Insert this week's days as a list", (_, _) => string.Join("\n", Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(i - ((int)DateTime.Today.DayOfWeek + 6) % 7)).Select(d => $"## {d:dddd d MMM}"))),
        new("todo", "Insert a checkbox", (_, _) => "[ ] "),
        new("uuid", "Insert a new UUID", (_, _) => Guid.NewGuid().ToString()),
        new("sort", "Sort the note's lines (keeps line 1 if it's a mode)", (pad, _) => { Transform(pad, TextCommands.Sort); return null; }),
        new("dedupe", "Remove duplicate lines", (pad, _) => { Transform(pad, TextCommands.Dedupe, keepBlank: true); return null; }),
        new("reverse", "Reverse line order", (pad, _) => { Transform(pad, ls => ls.Reverse(), keepBlank: true); return null; }),
        new("checklist", "Make every line a checkbox", (pad, _) => { Transform(pad, ls => TextCommands.AsList(ls, "check"), keepBlank: true); return null; }),
        new("bullets", "Make every line a bullet", (pad, _) => { Transform(pad, ls => TextCommands.AsList(ls, "bullet"), keepBlank: true); return null; }),
        new("numbered", "Number every line 1. 2. 3.", (pad, _) => { Transform(pad, ls => TextCommands.AsList(ls, "number"), keepBlank: true); return null; }),
        new("plain", "Remove list markers and checkboxes", (pad, _) => { Transform(pad, ls => TextCommands.AsList(ls, "plain"), keepBlank: true); return null; }),
        new("uncheck", "Untick every checkbox", (pad, _) => { Transform(pad, TextCommands.Uncheck, keepBlank: true); return null; }),
        new("tidy", "Trim trailing spaces and extra blank lines", (pad, _) => { Transform(pad, TextCommands.Tidy, keepBlank: true); return null; }),
        new("upper", "UPPERCASE the current line", (pad, _) => { Line(pad, s => s.ToUpper(CultureInfo.CurrentCulture)); return null; }),
        new("lower", "lowercase the current line", (pad, _) => { Line(pad, s => s.ToLower(CultureInfo.CurrentCulture)); return null; }),
        new("title", "Title Case the current line", (pad, _) => { Line(pad, s => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower(CultureInfo.CurrentCulture))); return null; }),
        new("json", "Pretty-print the note as JSON", (pad, _) => { PrettyJson(pad); return null; }),
        new("count", "Count words, lines and characters", (pad, _) => { Count(pad); return null; }),
        new("clear", "Clear done checklist items", (pad, _) => { Transform(pad, TextCommands.ClearDone, keepBlank: true); return null; }),
        new("ocr", "Read text from a screen snip", (pad, _) => { OcrRequested?.Invoke(pad); return null; }),
        new("showcase", "Insert the feature showcase page", (_, _) => Showcase.Text),
        new("lorem", "Insert placeholder text", (_, _) => "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."),
    ];

    private static CompletionWindow? _open;
    public static bool IsOpen => _open is not null;

    public static void OnTextEntered(PadView pad, string? typed)
    {
        if (string.IsNullOrEmpty(typed) || _open is not null) return;
        var area = pad.Editor.TextArea;
        var doc = pad.Editor.Document;
        var line = doc.GetLineByOffset(area.Caret.Offset);
        var before = doc.GetText(line.Offset, area.Caret.Offset - line.Offset);
        if (typed == "/" && (before.Length == 1 || char.IsWhiteSpace(before[^2])) && pad.Mode is not (NoteMode.Code or NoteMode.Math))
        {
            Show(pad, area.Caret.Offset - 1, Commands.Select(c => (ICompletionData)new Item("/" + c.Name, c.Summary, pad, c)));
            return;
        }
        // A code fence: offer the enabled languages; picking one closes the fence and puts the caret inside it.
        if (typed == "`" && before.TrimStart() == "```" && line.EndOffset == area.Caret.Offset && pad.Mode != NoteMode.Code
            && pad.Structure?.Fences.Any(f => f.Open == line.LineNumber) == true)   // opening fences only, never a closing one
        {
            var langs = CodeColors.Languages.Where(l => !CodeColors.Disabled.Contains(l.Id))
                .Select(l => (ICompletionData)new FenceItem(l.Aliases[0], l.Name, pad)).ToList();
            if (langs.Count > 0) Show(pad, area.Caret.Offset, langs);
            return;
        }
        if (line.LineNumber == 1 && before.Trim().Length == 1 && line.EndOffset == area.Caret.Offset && doc.LineCount == 1 && char.IsLetter(typed[0]))
        {
            var items = Modes.Keywords.Where(k => k.Word.StartsWith(before.Trim(), StringComparison.OrdinalIgnoreCase))
                .Select(k => (ICompletionData)new Item(k.Word, k.Word == "list" ? $"Your default: {ListDefaultName(pad.Settings.ListDefault)}" : k.Summary, pad, null, k.Mode == NoteMode.Code ? "code py" : k.Word)).ToList();
            if (items.Count > 0) Show(pad, line.Offset, items);
        }
    }

    /// <summary>Names the configured list style in the keyword popup.</summary>
    internal static string ListDefaultName(string kind) => kind switch
    {
        "check" => "checkboxes",
        "number" => "numbers",
        _ => "bullets"
    };

    private static void Show(PadView pad, int start, IEnumerable<ICompletionData> items)
    {
        var p = pad.Palette;
        var window = new CompletionWindow(pad.Editor.TextArea)
        {
            StartOffset = start, CloseWhenCaretAtBeginning = true, CloseAutomatically = true,

            MinWidth = 300, Width = 340,
        };
        window.CompletionList.Foreground = new SolidColorBrush(p.Ink);
        window.CompletionList.Background = new SolidColorBrush(p.Surface);
        foreach (var item in items) window.CompletionList.CompletionData.Add(item);
        window.Closed += (_, _) => _open = null;
        _open = window;
        window.Show();
    }

    /// <summary>A fence language: inserts it, the body line and the closing fence.</summary>
    private sealed class FenceItem(string word, string name, PadView pad) : ICompletionData
    {
        public Avalonia.Media.IImage? Image => null;
        public string Text => word;
        public object Content => new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10,
            Children =
            {
                new TextBlock { Text = word, FontWeight = Typography.Strong, Foreground = new SolidColorBrush(pad.Palette.Keyword), MinWidth = 70 },
                new TextBlock { Text = name, Foreground = new SolidColorBrush(pad.Palette.Muted), FontSize = Typography.Small, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center },
            }
        };
        public object Description => name;
        public double Priority => 0;

        public void Complete(TextArea area, ISegment segment, EventArgs args)
        {
            // Capture before replacing: the completion segment moves with the edit.
            var start = segment.Offset;
            var line = area.Document.GetLineByOffset(start);
            var indent = new string(' ', area.Document.GetText(line).Length - area.Document.GetText(line).TrimStart().Length);
            area.Document.Replace(segment, word + "\n" + indent + "\n" + indent + "```");
            area.Caret.Offset = Math.Min(area.Document.TextLength, start + word.Length + 1 + indent.Length);
        }
    }

    private sealed class Item(string text, string summary, PadView pad, Command? command, string? insert = null) : ICompletionData
    {
        public Avalonia.Media.IImage? Image => null;
        public string Text => text;
        public object Content => new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 10,
            Children =
            {
                new TextBlock { Text = text, FontWeight = Typography.Strong, Foreground = new SolidColorBrush(pad.Palette.Keyword), MinWidth = 86 },
                new TextBlock { Text = summary, Foreground = new SolidColorBrush(pad.Palette.Muted), FontSize = Typography.Small, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center },
            }
        };
        public object Description => summary;
        public double Priority => 0;

        public void Complete(TextArea area, ISegment segment, EventArgs args)
        {
            if (command is null)
            {
                // Enter on a mode keyword means "that's my line 1": accept it and start line 2.
                var newline = args is Avalonia.Input.KeyEventArgs { Key: Avalonia.Input.Key.Enter } ? "\n" : "";
                area.Document.Replace(segment, (insert ?? text) + newline);
                if (newline.Length > 0 && Modes.Detect(text).Mode == NoteMode.List)
                    area.Document.Insert(area.Caret.Offset, ListEditing.Marker(ListEditing.KindOf(Modes.ListKindOf(text, pad.Settings.ListDefault))));
                return;
            }
            area.Document.Replace(segment, "");
            var output = command.Run(pad, null);
            if (output is not null) area.Document.Insert(area.Caret.Offset, output);
        }
    }

    // Line-transform commands leave a mode keyword on line 1 in place.
    private static void Transform(PadView pad, Func<IEnumerable<string>, IEnumerable<string>> f, bool keepBlank = false)
    {
        var doc = pad.Editor.Document;
        var result = TextCommands.Apply(doc.Text, pad.ModeKeywordLength > 0, f, keepBlank);
        // One replace = one undo step; Ctrl+Z restores the note as it was.
        if (result != doc.Text) doc.Replace(0, doc.TextLength, result);
    }

    private static void Line(PadView pad, Func<string, string> f)
    {
        var doc = pad.Editor.Document;
        var line = doc.GetLineByOffset(pad.Editor.CaretOffset);
        var caret = pad.Editor.CaretOffset;
        doc.Replace(line.Offset, line.Length, f(doc.GetText(line)));
        pad.Editor.CaretOffset = Math.Min(caret, doc.TextLength);
    }

    private static void PrettyJson(PadView pad)
    {
        try
        {
            using var json = JsonDocument.Parse(pad.Editor.Document.Text.Trim());
            pad.Editor.Document.Text = JsonSerializer.Serialize(json.RootElement, PrettyJsonOptions);
        }
        catch (JsonException ex) { pad.RaiseToast("Not valid JSON · " + ex.Message.Split('.')[0]); }
    }

    private static void Count(PadView pad)
    {
        var text = pad.Editor.Document.Text;
        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        pad.RaiseToast($"{words} words · {pad.Editor.Document.LineCount} lines · {text.Length} characters");
    }
}
