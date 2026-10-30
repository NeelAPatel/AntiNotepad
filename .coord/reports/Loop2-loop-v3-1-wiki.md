# Loop2-loop-v3-1-wiki

## Summary
Rewrote the local GitHub-wiki Markdown for loop v3.1, dated 2026-10-30. The guide separates written rules, actual script enforcement, and evidence from the Loop2 rehearsal. Nothing was published.

## Change list
- Home: purpose, start-here navigation, authority, known limits.
- Getting-Started (new): prerequisites, exact master handover prompt, first-item walkthrough, commands, widgets, comparison with ordinary chat.
- Dev-Loop-Overview: existing flow image and eight phases with actor, model, payload, outputs and safeguards.
- Who-Does-What: Claude chats versus subagents versus headless Codex, exact sourced openings, permissions, wrapper stdin, caps and failover, script/hook reference.
- Parallel-and-Branches (new): existing git diagrams, three-item cap, overlap/hot files, catch-up, retest, conflicts, hand-test queue, preflight and no-worktree exception.
- Backlog-Scans (new): snapshot scouts, research contents, delta and rescan thresholds.
- Tiers-and-Gates, Triage-and-Lessons, Where-Files-Live, Glossary, Loop-History: v3.1 rules and evidence.
- Worked-Example-Loop2 (new): real step hashes, gates/reviews, seven fixed findings, guarded minor limitation and eight rehearsal checks.
- Worked-Example-A55: retained because cited card/audit/implementation/review remain; explicitly loop v2, era one, with archived versus rewritten hashes.
- Loop-v2.2-Plan: removed; history explains absorption.
- Sidebar: Getting Started second and links to new pages.
- Existing SVGs unchanged.

## Behavior changes
Documentation only. No product, rule, script or configuration changes.

## Diagram-page differences and source qualifications
Compared build/loop-v3-1-flow-page.html const N with the rules and implementations:
1. f3 and b2 assume a .director.codex.md output suffix. codex-run.sh attempt uses the successful role; audit handoff reads this invocation's DONE line, including worker/backup. Wiki does not assume director output after failover.
2. b2 uses the lens_reviewer header as the director box's heading. Wiki separates the card's For Codex role line from the role definition's actual reviewer opening.
3. c3 says Scope fails for any file outside Files. gate.ps1 Get-ChangedFiles excludes .coord/; Scope also warns on base/dev, -Ci and -LooseScope. Wiki states those boundaries.
4. c3 says Head is the exact commit. gate.ps1 appends -dirty for tracked changes other than STATUS. Wiki explains tested-state evidence and merge-preflight's exact-tip requirement.
5. a2 says checkout must be clean. approve.ps1 permits unstaged STATUS changes; wiki describes this exception.
6. m3 says every card phase is checked and checkout is clean. merge-preflight.ps1 checks Request, Plan, Approve|Go, Check and Hand test, not a separate Findings section, and allows tracked STATUS changes. It tests literal open-question lines and selected placeholders, not semantic correctness of all prose.
7. h2 says anything not visible skips Hand test. docs/loop.md Tiers always requires High hand testing; wiki preserves that exception.
8. c5 labels design review High only. High requires it; .claude/agents/design-reviewer.md also permits Medium when requested. Wiki states both.
9. c2's catch-up description omits fetch behavior: catch-up.ps1 fetches origin but merges local dev, only noting origin/dev divergence; it never pushes. A merge commit is not produced for an already-current branch or unresolved conflict.
10. s2 says the first line is Scanned at. The scan template starts the report with that stamp; wiki describes the stamped format and validation, without implying all illustrative filenames exist in this checkout.
11. f1 says ten reads; next-task step 2 says about ten tool calls. Wiki uses the skill's bound.
12. The diagram's scout/finding/template openings sometimes combine wrapped lines or prefix a role label. Wiki quotes exact sentences or labels them as opening excerpts, rather than inventing a whole fixed payload.
13. The discussion-page planned example of four Haiku scouts is not an observed live run. Wiki describes the documented workflow and expressly preserves the no-live-scan limit.

Rule-file tension: docs/loop.md Sessions table says the auditor commits its report, but its Branches section and next-task step 2 say no commit before Approve. Wiki follows the detailed launch rule: auditor commits nothing; Approve commits the planning files.
Evidence tension: Loop2 card summaries describe the enumeration failure as catch-up and filename case as spaces. Detailed fresh reviews prove overlap enumeration and Git-quoted Unicode paths respectively. Worked example follows those reports and distinguishes the additional related hardening.

## Claims dropped or narrowed for lack of evidence
- No fixed master model or effort; no invented initial prompt for auditor/reviewer/tester or per-worker delegation. Rules specify actions but no exact opening for those roles.
- No measured token, price, speed or reliability savings. Model routing is documented; savings are not benchmarked.
- No claim the nested lens_reviewer spawned: rehearsal check 7 only read definitions and saw no trust warning.
- No claim live scan-backlog orchestration ran: prepared scan-delta data is narrower evidence.
- No automatic CI on pushes, automatic publishing, guaranteed conflict prevention, or concurrent dev app instances.
- No universal scope enforcement across coordination files or semantic validation of every card phase.
- No universal squash-history claim for Loop2 itself: user authorized small commits and an exceptional fast-forward; ordinary items use the current squash rule.
- A55: removed unsupported historical session/launch detail and the claim a helper read the screen badge. Its card explicitly says it did not. Kept structural proof distinct from user-observed pixel containment.
- Replaced broad old history narrative with verified git subjects. Removed active-v2.2 instructions because docs/v2.2/README.md says absorbed, never activated.
- Generated .coord/roadmap.html is absent on this branch. Wiki describes it as generated output and cites its script rather than backticking it as an extant source file.

## Known limits
- Master model unspecified: docs/loop.md, Sessions.
- Live scan-backlog not exercised: Loop2 rehearsal Summary and Hand checks.
- Nested role definitions visible, runtime lens_reviewer spawn unproved: rehearsal check 7 and Surprises.
- CI manual: .github/workflows/gate.yml workflow_dispatch and docs/loop.md Triggers.
- One dev copy at a time, oldest accepted hand test first: docs/loop.md Branches and docs/dev-loop.md Dev copy.

## Plan deviations
None. STATUS and the card were not edited, as explicitly required by this task. No commits, branch changes, merges, pushes, interactive app launches or installed-copy interaction. The requested gate ran headless selftests in its isolated build/data folders and cleaned them up.

## Contracts
None; documentation only.

## Lens review
Independent lens_reviewer returned VERDICT: accept. Fixed its minor findings: qualified the Overview Scope summary with coordination-file/CI/loose exclusions, and corrected two Dev instance section references to Dev copy. Review checked sources, payloads, links/images, rehearsal boundaries and scope; no blocker or major finding. No runtime product behavior changed.

## Checks run
- Required backticked-file path command: no output.
- Additional literal-directory path check: no unresolved backticked directories. Generated research/log directories are described as output locations, not extant sources.
- All 14 Markdown pages have opening In short summaries and ending Sources; all local wiki links and SVG targets resolve.
- git diff --check: PASS (line-ending notices only).
- SVG diff: empty. Working-tree status contains only docs/wiki changes and this report; card and STATUS unchanged.
- Exact requested online gate: FAIL because NuGet vulnerability-data requests could not reach api.nuget.org in the restricted environment (NU1900, two warnings). Compilation exited 0; selftest, seam, format, scope and size passed. Cause: env, not documentation or product.
- Documented sandbox retry, tools/gate.ps1 -Task Loop2-loop-v3-1-wiki -Offline: GATE PASS; Build 0 warnings/0 errors; SELFTEST PASS; CONTRACTS 13 rows, 12 pass, 1 manual; seam PASS; Format PASS; Scope PASS (18 files, all allowed); Size PASS; Cleanup PASS. NuGet audit disabled for this build only. LoopLint SKIP because no live rule/script files changed.
- Gate Head is 6878c9f587cf28735f708541700213e2948bb783-dirty, as expected for uncommitted wiki edits. This is not merge-ready exact-tip evidence; the manager must commit and re-gate before its normal merge preflight. Online vulnerability audit remains unverified.

## Hand checks for the user
Read Home and Getting Started, then inspect phase/role details and worked evidence. No product UI change or app hand test.
