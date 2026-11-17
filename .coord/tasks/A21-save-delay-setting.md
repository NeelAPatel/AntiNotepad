# A21-save-delay-setting: user-chosen save delay in Settings

Item card (`docs/loop.md`). Claude fills Request to Approve before any build; Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB (logs go in reports).

Tier: High · save-paths-timers (confirmed in Findings, own scout + Codex audit)
State: accepted
Branch: `feat/A21-save-delay-setting` in `.claude/worktrees/feat/A21-save-delay-setting` (created at Approve)

## Request
User, 2026-11-12 (split off A16-saved-label-fade): wants a Settings choice for the save delay, fixed 0.4 s after the last edit today, with 5 s, 10 s and 30 s options; default unchanged. Numbered A21 on the user's go, 2026-11-16 ("Start #2 with our pstack").

- **Goal:** the user can pick how long after their last edit a note saves (default stays 0.4 s; options 5, 10, 30 s) in Settings.
- **Done check:** (1) fresh settings = 0.4 s, behavior identical to today; (2) each option delays the save by that long after the last keystroke and a new edit restarts the wait; (3) a pending edit still saves on hide, quit, note switch, sign-out and Windows shutdown, whatever delay is chosen; (4) the setting survives restart; (5) "Saved <time>" label follows the real save time.
- **Proof:** selftest checks for default value, settings round trip and the flush-on-exit paths; a log line with the delay and actual save time per option; hand test in the dev copy (type, wait, kill-free quit/hide check) by the user.
- **What we know:** save delay is a fixed timer today (find it, do not assume); A30 (restart/sign-out saves a just-typed edit) still has a live check open; Settings uses `Settings` model plus `UI/Settings`; visible Settings change is lab-first.
- **Constraints:** High tier until Findings prove lower; lab-first for the Settings control; dev copy only (never the installed copy); one Codex run unless the plan says more; follow `.claude/rules/coding.md`.

## Findings
Audit: `.coord/reports/A21-save-delay-setting-audit.director.codex.md` (READY yes). Kind: feature + label fix.
- Now: `PadWindow.cs:30` fixed 400 ms restart timer; `Edited()` -> `ScheduleSave()` -> tick -> `QueueSave()`; Store adds a 50 ms coalesce. Settings/geometry changes share the same timer (settings live in `index.json`).
- Already immediate: hide, click-away (`Deactivated`), quit, session end (sign-out/shutdown), folder move.
- Gap 1: note switch, new note, slot/stack/search jump only restart the timer (`PadWindow.Navigation.cs`); `PadView.Flush()` copies caret only. Text stays in memory, not on disk.
- Gap 2: "Saved <ago>" uses `Note.Modified` (last edit), refreshed on edit, not on save completion. Wrong at 5-30 s.
- `Modified` stays last edit (search, trash, clipboard note read it). Dirty clears only on durable completion (OK).
- Crash/kill loses up to the delay. Not durable against power loss (unchanged).
- Tier: High confirmed. `SettingsWindow.cs` 746 and `PadWindow.Layout.cs` 696 lines: split candidates, separate items.

## Plan
**Now:** 0.4 s fixed; switching notes does not save; label shows last edit time.
**After:** Settings > Saving: "Save after I stop typing" 0.4 s (default) / 5 / 10 / 30 s (layout A: dropdown row "Save after I stop typing", user pick 2026-11-16, in a new "Saving" group). New `Settings.SaveDelayMs` (400), persistence-only flags, invalid value -> 400. Timer uses it; changing it while pending restarts the wait. Every note transition (switch, new, slot/stack/search jump, split) saves now (`QueueSave`, not `Flush`). Hide, blur, quit, session end unchanged. Label: in-memory `Note.SavedAt` set when the writer's durable completion clears `Dirty`; label = `SavedAt ?? Modified`, refreshed on completion; a waiting edit keeps the previous time (lab Q1). No file-format change. One log line: delay and actual save ms.
**Tests:** selftests for default/invalid/round trip of `SaveDelayMs`; timer interval follows setting; pending edit saved on each transition; stale completion does not set SavedAt; `Modified` unchanged by save; CONTRACTS row. Gate + Codex review + design review. Hand test (user, dev copy): each delay, second keystroke restarts the wait, hide/quit/switch/Settings, restart keeps the choice, label. Shutdown/sign-out stays the open A30 live check.
**Role:** `worker` (shared state, timing), one run. **Tier:** High.

## Files
- `product/AntiNotepad/Model/Settings.cs`
- `product/AntiNotepad/Model/SettingsService.cs`
- `product/AntiNotepad/Model/Note.cs`
- `product/AntiNotepad/Model/Store.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/UI/PadWindow.cs`
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/SelfTest/Store.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/Theme/ChromeTokens.cs`
- `product/AntiNotepad/SelfTest/Design.cs`

## Contracts
- A pending edit is on disk after any note transition, hide, quit or session end, whatever the delay.
- `SavedAt` is set only by a durable completion whose revision matches; `Modified` is never changed by a save.

## Approve
User, 2026-11-16: "Approve" (layout: "A: dropdown").

## For Codex
Role: worker, because shared state and timing across timer, Store completion and navigation. Branch is checked out: never switch, merge or commit. Read `AGENTS.md`, `.claude/rules/coding.md`, then the Files; re-read before editing. Audit: `.coord/reports/A21-save-delay-setting-audit.director.codex.md`.
- Build exactly the Plan's "After" for the Files above; no drive-by changes. If Findings are wrong, stop and report.
- Control: a Dropdown row "Save after I stop typing" (hint "Your note is also saved when you hide, switch notes, or quit.") in a new "Saving" group below "Window" on the page that holds Opening/Window; options 0.4 s (400, default), 5, 10, 30 s; same pattern as "Fresh note after". Value in `Settings.SaveDelayMs`, persistence-only flags, invalid -> 400. Timer interval token goes in `Theme/` or `Motion/` per coding rules.
- Note transitions save now via `QueueSave()` (not `Flush()`); hide, blur, quit, session end unchanged. Changing the delay with a timer pending restarts the wait.
- `Note.SavedAt` (in memory, not serialized) set only when a durable completion clears `Dirty` for the matching revision; label = `SavedAt ?? Modified`; refresh the active label after completion; `Modified` untouched. Log line: delay and actual save ms.
- Build into `build/A21-save-delay-setting-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, delete it. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A21-save-delay-setting -Offline`. Never launch or kill AntiNotepad, never edit STATUS or this card. Offline only.
- Done means: Plan tests exist and pass; CONTRACTS rows added; 0 warnings, `SELFTEST PASS`, gate passes.

Report `.coord/reports/A21-save-delay-setting.md`: 1 Summary; 2 Change list (`file:lines | what | Plan line`); 3 Behavior changes; 4 Out-of-scope files; 5 Plan deviations; 6 Contracts; 7 Lens review; 8 Checks run; 9 Hand checks for the user.

## Check
- Round r5 (final): gate PASS (my run), selftest 26 rows. Codex review r4 rework (2 major, 1 minor), all fixed in r5 with before/after tests; r5 re-review accept (1 minor to pending). Design review: accept-with-followup (scheduler extract, blank-note rule in 3 places, tracking set); follow-ups in pending, not blocking.
- Rounds: r1 failed gate; r2 fixed tests; user hand test 1 failed; r3 timed out at 45 min; r4 finished it; r5 fixed review findings.
- Bounds: within plan, with the user-approved rule change of 2026-11-16 (1 s idle gate, safety write, new label). 14 product files.
- Misses: r1 ended without a green selftest; r3 hit the time limit with no report. Lesson: ask for a draft report after the first green selftest (done in r4).

- Tidy-up (2026-11-17): caught up to dev (RETEST: no), 14 commits folded into 2 (product, card and reports); product tree identical to before; gate PASS at the tip.

## Hand test
User, 2026-11-16 (dev copy, delay 5 s): FAILED. (1) New blank note already says "Saved just now"; expected "-" (blank notes are never saved). (2) After typing, still "Saved just now" while the timer runs. (3) Every 5 s the top bar and pager bar show up, probably tied to the save.
Rule the user wants: timer starts only when typing stops (no edit for 1 s, then count the rest of the 5/10/30 s); label "1s..59s ago", then "1m..4m ago", then a timestamp; save at once on note switch, hide, minimize, quit. Asked for a state graph of the edge cases: `tools/playground/a21-save-states-lab.html`.
Cause (read from code, not yet run): `SavedText` falls back to `Modified` (the last edit time) when `SavedAt` is empty, so an unsaved note shows "just now"; `OnNoteSaved` calls the full `UpdateChrome`, the likely source of the bar flash (unverified).
User, 2026-11-16 (hand test 2, dev copy, 5 s): blank note "-" works; leaving saves (switch, hide, minimize) works; split-view click-away save works; restart keeps the choice works; type-then-stop shows "Saved just now" but never counts up; nonstop typing works "but visually has issues, as detailed earlier". "The visibility of the bars doesn't matter. The text should update regardless." Cause: the label clock runs only while `_topShown` (slide styles), never in fade mode. Fix goes in r6.
User, 2026-11-17 (hand test 3, dev copy from r6 build): "all passed".
