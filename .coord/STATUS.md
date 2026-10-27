# STATUS

Updated: 2026-10-26
Product: AntiNotepad (`product/AntiNotepad`), version label 0.6.0 (untested).

## Working rules right now
- Codex is back (user, 2026-10-25): director Sol medium delegating to Luna xhigh; Claude briefs, reviews, commits.
- Computer Use is off until the user's explicit go-ahead; live checks are done by the user by hand in the dev copy.
- Never run `AntiNotepad.exe --quit` without `ANTINOTEPAD_DATA`: it targets the installed copy's instance.

## Where things are
- `dev` reverted 2026-10-25 to just after A30 (user): A31, A32 and A33 reverted, A34 dropped. Product code = r4 +
  A29-incremental-analysis + A30-background-save-and-log; windowing identical to r4 (Windows corners/shadow, bars, app's own
  show fade, A22-A25 taskbar stand-in). Reason: A32/A34 forced caption-window styles onto the frameless pad, losing corners
  and shadow and breaking Docked/Overlay/Float. 0 warnings, SELFTEST PASS.
- `main`: tag `antinotepad-v0.6.0-untested-r6` (2026-10-25), the user-approved good version = reverted dev (r4 windowing
  + A29 + A30). Installed copy: r5 (broken frame) until the user reinstalls r6 with `tools/install.ps1`.
- Finished briefs, reports and the A1 audit doc were removed from the tree on 2026-10-25; they live in git under tag
  `archive/coord-2026-10-25`. Locked choices stay in `docs/decisions.md`.

## Now
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
  starts per task on the user's go, from that plan, with the user's hand test before merge. Next: A43a-runtime-log-and-launch.
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
  -> merge. A43a dispatched on `reorg/A43a-runtime-log-and-launch`. Doc fix pending: `docs/dev-loop.md:11` (custom
  ANTINOTEPAD_DATA follows its own location.json after an in-app move).
- A40-settings-change-event merged into dev (565a1e5, user-approved live 2026-10-26): SettingsService + one Changed event,
  SettingsWindow on ISettingsHost, all writers migrated; guards now 12 LAYERS lines + `SETTINGS Changed`; 0 warnings, SELFTEST PASS.
  Fix: SearchGrid/SplitSlot/Split now saved. Folding/InlineMath apply only on note bind/analysis (pre-existing; backlog).
- Native-window work (A32/A34) parked; any retry must not change the window's frame styles and
  goes lab-first. A34 (lab) was dropped: thick native border, bars broken in all three styles.

## Open (needs the user)
- Reinstall the installed copy with r6 (`tools/install.ps1` from Windows Terminal) to get the r4 window back.
- Live check not yet done: A30 Windows restart/sign-out saving an edit typed just before.
- N3-release-and-update-plan: proposal at tag `proposal/N3-release-plan`; discussed in a separate chat ("Production readiness
  for antinotepad"): publishing, GitHub repo, license, signing, version label, installer.

## Backlog (someday)
- Settings window runs about 90 threads (memory is stable, freed 5 of 5 cycles).
- A45-dev-build-badge (user wish, 2026-10-26): the dev badge shows which task/build is live (e.g. "dev · A39"), so the user
  knows exactly what is being checked. Lab-first.
- A47-hotkey-recorder (user wish, 2026-10-26): binding a hotkey in Settings should record the actual key press, not typed text.
  Lab-first.
- A48-answers-toggle-clarity (user wish, 2026-10-26): the Settings toggle "Answers" (`Settings.InlineMath`) should say what it does
  (basic `=` math on any note; off = results only in math-mode notes), e.g. "Basic math in all notes", and apply live
  (today it applies only on note bind/analysis). Lab-first.
- A46-motion-timings (after A44, if needed): user finds the Settings page change and the pill appearing too fast;
  investigate Windows 11 motion timings as the reference across the app. Lab-first.
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
