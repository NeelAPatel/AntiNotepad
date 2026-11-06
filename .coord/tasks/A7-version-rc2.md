# A7-version-rc2: stamp the app as 0.7.0-rc2

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build; Codex reads the whole card and
follows "For Codex". Codex never edits this file; Check and Hand test are filled by the sub-session afterwards.

Tier: Low · fast lane (one property file, no visible change) (rules: `docs/loop.md`)
Branch: `chore/A7-version-rc2` in `.claude/worktrees/chore/A7-version-rc2` (created at Approve)

## Request
2026-11-06, user's go for the lean release (Master 4 relayed it): "Go, lean release". Context: `v0.7.0-rc2` is about to be
cut from dev (A4-design-tokens, A5-search-bar-overlap, A6-note-switch-instant-swap, all hand-tested). The app still stamps
version 0.6.0 with label "untested". Fix the version, check a Release build. No audit, no lab, no hand test.

## Findings
- **Kind and why:** process (release preparation); the stamped version is wrong for the release candidate.
- **Now:** `product/AntiNotepad/AppInfo.props` has `AppVersion` 0.6.0 and `AppVersionLabel` untested. `InformationalVersion`
  is `$(AppVersion)-$(AppVersionLabel)`, so the build reads 0.6.0-untested.
- **Reproduction (bugs):** not a bug.
- **Where:** `AppInfo.props` only. Grep for `AppVersionLabel` and `untested` in the source (`.cs`, `.props`, `.csproj`,
  `.axaml`, `.ps1`, `.sh`) finds no other reader; the only hits are the file itself and its copies in other worktrees.
- **Risk:** none visible. A wrong value would only mislabel the build; the Release build and selftest confirm it compiles.
- **Source:** master scout (Master 4), grep in this session.

## Plan
- **After:** `AppVersion` is `0.7.0` and `AppVersionLabel` is `rc2`, so the assembly version reads 0.7.0 and the
  informational version reads `0.7.0-rc2`. Nothing else changes.
- **Expected time:** under 30 minutes, request to merge. Small one-line change in one file.
- **Must not change:** every other property, behavior, look, file and setting.
- **Scope questions:** None
- **Test plan, code:** None new. Existing `--selftest` must print `SELFTEST PASS` on the Release build.
- **Test plan, UI:** None (not visible)
- **Gates:** `tools/gate.ps1 -Task A7-version-rc2` (online first so the NuGet audit runs; `-Offline` only if no network,
  recorded). Catches scope drift against `## Files` and build or selftest failures.
- **Needs from you:** Nothing

## Files
Only these files may change (exact paths; `tools/gate.ps1` reads this list). Any other file needs a line under
"Out-of-scope files" in the report.
- `product/AntiNotepad/AppInfo.props`

## Contracts
`CONTRACTS` rows this item touches (`product/AntiNotepad/SelfTest/Contracts.cs`). Keep them passing; add a row for any new
promise and for every bug fixed.
- None

## Approve
User's go, 2026-11-06, verbatim: "Go, lean release". Scope: fix the version in `AppInfo.props` (0.6.0 to 0.7.0, untested to
rc2) and check a Release build. Approved by the user via Master 4 (`local_f12601a5-afab-4f84-8672-8773e12947da`).

## For Codex
Role: quick, a two-line property change in one known file (`docs/agents.md`: `quick` = gpt-6-luna medium). Product
`product/AntiNotepad`. The branch is already checked out: never switch, merge or commit (you cannot write to `.git`). Read
`AGENTS.md`, then the files above. Line numbers are from the current tree; re-read before editing.

Constraints:
- Build exactly the Plan's "After" for the Files above. No drive-by refactors, renames, icon/token/style edits outside it.
- If the Findings turn out wrong (the bug does not reproduce, the cause is elsewhere, the plan cannot work), stop and
  report that instead of building something else.
- Build into `build/A7-version-rc2-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A7-version-rc2 -Offline`; the manager
  re-runs it online.
- Never launch or kill AntiNotepad. Never run `--quit`. Never edit `.coord/STATUS.md` or this card.

Done means (offline checks only: build, selftest, gate, script checks, files; never ports, servers, browser or screen):
- `AppInfo.props` has `<AppVersion>0.7.0</AppVersion>` and `<AppVersionLabel>rc2</AppVersionLabel>`; no other line changes.
- Build has 0 warnings, `SELFTEST PASS`, the gate passes.

Report: `.coord/reports/A7-version-rc2.md`, short, with exactly these sections:
1. **Summary**: two or three sentences.
2. **Change list**: one row per changed block: `file:line-range | what changed | which Plan line it serves`.
3. **Behavior changes**: every changed public method result or meaning, new or changed shared state, timing, default, or
   setting effect. Write "None" only if true; the review checks this against the diff.
4. **Out-of-scope files**: file and why, or "None".
5. **Plan deviations**: anything done differently from the Plan, and why, or "None".
6. **Contracts**: rows added or changed, or "None".
7. **Lens review**: findings and what you did with each. (Low tier, fast lane: write "Skipped: Low tier, one property line".)
8. **Checks run**: build, selftest, gate result lines.
9. **Hand checks for the user**: "None (not visible)".

## Check
- Gate (online, `tools/gate.ps1 -Task A7-version-rc2`): GATE: PASS (selftest PASS, seam PASS, tokens PASS, scope PASS,
  size PASS, cleanup PASS; LoopLint skipped, no loop files).
- Release build (`dotnet build -c Release`): 0 warnings, 0 errors.
- Release selftest with `ANTINOTEPAD_DATA` under `build/`: exit 0, `SELFTEST PASS`.
- Built exe version: FileVersion 0.7.0.0, ProductVersion 0.7.0-rc2.
- Bounds check: AppVersion and AppVersionLabel within plan; no other file changed; no other readers of the label.
- Noticed: the sandbox could not reach NuGet's vulnerability endpoint (NU1900) during Codex's direct build; the online gate ran fine.
- Misses: None

## Hand test
none: no visible change; user's go 2026-11-06
