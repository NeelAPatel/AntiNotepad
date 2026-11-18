# A21b-save-scheduler-extract: move save timing out of PadWindow into a scheduler

Item card (`docs/loop.md`). Claude fills Request to Approve before any build; Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB (logs go in reports).

Tier: High · save-paths
State: hand test
Branch: `refactor/A21b-save-scheduler-extract` in `.claude/worktrees/refactor/A21b-save-scheduler-extract` (created at Approve, from dev's tip)

## Request
Director (Master 6), 2026-11-18, user's go to start Findings (pending: `idea: A21b-save-scheduler-extract`, from the A21 design review; follows merged `A24-save-delay-cleanup`).
- **Goal:** same behavior, less in `UI/PadWindow.cs`: `SaveTiming`, the per-note deadline map and the data deadline move into a `NoteSaveScheduler`; one `Store.ResolveEmptyEdit(note)` for the blank-note rule now in three places.
- **Done check:** (1) a pending edit still saves on switch, hide, quit and session end; (2) save delays 0.4/5/10/30 s unchanged; (3) "Saved" label unchanged; (4) selftest and gate pass, no new warnings; (5) PadWindow.cs shorter, before/after line counts.
- **Proof:** existing A21/A24 selftests pass, plus a scheduler selftest that needs no window; one quick type-wait-switch check in your slot.
- **What we know:** A21 and A24 reports and reviews in `.coord/reports/` and `.coord/tasks/A21-*`, `A24-*`; code in `UI/PadWindow*.cs`, `Model/Store.cs`, `Model/Note.cs`.
- **Stale check (user, 2026-11-18):** development moves fast. Findings first re-reads the code on dev's tip: is the extract still worth it, is `PadWindow.cs` still the problem, did A24 or later work already cover part of it? Weigh pros and cons (risk to save paths vs gain) and recommend do, shrink to the `ResolveEmptyEdit` part only, or drop. Stop at the Plan for the user.
- **Constraints:** no behavior change; no timing value changes; High tier; Codex offline only; follow `.claude/rules/coding.md`; hand test in your slot's dev copy only. Other items may touch `PadWindow*.cs`; run `tools/catch-up.ps1` before the merge check.
## Findings
Re-read on dev tip e626863 (A24 merged), 2026-11-18. Nothing since A24 touched PadWindow.cs; A24 did not cover this.
- `UI/PadWindow.cs` is 739 lines (over the 600 trigger). Save timing is about 130 of them: schedule map and data deadline (`_saveSchedules`, `_dataSaveDeadline`), Schedule/Cancel/SaveImmediately/SaveTimerTick/ArmSaveTimer, and the pure `SaveTiming` class.
- `SaveTiming` is already pure and tested; the rest of the group is glue (timer, `QueueSave`, `Store`, window state). Callers sit in 4 files (Keys, Layout, Navigation, PadWindow).
- Blank-note rule ("blank, never saved: discard unless a write is in flight, else keep dirty") is copied in `ScheduleNoteSave` and `SaveImmediately`, with 2 more `DiscardUnpersistedEmptyEdit` calls in Navigation (drop, delete). State lives on `Note` (`Model/Note.cs`).
- Selftests reach in by reflection (`_saveTimer`, `_saveSchedules`) and by `PadWindow.SaveTiming`; a full extract rewrites that proof. Session end and quit have no PadWindow-level pending-edit check today.
- Full extract: about 100 fewer lines, but High-tier risk, a rewritten proof, and the glue stays in the window.
- Recommendation: **shrink** to `ResolveEmptyEdit` (the rule is data-loss-sensitive and has drifted copies); leave `NoteSaveScheduler` for later.

## Plan
- **After:** one `Note.ResolveEmptyEdit()` replaces the blank-note block copied in `ScheduleNoteSave` and `SaveImmediately` (Navigation's 2 `DiscardUnpersistedEmptyEdit` calls stay unless identical). The pure `SaveTiming` class moves unchanged to `UI/SaveTiming.cs` (own commit; selftest refs renamed). `PadWindow.cs` 739 -> about 675 lines.
- **Must not change:** delays 0.4/5/10/30 s, the "Saved" label, timer and scheduler glue, `_saveSchedules`.
- **Tests:** no-window selftest of ResolveEmptyEdit (discard; write in flight stays dirty; saved note untouched). New: pending edit survives `PrepareForSessionEnd` and quit. Existing switch, hide, minimize, tick checks pass. Pure move proven by equal `--selftest` output (timing lines skipped).
- **Gates:** build 0 warnings, selftest, gate, Codex review, design review. Hand test (slot dev copy): type, wait, switch, hide.

## Files
- `product/AntiNotepad/Model/Note.cs`
- `product/AntiNotepad/UI/SaveTiming.cs`
- `product/AntiNotepad/UI/PadWindow.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Store.cs`

## Contracts
- None new (no bug fixed); existing save rows keep passing.

## Approve
User, 2026-11-18: "Approve shrink + move"

## For Codex
Role: terra (`docs/agents.md`), a few files, shared save state. Branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then the Files.
- Build the Plan's "After" only; two steps, move first (pure), then ResolveEmptyEdit. Build into `build/A21b-save-scheduler-extract-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, delete it. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A21b-save-scheduler-extract -Offline`.
- Never launch AntiNotepad, edit STATUS or this card; offline checks only.
- Done means: Plan met, tests pass, 0 warnings, `SELFTEST PASS`, gate passes.

Report `.coord/reports/A21b-save-scheduler-extract.md`: 1 Summary; 2 Change list; 3 Behavior changes; 4 Out-of-scope files; 5 Plan deviations; 6 Contracts; 7 Lens review; 8 Checks run; 9 Hand checks.
## Check
- Gate (offline): PASS, build 0 warnings, SELFTEST PASS, 32 contract rows (31 pass, 1 manual). The online run failed only on NU1900 (no network).
- Codex review: accept, no findings. Design review: accept.
- Bounds: ResolveEmptyEdit takes a `blank` argument (both callers need it): justified. The move commit also holds the quit and session-end checks: justified. PadWindow.cs 739 -> 680: within plan.
- Noticed: SaveTiming.cs ends with a blank line (cosmetic); SelfTest resets SessionEnd through its backing field (same pattern as the Editor selftests); Navigation still calls DiscardUnpersistedEmptyEdit twice.
- Misses: none.
## Hand test
