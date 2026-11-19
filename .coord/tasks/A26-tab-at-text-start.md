# A26-tab-at-text-start: Word's Tab rule for list items

Item card (`docs/loop.md`). Claude fills Request to Approve before any build; Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB (logs go in reports).

Tier: Medium · editor-feature
State: hand-tested
Branch: `feat/A26-tab-at-text-start` in `.claude/worktrees/feat/A26-tab-at-text-start` (created at Approve, from dev's tip)

## Request
Director (Master 6), 2026-11-18, user's go to start Findings (pending: `idea: tab-at-text-start`, chosen by the user during the `A22-wrap-hanging-indent` hand test, 2026-11-17).
- **Goal:** Word's Tab rule for list items. Tab and Shift+Tab change a list item's level only when the caret is at the start of its text; elsewhere Tab inserts 2 spaces. Alt+Shift+Right and Left change the level from anywhere. Ctrl+Tab inserts 2 spaces at the text start (Word's Ctrl+Tab behavior is unverified; say what you assume).
- **Done check:** (1) Tab at text start indents a list item; Tab mid-text inserts 2 spaces; (2) Shift+Tab outdents at text start; (3) Alt+Shift+Right and Left change the level from anywhere; (4) checkboxes, bullets and numbered lists follow the same rule; (5) a selection spanning several lines keeps a sensible, stated behavior; (6) selftest and gate pass.
- **Proof:** selftests on the key rules (no window needed if the command code allows it); a short hand check in your slot: Tab at three caret spots on a list.
- **What we know:** today Tab indents a list line from any caret position (`Editor/PadView.cs`, `DocumentCommands.Indent`). Open: checkboxes and numbered lists, selections spanning several lines (decide in the Plan and say so; ask the user only if it is a real taste call). Keep the existing wrap hanging indent (merged A22) working.
- **Stale check (user, 2026-11-18):** ideas age fast. Findings first re-reads the code on dev's tip and confirms Tab still behaves as described, then weighs pros and cons (could this surprise users who rely on Tab anywhere?) and recommends do, shrink or drop. Stop at the Plan for the user.
- **Constraints:** Codex offline only; follow `.claude/rules/coding.md`; hand test in your slot's dev copy only (`docs/dev-loop.md`), never the installed copy and never `--quit` without `ANTINOTEPAD_DATA`. Another item (`A21b-save-scheduler-extract`) edits `PadWindow*.cs` and `Model/`; stay in `Editor/` and run `tools/catch-up.ps1` before the merge check. Self-test first; ask the user only for the final quick check.

## Findings
- **Kind and why:** polish; user chose it in the A22 hand test; a mid-line Tab that moves the whole item is a surprise.
- **Now (dev tip, re-read 2026-11-18, stale check passed):** `PadView.OnKeyDown` (PadView.cs:862) handles Tab only with an empty selection on a list line, from any caret column: `DocumentCommands.Indent` adds/removes 2 leading spaces, then `Renumber`. A selection, or a non-list line, falls through to AvaloniaEdit's own Tab (2 spaces, `ConvertTabsToSpaces`). Alt+Shift+arrows are unbound here (AvaloniaEdit uses them for box selection). List line = `ListEditing.ListPrefix` (bullet, checkbox, number).
- **Pros:** the common flow (Enter, then Tab) puts the caret right after the marker, so it still works; mid-text Tab stops moving items by accident. **Cons:** users who Tab from mid-text now get spaces; Shift+Tab mid-text must not fall back to AvaloniaEdit's outdent. Mitigation: Ctrl+[ / Ctrl+], plus the help line. **Recommend: do** (small).
- **Where / Risk:** `Editor/` only (no `PadWindow*.cs`, no `Model/`). Risk: Alt+Shift box selection lost on list lines. PadView.cs passes 600 lines; split is a separate item, so keep new logic in `ListEditing`/`DocumentCommands`.
- **Source:** own scout (5 files read; below the Codex-audit floor).

## Plan
- **After:** on a list line with an empty selection, "text start" = caret at or before the end of the list marker (leading spaces, bullet, checkbox or number). Tab: at text start indent, else normal Tab (2 spaces). Shift+Tab: at text start outdent, else swallowed (no-op, as Word). Alt+Shift+Right/Left: change level from anywhere; with a selection, every list line it touches (then renumber). Ctrl+Tab: 2 spaces at the caret (assumed; Word's is unverified). Bullets, checkboxes and numbers share one rule. Non-list lines and Tab with a selection: unchanged (AvaloniaEdit's block indent).
- **Data shape:** one pure decision, `ListEditing.TabDecision(text, caretColumn, shift, ctrl, alt)` returning `Indent | Outdent | InsertSpaces | Swallow | Default`; PadView only maps the key to it, so the rule lives in one place and the selftests hit it directly.
- **Checkpoint:** one Codex worker (`terra`, the files are coupled by one rule); no fan-out; no `architect` (one obvious shape).
- **Expected time:** under an hour.
- **Must not change:** wrap hanging indent, Enter continuation, renumbering, Ctrl+Enter.
- **Scope questions:** None; selection rule above is my call (keeps today's behavior).
- **Test plan:** pure `ListEditing.IsAtTextStart(text, column)` and `ListEditing.TabDecision` selftests: bullet, checkbox, numbered, indented, caret before/inside/after marker, Shift+Tab mid-text. `CONTRACTS` row. UI: a headless key check if the selftest harness allows, else hand check: Tab at 3 caret spots on a list, Alt+Shift+Right/Left.
- **Gates:** build 0 warnings; `--selftest`; gate offline; Codex review (Medium).
- **Needs from you:** Approve; later the quick hand check.

## Files
Only these files may change (exact paths; `tools/gate.ps1` reads this list). Others need an "Out-of-scope files" line in the report.
- `product/AntiNotepad/Editor/ListEditing.cs`
- `product/AntiNotepad/Editor/DocumentCommands.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/UI/HelpOverlay.cs`
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/SelfTest/Lists.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
- `docs/decisions.md` (Alt+Left/Right note-switch lock)

## Contracts
- New: Tab changes a list level only at text start; Ctrl+[ / Ctrl+] from anywhere.

## Approve
User, 2026-11-18: "Approve"

## For Codex
Role: terra, one rule across a few coupled files in `Editor/`. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then the Files; re-read before editing.
- Build exactly the Plan's "After" for the Files above; no drive-by changes. If the Findings are wrong, stop and report that.
- Build into `build/A26-tab-at-text-start-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, delete the folder. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A26-tab-at-text-start -Offline`.
- Never launch or kill AntiNotepad, never run `--quit`, never edit `.coord/STATUS.md` or this card. Offline checks only (no ports, servers, browser).
- Done means: `ListEditing.TabDecision` exists with selftests and a `CONTRACTS` row; PadView follows it; build 0 warnings, `SELFTEST PASS`, gate passes.
- Follow `.claude/rules/coding.md` (headers, summaries, no history comments). Keep new logic out of PadView beyond key mapping.

Report `.coord/reports/A26-tab-at-text-start.md`, short: 1 Summary; 2 Change list (`file:lines | what | Plan line`); 3 Behavior changes; 4 Out-of-scope files; 5 Plan deviations; 6 Contracts; 7 Lens review; 8 Checks run; 9 Hand checks for the user.

## Check
- Round 3 gate PASS (build 0 warnings, selftest, seam, tokens, format, scope, size). Rebased on dev (design hash re-taken after conflict).
- Codex review r2: rework, one major (renumber once per selected line, 6.9 s at 4,000 lines); fixed in r3 (renumber once per block, 2,000-line selftest). Not re-reviewed; the fix is contained in `DocumentCommands`/`ListEditing`.
- Bounds: Alt+Shift+arrows swapped for Ctrl+[ / Ctrl+] and `PadWindow.Keys.cs` edited: **justified**, user's words 2026-11-18. `docs/decisions.md` line: **justified**, user's "lock that in". `SelfTest/Design.cs`: **within plan** (help text).
- Misses: Findings said Alt+Shift+arrows were unbound (found at Build, cause plan, caught by Codex, should have caught by Findings searching `UI/` key routing, lesson: grep window-level key handlers for any new shortcut).
- Noticed: user's shortcut cleanup (Alt+Shift+arrows first/latest note, drop duplicates, empty rows, move-note key) is not an item; Ctrl+T appears twice in the help sheet.

## Hand test
Build: Red dev slot (AntiNotepad (dev), worktree build/dev, DATA build/testdata). User, 2026-11-18: "All passed"
