# A38-version-0-7-1: stamp the app as 0.7.1

Item card (`docs/loop.md`). Master 7 fills Request, Findings, Plan and Approve; the item chat edits the one line itself (no Codex run) and fills Check and Hand test.

Tier: Low · fast lane (one property file, no visible change)
Branch: `chore/A38-version-0-7-1` in `.claude/worktrees/chore/A38-version-0-7-1` (created at Approve, from dev's tip)

## Request
User via Master 7, 2026-11-22: "Start 0.7.1". Dev now holds the whole A35-ux-pass (merge 186cfca: data-loss fixes, icon registry, labels, spacing tokens, targets and keyboard, contrast, design contract) plus the T20 installer fix. This item makes the app report `0.7.1`. Master 7 then moves `main` and tags `v0.7.1` only on the user's explicit go, and the user installs with `tools/install.ps1`. Model: A37-version-0-7-0 (`.coord/tasks/A37-version-0-7-0.md`).

## Findings
- **Kind and why:** process (release preparation).
- **Now:** `product/AntiNotepad/AppInfo.props` has `AppVersion` 0.7.0 and an empty `AppVersionLabel`, so the informational version reads `0.7.0`.
- **Where:** `AppInfo.props` only. Master 7 grepped `product/` and `tools/` sources: no other literal `0.7.0` outside obj and bin. Re-check docs that quote the version.
- **Risk:** none visible.

## Plan
- **After:** `AppVersion` is `0.7.1`; label stays empty; the built exe reports `0.7.1`. Nothing else changes.
- **Must not change:** every other property, behavior, look, file and setting.
- **Test plan:** Release build 0 warnings, `--selftest` prints `SELFTEST PASS`, the built exe's ProductVersion reads `0.7.1` (read the file, never run the exe otherwise, never `--quit`). UI: None (not visible).
- **Needs from you:** nothing.

## Files
- `product/AntiNotepad/AppInfo.props`

## Contracts
None.

## Approve
User, 2026-11-22: "Start 0.7.1".

## For Codex
No Codex run: a one-line edit, done by the item chat itself.

## Check
- Method: `AppInfo.props` `AppVersion` 0.7.0 to 0.7.1; label stays empty, so `InformationalVersion` is `0.7.1`. Re-grep of docs and tools: only generic `v0.7.0` examples (dev-loop, glossary) and the a35d lab mockup remain; none need to follow.
- Release build: 0 warnings, 0 errors; `--selftest`: `SELFTEST PASS` (own data folder); built exe ProductVersion `0.7.1`, FileVersion `0.7.1.0` (read, not run); build folder deleted.
- Gate (`pwsh tools/gate.ps1 -Task A38-version-0-7-1`, online, clean tree): GATE: PASS (build, selftest, seam, tokens, contract, format, scope, size, cleanup PASS; LoopLint skipped).
- Bounds check: within plan; one file (`AppInfo.props`), one code commit.
- Misses: None

## Hand test
None (not visible).
