# A42d-list-commands: move list continue and renumber commands out of PadView, behavior-neutral

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Go before any build; Codex reads the whole card and
follows "For Codex". Codex never edits this file; Check and Hand test are filled by the sub-session afterwards.

Tier: Medium (editor input: Enter, Tab renumber, caret and undo grouping) (rules: `docs/loop.md`)
Branch: `feat/A42d-list-commands` in `.claude/worktrees/A42d-list-commands`

## Request
Plan row Scope (user order, 2026-10-29): "Move list commands out of `PadView` into its own Editor class,
behavior-neutral". Origin: A42-padview-commands re-audit (A42d optional, after A42c).
User, 2026-10-29: "Lets do A42c then A42d and finally C29"; "Start A42d". Screenshots: none.
Boundary: timer start on Enter in timer notes stays as today (separate idea timer-enter-no-autostart, user 2026-10-29).

## Findings
- **Kind and why:** polish/reorg, behavior-neutral. `Editor/PadView.cs:746-831` still owns list recognition
  (`IsListLine`), Enter continuation (`ContinueList`), `Renumber` and `Insert`; none of it can be tested without the app.
- **Now:** unmodified Enter, empty selection, completion closed (`:675-680`): continues bullets (`-*+` kept), numbers
  (`.`/`)`; `9.` -> `10.`) and checkboxes (`[x]`/`[X]` continue unchecked), splitting mid-line at the caret. Empty item
  (whole line): two+ indent chars -> remove two and renumber; otherwise clear the line (exit). 999,999,999 and oversized
  numbers decline; Code mode declines; List mode supplies `[ ] ` for a plain line. Continue + renumber share one
  `RunUpdate` (one Ctrl+Z); Tab indent + renumber share another. `Insert` calls `BringCaretToView` before renumber.
  Declined continuation in a timer note falls through to `StartTimerOnLine` (`:680`, `:711-719`). Existing selftests
  cover parsers and A42c's single edits, not continuation or combined renumber.
- **Reproduction (bugs):** not a bug; source trace of Enter and Tab paths is the baseline (audit section 3).
- **Where:** `PadView.cs`, `ListEditing.cs`, `DocumentCommands.cs` (both from A42c; no new class), selftests. Downstream
  and untouched: per-note document/undo cache (limit 400), NoteAnalysis (140 ms debounce over 1,500 lines), `Edited` ->
  400 ms save timer, timer start on Enter.
- **Risk:** edit order/size changes, stale offsets after `9 -> 10` growth, reading the caret line too early, splitting
  either undo group, or a whole-block replacement changing caret anchoring and change notifications.
- **Source:** `.coord/reports/A42d-list-commands-audit.md` (READY: yes, fb40d66). Tier Medium confirmed.

## Plan
- **After:** `ListEditing` decides continuation (what marker to insert, or how to empty/outdent, or decline) and
  numbering (which line gets which number), as pure code. `DocumentCommands` applies them to a live `TextDocument`, line
  by line, with live offsets (no whole-block replace). `Renumber` moves too. `PadView` keeps key/modifier/selection/
  completion routing, the two `RunUpdate` scopes, the post-edit `CaretLine` read, the timer fall-through, and a thin
  `Insert` (insert at caret + `BringCaretToView`, together). You see no difference.
- **Must not change:** any text, number, caret, scroll, undo step; the whole-line emptiness rule and its
  character-based outdent; both renumber calls on empty nested Enter; timer Enter auto-start and Ctrl+Enter exactly as
  today; checkbox/indent from A42c; `TextCommands` grammar; analysis, caches, save timing. No generic command framework.
- **Scope questions:** None (existing quirks are preserved).
- **Test plan, code:** pure table cases (bullets, `.`/`)`, checked/unchecked/bullet-wrapped boxes, mid/end, empty
  nested/root/one-indent, Code decline, List fallback blank-first/blank-later, 999,999,999 and ten-digit declines). Real
  `TextDocument` checks through the production code: exact text and one Undo (CanUndo false after) for Enter
  continue+renumber, nested outdent+renumber, root exit, Tab and Shift+Tab with renumber, e.g. Tab on line 1 of
  `8. a/9. b/9. c/9. d` -> `  1. a/9. b/10. c/11. d`; Shift+Tab on `8. a/  1. b/9. c` -> `8. a/9. b/10. c`; plus block
  boundary, nested level, bullet reset, overflow, nonzero offset and CRLF fixtures. Existing LAYERS bans kept. New
  CONTRACTS row `list-commands-preserved`; `checkbox-indent-preserved` kept.
- **Test plan, UI (hand test in the dev copy):**
  1. Enter at the end and in the middle of `- ab`, `9. ab`, `9) ab`, `[x] ab`: same markers as before (`10.`, `[ ]`),
     text after the caret moves down, caret sits after the new marker.
  2. Enter on an empty nested item (outdents) and an empty root item (ends the list).
  3. Tab / Shift+Tab in a numbered list with 9 -> 10: numbering as before; one Ctrl+Z undoes each Enter and each Tab.
  4. Shift+Enter, Enter with a selection, Enter with completion open: normal Enter as before.
  5. Long note: Enter on a list item at the bottom keeps the caret visible; typing in a long list feels the same.
  6. Timer note: Enter after `10s test` still starts the timer (unchanged on purpose); Ctrl+Enter still toggles it.
- **Gates:** `tools/gate.ps1` (build 0 warnings, selftest, LAYERS, CONTRACTS, seam, format, scope = Files, size: PadView
  should shrink); Codex fresh lens review (edit order, live offsets, undo scopes, caret read timing, timer fall-through);
  new CONTRACTS row; hand test (caret, scroll, undo on screen). No reproduction test, lab or design review.
- **Needs from you:** go, then the hand test.

## Files
Only these files may change (exact paths; `tools/gate.ps1` reads this list). Any other file needs a line under
"Out-of-scope files" in the report.
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/Editor/ListEditing.cs`
- `product/AntiNotepad/Editor/DocumentCommands.cs`
- `product/AntiNotepad/SelfTest.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`

## Contracts
- All existing rows keep passing (12 rows: 11 pass, 1 manual), incl. `checkbox-indent-preserved`.
- New `list-commands-preserved`: Enter continuation and Tab/Shift+Tab renumber produce the same text and one Undo step
  as before the move; live caret position and scrolling are manual.

## Go
User, 2026-10-29: "Go" (plan as written).

## For Codex
Role: director (gpt-6.1-sol medium), delegating to a worker (gpt-6-luna xhigh). Product `product/AntiNotepad`. The branch is already
checked out: never switch, merge or commit (you cannot write to `.git`). Read `AGENTS.md`, then the files above. Line
numbers are from the current tree; re-read before editing.

Constraints:
- Build exactly the Plan's "After" for the Files above. No drive-by refactors, renames, icon/token/style edits outside it.
- If the Findings turn out wrong (the bug does not reproduce, the cause is elsewhere, the plan cannot work), stop and
  report that instead of building something else.
- Build into `build/A42d-list-commands-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A42d-list-commands -Offline`; the manager
  re-runs it online.
- Never launch or kill AntiNotepad. Never run `--quit`. Never edit `.coord/STATUS.md` or this card.

Done means:
- `ListEditing` owns pure continuation and numbering decisions; `DocumentCommands` applies them line by line with live
  offsets; `Renumber`, `ContinueList` and `IsListLine` logic no longer lives in `PadView`.
- `PadView` keeps routing, both `RunUpdate` scopes, the post-edit caret read, the timer fall-through and the thin `Insert`.
- New pure and real-`TextDocument` selftests and CONTRACTS `list-commands-preserved` exist and pass.
- The Plan's code tests exist and pass; build 0 warnings, `SELFTEST PASS`, the gate passes.

Before you report (director only): spawn the `lens_reviewer` agent on your diff (it reads
`.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why not. Workers without spawning: run the same
checklist yourself and say so.

Report: `.coord/reports/A42d-list-commands.md`, short, with exactly these sections:
1. **Summary**: two or three sentences.
2. **Change list**: one row per changed block: `file:line-range | what changed | which Plan line it serves`.
3. **Behavior changes**: every changed public method result or meaning, new or changed shared state, timing, default, or
   setting effect. Write "None" only if true; the review checks this against the diff.
4. **Out-of-scope files**: file and why, or "None".
5. **Plan deviations**: anything done differently from the Plan, and why, or "None".
6. **Contracts**: rows added or changed, or "None".
7. **Lens review**: findings and what you did with each.
8. **Checks run**: build, selftest, gate result lines.
9. **Hand checks for the user**: the Plan's UI checks, plus anything new, or "None (not visible)".

## Check
- **Gate (online, run by Master 1, 2026-10-29):** GATE: PASS. Build 0 warnings 0 errors; SELFTEST PASS; CONTRACTS 13 rows, 12 pass, 1 manual; Seam PASS; Format PASS (5 changed files); Scope PASS (5 files); Size WARN (SelfTest.cs 3089 -> 3247 lines, planned tests; PadView shrank by 75); Cleanup PASS.
- **Gate (this session):** online run failed twice on NU1900 only (nuget.org unreachable in sandbox, env); `-Offline` run GATE: PASS, same results.
- **Codex review:** `.coord/reports/A42d-list-commands.review.md` VERDICT: accept, no proven findings. Unproven: live caret/scroll feel (hand test).
- **Bounds check:** Change list (5 Files-listed product files) = within plan. Behavior changes "None" = matches diff and review. Out-of-scope files: only reports = within plan. Plan deviations: None. Contracts: `list-commands-preserved` added = within plan.
- **Noticed:** none.
- **Misses:** found at gate in Check; cause env; caught by gate; should have caught: none (sandbox has no network); lesson: Check sessions run the gate with -Offline and Master reruns online.

## Hand test
Dev copy from the worktree (PID 12308, build/dev, DATA build/testdata, log checked). Checks 1-6 given.
User, 2026-10-29: "all passed"
