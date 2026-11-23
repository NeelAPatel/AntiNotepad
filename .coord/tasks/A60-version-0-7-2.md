# A60-version-0-7-2: stamp the app as 0.7.2

Item card (`docs/loop.md`). Done inside the D5-public-prep worktree on the owner's word; one property file, no Codex run.

Tier: Low · fast lane (one property file, no visible change)
Branch: `D/D5-public-prep` (commit ahead of the public-prep checkpoint; no separate branch)

## Request
User, 2026-11-23: "we can bump up to 0.7.2". The first public portable release needs its own tag: `v0.7.1` points to code from before the README, LICENSE and release kit existed, so a zip built from it carries no license. This item makes the app report `0.7.2`. Model: A38-version-0-7-1.

## Findings
- **Kind and why:** process (release preparation).
- **Now:** `product/AntiNotepad/AppInfo.props` has `AppVersion` 0.7.1 and an empty `AppVersionLabel`.
- **Where:** `AppInfo.props` only; `tools/package-release.ps1` reads the version from there and requires the tag to match.
- **Risk:** none visible.

## Plan
- **After:** `AppVersion` is `0.7.2`; label stays empty; the built exe reports `0.7.2`. Nothing else changes.
- **Must not change:** every other property, behavior, look, file and setting.
- **Test plan:** build 0 errors, `--selftest` prints `SELFTEST PASS` with its own data folder, the packaged zip is named `AntiNotepad-0.7.2-win-x64.zip`. UI: None (not visible).

## Files
- `product/AntiNotepad/AppInfo.props`

## Contracts
None.

## Approve
User, 2026-11-23: "we can bump up to 0.7.2".

## For Codex
No Codex run: a one-line edit.

## Check
See the commit message and the closing report of the D5-public-prep chat.

## Hand test
None (not visible).
