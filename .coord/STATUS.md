# STATUS

Updated: 2026-11-06
Product: AntiNotepad (`product/AntiNotepad`), version label 0.6.0 (untested).

## Working rules right now
- Two points of contact (user, 2026-10-30: "This chat is Master point of contact for the actual app development. Loop Hub 1 is the master for the meta level items such as Dev Loop"):
  - `Master 2 · Loop v5 · era-1-start`: app development (`A`, `D` items).
  - `Loop Hub 2 · Loop v5 · loop-tools`: meta items (`Loop`, `T`: the dev loop, the roadmap portal, tooling). Took over
    from `Loop Hub 1 · Loop v3 · dev-loop (closed)` on 2026-11-01 (user, relayed by Master 2); its notes are
    `.coord/directors/loop-hub-2.md`. It edits only its own `Loop`/`T` rows plus one "Now" line of its own; Master 2
    owns STATUS overall and every `A`/`D` row.
  - Both squash-merge onto dev from the main checkout: before a merge, `git status`, check the other has no merge in
    progress, and tell the other chat in one line.
  - The previous master, `Master 1 - loop v2 - bugs-design-system-ownership` ("Era0 - ..."), is closed (2026-10-29).
- **Loop v3.2 is live** (Loop4-retro-fixes, 2026-10-31; v3.1 was Loop2-loop-v3-1). **Loop freeze:** lifted (Loop4-retro-fixes settled the retro's questions); rules change only on a miss repeated in `.coord/lessons.md` or on the user's ask.
- **Codex is allowed** (paused 2026-10-31; lifted by the user 2026-11-01: "Codex allowed"). Codex roles are picked by the job
  (`docs/agents.md`): quick = Luna medium, terra = Terra medium, worker = Luna xhigh, director = Sol medium; no run caps;
  Codex checks run offline only. Claude briefs, reviews, commits.
- Computer Use is off until the user's explicit go-ahead; live checks are done by the user by hand in the dev copy.
- Never run `AntiNotepad.exe --quit` without `ANTINOTEPAD_DATA`: it targets the installed copy's instance.

## Where things are
- **History rewritten (2026-10-29, user: "Yes, rewrite"):** 623 era-0 commits became 156 (option B, preview
  `.coord/history-preview.md`). `main` and `dev` both at 8d18f72 (`Loop1-process-v3`), files identical to the old dev.
  Commit hashes quoted below and in older cards and reports point into the old history.
- **Tags now:** `v0.1.0`, `milestone/overnight-rebuild`, `v0.5.0`, `v0.6.0-rc1` to `rc9` (rc9 = the old
  `antinotepad-v0.6.0-untested-r9`; each release tag checked to carry exactly the files it shipped). All other old tags
  were deleted locally and on GitHub.
- **Old history kept:** tags `archive/full-history-2026-10-29` (old dev tip) and `archive/main-2026-10-29` (old main
  tip) on GitHub, plus the bundle `F:\LocalVSProj\Visual Studio Apps\AntiNotepad-history-backup-2026-10-29.bundle`
  (all refs, verified).
- Installed copy: `v0.7.0-rc1` (user installed 2026-11-01 with `tools/install.ps1`; dev builds removed the same day). Windowing is still r4's (r6 rollback, 2026-10-25).
- Locked choices: `docs/decisions.md`.

## Now
- Master 4 (top-level chat beside Loop Hub 2, user 2026-11-06; took over design-tokens from Master 3; notes `.coord/directors/master-4.md`): A4-design-tokens merged to dev 2026-11-05; branch feat/A4-design-tokens kept with its worktree. 2026-11-06: STATUS tidied and the roadmap brought up to date. Next (user order): plan of action for search-bar-overlap and note-switch-instant-swap (two worktrees, Codex), then the v0.7.0-rc2 release. Design backlog: scale-apply, design-plugins-review, timer-names (deferred).
- Release `v0.7.0-rc1` (user, 2026-11-01): main fast-forwarded to dev after Era 1, tagged, pushed. Release build 0 warnings, SELFTEST PASS. User installed it on the production copy; dev builds removed.
- Loop Hub 2: T9-roadmap-step-rows (Low, side tool) **done** (2026-11-06; Master 4's fix request for the user's "update the roadmap site end to end"): umbrella step rows show in their era, step cards fill their drawers, paused rows leave "Moving without you"; gate on dev PASS. Earlier: Loop6-loop-v5-wiki done (loop v5.0.0); GitHub wiki republish waits for the user's go.
- Loop Hub 2: T6-gate-format-split-baseline (Low, docs-tools) **done** (2026-11-01): gate Format counts lines moved verbatim into a new file as older code (limit: common lines like `}` always match); gate on dev PASS. All Loop Hub 2 items from the Era 1 go are done.
- Loop Hub 2: T4-selftest-diff-skip-timing (Low, docs-tools) **done** (2026-11-01): `docs/coding.md` "How to split" says how to prove a pure move without timing lines; gate on dev PASS.
- Loop Hub 2: T7-paused-state (Low, docs-tools) **done** (2026-11-01): plan rows can be `paused` (branch, worktree, card kept; not counted against the cap; hook skips them); gate on dev PASS. Master 2 sets D1-ui-region-names to `paused`.
- Loop Hub 2: T5-review-gate-stale-head (Low, docs-tools) **done** (2026-11-01; user go via Master 2): merge preflight now accepts card/report-only commits after the gate; gate on dev PASS. `v0.7.0-rc1` Release marker removed (user via Master 2: "no release points at this time").
- Loop Hub 2: T2-hot-files-stale-path (Low, docs-tools) **done**, squashed onto dev (2026-11-01, user: "Approve both"); gate on dev PASS. T3-codex-unpause-docs (Low, docs-tools) **done** too: the "Codex is paused" rule is gone from `AGENTS.md`, `docs/loop.md`, `docs/agents.md`; gate on dev PASS.
- D1-ui-region-names (Low, docs-tools): **paused indefinitely** (user, 2026-11-01). Branch pushed at d96a11f; review of names
  and pictures (its hand test) is what is left. Worktree kept.
- A3-calc-split (Low, pure-move): **done**, squashed onto dev (2026-11-01, user's merge order A2, A3, D1); gate on dev PASS.
  `Calc/Calculator.cs` 1,383 -> 219 lines plus six partial parts (Units, Dates, Values, Lexer, Parser, SelfTest), each
  under 600; whitespace-only format pass. Docs for both splits (`docs/coding.md`, `AGENTS.md`) updated. Built by Codex quick.
- A2-store-split (Low, pure-move): **done**, squashed onto dev (2026-11-01, user's merge order A2, A3, D1); gate on dev PASS.
  `Model/Store.cs` 1,006 -> 556 lines plus `Note.cs`, `Settings.cs`, `Model/Search/SearchTypes.cs`, `Model/Search/Store.Search.cs`
  (sorted lines 670 = 670). Built by Codex quick. A3-calc-split now catches up and writes the docs.
- A1-selftest-split (Low, pure-move): **done**, squashed onto dev (2026-11-01, user: "Merge it"); gate on dev PASS.
  `SelfTest.cs` is now ten `SelfTest/` files by area (each under 600 lines), moved verbatim (sorted lines 2,950 = 2,950).
  Built by a Haiku sub-session. Next: D1-ui-region-names / store-split (numbers on your yes).
- Loop4-retro-fixes (Low, docs-tools): **done**, squashed onto dev (2026-10-31, user: "fix these and merge"); gate on dev
  PASS. Loop v3.2: Codex roles by job, no caps, offline Codex checks, expected time, fast lane, masters per area,
  master notes in `.coord/directors/` (gitignored). Master 2 told in one line.
- T1-roadmap-overview (Low, docs-tools): **done**, squashed onto dev (2026-10-30, user: "okay it works ... merge it into
  dev"); gate on dev PASS. Live overview at http://localhost:47811 (`tools/roadmap-serve.ps1`, auto-started by the
  status-card hook); how-to in `docs/wiki/Roadmap-Portal.md`, linked from `README.md`. Follow-ups are `idea:` rows in Plan.
- Loop2-loop-v3-1-wiki (Low, docs-tools): **done**, squashed onto dev (2026-10-30, user: "Merge to dev and publish"); gate
  on dev PASS. `docs/wiki/` rewritten for loop v3.1 (Getting Started, Parallel and Branches, Backlog Scans, a Loop2 worked
  example, Known limits; every claim cites a file), two Codex accuracy reviews accept. Noticed: `docs/loop.md` "Sessions"
  says the auditor "commits" its report, but "Branches" and `/next-task` step 2 commit nothing before Approve (docs fix,
  needs you; loop freeze applies).
- Loop2-loop-v3-1 (Low, docs-tools): **done**, step commits fast-forwarded onto dev (2026-10-30, user: "manage it to
  completion"). dev runs **loop v3.1**: `tools/approve.ps1`, `tools/catch-up.ps1`, `tools/merge-preflight.ps1`,
  `tools/files-overlap.ps1`, `tools/scan-delta.ps1` + `/scan-backlog`, strict gate scope, `tools/loop-lint.ps1`,
  read-only Codex audits; dormant v2.2 absorbed (`tools/v2.2/` removed). Rehearsal 8/8 PASS. Left: the wiki pages, after
  the user's OK on the flow diagram (https://claude.ai/artifact/4CfTQAuvL4ZXwBXwYsqWi7). Next: Master 2 starts the backlog.
- **Era 0 ended (2026-10-29); era 1 started (user, 2026-10-30: "Old era is Era 0, we are now in Era 1").** Process redesign settled (`.coord/process-redesign.md`) and rolled out as **loop v3**
  by Loop1-process-v3 (squash-merged; gate on dev PASS). History rewritten (see "Where things are"). Next: era 1 with
  fresh counters and a new master, `Master 2 · Loop v3 · <slug>`; first candidates are the three file splits from
  `docs/coding.md`, `D1-ui-region-names` and `idea: wiki-loop-v3`. Any other clone of the repo must be re-cloned.
- C30-loop-docs-v22-plan merged into dev (e2898d1, 2026-10-29, user: "merge it to main"; gate on dev PASS, CONTRACTS 13
  rows: 12 pass, 1 manual): loop v2.2 plan (`docs/loop-v2.2-plan.md`), wiki source `docs/wiki/` (published to
  AntiNotepad.wiki.git 78965eb), and the dormant v2.2 drop (`docs/v2.2/`, `tools/v2.2/`, called by nothing). dev and main
  stay on loop v2.1 until the user triggers v2.2 (`docs/v2.2/ACTIVATION.md`). Noticed: `docs/loop.md` contradictions
  (v2 attribution, gate row, rework counting, parallel items vs one dev copy), listed in the C30 card.
- Release r9 (user, 2026-10-29): dev merged into main --no-ff (8ec1423), tag `antinotepad-v0.6.0-untested-r9`, pushed.
  Loose threads found (not touched; user is resetting history and naming in the session "Deleting old commits"):
  unmerged `feat/A38-mvvm-audit` (1 commit, audit doc) and `feat/C30-loop-docs-v22-plan` (since merged, below);
  local-only `feat/loop-article` (already in dev); 5 reorg worktrees (`reorg/A40`-`A44`, merged; 3 hold untracked Codex
  logs); 5 orphan folders in `.claude/worktrees/` (A45, A53, A54, C25 about 3.6 GB of build output, C20 empty).
- A42d-list-commands merged into dev (6089740, 2026-10-29; user hand test "all passed" on checks 1-6): list Enter
  continuation, list recognition and Renumber decided in `Editor/ListEditing`, applied line by line with live offsets by
  `Editor/DocumentCommands`; `PadView` keeps routing, both undo scopes, the post-edit caret read, timer Enter fall-through
  and thin `Insert` (-75 lines). CONTRACTS `list-commands-preserved`. Gate on dev PASS (0 warnings, SELFTEST PASS,
  CONTRACTS 13 rows: 12 pass, 1 manual). A42 reorg done. Check-session sandbox could not reach nuget.org (NU1900); Master 1
  ran the online gate (lessons: env). Next: C29-headless-render-probe (user order).
- A42c-checkbox-indent merged into dev (178d4c0, 2026-10-29; user hand test "All of these pass" on checks 1-5):
  checkbox toggle and Tab/Shift+Tab indent edits decided in `Editor/ListEditing` (checkbox regex moved there, groups
  unchanged), applied by new `Editor/DocumentCommands`; `PadView` keeps routing and Tab's single undo group (-28 lines).
  CONTRACTS `checkbox-indent-preserved`. Gate on dev PASS (0 warnings, SELFTEST PASS, CONTRACTS 12 rows: 11 pass, 1
  manual). New idea from the hand test: timer-enter-no-autostart (Plan, needs you). Next: A42d-list-commands, then
  C29-headless-render-probe (user order).
- C28-loop-cheap-checks merged into dev (c30a063, 2026-10-29, user: "lets do 1-5"; gate on dev PASS): loop v2.1. Gate
  Format step (new problems on changed lines only), Codex ledger and logs in the main checkout, triage on every failure
  (`/review-task` 4a), `.coord/lessons.md` (master-owned; Misses and Scores filled at Merge), quiet relays. Master 1
  continues as is (minor version). Queued: C29-headless-render-probe (product code, after A42c-checkbox-indent).
- Release r8 (user, 2026-10-29: "Release now"): dev merged into main --no-ff (42ad744), tag
  `antinotepad-v0.6.0-untested-r8`, both pushed. Release build 0 warnings, SELFTEST PASS. Next: A42c-checkbox-indent.
- A55-split-view-clip merged into dev (2a7a482, 2026-10-29; user hand test "all passed" on checks 0-5): cause was each
  PadView sitting directly in the split grid while NoteSwipe/arrival translate the whole PadView, so a pulled pane slid over
  the divider. Each pane now sits in a stationary ClipToBounds host in its column; motion unchanged; right host follows
  split. CONTRACTS `split-pane-clipping` (structural pass, pixel containment manual). Gate on dev PASS (0 warnings,
  SELFTEST PASS, CONTRACTS 11 rows: 10 pass, 1 manual). First item run by Master 1 on loop v2. New unnumbered idea from the
  hand test: floating-scrollbar (Plan, needs you). Next: Release marker (dev -> main, on your go), then A42c-checkbox-indent.
- C27-session-naming merged into dev (c719c06, 2026-10-29, user request; gate on dev PASS): loop version v2; one orchestrating master named
  `Master <N> - loop v<V> - <slug>`; step sub-sessions `<ID>-<step>-<slug>` (scan/audit Haiku; build/review/test Sonnet
  medium); handover checklist and the new master's resume prompt in `docs/loop.md`. Old master closed. Next:
  A55-split-view-clip, the first item on the item card, in Master 1.
- C26-item-card-loop merged into dev (2d99362, 2026-10-29, user: "Yes, merge it"): every item has one item card
  (`.coord/templates/item.md`) through Request > Findings > Plan > Go > Build > Check > Hand test > Merge; Findings for
  every tier (master scout for Low), reproduction before building a bug, gates picked per item, a bounds check (requested
  vs approved vs done) at Check, and the five-line status card on every reply, reminded by a `UserPromptSubmit` hook
  (`tools/loop-card.ps1`). Gate on dev PASS (0 warnings, SELFTEST PASS, CONTRACTS 10 rows: 9 pass, 1 manual). Proposed
  follow-up (unnumbered, needs the user): `codex-run.sh` writes `usage.tsv` and logs to the worktree, so caps and logs
  are lost on worktree removal. Next: A55-split-view-clip, the first item on the new card.
- A45-dev-build-badge merged into dev (56a85cd, 2026-10-29; user hand test passed): `tools/dev.ps1` stamps the branch and
  build time into dev builds only; the top-bar chip shows ` <ID> · <time>`, expands on hover to
  `dev/<branch>@<date time>`, tooltip always full. CONTRACTS `dev-badge-build-stamp`. Gate on dev PASS (0 warnings, SELFTEST
  PASS, CONTRACTS 10 rows: 9 pass, 1 manual). Next: A55-split-view-clip.
- C25-dev-instance-handoff merged into dev (2f6c123, 2026-10-29; user hand test passed 4 of 4): cause was every dev run
  sharing the `AntiNotepad-dev` mutex while `dev.ps1` tracked only its own checkout's PID. `tools/dev.ps1` now takes a
  launcher mutex, sends the graceful dev quit via a staged helper, force-closes only a verified repo dev build if that
  fails, then builds and launches; sub-sessions open their build after review acceptance. Gate on dev PASS; Codex review
  accept; design review accept-with-followup. Proposed follow-ups (unnumbered, need the user): pin dev.ps1's copy of the
  mutex names to `SingleInstance.cs` (comment + gate check); comment the checkout-root assumption; skip the helper build
  when no dev copy runs. Next: A45-dev-build-badge.
- C24a-roadmap-local-page merged into dev (6b5483f, 2026-10-29, user: "local page is fine"): `/roadmap` opens
  `.coord/roadmap.html`, rebuilt by PostToolUse and Stop hooks and reloading itself every 3 s; no republish prompts. The
  Artifact stays, published only on request.
- A54-link-hover-click merged into dev (f32b92a, 2026-10-29; user hand test passed): cause was `PadView.OnViewMoved`
  resetting the cursor to the I-beam after AvaloniaEdit set the hand; Ctrl+click now handled in the tunnel press. User widened
  scope in r3: every URL (short, expanded on the caret line, Shorten links off) is Ctrl+clickable. CONTRACTS
  `shortened-link-opens`. Gate on dev PASS (0 warnings, SELFTEST PASS, CONTRACTS 9 rows: 8 pass, 1 manual). Next: A55-split-view-clip.
- A53-split-divider-height merged into dev (7299e8b, 2026-10-29): after hand-test rounds (full height, mirrored gap) the user
  kept the divider ending above the pager strip; merged as a guard, no visible change: CONTRACTS `split-divider-above-pager`.
  Gate on dev PASS (0 warnings, SELFTEST PASS, CONTRACTS 8 rows: 7 pass, 1 manual). Next: A54-link-hover-click.
- C24-roadmap-artifact (user, 2026-10-29): `/roadmap` widget replaced by a roadmap Artifact
  (https://claude.ai/artifact/VU8guxUCfxJTSZ1MxNaqyy) that a Claude Stop hook republishes when the Plan changes (main
  checkout only). Merged into dev (783e9ab, 2026-10-29, user asked); gate on dev PASS.
- C23-idea-numbering (user, 2026-10-29): numbers only on user confirmation; A53+ dissolved; Plan redrawn as unnumbered
  ideas with steps (design-tokens, design-unify, code-ownership, notes-folder-move-flow, ...). Next: user confirms ideas.
- A52-move-old-folder-rename-warning merged into dev (3ced024, 2026-10-29; user hand test passed 3 of 3): cause was a trailing
  separator in the source path; `Move` now returns an explicit retention outcome and Settings shows one truthful message.
  New CONTRACTS row `notes-move-renames-old-folder`. Gate on dev PASS (0 warnings, SELFTEST PASS, CONTRACTS 7 rows: 6 pass,
  1 manual, seam 0). Follow-ups are steps of idea notes-folder-move-flow.
- C22-roadmap-step-rows (user, 2026-10-29): one ID = one idea, work toward it = lettered steps under it; A52's six
  follow-ups folded into A78-notes-folder-move-flow (steps a-f); `/roadmap` indents steps. Merged 5887137.
- C20-branch-worktree-loop (user, 2026-10-29): every item runs on `feat/<ID-slug>` in `.claude/worktrees/<ID-slug>`; dev gets
  only `--no-ff` merges plus one STATUS commit each (STATUS edited live, committed at merge). Merged 6f8da54.
- C19-subsession-routing (user, 2026-10-29): every Codex run gets its own linked sub-session (Haiku for pure runs, Sonnet medium
  when reasoning is needed); master opens updates with a status card. A59-design-ownership-audit done, 7 corrections applied,
  A76-behavior-timer-names added; design-system rows now planned. Next: A52-move-old-folder-rename-warning on the user's go.
- C17-roadmap-view (user, 2026-10-28): `/roadmap` and `tools/roadmap.ps1` render the Plan section as a table (Claude hook
  keeps `.coord/roadmap.html` current); Plan rewritten so every row has an `<ID>-<slug>`, a scope and a State; new IDs
  C14-a38-audit-branch, C15-worktree-branch-cleanup, C16-archive-finished-sessions, A74-settings-thread-count;
  P1-project-reevaluation plan rows added as `proposed`. Next: user decides
  the 8 open P1 decisions.
- C13-dev-loop-kit (user, 2026-10-28): new dev loop, runbook `docs/loop.md`. Tiers, `tools/gate.ps1`, `CONTRACTS`
  selftest (6 rows, 5 pass, 1 manual), Codex `lens-review` skill + `lens_reviewer` role + `reviewer` tier, Claude
  `design-reviewer` agent + `/next-task`, `/review-task`, `/merge-task` skills, manual CI workflow `gate`. Dogfooded:
  in-run lens review accepted; fresh Codex review found a gate false PASS (no exe -> selftest skipped) -> r2 fixed it,
  plus `-Offline` (sandbox NU1900) and `-SizeGrowth`. Manager gate online PASS; false-pass probe FAIL as intended.
  Detected by a fresh Claude (agent + skills) and a fresh Codex (skill + role). Claude `design-reviewer` smoke run:
  accept-with-followup (rework rounds' scope/report paths, stale committed gate reports, contract checks run twice) ->
  manager fixed docs/skills and ignored `*.gate.md`; r3 fixed rework-round scope and made CONTRACTS the only caller of its
  four checks. Merged into dev (9bf5581, user: "Yes, merge it"), gate on dev PASS. Not yet exercised: Codex role loading
  inside a worktree, GitHub-hosted CI. Next: new Opus session resumes Priority 1 with this loop (`/next-task`).
- GitHub (user, 2026-10-28): private repo `NeelAPatel/AntiNotepad` created as origin; all branches and tags pushed,
  default branch `main`. History scanned first (no secrets, largest blob 2.9 MB font). The older WinUI repo is
  `Archived-AntiNotepad` (untouched). Public release stays with N3. Next: Priority 1 audit (can add a CI gate).
- C4-coord-archive (user, 2026-10-28): finished A35-A44 briefs, Codex reports and the A35 handoff committed, tagged
  `archive/coord-2026-10-28` and removed from the tree (kept: A42-padview-commands plan/impact-r2 for A42c/A42d, N3
  brief). Report paths in the entries below now resolve under that tag. `.gitignore` adds settings.local.json, `.vs/`,
  `*.user`. Plan 2.8 done.
- A35-bar-insets merged into dev (fad7d61, user-approved live): design tokens + insets, ink-centered glyphs, scrollable
  top gap, top-bar blur behind; one commit per feature. 
  A35 branches and worktree deleted (user). Lab history no longer kept. Installed copy not upgraded.
- A36-drop-pill-lab merged: dev-only Lab row and pill blur code removed; Solid pill ships; top-bar blur now in
  `UI/PadWindow.TopBlur.cs`. Branch and worktree deleted. Nothing in flight.
- A37-caret-drag-jump merged (user-approved live): SmoothCaret glide restarts from the drawn position, fixing the caret
  jumping back to the pause point during drag-select. Nothing in flight at that point.
- A38-mvvm-audit done (branch `feat/A38-mvvm-audit`, `docs/A38-mvvm-audit.md`, not merged): no MVVM; targeted
  refactors A39-A44 on `reorg/<ID-slug>` branches off dev.
- A39-motion-out-of-ui merged into dev (bac8a6c, user-approved live 2026-10-26): Editor no longer depends on UI
  (`Anim` -> `Motion/`, `ChromeTokens` -> `Theme/`, context menu shown via the host); IL-based `LAYERS` selftest with 8
  one-way rules. Branch kept until the reorg series ends.
- Reorg series, one at a time from the latest dev, in order: A40-settings-change-event, A41-padwindow-split,
  A42-padview-commands, A43-app-context, A44-platform-seam. Read-only investigator sessions wrote each plan on its own
  branch (`reorg/<ID-slug>`: `.coord/tasks/<ID-slug>-plan.md` + impact report); they never implement. Implementation
  starts per task on the user's go, from that plan, with the user's hand test before merge. Next: A44-platform-seam re-audit on the latest dev.
  Rule (user, 2026-10-26): after each task merges, the next task's investigator merges the latest dev into its branch and
  re-runs a read-only Codex delta audit (`<ID-slug>-impact-r2`), updating its plan with a "Recommendation on handling"
  (scope, split into sub-tasks or not, riskiest parts and hand checks). The user decides from that before implementation.
  A42 follows once A41 is done, then A43, A44.
- A41-padwindow-split re-audit done (6b91ebe on `reorg/A41-padwindow-split`): original scope dropped. User's sequence
  (2026-10-26): A41a-hotkeys -> hand test -> merge -> A41b-clipboard-policy re-audit -> implement -> hand test -> merge.
  Save scheduler, navigation core, taskbar/blur/OCR extraction skipped. A41a-hotkeys implemented on `reorg/A41a-hotkeys` (2677946: `Platform/HotkeyRegistrar`, `HOTKEYS swap` selftest, 3 LAYERS rows; gate passed, guards proven); user hand test passed 2026-10-26; merged into dev (2b85bfb, 0 warnings, SELFTEST PASS).
- A41b-clipboard-policy re-audit done (21b318b on `reorg/A41-padwindow-split`): do it now, before A42; small and optional
  (value = selftest on dedupe/format rules; privacy check stays in the window). Dispatched on `reorg/A41b-clipboard-policy`.
- A41b-clipboard-policy implemented on `reorg/A41b-clipboard-policy` (`Editor/ClipboardCapturePolicy`, `CLIPBOARD policy`
  selftest, 11 LAYERS rows); user hand test passed 2026-10-26; merged into dev (bbcaa6b, 0 warnings, SELFTEST PASS).
  A41 done.
- A42-padview-commands re-audit done (on `reorg/A42-padview-commands`): split, stop after A42b. User's sequence
  (2026-10-26): A42a-math-notes -> hand test -> merge -> A42b-note-analysis-file re-audit -> implement -> hand test -> merge
  -> stop at the gate (A42c-checkbox-indent, A42d-list-commands optional; ChipText/FoldSelection skipped). A42a dispatched
  and implemented on `reorg/A42a-math-notes` (`Editor/MathNotes`, `Editor/ListEditing`, 31 LAYERS rows, `MATH NOTES`/`LIST EDITING`
  selftests; PadView 1,190 -> 1,132; gate passed, guards proven); user hand test passed 2026-10-26; merged into dev
  (7f845be, 0 warnings, SELFTEST PASS, 57 LAYERS rows). 
- A42b-note-analysis-file (`reorg/A42b-note-analysis-file`): `NoteAnalysis` moved verbatim; `PadView.cs` 1,132 -> 864 lines; build 0 warnings/errors and `SELFTEST PASS`, all 16 new LAYERS rows 0 refs. Report: `.coord/reports/A42b-note-analysis-file.md`. Guard probes (PadView, Window, TextArea, Dispatcher fields) each failed the selftest. User hand test passed 2026-10-26 (about half-second lag in a 6,000-line note, accepted; see A49-large-note-perf in Backlog); merged into dev (6230aa9, 0 warnings, SELFTEST PASS, 73 LAYERS rows). A42 stopped at the gate:
  A42c-checkbox-indent and A42d-list-commands not started (user moved on to A43).
- A43-app-context re-audit done (3fe4004 on `reorg/A43-app-context`): smaller scope, no broad AppRuntime. User's sequence
  (2026-10-26): A43a-runtime-log-and-launch -> hand test -> merge -> A43b-notes-session re-audit -> implement -> hand test
  -> merge. A43a-runtime-log-and-launch implemented on `reorg/A43a-runtime-log-and-launch` (8c60766: `RuntimeLog`, `TaskbarTrace`, `AppLaunch`; `Program.` tokens 75 -> 21; 80 LAYERS lines, `RUNTIME LOG`/`TASKBAR TRACE` selftests; gate passed, guards proven); user hand test passed
  2026-10-26; merged into dev (5245a62, 0 warnings, SELFTEST PASS). 
- A43b-notes-session re-audit done (5787060 on `reorg/A43-app-context`): one task, two review stages (services injection,
  then NotesSession); bootstrap wiring stays in Program; doc fix bundled. Dispatched on `reorg/A43b-notes-session`. Doc fix pending: `docs/dev-loop.md:11` (custom
  ANTINOTEPAD_DATA follows its own location.json after an in-app move).
- A43b-notes-session implemented on `reorg/A43b-notes-session` (6993485: `NotesSession`, `INotesLocation`, Store/SettingsService injected; `Program.` tokens 21 -> 0; 89 LAYERS lines, `NOTES SESSION move`, static-exposure and DATA/DEV selftests; gate passed, guards proven). Hand test by Claude via GUI passed (second move, edit saved in new folder, nested refusal, restart reload). Not merged. Report: `.coord/reports/A43b-notes-session.md`.
- A43b-notes-session approved by the user 2026-10-27 ("Approve") after live checks; parent session merges into dev. A43b-notes-session-r2 completed on `reorg/A43b-notes-session`: unavailable stores retain a valid in-memory selection; `UNAVAILABLE STORE startup` passes. Build 0 warnings/errors and SELFTEST PASS; check-output removal was refused by tool policy. Report: `.coord/reports/A43b-notes-session-r2.md`.
- A43b-notes-session-r3 implemented on `reorg/A43b-notes-session`: unavailable-folder recovery now writes a validated folder pointer for the next start; build 0 warnings/errors and `SELFTEST PASS`, including `NOTES SESSION use-folder`, with the 89 existing LAYERS rows unchanged. No GUI launched; manager hand check remains. Report: `.coord/reports/A43b-notes-session-r3.md`.
- A43b-notes-session merged into dev 2026-10-27 (ffca0fb; includes r2 crash fix 79d2f7b and r3 recovery a44a636, both
  user-checked live): 0 warnings, SELFTEST PASS (`NOTES SESSION move/use-folder`, `UNAVAILABLE STORE startup`), `Program.`
  tokens outside Program.cs 0. A43 done. Reorg A39-A43b all merged; only A44-platform-seam (plan only) and
  `feat/A38-mvvm-audit` (audit doc) hold commits not in dev.
- A44-platform-seam re-audit done (6c7f5e6 on `reorg/A44-platform-seam`): split A44a-native-facade -> A44b-interop-guards
  -> A44c-shell-folder (optional), facade `DesktopPlatform`, dev metrics move skipped. A44a-native-facade implemented on
  `reorg/A44a-native-facade` (01c01c6): build 0 warnings/errors, `SELFTEST PASS` with 89 LAYERS lines; compile and layer
  guard probes passed. Report: `.coord/reports/A44a-native-facade.md`. Gated by the manager (own build 0 warnings, SELFTEST PASS, `Native.` outside Platform 0). User hand test 2026-10-28: 9 of 10 passed; #10 link click failed but is not from A44a (only the call
  name changed; short URLs are never clickable) -> A54-link-hover-click. Merged into dev (67b5031): 0 warnings, SELFTEST
  PASS, 89 LAYERS lines + `PLATFORM DesktopPlatform surface`, `Native.` outside Platform 0. A44b-interop-guards dispatched
  2026-10-28 (user: "Start A44b") on `reorg/A44b-interop-guards` (Sonnet manager, Codex luna xhigh, no computer use).
- A44b-interop-guards committed on `reorg/A44b-interop-guards` (98d91f8), user signed off 2026-10-28 (computer-use hand test passed; Explorer restart and tray right-click menu not exercised): GlobalHotkey/TaskbarProxy externs
  nested in private `Win32`; LAYERS +13 lines (102 total: P/Invoke-holder, raw-type, pinvoke-outside-Platform) + `PLATFORM P/Invoke holders`;
  `tools/check-platform-seam.ps1` (manager gate, 0 violations in 54 files). Manager gate: 0 warnings, SELFTEST PASS, script fails
  on a temp UI IntPtr in an interpolation hole. Report: `.coord/reports/A44b-interop-guards.md`. Merged into dev (45f7ebe): 0 warnings, SELFTEST PASS,
  102 LAYERS lines, `tools/check-platform-seam.ps1` 0 violations. Not yet exercised by hand: Explorer restart, tray
  right-click menu. A44c-shell-folder dispatched 2026-10-28 (user: "okay do a44c") on
  `reorg/A44c-shell-folder` (Sonnet manager, Codex luna xhigh, no computer use).
  A44c-shell-folder implemented (a0af213): `DesktopPlatform.OpenFolder`, SettingsWindow keeps Exists/catch/toasts; LAYERS +2 (`UI/Editor->Process.Start`, 104 total), seam script also bans `Process.Start`/`ProcessStartInfo`; guard proven with a temp probe. Manager gate: 0 warnings, SELFTEST PASS, script 0 violations. User hand test passed 2026-10-28 (Open notes/recovered folder, spaces, missing-folder toast, after move); merged into dev (adc9302): 0 warnings, SELFTEST PASS, 104 LAYERS lines, seam script 0 violations. A44 and the A39-A44 reorg series done. Released to main as `antinotepad-v0.6.0-untested-r7` (Plan step 0 done). Next: Plan Priority 1.
- A40-settings-change-event merged into dev (565a1e5, user-approved live 2026-10-26): SettingsService + one Changed event,
  SettingsWindow on ISettingsHost, all writers migrated; guards now 12 LAYERS lines + `SETTINGS Changed`; 0 warnings, SELFTEST PASS.
  Fix: SearchGrid/SplitSlot/Split now saved. Folding/InlineMath apply only on note bind/analysis (pre-existing; backlog).
- Native-window work (A32/A34) parked; any retry must not change the window's frame styles and
  goes lab-first. A34 (lab) was dropped: thick native border, bars broken in all three styles.

## Plan (user order, 2026-10-29)
Shown live by the roadmap server (http://localhost:47811, `/roadmap`); a snapshot Artifact (`tools/roadmap.url`) only on request.
Format (the script parses this section): `###` = priority; a row with only a bold first cell = block; Item is a numbered
`<ID>-<slug>` (confirmed by the user), an unnumbered `idea: <slug>` with its `step: <slug>` rows right under it, a
`Decision: ...` or `Release marker`. Rows run in the order listed; the script numbers them by position. Numbering rules:
`docs/loop.md` ("Numbering"). A53 and later from before 2026-10-29 were dissolved (user); the "was" notes below point to
old reports. State: `planned`, `needs you`, a `docs/loop.md` state (`briefing` ... `hand-tested`), `done` or `dropped`.
`/next-task` and `/merge-task` keep State current. Held until the user brings them back: "Open (needs the user)" and the
A44b-interop-guards leftover hand checks (Explorer restart, tray right-click menu). P1 background:
`.coord/tasks/P1-project-reevaluation-plan.md`, `.coord/reports/A59-design-ownership-audit.md` (old ID).

### Pending work
| Item | Scope | Tier | Effort | State |
|---|---|---|---|---|
| **Next (user, 2026-11-06: "3, then 1 and 2 together as worktrees with codex and then 5 release")** | | | | |
| idea: search-bar-overlap | fix/: in Overlay mode, opening search lets the top bar (badge, Saved label, icons) cover the search field; move the content down far enough that it never overlaps, in Docked, Overlay and Float alike (user, 2026-11-05, A4n hand test, screenshot: "i think we permanently move the elements down enough so it doesnt overlap regardless of Docked mode or Overlay/Floating mode. I think this should be a fix/ to add to the backlog"). Same view: the top bar can no longer be used to drag the window while search is open (user, 2026-11-05: "in search view, you can no longer access the top bar header to move window arround"); overlap pre-existing, not from A4n (SearchOverlay top margin 14 vs bar row 40, UI/SearchOverlay.cs:47, same on the umbrella); drag loss also pre-existing (Overlay/Float: _overlayHost covers the bar zone and the hidden bar layer is not hit-testable, UI/PadWindow.Layout.cs:337-340,397); fix idea: top margin DockedRowHeight on _overlayHost in Sliding modes **Next (user, 2026-11-06):** "1 and 2 together as worktrees with codex"; a plan of action first. | Medium | Small | planned |
| idea: note-switch-instant-swap | Switching notes by scroll or swipe shows the next page at once for part of the change: `Fire()` (`UI/NoteSwipe.cs:211`) swaps the text immediately and only then plays the 190 ms arrive animation. Predates design-tokens, not caused by A4e-motion-tokens (user, A4e hand test, 2026-11-02: "okay lets put this issue as a potential bug on the roadmap"; relayed by Master 3). Visible, so lab-first. **Next (user, 2026-11-06):** runs beside search-bar-overlap in its own worktree; a plan of action first. | Medium | Small | planned |
| Release marker | v0.7.0-rc2: dev -> main after both fixes, on your go | - | - | planned |
| **Loop v3.1 (first; the backlog waits for it, user 2026-10-30)** | | | | |
| Loop2-loop-v3-1 | Loop v3.1: dormant v2.2 absorbed (overlap, catch-up, conflicts, hand-test queue), approve/merge preflights, strict scope, read-only audits, loop-lint, loop.md fixes, rehearsal; one branch, one commit per step, run by the loop chat | Low | Large | done |
| **Era 1 start (Master 2 priority, 2026-11-01: done rows first, then run order; numbers only on your yes)** | | | | |
| T1-roadmap-overview | The roadmap becomes your central overview, live from STATUS, item cards, Codex runs and read-only git (user, loop chat, 2026-10-30: "I need Roadmap to become my central overview of the project"; chose "Local server"). A fixed page `tools/roadmap/index.html` served by `tools/roadmap-serve.ps1` (built-in .NET HttpListener, localhost:47811, read-only) with `/data` from `tools/roadmap-data.ps1`; the page polls every 3 s and keeps its state; `/roadmap` starts the server; the hook rebuild retires. Design approved after three mockup rounds (2026-10-30, "Yes, write the plan"): Your turn, Moving without you, hand-test queue, recent Codex runs, Plan with ID column and release points, Work log, branch lanes with health, Releases, drawer, legend, light/dark. Steps T1a-roadmap-server, T1b-roadmap-page, T1c-roadmap-switch. Branch `chore/T1-roadmap-overview` | Low | Large | done |
| Loop3-proposals-in-status | One rule line in `docs/loop.md`: every item or proposal a chat names goes into the STATUS Plan as an unnumbered `proposed` row at once, before or with the question; matching steps in `/next-task`, `/scan-backlog`, `/review-task` ("Noticed" follow-ups become rows at the time, not at Merge); no script. Also align `docs/loop.md` "Sessions" (auditor "commits") with "Branches" and `/next-task` step 2 (nothing committed before Approve). User confirmed "Loop3, rule + skills" in the loop chat; loop freeze lifted for this item only. **Proposed addition (loop chat, 2026-10-30; you decide at Approve):** the Era 0 / Era 1 rename in `docs/loop.md`, `docs/glossary.md`, `docs/coding.md`, the `/review-task` skill, three wiki pages and `tools/loop-lint.ps1`/`.allow` (old forms kept, loop-lint still passes); the wiki is republished after Merge on your go; tag names and old cards are not renamed. Branch `chore/` | Low | Small | done |
| idea: approve-moves-all-files | `tools/approve.ps1` moves only `.md` planning files; a card's mockups and samples had to be moved by hand (T1-roadmap-overview, Noticed). | Low | Small | done |
| idea: codex-run-no-caps | Remove per-role run caps and their failover from `.coord/tools/codex-run.sh` and `docs/agents.md`; the only Codex limit is the usage % (user, 2026-10-30). | Low | Small | done |
| Loop4-retro-fixes | A lighter, faster loop from the T1-roadmap-overview retro: Codex roles by job (Luna medium / Terra medium / Luna xhigh / Sol medium), no run caps, offline-only Codex checks, expected time in plans, side-task fast lane, masters per area with sub-sessions per phase, master notes reloaded after compaction, `approve.ps1` moves all planning files, loop.md wording fixes. Absorbed `idea: codex-speed-by-tier`, `idea: codex-run-no-caps`, `idea: approve-moves-all-files`, `idea: loop-md-contradictions`, `Loop3-proposals-in-status`. | Low | Medium | done |
| A1-selftest-split | Split `SelfTest.cs` (3,247 lines) into ten `SelfTest/` files by area, verbatim; first, because the LAYERS rows live there and every later split edits them. Numbered by the user 2026-11-01 ("Start A1"); builder on Haiku (user: "replace codex subagents with haiku"). Card `.coord/tasks/A1-selftest-split.md` | Low | Medium | done |
| A2-store-split | Split `Model/Store.cs` (1,006 lines) into `Model/Search/`, `Note.cs`, `Settings.cs`, `Store.cs`, verbatim; after selftest-split (shares LAYERS, `AGENTS.md`, `docs/coding.md`). Would be A2 Numbered by the user 2026-11-01 (parallel; merge order A2, A3, D1). Card `.coord/tasks/A2-store-split.md` | Low | Small | done |
| A3-calc-split | Split `Calc/Calculator.cs` (1,383 lines) into five files in `Calc/` (units, dates, lexer, parser, format), verbatim; after store-split. Open: lettered steps if over 400 moved lines. Would be A3 Numbered by the user 2026-11-01 (parallel; merge order A2, A3, D1). Card `.coord/tasks/A3-calc-split.md`; also writes both `docs/coding.md` rows and `AGENTS.md` | Low | Medium | done |
| D1-ui-region-names | Names for the parts of the window in `docs/glossary.md`, so cards, comments and chat use the same words; can run beside selftest-split (no shared files). Would be D1 Numbered by the user 2026-11-01 (parallel; merge order A2, A3, D1). Card `.coord/tasks/D1-ui-region-names.md`; a D1 lead session owned by Master 2 **Scope change (user, in D1 chat, 2026-11-01):** SVGs replaced by an interactive HTML region map (`docs/regions/`, hover a name to highlight it on a screenshot, similarity % and top 5), screenshots from the dev copy ("Yes, both"); page built by Codex terra (b3255a3); captures blocked: the Computer Use grant resolved to the installed exe and was denied. Expected 2-3 h **Paused indefinitely (user, 2026-11-01):** branch `docs/D1-ui-region-names` pushed at d96a11f (map with six dev-copy captures, gate PASS; pause on the card), waiting on the user's review; worktree kept | Low | Small | paused |
| T4-selftest-diff-skip-timing | A verbatim-move check that compares `--selftest` output should skip wall-clock lines (`A18-search-speed timing` differs per run). Noticed in A1-selftest-split Check Numbered by the user 2026-11-01 (go relayed by Master 2: "finish the remaining three Era 1 items"); fast lane, Loop Hub 2. Card `.coord/tasks/T4-selftest-diff-skip-timing.md` | Low | Small | done |
| T2-hot-files-stale-path | `tools/catch-up.ps1:12`, `tools/files-overlap.ps1:11` and `.coord/templates/item.md:34` still name `SelfTest.cs` after A1-selftest-split (harmless: `SelfTest/*.cs` is listed too). Noticed by Master 2. Numbered by the user 2026-11-01 ("Approve both"); fast lane, Loop Hub 2. Card `.coord/tasks/T2-hot-files-stale-path.md` | Low | Small | done |
| T5-review-gate-stale-head | `tools/merge-preflight.ps1` failed "stale Head: re-run the gate" on both A1-selftest-split and A2-store-split, because `/review-task` commits the card after its gate run; make review run the gate last (or preflight accept card-only commits). Seen twice; Loop Hub 2's area Numbered by the user 2026-11-01 (go relayed by Master 2: "finish the remaining three Era 1 items"); fast lane, Loop Hub 2. Card `.coord/tasks/T5-review-gate-stale-head.md` | Low | Small | done |
| T6-gate-format-split-baseline | `tools/gate.ps1` Format treats every line of a split's new files as changed, so a pure move must reformat (A3-calc-split did a whitespace pass and lost the sorted-lines proof); baseline new files against the file they were split from. Noticed in A3 Check; Loop Hub 2's area Numbered by the user 2026-11-01 (go relayed by Master 2: "finish the remaining three Era 1 items"); fast lane, Loop Hub 2. Card `.coord/tasks/T6-gate-format-split-baseline.md` | Low | Small | done |
| T7-paused-state | A `paused` State for plan rows: a started item the user set aside keeps its ID, branch, worktree and card and does not count against the 3-item cap (`docs/loop.md`, `tools/loop-card.ps1`); first user D1-ui-region-names (user, 2026-11-01: "State 'paused' (Recommended)" instead of a D1-PAUSED rename). Fast lane, Loop Hub 2. Card `.coord/tasks/T7-paused-state.md` | Low | Small | done |
| T9-roadmap-step-rows | Roadmap page: done umbrella step rows (A4a-A4n) showed in Era 0 because only the umbrella A4-design-tokens is in the log; card headers with the branch on the `Tier:` line (A4m, A4n) were skipped, so their drawers were empty; `paused` rows showed under "Moving without you". Fix: era fallback on the ID without the step letter, parser reads both header forms, `paused` counts as deferred. User to Master 4, 2026-11-06: "update the roadmap site end to end" (relayed). Fast lane, Loop Hub 2. Card `.coord/tasks/T9-roadmap-step-rows.md` | Low | Small | done |
| T3-codex-unpause-docs | User lifted the Codex pause (2026-11-01: "now that Codex is available to be used as intended originally", then "Codex allowed"); drop the "Codex is paused" lines in `AGENTS.md` Roles, `docs/loop.md` Rules and the `docs/agents.md` header. Numbered by the user 2026-11-01 ("Approve both"); fast lane, Loop Hub 2. Card `.coord/tasks/T3-codex-unpause-docs.md` | Low | Small | done |
| Loop5-reply-style | Replies the user can read cold: a project output style (plain words, glosses, fixed-column tables, bullet hierarchy, visuals for overviews, a decision block that lists what was already decided without the user, peer FYIs silent) plus a one-line default status card, full card only on phase change or a decision (`docs/loop.md`, `tools/loop-card.ps1`). User, 2026-11-02: "reading exhaustion", "assumption that im keeping track when im not", "Yes, open Loop5". Fast lane, Loop Hub 2. Approved by the user 2026-11-02 ("Approve"). Card `.coord/tasks/Loop5-reply-style.md` | Low | Small | done |
| Loop6-loop-v5-wiki | Loop version is **v5.0.0** (user, 2026-11-04; v4 skipped); version rule gets a patch level `x.y.1` for chat-output changes; hook line names the version so every chat knows; wiki pages refreshed to v5 and a new swimlane flow `docs/wiki/images/loop-v5-flow.svg` in the v3.1 format with updated bubbles (Codex role by job, masters per area, fast lane, hand test only if visible). User: "take a worktree off dev, create a subchat, and update the wiki + diagram". Wiki republish waits for the user's go. Fast lane, Loop Hub 2. Card `.coord/tasks/Loop6-loop-v5-wiki.md` | Low | Medium | done |
| Release marker | dev -> main with tag `v0.7.0-rc1` after Era 1 (user, 2026-11-01: "Make a release marker at this point"); D1-ui-region-names stays paused and is not in it | - | - | done |
| **Bugs** | | | | |
| A53-split-divider-height | Split-view divider stops short of the window's bottom edge; make it full height (was A58) | Low | Small | done |
| A54-link-hover-click | Every URL opens on Ctrl+click; shortened links always show a hand, other URLs show it while Ctrl is held (widened in r3, user) | Low | Small | done |
| C25-dev-instance-handoff | `tools/dev.ps1` closes whatever dev copy is running from any checkout (shared record, dev-only, never the installed copy) and launches its own; the implementation sub-session starts its dev copy when the gate passes | High | Small | done |
| A45-dev-build-badge | Dev badge names the exact build: branch glyph U+E0A0 (fallback ⎇), `dev/<branch>@<ISO build time>` stamped at build; short form expands on hover, tooltip always shows the full text; approved at its hand test (dev-only) | Low | Small | done |
| C26-item-card-loop | One item card per item (Request > Findings > Plan > Go > Build > Check > Hand test > Merge), findings every tier, reproduction before bug builds, gates per item, bounds check, status card hook | Low | Small | done |
| C27-session-naming | Master and sub-session names, loop version v2, master handover checklist and resume prompt | Low | Small | done |
| C28-loop-cheap-checks | Gate format check on changed lines, worktree-safe Codex ledger, triage on failure, lessons and reviewer scores, quiet relays (loop v2.1) | Low | Small | done |
| C30-loop-docs-v22-plan | Loop v2.2 plan (parallel items, catch-up, conflicts), GitHub wiki pages, dormant v2.2 drop (docs and scripts, inactive) | Low | Small | done |
| idea: loop-md-contradictions | Fix the `docs/loop.md` contradictions C30 noticed (v2 attribution, gate row, rework counting, parallel items vs one dev copy) **Proposed (Master 2, 2026-10-30):** add the auditor-commit wording (`docs/loop.md` "Sessions" vs "Branches" and `/next-task` step 2); held by the loop freeze. | Low | Small | done |
| A55-split-view-clip | In split view, text scrolls across the divider; clip each pane at the divider (was A57) | Medium | Medium | done |
| Release marker | dev -> main with a tag, on your go (r8, 42ad744) | - | - | done |
| **Reorg leftovers** | | | | |
| A42c-checkbox-indent | Move checkbox indent logic out of `PadView` into its own Editor class, behavior-neutral | Medium | Small | done |
| A42d-list-commands | Move list commands out of `PadView` into its own Editor class, behavior-neutral | Medium | Medium | done |
| C29-headless-render-probe | Off-screen Avalonia render helper for the selftest, so audits can reproduce visual bugs and `split-pane-clipping` checks pixels automatically instead of manually; run after A42d-list-commands (user order 2026-10-29: A42c, A42d, then C29; all touch the selftest) **Proposed (Master 2, 2026-10-30):** renumber as an `A` item (product selftest code), run after selftest-split; you choose A, T or idea. | Medium | Small | proposed |
| **Design system and ownership** | | | | |
| idea: design-tokens | One source of truth for every design value (fonts, radius, shadows, motion, alpha and opacity, behavior timers); the app looks and moves exactly as today. In parts, obvious first, fuzzier later (user, 2026-11-01: "do the obvious ones first and slowly go into the fuzzier tokens"); each step defines its own tokens and swaps their call sites, so steps run one after another. Spacing out (ratchet only). Proof per step: a design dump of applied control properties, byte-identical to dev, plus the hand test. Director: Master 3 · Loop v3 · design-tokens **Done:** On dev, 2026-11-05; feat/A4-design-tokens kept for return work. | - | - | done |
| A4a-token-scan | Step 1 of A4-design-tokens (user, 2026-11-01: "make a branch and start with token scan only"). `tools/check-design-tokens.ps1`: fresh inventory of hard-coded design values (refreshes A59's stale TSV) plus a gate `Tokens` step: per file and category the count may not rise (spacing included); no product change. Card `.coord/tasks/A4a-token-scan.md`; squashed into umbrella `feat/A4-design-tokens` (8cdab0a), reaches dev with the umbrella **Done:** On dev; umbrella. | Low | Small | done |
| A4b-design-dump | Step 2 of A4-design-tokens (user, 2026-11-01: "Start design-dump"). `--design-dump` plus a selftest `DESIGN dump` line: builds the pad, Settings, overlays and menu offline (never shown) for all palettes, pins a hash of their applied fonts, radius, colors, opacity, shadows; type, shape and paint steps must keep it unchanged. Card `.coord/tasks/A4b-design-dump.md`; squashed into umbrella `feat/A4-design-tokens` (5d8c394), reaches dev with the umbrella **Done:** On dev; umbrella. | Low | Small | done |
| A4c-type-tokens | Step 3 of A4-design-tokens (user, 2026-11-02: "Start type-tokens"). `Theme/Typography.cs`: UI, mono and editor-fallback families, weights, text and icon sizes by role at today's values; all 105 font literals in 15 files use them; editor relative heading scales kept. Proof: DESIGN hash unchanged plus hand look. Card `.coord/tasks/A4c-type-tokens.md`; squashed into umbrella `feat/A4-design-tokens` (d7b95e8), reaches dev with the umbrella **Done:** On dev; umbrella. | Low | Medium | done |
| A4d-shape-tokens | Step 4 of A4-design-tokens (user, 2026-11-02: "Shape, then trim"). `Theme/Shape.cs`: corner radii by role, a `Round(size)` helper for circles, the menu and dropdown popup shadows; all 25 radius and 2 shadow literals in 7 UI files use them; `ChromeTokens` untouched. Proof: DESIGN hash unchanged plus hand look. Card `.coord/tasks/A4d-shape-tokens.md`; branch off the umbrella **Done:** On dev; umbrella. | Low | Small | done |
| A4e-motion-tokens | Step 5 of A4-design-tokens (user, 2026-11-02: "yes start motion tokens"). `Motion/MotionTokens.cs`: one named duration per animation role (13 `Anim.Run` sites plus the 80 ms caret glide, 120-200 ms) at today's values; easing already single (`Anim.EaseOut`); timers stay out. Card `.coord/tasks/A4e-motion-tokens.md`; branch off the umbrella **Done:** On dev; umbrella. | Low | Small | done |
| A4g-control-groups | Step 7 of A4-design-tokens (user, 2026-11-02: "Yes, do both"). After motion-tokens (user, 2026-11-02: "grouping controls based on purpose makes sense. we should do that"). Read-only control map from the design dump: every element grouped by kind (button, dropdown, field, card, popup, badge, text role) with where each group disagrees with itself; then group tokens (`Button`, `Popup`, `Card`, ...) over the role tokens, values unchanged; folds in the ChromeTokens aliases (11.5, 12, 16, 8, 12, 3.5). Card `.coord/tasks/A4g-control-groups.md`: map first (report only), names checkpoint, then group tokens **Done:** On dev; umbrella; map only, Part 2 dropped. | Medium | Small | done |
| A4h-fluent-study | Step 8 of A4-design-tokens (user, 2026-11-02: "Yes, do both"). With control-groups (user, 2026-11-02: "examine design language and principles followed by microsoft for their sizes and observe patterns"). Notes on Windows 11 Fluent 2 sizes (type ramp, 4 px grid, control and overlay radii, elevation) set beside ours, with the patterns we break (odd and half sizes); input to design-unify, no code. Card `.coord/tasks/A4h-fluent-study.md`: `docs/design-fluent.md` by a web-enabled Claude sub-session (Codex is offline) **Done:** On dev; umbrella. | Low | Small | done |
| A4i-systems-study | Step 9 of A4-design-tokens. Replaces design-questions (user, 2026-11-03: "my intention here is to learn from microsoft's fluent and maybe some others to understand how design systems are built and organized --> map it to our controls and control groups. so instead of using 7~10 different numbers we reduce it to maybe 5 numbers which follow a pattern/theme and consistency, with a few exceptiosn here and there"; path: "Yes, this path."). Doc: how Fluent 2, Material 3 and Apple HIG organize tokens (scale > role > control tiers), no code. Card `.coord/tasks/A4i-systems-study.md`: `docs/design-systems.md` by a web-enabled Claude sub-session **Done:** On dev; umbrella. | Low | Small | done |
| A4j-scale-proposal | Step 10 of A4-design-tokens (user, 2026-11-03: "Yes, do both"). Card `.coord/tasks/A4j-scale-proposal.md`, `docs/design-scale.md`, recommended + alternative per category. After systems-study: per category (corners, text, spacing, alpha, motion) a ~5-step scale, every control kind from the A4g map mapped onto it, exceptions named; asks to lift the ChromeTokens lock; on paper **Done:** On dev; umbrella. | Medium | Small | done |
| A4k-playground-measure | After scale-proposal (user, 2026-11-04: promised as part of design-tokens): measure a dev copy by computer control (screens in several sizes, states, overlay modes, controls highlighted) plus the design dump; a local web page (localhost, like the roadmap) redraws pad + top bar + pager, search/menu/toast, dark/light and overlay modes in HTML to the measured pixels, on a sticky zoomable canvas; a scrollable pane of adjuster cards per new token (number box/slider/dropdown), grouped by section, each listing the old tokens it replaced; starts from the recommended scale. Answers (user, 2026-11-04): local page; HTML redraw OK; Settings window not in v1; output: "the adjuster is to preview in the html --> export to Live would be nice where i give you the new values i want to try and you make a copy of that app for me" **Done:** On dev; umbrella. | Medium | Large | done |
| A4l-playground-page | Step 2 of the token playground (user, 2026-11-04: "Yes, do both"). Card `.coord/tasks/A4l-playground-page.md`. Local page on localhost:47812: sticky zoomable HTML redraw of the pad from `docs/playground/measure.md`, adjuster cards per section from `tokens.json` (Today / Recommended / Alternative presets, top-bar lock switch), onion-skin compare, Save picks to `docs/playground/picks.json` for the lab copy. Offered in two cuts (pad + bar + pager first, then search/menu/toast, modes, light, motion) **Done:** On dev. | Medium | Large | done |
| A4m-playground-align | Step 3 of the token playground (user, 2026-11-05: "let me know when all the design token data and the localhost playground are both aligned and ready for me to review"). Card `.coord/tasks/A4m-playground-align.md`, audit 37 rows (2 block review: Alt preset mapping, PagerDotButtonHeight literal). **Done:** On dev. | Medium | Medium | done |
| A4n-scale-lab | Lab build with every Recommended value from docs/design-scale.md (user, 2026-11-05: "Make a dev build using all recommended values, regardless if its on the playground or not"). Values only; 13 exceptions kept; lock question answered yes for the lab only. Card `.coord/tasks/A4n-scale-lab.md`. **Done:** On dev; hand test PASS 2026-11-05: "looks good. we can wrap up and merge". | Medium | Medium | done |
| step: scale-apply | After the lab is approved: tokens become scale > role > control tiers and the app adopts the scale; new DESIGN hash | High | Medium | needs you |
| step: design-plugins-review | After design-questions (user, 2026-11-02): install the Design (Anthropic) and design-skills plugins, run them end to end on the project, compare with our findings; then decide whether a top-level Design Master chat reviews the design independently | Low | Small | needs you |
| A4f-paint-tokens | Step 6 of A4-design-tokens (user, 2026-11-02: "Start paint-tokens"). `Theme/Paint.cs` with hover, press, fill, border and dim alphas and opacities by role (byte `WithAlpha` and brush opacity kept apart; about 85 values); shared brush/pen factories, each editor cache keeps its limits. Back in after motion-tokens (user, 2026-11-02: "we might want to do this"). Note for design-unify, lab-first (user): the Acrylic and Mica tint slider range is too narrow, "as if you have a bar from 0~100 but you picked two somewhat close spots on the line"  Card `.coord/tasks/A4f-paint-tokens.md`: 50 sites in 13 files (opacity 45, alpha 5), no brush factories **Done:** On dev; umbrella. | Medium | Medium | done |
| step: timer-names | Fuzziest: named behavior timers (save, debounces, toast hold, caret blink, bar sleep; about 40 values) at today's values, owned where they are used (user, 2026-11-01: timers included) Trimmed from design-tokens (user, 2026-11-02: "Shape, then trim"); revive with design-unify if those values need changing | High | Medium | deferred |
| Release marker | dev -> main after the look-identical work, on your go | - | - | planned |
| idea: design-unify | You choose a unified look (radius scale, font sizes, shadows, motion timings) and it is applied; nothing changes without your lab approval **Done:** By A4-design-tokens (Recommended scale applied, user 2026-11-05). | - | - | done |
| step: design-reference | Claude Design System page from today's values plus a canvas comparing options; no code **Done:** Covered by A4-design-tokens (A4j-A4n). | Low | Small | done |
| step: unify-lab | Lab build: today's look next to the proposed scales and Windows 11 timings; you pick **Done:** Covered by A4-design-tokens (A4j-A4n). | Medium | Medium | done |
| step: apply-radius | Apply the corner-radius scale you picked **Done:** Covered by A4-design-tokens (A4j-A4n). | Medium | Small | done |
| step: apply-type | Apply the font-size ramp you picked **Done:** Covered by A4-design-tokens (A4j-A4n). | Medium | Small | done |
| step: apply-shadows | Apply the shadow tiers you picked **Done:** Covered by A4-design-tokens (A4j-A4n). | Medium | Small | done |
| step: apply-motion | Apply the animation timings you picked **Done:** Covered by A4-design-tokens (A4j-A4n). | Medium | Small | done |
| idea: code-ownership | Every responsibility has one owner and no copies; behavior unchanged | - | - | needs you |
| step: store-split | Move `Note`, `Settings`, `StoreData` and the search types out of `Model/Store.cs`. Done by A2-store-split (user, 2026-11-06: tidy) | Low | Small | done |
| step: notes-path-dedupe | One same-or-nested path rule in `Platform/` replacing the copies in `NotesSession`, `NotesLocation`, `SettingsWindow` and `KeepOldCopy`; CONTRACTS row | High | Small | needs you |
| step: ui-kit-merge | One control kit: Settings' private controls merge into `Kit`; `Material()` and `Ago()` move to their owners | Medium | Medium | needs you |
| step: ownership-map | `docs/architecture.md` owner per responsibility; `NotesSession`, `RuntimeLog` into `App/`; LAYERS rows for `App/` and `Motion/` | Low | Small | needs you |
| step: behavior-timer-names | Optional: name save, debounce, toast and taskbar timers beside their owners, values unchanged | High | Small | needs you |
| **Cleanup** | | | | |
| C14-a38-audit-branch | Merge the A38 MVVM audit doc into dev or delete `feat/A38-mvvm-audit`. Obsolete: the history rewrite left only `main` and `dev` (user, 2026-11-06: tidy) | Low | Small | dropped |
| C15-worktree-branch-cleanup | Remove leftover worktrees and finished `reorg/*` branches. Obsolete: none left after the history rewrite (user, 2026-11-06: tidy) | Low | Small | dropped |
| idea: archive-finished-sessions | Archive finished Claude sessions (era-0 C16 retired); no code (user, 2026-11-06: tidy) | Low | Small | needs you |
| idea: release-checklist | A dev -> main release checklist that re-checks gate rules; doc only. Tag names were settled by loop v3 (era-0 C18 retired) (user, 2026-11-06: tidy) | Low | Small | needs you |
| Release marker | dev -> main with a tag, on your go | - | - | planned |
| **Performance** | | | | |
| idea: settings-thread-count | Find why the Settings window runs about 90 threads; report, fix only if cheap (was A74) | Low | Small | needs you |
| idea: large-note-perf | Measure then reduce typing lag in a 6,000-line note; one change at a time (ideas in Backlog; era-0 A49 retired) (user, 2026-11-06: tidy) | Medium | Large | needs you |
| **Features and polish (lab-first where visible)** | | | | |
| idea: notes-folder-move-flow | Choosing and moving the notes folder is clear and safe; first step follows code-ownership's notes-path-dedupe | - | - | needs you |
| step: path-validator | One shared Platform check for a chosen folder: blocks invalid, same, nested, not writable, has notes; warns new, non-empty, network, low space, cloud-synced | High | Medium | needs you |
| step: editable-path-field | Editable path box plus Browse in Settings with a validation line; Migrate enabled only when valid or warned; lab mock first | Medium | Medium | needs you |
| step: move-preview | Before moving, show note count, size and what happens to the old folder | Medium | Small | needs you |
| step: move-undo | "Move back" after a move, using the kept old copy | High | Medium | needs you |
| step: picker-last-location | The folder picker opens at the last picked folder | Low | Small | needs you |
| step: single-result-message | Settings shows the move's own result message instead of rebuilding it | Low | Small | needs you |
| idea: recovered-folder-always-open | Recovered-notes folder always exists and its Open button always works (era-0 A51 retired) (user, 2026-11-06: tidy) | Low | Small | needs you |
| idea: answers-toggle-clarity | Rename the "Answers" toggle to say what it does, and apply it live; lab-first (era-0 A48 retired) (user, 2026-11-06: tidy) | Low | Small | needs you |
| idea: native-timer-notification | Windows notification when a timer ends (was A55) | Medium | Medium | needs you |
| idea: hotkey-recorder | Set a hotkey in Settings by pressing the keys; lab-first (era-0 A47 retired) (user, 2026-11-06: tidy) | Medium | Medium | needs you |
| idea: chime-picker | Choose the timer chime: native sound plus two more, stretch (was A56) | Low | Medium | needs you |
| idea: floating-scrollbar | Modern floating scrollbar at the side, shown only while scrolling (user, 2026-10-29, A55 hand test: "a scroll bar shows, i didn't expect that"); open: hide vs fade, both split panes, existing bar styles | - | - | needs you |
| idea: timer-enter-no-autostart | In a timer note, Enter after a timer line (e.g. `10s test`) starts that timer at once (`Editor/PadView.cs:698`, existing by design); user wants to write several timers first (user, 2026-10-29, A42c hand test: "that shouldnt be the case because i might be writing multiple timers"); open: what starts a timer then (Ctrl+Enter, chip click only?) | - | - | needs you |
| idea: wrap-hanging-indent | When an indented bullet line soft-wraps, the wrapped part should start under the bullet's text, not under the bullet itself: in "• We're at a point w… they're easier to", "they're" sits about 4 columns left of "We're" (user, 2026-10-29, screenshots). Wanted: the same alignment as a typed new line, just without the bullet. Open: plain indented lines with no bullet, numbered lists and checkboxes | - | - | needs you |
| idea: selection-convert-menu | Select several lines, right-click, and turn them into a list, checkboxes, timers etc.: each selected line gets the matching prefix (user, 2026-10-29, screenshot of the current right-click menu: Cut/Copy/Paste/Delete, Undo/Redo, Select all, New note, Search, split, pin, Settings, Hide). Open: which kinds (bullet, numbered, checkbox, timer, others?), a submenu or flat entries, toggling off or swapping lines that already have a prefix, blank lines in the selection | - | - | needs you |
| idea: pinned-accent-ring | When the window is pinned (Ctrl+T), draw an accent-colored ring 2-4 px thick around the window's edge, so pinned is visible at a glance (user, 2026-10-29). Open: exact thickness, whether it follows the rounded corners, theme accent or a fixed color, a fade on pin/unpin, how it works with the frameless window and its shadow | - | - | needs you |
| idea: wiki-loop-v3 | Bring `docs/wiki/` up to loop v3. Done by Loop2-loop-v3-1-wiki and Loop6-loop-v5-wiki (user, 2026-11-06: tidy) | - | - | dropped |

## Open (needs the user)
- Live check not yet done: A30 Windows restart/sign-out saving an edit typed just before.
- N3-release-and-update-plan: proposal at tag `proposal/N3-release-plan`; discussed in a separate chat ("Production readiness
  for antinotepad"): publishing, GitHub repo, license, signing, version label, installer.

## Backlog (someday)
IDs A53 and later below are dissolved (user, 2026-10-29); the Plan holds those as unnumbered ideas.
- A44a hand test (user, 2026-10-28), proposed backlog IDs (parent may renumber):
  - A54-link-hover-click (done, merged f32b92a): Ctrl+click on a link does nothing and links show no hand cursor. Only shortened URLs (over 34 chars, caret off the line) become `LinkGenerator` elements with Hand and Pressed (`Editor/Elements.cs`); short URLs are plain colored text. Not changed by A44a (its diff is only the OpenUrl name); confirm on the dev baseline.
  - (old A55, now idea native-timer-notification): a native Windows popup notification when a timer ends.
  - (old A56, now idea chime-picker) (stretch): chime picker, native sound plus two more.
  - (old A57, now A55-split-view-clip) (after current tasks): in split view, scrolling text overlaps across the divider; the divider should crop the visual of each pane.
  - (old A58, now A53-split-divider-height) (after current tasks): the split divider does not reach the bottom edge.
- Settings window runs about 90 threads (memory is stable, freed 5 of 5 cycles).
- A45-dev-build-badge (done, merged 56a85cd; user wish 2026-10-26; widened 2026-10-29 to branch glyph + `dev/<branch>@<build time>`, now in the Plan): the dev badge shows which task/build is live (e.g. "dev · A39"), so the user
  knows exactly what is being checked. Lab-first.
- A47-hotkey-recorder (user wish, 2026-10-26): binding a hotkey in Settings should record the actual key press, not typed text.
  Lab-first.
- A48-answers-toggle-clarity (user wish, 2026-10-26): the Settings toggle "Answers" (`Settings.InlineMath`) should say what it does
  (basic `=` math on any note; off = results only in math-mode notes), e.g. "Basic math in all notes", and apply live
  (today it applies only on note bind/analysis). Lab-first.
- A46-motion-timings (after A44, if needed): user finds the Settings page change and the pill appearing too fast;
  investigate Windows 11 motion timings as the reference across the app. Lab-first.
- A51-recovered-folder-always-open (user, 2026-10-27, A43b-notes-session hand test): the "Recovered notes" row shows Open only when the recovered folder has files; the recovered folder should always exist (even empty) and Open should always work. Today the row says Empty with no Open (`SettingsWindow.cs`, `recoveredCount is > 0`).
- A52-move-old-folder-rename-warning (user, 2026-10-27): moving notes logs "Notes are now in the new folder, but the old folder could not be renamed and remains at <old>: The parameter is incorrect." (seen moving `test targetA` to `B`). Notes move fine; investigate and fix the old-folder rename in `NotesLocation.Move` (looks like a path-format problem), then update the user-facing message. Reproduce on a pre-A43b build first.
- A49-large-note-perf (user observation, 2026-10-26, A42b live check): a 6,000-line note with live rendering shows about a half-second
  lag when typing; accepted as within the intended limits. CPU and GPU both rise during active scrolling and drop at idle.
  Task Manager during fast scrolling in the dev copy (user screenshot): 4.2% CPU, 2.8% GPU, 113.7 MB, 2 processes.
  Ideas, measure first (timestamps in the analysis path, a profiler pass on the 6,000-line fixture) and change one at a time:
  1. Cap or window the incremental analysis to the visible lines plus a margin, finish the rest in idle slices.
  2. Raise the 140 ms debounce adaptively with line count (for example above 2,000 lines).
  3. Throttle colorizer, folding and inline-math redraws while scrolling; skip off-screen lines in line transformers.
  4. Cache per-line brush/typeface lookups and shaped text; avoid per-redraw allocations (ChipText / FoldSelection).
  5. Cheaper full-fallback after a big paste or delete (over 1,500 lines): chunk it across frames.
  6. Check GPU cost of translucency/blur and the scroll animation while scrolling (less repainting, a cheaper blur path).
  7. Add a `--perf` selftest or stress scenario with a latency budget (keystroke to repaint on the 5,000-line fixture) so
     regressions are caught. Lab-first for anything visible.

## Keep
- Backups made from Claude shells live only in `%LOCALAPPDATA%\Packages\Claude_pzs8sxrjxfjjc\LocalCache\Local\`:
  `AntiNotepad-backup-20261024-155646` and `Antipad-backup-2026-10-23`. Don't delete.
- Real backups from upgrades: `%LOCALAPPDATA%\AntiNotepad-backup-20261024-215228`, `Programs\AntiNotepad.previous-*`.
