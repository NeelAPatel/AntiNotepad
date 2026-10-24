# A19-demo-slot-notes

You are a worker (gpt-6-luna xhigh). Repo: this folder, a git worktree on branch `feat/<this task's ID-slug>`. Never switch
branch, merge or commit (.git is read-only for you). App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10).
Read first: `AGENTS.md`, `docs/dev-loop.md`, and in `docs/audit/A1-codebase-audit.md` the finding(s) named in the task.
Match the surrounding code style, naming and comment density. First-party .NET/Avalonia only, no new packages.
Other workers edit other files in parallel: stay inside "Files in scope". Add selftest checks as ONE new method at the END of
`SelfTest.cs` with one call line in `Run` (other workers do the same; keep it easy to merge).

## Standard "Done means" (unless the task says otherwise)
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/<id>-check`: 0 errors, 0 warnings.
  `build/<id>-check/AntiNotepad.exe --selftest` prints SELFTEST PASS. Delete `build/<id>-check` if you can.
- Never launch AntiNotepad any other way, never kill it, never send it `--quit`, and never read or write
  `%LOCALAPPDATA%\AntiNotepad`, `%LOCALAPPDATA%\Programs\AntiNotepad` or the real Documents folder.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/<ID-slug>.md`: files changed, what was wrong, the fix, the selftest checks added, anything a person
  must check by eye or by hand (the manager cannot do live UI tests this round; mark those "needs a live check"), and anything
  not done.


Two separate changes (the manager commits them separately, so keep them in separate files as listed).

## Change 1: feel defaults (`Model/Store.cs`, `Settings` class only)
The user picked these defaults for a fresh install:
- Scrolling: `Scrolling = "rate"` (Rate-matched). Distance per click Normal (`ScrollDistance = 1`), Speed boost Normal
  (`ScrollBoost = 1`): already the defaults.
- Switching between notes: `Swipe = "progress"`. Distance to switch Normal (`SwipeLength = 220`), Changes at "A third"
  (`SwipeFiresAt = 0.35`): already the defaults.
So only `Scrolling` and `Swipe` change. Grep `SelfTest.cs`, `docs/decisions.md` and `docs/` for anything that asserts or states the
old defaults (Lines, Detent) and update it. Existing users keep their saved values (they are stored in index.json); that is fine.

## Change 2: five demo notes in slots 1-5 (`Model/Welcome.cs`)
Replace the first-run seeding: today `Welcome.SeedIfNew` puts Math, List and Tour notes on the stack. Instead, on a first run
(same `Settings.Welcomed` gate; keep the rule that it only runs when the store is available, which `Program.cs` already enforces;
do not edit Program.cs), fill **slot N with note N** below, only where that slot is empty. Leave the stack as it is (no seeded
stack notes, so nothing duplicates). The first run opens on the welcome note (slot 5: set `ViewSlot` inside `SeedIfNew`); if that
doesn't work cleanly with how slots are shown, pick the cleanest alternative and say why in the report.
Keep the note texts as readable source constants in `Welcome.cs` so the team can keep editing them ("slotted so we can continue to
work on it"). Use `{{AppInfo.Name}}` interpolation where the app name appears, as today.
Check `UI/MotionPreview.cs` and `Model/Showcase.cs` only for references to the removed constants; don't change their own samples.

### Note 1 (verbatim; it is a markdown demo; keep every line, blank lines included)
````
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
````
(Trailing spaces after "lists:" and "numbered lists:" may be dropped.) A C# raw string with enough quotes (`""""`) holds the code
fence. Report how the app renders `divider` + `---` (rule vs anything else), `__*~~**all at once**~~*__`, and the odd numbered-list
indents, by reading the colorizer code; do not change editor rendering (design changes go lab-first).

### Note 2 (math; the current `Math` constant, unchanged)
### Note 3 (list; the current `List` constant, unchanged)

### Note 4 (timer)
Line 1 `timer`, then five example timers that the parser in `Model/Timers.cs` accepts, showing different forms (minutes, seconds,
hours+minutes without spaces, a clock form like `5:00`, a word unit like `min`), each with a short label (e.g. tea, focus,
stretch, laundry, eggs). Read how timers start: if a seeded timer line would start counting or ring on a fresh install without the
user doing anything, say so in the report and pick durations/forms so a first run doesn't ring five alarms at once (prefer timers
that start only when the user starts them, if that's how the app works).

### Note 5 (welcome)
A new standardized "Welcome to {{AppInfo.Name}}" guide, the note the first run opens on. Plain, short, scannable, US spelling.
Cover, using only features and shortcuts that really exist (verify each one in the code: hotkeys in `Platform/`, shortcuts in
`UI/PadWindow*.cs` and the F1 sheet, slash commands in `Editor/Completion.cs`, modes in `Model/Modes.cs`):
- what it is (hotkey to show/hide, Esc, saves as you type, where notes live: Settings > Storage);
- notes vs slots: the stack (flip with swipe / Ctrl+[ and Ctrl+]), Ctrl+1..9 slots, and that slots 1-4 hold demos: 1 markdown,
  2 math, 3 checklist, 4 timers;
- line-1 modes (math, list, timer, code, clipboard), slash commands (a few examples), Ctrl+F search, split view (Ctrl+\);
- Settings (Ctrl+,), F1 for all shortcuts, how to clear a demo note.
Use the app's own markdown (headings, lists, `[ ]` checkboxes) so the note itself looks good. Keep it under ~35 lines.

## Files in scope
`Model/Welcome.cs`, `Model/Store.cs` (Settings defaults only; another worker edits the search code in this file), `SelfTest.cs`,
`docs/decisions.md` (default wording only).

## Selftest (one method at the end)
- a fresh StoreData seeded by `SeedIfNew` has slots 1-5 non-empty with the expected first lines (`# h1`, `math`, `list`, `timer`,
  the welcome heading), slots 6-9 empty, the stack has no seeded notes, and the view opens on slot 5;
- a slot that already has text is not overwritten; a second call does nothing;
- every line 2..6 of note 4 parses with `Timers.TryParse`;
- new `Settings()` defaults: Scrolling "rate", Swipe "progress".
Update any existing selftest that expected the old stack seeding.

Build check folder id: `a19`. In the report, paste the final text of notes 4 and 5.

