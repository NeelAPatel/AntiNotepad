# Lessons

What the loop missed and how each reviewer scored, so rules improve from evidence (`docs/loop.md`, "Triage, lessons,
scores"). One line per row; append only. Lives on dev like STATUS: only the master writes it, at Merge, copying the
card's "Misses" lines and adding the item's Scores row; branches never edit it. When a cause repeats, the master proposes
a rule change.

## Misses
Every failed gate, rework round, ask-user stop, failed hand test, and bug found after merge.
Cause: `plan` (findings or plan wrong), `code`, `test` (test or contract wrong), `env` (tooling, build, flaky), `conflict`, `judgment`
(a call only the user could make). Caught by / should have caught: `gate`, `format`, `codex-review`, `design-review`,
`contract`, `hand-test`, `user-later`.

| Date | Item | Found at | Cause | Caught by | Should have caught | Lesson (one line) | Rule change |
|---|---|---|---|---|---|---|---|
| 2026-10-29 | A53-split-divider-height | Hand test | plan | hand-test | findings | dev already did what was asked; nobody checked Now | C26-item-card-loop (Findings "Now") |
| 2026-10-29 | A54-link-hover-click | Check r1 | plan | manager | findings | bug built without a reproduction; r1 claimed no fix | C26-item-card-loop (reproduce first) |
| 2026-10-29 | A54-link-hover-click | Hand test | judgment | hand-test | plan | scope (all URLs) asked only at hand test | C26-item-card-loop (scope questions) |
| 2026-10-29 | A42d-list-commands | Check (gate) | env | gate | none (sandbox has no network) | Check session sandbox cannot reach nuget.org (NU1900); master ran the online gate | - |
| 2026-10-30 | Loop2-loop-v3-1 | Check 3.1a | code | codex-review | codex-review | catch-up passed on a git enumeration failure | - |
| 2026-10-30 | Loop2-loop-v3-1 | Check 3.1b | code | codex-review | codex-review | merge preflight accepted labeled template placeholders | - |
| 2026-10-30 | Loop2-loop-v3-1 | Check 3.1c | code | codex-review | codex-review | RETEST missed incoming paths with spaces | - |
| 2026-10-30 | Loop2-loop-v3-1 | Check 3.1f | code | codex-review | codex-review | report copy broke after failover; capacity match too wide | - |
| 2026-10-30 | Loop2-loop-v3-1 | Build | env | manager | gate | Codex "model is at capacity" did not fail over to the backup | Loop2-loop-v3-1 (3.1f failover) |
| 2026-10-30 | Loop2-loop-v3-1 | Check 3.1g | code | codex-review | codex-review | lint failed escaped Markdown links; file as -Root passed | - |
| 2026-10-30 | T1-roadmap-overview | Check T1a | judgment | manager | manager | builder skipped the live refresh check in Done means; loop chat ran it | - |
| 2026-10-30 | T1-roadmap-overview | Hand test r1 | plan | hand-test | plan | approved lane design never drew where a branch starts or joins | - |
| 2026-10-30 | T1-roadmap-overview | Check r2 | plan | manager | plan | r2 brief set the fork parent as merge-base with dev; wrong for branch-from-branch | - |
| 2026-10-30 | T1-roadmap-overview | Build r3 | env | user | plan | Codex runs kept starting servers its sandbox cannot bind; xhigh on a Low item; user stopped the run; override: loop chat finished r3 on the user's go (rule "Codex implements") | idea: codex-speed-by-tier |

## Scores
One row per merged item: what each check found. A reviewer with many empty rows on items that later had misses is not
earning its run.

| Date | Item | Tier | Rework rounds | Gate fails | Codex review (proven) | Design review (findings) | Hand-test fails | Misses after merge |
|---|---|---|---|---|---|---|---|---|
| 2026-10-29 | A55-split-view-clip | Medium | 0 | 0 | 0 | - | 0 | - |
| 2026-10-29 | A42c-checkbox-indent | Medium | 0 | 0 | 0 | - | 0 | - |
| 2026-10-29 | A42d-list-commands | Medium | 0 | 0 (1 env, offline pass) | 0 | - | 0 | - |
| 2026-10-29 | C30-loop-docs-v22-plan | Low | 1 (dormant follow-up) | 0 | - | - | - | - |
| 2026-10-29 | Loop1-process-v3 | Low | 0 | 0 | - | - | - | - |
| 2026-10-30 | Loop2-loop-v3-1 | Low | 5 (-r2 on a, b, c, f, g) | 0 | 7 | - | - | - |
| 2026-10-30 | T1-roadmap-overview | Low | 2 (-r2, -r3; r3 finished by the loop chat) | 0 | - | - | 1 | - |
