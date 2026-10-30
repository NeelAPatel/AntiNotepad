# Loop2-loop-v3-1-preflights-review lens review

## Findings (proven)
1. [major] `tools/merge-preflight.ps1:35-37` - An unfilled template Plan passes the phase check. The placeholder expression only matches a line beginning with optional whitespace/bullet followed immediately by `<...>`. The actual item template (`.coord/templates/item.md:20-27`) puts its seven Plan placeholders after labels, such as `- **After:** <what it will do, next to "Now">`. These lines are nonempty and none matches the rejection expression. In an in-memory probe, I filled Request, Approve, Check and Hand test with `Completed.`, retained the template Plan verbatim, and executed the actual validation loop extracted from lines 33-43: all five checks passed. With clean/caught-up checkouts and a matching PASS gate, the remaining checks do not inspect Plan content, so this reaches `MERGE-PREFLIGHT: PASS (ready)` without a filled Plan. Smallest fix: recognize placeholder values after template field labels as well as standalone placeholder lines; add a focused probe using the real unfilled Plan template. Apply the same placeholder rule to the Approve validator for consistency.

## Behavior changes check
Report's list vs the diff: matches the intended changes, except the claim that merge preflight rejects missing phases is incomplete for unfilled labeled Plan fields (finding 1). Both new preflights check native Git exit codes through `Invoke-Git` or immediately after direct calls; overlap's child-process exit code is also checked. No additional changed shared contracts or callers were found beyond the two skill invocations and consumers of the gate report/hook output.

## Scope and plan check
In plan: the reviewed changes comprise exactly the brief's six files: `tools/approve.ps1`, `tools/merge-preflight.ps1`, `tools/gate.ps1`, `tools/loop-card.ps1`, `.claude/skills/next-task/SKILL.md`, and `.claude/skills/merge-task/SKILL.md`. The new scripts were read in full, along with the changed-file context and overlap dependency.

The skill diff changes only next-task step 4 and merge-task steps 1-2. The outside-Files question remains; other steps are untouched. Gate changes add only the header, initial HEAD/dirty capture and Summary line. The hook adds only its count and warning. Existing STATUS and unrelated coordination files are outside this implementation diff; they were not attributed to it. No product files changed in the reviewed diff.

STATUS parsing follows the existing hook's Plan/table rules. Approve ignores the exact Task row and rejects count >= Cap; the hook warns only above three. Both use all eight prescribed state forms. Merge accepts `## Go`, rejects standalone placeholders and open `ask the user` lines, and permits the same line when it contains `answered`, as specified. Gate records initial full HEAD, checks both new Git commands' exits, excludes only STATUS from tracked dirtiness, and emits Head immediately after GATE. Merge's exact Head comparison rejects the `-dirty` suffix.

## Unproven risks
- None beyond the proven finding above.

## Checks run
- Read the lens skill, STATUS, item card, step brief, implementer report, gate report, six implementation files, item template, coding rules and overlap dependency; inspected `git diff -- tools .claude` and untracked preflights.
- In-memory execution of merge-preflight's actual phase-validation loop against a card retaining the real template Plan: `All five phase checks: PASS with the template Plan still unfilled` (false acceptance reproduced; no fixture files created).
- In-memory expression checks: all eight past-Approve forms counted; `done` and `planned` excluded; filled Go and Approve accepted; standalone Approve placeholder rejected.
- Reviewed every Git invocation in both preflights and the gate additions for exit handling; no unchecked native exit in the new preflight paths.
- `pwsh -NoProfile -File tools/loop-card.ps1`: exit 0; normal reminder and `In flight per STATUS: Loop2-loop-v3-1 (approved, Low).`
- `git diff --check -- tools .claude`: exit 0. Branch remained `chore/Loop2-loop-v3-1`.
- Existing gate report says `GATE: PASS`, `Head: ac450d2ca41f41c2095a0b7b624c8635658dc9d8-dirty`, build/selftest PASS. Those are implementer evidence; no build, gate, clone or mutation probes were rerun in this read-only review. Only this report was written.

VERDICT: rework
