# A21-save-delay-setting-r3: save timing and "Saved" label rework

Rework round of `A21-save-delay-setting` (`docs/loop.md`). Read the card `.coord/tasks/A21-save-delay-setting.md` first, including its "Hand test" section; this file overrides the card's Plan where they differ. Do not edit the card.

Tier: High (same)
Role: worker, because shared timer, Store completion and per-note state.

## Why this round
User hand test (dev copy, delay 5 s) failed: a blank new note says "Saved just now"; after typing it still says "just now"; the top bar and pager bar appear every 5 s. User wants this rule (user, 2026-11-16, chat):
- Timer starts only when typing stops: no edit for 1 s, then count the rest of the chosen delay N (5/10/30 s; 0.4 s default behaves as today, no 1 s wait). Save lands N s after the LAST edit. Any new edit restarts it.
- Safety write (user chose it): if the user keeps typing without a 1 s pause, still write N s after the first unsaved edit, and again every N s while edits continue.
- Save at once on note switch, hide, minimize, click away, quit, sign-out, shutdown (existing paths stay; minimize must be added if missing).
- Label: never-saved blank note shows "-". Otherwise `Saved just now` (under 1 s), `Saved 1s ago` to `59s`, `Saved 1m ago` to `4m`, then a timestamp `Saved 14:02` (add the date when not today). A waiting edit keeps the previous label. Never a number for under 1 s. A time in the future reads "just now".
- Root causes found: `SavedText` (`UI/PadWindow.Layout.cs`) falls back to `Note.Modified` when `SavedAt` is empty; `OnNoteSaved` (`UI/PadWindow.cs`) calls the full `UpdateChrome`, which rebuilds the meta row and the pill and probably wakes the bars. A save must only change the label text: it must never wake or show the bars.

## Edge cases (all must be handled and tested; user approved 15-26)
1. Blank, never-saved note: no timer, no write, label "-". Delete all text in a never-saved note: back to blank, timer cancelled. Deleting all text in a saved note saves the empty text as today (check the code first).
2. Disk write fails: label keeps the old time, edit stays unsaved and retries at the next tick, existing error toast stays.
3. Split view or two notes edited: the timer must be per note (or one save covers every unsaved note). No edit may wait past its own N s.
4. Undo and redo, paste, replace-all, calculator and timer-note changes count as edits.
5. Sleep/wake or clock change: countdown uses a steady clock (`Stopwatch`/`Environment.TickCount64`); label uses wall clock and never goes negative.
6. Pad hidden when the timer fires: save anyway, no UI work; label refreshes when shown.
7. Label redraw clock: a once-a-second refresh only while the top bar is visible, aligned to whole seconds after `SavedAt`; nothing runs while hidden.
8. Note deleted or trashed while unsaved: cancel its timer; trash keeps current text.
9. IME composition in progress: the safety write only reads text and must not disturb composition.
10. Large note with safety write: write stays off the typing thread (keep the 50 ms coalescing); report a timing sample.
11. Switch notes and straight back: first note was saved on leaving; no extra write.
12. Delay changed mid-countdown: pending edits save at once; the new delay applies from the next edit.
13. Saving must not touch the editor's undo stack: it only copies text out. Add a check: type, save, undo, text returns.

## Files
- `product/AntiNotepad/Model/Note.cs`
- `product/AntiNotepad/Model/Store.cs`
- `product/AntiNotepad/UI/PadWindow.cs`
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/Kit.cs`
- `product/AntiNotepad/Theme/ChromeTokens.cs`
- `product/AntiNotepad/SelfTest/Store.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest/Design.cs`

If another file is truly needed, stop and say why in the report; do not edit it.

## Done means
Offline checks only. Pull the timing rules into a small pure type (state in, next state and deadline out) so selftests can drive it with a fake clock; test each edge case above that can run offline, plus the label text table and the "save does not wake the bars" rule. Selftest names the new checks; update the CONTRACTS rows. Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A21-save-delay-setting-r3 -Offline` passes: run it yourself and quote its FINAL line in the report. If any process hangs, tell me the PID and path, do not wait. Never launch or kill AntiNotepad.

Report: `.coord/reports/A21-save-delay-setting-r3.md`, card report format, plus section 10 "Hand checks for the user" (new blank note, type at 5 s, nonstop typing, switch/hide/minimize, bars stay still on a save).
