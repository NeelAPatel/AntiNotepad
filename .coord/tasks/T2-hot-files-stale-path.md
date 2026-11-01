# T2-hot-files-stale-path: drop the stale `SelfTest.cs` name from loop hot-file lists and the card template

Item card (`docs/loop.md`), fast lane (side task, `docs/loop.md` "Rules").

Tier: Low · docs-tools (rules: `docs/loop.md`)
Branch: `chore/T2-hot-files-stale-path` in `.claude/worktrees/chore/T2-hot-files-stale-path` (created at Approve)

## Request
Master 2 noticed (2026-11-01, STATUS `idea: hot-files-stale-path`): after A1-selftest-split, `tools/catch-up.ps1:12`,
`tools/files-overlap.ps1:11` and `.coord/templates/item.md:34` still name `SelfTest.cs`. Loop Hub 2 proposed it as
T2-hot-files-stale-path; the user approved (below).

## Findings
- **Kind and why:** process; `product/AntiNotepad/SelfTest.cs` no longer exists (A1-selftest-split, c9a1fa9), so the
  name is dead weight and misleads card writers. Harmless: `SelfTest/*.cs` is already on both lists.
- **Now:** `tools/catch-up.ps1:12` and `tools/files-overlap.ps1:11` list `'product/AntiNotepad/SelfTest.cs',` in
  `$HotFiles` next to `SelfTest/Contracts.cs` and `SelfTest/*.cs`; `.coord/templates/item.md:34` has the Files example
  `` `product/AntiNotepad/SelfTest.cs` (or the matching `SelfTest/*.cs` part) ``. No other live loop file names it
  (`rg` over `tools`, `.coord/templates`, `.claude/skills`, `docs/loop.md`, `docs/agents.md`).
- **Where:** the three lines above; nothing calls them besides the scripts themselves.
- **Risk:** none beyond a typo in a PowerShell array (the gate's loop-lint and a `-DryRun`/run of each script catch it).
- **Source:** master scout.

## Plan
- **After:** both `$HotFiles` arrays list `SelfTest/Contracts.cs` and `SelfTest/*.cs` only; the template line reads
  `` `product/AntiNotepad/SelfTest/<area>.cs` ``.
- **Expected time:** about 10 minutes, request to merge.
- **Must not change:** every other `$HotFiles` entry and the rest of both scripts and the template.
- **Scope questions:** None.
- **Test plan, code:** run `pwsh -NoProfile -File tools/files-overlap.ps1 -Task T2-hot-files-stale-path` from the
  worktree (exit 0, no parse error); `tools/catch-up.ps1` parses (`[System.Management.Automation.Language.Parser]::ParseFile`).
- **Test plan, UI:** None (not visible).
- **Gates:** `tools/gate.ps1 -Task T2-hot-files-stale-path`: scope (only these files), loop-lint (template and script
  references stay consistent), format.
- **Needs from you:** Nothing.

## Files
- `tools/catch-up.ps1`
- `tools/files-overlap.ps1`
- `.coord/templates/item.md`

## Contracts
None.

## Approve
User, 2026-11-01: "Approve both" (T2-hot-files-stale-path and T3-codex-unpause-docs).

## For Codex
Not used: fast lane, three lines; the builder sub-session edits them itself and says so in its report.

## Check
- Gate `tools/gate.ps1 -Task T2-hot-files-stale-path`: GATE: PASS (Build, Selftest, Seam, Format, Scope, LoopLint, Size, Cleanup all PASS).
- Code test: both scripts parse clean; `files-overlap.ps1 -Task T2-hot-files-stale-path` PASS, exit 0.
- Bounds check: request = drop stale name in 3 places; plan = same three lines; done = exactly those three lines, nothing else changed.
- Misses: None.

## Hand test
None (not visible).
