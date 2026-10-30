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
