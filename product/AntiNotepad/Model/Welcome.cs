using AntiNotepad;

namespace AntiNotepad.Model;

/// <summary>First-run demo notes in the permanent slots.</summary>
internal static class Welcome
{
    public static void SeedIfNew(StoreData data, SettingsService settings)
    {
        if (data.Settings.Welcomed) return;
        settings.Update(current => current.Welcomed = true);

        var notes = new[] { Markdown, Math, List, Timers, Guide };
        for (var i = 0; i < notes.Length; i++)
            if (data.Slots[i].IsEmpty) data.Slots[i].Text = notes[i].ReplaceLineEndings("\n");  // a CRLF checkout puts \r into the raw strings

        data.ViewSlot = 5;
    }

    private const string Markdown = """"
        # h1
        ## h2
        ### h3
        #### h4
        ##### h5
        ###### h6


        **bold**
        *italic*
        ~~strikethrough~~
        __underline__
        `single quote`
        __*~~**all at once**~~*__

        divider
        ---

        lists:
        - text
          - text
            - text
        - text

        numbered lists:
        1. text
          2. text
            3. text
        4. text

        ```py
        def process_numbers(numbers: list[int]) -> list[int]:
            results: list[int] = []
            # This is a comment
            for number in numbers:
                if number % 2 == 0:
                    results.append(number * 2)
                else:
                    results.append(number)

            return results


        numbers: list[int] = [1, 2, 3, 4, 5]
        output: list[int] = process_numbers(numbers)

        print("Processed numbers:", output)
        ```
        """";

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

    private const string Timers = """
        timer
        1m tea
        30s focus
        1h30m laundry
        5:00 eggs
        2min stretch
        """;

    private static readonly string Guide = $$"""
        # Welcome to {{AppInfo.Name}}
        A quick guide to your hotkey scratchpad.
        - Press `Ctrl+Alt+Shift+S` to show or hide it; press `Esc` to hide.
        - It saves as you type. Notes live in Settings > Storage.

        ## Notes and slots
        - The stack holds temporary notes; swipe or use `Ctrl+[` / `Ctrl+]` to flip through it.
        - Slots 1–4 hold demos: 1 Markdown, 2 math, 3 checklist, 4 timers.
        - `Ctrl+1`–`Ctrl+9` opens a permanent slot. `Ctrl+\` opens a slot beside your note in split view.

        ## Modes and tools
        - Set line 1 to `math`, `list`, `timer`, `code py`, or `clipboard`.
        - Type `/` for commands such as `/date`, `/todo`, `/sort`, or `/json`.
        - `Ctrl+F` searches your notes.
        - [ ] Click the box to check off this example.

        ## Help
        - `Ctrl+,` opens Settings. `F1` shows all shortcuts.
        - To clear a demo, open its slot and click **Clear slot** at the bottom; the note moves to Trash.
        """;
}
