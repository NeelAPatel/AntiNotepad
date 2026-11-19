# A32-selection-convert-menu: right-click Convert to list on selected lines

Item card (`docs/loop.md`). Claude fills Request to Approve before any build; Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB (logs go in reports).

Tier: Medium · editor-feature
State: approved
Branch: `feat/A32-selection-convert-menu` in `.claude/worktrees/feat/A32-selection-convert-menu`

## Request
User via Master 7, 2026-11-19 (idea `selection-convert-menu`): select lines, right-click, Convert to list, then Checklist, Bullet list or Numbered list; each selected line gets the matching prefix. Done check: submenu only with a selection; every choice prefixes the lines; one Undo reverses it; existing entries unchanged; selftests cover prefixing, blanks, already-prefixed lines; gate passes. Constraint: do not touch save/shutdown code (A30 open).

## Findings
Own scout, 2026-11-19.
- **Now:** prefix grammar is `ListEditing.ListPrefix` (checkbox `[ ] `/`- [ ] `, bullets `-*+`, numbers `N.`/`N)`); `list`/`todo` keywords in `Model/Modes.cs` (checklist only). `PadView.RequestMenu()` builds a flat `MenuEntry` list shown by `UI/PadMenu.cs`: no submenu support exists. Only context handler is `TextArea.ContextRequested`; no key conflicts.
- **Pattern:** `DocumentCommands.IndentListLines` groups edits with `RunUpdate` (one undo); `RenumberBlock` ends a block at any blank line.
- **A31 link:** both use three kinds, Checklist/Bullet/Numbered; shared `ListKind` enum in `ListEditing.cs`, second merger adopts it.

## Plan
- **After:** with a selection, right-click shows "Convert to list ›" opening Checklist, Bullet list, Numbered list. Each non-blank selected line gets that prefix.
- **Decisions (user, 2026-11-19):** submenu; existing prefixes swapped (indent kept); blank lines skipped; numbered obeys existing renumber (blank line splits runs, each restarts at 1, indent levels separate); timer and other edge cases near the end of this item.
- **Must not change:** existing menu entries, save/shutdown code, typing speed, line 1 mode keyword.
- **Test plan:** selftests: each kind, swap of `[x]`/`-`/`3.`, blanks skipped, blank splits numbering, indent kept, keyword line 1 untouched, one undo, no entry without selection. `CONTRACTS` row. Hand: dev slot, select mixed lines with a blank, convert each kind, one Ctrl+Z.
- **Gates:** `tools/gate.ps1` (build, selftest, format, tokens) + Codex review.
- **Needs from you:** hand test; later timer/edge cases.

## Files
- `product/AntiNotepad/Editor/ListEditing.cs`
- `product/AntiNotepad/Editor/DocumentCommands.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/Editor/MenuEntry.cs`
- `product/AntiNotepad/UI/PadMenu.cs`
- `product/AntiNotepad/SelfTest/Lists.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`

## Contracts
- Add: Convert to list is one undo step and skips blank lines.

## Approve
User, 2026-11-19: "Approve" (to "Approve this plan and the lab mock?"; lab mock shown in chat).

## For Codex
Role: terra, because a few files, one new submenu and pure line logic. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then the Files; re-read before editing.
- Build exactly the Plan's "After"; new pure `ListEditing.ConvertLine`, `DocumentCommands.ConvertToList` (in `RunUpdate`, then `RenumberBlock` with converted lines fresh); `MenuEntry` gains an optional child list rendered by `PadMenu` as a hover/keyboard submenu (Right opens, Left/Esc closes). No drive-by changes.
- Build into `build/A32-selection-convert-menu-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, delete the folder. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A32-selection-convert-menu -Offline`.
- Never launch or kill AntiNotepad, never edit STATUS or this card. Offline checks only.
- Done means: submenu code and conversion exist, Plan tests pass; build 0 warnings, `SELFTEST PASS`, gate passes.

Report `.coord/reports/A32-selection-convert-menu.md`: 1 Summary; 2 Change list; 3 Behavior changes; 4 Out-of-scope files; 5 Plan deviations; 6 Contracts; 7 Lens review; 8 Checks run; 9 Hand checks.

## Check
Round r2 (2026-11-19). Gate -Offline: PASS (build 0 warnings, SELFTEST PASS, 35 contracts: 34 pass, 1 manual). Plain gate fails only on NU1900 offline NuGet audit (env). Codex review r1: rework, 1 major (submenu stayed open on sibling hover), fixed in r2; its 11-check probe passed (selection edges, CRLF, blank-split numbering, one undo). Bounds: within plan; adjacent unselected numbered lines renumber per the existing block rule (justified; Undo restores). Formatter run on PadMenu.cs after the gate Format fail. r2 not re-reviewed by Codex: hand test covers popup behavior. Misses: format failure got past Codex; should have run dotnet format in the brief.

## Hand test
User, 2026-11-19: "Tests passes" (r1); icons tried and dropped; final r5 menu: "looks good lock this in".
Check update: r3 shared popup window setup fixed the submenu corners; r4/r5 icon attempts removed by the user's word (text-only submenu). Final gate -Offline PASS.
