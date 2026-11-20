# A33-help-toggle-f1: F1 opens and closes Help

Item card (`docs/loop.md`). Claude fills Request to Approve before any build; Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB (logs go in reports).

Tier: Low
State: plan ready
Branch: `feat/A33-help-toggle-f1` in `.claude/worktrees/feat/A33-help-toggle-f1` (created at Approve, from dev's tip)

## Request
User via Master 7, 2026-11-20: start `idea: help-toggle-f1` (from the A29 work, 2026-11-19).
- **Goal:** pressing F1 while Help is open closes it (today F1 only opens it; Esc closes).
- **Done check:** (1) F1 opens Help, F1 again closes it; (2) Esc still closes; (3) F1 with Search or Settings open behaves as today; (4) the key goes through A29's shortcut table (`UI/Shortcuts.cs`, `docs/shortcuts.md`), so Settings > Shortcuts rebinding still works and the help sheet and docs table stay in sync; (5) a selftest covers the toggle; (6) gate passes.
- **Findings first (short):** how Help opens and closes today in `UI/PadWindow.Keys.cs` and `UI/HelpOverlay.cs`; whether the Help overlay itself receives keys while open. A29 added a shortcut table with a drift selftest: use it, add no second key path.
- **Constraints:** Codex offline only; follow `.claude/rules/coding.md`; hand test in your slot's dev copy only, never the installed copy. A30 stays an open live check on the backlog: do not touch save or shutdown code.

## Findings
Tier: Low (docs-tools if anything). **The goal is already met on dev**, built by A29-shortcut-map (commit fa982a5, 2026-11-19).
- Now: `UI/Shortcuts.cs` has the `help` row (F1, Pad scope) -> `ShortcutAction.Help`; `PadWindow.Keys.cs` `DispatchShortcut` calls `ToggleHelp()` (closes if `_overlay is HelpOverlay`, else opens).
- The overlay does not need to receive keys: the window listens with a Tunnel `KeyDown` handler (`PadWindow.cs:108`) and, while an overlay is open, still dispatches Help, Search and Settings keys (`Keys.cs:36`).
- Esc: handled first (`Keys.cs:24`), closes the overlay. Unchanged.
- Search open + F1: `ToggleHelp` opens Help in its place, as before. Settings is its own window, outside this handler.
- Rebinding: the toggle runs off the table action, so a user-bound Help key also toggles. Docs: `docs/shortcuts.md` says "Help toggles with F1 or its assigned key".
- Tests: `SelfTest/UI.cs:393-415` covers F1 open, close, reopen, and a rebound key closing and reopening. Contracts row at `SelfTest/Contracts.cs:243`.
- Evidence: Release build + `--selftest` on dev tip prints SELFTEST PASS (build/A33-check, this chat).

## Plan
Done-check map: (1) met, (2) met, (3) met, (4) met, (5) met, (6) gate not needed, no code change.
- **After = now.** Nothing to build. Files: none.
- Gap only: no selftest line for "F1 with Search open replaces it with Help" (done check 3). Optional add of one Expect in `SelfTest/UI.cs` (~4 lines); not required.
- Recommend: **drop** the item (already delivered by A29-shortcut-map), clear the pending row. Option B: approve the one-line selftest addition.

## Approve
User, 2026-11-20 (verbatim): "oh wtheck, F1 is already done yeah you're right, just add the self test and call it a day"
Scope: test-only. One Expect in `SelfTest/UI.cs`: F1 with Search open swaps in Help. No product code.

## Files
- `product/AntiNotepad/SelfTest/UI.cs`

## Check
2026-11-20: diff is 9 lines in `SelfTest/UI.cs` only (Files matches plan). `tools/gate.ps1 -Task A33-help-toggle-f1`: GATE PASS, SELFTEST PASS incl. the new "F1 with Search open swaps in Help". Test-only, no UI change: hand test skipped. Tier Low, accepted.
