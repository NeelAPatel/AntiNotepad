# Loop2-loop-v3-1-gate-and-audit-hardening: step 3.1f, strict scope by default and read-only audits

Step brief of `Loop2-loop-v3-1` (read the card `.coord/tasks/Loop2-loop-v3-1.md` first; the user chose "strict by
default" on 2026-10-30).

Tier: Low · docs-tools
Role: director (gpt-6.1-sol medium)

## Why this step
- `tools/gate.ps1` Scope only FAILs with `-StrictScope`, which no skill passes, so a file outside the card's Files is a
  WARN and the bounds check is the only stop. A missing brief or `## Files` list is also only a WARN.
- Findings audits run `codex exec --sandbox workspace-write` in the shared main checkout (`.coord/tools/codex-run.sh`);
  "read-only" is only an instruction in `.coord/templates/findings.md`, which also tells Codex to write the report file.

## After
1. **`tools/gate.ps1` Scope:**
   - Out-of-scope files, a missing brief, a brief with no `## Files` section or no backticked paths, and the rework-brief
     equivalents are **FAIL** by default.
   - They stay **WARN** when: `-Ci`; HEAD is the `-Base` branch itself (the dev re-gate in `/merge-task` step 4) — detect
     with `git rev-parse --abbrev-ref HEAD` equal to `$Base`; or the new switch `-LooseScope` is given (its summary says
     "loose scope").
   - Remove `-StrictScope` (no caller uses it). Everything else in the gate is unchanged.
2. **`.coord/tools/codex-run.sh`:** env `CODEX_SANDBOX` (default `workspace-write`; allowed `workspace-write` or
   `read-only`, anything else exits 2 with a message) replaces the hard-coded `--sandbox workspace-write`. The `PLAN:`
   line prints the sandbox. Header comment lists the new env var. Also: the failover grep treats
   "at capacity" / "model is at capacity" like a rate limit (seen 2026-10-30: a reviewer run failed with "Selected model
   is at capacity" and did not fail over).
3. **`.coord/templates/findings.md`:** the audit runs read-only: it never writes files; its final message **is** the
   report (sections 1-7 and the `READY:` line); probes that need a build are described as steps for the user/manager
   instead of run (remove the "probe in `build/<ID-slug>-audit`" instruction, keep "exact code path quoted step by step").
4. **`.claude/skills/next-task/SKILL.md` step 2 (Medium, High):** the auditor runs
   `CODEX_SANDBOX=read-only bash .coord/tools/codex-run.sh director <ID-slug>-audit .coord/tasks/<ID-slug>-audit.md`, then
   copies `.coord/reports/<ID-slug>-audit.director.codex.md` to `.coord/reports/<ID-slug>-audit.md` (the audit report the
   loop already names). Nothing else in the step changes.
5. **`docs/agents.md`:** the sandbox bullet says audits run `CODEX_SANDBOX=read-only`; builds and reviews keep
   `workspace-write`. **`docs/loop.md` Gates menu**, gate row "Catches": "files outside the plan" becomes "files outside
   the plan (FAIL on item branches; WARN on dev, CI or `-LooseScope`)". Nothing else.

## Files
- `tools/gate.ps1`
- `.coord/tools/codex-run.sh`
- `.coord/templates/findings.md`
- `.claude/skills/next-task/SKILL.md`
- `docs/agents.md`
- `docs/loop.md`

## Done means
- In a throwaway clone under `build/Loop2-loop-v3-1-probe` (deleted after; never this checkout), with a tiny card: an
  item branch with a stray file gives `Scope: FAIL`; the same with `-LooseScope` gives WARN; on `dev` gives WARN; a
  missing brief on an item branch gives FAIL. (Build/selftest may be skipped or failing in the clone: only the Scope line
  matters; say how you isolated it.)
- `CODEX_DRY=1 CODEX_SANDBOX=read-only bash .coord/tools/codex-run.sh director Loop2-loop-v3-1-probe-audit <any file>`
  prints `sandbox=read-only`; `CODEX_SANDBOX=bogus` exits 2.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline` passes on this branch (strict: every changed file
  is in the card's Files).

## Constraints
Work in this checkout (`chore/Loop2-loop-v3-1`; no worktree). Never switch branches, merge, commit or push here; never
edit `.coord/STATUS.md`, the card, or `product/`. Never launch or kill AntiNotepad. Keep changes small, in the existing
style (`docs/coding.md`).

Report: `.coord/reports/Loop2-loop-v3-1-gate-and-audit-hardening.md` with the item template's report sections. Note for
the manager: a real (non-dry) read-only Codex run is checked separately by the manager, not by you.
