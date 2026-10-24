# A19-demo-slot-notes

## Changes

- `product/AntiNotepad/Model/Store.cs`: changed fresh-install defaults to rate-matched scrolling and progress swipe. Scroll and swipe tuning defaults remain unchanged; saved values continue to load from `index.json`.
- `product/AntiNotepad/Model/Welcome.cs`: first-run seeding now fills empty slots 1–5 with readable source constants for Markdown, math, checklist, timers, and the guide. It leaves occupied slots and the stack alone, then selects slot 5. `Program.cs` still gates this on the store being available.
- `product/AntiNotepad/SelfTest.cs`: added one welcome-slot check group for expected first lines, occupancy, empty slots and stack, selected slot, idempotence, timer parsing, and the new settings defaults. No earlier selftest asserted the former stack seeding.
- `docs/decisions.md`: clarified the selected scrolling and swipe defaults.

`UI/MotionPreview.cs` and `Model/Showcase.cs` contain no references to the removed welcome constants; neither file changed.

## Verification

- Final Release build: `dotnet build product/AntiNotepad -c Release -o build/a19-check -p:NuGetAudit=false` — 0 warnings, 0 errors. `NuGetAudit=false` was needed because the standard build could not reach NuGet's vulnerability-data endpoint and emitted NU1900 warnings; the standard build itself succeeded with 0 errors.
- `build/a19-check/AntiNotepad.exe --selftest` — `SELFTEST PASS`.
- `git diff --check` — clean.
- The build output folder remains: the sandbox rejected the recursive cleanup command after its resolved path was confirmed inside this worktree.

## Rendering notes

- `divider` stays ordinary text. The following `---` matches the divider pattern: `NoteColorizer` paints the dashes faintly and `DividerRenderer` draws a thin rule across the note. Clean Markdown can hide the dashes away from the caret.
- In `__*~~**all at once**~~*__`, the outer `__` applies underline, `**all at once**` applies bold, and the inner `~~...~~` applies strikethrough. The surrounding single `*` markers do not match the italic pattern in this nesting, so they remain visible as literal characters.
- The numbered-list colorizer does not parse or restyle list markers. The two and four leading spaces stay in the text; the separate indentation guide uses two-space levels for plain notes.

## Timer behavior

The five timer lines below all parse. `PadView` creates ready entries without an end time; timers start only after the user toggles one with Ctrl+Enter or clicks its timer chip. A fresh install does not start or ring these timers automatically.

```text
timer
1m tea
30s focus
1h30m laundry
5:00 eggs
2min stretch
```

## Needs a live check

- Needs a live check: verify first-run startup visibly opens slot 5 and the guide renders as intended.
- Needs a live check: review the Markdown demo's headings, combined emphasis, rule, list indents, and fenced code; confirm timer chips appear ready and start only on interaction; confirm deleting a demo moves it to Trash.
- No live GUI check was performed for this task.

## Note 5 text

```text
# Welcome to AntiNotepad
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
- [ ] Press `Ctrl+Enter` to check off this example.

## Help
- `Ctrl+,` opens Settings. `F1` shows all shortcuts.
- To clear a demo, open its slot and press `Ctrl+Shift+Backspace`; the note moves to Trash.
```
