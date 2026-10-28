# C13-dev-loop-kit-review: lens review of C13-dev-loop-kit

Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`.
Branch: `feat/C13-dev-loop-kit` (already checked out; never switch, merge, commit or edit product files).
Tier: Medium

## Inputs
- Brief: `.coord/tasks/C13-dev-loop-kit.md`
- Implementer report: `.coord/reports/C13-dev-loop-kit.md`
- Gate report: `.coord/reports/C13-dev-loop-kit.gate.md`
- Diff: everything is uncommitted on this branch: `git diff -- product tools .coord/tools .github` plus the untracked files
  `tools/gate.ps1`, `product/AntiNotepad/SelfTest/Contracts.cs`, `.github/workflows/gate.yml`.

## Do
Follow `.agents/skills/lens-review/SKILL.md` exactly. For this task, also check the gate script itself as a tool others will
rely on: does it ever pass when it should fail (wrong exit code, a swallowed step, a selftest failure not caught), can its
cleanup delete anything outside `build/`, and does the scope step parse briefs written from `.coord/templates/brief.md`?
You may build into `build/C13-dev-loop-kit-review` and run `--selftest`, or run the gate with a different `-Task` name, to
prove a finding; delete what you create after. Never change files under `product/` or `tools/`.

## Write: `.coord/reports/C13-dev-loop-kit.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
