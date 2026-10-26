# A41-padwindow-split-impact-r2: READ-ONLY delta audit of PadWindow on the latest dev

Role: director (gpt-6.1-sol medium; luna xhigh subagents allowed). Branch `reorg/A41-padwindow-split` (this worktree, merged with
dev edea565). **READ-ONLY: edit no file except your report `.coord/reports/A41-padwindow-split-impact-r2.md`.** No builds into the
tree, never launch or kill AntiNotepad. Product: `product/AntiNotepad`.

## Context
Read first: `AGENTS.md`, `docs/decisions.md`, your earlier output `.coord/reports/A41-padwindow-split-impact.md` and the plan
`.coord/tasks/A41-padwindow-split-plan.md` (written against dev b54b476). Since then dev gained:
- A39-motion-out-of-ui (bac8a6c): `Anim` -> `Motion/`, `ChromeTokens` -> `Theme/`, `MenuEntry` -> `Editor/`, context menu shown via a
  host delegate, IL-based table-driven `LAYERS` selftest.
- A40-settings-change-event (565a1e5): `Model/SettingsService` with one `Changed` event, an ordered settings handler in
  PadWindow, `SettingsWindow` on `ISettingsHost`, all settings writers migrated (incl. PadWindow.Keys/Navigation/Layout), new
  `LAYERS` and setter guards. Use `git diff b54b476 HEAD -- product/AntiNotepad` and `git log` to see exactly what changed.
The PadWindow partials are now 2058 lines (W 555, L 493, N 404, K 285, B 321).

## Do
1. **What moved or changed**: list, by member and file:line, everything A39/A40 changed in the five partials and in the callers
   (Program, Tray, SettingsWindow, SearchOverlay, PadView). Which of your earlier findings are now stale or wrong?
2. **Re-map responsibilities** on the current code (same table shape as the first report: what, lines, state, dependencies,
   external callers). Mark which entries changed.
3. **Re-verdict each extraction candidate** (hotkeys, save scheduler, clipboard policy, navigation core, taskbar glue, trims,
   timer attention, OCR): SAFE / ENTANGLED / LEAVE, and whether A40's ordered settings handler changes the hotkey registration,
   taskbar apply and appearance wiring (where is `RegisterHotkey`, `RegisterNewNoteHotkey`, `ApplyTaskbarSetting` called now,
   in what order, and who else writes settings: bounds, split, pin, zoom, search grid).
4. **Does the earlier extraction order, risk list and guard list still hold?** Give the corrected order, sub-steps, and any new
   risk A39/A40 introduced (for example the settings Changed event firing during show/hide, handler order, replaced Settings
   object, the host delegate for the menu).
5. **Guards**: with the new IL `LAYERS` table and setter guards, state concretely which proposed rules can be added as table rows
   and which need separate checks; name any rule that A39/A40 already provide so A41 need not add it.
6. **Value check** (be blunt): measured against line counts and entanglement, is the original scope still worth doing? What is the
   smallest slice with real regression-risk reduction? What can be skipped?
7. **Show/hide speed and focus**: which proposed steps touch the hot path (hotkey -> post -> toggle -> show/focus; hide) even
   indirectly, and how each can be checked by hand in the dev copy and by a log/selftest.
8. **Uncertain** list.

## Report
`.coord/reports/A41-padwindow-split-impact-r2.md`, sections 1-8, tables where useful, file:line for every claim. Keep it shorter
than the first report: focus on deltas.

## Done means
Only the report file is added by you (`git status` shows nothing else new from your run).
