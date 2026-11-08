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
| 2026-10-31 | Loop4-retro-fixes | Build | judgment | manager | manager | a cleanup `rm -rf .coord/reports` in the worktree deleted tracked reports; restored from git at once | - |
| 2026-10-31 | Loop4-retro-fixes | Build | judgment | - | - | override: the loop chat built the item itself on the user's go (rule "Codex implements") | - |
| 2026-11-01 | A1-selftest-split | Check | judgment | reviewer | builder | build report missing the `## Plan deviations` heading; added at review (fill the report from the template headings before committing) | - |
| 2026-11-01 | A1-selftest-split | Build | judgment | - | - | override: the builder ran on Haiku, not Sonnet medium, on the user's go ("replace codex subagents with haiku"; rule: loop.md Sessions builder row) | - |
| 2026-11-01 | A2-store-split | Merge | plan | master | review | merge preflight "stale Head": `/review-task` committed after its gate, gate re-run by the master (second time after A1) | idea: review-gate-stale-head |
| 2026-11-01 | A3-calc-split | Check | env | reviewer | gate | gate Format flags every line of a split's new files; a whitespace pass was needed, so the sorted-lines proof ran before it, not after | idea: gate-format-split-baseline |
| 2026-11-01 | A4a-token-scan | Merge | judgment | - | - | override: steps squash onto umbrella branch `feat/A4-design-tokens`, which lands on dev once at the end, on the user's go ("branch off dev for feat/design-tokens, and then sub branches"; rule: one squashed commit per item onto dev, `docs/loop.md`); `tools/approve.ps1` always bases on dev, so step branches are cut by hand | idea: approve-base-branch |
| 2026-11-02 | A4b-design-dump | Build r1 | plan | reviewer | report | r1 wrote no code: brief left hidden-window cleanup and reaching the private `PadMenu.BuildMenu` undecided; r2 brief decided both | - |

| 2026-11-06 | A5-search-bar-overlap | Check (gate r1) | plan | gate | plan | Files listed the `SelfTest/` directory; name the exact area file | - |
| 2026-11-06 | A5-search-bar-overlap | Check (gate r1) | code | format | codex-review | two properties per line in an initializer; brief "one property per line" | - |
| 2026-11-06 | A5-search-bar-overlap | Check (gate r2) | plan | gate | plan | round brief said "none" under Files; always list a file under round Files | - |
| 2026-11-06 | A5-search-bar-overlap | Merge | plan | master | review | merge preflight "stale Head": catch-up for a STATUS-only dev commit forced a full gate re-run | - |
| 2026-11-06 | A6-note-switch-instant-swap | Check (gate r2) | plan | gate | plan | round brief lacked a Files list (second time after A5 r2) | idea: round-brief-files |
| 2026-11-06 | A6-note-switch-instant-swap | Merge | judgment | - | - | override: merged before the final hand test on the user's go ("Merge A5 and A6, both into dev, then run a new build"; rule: hand test before Merge, docs/loop.md) | - |
| 2026-11-08 | L7-lv4-playbook | Approve | judgment | - | - | override: L7 started with 3 items already building (cap 4, `approve.ps1 -Cap 4`) on the user's go ("yes L7 can start building by itself"; rule: cap 3 past Approve, docs/loop.md) | - |
| 2026-11-08 | L2-lv4-worktree-rules, L5-lv4-status-line | Approve | judgment | - | - | override: both passed Approve with L3, L4 and L7 holding the 3 slots (one-off cap bypass, cap stays 3) on the user's go, given in the "Agentic Levels 4+5 · native features" chat and relayed by it (rule: cap 3 past Approve, docs/loop.md) | - |
| 2026-11-06 | A7-version-rc2 | Merge | plan | master | review | merge preflight "stale Head" again (third time: A2, A5, A7); master re-ran the gate in the worktree | - |
| 2026-11-06 | Loop8-merge-policy | Build (scratch test) | code | builder | brief | per-commit selftest always failed: the app is a GUI-subsystem exe, so `& $exe` returns no output; copy the gate's process runner whenever a script launches the app | - |

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
| 2026-10-31 | Loop4-retro-fixes | Low | 1 (-r2: wording the user asked for) | 0 | - | - | - | - |
| 2026-11-01 | A1-selftest-split | Low | 0 | 0 | - | - | - | - |
| 2026-11-01 | A2-store-split | Low | 0 | 0 | - | - | - | - |
| 2026-11-01 | A3-calc-split | Low | 0 | 0 | - | - | - | - |
| 2026-11-01 | T2-hot-files-stale-path | Low | 0 | 0 | - | - | - | - |
| 2026-11-01 | T3-codex-unpause-docs | Low | 0 | 0 | - | - | - | - |
| 2026-11-01 | T5-review-gate-stale-head | Low | 0 | 0 | - | - | - | - |
| 2026-11-01 | T7-paused-state | Low | 0 | 0 | - | - | - | - |
| 2026-11-01 | T4-selftest-diff-skip-timing | Low | 0 | 0 | - | - | - | - |
| 2026-11-01 | T6-gate-format-split-baseline | Low | 0 | 0 | - | - | - | - |
| 2026-11-02 | Loop5-reply-style | Low | 0 | 0 | - | - | - | - |
| 2026-11-04 | Loop6-loop-v5-wiki | Low | 0 | 0 | - | - | - | 0 |
| 2026-11-06 | T9-roadmap-step-rows | Low | 0 | 1 | - | - | - | - |
| 2026-11-06 | Loop7-top-level-rules | Low | 1 | 0 | - | - | - | - |
| 2026-11-06 | Loop8-merge-policy | Medium | 0 | 0 | 4 | - | - | - |
| 2026-11-06 | T10-roadmap-live-reload | Low | 0 | 0 | - | - | - (user said "merge it" before reporting checks) | - |
| 2026-11-06 | A5-search-bar-overlap | Medium | 1 (-r2 whitespace) | 2 | 0 | - | 0 | - |
| 2026-11-06 | A6-note-switch-instant-swap | Medium | 1 (-r2 lab removal) | 1 | 0 | - | - (merged before hand test) | - |
| 2026-11-06 | A7-version-rc2 | Low | 0 | 0 | - | - | - (no visible change) | - |
| 2026-11-06 | T11-roadmap-hide-rows | Low | 0 | 0 | - | - | 0 | - |
| 2026-11-08 | L1-lv4-native-guards | Medium | 0 | 0 | - | - | 0 | - |
| 2026-11-08 | L2-lv4-worktree-rules | Medium | 0 | 0 | - | - | 0 | - |
| 2026-11-08 | L3-lv4-agent-roster | Low | 1 (round 2 tests) | 0 | - | - | 0 | - |
| 2026-11-08 | L4-lv4-skill-diet | Low | 0 | 0 | - | - | 0 | - |
