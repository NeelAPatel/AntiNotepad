**In short**

Loop2-loop-v3-1 turned several written promises into checks that scripts can enforce. Reviews found seven concrete defects before the work reached dev, and the fixes were checked again. A rehearsal then passed eight checks in a disposable clone; it did not prove every live chat behavior.

# Worked Example: Loop2-loop-v3-1

## Request and findings

The user asked on 2026-10-30: "assess the loop with the intention of best version yet and come up with a strong update patch for v3.1. (calling it 3.0.1 was my mistake)". They also required a branch, small commits and no worktree. These exact words and the scope decisions are in `.coord/tasks/Loop2-loop-v3-1.md`, Request and Approve.

The scout found that the dormant overlap and catch-up scripts recognized only feature branches, while v3 had five branch prefixes. The cap of three approved items was checked by eye, scope violations normally warned, and Findings audits had a writable sandbox. Those findings, their risks and the decision to use scripts rather than add reviewers are recorded in `.coord/tasks/Loop2-loop-v3-1.md`, Findings.

## Plan and approval

This was Low tier, docs-tools, with extra fresh Codex reviews for scripts that could block or falsely pass an item. The user approved a single branch without parallel chats. This was an explicit exception to the normal one-squashed-commit route: small step commits were kept by fast-forwarding dev. Do not copy that exception into a normal item; compare `.coord/tasks/Loop2-loop-v3-1.md`, Decisions and Approve, with `docs/loop.md`, "Branches, worktrees and commits".

After the user questioned the size of the process, the second plan trimmed conflict handling and the hand-test queue, and replaced a queue implementation with backlog research. That change is preserved in `.coord/tasks/Loop2-loop-v3-1.md`, "Round 2 scope change", and commit `093c4e8`.

## Steps and their checks

The hashes and subjects below come from `git log --oneline 8d18f72..12e9e1c`. Gate results and review outcomes are recorded in `.coord/tasks/Loop2-loop-v3-1.md`, Check, with the detailed reports named below.

| Step | Result and commit | Gate and review outcome |
|---|---|---|
| Card | `c644877` — card | Request, Findings, Plan and approval recorded in `.coord/tasks/Loop2-loop-v3-1.md`. |
| 3.1a | `ac450d2` — fix-loop-scripts | Gate PASS. Fresh review required rework; r2 report records corrected Git-error handling and an accepting lens review: `.coord/reports/Loop2-loop-v3-1-fix-loop-scripts-r2.md`, Checks run and Lens review. |
| 3.1b | `0909d14` — approve and merge preflights | Gate PASS after r2. Fresh r2 review accepted with one guarded minor limitation: `.coord/reports/Loop2-loop-v3-1-preflights-r2-review.review.md`. |
| 3.1c | `665aa6c` — catch-up points | Gate PASS after r2; fresh r2 review accepted: `.coord/reports/Loop2-loop-v3-1-catch-up-points-r2-review.review.md`. |
| Re-plan | `093c4e8` — re-plan round 2 (trim, scan-backlog, diagram) | Approved scope adjustment, rather than an implementation failure: `.coord/tasks/Loop2-loop-v3-1.md`, Plan. |
| 3.1d | `b000381` — conflict route and hand-test turns | Gate PASS, worker checklist self-review, no separate lens reviewer: `.coord/reports/Loop2-loop-v3-1-conflicts-and-queue.md`, Lens review and Checks run. |
| 3.1e | `464abc8` — scan-backlog and scan-delta | Gate PASS; fresh review accepted without proven findings: `.coord/reports/Loop2-loop-v3-1-scan-backlog-review.review.md`. |
| 3.1f | `479afd3` — strict scope and read-only audits | Gate PASS after r2; fresh r2 review accepted: `.coord/reports/Loop2-loop-v3-1-gate-and-audit-hardening-r2-review.review.md`. |
| 3.1g | `f9240eb` — loop-lint | Gate PASS after r2; fresh r2 review accepted: `.coord/reports/Loop2-loop-v3-1-loop-lint-r2-review.review.md`. |
| 3.1h | `68b4d9b` — rules text says loop v3.1 | Gate and lint PASS. Worker self-review; the additional fresh review of the gate trigger fix accepted: `.coord/reports/Loop2-loop-v3-1-loop-md-v3-1.md` and `.coord/reports/Loop2-loop-v3-1-loop-md-v3-1-review.review.md`. |
| 3.1i | `a50c538` — rehearsal | Eight checks PASS in a throwaway clone. No new code or separate lens review: `.coord/reports/Loop2-loop-v3-1-rehearsal.md`, Summary and Lens review. |
| Close | `12e9e1c` — close, card Check, STATUS and lessons | Card records within-plan bounds, no UI hand test, and the wiki and publication still outstanding at that close: `.coord/tasks/Loop2-loop-v3-1.md`, Check and Hand test. |

## Seven proven findings and their fixes

These are the seven findings fixed from the first fresh reviews. The reports give the original line numbers and proof. The table follows those detailed reports where the card's short summary is less exact.

| # | What the reviewer proved | Fix and evidence |
|---|---|---|
| 1 | Failed Git worktree enumeration could still print an overlap PASS. | Check native Git exit codes before success. `.coord/reports/Loop2-loop-v3-1-fix-loop-scripts-review.review.md`, finding 1; `.coord/reports/Loop2-loop-v3-1-fix-loop-scripts-r2.md`, Change list and failure-injection checks. The failure was in overlap enumeration; catch-up error handling was hardened too. |
| 2 | Labeled placeholders in an untouched Plan could pass merge preflight. | Both preflights reject placeholder values after optional bullets and labels. `.coord/reports/Loop2-loop-v3-1-preflights-review.review.md`, finding 1; `.coord/reports/Loop2-loop-v3-1-preflights-r2.md`, real-template probes. |
| 3 | Git-quoted Unicode filenames could suppress a required repeated hand test. | Read incoming and conflicting filenames with NUL delimiters and compare literal paths. `.coord/reports/Loop2-loop-v3-1-catch-up-points-review.review.md`, finding 1; `.coord/reports/Loop2-loop-v3-1-catch-up-points-r2.md`, real-Git Unicode probes. The proven case was Unicode quoting, not just spaces. |
| 4 | Audit handoff copied the director report even when failover succeeded as worker or backup. | Copy the report named by this invocation's final DONE line; copy nothing after failure. `.coord/reports/Loop2-loop-v3-1-gate-and-audit-hardening-review.review.md`, finding 1; `.coord/reports/Loop2-loop-v3-1-gate-and-audit-hardening-r2.md`, Change list. |
| 5 | A broad capacity match treated unrelated failures, including disk capacity, as provider limits. | Require model context for the added capacity diagnostic. Same hardening review, finding 2, and r2 report, extracted-regex checks. The r2 report explicitly says Bash execution was blocked in that sandbox. |
| 6 | Valid Markdown links with escaped punctuation failed the missing-path check. | Decode Markdown escapes for ASCII punctuation before URI decoding. `.coord/reports/Loop2-loop-v3-1-loop-lint-review.review.md`, finding 1; `.coord/reports/Loop2-loop-v3-1-loop-lint-r2-review.review.md`, actual-script probes. |
| 7 | Passing a file as lint's Root produced PASS with zero checked files. | Require a directory and reject an empty live inventory. Same lint review, finding 2, and r2 review, actual file-root and empty-directory checks. |

One further minor finding remained in the accepting preflight r2 review: its helper does not recognize the real template's multiline Check placeholder, but the separate open-question guard rejects that template. It is a qualified acceptance, not evidence that every placeholder form is recognized. Source: `.coord/reports/Loop2-loop-v3-1-preflights-r2-review.review.md`, finding 1 and Checks run.

## The 3.1i rehearsal

The builder used a clone of `68b4d9b`, with an offline bare origin, and ran real builds and selftests. The following compresses the eight numbered checks without upgrading their evidence. Source: `.coord/reports/Loop2-loop-v3-1-rehearsal.md`, Setup, Checks and Surprises.

| Check | What happened | What it established |
|---|---|---|
| 1 — Approve | Created a nested item worktree and committed the card. | The scripted worktree route worked in the clone. |
| 2 — Strict scope | A stray file failed; removing it made the gate pass. | Scope refused an unlisted changed file. |
| 3 — Behind dev | First rejected an unfinished Check; after the card was filled, rejected the behind branch. | Both refusals worked, in the script's actual order. |
| 4 — Catch-up | Merged an unrelated dev change, printed RETEST no, and passed a new gate whose Head matched HEAD. | Catch-up and gate freshness worked for that case. |
| 5 — Merge | Preflight passed, squash added one dev commit, and worktree and branch removal succeeded. | The clean merge and cleanup route worked. |
| 6 — Scan delta | Reported one of two Where files changed, a nearby file and a merged card; RESCAN no. | Delta classification worked on prepared research data. |
| 7 — Nested Codex | Read the project role definition without a trust warning; spawned nothing. | The nested worktree exposed `.codex/agents/lens-reviewer.toml`; live role spawning was not proved. |
| 8 — Lint | PASS across 37 live files. | The normal lint inventory passed in the clone. |

This was not a live `/scan-backlog` with Haiku scouts. Nor did it prove a nested `lens_reviewer` spawn. Those limits are explicit in `.coord/reports/Loop2-loop-v3-1-rehearsal.md`, Summary and Surprises, and remain listed on [Home](Home).

## Close and what to copy

Copy the discipline: preserve the request, agree on scope, give scripts a deliberate failure case, fix proven findings, and state what a rehearsal did and did not prove. Do not copy the exceptional fast-forward step history as the normal merge rule. The close card and `docs/loop.md`, "Branches, worktrees and commits", distinguish them.

## Sources

- `.coord/tasks/Loop2-loop-v3-1.md` — request, approved plan, step results and close.
- `.coord/reports/Loop2-loop-v3-1-fix-loop-scripts-review.review.md` and `.coord/reports/Loop2-loop-v3-1-fix-loop-scripts-r2.md` — enumeration finding and fix.
- `.coord/reports/Loop2-loop-v3-1-preflights-review.review.md`, `.coord/reports/Loop2-loop-v3-1-preflights-r2.md` and `.coord/reports/Loop2-loop-v3-1-preflights-r2-review.review.md` — placeholder finding, fix and remaining qualification.
- `.coord/reports/Loop2-loop-v3-1-catch-up-points-review.review.md`, `.coord/reports/Loop2-loop-v3-1-catch-up-points-r2.md` and `.coord/reports/Loop2-loop-v3-1-catch-up-points-r2-review.review.md` — filename finding and fix.
- `.coord/reports/Loop2-loop-v3-1-conflicts-and-queue.md` — conflict rule and hand-test turns.
- `.coord/reports/Loop2-loop-v3-1-scan-backlog-review.review.md` — accepted scan review and live-orchestration limit.
- `.coord/reports/Loop2-loop-v3-1-gate-and-audit-hardening-review.review.md`, `.coord/reports/Loop2-loop-v3-1-gate-and-audit-hardening-r2.md` and `.coord/reports/Loop2-loop-v3-1-gate-and-audit-hardening-r2-review.review.md` — audit/failover findings and fixes.
- `.coord/reports/Loop2-loop-v3-1-loop-lint-review.review.md` and `.coord/reports/Loop2-loop-v3-1-loop-lint-r2-review.review.md` — lint findings and accepted fixes.
- `.coord/reports/Loop2-loop-v3-1-loop-md-v3-1.md` and `.coord/reports/Loop2-loop-v3-1-loop-md-v3-1-review.review.md` — rule update and trigger review.
- `.coord/reports/Loop2-loop-v3-1-rehearsal.md` — eight checks, limitations and cleanup.
- `docs/loop.md` — current rules; "Sessions" and "Branches, worktrees and commits".
- Git history: `git log --oneline 8d18f72..12e9e1c` — step hashes and subjects.
