// Defines configurable in-window shortcuts and builds the allocation-free key route used by the pad.
using Avalonia.Input;
using AntiNotepad.Model;

namespace AntiNotepad.UI;

internal enum ShortcutScope { Pad, Search }

internal enum ShortcutAction
{
    None, NewNote, NextNote, PreviousNote, NextAlias, PreviousAlias, FirstNote, LatestNote,
    MoveEarlier, MoveLater, DeleteNote, Search, SearchAliasP, SearchAliasK, Settings, Help,
    OpenSlot, KeepSlot, Stack, Split, KeepOnTop, TextSizeUp, TextSizeDown, CopyNote, Ocr, Quit,
    ToggleCheckbox, Fold, Unfold, Grid, Trash,
}

internal readonly record struct ShortcutRoute(ShortcutAction Pad, ShortcutAction Search);
internal readonly record struct ShortcutGesture(Key Key, KeyModifiers Modifiers);

internal sealed record ShortcutDefinition(
    string Id, string Name, string Section, ShortcutScope Scope, ShortcutAction Action,
    string DefaultKey, string Help, bool DigitFamily = false);

internal sealed record ShortcutHelpRow(string Section, string Keys, string Help);

/// <summary>Owns the shortcut table, validates saved overrides, and resolves a key event with one dictionary lookup.</summary>
internal sealed class ShortcutMap
{
    private static readonly ShortcutDefinition[] Table =
    [
        new("new-note", "New note", "Notes", ShortcutScope.Pad, ShortcutAction.NewNote, "Ctrl+N", "Create a note."),
        new("next-note", "Next note", "Notes", ShortcutScope.Pad, ShortcutAction.NextNote, "Alt+Right", "Move to the next note; at the end, create a note."),
        new("previous-note", "Previous note", "Notes", ShortcutScope.Pad, ShortcutAction.PreviousNote, "Alt+Left", "Move to the previous note."),
        new("next-alias", "Next note (alias)", "Notes", ShortcutScope.Pad, ShortcutAction.NextAlias, "", "Another key for the next note."),
        new("previous-alias", "Previous note (alias)", "Notes", ShortcutScope.Pad, ShortcutAction.PreviousAlias, "", "Another key for the previous note."),
        new("first-note", "First note", "Notes", ShortcutScope.Pad, ShortcutAction.FirstNote, "", "Open the oldest note in the stack."),
        new("latest-note", "Latest note", "Notes", ShortcutScope.Pad, ShortcutAction.LatestNote, "", "Open the newest note in the stack."),
        new("move-earlier", "Move note earlier", "Notes", ShortcutScope.Pad, ShortcutAction.MoveEarlier, "", "Move the current stack note one place earlier."),
        new("move-later", "Move note later", "Notes", ShortcutScope.Pad, ShortcutAction.MoveLater, "", "Move the current stack note one place later."),
        new("delete-note", "Delete note", "Notes", ShortcutScope.Pad, ShortcutAction.DeleteNote, "", "Move the current note to the trash."),
        new("search", "Search notes", "Search", ShortcutScope.Pad, ShortcutAction.Search, "Ctrl+F", "Toggle Search."),
        new("search-alias-p", "Search notes (P alias)", "Search", ShortcutScope.Pad, ShortcutAction.SearchAliasP, "", "Another key to toggle Search."),
        new("search-alias-k", "Search notes (K alias)", "Search", ShortcutScope.Pad, ShortcutAction.SearchAliasK, "", "Another key to toggle Search."),
        new("settings", "Settings", "Window", ShortcutScope.Pad, ShortcutAction.Settings, "", "Open Settings."),
        new("help", "Help", "Window", ShortcutScope.Pad, ShortcutAction.Help, "F1", "Show this shortcut list."),
        new("open-slot", "Open slot 1–9", "Slots", ShortcutScope.Pad, ShortcutAction.OpenSlot, "", "Open the numbered slot; use the same key again to return to the stack.", true),
        new("keep-slot", "Keep note in slot 1–9", "Slots", ShortcutScope.Pad, ShortcutAction.KeepSlot, "", "Move the current note into the numbered slot.", true),
        new("stack", "Back to the stack", "Slots", ShortcutScope.Pad, ShortcutAction.Stack, "", "Return from a slot to the note stack."),
        new("split", "Split view", "Slots", ShortcutScope.Pad, ShortcutAction.Split, "Ctrl+\\", "Show a slot beside the current note."),
        new("keep-on-top", "Keep on top", "Window", ShortcutScope.Pad, ShortcutAction.KeepOnTop, "Ctrl+T", "Keep the pad above other windows."),
        new("text-size-up", "Text size up", "Writing", ShortcutScope.Pad, ShortcutAction.TextSizeUp, "Ctrl++", "Increase note text size."),
        new("text-size-down", "Text size down", "Writing", ShortcutScope.Pad, ShortcutAction.TextSizeDown, "Ctrl+-", "Decrease note text size."),
        new("copy-note", "Copy the whole note", "Writing", ShortcutScope.Pad, ShortcutAction.CopyNote, "Ctrl+Shift+C", "Copy all note text."),
        new("ocr", "Read text from a screen snip", "Writing", ShortcutScope.Pad, ShortcutAction.Ocr, "Ctrl+Shift+O", "Read text from a screen snip."),
        new("checkbox-timer", "Checkbox or timer", "Writing", ShortcutScope.Pad, ShortcutAction.ToggleCheckbox, "", "Toggle a checkbox or timer at the caret."),
        new("fold", "Fold at caret", "Writing", ShortcutScope.Pad, ShortcutAction.Fold, "Ctrl+Shift+[", "Fold the block at the caret."),
        new("unfold", "Unfold at caret", "Writing", ShortcutScope.Pad, ShortcutAction.Unfold, "Ctrl+Shift+]", "Unfold the block at the caret."),
        new("quit", "Quit", "Window", ShortcutScope.Pad, ShortcutAction.Quit, "Ctrl+Shift+Q", "Quit AntiNotepad."),
        new("grid", "Grid view (in search)", "Search", ShortcutScope.Search, ShortcutAction.Grid, "", "Switch between list and grid results."),
        new("trash", "Trash (in search)", "Search", ShortcutScope.Search, ShortcutAction.Trash, "", "Open the trash or return to notes."),
    ];

    private static readonly ShortcutHelpRow[] FixedHelp =
    [
        new("Anywhere", "Esc", "Hide the pad or close a panel."),
        new("Writing", "Enter", "Continue a list or accept a completion."),
        new("Writing", "Tab / Shift+Tab", "Indent list items at text start."),
        new("Writing", "Ctrl+[ / Ctrl+]", "Change list level anywhere."),
        new("Writing", "Ctrl+V", "Paste."),
        new("Writing", "Ctrl+Z / Ctrl+Y", "Undo / redo (Ctrl+Z can undo a delete toast)."),
        new("Writing", "Ctrl+Home", "Scroll to the top of the note."),
        new("Writing", "Ctrl+click", "Open a link."),
    ];

    private readonly Dictionary<ShortcutGesture, ShortcutRoute> _routes = [];
    private readonly Dictionary<string, ShortcutGesture> _effective = new(StringComparer.Ordinal);
    private Settings _settings = null!;

    internal static IReadOnlyList<ShortcutDefinition> Definitions => Table;

    /// <summary>Builds the effective shortcuts from defaults and the saved changes.</summary>
    internal ShortcutMap(Settings settings) => Refresh(settings);

    /// <summary>Rebuilds routes after Settings changes; key events only read the completed dictionary.</summary>
    internal void Refresh(Settings settings)
    {
        _settings = settings;
        _routes.Clear();
        _effective.Clear();
        var overrides = settings.ShortcutOverrides;
        var candidates = new Dictionary<string, ShortcutGesture>(StringComparer.Ordinal);
        var cleared = new HashSet<string>(StringComparer.Ordinal);
        var conflicting = new HashSet<string>(StringComparer.Ordinal);
        if (overrides is not null)
        {
            foreach (var definition in Table)
            {
                if (!overrides.TryGetValue(definition.Id, out var value)) continue;
                if (value == "") { cleared.Add(definition.Id); continue; }
                if (value is not null && TryParse(value, out var gesture)
                    && BlockedReason(value, settings, definition) is null)
                    candidates.Add(definition.Id, gesture);
            }

            for (var leftIndex = 0; leftIndex < Table.Length; leftIndex++)
            {
                var left = Table[leftIndex];
                if (!candidates.TryGetValue(left.Id, out var leftGesture)) continue;
                for (var rightIndex = leftIndex + 1; rightIndex < Table.Length; rightIndex++)
                {
                    var right = Table[rightIndex];
                    if (left.Scope == right.Scope && candidates.TryGetValue(right.Id, out var rightGesture)
                        && Overlaps(left, leftGesture, right, rightGesture))
                    {
                        conflicting.Add(left.Id);
                        conflicting.Add(right.Id);
                    }
                }
            }
        }

        foreach (var definition in Table)
            if (candidates.TryGetValue(definition.Id, out var gesture) && !conflicting.Contains(definition.Id))
                _effective.Add(definition.Id, gesture);

        foreach (var definition in Table)
        {
            if (cleared.Contains(definition.Id)
                || candidates.ContainsKey(definition.Id) && !conflicting.Contains(definition.Id)) continue;
            if (definition.DefaultKey.Length > 0 && TryParse(definition.DefaultKey, out var fallback)
                && !HasConflict(definition, fallback)) _effective.Add(definition.Id, fallback);
        }

        foreach (var definition in Table)
        {
            if (!_effective.TryGetValue(definition.Id, out var gesture)) continue;
            if (definition.DigitFamily)
            {
                for (var digit = 1; digit <= 9; digit++)
                {
                    AddRoute(gesture with { Key = (Key)((int)Key.D1 + digit - 1) }, definition);
                    AddRoute(gesture with { Key = (Key)((int)Key.NumPad1 + digit - 1) }, definition);
                }
            }
            else AddRoute(gesture, definition);
        }
    }

    /// <summary>Looks up a normalized key and its exact modifiers without allocating.</summary>
    internal bool TryGet(Key key, KeyModifiers modifiers, out ShortcutRoute route) =>
        _routes.TryGetValue(new ShortcutGesture(Normalize(key), modifiers), out route);

    /// <summary>Returns the effective display label for an action row, including its digit family when applicable.</summary>
    internal string GetLabel(string id)
    {
        var definition = Find(id);
        if (definition is null || !_effective.TryGetValue(id, out var gesture)) return "";
        var text = Format(gesture);
        return definition.DigitFamily ? FamilyLabel(gesture) : text;
    }

    /// <summary>Returns the current recorded value expected by HotkeyRecorder.</summary>
    internal string GetRecordedValue(string id) => GetLabel(id);

    /// <summary>Splits a display label into key caps while preserving a plus key at the end.</summary>
    internal static string[] LabelParts(string label)
    {
        const StringSplitOptions options = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
        if (label.EndsWith("++", StringComparison.Ordinal))
            return label[..^2].Split('+', options).Append("+").ToArray();
        return label.Split('+', options);
    }

    /// <summary>Finds a same-scope action that owns the proposed gesture.</summary>
    internal ShortcutDefinition? FindConflict(string id, string text)
    {
        var definition = Find(id);
        if (definition is null || !TryParse(text, out var gesture)) return null;
        foreach (var other in Table)
            if (other.Id != id && other.Scope == definition.Scope
                && _effective.TryGetValue(other.Id, out var current) && Overlaps(definition, gesture, other, current))
                return other;
        return null;
    }

    /// <summary>Builds the live Help rows from assigned actions and the fixed editor keys.</summary>
    internal static IReadOnlyList<ShortcutHelpRow> HelpRows(ShortcutMap map)
    {
        var rows = new List<ShortcutHelpRow>(Table.Length + FixedHelp.Length + 1)
        {
            new("Anywhere", GlobalHotkeyLabel(map._settings.Hotkey), $"Show / hide {AppInfo.Name} (change in Settings)."),
        };
        foreach (var definition in Table)
            if (map._effective.TryGetValue(definition.Id, out var gesture))
            {
                var keys = Format(gesture);
                if (definition.DigitFamily) keys = FamilyLabel(gesture);
                rows.Add(new ShortcutHelpRow(definition.Section, keys, definition.Help));
            }
        rows.AddRange(FixedHelp);
        return rows;
    }

    /// <summary>Produces the stable table embedded in docs/shortcuts.md and checked by selftest.</summary>
    internal static string DocumentationTable()
    {
        var lines = new List<string> { "| Action | Section | Scope | Default |", "|---|---|---|---|" };
        foreach (var definition in Table)
            lines.Add($"| {definition.Name} | {definition.Section} | {definition.Scope} | {(definition.DefaultKey.Length == 0 ? "Unassigned (bindable)" : definition.DefaultKey)} |");
        return string.Join('\n', lines);
    }

    /// <summary>Returns a concise refusal reason for reserved and text-editing gestures.</summary>
    internal static string? BlockedReason(string text, Settings settings, ShortcutDefinition? definition = null)
    {
        if (!TryParse(text, out var gesture)) return "That key cannot be recorded.";
        var key = Normalize(gesture.Key);
        var modifiers = gesture.Modifiers;
        var ctrl = modifiers.HasFlag(KeyModifiers.Control);
        var alt = modifiers.HasFlag(KeyModifiers.Alt);
        var shift = modifiers.HasFlag(KeyModifiers.Shift);
        var navigation = key is Key.Left or Key.Right or Key.Up or Key.Down or Key.Home or Key.End or Key.PageUp or Key.PageDown;
        var editing = key is Key.Back or Key.Delete or Key.Insert;

        if (definition?.DigitFamily == true && !IsDigit(key)) return "Use a number key from 1 to 9 for this action.";
        if (modifiers == (KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Shift)
            && (IsDigit(key) || key is >= Key.F1 and <= Key.F9 or Key.S)) return "Those keys are reserved for development copies.";
        if (settings.Hotkey is { Length: > 0 } hotkey && TryParse(hotkey, out var global) && gesture == global
            || settings.NewNoteHotkey is { Length: > 0 } newNote && TryParse(newNote, out var newGlobal) && gesture == newGlobal)
            return "That key is assigned to a global hotkey.";
        if (modifiers == KeyModifiers.None && (key is >= Key.A and <= Key.Z or >= Key.D0 and <= Key.D9
            || key is Key.Space or Key.Enter or Key.Tab or Key.Back or Key.Delete or Key.Insert || navigation))
            return "Typing and caret keys belong to the text box.";
        if (modifiers == KeyModifiers.Shift && (key is >= Key.A and <= Key.Z or >= Key.D0 and <= Key.D9
            || key is Key.Space or Key.OemPlus or Key.OemMinus or Key.OemComma or Key.OemPeriod or Key.OemQuestion
                or Key.OemBackslash or Key.OemOpenBrackets or Key.OemCloseBrackets or Key.OemTilde or Key.OemSemicolon or Key.OemQuotes))
            return "Shift plus this key types a letter or symbol.";
        if (!ctrl && !alt && shift && (navigation || key is Key.Back or Key.Insert or Key.Delete)) return "That key selects or edits text.";
        if (ctrl && !alt && navigation && key is not (Key.PageUp or Key.PageDown)) return "That key moves or selects text.";
        if (ctrl && !alt && shift && key is Key.PageUp or Key.PageDown) return "That key extends the text selection.";
        if (ctrl && !alt && !shift && key is Key.Back or Key.Delete) return "That key deletes a word.";
        if (ctrl && !alt && key == Key.Insert) return "Ctrl+Insert copies text.";
        if (alt && shift && !ctrl && navigation) return "That key selects a rectangular block of text.";
        if (ctrl && alt && shift && key is Key.Left or Key.Right) return "That key selects a rectangular block of text.";
        if (ctrl && !alt && !shift && key is Key.A or Key.C or Key.X or Key.V or Key.Z or Key.Y or Key.D or Key.I)
            return "That key is reserved for text editing.";
        if (ctrl && shift && !alt && key == Key.Z) return "That key is reserved for redo.";
        if (key == Key.Escape || key == Key.Tab && modifiers == KeyModifiers.Shift
            || key == Key.Enter && modifiers == KeyModifiers.Shift
            || ctrl && key is Key.V or Key.Z or Key.Y
            || ctrl && key is Key.OemOpenBrackets or Key.OemCloseBrackets && !shift)
            return "That key has a fixed app or editor action.";
        if (alt && !ctrl && key is Key.F4 or Key.Tab or Key.Space or Key.Escape
            || ctrl && !alt && key == Key.Escape
            || ctrl && alt && key == Key.Delete
            || key == Key.PrintScreen)
            return "Windows uses that key.";
        return null;
    }

    /// <summary>Maps supported in-window keys to canonical text, allowing F1 as an unmodified shortcut.</summary>
    internal static string? RecorderGestureText(Key key, KeyModifiers modifiers)
    {
        if (modifiers.HasFlag(KeyModifiers.Meta)) return null;
        var normalized = Normalize(key);
        var names = ModifierText(modifiers);
        var main = KeyText(normalized);
        if (main is null || names.Length == 0 && normalized is not (>= Key.F1 and <= Key.F24)) return null;
        return names.Length == 0 ? main : names + "+" + main;
    }

    /// <summary>Parses the recorder's canonical text into an Avalonia key gesture.</summary>
    internal static bool TryParse(string text, out ShortcutGesture gesture)
    {
        gesture = default;
        if (string.IsNullOrWhiteSpace(text)) return false;
        string prefix;
        string main;
        if (text.EndsWith("++", StringComparison.Ordinal)) { prefix = text[..^2]; main = "+"; }
        else
        {
            var separator = text.LastIndexOf('+');
            prefix = separator < 0 ? "" : text[..separator];
            main = separator < 0 ? text : text[(separator + 1)..];
        }
        var modifiers = KeyModifiers.None;
        if (prefix.Length > 0)
            foreach (var part in prefix.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                var flag = part switch { "Ctrl" => KeyModifiers.Control, "Alt" => KeyModifiers.Alt, "Shift" => KeyModifiers.Shift, _ => KeyModifiers.None };
                if (flag == KeyModifiers.None || modifiers.HasFlag(flag)) return false;
                modifiers |= flag;
            }
        if (!TryKey(main, out var key) || modifiers.HasFlag(KeyModifiers.Meta)) return false;
        gesture = new ShortcutGesture(Normalize(key), modifiers);
        return true;
    }

    /// <summary>Checks whether an effective action already owns an overlapping key in this scope.</summary>
    private bool HasConflict(ShortcutDefinition definition, ShortcutGesture gesture)
    {
        foreach (var other in Table)
            if (other.Id != definition.Id && other.Scope == definition.Scope
                && _effective.TryGetValue(other.Id, out var current) && Overlaps(definition, gesture, other, current))
                return true;
        return false;
    }

    /// <summary>Checks exact key overlap, including digit families and the physical plus-key alias.</summary>
    private static bool Overlaps(ShortcutDefinition left, ShortcutGesture a, ShortcutDefinition right, ShortcutGesture b)
    {
        if (a.Modifiers != b.Modifiers)
        {
            var plusKey = Normalize(a.Key) == Key.OemPlus && Normalize(b.Key) == Key.OemPlus;
            var physicalPlus = plusKey && a.Modifiers.HasFlag(KeyModifiers.Control) && b.Modifiers.HasFlag(KeyModifiers.Control)
                && (a.Modifiers ^ b.Modifiers) == KeyModifiers.Shift;
            if (!physicalPlus) return false;
        }
        if (left.DigitFamily) return right.DigitFamily || IsDigit(b.Key);
        if (right.DigitFamily) return IsDigit(a.Key);
        return Normalize(a.Key) == Normalize(b.Key);
    }

    /// <summary>Adds an action route and its physical Ctrl-plus alias when needed.</summary>
    private void AddRoute(ShortcutGesture gesture, ShortcutDefinition definition)
    {
        AddRouteKey(gesture, definition);
        if (Normalize(gesture.Key) == Key.OemPlus && gesture.Modifiers.HasFlag(KeyModifiers.Control)
            && !gesture.Modifiers.HasFlag(KeyModifiers.Shift))
            AddRouteKey(gesture with { Modifiers = gesture.Modifiers | KeyModifiers.Shift }, definition);
    }

    /// <summary>Adds one gesture to the route table for its action scope.</summary>
    private void AddRouteKey(ShortcutGesture gesture, ShortcutDefinition definition)
    {
        var key = new ShortcutGesture(Normalize(gesture.Key), gesture.Modifiers);
        _routes.TryGetValue(key, out var route);
        route = definition.Scope == ShortcutScope.Pad
            ? route with { Pad = definition.Action }
            : route with { Search = definition.Action };
        _routes[key] = route;
    }

    private static ShortcutDefinition? Find(string id) => Table.FirstOrDefault(definition => definition.Id == id);

    private static bool IsDigit(Key key) => key is >= Key.D0 and <= Key.D9 or >= Key.NumPad0 and <= Key.NumPad9;

    private static Key Normalize(Key key) => key switch
    {
        Key.Add => Key.OemPlus,
        Key.Subtract => Key.OemMinus,
        Key.OemPipe => Key.OemBackslash,
        _ => key,
    };

    private static string GlobalHotkeyLabel(string? gesture) => string.IsNullOrWhiteSpace(gesture) ? "Ctrl+Alt+Shift+S" : gesture;

    private static string FamilyLabel(ShortcutGesture gesture)
    {
        var modifiers = ModifierText(gesture.Modifiers);
        return modifiers.Length == 0 ? "1 … 9" : modifiers + "+1 … 9";
    }

    private static string ModifierText(KeyModifiers modifiers)
    {
        var text = "";
        if (modifiers.HasFlag(KeyModifiers.Control)) text = "Ctrl";
        if (modifiers.HasFlag(KeyModifiers.Alt)) text += text.Length == 0 ? "Alt" : "+Alt";
        if (modifiers.HasFlag(KeyModifiers.Shift)) text += text.Length == 0 ? "Shift" : "+Shift";
        return text;
    }

    private static string Format(ShortcutGesture gesture)
    {
        var key = KeyText(Normalize(gesture.Key)) ?? gesture.Key.ToString();
        var modifiers = ModifierText(key == "+" ? gesture.Modifiers & ~KeyModifiers.Shift : gesture.Modifiers);
        return modifiers.Length == 0 ? key : modifiers + "+" + key;
    }

    private static string? KeyText(Key key) => key switch
    {
        >= Key.A and <= Key.Z => key.ToString(),
        >= Key.D0 and <= Key.D9 => ((int)key - (int)Key.D0).ToString(System.Globalization.CultureInfo.InvariantCulture),
        >= Key.F1 and <= Key.F24 => key.ToString(),
        Key.Left => "Left",
        Key.Right => "Right",
        Key.Up => "Up",
        Key.Down => "Down",
        Key.Home => "Home",
        Key.End => "End",
        Key.PageUp => "PageUp",
        Key.PageDown => "PageDown",
        Key.OemPlus => "+",
        Key.OemMinus => "-",
        Key.OemComma => ",",
        Key.OemPeriod => ".",
        Key.OemQuestion => "/",
        Key.OemBackslash => "\\",
        Key.OemOpenBrackets => "[",
        Key.OemCloseBrackets => "]",
        Key.OemTilde => "~",
        Key.OemSemicolon => ";",
        Key.OemQuotes => "'",
        Key.Space => "Space",
        Key.Escape => "Esc",
        Key.Tab => "Tab",
        Key.Enter => "Enter",
        Key.Back => "Backspace",
        Key.Delete => "Delete",
        Key.Insert => "Insert",
        Key.PrintScreen => "PrintScreen",
        _ => null,
    };

    private static bool TryKey(string text, out Key key)
    {
        key = default;
        if (text.Length == 1 && text[0] is >= 'A' and <= 'Z') { key = (Key)((int)Key.A + text[0] - 'A'); return true; }
        if (text.Length == 1 && text[0] is >= '0' and <= '9') { key = (Key)((int)Key.D0 + text[0] - '0'); return true; }
        if (text.Length is >= 2 and <= 3 && text[0] == 'F' && int.TryParse(text.AsSpan(1), out var function) && function is >= 1 and <= 24)
        { key = (Key)((int)Key.F1 + function - 1); return true; }
        key = text switch
        {
            "+" => Key.OemPlus,
            "-" => Key.OemMinus,
            "," => Key.OemComma,
            "." => Key.OemPeriod,
            "/" => Key.OemQuestion,
            "\\" => Key.OemBackslash,
            "[" => Key.OemOpenBrackets,
            "]" => Key.OemCloseBrackets,
            "~" => Key.OemTilde,
            ";" => Key.OemSemicolon,
            "'" => Key.OemQuotes,
            "Space" => Key.Space,
            "Esc" => Key.Escape,
            "Tab" => Key.Tab,
            "Enter" => Key.Enter,
            "Backspace" => Key.Back,
            "Delete" => Key.Delete,
            "Insert" => Key.Insert,
            "Left" => Key.Left,
            "Right" => Key.Right,
            "Up" => Key.Up,
            "Down" => Key.Down,
            "Home" => Key.Home,
            "End" => Key.End,
            "PageUp" => Key.PageUp,
            "PageDown" => Key.PageDown,
            "PrintScreen" => Key.PrintScreen,
            _ => default,
        };
        return KeyText(key) is not null && KeyText(key) == text;
    }
}
