# Pending work

Rows not in work now: proposed, needs you, planned, paused and deferred (user order, as they were in STATUS). Same 5 columns as the STATUS Pending work table; `tools/roadmap-data.ps1` reads this file. Approve moves a row to STATUS; Merge moves it to `status-archive.md`. One line per item.

| Item | Scope | Tier | Effort | State |
|---|---|---|---|---|
| **Next up (user, 2026-11-16: higher priority than the rest; order is a proposal)** | | | | |
| Loop15-preflight-stale-head | merge-preflight fails on "stale Head" at many merges; Findings first (may drop). Card in `.coord/tasks/` (user via Master 6, 2026-11-18) | Low | Small | request |
| T15-ffmpeg-frame-capture-trial | Trial: record the dev window with ffmpeg, tile frames into one sheet so Claude can judge motion. Card in `.coord/tasks/`. Test 1 done; worktree `chore/T15-ffmpeg-frame-capture-trial` kept. Paused by the user 2026-11-17 | Low | Small | paused |
| **Era 1 start (Master 2 priority, 2026-11-01: done rows first, then run order; numbers only on your yes)** | | | | |
| D1-ui-region-names | Names for the parts of the window in `docs/glossary.md`, so cards, comments and chat use the same words; can run beside selftest-split (no shared files). Would be D1 Numbered by the user 2026-11-01 (parallel; merge order A2, A3, D1). Card `.coord/tasks/D1-ui-region-names.md`; a D1 lead session owned by Master 2 **Scope change (user, in D1 chat, 2026-11-01):** SVGs replaced by an interactive HTML region map (`docs/regions/`, hover a name to highlight it on a screenshot, similarity % and top 5), screenshots from the dev copy ("Yes, both"); page built by Codex terra (b3255a3); captures blocked: the Computer Use grant resolved to the installed exe and was denied. Expected 2-3 h **Paused indefinitely (user, 2026-11-01):** branch `docs/D1-ui-region-names` pushed at d96a11f (map with six dev-copy captures, gate PASS; pause on the card), waiting on the user's review; worktree kept | Low | Small | paused |
| **Loop Level 4: config enforces the loop (user, 2026-11-08, via "Loop Levels 4+5 · native features"; approved; merge in this order; new prefix `L`, Loop1-Loop8 keep theirs)** | | | | |
| L16-lv41-agent-progress-panel | Level 4.1: a Claude Code mod (panel beside the director chat, see the `plugin-authoring` skill) with one row per linked sub-chat: item, phase, progress bar, and an icon while Codex runs for it. Why: the sidebar does not show which sub-chats are working; the `[Codexing..] ` title prefix (stopgap since 2026-11-08) depends on the chat remembering. Wanted for day-to-day use after Level 4; showcase piece. Inputs: linked sessions and state, STATUS states, card phase, Codex process or log activity. Unverified: what a mod can read, how to detect Codex reliably, where mod files live (repo or user config; decides the branch flow). Claude Code Desktop does not appear to draw status lines, so the Desktop view of loop info (worktree, item, in-flight items, phase) belongs here too, above the chat next to the per-chat progress bars and Codex icon; it can reuse the logic in `tools/status-line.ps1`. Pairs with L5-lv4-status-line and L8-lv5-loop-state-tool, blocked by neither. First step: findings on the three unknowns, then a plan for the user. Owner Loop Hub 2. User via "Agentic Levels 4+5 · native features", 2026-11-08. Deferred by the user 2026-11-12 (check lot-map first) | Low | Medium | deferred |
| idea: delete-note-button | Delete note button in the bottom pill bar, part of the bottom-bar overhaul. Today delete is key-only (Ctrl+Shift+Backspace) and search. Seen in A29-shortcut-map (user, 2026-11-19) | Medium | Small | proposed |
| idea: roadmap-l-prefix | Roadmap page colors `L` rows as old items (grey): `family()` in `tools/roadmap/index.html` knows A, Loop, T, D, C only; add `L` with the loop color. Added by Roadmap Hub 1 (2026-11-08) | Low | Small | proposed |
| **Loop Level 5: the loop runs as an automated pipeline (later; the user discusses it in "Loop Levels 4+5 · native features" first)** | | | | |
| L8-lv5-loop-state-tool | Loop state in card front matter, `loop.ps1` verbs, a can-merge hook | Medium | Large | proposed |
| L9-lv5-headless-review | Headless review runs that return JSON verdicts | Medium | Medium | proposed |
| L10-lv5-ci | CI on push to dev and on release tags | Medium | Medium | proposed |
| L11-lv5-scheduled-routines | Scheduled routines: nightly backlog scan, doc-drift check | Low | Medium | proposed |
| L12-lv5-loop-config | One `loop.config.json` plus a lint for it | Low | Medium | proposed |
| L13-lv5-metrics-page | Metrics page from usage and lessons | Low | Medium | proposed |
| L14-lv5-public-writeup | Public write-up of the results | Low | Small | proposed |
| L15-lv5-roadmap-loop-state | Roadmap page reads item phase from L8's card front matter instead of parsing STATUS table text. Added by Roadmap Hub 1 (2026-11-08) | Low | Medium | proposed |
| **Reorg leftovers** | | | | |
| C29-headless-render-probe | Off-screen Avalonia render helper for the selftest, so audits can reproduce visual bugs and `split-pane-clipping` checks pixels automatically instead of manually; run after A42d-list-commands (user order 2026-10-29: A42c, A42d, then C29; all touch the selftest) **Proposed (Master 2, 2026-10-30):** renumber as an `A` item (product selftest code), run after selftest-split; you choose A, T or idea. | Medium | Small | proposed |
| **Design system and ownership** | | | | |
| step: scale-apply | After the lab is approved: tokens become scale > role > control tiers and the app adopts the scale; new DESIGN hash | High | Medium | plan ready |
| step: design-plugins-review | After design-questions (user, 2026-11-02): install the Design (Anthropic) and design-skills plugins, run them end to end on the project, compare with our findings; then decide whether a top-level Design Master chat reviews the design independently | Low | Small | plan ready |
| step: timer-names | Fuzziest: named behavior timers (save, debounces, toast hold, caret blink, bar sleep; about 40 values) at today's values, owned where they are used (user, 2026-11-01: timers included) Trimmed from design-tokens (user, 2026-11-02: "Shape, then trim"); revive with design-unify if those values need changing | High | Medium | deferred |
| idea: code-ownership | Every responsibility has one owner and no copies; behavior unchanged | - | - | plan ready |
| step: notes-path-dedupe | One same-or-nested path rule in `Platform/` replacing the copies in `NotesSession`, `NotesLocation`, `SettingsWindow` and `KeepOldCopy`; CONTRACTS row | High | Small | plan ready |
| step: ui-kit-merge | One control kit: Settings' private controls merge into `Kit`; `Material()` and `Ago()` move to their owners | Medium | Medium | plan ready |
| step: ownership-map | `docs/architecture.md` owner per responsibility; `NotesSession`, `RuntimeLog` into `App/`; LAYERS rows for `App/` and `Motion/` | Low | Small | plan ready |
| step: behavior-timer-names | Optional: name save, debounce, toast and taskbar timers beside their owners, values unchanged | High | Small | plan ready |
| **Cleanup** | | | | |
| idea: archive-finished-sessions | Archive finished Claude sessions (era-0 C16 retired); no code (user, 2026-11-06: tidy) | Low | Small | plan ready |
| **Performance** | | | | |
| A11-settings-thread-count | Find why the Settings window runs about 90 threads; report, fix only if cheap (was A74); scope confirmed by user 2026-11-09. Report done on `fix/A11-settings-thread-count` (no change needed: about 71 of 81 threads are GPU driver and ANGLE threads, Settings adds 1-2, no leak). Paused by the user 2026-11-12, not merged | Low | Small | paused |
| idea: large-note-perf | Measure then reduce typing lag in a 6,000-line note; one change at a time (ideas in Backlog; era-0 A49 retired) (user, 2026-11-06: tidy) | Medium | Large | plan ready |
| **Features and polish (lab-first where visible)** | | | | |
| idea: notes-folder-move-flow | Choosing and moving the notes folder is clear and safe; first step follows code-ownership's notes-path-dedupe | - | - | plan ready |
| step: path-validator | One shared Platform check for a chosen folder: blocks invalid, same, nested, not writable, has notes; warns new, non-empty, network, low space, cloud-synced | High | Medium | plan ready |
| step: editable-path-field | Editable path box plus Browse in Settings with a validation line; Migrate enabled only when valid or warned; lab mock first | Medium | Medium | plan ready |
| step: move-preview | Before moving, show note count, size and what happens to the old folder | Medium | Small | plan ready |
| step: move-undo | "Move back" after a move, using the kept old copy | High | Medium | plan ready |
| step: picker-last-location | The folder picker opens at the last picked folder | Low | Small | plan ready |
| step: single-result-message | Settings shows the move's own result message instead of rebuilding it | Low | Small | plan ready |
| idea: answers-toggle-clarity | Rename the "Answers" toggle to say what it does, and apply it live; lab-first (era-0 A48 retired) (user, 2026-11-06: tidy) | Low | Small | plan ready |
| idea: native-timer-notification | Windows notification when a timer ends (was A55) | Medium | Medium | plan ready |
| idea: chime-picker | Choose the timer chime: native sound plus two more, stretch (was A56) | Low | Medium | plan ready |
| idea: floating-scrollbar | Modern floating scrollbar at the side, shown only while scrolling (user, 2026-10-29, A55 hand test: "a scroll bar shows, i didn't expect that"); open: hide vs fade, both split panes, existing bar styles | - | - | plan ready |
| idea: timer-enter-no-autostart | In a timer note, Enter after a timer line (e.g. `10s test`) starts that timer at once (`Editor/PadView.cs:698`, existing by design); user wants to write several timers first (user, 2026-10-29, A42c hand test: "that shouldnt be the case because i might be writing multiple timers"); open: what starts a timer then (Ctrl+Enter, chip click only?) | - | - | plan ready |
| **UI / UX / animation running list (user, 2026-11-12; unnumbered until scope is confirmed; lab-first where visible)** | | | | |
| A34-pill-bar-per-mode | (idea: pill-bar-per-mode + permanent-view-swap-button) Pill bar: Standard Notes shows pagination only, Permanent Notes dot selectors only; Permanent selector leaves the bar and a top-right button swaps views. First step of the bottom-bar revamp; localhost mockup lab with ideas and recommendations first. Card in `.coord/tasks/` (user via Master 7, 2026-11-20) | Medium | Medium | request |
| idea: overlay-scroll-feel | Help and Search overlays still scroll the stock way; make them follow the Feel scroll style like Settings (A19-settings-scroll-feel) | - | - | plan ready |
| idea: win11-window-motion | Program window animations (including Settings) snap in/out or do not follow Windows 11 / WinUI 3 standard motion; align them. Overlaps an old someday note on motion timings in the Backlog section below (never a real item) | - | - | plan ready |
| idea: taskbar-summon-timing | Summoning from the taskbar animates but feels off and too fast | - | - | plan ready |
| idea: settings-tab-motion | Settings tab switching animates at the wrong speed and style; match Windows 11 / WinUI 3 | - | - | plan ready |
| D2-ui-motion-research | Deep research report (no product code): app design, UI, motion and consistency principles; WinUI 3 / Fluent motion; Avalonia's animation tools; skills and plugins for design and motion; how AI can implement and evolve a design consistently. Ends in a short recommendation for this app's motion rules (purpose, easing, timing) and a way for the user to judge motion by seeing it, not numbers. Asked by the user 2026-11-12 after A14's lab values; Haiku sub-chat + Codex Sol. Paused by the user 2026-11-12 (sub-chat stopped, nothing written yet that matters) | Low | Medium | paused |
| idea: split-view-modes | After the items above: left pane stays Standard Notes; Split View click offers Permanent stack or Regular notes for the right pane. Pill bars: Standard right pane = bar 1 normal, bar 2 pagination only; Permanent right pane = bar 1 normal, bar 2 permanent selector. Related: two old someday notes in the Backlog section (split text overlap, divider height) | - | - | plan ready |
| idea: status-pill-redesign | Restyle the pad's notice pill (today's toast: deleted note, empty note, paste): status dot (green found, grey fallback), position choices (bottom center, bottom right, top right, top bar after Saved), optional detail suffix such as "2 lists, 1 table", hold time, one place for wording. Lab with the mock: `tools/playground/a23-paste-pill-lab.html` on branch `feat/A23-paste-keep-structure`. A23 itself only reuses today's toast. From the A23 hand test, user 2026-11-17 | Low | Medium | proposal |
| idea: table-horizontal-scroll | Pasted markdown tables wrap when a row is wider than the window and the pipes break apart. Needs a no-wrap or horizontally scrollable region for table lines (and a call on column width). From A23 hand test, user 2026-11-17 | Medium | Medium | proposal |
| A28-emoji-render-gaps | Findings started 2026-11-18 (chat a28-emoji; card in `.coord/tasks/`). Some emoji draw wrong in the editor: green circle shows as a grey disc while 💡 🥺 🔥 draw in color. Paste keeps the right character, so this is text rendering or font fallback. From A23 hand test, user 2026-11-17 | Low | Small | deferred (user, 2026-11-18: parked; not reproduced; resume with the exact grey character pasted into a dev slot) |
| idea: bars-edge-reveal | Top and bottom bars flash on and off when the mouse rests inside the note and you type then pause (Fade while typing hides them, the pause shows them again). Wanted (user, 2026-11-18, A21b hand test): bottom bar shows briefly on note switch then hides; also shows when the mouse is within about 10-12 px of the bottom edge. Top bar shows within about 10-12 px of the top edge, and showing it shows the bottom bar too. A typing pause alone never reveals them. Must avoid edge jitter. Jitter ideas: (1) hysteresis: enter at 10-12 px, leave only past about 40 px; (2) dwell: pointer stays in the strip about 150 ms before reveal; (3) minimum on-screen time about 600 ms and a hide delay after leaving; (4) require real pointer movement, so a still pointer never triggers; (5) typing hides at once and holds the bars closed until the pointer leaves the strip and re-enters; (6) one state machine (hidden, revealing, shown, hiding) instead of separate timers; (7) hit-test the strip in window coordinates, not over bar controls that move. Open: lab-first (visible); Settings text for Fade while typing; how Overlay and Float below bar styles behave | Medium | Medium | proposal |

## Backlog (someday)
Moved from STATUS by Loop10c-status-shape. IDs A53 and later are dissolved (user, 2026-10-29); the table above holds those as unnumbered ideas.
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
