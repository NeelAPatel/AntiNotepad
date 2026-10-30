# Loop2-loop-v3-1-wiki lens review

## Findings (proven)
1. [minor] `docs/wiki/Dev-Loop-Overview.md:117` and `docs/wiki/Tiers-and-Gates.md:29,33` - The claims that scope excludes “coordination files” and that all changed files outside the exact Files entries are checked as scope violations are broader than the implementation. `tools/gate.ps1:225-232` filters only paths beginning with `.coord/`; other coordination files such as `docs/agents.md` remain in `Get-ChangedFiles` and are checked. The rehearsal proves this boundary: `.coord/reports/Loop2-loop-v3-1-rehearsal.md:22` made a stray `docs/agents.md` fail Scope. Smallest fix: say “`.coord/` paths” wherever that exclusion is meant, and keep the exact-Files claim qualified by that exception.
2. [minor] `docs/wiki/Dev-Loop-Overview.md:113` - “Medium/High gets a fresh Sol medium Codex reviewer” omits the documented effort override. `docs/agents.md:15` and `.coord/tools/codex-run.sh:30` define the reviewer as `gpt-6.1-sol` with `CODEX_REVIEW_EFFORT`, defaulting to medium. Smallest fix: say “medium by default; `CODEX_REVIEW_EFFORT` can override it.”
3. [minor] `docs/wiki/Where-Files-Live.md:32` - “uncommitted edits ride the next item commit” omits the documented fallback. `docs/loop.md:245-248` says STATUS rides the next item commit, **or** gets a `chore: status` commit at the end of the day when nothing lands. Smallest fix: include that status-commit fallback.
4. [minor] `docs/wiki/Who-Does-What.md:55` - The statement that the Sessions-table wording “auditor ... commits it” does not authorize a pre-Approve commit is a resolution of an internal source conflict, not an uncontested fact. `docs/loop.md:208` says the auditor “commits it,” while `docs/loop.md:241-242` and `.claude/skills/next-task/SKILL.md:37-46` say the audit remains untracked and the auditor commits nothing before Approve. Smallest fix: explicitly label this as a live `docs/loop.md` inconsistency, or correct the Sessions table so the wiki can cite one settled rule.

## Behavior changes check

Report's list vs the diff: matches: documentation-only; no product, script, rule or configuration behavior changed. The findings above are accuracy/qualification issues in the wiki text, not implementation behavior changes.

## Scope and plan check

In plan. The changed wiki pages and deletion of `docs/wiki/Loop-v2.2-Plan.md` are listed in `.coord/tasks/Loop2-loop-v3-1-wiki.md`; the three SVGs are unchanged. The two `.coord/reports/Loop2-loop-v3-1-wiki*` files are the requested implementation report and wrapper output, not product changes. No unrelated product or rule-file hunks were found.

## Unproven risks

- No live Claude/Codex orchestration or GUI hand test was run in this read-only review; the rehearsal's stated limits for live `/scan-backlog` and nested `lens_reviewer` spawning remain applicable.

## Checks run

- Read all 14 current `docs/wiki/*.md` pages, the step brief, implementer report, cited rules, skills, templates, scripts, rehearsal report, step reports and relevant Git history.
- Required backticked-file path check: no unresolved paths.
- `git diff --check 12e9e1c HEAD -- docs/wiki`: PASS (no output).
- SVG scope check against `6878c9f`: empty diff for `docs/wiki/images`.
- Wiki page-link check: no broken local page or image targets.
- Verified all listed Loop2 hashes and subjects with `git show`; verified the A55 card/reports and archive tag exist.

VERDICT: accept
