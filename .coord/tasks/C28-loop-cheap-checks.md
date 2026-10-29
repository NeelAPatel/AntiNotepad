# C28-loop-cheap-checks: format check, worktree-safe ledger, triage, lessons, reviewer scores, quiet relays

Item card (`docs/loop.md`). Done by Claude directly (tools and process docs; no product code).

Tier: Low
Branch: `feat/C28-loop-cheap-checks` in `.claude/worktrees/C28-loop-cheap-checks`

## Request
User, 2026-10-29: "Okay lets do 1-5 and improve the dev loop. While we're making it more strict i dont want to waste too
many tokens and usage% on these instead of the actual work at hand." (Items: 1 zero-token checks, 2 triage on failure,
3 lessons and retro, 4 reviewer scores, 5 quieter relays.)

## Findings
- **Kind and why:** process. Every failure went to a Codex rework regardless of cause; lessons reached the rules only by
  hand; seven empty Codex reviews in a row could not be scored; Codex runs in worktrees wrote `usage.tsv` and logs into
  the worktree (caps not shared, logs lost); Master 1 woke 3-4 times per item to say "still running".
- **Now:** `dotnet format --verify-no-changes` on dev already reports older problems (e.g. `UI/SettingsWindow.cs`), so a
  whole-file check would fail every gate.
- **Where:** `tools/gate.ps1`, `.coord/tools/codex-run.sh`, `/review-task`, `/merge-task`, `docs/loop.md`, item template.

## Plan
- **After:** gate Format step fails only on problems on changed lines; ledger and logs in the main checkout; triage table
  in `/review-task` (no extra agent); `.coord/lessons.md` (master-owned) with Misses and Scores, filled at Merge; repeated
  causes become user-approved `idea:` rows; quiet relays; dev badge named as the user's first hand check while Computer
  Use is off; loop version v2.1.
- **Must not change:** product code, existing gate steps, phases.
- **Test plan, code:** a probe bad indent on a changed line fails Format while 18 older problems are ignored; dry run
  shows the ledger and log in the main checkout; full gate passes without the probe.
- **Gates:** gate.
- **Needs from you:** nothing. The headless render probe (item 1b) is product code: queued for Master 1 via Codex.

## Files
- `tools/gate.ps1`
- `.coord/tools/codex-run.sh`
- `.coord/lessons.md`
- `.coord/templates/item.md`
- `.claude/skills/review-task/SKILL.md`
- `.claude/skills/merge-task/SKILL.md`
- `docs/loop.md`
- `.coord/tasks/C28-loop-cheap-checks.md`

## Contracts
None.

## Go
User, 2026-10-29, the request above.

## Check
- Format probe: indent change on `UI/SettingsWindow.cs:62` -> `[FAIL] Format: 1 format problem(s) on changed lines; 18
  older one(s) ignored`; reverted. `dotnet format` reports whitespace at the end of the line before, so line N-1 counts.
- Ledger dry run: `log=F:/.../AntiQuickPad/.coord/logs/C28-loop-cheap-checks.worker.log` (main checkout).
- Bounds: within plan. Justified: triage done by the Check session rather than a separate Haiku bot (same result, no
  extra context to load); lessons master-owned so branches never conflict on it.
- Misses: None.

## Hand test
None (not visible).
