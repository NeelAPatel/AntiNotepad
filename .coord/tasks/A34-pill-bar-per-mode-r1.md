# A34-pill-bar-per-mode-r1: pinned design snapshot update

Role: quick (a one-constant change plus a diff read; docs/agents.md). Work only in this worktree. Never commit, never launch AntiNotepad, no ports. `product/AntiNotepad/SelfTest/Design.cs` is now in the card's `## Files`.
Set `AVALONIA_TELEMETRY_OPTOUT=1`.

## Why
Run 1 built A34 (see `.coord/reports/A34-pill-bar-per-mode.md`). Selftest fails only on the pinned design dump hash: the new Settings rows and the new bar controls legitimately change the dump.

## Do
1. Build the current branch into `build/A34-check` and write its dump: `AntiNotepad.exe --design-dump build/A34-new-dump.txt`.
2. Build the baseline (dev tip, before A34) the same way: `git archive dev product | tar -x -C build/A34-baseline-src` (run `git archive` only; reading `.git` is fine, writing it is not), build it into `build/A34-base`, and write `build/A34-base-dump.txt`.
3. Diff the two dumps. Write the diff summary into your final message grouped as: expected (new Settings rows Show Delete button, Show New note button, Slot dots; the new bar containers, corner buttons, swap button; count button height 32; dot button height 32; pill content) and UNEXPECTED (anything else: a changed value on an existing control, a removed control, a moved style). If anything is UNEXPECTED do not change the hash; list it and stop.
4. Only when every difference is expected: set `DesignHash` in `SelfTest/Design.cs` to the new dump's hash, rerun `--selftest` and `pwsh -NoProfile -File tools/gate.ps1 -Task A34-pill-bar-per-mode -Offline`.
5. Delete `build/A34-check`, `build/A34-base`, `build/A34-baseline-src` and the dumps (keep the dump diff text in `.coord/reports/A34-pill-bar-per-mode-r1.md` instead).

## Done means
Selftest and gate pass offline, or you stopped at step 3 with the UNEXPECTED list. Final message 6 lines max.
