# A37-version-0-7-0: stamp the app as plain 0.7.0

Item card (`docs/loop.md`). Master 7 fills Request, Findings, Plan and Approve; the item chat edits the property file itself (no Codex run) and fills Check and Hand test.

Tier: Low · fast lane (one property file, no visible change)
Branch: `chore/A37-version-0-7-0` in `.claude/worktrees/chore/A37-version-0-7-0` (created at Approve, from dev's tip)

## Request
User via Master 7, 2026-11-20: "make a 0.7.0 full release", and "as in whatever code we have now for the program, congrats we finished 0.7.0". `v0.7.0-rc4` is tagged on main (fb721cb). This item makes the app report plain `0.7.0` for the full release tag `v0.7.0`. Model: A36-version-rc4.

## Findings
- **Now:** `AppInfo.props` has `AppVersion` 0.7.0, `AppVersionLabel` rc4, and `InformationalVersion` = `$(AppVersion)-$(AppVersionLabel)`. An empty label would read `0.7.0-` (trailing dash).
- **Check first:** how the v0.5.0 plain release handled it (git log of `AppInfo.props`, `4483f32`, earlier tags), and every reader of `InformationalVersion` / `AppInfo` (About text, `tools/install.ps1`, `tools/dev.ps1`, selftests, docs). Choose the smallest change so the plain version reads `0.7.0` with no dash and rc labels still work later.
- **Risk:** low; a Release build and selftest confirm.

## Plan
- **After:** built app reports `0.7.0` (no label, no dash). Nothing else changes.
- **Must not change:** other properties, behavior, look.
- **Test plan:** Release build 0 warnings, `--selftest` `SELFTEST PASS` (never run the exe otherwise, never `--quit`); a read of the built exe's ProductVersion shows `0.7.0`. UI: None (not visible).
- **Needs from you:** nothing.

## Files
- `product/AntiNotepad/AppInfo.props`
- `product/AntiNotepad/UI/SettingsWindow.cs` (About row would read `0.7.0 ()` with an empty label; one line)

## Contracts
None.

## Approve
User, 2026-11-20: "make a 0.7.0 full release".

## For Codex
No Codex run: a small property edit, done by the item chat itself.

## Check
- Method: `AppInfo.props` now sets `AppVersionLabel` empty; `InformationalVersion` defaults to `$(AppVersion)` and gets `-$(AppVersionLabel)` only when the label is non-empty, so an rc label still works later. `AppInfo.Version/Label` and the selftest identity check already handled an empty label; `tools/install.ps1` reads only `AppVersion`. The About row (`SettingsWindow.cs:690`) printed `({Label})`; it now omits the parentheses when the label is empty.
- Release build: 0 warnings, 0 errors; `--selftest`: `SELFTEST PASS`; built exe ProductVersion `0.7.0`, FileVersion `0.7.0.0`; build folder deleted.
- Gate (`pwsh tools/gate.ps1 -Task A37-version-0-7-0`): GATE: PASS (build, selftest, seam, tokens, format, scope, size, cleanup PASS; LoopLint skipped).
- Bounds check: within plan; two files only (props and the one About line), both listed in Files; one code commit, so no commit review.
- Misses: None

## Hand test
None (not visible).
