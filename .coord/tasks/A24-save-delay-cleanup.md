# A24-save-delay-cleanup: tidy the save-delay code, no visible change

Item card (`docs/loop.md`). Claude fills Request to Approve before any build; Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB (logs go in reports).

Tier: High · save paths (Store/save writes; raised from Low in Findings, item chat 2026-11-17)
State: hand-tested
Branch: `refactor/A24-save-delay-cleanup` in `.claude/worktrees/refactor/A24-save-delay-cleanup` (created at Approve, from dev's tip)

## Request
Director (Master 6), 2026-11-17, from the A21 reviews, user's go to start. Two small follow-ups from `A21-save-delay-setting` (merged), in pending as `idea: save-delay-choices` and `idea: A21c-body-write-tracking-simplify`:
- One list of allowed save delays in Model, shared by the setter and the Settings dropdown; `NoteSaved` becomes `Action<Note>` (drop the delay argument); comments for the UI-thread and BindPanes save rules.
- One per-note flag instead of the revision set `_unsupersededBodyWrites` (fixes its leak for deleted notes); fold `Note.HasPendingBodyWrite` into it.

- **Goal:** the same behavior with less code: one source of save-delay values, one pending-write flag per note, no leak.
- **Done check:** (1) 0.4, 5, 10 and 30 s still work and an invalid value still falls back to 400 ms; (2) a pending edit still saves on switch, hide, quit and session end; (3) "Saved" label unchanged; (4) deleting a note with a pending write leaves nothing behind; (5) selftest and gate pass with no new warnings.
- **Proof:** the existing A21 selftests still pass, plus one new check for the deleted-note leak; before and after line counts; no hand test needed beyond one quick type-wait-switch check in the dev copy.
- **What we know:** A21 reports and review notes in `.coord/reports/` and `.coord/tasks/A21-save-delay-setting*.md`; code in `Model/Settings.cs`, `Model/Note.cs`, `Model/Store.cs`, `UI/PadWindow*.cs`, `UI/SettingsWindow.cs`. Do not do the larger `idea: A21b-save-scheduler-extract` (separate).
- **Constraints:** no behavior change; Codex offline only; follow `.claude/rules/coding.md`; if the dev copy runs, another item may run one too (use the dev launcher, not the installed copy). Self-test first; low involvement from the user: ask them only for the final quick check.

## Findings
Own scout, 2026-11-17 (Low audit lane; tier raised to High by the tiers table: save paths).
- Delay list is written 3 times: `Settings.SaveDelayMs` setter (400/5000/10000/30000), `ChromeTokens.SaveDelay*Ms`, and the 4 labelled entries in `UI/SettingsWindow.cs:540`.
- `Store.NoteSaved` is `Action<Note,int>`; the int (delay at snapshot) only feeds one log line in `PadWindow.OnNoteSaved`; the A21 selftest handler ignores it.
- Pending-write state is two collections in `Model/Store.cs`: `_pendingBodies` (text queued) plus `_unsupersededBodyWrites` (id -> revisions written but not yet acknowledged). Writer adds a revision (line ~398); `ApplyCompletedSaves` removes the whole entry only when the acknowledged note's revision matches (line ~538). A note deleted for good never gets an acknowledgement, so its entry leaks. Only `Count > 0` is ever read, so the revisions are never used.
- Readers: `Store.HasPendingBodyWrite(note)` (PadWindow x2) and the static hook `Note.HasPendingBodyWrite` (Note.Touched), re-set in two Store spots.
- Nothing else reads the revision set; no selftest touches it directly.

## Plan
**After:** same behavior, less code.
1. `Settings.SaveDelayChoicesMs` (400, 5000, 10000, 30000): used by the setter and by the Settings dropdown (label from the value: 0.4 s, 5 s, 10 s, 30 s). `ChromeTokens.cs` is not edited (T17 touches it): the list is built from the existing token constants, which stay.
2. `NoteSaved` becomes `Action<Note>`; `OnNoteSaved` logs the current setting; selftest handler updated.
3. `Note.BodyWriteInFlight` (volatile bool, set by Store under `_saveGate`) replaces `_unsupersededBodyWrites` and the static `Note.HasPendingBodyWrite`. Set when a body is queued in the snapshot; cleared when an acknowledgement with the matching revision lands (the old removal point) and when a note is dropped from the store. `Store.HasPendingBodyWrite` goes; PadWindow reads the flag.
4. Comments (coding rules): one on the UI-thread rule for save callbacks (`NoteSaved` fires on the caller of `ApplyCompletedSaves`, UI work only if `CanTouchPadUi`), one on why `BindPanes` queues a save when the shown notes change (from the A21 r2/r4 reviews).
**Test plan:** existing A21 selftests unchanged except the handler signature; new check: a note with a queued body is deleted for good, then a save runs; its flag is clear and nothing is left (done check 4). Settings round trip and invalid-to-400 checks already cover (1). Gate (offline) plus line counts before/after. No timing change, so the safety/idle logic (`SaveTiming`) is not touched.
**Gates:** build, `--selftest`, `tools/gate.ps1 -Offline` (format, tokens), Codex review, design review (High). Hand test: type, wait, switch note in the dev copy.
**Risk:** the flag must clear exactly where the revision entry did; the review checks that.

## Files
- `product/AntiNotepad/Model/Settings.cs`
- `product/AntiNotepad/Model/Note.cs`
- `product/AntiNotepad/Model/Store.cs`
- `product/AntiNotepad/UI/PadWindow.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Store.cs`

## Contracts
- Behavior unchanged: 0.4/5/10/30 s work; any other value falls back to 400 ms; a pending edit saves on switch, hide, quit and session end; the "Saved" label is unchanged.
- A note deleted for good with a pending body leaves no pending-write state behind (new selftest).

## Approve
User, 2026-11-17: "Approve"

## For Codex
Role: terra (a few files, shared save state; reasoning needed, not xhigh). Do the Plan exactly; follow `.claude/rules/coding.md` (touched files brought up to standard; no item IDs in comments). Do not touch `SaveTiming` or any timing value. Do not commit.
Done means (offline only): build into `build/A24-save-delay-cleanup-check`, run `--selftest` there (all pass, new leak check included), `tools/gate.ps1 -Offline -Task A24-save-delay-cleanup` passes with no new warnings; delete the build folder after.
Report `.coord/reports/A24-save-delay-cleanup.md`: Changed, Tests run (output tails), before/after line counts of the touched files, Risks.

## Check
2026-11-17. Gate PASS (offline) and `--selftest` PASS, 0 warnings, incl. the new queued-note deletion check. Reviewer: accepted (flag set where `_pendingBodies` is set, cleared at the old removal point; thread safety ok). Design review: accepted.
- Bounds: request, plan and result match. ChromeTokens.cs dropped from Files (T17 overlap); the list uses the existing token constants.
- Misses: plan step "clear the flag when a note is dropped" not implemented; a deleted note keeps its own unused flag, so no store state is left (build report corrected). Log line shows the current delay, not the snapshot's.
- Notes: first Model-to-Theme `using` (deliberate). Store.cs 560 to 552 lines. Card is over its 4 KB budget.

## Hand test
User, 2026-11-17: "Works" (type, wait, switch in the dev copy).
