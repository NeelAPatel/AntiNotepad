# A12-version-rc3: stamp the app as 0.7.0-rc3

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build; the sub-chat edits the one line itself (no Codex run) and fills Check and Hand test.

Tier: Low · fast lane (one property file, no visible change) (rules: `docs/loop.md`)
Branch: `chore/A12-version-rc3` in `.claude/worktrees/chore/A12-version-rc3` (created at Approve)

## Request
2026-11-12 (user): "make a simple release to main. then run dev." and, asked for the name: "0.7.0-rc3". Context: `v0.7.0-rc3` is about to be cut from dev (adds A9-pinned-accent-ring and A10-hotkey-recorder since rc2). The app stamps `0.7.0-rc2`. Same job as the earlier A7-version-rc2.

## Findings
- **Kind and why:** process (release preparation); the stamped version label would be wrong for the release.
- **Now:** `product/AntiNotepad/AppInfo.props` has `AppVersion` 0.7.0 and `AppVersionLabel` rc2 (from A7-version-rc2), so the informational version reads `0.7.0-rc2`.
- **Reproduction (bugs):** not a bug.
- **Where:** `AppInfo.props` only; check with grep that `rc2` is not hard-coded anywhere else in `.cs`, `.props`, `.csproj`, `.ps1`, `.sh` and docs the build reads.
- **Risk:** none visible; a Release build and selftest confirm it compiles.
- **Source:** master scout, from the A7-version-rc2 card.

## Plan
- **After:** `AppVersionLabel` is `rc3`; the informational version reads `0.7.0-rc3`. Nothing else changes.
- **Expected time:** under 20 minutes, request to merge.
- **Must not change:** every other property, behavior, look, file and setting.
- **Scope questions:** None
- **Test plan, code:** none new. A Release build prints 0 warnings and `--selftest` prints `SELFTEST PASS`.
- **Test plan, UI:** None (not visible).
- **Gates:** build 0 warnings, selftest (the tier's defaults).
- **Needs from you:** nothing.

## Files
- `product/AntiNotepad/AppInfo.props`

## Contracts
None.

## Approve
2026-11-12 (user): "0.7.0-rc3" (the version name), after the director proposed this stamp item first, then the release.

## For Codex
No Codex run: a one-line edit, done by the sub-chat itself.

## Check
(pending)

## Hand test
None (not visible).
