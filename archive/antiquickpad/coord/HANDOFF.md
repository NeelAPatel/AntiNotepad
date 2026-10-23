# HANDOFF (overwritten at every pause or manager swap)

For: the Opus manager session running the UI overhaul (M8-antinote-ui-overhaul), resuming after a context compaction. Written 2026-10-22.

## Read first
AGENTS.md, .coord/STATUS.md, docs/feedback/TRACKER.md, docs/decisions-log.md (entries from 2026-10-21 on), docs/function-contract.md (Commands, Ownership), docs/design/tokens.md.

## State (branch feat/ui-overhaul, all committed, nothing merged to dev)
- Baseline tag `baseline/m8c-v1-compact` (user-approved V1 look). After it: F54-measure-heavy-index-crash, M6c-design-tokens (Tokens/quiet.tokens.json drives the shell; contrast/targets/names/focus ring), F57-pin-state-visual, M11-settings-window (M11a function + M11b UI; More menu, menu.toggle and shell.quiet retired; `settings:` route kind). User accepted M11 "fine for now"; F59 = later navigation-view settings overhaul.
- Gate: Release build 0 warnings, `--selftest` PASS (BOUNDARY, CONFORMANCE quiet 31 commands, REVEAL, PRESENTATION, TOKENS, M11 SETTINGS, F37 hidden windows). Set ANTIQUICKPAD_ALIGNMENT_OUTPUT to a scratch path or the selftest rewrites docs/design/m6/built/T015.
- Last --measure (after M8c): baseline median 29.97 ms (gate 30). Re-run needs the user's OK (opens a window, sends Ctrl+Alt+F9).

## Next
1. M6d-motion-system (order A): spring/interruptible motion; folds F5, F21, F22, F23, F33, F45, F47. Must not hurt show-to-focus.
2. Open items: F58-keyboard-reach-bars (needs user call), F52 re-checks (F31, F39, F46, F49), M7 data folder, F44 undo-delete, F55 note loading (Later), F59 settings overhaul (Later).

## Working rules learned this phase
- User wants Claude usage first (not Codex) until their Claude reset; manager does the work directly.
- Codex sandbox cannot read the screen; worker tier failed on compile errors; budget tags used: day-2026-10-21 (all), eve-2026-10-21 (director 2/3).
- Python edits through heredocs mangle `\uXXXX` escapes in C# strings; write such lines with the Edit tool or a raw-string script file.
- After each TRACKER.md change: `python .coord/tools/tracker-sync.py`, then ArtifactData set `tracker/current` (current db version 17) on https://claude.ai/artifact/Q4k56bd74Y4bmtCMLwoG5X, commit docs/tracker-board.html.
- Relaunch the pad after gates (`--quit` closes it): `Start-Process .\bin\Release\net10.0\AntiQuickPad.exe` from src/AntiQuickPad.

## User preferences
Concise structured answers, say what needs them last; physical checks of anything visual (give a test log file); name items `<ID>-<slug>`; local git only, merge to dev only on their say-so.
