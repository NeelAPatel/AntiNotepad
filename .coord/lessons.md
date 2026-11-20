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
| 2026-11-19 | Loop16-poteto-trigger | Live check, review | test, code | codex-review | test | Stop self-tests were built from a hand-made transcript, so the real `isMeta` skill message, CRLF and a lowercase marker slipped; build hook self-tests from real transcript shapes | - |
| 2026-11-19 | Loop16-poteto-trigger (merge) | Merge | plan | merge-preflight | brief | the card heading was `## Check (item chat, date)`; preflight needs a bare `## Check` | - |
| 2026-11-19 | Loop17-chat-title-selfheal | Check | code | codex-review | test | paused/parked rows were skipped, a bare `paused` row lost its phase, and `done` gave no `[Merged]`; build title-hint self-tests from real STATUS row shapes | - |
| 2026-11-19 | Loop17-chat-title-selfheal | Build | plan | user | item chat | the chat changed its title before its STATUS row; set the row and the title in the same step | - |
|---|---|---|---|---|---|---|---|
| 2026-11-18 | A26-tab-at-text-start | Build | plan | codex-review | brief | Findings said Alt+Shift+arrows were unbound; they were taken at window level (first/latest note). Grep window-level key handlers in `UI/` before proposing any new shortcut | - |
| 2026-11-17 | A21-save-delay-setting (merge) | Merge | judgment | user-later | brief | the director staged A22/A23 cards and launch.json into the A21 merge commit; approve.ps1 then refused the next item (card must be untracked). At merge, `git add` only STATUS, lessons, pending, archive and the item's own files | - |
| 2026-11-17 | A23-paste-keep-structure | Review r4 | code | codex-review | brief | deep-nesting stack overflow, quadratic parse and dropped overlapping Ctrl+V found at review; a stress selftest (deep nesting, big input, overlapping calls) should have caught them; paste and parse briefs need size and depth checks up front | - |
| 2026-11-17 | A21-save-delay-setting | Build r1, r3 | env | gate, hand-test | brief | r1 ended without a green selftest and r3 hit the 45-minute limit with no report; ask Codex for a draft report after the first green selftest (done from r4). Preflight stale Head again (T16, A21): item chat edits the card after the gate | - |
| 2026-11-09 | A9-pinned-accent-ring | Build r1 (gate) | test | gate | brief | the new ring selftest hung the whole selftest (it set up Avalonia differently from the Design dump and stalled a later contract); a PadWindow selftest must install the Fluent theme, use a loaded seeded store and run last among contracts | - |
| 2026-11-06 | A7-version-rc2 | Merge | plan | master | review | merge preflight "stale Head" again (third time: A2, A5, A7); master re-ran the gate in the worktree | - |
| 2026-11-06 | Loop8-merge-policy | Build (scratch test) | code | builder | brief | per-commit selftest always failed: the app is a GUI-subsystem exe, so `& $exe` returns no output; copy the gate's process runner whenever a script launches the app | - |
| 2026-11-17 | A24-save-delay-cleanup | Build | plan | item chat | codex-review | plan step "clear the flag when a note is dropped" not implemented (a deleted note keeps its own unused flag, no store state left); card edit silently missed on CRLF line endings, so preflight failed on empty Check | - |
| 2026-11-19 | A32-selection-convert-menu | Check | process | item chat | codex-review | format failure got past Codex review; brief should have run dotnet format first | - |
| 2026-11-19 | A29-shortcut-map | Build run 1 | env | gate, run overlap | brief | Codex worker run 1 timed out on a hung selftest (gate and run overlapped) and wrote no report | - |
| 2026-11-19 | A29-shortcut-map | Hand test | plan | user | lab | lab export kept Ctrl+Enter as a default and the Help row icon was off; the user wanted no default key | - |
| 2026-11-20 | A31-list-default-type | Check | code | gate | brief | gate Scope and Format failed on the card; lesson: one path per Files bullet | - |
| 2026-11-20 | A34-pill-bar-per-mode | Approve | plan | files-overlap tool | card writer | card had no `## Files` section so approve.ps1 failed; add Files bullets when writing the Plan | - |
| 2026-11-20 | A34-pill-bar-per-mode | Build | plan | Codex run 1 stopped | card writer | brief named a file outside Files; check the Files list against the brief | - |
| 2026-11-20 | A34-pill-bar-per-mode | Gate | code | gate Format | brief | Codex edits could not be gate-formatted offline; run `dotnet format whitespace --include <file>` before reporting | - |
| 2026-11-20 | A34-pill-bar-per-mode | Build r11 | plan | item chat | plan | r11 on terra produced no changes twice for a large refactor; a refactor over 4 files with shared state starts on `worker` | - |
| 2026-11-20 | A34-pill-bar-per-mode | Check | brief | codex-review | brief | brief did not forbid eager data reads on the hide path; briefs for chrome code state no new work on the show/hide path | - |
| 2026-11-20 | A34-pill-bar-per-mode | Merge | env | merge-preflight | - | per-commit selftest failed once then passed on retry (flaky); design-hash updates sat in later commits and were folded into the first bar commit | - |

## Scores
One row per merged item: what each check found. Empty rows on items that later had misses mean the reviewer is not earning its run.

| Date | Item | Tier | Rework rounds | Gate fails | Codex review (proven) | Design review (findings) | Hand-test fails | Misses after merge |
|---|---|---|---|---|---|---|---|---|
| 2026-11-19 | Loop16-poteto-trigger | Medium | 1 | 0 | 2 | - | 0 | - |
| 2026-11-19 | A29-shortcut-map | Medium | 4 | 0 | 5 | - | 2 | - |
| 2026-11-20 | A31-list-default-type | Medium | 1 | 1 | 0 | - | 0 | - |
| 2026-11-20 | A33-help-toggle-f1 | Low | 0 | 0 | 0 | - | 0 | - |
| 2026-11-20 | A34-pill-bar-per-mode | Medium | 4 | 2 | 4 | - | 4 | - |
| 2026-11-20 | A36-version-rc4 | Low | 0 | 0 | 0 | - | 0 | - |
| 2026-11-20 | A37-version-0-7-0 | Low | 0 | 1 | 0 | - | 0 | - |
| 2026-11-19 | A32-selection-convert-menu | Medium | 4 | 1 | 1 | - | 0 | - |
| 2026-11-19 | Loop17-chat-title-selfheal | Medium | 1 | 0 | 2 | - | 0 | - |
| 2026-11-19 | Loop20-codex-labs-and-scout | Medium | 0 | 0 | 0 | - | 0 | - |
| 2026-11-19 | Loop19-review-task-chat-runnable | Low | 0 | 0 | 0 | - | 0 | - |
| 2026-11-08 | L1-lv4-native-guards | Medium | 0 | 0 | - | - | 0 | - |
| 2026-11-08 | L2-lv4-worktree-rules | Medium | 0 | 0 | - | - | 0 | - |
| 2026-11-08 | L3-lv4-agent-roster | Low | 1 (round 2 tests) | 0 | - | - | 0 | - |
| 2026-11-08 | L4-lv4-skill-diet | Low | 0 | 0 | - | - | 0 | - |
| 2026-11-08 | L5-lv4-status-line | Low | 1 (r2 wrap global line) | 0 | - | - | 0 | - |
| 2026-11-08 | L6-lv4-docs-diet | Low | 1 (r2) | 0 | - | - | 0 | - |
| 2026-11-08 | L7-lv4-playbook | Low | 0 | 0 | - | - | 0 | - |
| 2026-11-09 | A9-pinned-accent-ring | Medium | 3 (r2 selftest hang, r3 pin must not fade chrome) | 1 | 0 | - | 1 (pin button faded toolbar; fixed in r3) | - |
| 2026-11-12 | A10-hotkey-recorder | Medium | 4 (r2 Esc routing, r3 format, r4 no Windows key) | 1 | 0 | - | 1 (Windows key allowed; fixed in r4) | - |
| 2026-11-12 | A12-version-rc3 | Low | 0 | 0 | - | - | 0 | - |
| 2026-11-12 | Loop9-loop-slim | Medium | 0 (override: one umbrella branch for steps a-g instead of one branch per step, user "Approve, start it") | 0 | - | - | 0 | - |
| 2026-11-12 | Loop10-status-split | Medium | 1 (Check missing at merge; builder ran bounds check itself, /review-task is user-only) | 0 | - | - | 0 | - |
| 2026-11-12 | T12-settings-drift-readers | Low | 1 (Hand test line missing at preflight) | 0 | - | - | 0 | - |
| 2026-11-12 | T13-roadmap-fingerprint | Low | 0 (stale gate head at preflight; re-gated at tip) | 0 | - | - | 0 | - |
| 2026-11-12 | A13-search-toggle-off | Low | 0 (Check section missing at preflight; build chat filled it) | 0 | - | - | 0 | - |
| 2026-11-12 | A14-search-open-fade-up | Low | 0 (builder fixed Codex flaw: hide must end overlay at once; stale gate head at preflight) | 0 | - | - | 0 | - |
| 2026-11-12 | A15-notes-edge-pills | Low | 0 (scope widened at Approve: Win11 toast motion, by user) | 0 | - | - | 0 | - |
| 2026-11-12 | A16-saved-label-fade | Low | 0 | 0 | - | - | 0 (pass taken from user's /merge-task words, not a direct reply) | - |
| 2026-11-12 | A17-settings-toggle-animation | Low | 0 | 0 | - | - | 0 (hover fill tried and dropped at user request) | - |
| 2026-11-12 | A18-settings-segment-animation | Medium | 1 (round 2: glide lost on Settings rebuild) | 0 | - | - | 0 | - |
| 2026-11-13 | A19-settings-scroll-feel | Medium | 0 | 0 | - | - | 0 | - |
| 2026-11-15 | A20-overscroll-speed | Medium | 2 (r2 carry-over, r3 vertical overscroll dropped by user) | 0 | - | - | 0 | - |

| 2026-11-12 | Loop11-one-chat-flow | Low | 0 (user asked to fix then merge: card intro typo and lost sentence fixed on branch) | 0 | - | - | 0 | - |
| 2026-11-13 | T14-dirty-allowlist-codex-wake | Low | 0 (A16/A17 launched Codex detached with & or nohup, so no exit wake-up; Hand test section was missing at preflight) | 0 | - | - | 0 | - |
| 2026-11-16 | T16-roadmap-table-columns | Low | 0 (scope grew twice at the user's word: groups/filters, Excel-style filters; Check and Hand test sections missing and gate stale after the card commit at preflight) | 0 | - | - | 0 | - |
| 2026-11-17 | A21-save-delay-setting | High | 5 (r2, r4, r5, r6 plus r3 timed out) | 1 (Scope: Design.cs missing from Files) | 3 (2 major, 1 minor, fixed r5) | accept-with-followup (3, in pending) | 2 | - |
| 2026-11-17 | Loop12-chat-title-scheme | Medium | 0 (gate stale after the hand-test card commit at preflight) | 0 | - | - | 0 | - |
| 2026-11-17 | Loop12-chat-title-scheme-r2 | Medium | 0 (rework of a merged item) | 0 | - | - | 0 | - |
| 2026-11-17 | A22-wrap-hanging-indent | High | 1 (r2: format gate, result chips on wrapped lines) | 1 (Scope: directory Files entry; Check and Hand test sections missing and branch behind dev at preflight) | - | - | 0 | - |
| 2026-11-17 | A23-paste-keep-structure | Medium | 4 (r2-r5; scope grew to tables, marks, emoji, toast) | 0 | 3 (2 major, 1 minor, fixed r5) | - | 0 (1 emoji drawing gap logged, not a paste fault) | - |
| 2026-11-17 | A24-save-delay-cleanup | High | 0 | 0 | 0 | 0 | 0 | -
| 2026-11-17 | T17-multi-dev-instances | High | 0 | 0 | 0 | 1 (accept; tooltip removed) | 0 | -
| 2026-11-18 | A25-page-edge-stuck-after-hide | Low | 0 | 0 | - | - | 0 | -
| 2026-11-18 | Loop15-preflight-stale-head | Low | 0 | 0 | - | - | 0 | -
| 2026-11-18 | D3-release-checklist | Low | 0 | 0 | - | - | 0 | -
| 2026-11-18 | A21b-save-scheduler-extract | High | 0 | 0 | 0 | 0 | 0 | -
| 2026-11-18 | A27-recovered-folder-always-open | Medium | 0 | 1 | 0 | - | 0 | -
| 2026-11-18 | A26-tab-at-text-start | Medium | 2 | 0 | 1 | - | 0 | -
Older rows: `.coord/lessons-archive.md` (budget: 3 KB here).
