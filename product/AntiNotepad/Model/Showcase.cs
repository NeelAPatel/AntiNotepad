using AntiNotepad;

namespace AntiNotepad.Model;

/// <summary>
/// One note that exercises as many editor features as possible: the Settings preview text, the dev copy's first note
/// (a fixed page to screenshot after every change, so visual regressions show), and /showcase for anyone curious.
/// Line-1 modes (math units, timers, code notes) need notes of their own; the last section points to them.
/// </summary>
internal static class Showcase
{
    public static readonly string Title = $"{AppInfo.Name} showcase";

    /// <summary>Dev runs with the AppInfo-derived data override always start with the showcase as their first note.</summary>
    public static void EnsureInDev(StoreData data, bool isDev)
    {
        if (!isDev || data.Stack.Any(n => n.TitleCache == Title)) return;
        data.Stack.Insert(0, new Note { Text = Text, TitleCache = Title });
        data.Current = Math.Min(data.Current + 1, data.Stack.Count - 1);
    }

    public static readonly string Text = $$"""
        # {{AppInfo.Name}} showcase
        Every editor feature on one page. Scroll it, fold it, resize the window.

        ## Text
        Plain text, **bold**, *italic*, ~~struck out~~ and `inline code`.
        > A quoted line, for things someone else said.
        A long link shrinks to its host: https://learn.microsoft.com/en-us/windows/apps/design/style/acrylic

        ## Checklist
        [ ] an open task
        [x] a finished task
        [ ] Ctrl+Enter ticks the line you're on

        ## Lists
        - groceries
          - lemons, garlic, parmesan
          - the dark-roast coffee
            - whole beans, not ground
        - errands
        1. book the car service
        2. return the library books
        3. renew the passport

        ## Answers
        19.99 * 3 =
        1450 / 3 =
        (12 + 30) * 1.15 =
        2.5 * 4 + 1 =

        ## Code
        ```py
        def split(total, people):
            return round(total / people, 2)
        ```
        ```js
        const total = items.reduce((a, b) => a + b, 0);
        ```
        ```json
        { "theme": "dark", "fontSize": 15, "pinned": false }
        ```
        ```cs
        var notes = store.Stack.Where(n => !n.IsEmpty).ToList();
        ```
        ```sql
        SELECT title, modified FROM notes WHERE deleted IS NULL;
        ```
        ```ps1
        Get-ChildItem -Recurse -Filter *.md | Measure-Object
        ```

        ## Headings fold
        ### A third-level heading
        Hover the left edge for chevrons, or Ctrl+Shift+[ to fold where the caret is.
        ### Another one
        Indented blocks and code blocks fold too.

        ## Long tail
        Lines below are here so there is always something to scroll.
        - Monday: standup, review the scroll picks
        - Tuesday: settings window mockup
        - Wednesday: showcase screenshots
        - Thursday: memory check after a long session
        - Friday: tidy the trash, back up the notes folder
        - Saturday: farmers market, bike shop
        - Sunday: nothing at all
        The quick brown fox jumps over the lazy dog, again and again, until this line wraps in a narrow pad.
        A second long line to wrap: {{AppInfo.Name}} keeps every note in its own file, so a crash can't take the others with it.

        ## Other note types
        Line 1 decides a note's type. Try a new note (Ctrl+N) starting with:
        - math: variables, units, percentages, dates
        - list: every new line gets a checkbox
        - timer: 25m focus, Enter starts it
        - code py: the whole note is code
        - clipboard: collects everything you copy
        end of the showcase
        """;
}
