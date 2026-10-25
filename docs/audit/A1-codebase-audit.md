# AntiNotepad code audit

A1-codebase-audit, re-audited by A5-audit-refresh, decisions applied by A6-audit-decisions · 2026-10-23 · all 91 tracked files

> **Update (A6-audit-decisions):** your 8 decisions are applied on `feat/A6-audit-decisions` (not yet in dev). 13 findings
> are fixed there, plus the `/ocr` repeat bug. **12 remain open**, all medium or low; none needs a decision. See
> "Fixed in A6" below. The rest of this document is the A5 audit as written.

## In short

AntiNotepad builds cleanly, passes its selftest, and has no outdated or vulnerable packages. The code is in good shape for
everyday use. The real risks are in **rare situations**: a damaged or locked file, a failed save, or odd pasted text. In
those cases the app can **lose note text** or **crash**. None of these risks has been seen happening; they were found by
reading the code.

Since the first audit, 7 small issues were fixed. The re-audit found 5 new ones. **25 are open**: 7 high, 14 medium,
4 low. Most of the high ones need a short decision from you before they can be fixed.

Severity: **High** means it can lose notes or crash the app. **Medium** means a real risk or wasted work with a specific
trigger. **Low** means tidy-up.

## At a glance

| | High | Medium | Low | Total |
|---|---:|---:|---:|---:|
| **Open** | 7 | 14 | 4 | **25** |
| Fixed since A1 | 0 | 1 | 6 | 7 |

| Open, by area | Count | Findings |
|---|---:|---|
| Your notes (data safety) | 5 | A1-1, A1-2, A5-1, A5-3, A1-19 |
| Crashes | 4 | A1-4, A1-5, A1-6, A5-2 |
| Speed and memory | 10 | A1-3, A1-7 to A1-14, A1-16 |
| Install and tools | 5 | A1-15, A1-17, A1-20, A5-4, A5-5 |
| Repo housekeeping | 1 | A1-27 |

## What needs your decision

Each item has a recommendation. One word back ("yes to all", or the items you'd change) is enough.

1. **Note files the app can't account for** (A1-1, A5-1). Right now an unlisted note file is deleted, and an unreadable one
   opens as empty and can be overwritten.
   *Recommend:* never delete these; move unlisted files to a `recovered` folder; open an unreadable note read-only with a
   "couldn't read, retry" message.
2. **Quitting after a failed save** (A1-2). Right now it shows a toast and quits anyway.
   *Recommend:* don't quit; show "Couldn't save" with Retry and Quit anyway.
3. **Limits for strange input** (A1-4, A1-5, A1-6). These stop crashes from pasted text.
   *Recommend:* list numbers above 9 digits stop auto-numbering; timers above 99 days are ignored as timers; calculations
   deeper than 64 brackets show "too complex".
4. **Text recognition when you switch notes** (A5-3).
   *Recommend:* the text still lands in the note you started from.
5. **Timers** (A1-3, A1-16).
   *Recommend:* Settings preview countdowns stop when Settings closes; a running timer stops when its line or note is
   deleted, and keeps running when you only edit its label.
6. **Mixed currencies** (A1-19), for example `$5 + €3`.
   *Recommend:* show an error instead of a total.
7. **Which processors the installer targets** (A1-20).
   *Recommend:* Windows x64 only for now; add ARM64 if you ever use an ARM PC.
8. **Saved session files in git** (A1-27).
   *Recommend:* stop tracking `.claude/atlas/` and ignore it.

## Ready to fix without a decision

- **Safe, mechanical:** A5-4 and A5-5 (two small tool-script checks).
- **Clear fix, needs care:** A5-2 (fall back to the backup note list), A1-15 (quit request during startup), A1-17 (installer
  rollback), and the speed items A1-7 to A1-14.

## The serious ones, explained

### A1-1-recovery-deletes-bodies · High · Your notes
- **What can happen:** if the note list (`index.json`) is damaged, the next start can permanently delete note text that is
  still on disk.
- **Why:** after loading, `RemoveOrphans()` deletes every body file the loaded list doesn't name. If both list files fail,
  or the older backup list is used, real notes look "orphaned". A crash between writing a new note and writing the list has
  the same effect. `Model/Store.cs:243`
- **Fix:** never delete during recovery; move unlisted files aside; save body and list as one unit. Effort M.

### A1-2-quit-after-save-failure · High · Your notes
- **What can happen:** if saving fails (disk full, no permission), Quit still closes the app, and the latest edits are lost.
- **Why:** `Quit()` calls `Save()`, which only shows a toast on failure, then closes. The installer's `--quit` can't tell
  either. `UI/PadWindow.cs:183`
- **Fix:** `Save()` returns success; Quit stops and offers Retry or Quit anyway. Effort M.

### A5-1-body-read-failure-overwrite · High · Your notes · new
- **What can happen:** a note that briefly can't be read, for example while another program locks the file, opens empty.
  If you type in it, your typing replaces the original text on disk.
- **Why:** `ReadBody` turns a read error into an empty string, which is cached as the note's text. Autosave later writes
  that over the real file. `Model/Store.cs:178`
- **Fix:** keep "unreadable" separate from "empty"; block edits and saves to that note until a read succeeds. Effort M.

### A1-4, A1-5, A1-6 · High · Crashes
- **What can happen:** some unusual pasted text crashes the app:
  - a list number with more than 10 digits, when you press Enter or Tab;
  - a huge timer such as `999999999999999999999h`;
  - a calculation nested thousands of brackets deep.
- **Why it's worse than a normal crash:** the crash handler only logs (`Program.cs:31`, `Handled` is never set), and
  autosave keeps the text. The same note can crash the app again each time it is opened.
- **Why, per finding:**
  - A1-4: `int.Parse` with no bounds check, `Editor/PadView.cs:597`.
  - A1-5: `TimeSpan.FromSeconds` overflows, and `\d` also matches non-ASCII digits, `Model/Timers.cs:57`.
  - A1-6: the recursive parser has no depth limit, `Calc/Calculator.cs:683`.
- **Fix:** bounded `TryParse` and a parser depth limit, with the limits from decision 3. Effort S, S, M.

### A1-3-preview-timer-retains-settings · High · Speed and memory
- **What can happen:** a countdown started in the Settings preview keeps the closed Settings window in memory, ticking 4
  times a second, until the countdown ends.
- **Why:** the preview has its own `Timers`, whose running `DispatcherTimer` keeps the preview, and through it the window,
  alive. `Release()` doesn't stop it. `Editor/PadView.cs:136`
- **Measured:** five plain open/close cycles freed the window every time (see Evidence). This trigger is the most likely
  cause of the `freed=False` you saw.
- **Fix:** stop and unsubscribe the preview's timers on release. Effort M.

## All open findings

| ID | Sev | Area | What it means | Where | Effort | Decision |
|---|---|---|---|---|---|---|
| A1-1-recovery-deletes-bodies | High | Notes | A damaged note list can delete notes still on disk | `Model/Store.cs:243` | M | 1 |
| A1-2-quit-after-save-failure | High | Notes | Quit after a failed save loses recent edits | `UI/PadWindow.cs:183` | M | 2 |
| A5-1-body-read-failure-overwrite | High | Notes | An unreadable note opens empty; typing overwrites it | `Model/Store.cs:178` | M | 1 |
| A1-4-list-marker-overflow | High | Crash | A huge list number crashes on Enter or Tab | `Editor/PadView.cs:597` | S | 3 |
| A1-5-timer-parser-can-throw | High | Crash | A huge or odd timer value crashes | `Model/Timers.cs:57` | S | 3 |
| A1-6-calculator-unbounded-recursion | High | Crash | Very deep brackets crash the calculator | `Calc/Calculator.cs:683` | M | 3 |
| A1-3-preview-timer-retains-settings | High | Memory | A preview countdown keeps closed Settings alive | `Editor/PadView.cs:136` | M | 5 |
| A5-3-ocr-result-follows-rebound-pane | Med | Notes | Recognized text can land in the wrong note | `UI/PadWindow.Navigation.cs:346` | M | 4 |
| A1-19-mixed-currency-addition | Med | Notes | `$5 + €3` shows `$8` | `Calc/Calculator.cs:296` | S | 6 |
| A5-2-invalid-index-bypasses-backup | Med | Crash | A note list with missing parts stops startup; the backup is never tried | `Model/Store.cs:190` | M | none |
| A1-16-timer-history-never-pruned | Med | Memory | Old timers pile up in long sessions | `Model/Timers.cs:25` | M | 5 |
| A1-7-stale-fence-cache | Med | Memory | Moving code blocks leaves old copies in memory | `Editor/Structure.cs:176` | M | none |
| A1-8-whole-document-analysis | Med | Speed | Every keystroke re-checks the whole note | `Editor/PadView.cs:343` | L | none |
| A1-9-fence-copy-per-colored-line | Med | Speed | Scrolling a code block copies all of it per line | `Editor/Structure.cs:209` | M | none |
| A1-10-render-path-allocation | Med | Speed | Drawing creates new brushes and text each frame | `Editor/Renderers.cs:111` | M | none |
| A1-11-synchronous-note-search | Med | Speed | Search rereads notes from disk on each letter | `UI/SearchOverlay.cs:79` | M | none |
| A1-12-settings-search-materializes-all | Med | Speed | Settings search rebuilds every page on each letter | `UI/SettingsWindow.cs:256` | M | none |
| A1-13-progress-swipe-close-retention | Med | Memory | Closing Settings mid-swipe may leave a timer running | `UI/NoteSwipe.cs:131` | S | none |
| A1-14-synchronous-save-and-log | Med | Speed | Saves and log writes run on the UI thread | `UI/PadWindow.cs:60` | L | none |
| A1-15-startup-quit-is-dropped | Med | Tools | A quit request during startup is ignored | `Program.cs:74` | M | none |
| A1-17-upgrade-has-no-rollback | Med | Tools | A failed upgrade can't restore the old version | `tools/install.ps1:132` | M | none |
| A1-20-cross-platform-payload | Low | Tools | The build ships Linux and macOS files too | `AntiNotepad.csproj:20` | S | 7 |
| A1-27-tracked-session-snapshots | Low | Repo | Claude session files are tracked in git | `.claude/atlas/*.json` | S | 8 |
| A5-4-worker-output-slug-unchecked | Low | Tools | A bad task name can write logs outside `.coord/` | `.coord/tools/codex-run.sh:25` | S | none |
| A5-5-hotkey-case-depends-on-language | Low | Tools | The hotkey helper breaks on "i" under Turkish locale | `tools/press-hotkey.ps1:11` | S | none |

Paths are under `product/AntiNotepad/` unless they start with `tools/` or `.coord/`.

## Medium and low findings: technical notes

**Your notes**
- **A5-3 (new):** `OcrIntoPane` awaits recognition, then reads `pad.Editor.Document` and the caret. If the pane was rebound
  to another note meanwhile, the text goes there. Fix: capture the target document before the await.
- **A1-19:** `TryAdd` keeps `l.Cur ?? r.Cur` without comparing currencies. Fix: reject mismatched currency codes.

**Crashes**
- **A5-2 (new):** a note list that parses but has, for example, `"Settings": null` is accepted, then `Normalize()`
  dereferences it outside any `try`, so startup crashes without trying `index.json.bak`. Fix: validate each candidate
  before accepting it.

**Speed and memory**
- **A1-16:** `Timers._entries` only grows: keys are note id plus line text, and each 250 ms tick scans them all. Fix: prune
  entries nothing references.
- **A1-7:** the code-block color cache is keyed by opening line number, and stale keys stay until the note is reopened.
  This is bounded by line count, not a true leak. Fix: prune after each structure update.
- **A1-8:** `Analyse()` rebuilds structure, folds and math for the whole note after every edit (up to 1500 lines; larger
  notes after a 140 ms pause). Some blank-line and indent scans are quadratic. Fix: incremental analysis. Effort L.
- **A1-9:** `Get(fence)` copies the full code-block text for every colored line, before the cache check. Fix: validate once
  per pass.
- **A1-10:** the answer chips, fold chevrons, edge fade and glyph icons create a new `FormattedText`, brush or pen on each
  render. Fix: cache by palette, font and DPI.
- **A1-11:** each search keystroke calls `PeekText()` for every note, once per term, which reads unloaded notes from disk on
  the UI thread. Fix: debounce, read each note once, search in the background.
- **A1-12:** Settings search builds all six sections' controls, then filters them. Fix: filter descriptions first, build
  only the matches.
- **A1-13:** `NoteSwipe` returns before `timer.Stop()` while `_owed` is not zero. If frames stop arriving after close, the
  timer may repeat forever. Not verified at runtime. Fix: cancel the swipe on preview release.
- **A1-14:** `Store.Save()` serializes and writes files synchronously on the UI thread, and so does logging. Fix: save a
  snapshot in the background and await the final save on quit. Effort L.

**Install and tools**
- **A1-15:** a `--quit` that arrives before `_window` is attached is dropped, so the installer waits 5 s and aborts safely.
  Fix: remember the pending quit.
- **A1-17:** `install.ps1` copies over the old install before its selftest, with no rollback. Your notes are not affected.
  Fix: stage the new files, keep the old install, then swap.
- **A1-20:** a framework-dependent publish with no runtime identifier ships Linux and macOS native libraries (24 `.so`, 1
  `.dylib`). Fix: publish for `win-x64`.
- **A5-4 (new):** the task argument goes into log and report paths unchecked, so `../` would escape `.coord/`. Fix: accept
  only `<ID>-<slug>`.
- **A5-5 (new):** `$Key.ToUpper()` uses the current culture, and Turkish turns `i` into `İ`, which isn't a byte. Fix:
  `ToUpperInvariant()`.

**Repo housekeeping**
- **A1-27:** four `.claude/atlas/*.json` files are Conversation Atlas session snapshots. They hold prompts and absolute
  paths and change every session. Fix: `git rm --cached` and add `.claude/atlas/` to `.gitignore`.

## Fixed since A1

All seven were checked again in the re-audit, and A1-24 and A1-25 were also checked in the dev copy. None introduced a new
problem.

| ID | What changed |
|---|---|
| A1-18-hotkey-helper-held-modifiers | `press-hotkey.ps1` checks the key first and always releases Ctrl, Alt and Shift |
| A1-21-zero-caller-api | 9 unused members removed (for example `Kit.Switch`, `Palette.Brush`, `Store.Folder`) |
| A1-22-json-options-per-command | `/json` reuses one options object |
| A1-23-leftover-icon-and-fence-state | Icon Lab's `Snap` toggle and a dead fence variable removed |
| A1-24-disabled-caret-still-ticks | With blinking off, the caret timer stops; confirmed solid in 6 of 6 dev captures |
| A1-25-tray-empty-title-sentinel | Tray slots use an explicit "occupied" flag; you confirmed a note titled "empty" is clickable |
| A1-26-worker-effort-mismatch | Codex workers run luna xhigh everywhere, matching AGENTS.md |
| A1-12-settings-search-materializes-all | Settings search builds only matching rows; rows create their controls lazily, so the page methods stay the one definition (A27-settings-search-build-matches, 2026-10-24, user-tested live) |
| A1-7-stale-fence-cache | The code-block color cache is pruned to current fences after each structure update (A28-editor-render-memory, 2026-10-25, user-tested live) |
| A1-9-fence-copy-per-colored-line | An unchanged code block's text is copied once, keyed by document version and body bounds (A28) |
| A1-10-render-path-allocation | Chips, timer icons, chevrons, glyph icons, edge fade, paper, dividers and checkboxes reuse bounded per-view brushes, pens, text and geometry (A28) |
| A1-8-whole-document-analysis | After an edit only the changed lines (plus a neighbor each side) are re-analysed; structural changes fall back to a full rebuild (A29-incremental-analysis, 2026-10-25, user-tested live) |
| A1-14-synchronous-save-and-log | Autosave, hide and logging write on background queues; quit, retry, session end and folder moves still save synchronously (A30-background-save-and-log, 2026-10-25, user-tested live) |

## Fixed in A6 (your decisions)

| ID | What changed | Task |
|---|---|---|
| A1-1-recovery-deletes-bodies | Unknown note files move to `recovered\` and are never deleted. Removed notes delete only their own file. A load from the backup list deletes nothing. | A7 |
| A5-1-body-read-failure-overwrite | A missing or unreadable note opens read-only with a hint pointing to the recovered folder, and is never saved over | A7 |
| A5-2-invalid-index-bypasses-backup | An incomplete `index.json` falls back to `index.json.bak` | A7 |
| Decision 1: notes folder | Notes live in `Documents\AntiNotepad` by default. Settings › About shows the folder with Open, Change… (a verified move that keeps a backup) and Open recovered. Log and folder pointer stay in `%LOCALAPPDATA%\AntiNotepad`. | A7 |
| A1-2-quit-after-save-failure | A failed save on quit shows "Couldn't save your notes" with Retry and Quit anyway. `--quit` does not exit. | A8 |
| A1-20-cross-platform-payload | The installer publishes `win-x64`: no Linux or macOS files | A8 |
| A1-4, A1-5, A1-6 | List numbers above 9 digits stop auto-numbering; timers above 99 days or with non-ASCII digits aren't timers; calculations deeper than 64 levels are invalid instead of crashing | A9 |
| A1-19-mixed-currency-addition | `$5 + €3` is an error | A9 |
| A1-3, A1-16 | Settings preview timers stop on close. Editing a timer's label keeps it running. Deleting its line or note stops it. | A9 |
| A5-3 and the `/ocr` repeat bug | A second `/ocr` starts a new snip. The text lands in the note you started from. | A10 |
| A1-27-tracked-session-snapshots | `.claude/atlas/` untracked and ignored | A6 |

Checked: the merged branch builds with 0 warnings and passes the selftest (two new check groups: notes location and recovery,
input limits). In the dev copy: unknown file moved to `recovered\`; the unreadable-note hint shows; a read-only `index.json`
plus `--quit` showed the save prompt and the app stayed open.

## Evidence

| Check | Result |
|---|---|
| Release build | 0 warnings, 0 errors |
| `--selftest` | SELFTEST PASS |
| Analyzers (`latest-recommended`) | 50 suggestions, down from 53: CA1859 ×18 (return types), CA1305 ×17 and CA1310 ×8 (culture), CA1822 ×3, and one each of CA1862, CA1852, CA1720, CA1001. The culture warnings that matter are in A1-4 and A1-5; the rest are tidy-up. |
| Packages | No top-level updates. Nothing vulnerable or deprecated. AvaloniaEdit 12.0.0 is the newest release, so its split from Avalonia 12.1.4 is only something to watch. SkiaSharp and HarfBuzz versions are chosen by Avalonia; don't bump them separately. |
| Dependencies | All three packages and the bundled Fluent icon font (used by Settings, MIT licence shipped) are in use. Nothing is replaceable by a first-party option. |
| Settings memory (dev copy, 5 open/close cycles) | Window freed 5 of 5 times. Managed heap back to 3 MB after each close. Private memory went from 122 MB to a flat 157 MB: a one-time native cost, not growth. About 90 threads throughout, which is worth a look later. |

## How this audit was done

- **A1:** a Codex director with three luna xhigh readers covered every tracked file. The Claude session collected the
  build, analyzer, package, selftest and memory evidence, opened every High finding in the code, and moved A1-7 down to
  Medium.
- **A5:** the same setup re-checked all 27 findings at their current lines, verified the 7 fixes, and swept every file again,
  which produced A5-1 to A5-5. The Claude session checked each new finding against the code.
- **Not tested:** nothing was crash-tested or fault-injected (disk full, locked files, races), and no live performance
  numbers were taken. Unused-code searches are text searches, not compiler-proven.

## Appendix: file checklist

Every tracked file was reviewed. A dash means nothing to report.

| File | Findings | Note |
|---|---|---|
| `.claude/atlas/0fc364ed-0bfc-4b86-b14a-cdf5aec44d80.json` | A1-27-tracked-session-snapshots | Generated conversation state remains tracked. |
| `.claude/atlas/2f45f362-d9fb-4b01-818e-4bd362de7768.json` | A1-27-tracked-session-snapshots | Generated conversation state remains tracked. |
| `.claude/atlas/4f4ae1d6-e998-4eb9-a1be-e100a230655b.json` | A1-27-tracked-session-snapshots | Generated conversation state remains tracked. |
| `.claude/atlas/966c1dc8-0742-4d57-af39-7b41c8396fd3.json` | A1-27-tracked-session-snapshots | Generated conversation state remains tracked. |
| `.coord/STATUS.md` | - | Current dispatch and prior gates read. |
| `.coord/reports/A1-codebase-audit.director.codex.md` | - | Historical report; claims treated as prior evidence. |
| `.coord/reports/A1-codebase-audit.md` | - | Historical report; claims treated as prior evidence. |
| `.coord/reports/A2-dead-code-tidy.md` | A1-21-zero-caller-api, A1-23-leftover-icon-and-fence-state | Historical fix report checked against final source. |
| `.coord/reports/A2-dead-code-tidy.worker.codex.md` | A1-21-zero-caller-api, A1-23-leftover-icon-and-fence-state | Historical fix report checked against final source. |
| `.coord/reports/A3-runtime-tidy.md` | A1-22-json-options-per-command, A1-24-disabled-caret-still-ticks, A1-25-tray-empty-title-sentinel | Historical fix report checked against final source. |
| `.coord/reports/A3-runtime-tidy.worker.codex.md` | A1-22-json-options-per-command, A1-24-disabled-caret-still-ticks, A1-25-tray-empty-title-sentinel | Historical fix report checked against final source. |
| `.coord/reports/A4-tooling-tidy.md` | A1-18-hotkey-helper-held-modifiers, A1-26-worker-effort-mismatch | Historical fix report checked against final source. |
| `.coord/reports/A4-tooling-tidy.worker.codex.md` | A1-18-hotkey-helper-held-modifiers, A1-26-worker-effort-mismatch | Historical fix report checked against final source. |
| `.coord/reports/I6-win11-menu-toolbar-in-app.md` | - | Historical report; claims treated as prior evidence. |
| `.coord/reports/I6-win11-menu-toolbar-in-app.worker.codex.md` | - | Historical report; claims treated as prior evidence. |
| `.coord/reports/I7-menu-corner-artifacts.md` | - | Historical report; claims treated as prior evidence. |
| `.coord/reports/I7-menu-corner-artifacts.worker.codex.md` | - | Historical report; claims treated as prior evidence. |
| `.coord/reports/I7b-menu-corners-dwm.backup.codex.md` | - | Historical report; claims treated as prior evidence. |
| `.coord/reports/I7b-menu-corners-dwm.md` | - | Historical report; claims treated as prior evidence. |
| `.coord/reports/N1-antinotepad-identity.director.codex.md` | - | Historical report; claims treated as prior evidence. |
| `.coord/reports/N1-antinotepad-identity.md` | - | Historical report; claims treated as prior evidence. |
| `.coord/reports/N2-canonical-install-and-dev-badge.director.codex.md` | - | Historical report; claims treated as prior evidence. |
| `.coord/reports/N2-canonical-install-and-dev-badge.md` | - | Historical report; claims treated as prior evidence. |
| `.coord/tasks/A1-codebase-audit.md` | - | Historical brief; current scope and references checked. |
| `.coord/tasks/A2-dead-code-tidy.md` | A1-21-zero-caller-api, A1-23-leftover-icon-and-fence-state | Historical fix brief matched to final source. |
| `.coord/tasks/A3-runtime-tidy.md` | A1-22-json-options-per-command, A1-24-disabled-caret-still-ticks, A1-25-tray-empty-title-sentinel | Historical fix brief matched to final source. |
| `.coord/tasks/A4-tooling-tidy.md` | A1-18-hotkey-helper-held-modifiers, A1-26-worker-effort-mismatch | Historical fix brief matched to final source. |
| `.coord/tasks/I6-win11-menu-toolbar-in-app.md` | - | Historical brief; current scope and references checked. |
| `.coord/tasks/I7-menu-corner-artifacts.md` | - | Historical brief; current scope and references checked. |
| `.coord/tasks/I7b-menu-corners-dwm.md` | - | Historical brief; current scope and references checked. |
| `.coord/tasks/N1-antinotepad-identity.md` | - | Historical brief; current scope and references checked. |
| `.coord/tasks/N2-canonical-install-and-dev-badge.md` | - | Historical brief; current scope and references checked. |
| `.coord/tasks/N3-release-and-update-plan.md` | - | Historical brief; current scope and references checked. |
| `.coord/tools/codex-run.sh` | A1-26-worker-effort-mismatch, A5-4-worker-output-slug-unchecked | Effort aligned; output task slug remains unchecked. |
| `.gitattributes` | - | Shell scripts enforce LF. |
| `.gitignore` | A1-27-tracked-session-snapshots | Build outputs ignored; Atlas state remains tracked. |
| `AGENTS.md` | A1-26-worker-effort-mismatch | Worker routing agrees with current policy. |
| `CLAUDE.md` | - | Delegates to AGENTS instructions. |
| `docs/agents.md` | A1-26-worker-effort-mismatch | Worker effort is consistently xhigh. |
| `docs/audit/A1-codebase-audit.md` | - | All prior findings reconciled against current source. |
| `docs/decisions.md` | - | Locked choices respected; retired context identified. |
| `docs/dev-loop.md` | - | Installed and dev isolation documented. |
| `product/AntiNotepad/AntiNotepad.csproj` | A1-20-cross-platform-payload | Dependency, resource and license references checked. |
| `product/AntiNotepad/AppInfo.cs` | - | Generated identity and resource consumers checked. |
| `product/AntiNotepad/AppInfo.props` | - | Single product identity source reviewed. |
| `product/AntiNotepad/Assets/Fonts/FluentSystemIcons-LICENSE.txt` | - | Microsoft MIT notice present and referenced. |
| `product/AntiNotepad/Assets/Fonts/FluentSystemIcons-Regular.ttf` | - | Binary size and Settings font reference checked. |
| `product/AntiNotepad/Calc/Calculator.cs` | A1-6-calculator-unbounded-recursion, A1-19-mixed-currency-addition | Parser depth and currency arithmetic remain open. |
| `product/AntiNotepad/Editor/ChevronGutter.cs` | A1-10-render-path-allocation | Drawing allocations and visible-line searches remain. |
| `product/AntiNotepad/Editor/Colorizers.cs` | A1-10-render-path-allocation | Per-span brushes remain in colorization. |
| `product/AntiNotepad/Editor/Completion.cs` | A1-22-json-options-per-command | Shared JSON formatting options verified. |
| `product/AntiNotepad/Editor/Elements.cs` | - | Generators and visible text spans reviewed. |
| `product/AntiNotepad/Editor/Motion.cs` | A1-24-disabled-caret-still-ticks | Blink timer follows focus and current setting. |
| `product/AntiNotepad/Editor/PadView.cs` | A1-3-preview-timer-retains-settings, A1-4-list-marker-overflow, A1-5-timer-parser-can-throw, A1-7-stale-fence-cache, A1-8-whole-document-analysis, A1-10-render-path-allocation, A1-16-timer-history-never-pruned, A1-24-disabled-caret-still-ticks, A5-1-body-read-failure-overwrite | Binding, analysis, lists, timers and lifecycle reviewed. |
| `product/AntiNotepad/Editor/Renderers.cs` | A1-10-render-path-allocation | Visible chips allocate drawing resources. |
| `product/AntiNotepad/Editor/Structure.cs` | A1-7-stale-fence-cache, A1-8-whole-document-analysis, A1-9-fence-copy-per-colored-line, A1-23-leftover-icon-and-fence-state | Cache, folds and simplified fence state reviewed. |
| `product/AntiNotepad/Editor/TextCommands.cs` | - | Transform commands and callers reviewed. |
| `product/AntiNotepad/Model/Modes.cs` | A1-21-zero-caller-api | Duplicate unused language list removed. |
| `product/AntiNotepad/Model/Showcase.cs` | - | Development seed and user command remain intentional. |
| `product/AntiNotepad/Model/Store.cs` | A1-1-recovery-deletes-bodies, A1-11-synchronous-note-search, A1-14-synchronous-save-and-log, A1-21-zero-caller-api, A5-1-body-read-failure-overwrite, A5-2-invalid-index-bypasses-backup | Recovery, storage, metadata and removed APIs checked. |
| `product/AntiNotepad/Model/Timers.cs` | A1-3-preview-timer-retains-settings, A1-5-timer-parser-can-throw, A1-16-timer-history-never-pruned | Parsing, private ownership and retained history reviewed. |
| `product/AntiNotepad/Model/Welcome.cs` | - | First-run seeding checks existing notes. |
| `product/AntiNotepad/Platform/DefaultDataFolderMigration.cs` | - | Legacy move, fallback and custom-path isolation reviewed. |
| `product/AntiNotepad/Platform/GlobalHotkey.cs` | - | Registration, parsing and handle ownership reviewed. |
| `product/AntiNotepad/Platform/Native.cs` | - | Clipboard, startup, hooks and Win32 ownership reviewed. |
| `product/AntiNotepad/Platform/Ocr.cs` | A5-3-ocr-result-follows-rebound-pane | Local recognition and resource disposal reviewed. |
| `product/AntiNotepad/Platform/SingleInstance.cs` | A1-15-startup-quit-is-dropped, A1-21-zero-caller-api | Quit signaling and removed state reviewed. |
| `product/AntiNotepad/Platform/Tray.cs` | A1-25-tray-empty-title-sentinel | Explicit slot occupancy replaces title sentinel. |
| `product/AntiNotepad/Program.cs` | A1-14-synchronous-save-and-log, A1-15-startup-quit-is-dropped, A1-25-tray-empty-title-sentinel | Startup, logging and tray state wiring reviewed. |
| `product/AntiNotepad/SelfTest.cs` | - | Existing capability checks read; not rerun. |
| `product/AntiNotepad/Theme/Palette.cs` | A1-21-zero-caller-api | Palette consumers and removed helper methods checked. |
| `product/AntiNotepad/UI/Anim.cs` | A1-21-zero-caller-api | Unused timer helper removed; frame helper reviewed. |
| `product/AntiNotepad/UI/HelpOverlay.cs` | - | Shortcut sheet and close callback reviewed. |
| `product/AntiNotepad/UI/Kit.cs` | A1-10-render-path-allocation, A1-21-zero-caller-api, A1-23-leftover-icon-and-fence-state | Unused controls removed; glyph snapping preserved. |
| `product/AntiNotepad/UI/MotionPreview.cs` | A1-3-preview-timer-retains-settings, A1-13-progress-swipe-close-retention | Preview countdown and swipe cleanup remain open. |
| `product/AntiNotepad/UI/NoteSwipe.cs` | A1-13-progress-swipe-close-retention | Progress timer lacks explicit release cancellation. |
| `product/AntiNotepad/UI/PadMenu.cs` | - | Popup, focus and native hook cleanup reviewed. |
| `product/AntiNotepad/UI/PadWindow.Keys.cs` | - | Keys, Settings references and trim lifecycle reviewed. |
| `product/AntiNotepad/UI/PadWindow.Layout.cs` | - | Chrome, pager, animations and toast flow reviewed. |
| `product/AntiNotepad/UI/PadWindow.Navigation.cs` | A5-3-ocr-result-follows-rebound-pane | Navigation, clipboard capture and OCR reviewed. |
| `product/AntiNotepad/UI/PadWindow.cs` | A1-2-quit-after-save-failure, A1-14-synchronous-save-and-log | Failed-save quit and synchronous save remain. |
| `product/AntiNotepad/UI/SearchOverlay.cs` | A1-11-synchronous-note-search | Search rereads bodies and rebuilds results. |
| `product/AntiNotepad/UI/SettingsParts.cs` | - | Control builders and popup lifetime reviewed. |
| `product/AntiNotepad/UI/SettingsWindow.cs` | A1-3-preview-timer-retains-settings, A1-12-settings-search-materializes-all | Search builds every section before filtering. |
| `product/AntiNotepad/antinotepad.ico` | - | Binary metadata and executable/tray references checked. |
| `tools/dev.ps1` | - | Owned process and bounded cleanup guards reviewed. |
| `tools/dev.sh` | - | Shell wrapper delegates to PowerShell implementation. |
| `tools/install.ps1` | A1-2-quit-after-save-failure, A1-17-upgrade-has-no-rollback, A1-20-cross-platform-payload | Quit gate, overlay copy and publish reviewed. |
| `tools/make-icon.ps1` | - | Local icon generation and drawing disposal reviewed. |
| `tools/press-hotkey.ps1` | A1-18-hotkey-helper-held-modifiers, A5-5-hotkey-case-depends-on-language | Cleanup fixed; uppercase conversion still depends on language. |
| `tools/uninstall.ps1` | A1-2-quit-after-save-failure | Bounded removal guards; save outcome depends on quit. |
