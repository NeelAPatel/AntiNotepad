# A36-version-rc4: stamp the app as 0.7.0-rc4

Item card (`docs/loop.md`). Master 7 fills Request, Findings, Plan and Approve; the item chat edits the one line itself (no Codex run) and fills Check and Hand test.

Tier: Low · fast lane (one property file, no visible change)
Branch: `chore/A36-version-rc4` in `.claude/worktrees/chore/A36-version-rc4` (created at Approve, from dev's tip)

## Request
User via Master 7, 2026-11-20: "okay correction. 0.7.0-rc4 is the tag and make a 0.7.0 full release". Dev is about to be released as `v0.7.0-rc4`, then as plain `v0.7.0` (a second item, A37-version-0-7-0, follows after this merges and rc4 is tagged). Same job as A12-version-rc3 (`.coord/tasks/A12-version-rc3.md`).

## Findings
- **Kind and why:** process (release preparation); the stamped label would be wrong for the release.
- **Now:** `product/AntiNotepad/AppInfo.props` has `AppVersion` 0.7.0 and `AppVersionLabel` rc3, so the informational version reads `0.7.0-rc3`.
- **Where:** `AppInfo.props` only. Grep that `rc3` is not hard-coded elsewhere the build reads (`.cs`, `.props`, `.csproj`, `.ps1`, docs).
- **Risk:** none visible.

## Plan
- **After:** `AppVersionLabel` is `rc4`; the informational version reads `0.7.0-rc4`. Nothing else changes.
- **Must not change:** every other property, behavior, look, file and setting.
- **Test plan:** none new. Release build 0 warnings, `--selftest` prints `SELFTEST PASS`. UI: None (not visible).
- **Needs from you:** nothing.

## Files
- `product/AntiNotepad/AppInfo.props`

## Contracts
None.

## Approve
User, 2026-11-20: "0.7.0-rc4 is the tag" (the version name).

## For Codex
No Codex run: a one-line edit, done by the item chat itself.

## Check
- Gate (`pwsh tools/gate.ps1 -Task A36-version-rc4`, online): GATE: PASS (build PASS 0 warnings 0 errors, selftest PASS, seam PASS, tokens PASS, format PASS, scope PASS, size PASS, cleanup PASS; LoopLint skipped, no loop files).
- Selftest: `SELFTEST PASS`.
- Bounds check: within plan; only `AppVersionLabel` rc3 to rc4 in `AppInfo.props`; grep found no other `rc3` in `product/` or `tools/` sources (only generated obj files). One code commit, so no commit review.
- Misses: None

## Hand test
None (not visible).
