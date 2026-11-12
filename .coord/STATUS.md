# STATUS

Updated: 2026-11-06
Product: AntiNotepad (`product/AntiNotepad`), version label 0.6.0 (untested).

## Working rules right now
- Live top-level chats (user, 2026-11-06):
  - `Master 4 · LPv5.3.0 · design-tokens`: app and design work; owns STATUS (notes `.coord/directors/master-4.md`).
  - `Loop Hub 3 · LPv5.3.0 · loop-tools`: `Loop` and `T` items (notes `.coord/directors/loop-hub-3.md`).
  - `Roadmap Hub 1 · LPv5.3.0 · roadmap`: the roadmap page and Plan shape (notes `.coord/directors/roadmap-hub-1.md`).
  - Masters 1-3 and Loop Hub 1 are closed.
- Top-level chats route work, they do not do it: they ask the user, and default to a Sonnet-or-lower sub-chat per
  change that runs Codex. After dispatch they step back; the user works with the sub-chat, which finishes and reports.
- Updates between top-level chats are need-to-know: tell another chat only when a change heavily affects its domain,
  and only after the task is finished; if unsure, ask the user.
- **Loop v5.3.0 is live** (L6-lv4-docs-diet, 2026-11-08: loop.md split by audience into loop.md and loop-sessions.md, coding rules path-scoped in `.claude/rules/coding.md`, AGENTS.md trimmed, `L` prefix; v5.2.0 Loop8-merge-policy, 2026-11-06: rebase plus one `--no-ff` merge commit per item, tidy commits that each build, worktree by default; v5.1.0 Loop7-top-level-rules; v5.0.0 Loop6-loop-v5-wiki). Each loop version bump updates this line and the three director names above. **Loop freeze:** lifted (Loop4-retro-fixes settled the retro's questions); rules change only on a miss repeated in `.coord/lessons.md` or on the user's ask.
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
- Loop Hub 3: Loop9-loop-slim (Medium) **done** (2026-11-12): loop slimmed (settings drift ignored, fewer `ask` prompts, own-notes-only hook, STATUS 82 to 54 KB, lessons 3 KB, hot-file budget WARN). Report `.coord/reports/Loop9-loop-slim.md`. Next: `Loop10-status-split` (card ready, waits for the user).
- Master 4 (top-level chat beside Loop Hub 2, user 2026-11-06; took over design-tokens from Master 3; notes `.coord/directors/master-4.md`): A4-design-tokens merged to dev 2026-11-05; branch feat/A4-design-tokens kept with its worktree. 2026-11-06: STATUS tidied and the roadmap brought up to date. A5-search-bar-overlap **done** 2026-11-06 (hand test: "perfect, its good enough"; gate on dev PASS offline, Size WARN Layout 575->606). A6-note-switch-instant-swap **done** 2026-11-06 (merged before its hand test, user: "Merge A5 and A6, both into dev, then run a new build"; gate on dev PASS offline). A7-version-rc2 **done**; **v0.7.0-rc2 released** 2026-11-06 (main fast-forwarded to dev, tagged; user: "Go, lean release"); installed copy waits for the user to run `tools/install.ps1`. Next: a professional release loop as a Loop item for Loop Hub 2, after the user says so on your go. Design backlog: scale-apply, design-plugins-review, timer-names (deferred).
- Release `v0.7.0-rc1` (user, 2026-11-01): main fast-forwarded to dev after Era 1, tagged, pushed. Release build 0 warnings, SELFTEST PASS. User installed it on the production copy; dev builds removed.
- Loop Hub 2: L7-lv4-playbook (Low, Level 4) **done** (2026-11-08): `docs/playbook.md`, a short habit guide with step-by-step examples for the next A, T and L items, linked from AGENTS.md. Gate on dev PASS; user hand test: "reads clearly" (no live walk-through yet; guard still log-only). **Level 4 complete** (L1 to L7 on dev); Level 5 rows L8 to L15 stay proposed.
- Loop Hub 2: L6-lv4-docs-diet (Low, Level 4) **done** (2026-11-08): loop is **v5.3.0**; `docs/loop.md` split by audience (sessions detail in `docs/loop-sessions.md`), coding rules path-scoped in `.claude/rules/coding.md`, AGENTS.md trimmed, skills point at moved sections, `L` prefix in naming rules. Gate on dev PASS; no UI, user: "Green light". Directors renamed to LPv5.3.0. Noticed: `docs/loop.md` states "Loop version: v5.3.0" twice (top and Sessions). L7 (playbook) is the last Level 4 item.
- Loop Hub 2: L5-lv4-status-line (Low, Level 4) **done** (2026-11-08): `tools/status-line.ps1` wraps the user's global status line and adds one `loop:` line (no cost or context %; Desktop does not draw status lines, a Desktop view would be a future mod); `tools/session-where.ps1` on SessionStart startup|resume; per-turn loop hook trimmed (487 to 279 chars). Gate on dev PASS; user hand test: "yeah good enough for now" (live terminal render unverified).
- Loop Hub 2: L4-lv4-skill-diet (Low, Level 4) **done** (2026-11-08): next-task, review-task, merge-task and scan-backlog skills made user-only, with live git facts and a size count. Gate on dev PASS; user hand test: "approve". Noticed: review-task still names design-reviewer; the L3 `reviewer` agent could replace it (one-word swap, not scheduled).
- Loop Hub 2: L3-lv4-agent-roster (Low, Level 4) **done** (2026-11-08): builder, reviewer and auditor agent files in `.claude/agents/` with fixed model, effort and tools; one pointer line each in `docs/agents.md` and `docs/loop.md`. Gate on dev PASS; user hand test: "ready to merge". Merged after L2 (user: "No, wait for L2"); L6 rebases over its `docs/loop.md` line.
- D1-ui-region-names (Low, docs-tools): **paused indefinitely** (user, 2026-11-01). Branch pushed at d96a11f; review of names
  and pictures (its hand test) is what is left. Worktree kept.

Older finished items: `.coord/status-archive.md`.

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
| **Era 1 start (Master 2 priority, 2026-11-01: done rows first, then run order; numbers only on your yes)** | | | | |
| Loop10-status-split | STATUS holds only active work; pending list, archive and cards hold the rest; roadmap reads all tiers; dedupe docs. Card: `.coord/tasks/Loop10-status-split.md`. Starts after Loop9 merges | Medium | Medium | building (a-b done; step c waits for the preview nod) |

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
