# STATUS archive

Finished history moved out of STATUS "Now" by Loop9-loop-slim (2026-11-12). Newest first, as it was in STATUS.

- Roadmap Hub 1 (roadmap director, user 2026-11-06; notes `.coord/directors/roadmap-hub-1.md`): T11-roadmap-hide-rows (Low, side tool) **done** (2026-11-06): Plan rows hide from All and show again from the other filters (eye chip in the left gap), All count shows hidden rows, Plan page groups Current, Next, Backlog and 4. Done; gate on dev PASS (offline); user: "testing is complete. i approve". Built by a Haiku sub-chat with Codex. T10-roadmap-live-reload done 2026-11-06.
- Loop Hub 2: L2-lv4-worktree-rules (Medium, Level 4) **done** (2026-11-08): `worktree.baseRef: head` so the app worktree button bases on the current branch, and a log-only SessionStart hook (`tools/worktree-check.ps1`) logs worktree path, branch and base to `.coord/logs/worktree-check.log`. Gate on dev PASS; user hand test: "Accept, test after merge" (app-button check after merge). L3 waits behind it (user: "No, wait for L2").
- Loop Hub 2: L1-lv4-native-guards (Medium, Level 4) **done** (2026-11-08): a Bash guard hook (`tools/guard-bash.ps1`, log-only via `tools/guard.mode`) logs merges from item worktrees and `--quit` without `ANTINOTEPAD_DATA`; `permissions.ask` on edits to STATUS, lessons and `docs/loop.md` (also prompts directors in the main checkout). Gate on dev PASS; user hand test: "Accept, prep merge". L2, L4 and L5 rebase over its `.claude/settings.json` change. L3, L4 and L7 building.
- Loop Hub 2: Loop8-merge-policy (Medium, docs-tools) **done** (2026-11-06): loop is **v5.2.0**; items land as one `--no-ff` merge commit after a commit review (tiny and back-and-forth commits folded, no fixed count) and a rebase onto dev; merge preflight builds and selftests each product commit; worktree for every item unless the user says not to; stale-Head and round-brief Files fixes; Sonnet vs Haiku routing; directors renamed to LPv5.2.0. Gate on dev PASS. Noticed: catch-up CLEAN line still says "push with -Push" after `-Push`. Wiki republished for v5.2.0. Professional release loop plan deferred to the next release (user, 2026-11-06: "not yet, save that for next release").
- Loop Hub 2: T6-gate-format-split-baseline (Low, docs-tools) **done** (2026-11-01): gate Format counts lines moved verbatim into a new file as older code (limit: common lines like `}` always match); gate on dev PASS. All Loop Hub 2 items from the Era 1 go are done.
- Loop Hub 2: T4-selftest-diff-skip-timing (Low, docs-tools) **done** (2026-11-01): `docs/coding.md` "How to split" says how to prove a pure move without timing lines; gate on dev PASS.
- Loop Hub 2: T7-paused-state (Low, docs-tools) **done** (2026-11-01): plan rows can be `paused` (branch, worktree, card kept; not counted against the cap; hook skips them); gate on dev PASS. Master 2 sets D1-ui-region-names to `paused`.
- Loop Hub 2: T5-review-gate-stale-head (Low, docs-tools) **done** (2026-11-01; user go via Master 2): merge preflight now accepts card/report-only commits after the gate; gate on dev PASS. `v0.7.0-rc1` Release marker removed (user via Master 2: "no release points at this time").
- Loop Hub 2: T2-hot-files-stale-path (Low, docs-tools) **done**, squashed onto dev (2026-11-01, user: "Approve both"); gate on dev PASS. T3-codex-unpause-docs (Low, docs-tools) **done** too: the "Codex is paused" rule is gone from `AGENTS.md`, `docs/loop.md`, `docs/agents.md`; gate on dev PASS.
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
