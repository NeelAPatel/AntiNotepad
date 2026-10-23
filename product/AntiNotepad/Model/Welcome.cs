using AntiNotepad;

namespace AntiNotepad.Model;

/// <summary>First-run notes that double as a tour. Swipe through them, then delete them.</summary>
internal static class Welcome
{
    public static void SeedIfNew(StoreData data)
    {
        if (data.Settings.Welcomed) return;
        data.Settings.Welcomed = true;
        if (data.Stack.Any(n => !n.IsEmpty)) return;
        data.Stack.Clear();
        data.Stack.Add(new Note { Text = Math });
        data.Stack.Add(new Note { Text = List });
        data.Stack.Add(new Note { Text = Tour });
        data.Current = data.Stack.Count - 1;
    }

    private static readonly string Tour = $$"""
        # Welcome to {{AppInfo.Name}}
        A scratchpad that is there when you press **Ctrl+Alt+Shift+S** and gone when you press it again (or Esc).

        Just type. Everything saves as you go.

        - Swipe sideways (or Ctrl+[ and Ctrl+]) to flip between notes
        - Going past the last note starts a fresh one; empty notes tidy themselves away
        - Ctrl+F searches every note · Ctrl+1…9 opens nine permanent slots
        - Ctrl+\ puts a slot beside the note you're writing

        Little things that just work:
        [ ] type [ ] for a checkbox, Ctrl+Enter ticks it
        [x] long links shrink: https://learn.microsoft.com/en-us/windows/apps/design/style/acrylic
        - 19.99 * 3 =
        - type / for commands like /date or /sort

        Line 1 can switch a note's mode: math, list, timer, code py, clipboard.
        Press F1 for every shortcut. Delete this note with Ctrl+Shift+Backspace.
        """;

    private const string Math = """
        math
        rent = 1450
        utilities = 180
        internet = 65
        total
        total / 3
        // units and percentages
        5 km in mi
        72 F in C
        2.5 GB in MB
        180 + 15%
        20% of rent
        // dates
        days until dec 25
        today + 30 days
        """;

    private static readonly string List = $$"""
        list
        [x] open {{AppInfo.Name}} with the hotkey
        [ ] flip to the next note
        [ ] try a timer note: line 1 "timer", then "10m tea"
        [ ] pick a palette in Settings (Ctrl+,)
        """;
}
