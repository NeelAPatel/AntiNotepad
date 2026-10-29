# A53-split-divider-height: split-view divider runs to the window's bottom edge

Role: director (gpt-6.1-sol medium), workers gpt-6-luna xhigh. Product `product/AntiNotepad`.
Branch: `feat/A53-split-divider-height` in `.claude/worktrees/A53-split-divider-height` (already checked out; never switch,
merge or commit: you cannot write to `.git`).
Tier: Low (one theme token and its layout use; visible, so the user hand-tests; rules: `docs/loop.md`)
Read first: `AGENTS.md`, then the files below. Line numbers are from the current tree; re-read before editing.

## Goal
User report: in split view the divider between the two panes stops short of the bottom edge. Cause: the divider's bottom
margin reserves the pager strip, `DividerBottomMargin = PagerStripHeight + DividerPagerClearance`
(`Theme/ChromeTokens.cs:70`, used at `UI/PadWindow.Layout.cs:31`), even though the pager layer is hidden most of the time
(`UI/PadWindow.Layout.cs:37`, `IsVisible = false`).
Wanted: the divider runs from its current top to the window's bottom edge. When the pager pill shows, it draws on top of
the divider (pill above divider in z-order), so the pill stays clean.
Must not change: divider width, color/alpha, top margin, pane widths and split behavior, the pager pill, toast position
(`ToastBottomMargin` keeps its own value), and anything outside split view.

## Files
Only these files may change. Touching any other file needs a line under "Out-of-scope files" in the report.
- `product/AntiNotepad/Theme/ChromeTokens.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/SelfTest.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`

## Contracts
Existing rows to keep passing: all current rows (`window-style-bits` in particular).
New row (this fix's promise): `split-divider-full-height`: in split view the divider's bottom margin is 0 (it reaches the
panes' bottom edge) and the pager layer is above the divider.

## Constraints
- Smallest change that meets the goal. No drive-by refactors or other token edits.
- If `DividerPagerClearance` becomes unused, remove it; keep `PagerStripHeight` (other users).
- Build into `build/A53-split-divider-height-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A53-split-divider-height -Offline`.
- Never launch or kill AntiNotepad. Never run `--quit`.

## Done means
- The divider reaches the bottom edge in split view; the pager pill draws over it.
- `CONTRACTS` has `split-divider-full-height` passing.
- Build 0 warnings, `SELFTEST PASS`, the gate passes.

## Before you report (director only)
Spawn the `lens_reviewer` agent on your diff (it reads `.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why
not.

## Report: `.coord/reports/A53-split-divider-height.md`
Keep it short. Use exactly these sections:
1. **Summary**: two or three sentences.
2. **Change list**: `file:line-range | what changed | which goal line it serves`.
3. **Behavior changes**: every visible or layout change. "None" only if true.
4. **Out-of-scope files**: file and why, or "None".
5. **Contracts**: rows added or changed.
6. **Lens review**: findings and what you did with each.
7. **Checks run**: build, selftest, gate result lines.
8. **Hand checks for the user**: what to look at in the dev copy (split view, pager showing and hidden, light and dark).
