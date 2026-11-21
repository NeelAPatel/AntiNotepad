# T20-install-selftest-root: installer self-check starts in the repo folder

Item card (`docs/loop.md`). Master 7 fills Request, Findings, Plan and Approve; the item chat builds (small, no Codex run) and fills Check and Hand test.

Tier: Low · fast lane (one tool script, no visible change)
Branch: `chore/T20-install-selftest-root` in `.claude/worktrees/chore/T20-install-selftest-root` (created at Approve, from dev's tip)

## Request
User via Master 7, 2026-11-20: after `v0.7.0` was cut, `tools/install.ps1` failed twice and rolled back to rc3. User chose: "Installer runs the check from the repo folder (Recommended)".
- **Goal:** `tools/install.ps1` runs the installed copy's `--selftest` with the repo root as working directory, so the two repo-reading checks pass.
- **Done check:** (1) the installer's selftest step passes on the current dev; (2) installing still never runs the app against real data or `--quit` without the guard; (3) rollback behavior unchanged.
- **Constraints:** never touch the installed copy (`%LOCALAPPDATA%\Programs\AntiNotepad`) and never run `install.ps1` without `-InstallRoot` / `-DryRun`. The user runs the real install.

## Findings (Master 7 read, 2026-11-20; confirm first)
- **Now:** `install.ps1` line 83 sets `WorkingDirectory` to the exe's folder for every call (`New-CleanAppProcessInfo`), including the selftest at line 254. Two checks walk up from `AppContext.BaseDirectory`, then `Environment.CurrentDirectory`, looking for `third_party/AvaloniaEdit/UPSTREAM.txt` (`SelfTest/Contracts.cs:299-313`, check `avaloniaedit-fork-documented`) and `docs/shortcuts.md` (`SelfTest/UI.cs:~350`, `shortcut-map`). An installed folder has no repo above it, so both fail: `SELFTEST FAIL`, rollback. Gate passes because it runs the exe in the repo.
- **Fix shape:** give the selftest call (only) a working directory of the repo root (the script's parent of `tools/`); other calls keep the exe folder. Smallest change; no product code.
- **Risk:** low. Check the `--quit` and launch calls keep the exe folder.

## Plan
- **After:** the selftest step in `install.ps1` starts in the repo root.
- **Test plan:** `pwsh -NoProfile -File tools/install.ps1 -DryRun`; and a real run of the script with `-InstallRoot <temp folder under build/>` (test mode, skips process checks and registry) that must reach `SELFTEST PASS` and finish; delete the temp folder. If the script has its own check mode (`-Check`), run it.
- **Needs from you:** the real install, in a normal terminal, after merge (hand test).

## Files
- `tools/install.ps1`

## Contracts
None.

## Approve
User, 2026-11-20: "Installer runs the check from the repo folder (Recommended)".

## For Codex
No Codex run: a small script edit, done by the item chat itself.

## Check
- Findings confirmed: `New-CleanAppProcessInfo` set the working directory to the exe folder for every call; two selftest contracts look for repo files from there.
- Change: `New-CleanAppProcessInfo` and `Invoke-InstalledCommand` take an optional working directory (default: exe folder); only the `--selftest` call passes `$repoRoot`. The `--quit` and launch calls are untouched.
- Proof (test mode, never the installed copy): `install.ps1 -DryRun` ok; `install.ps1 -InstallRoot build/A16-staged-upgrade-rollback-t20` reached `SELFTEST PASS` (CONTRACTS 41 rows, 40 pass, 1 manual) twice, exit 0; temp folders deleted. One earlier run failed `shortcut-map` with a file lock on `index.json.tmp` (not a path error; the two target checks passed in it); two reruns passed.
- Gate (`pwsh tools/gate.ps1 -Task T20-install-selftest-root`, on the committed tip): GATE: PASS (seam, tokens, format, scope, loop-lint, size, cleanup PASS; loop-lint budget warnings on docs/loop.md and lessons.md are pre-existing).
- Bounds check: within plan; one file changed, `tools/install.ps1`. Rollback code untouched. No Codex run, so no commit review.
- Noticed: `shortcut-map` can flake on an `index.json.tmp` lock (seen once); worth a watch if the real install hits it, rerun clears it.
- Misses: None

## Hand test
User runs `pwsh -NoProfile -File tools/install.ps1` after the merge; it must finish and the installed ProductVersion must read `0.7.0`.
