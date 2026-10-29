# A42c-checkbox-indent: move checkbox toggle and indent out of PadView, behavior-neutral

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Go before any build; Codex reads the whole card and
follows "For Codex". Codex never edits this file; Check and Hand test are filled by the sub-session afterwards.

Tier: Medium (raised from the plan row's Low at Request: not a verbatim move; it reshapes editor input commands (Tab, Ctrl+Enter, checkbox click) and their undo grouping) (rules: `docs/loop.md`)
Branch: `feat/A42c-checkbox-indent` in `.claude/worktrees/A42c-checkbox-indent`

## Request
Plan row Scope (user order, 2026-10-29): "Move checkbox indent logic out of `PadView` into its own Editor class,
behavior-neutral". Origin: A42-padview-commands re-audit, user's sequence 2026-10-26 (A42c optional, after A42b).
User, 2026-10-29: "Upgraded, start A42c". Screenshots: none.

## Findings
- **Kind and why:** polish/reorg, behavior-neutral. `PadView` still owns the checkbox toggle's text decisions and
  edits (`Editor/PadView.cs:657-676`) and the Tab indent edit (`:770-781`); they cannot be tested without typing in the app.
- **Now:** checkbox click (`Editor/Elements.cs:74`) and Ctrl+Enter (`PadView.cs:687-691`; Timer mode toggles the timer
  instead) call `ToggleCheckbox`: `[ ]` <-> `[x]`/`[X]` by changing one character; otherwise a `-*+` bullet becomes
  `[ ] `, or `[ ] ` is inserted. One edit, no explicit update group. Tab / Shift+Tab on a list line with no selection
  adds / removes up to two spaces, then `Renumber` runs on the post-edit caret line, both inside one `RunUpdate` (`:703`),
  so one Ctrl+Z undoes both. Caret is never set explicitly. The checkbox regex `CheckboxGenerator.Box`
  (`Elements.cs:46`) is also read by `Renderers.cs:409` and `Colorizers.cs:80`.
- **Reproduction (bugs):** not a bug; source trace of the three paths is the baseline (audit section 3).
- **Where:** `PadView.cs`, `ListEditing.cs` (exists since A42a, 19 lines), a new `DocumentCommands.cs`, the three regex
  readers. Downstream and untouched: per-note document/undo cache, NoteAnalysis (140 ms debounce over 1,500 lines),
  `Edited` -> host save timer, checkbox paint cache, timer Ctrl+Enter.
- **Risk:** splitting Tab's single undo group, reading the caret line too early, replacing a whole line instead of the
  small edit, or changing the regex groups that click hit-testing, painting and Done coloring depend on.
- **Source:** `.coord/reports/A42c-checkbox-indent-audit.md` (READY: yes, c04d5db). Tier Medium confirmed.

## Plan
- **After:** `ListEditing` decides each toggle/indent as one small edit (offset, length removed, text inserted), as pure
  code; new `DocumentCommands` applies that edit to the current line of a `TextDocument`; the checkbox regex moves to
  `ListEditing` unchanged (same groups) and its three readers point there. `PadView` keeps `ToggleCheckbox` as a thin
  facade, keeps all key routing, keeps the Tab `RunUpdate` with indent then `Renumber` inside it. You see no difference.
- **Must not change:** any text result, caret, selection, undo step, box look, Done strikethrough, timer Ctrl+Enter,
  fall-through to the editor's own Tab, modifier/mode handling (existing quirks kept as they are). `IsListLine`,
  `ContinueList`, `Renumber`, `Insert` stay in `PadView` (they are A42d-list-commands). `TextCommands` grammar untouched.
- **Scope questions:** None.
- **Test plan, code:** selftest checks: pure edit cases (`[ ]`, `[x]`, `[X]`, `[ ]task`, bullet with 1+ spaces, indented,
  plain, empty, numbered lines; indent/outdent with 0-3 spaces, tab-leading, nonzero offsets, LF and CRLF); real
  `TextDocument` checks for exact text and one Undo restoring each; regex groups unchanged for all three readers. LAYERS
  rows: existing `ListEditing` bans plus TextEditor/TextArea/TextView; `DocumentCommands` same bans with `TextDocument`
  allowed; one added guard proven to fail on a temporary real reference. New CONTRACTS row `checkbox-indent-preserved`
  (the checks above; combined Tab indent+renumber and caret stay manual).
- **Test plan, UI (hand test in the dev copy):**
  1. Click and Ctrl+Enter on `[ ]task`, `[X] task`, `- [ ] task` and a plain `- task`: same text, box, strikethrough as
     before; one Ctrl+Z undoes each.
  2. Click a box away from the caret: caret and selection stay where they were.
  3. Timer note: Ctrl+Enter still starts/stops the timer.
  4. Tab / Shift+Tab on a bullet, a checkbox and a nested numbered list: two spaces per step, a fresh nested level starts
     at 1, 9 -> 10 lines up; one Ctrl+Z undoes indent and renumber together.
  5. Tab on a plain line and with a selection: the editor's normal Tab, as before.
- **Gates:** `tools/gate.ps1` (build 0 warnings, selftest, LAYERS incl. new rows, CONTRACTS, seam, scope = Files, size:
  PadView should shrink); Codex fresh lens review (undo grouping, caret timing, edit offsets, missed regex readers, no new
  state); new CONTRACTS row `checkbox-indent-preserved`; hand test (caret, undo, click geometry). No reproduction test
  (not a bug), no lab build (nothing visible changes), no design review (Medium).
- **Needs from you:** go, then the hand test.

## Files
Only these files may change (exact paths; `tools/gate.ps1` reads this list). Any other file needs a line under
"Out-of-scope files" in the report.
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/Editor/ListEditing.cs`
- `product/AntiNotepad/Editor/DocumentCommands.cs` (new)
- `product/AntiNotepad/Editor/Elements.cs`
- `product/AntiNotepad/Editor/Renderers.cs`
- `product/AntiNotepad/Editor/Colorizers.cs`
- `product/AntiNotepad/SelfTest.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`

## Contracts
- All existing rows keep passing (11 rows: 10 pass, 1 manual).
- New `checkbox-indent-preserved`: checkbox toggle and Tab/Shift+Tab indent produce the same text and one Undo step as
  before the move; combined indent+renumber, caret and click geometry are manual.

## Go
User, 2026-10-29: "Lets do A42c then A42d and finally C29" (go on the plan as written).

## For Codex
Role: director (gpt-6.1-sol medium), delegating to a worker (gpt-6-luna xhigh). Product `product/AntiNotepad`. The branch is already
checked out: never switch, merge or commit (you cannot write to `.git`). Read `AGENTS.md`, then the files above. Line
numbers are from the current tree; re-read before editing.

Constraints:
- Build exactly the Plan's "After" for the Files above. No drive-by refactors, renames, icon/token/style edits outside it.
- If the Findings turn out wrong (the bug does not reproduce, the cause is elsewhere, the plan cannot work), stop and
  report that instead of building something else.
- Build into `build/A42c-checkbox-indent-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A42c-checkbox-indent -Offline`; the manager
  re-runs it online.
- Never launch or kill AntiNotepad. Never run `--quit`. Never edit `.coord/STATUS.md` or this card.

Done means:
- `ListEditing` owns the pure toggle/indent edit decisions and the checkbox regex (groups unchanged); `DocumentCommands`
  applies one edit to a `TextDocument` line; `PadView` keeps the facade, key routing and the Tab `RunUpdate` + `Renumber`.
- Elements, Renderers and Colorizers read the regex from `ListEditing`; no alias left behind.
- New selftest checks, LAYERS rows (one guard proven with a temporary reference, then reverted) and CONTRACTS
  `checkbox-indent-preserved` exist and pass.
- The Plan's code tests exist and pass; build 0 warnings, `SELFTEST PASS`, the gate passes.

Before you report (director only): spawn the `lens_reviewer` agent on your diff (it reads
`.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why not. Workers without spawning: run the same
checklist yourself and say so.

Report: `.coord/reports/A42c-checkbox-indent.md`, short, with exactly these sections:
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
<Filled by `/review-task`: gate result lines, review verdicts, then the bounds check, one line per difference between
Request, Plan and done: within plan | justified (why) | ask the user. "Noticed": side bugs as proposed follow-ups.>

## Hand test
<Filled by the sub-session: the build checked (dev badge text), and the user's result, verbatim.>
