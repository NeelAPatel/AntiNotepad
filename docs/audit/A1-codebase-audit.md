# A1-codebase-audit

Date: 2026-10-23 (America/New_York)  
Branch: `feat/A1-codebase-audit`  
Commit: `ce7588e`  
Scope: all 78 tracked files, in the order of `.coord/scratch/A1/files.txt`; the current working tree, including preexisting manager changes. No fixes.

Method: read the required guidance first, then delegate Editor, UI, and Model/Calc/Theme/Platform/root-resource reading to three `gpt-6-luna xhigh` workers. The Editor worker additionally reviewed every tooling script and ignore/attribute file. Director read the repo/tooling/history, verified key source paths, reconciled severities, and merged findings. Repo-wide reference searches checked types, public methods and Settings properties; these are lexical call-site checks, not compiler-proven reachability. Binary review covered metadata/size, references and licence shipping, not visual/font rendering. No build, selftest, app launch/quit/kill, installer, registry action, network research, branch switch, merge or commit was performed.

The exact 78-path list matches readable `git ls-files`. Only the deliverable and short report are authorized writes; `.coord/STATUS.md` is intentionally untouched despite the general dispatch rule. Findings are source-based risks unless stated otherwise; no performance timing or runtime leak reproduction is claimed. High means a concrete data-loss/crash/retention path, Medium a concrete risk or wasted work, Low cleanup. A conditional trigger does not imply it occurred in the manager's run.

## Measured evidence

These are the manager's already-collected inputs, quoted without rerunning:

| Input | Supplied result |
|---|---|
| `.coord/scratch/A1/build-release.txt` | `dotnet build -c Release`: `0 Warning(s)`, `0 Error(s)`. |
| `.coord/scratch/A1/analyzers-dedup.txt` | Release, `AnalysisLevel=latest-recommended`: **53** deduplicated warnings. |
| `.coord/scratch/A1/packages.txt` | No top-level updates; no vulnerable or deprecated packages, including transitives, according to that collected scan. |
| `.coord/scratch/A1/selftest.txt` | `SELFTEST PASS`, `exit=0`. |
| `.coord/scratch/A1/files.txt` | 78 tracked paths; checklist below preserves their order. |

| Analyzer rule | Count | Audit disposition |
|---|---:|---|
| CA1305 | 17 | Examined numeric boundaries seriously. List markers/timers have real unchecked parsing failures (A1-4-list-marker-overflow, A1-5-timer-parser-can-throw). Calculator parsing/formatting explicitly uses InvariantCulture (Calculator.cs:22,327-350,536). Settings integer choices at SettingsWindow.cs:319,352,366,416,457 come from fixed small literal values; double choices use invariant Num at :542 and Closest at :545. Prefer explicit providers for syntax, but these fixed choices do not justify separate crash findings. Kit.Ago (:224-227) uses current culture for human-facing dates, not persisted numeric syntax. |
| CA1310 | 8 | Primarily ASCII syntax prefixes (fences, links, headings); use ordinal comparisons when touched. No demonstrated locale-specific numeric corruption established from these warnings. |
| CA1859 | 18 | Concrete-return-type suggestions; low priority, no measured user impact. Grouped here rather than 18 findings. |
| CA1822 | 5 | Static-member suggestions; remove the truly unused Palette helpers first (A1-21-zero-caller-api). |
| CA1869 | 1 | New JSON options per command: A1-22-json-options-per-command. |
| CA1001 | 1 | Inspected App._tray (Program.cs:99). Normal Quit explicitly disposes it at Program.cs:114, calling Tray.Dispose (:49). App need not be declared a leak solely for lacking IDisposable; abnormal startup/OS termination disposal was not tested. |
| CA1862 / CA1852 / CA1720 | 1 each | SelfTest case comparison, sealing GlyphElement, Palette.String naming: tidy-up only; no standalone finding. |

AvaloniaEdit **12.0.0 is the newest published version in the supplied evidence**. The Avalonia 12.1.4 / AvaloniaEdit 12.0.0 split is a compatibility watch item, not an upgrade task. SkiaSharp/HarfBuzz versions are selected by Avalonia; do not independently bump their “latest” versions from the outdated list. The collected scan also names newer MicroCom.Runtime/Tmds.DBus.Protocol transitives; no direct override is justified by this audit.

Read-only inspection of the existing manager output `build/a1-check` found Linux and macOS native payloads, Windows x86/x64/arm64 payloads, and the font licence. WebAssembly packages appear in the dependency scan, but no actual WebAssembly/browser files were found in that output. This is existing Release-output inspection, not a fresh publish or installed-copy inventory.

## Summary table

| Category | High | Medium | Low | Total |
|---|---:|---:|---:|---:|
| Code debt | 0 | 0 | 2 | 2 |
| Dependencies | 0 | 0 | 1 | 1 |
| Inefficiency | 1 | 9 | 2 | 12 |
| Correctness | 5 | 4 | 1 | 10 |
| Repo hygiene | 0 | 0 | 2 | 2 |
| **Total** | **6** | **13** | **8** | **27** |

## Manager verification (Claude session)

- **Spot-check:** every High finding (A1-1 to A1-7) and the Mediums A1-13, A1-15 and A1-16 were opened at their file:line, and the
  quoted code matches. Checklist: 78 rows, same paths and order as `git ls-files`.
- **Severity change:** A1-7-stale-fence-cache moves from High to **Medium**. Stale keys are opening-line numbers, so the cache is
  bounded by the note's line count (one entry per line a fence once opened on). It is waste, not unbounded growth.
- **Aggravating fact for A1-4, A1-5 and A1-6:** `Program.cs:31` logs Dispatcher exceptions but never sets `Handled`, so an
  exception in analysis ends the process. The 400 ms autosave stores the offending text first. A note holding it can then
  crash the app again whenever it is analysed (a "poison note"). Not reproduced.
- **Memory, measured** (dev copy, Debug, `ANTINOTEPAD_DEBUG_SETTINGS_CYCLES=5`, Feel page with the live preview, log kept at
  `.coord/scratch/A1/dev-settings-cycles.log`, untracked):
  - `freed=True` in **5 of 5** cycles. Managed heap 7.3 MB at start, about 18 MB with Settings open, and 3.0 MB after each close.
  - Private bytes: 122 MB before the first open, then flat at 154-162 MB after every close. There is no growth per cycle.
  - Working set after trim: 4-58 MB. Threads: 89-91 throughout.
  - Reading: in the ordinary open/close path there is no managed leak. The residual of about 35 MB private is a one-time native
    cost (Skia, composition and font caches), not a retained window. The `freed=False` the user saw needs a trigger. The two
    concrete candidates are A1-3 (a countdown started in the preview) and A1-13 (closing during a swipe payout). The thread count,
    about 90 for a tray app, is worth a separate look; it was not analysed here.

## Findings

### A1-1-recovery-deletes-bodies

- **Category:** Correctness. **Severity:** High.
- **Location / evidence:** `product/AntiNotepad/Model/Store.cs:197-199`: `Normalize(); RemoveOrphans();`; `:246`: `if (!live.Contains(...)) File.Delete(file);`; `:260-263`: body `Atomic(...)` precedes index `Atomic(...)`.
- **Impact / trigger:** If both index candidates are missing/corrupt, default metadata names none of the existing bodies and startup deletes them. Recovering an older backup deletes newer notes' bodies. A crash after writing a new body but before committing its index entry also makes that body an orphan on the next successful load. Individual atomic replacements do not make the multi-file save transactional.
- **Suggested fix:** Preserve/quarantine unreferenced bodies; disable destructive cleanup in recovery, and commit/recover body/index generations together. Retain enough generations to recover from the backup index.
- **Effort:** M. **Safe to auto-fix?** No — recovery and retention policy must be chosen.

### A1-2-quit-after-save-failure

- **Category:** Correctness. **Severity:** High.
- **Location / evidence:** `product/AntiNotepad/UI/PadWindow.cs:176-183`: `Save(); ... Close(); QuitRequested?.Invoke();`; `:220`: `if (!_store.Save()) { Toast(...); return; }`.
- **Impact / trigger:** A disk-full, permissions or I/O failure only produces a toast; Quit still destroys the process and unsaved edits. The same behavior lets install/uninstall's graceful `--quit` report process exit despite a failed save.
- **Suggested fix:** Return save success, refuse normal shutdown on failure, and make quit failure observable to the requesting process; offer retry/explicit discard behavior.
- **Effort:** M. **Safe to auto-fix?** No — requires a failure UX and quit protocol decision.

### A1-3-preview-timer-retains-settings

- **Category:** Inefficiency. **Severity:** High.
- **Location / evidence:** `product/AntiNotepad/UI/MotionPreview.cs:108`: `new PadView(..., new Timers())`; `product/AntiNotepad/Editor/PadView.cs:136`: `_timers.Tick += () => ... view.InvalidateLayer(...)`; `:295-298`: release stops analysis/caret only. `product/AntiNotepad/Model/Timers.cs:85`: active entries keep `_tick` started; `product/AntiNotepad/UI/SettingsWindow.cs:71`: close calls `_preview.Release()`.
- **Impact / trigger:** Concrete conditional retention: type a timer note in the editable preview, start a long countdown, then close Settings. DispatcherTimer → Timers → Tick subscriber → preview PadView/TextView → visual parents can keep the closed Settings graph alive until completion, ticking every 250 ms. This proves a retention path, not that it caused the manager's observed `freed=False`.
- **Suggested fix:** Give the privately owned preview Timers a stop/dispose path, and unsubscribe PadView's handler on release. Do not stop the resident pad's shared timers accidentally.
- **Effort:** M. **Safe to auto-fix?** No — distinguish private ownership from shared timer lifetime.

### A1-4-list-marker-overflow

- **Category:** Correctness. **Severity:** High.
- **Location / evidence:** `product/AntiNotepad/Editor/PadView.cs:596`: `var own = int.Parse(m.Groups[3].Value);`; `:631`: `int.Parse(m.Groups[3].Value) + 1`; Enter/Tab dispatch at `:501-512`.
- **Impact / trigger:** A numbered marker longer than Int32 can throw on Enter/Tab without a catch. Int32.MaxValue also wraps on increment under the default unchecked arithmetic. The regex accepts arbitrary digit counts, so pasted note text reaches this path.
- **Suggested fix:** Use invariant bounded TryParse and overflow-safe increment; preserve the original marker or decline continuation when out of range.
- **Effort:** S. **Safe to auto-fix?** No — choose out-of-range list behavior.

### A1-5-timer-parser-can-throw

- **Category:** Correctness. **Severity:** High.
- **Location / evidence:** `product/AntiNotepad/Model/Timers.cs:22-23`: regexes accept `\d` and unbounded duration digits; `:40-41`: `int.Parse(...)`; `:51-57`: `double.Parse(..., CultureInfo.InvariantCulture)` then `TimeSpan.FromSeconds(total)`; `product/AntiNotepad/Editor/PadView.cs:351-353` calls it during analysis.
- **Impact / trigger:** Typing/pasting `999999999999999999999h` in a timer note can overflow TimeSpan; Unicode digits matched by `\d` may fail numeric parsing. `TryParse` does not actually contain exceptions. A representable TimeSpan can also exceed DateTime's range when starting at Timers.cs:75/81. These are unhandled UI-path crash risks.
- **Suggested fix:** Restrict grammar to intended digits, use invariant TryParse, reject nonfinite/out-of-range totals, and validate duration against both TimeSpan and DateTime limits.
- **Effort:** S. **Safe to auto-fix?** No — define valid duration limits and invalid-input behavior.

### A1-6-calculator-unbounded-recursion

- **Category:** Correctness. **Severity:** High.
- **Location / evidence:** `product/AntiNotepad/Calc/Calculator.cs:682-683`: unary signs recurse via `ParseUnary()`; `:732-739`: parentheses call `ParseExpr()` recursively; `:1049` catches `Exception`.
- **Impact / trigger:** A sufficiently deeply nested or unary-prefixed pasted math expression can exhaust the process stack. The ordinary exception catch cannot recover a stack overflow; normal edit analysis evaluates the input. This is a static crash risk, not a reproduced crash.
- **Suggested fix:** Enforce a parser nesting/work limit with a normal invalid-expression result, or use an explicit parse stack.
- **Effort:** M. **Safe to auto-fix?** No — parser limits need a chosen contract.

### A1-7-stale-fence-cache

- **Category:** Inefficiency. **Severity:** Medium (manager: was High; bounded by line count, see Manager verification).
- **Location / evidence:** `product/AntiNotepad/Editor/Structure.cs:177`: `Dictionary<int, Cache> _cache` keyed by opening line; `:215-216`: stores full TextDocument/highlighter/text entries. `product/AntiNotepad/Editor/PadView.cs:252` resets only on Bind.
- **Impact / trigger:** Moving/deleting fences while editing the same bound note never prunes obsolete opening-line keys. Repeatedly shifting a long code block retains old body strings, documents and highlighters until rebinding; growth is unbounded within that editing session.
- **Suggested fix:** Prune stale keys after each structure update and dispose/release obsolete highlighters as appropriate; prefer document-version/fence identity keys.
- **Effort:** M. **Safe to auto-fix?** No — coordinate cache invalidation with coloring lifecycle.

### A1-8-whole-document-analysis

- **Category:** Inefficiency. **Severity:** Medium.
- **Location / evidence:** `product/AntiNotepad/Editor/PadView.cs:328-345`: `else Analyse()` and copies every line into `lines`; `product/AntiNotepad/Editor/Structure.cs:43-44` scans both directions per blank line; `:83-94` scans descendants per indent candidate.
- **Impact / trigger:** Every edit at up to 1500 lines recomputes the entire structure, folds and math; larger notes defer the same work by 140 ms. Long blank runs/deep indentation have quadratic work. Flat, very large notes can still be costly below the line-count threshold. Visible slowdown was not measured.
- **Suggested fix:** Compute blank-neighbor/indent ranges in linear passes, cache structure by changes, and budget analysis by text size/work as well as line count.
- **Effort:** L. **Safe to auto-fix?** No — preserve folding and dependent-math semantics.

### A1-9-fence-copy-per-colored-line

- **Category:** Inefficiency. **Severity:** Medium.
- **Location / evidence:** `product/AntiNotepad/Editor/Structure.cs:187`: `Get(fence)` per colored line; `:210-211`: `var text = doc.GetText(from, to - from);` occurs before the cache check.
- **Impact / trigger:** Repainting visible lines inside a large fence repeatedly allocates and compares the complete fence body even on cache hits; work scales with fence size times rendered lines.
- **Suggested fix:** Validate body/version once per edit or coloring pass and reuse the cached inner document for its lines.
- **Effort:** M. **Safe to auto-fix?** No — requires correct edit/version invalidation.

### A1-10-render-path-allocation

- **Category:** Inefficiency. **Severity:** Medium.
- **Location / evidence:** `product/AntiNotepad/Editor/Renderers.cs:111-121`: new list, `FormattedText` and `SolidColorBrush` per visible chip; `product/AntiNotepad/Editor/ChevronGutter.cs:45,61-90`: invalidates on moves, creates pens and scans VisualLines per fold; `product/AntiNotepad/Editor/PadView.cs:279-287`: new gradient/stops; `product/AntiNotepad/Editor/Colorizers.cs:116-124`: segment brushes; `product/AntiNotepad/UI/Kit.cs:42`: text/brush per glyph render.
- **Impact / trigger:** Scrolling, pointer movement and active timer redraws create avoidable allocation churn and fold × visible-line lookup work. Timer chip redraws occur every 250 ms (Timers.cs:26, PadView.cs:136). No FPS/allocation measurements were run.
- **Suggested fix:** Cache palette/style drawing resources and formatted text where stable, update existing fade stops, index visible folds, and invalidate only on meaningful changes.
- **Effort:** M. **Safe to auto-fix?** No — cache keys must include palette, font, DPI, text and state.

### A1-11-synchronous-note-search

- **Category:** Inefficiency. **Severity:** Medium.
- **Location / evidence:** `product/AntiNotepad/UI/SearchOverlay.cs:43`: TextChanged calls `Refresh()`; `:79`: `terms.All(t => n.PeekText().Contains(...))`; `:104,215`: reads again for cards. `product/AntiNotepad/Model/Store.cs:180`: `File.ReadAllText(path)`.
- **Impact / trigger:** Every query keystroke can reread every unloaded note once per term, then read matching notes again and build every result control on the UI thread. Large collections/notes or slow storage provide a concrete blocking trigger.
- **Suggested fix:** Debounce/cancel queries, load each body once per query, use a bounded search cache/index and background I/O with UI result publication.
- **Effort:** M. **Safe to auto-fix?** No — coordinate query ordering, mutations and cache invalidation.

### A1-12-settings-search-materializes-all

- **Category:** Inefficiency. **Severity:** Medium.
- **Location / evidence:** `product/AntiNotepad/UI/SettingsWindow.cs:165-168`: query change calls ShowResults; `:254-260`: loops all Sections, `BuildSection(id, withPreview: false)` before filtering rows.
- **Impact / trigger:** Every character constructs all six Settings sections and their icon/choice illustration controls, excluding MotionPreview, then discards unmatched controls. Search is a concrete repeated allocation path.
- **Suggested fix:** Represent searchable rows as descriptors and filter before constructing controls; debounce if still necessary.
- **Effort:** M. **Safe to auto-fix?** No — preserve live callbacks and row grouping.

### A1-13-progress-swipe-close-retention

- **Category:** Inefficiency. **Severity:** Medium.
- **Location / evidence:** `product/AntiNotepad/UI/NoteSwipe.cs:131`: `if (Math.Abs(_owed) > 0.5) return;` precedes `timer.Stop()`; `:90-100` acknowledges hidden windows receive no frames. `product/AntiNotepad/UI/MotionPreview.cs:182-185` releases editor but not swipe timers.
- **Impact / trigger:** Suspected persistent root: close Settings during a progress-wheel payout; if the queued animation frame is no longer delivered, `_owed` remains and the 260 ms dispatcher settle timer repeats forever, retaining swipe delegates and preview. Closed-TopLevel RAF disposal behavior is unverified.
- **Suggested fix:** Add explicit NoteSwipe cancellation/disposal on preview release/detach; clear owed state and stop both settle timers.
- **Effort:** S. **Safe to auto-fix?** No — verify cancel/detach/reopen behavior.

### A1-14-synchronous-save-and-log

- **Category:** Inefficiency. **Severity:** Medium.
- **Location / evidence:** `product/AntiNotepad/UI/PadWindow.cs:60`: dispatcher save callback; `product/AntiNotepad/Model/Store.cs:255-274`: serialize, `File.WriteAllText`, `File.Replace` synchronously; `product/AntiNotepad/Program.cs:84-85`: synchronous log inspection/append.
- **Impact / trigger:** After the 400 ms typing pause, on blur/hide and Quit, dirty bodies and the index are written on the UI thread. Large notes/slow or contended storage can stall input/show-hide; timing logs also perform synchronous I/O on interaction paths. No timings measured.
- **Suggested fix:** Capture consistent immutable save snapshots and serialize writes off the UI thread, retaining dirty/error state and awaiting the final save; buffer diagnostic logging.
- **Effort:** L. **Safe to auto-fix?** No — ordering, retries and shutdown durability need design.

### A1-15-startup-quit-is-dropped

- **Category:** Correctness. **Severity:** Medium.
- **Location / evidence:** `product/AntiNotepad/Program.cs:72-75`: quit posts only `if (_window is { } w)`; `:90-94` later attaches the window and replays pending Show only. `product/AntiNotepad/Platform/SingleInstance.cs:104-105` signals once and waits up to five seconds.
- **Impact / trigger:** A `--quit` received during migration/store loading/Avalonia startup is consumed while `_window` is null. The resident then continues and the requester times out; install/uninstall safely abort, but shutdown is unreliable during startup.
- **Suggested fix:** Latch a pending quit, honor it when attached or abort initialization safely; acknowledge save/shutdown result to clients.
- **Effort:** M. **Safe to auto-fix?** No — synchronize lifetime states and save outcome.

### A1-16-timer-history-never-pruned

- **Category:** Inefficiency. **Severity:** Medium.
- **Location / evidence:** `product/AntiNotepad/Model/Timers.cs:25`: `_entries` dictionary; `:64-69` only adds/replaces; `:89` scans all values on every tick. `product/AntiNotepad/Editor/PadView.cs:411`: key includes note id and `line.Trim()`.
- **Impact / trigger:** Editing timer text creates new keys; completed/deleted/changed entries have no removal path. A long resident session accumulates history and every active 250 ms tick scans it all. Whether a running countdown should survive a line edit is a product choice, so stale alarms are not asserted as a separate bug.
- **Suggested fix:** Keep running timers according to a documented policy, prune inactive unreferenced entries and remove timers on final owner disposal.
- **Effort:** M. **Safe to auto-fix?** No — choose behavior for edited/deleted running timers.

### A1-17-upgrade-has-no-rollback

- **Category:** Correctness. **Severity:** Medium.
- **Location / evidence:** `tools/install.ps1:130-137`: `Copy-Item -Destination $installDirectory -Recurse -Force` precedes installed selftest; `:159-164` only reports failure.
- **Impact / trigger:** A failed/partial copy or failed selftest leaves the previous working installation overwritten with a mixed or invalid payload, with no rollback. Overlaying also leaves obsolete files removed from newer builds. Real notes are outside this copy and are not claimed to be deleted.
- **Suggested fix:** Validate staged payload first, keep the previous install as a rollback generation, and switch directories/files with a recovery plan after confirmed graceful quit.
- **Effort:** M. **Safe to auto-fix?** No — needs an upgrade/recovery transaction.

### A1-18-hotkey-helper-held-modifiers

- **Category:** Correctness. **Severity:** Medium.
- **Location / evidence:** `tools/press-hotkey.ps1:8`: sends modifier key-down; `:9`: `$vk = [byte][char]$Key.ToUpper()`; `:11` releases only afterward.
- **Impact / trigger:** Supplying a multicharacter/invalid key throws after Ctrl/Alt/Shift were pressed, skipping releases and leaving synthetic modifiers held. No helper was executed.
- **Suggested fix:** Validate the key before input; put modifier/key releases in finally.
- **Effort:** S. **Safe to auto-fix?** Yes — bounded helper validation and guaranteed cleanup preserve valid-key behavior.

### A1-19-mixed-currency-addition

- **Category:** Correctness. **Severity:** Medium.
- **Location / evidence:** `product/AntiNotepad/Calc/Calculator.cs:291-297`: TryAdd checks dimensions but constructs `new Q(l.V + sign * r.V, l.U ?? r.U, l.Cur ?? r.Cur, ...)`; `:728` preserves currency from numeric tokens and `:628` calls TryAdd.
- **Impact / trigger:** Mixed currencies are silently combined: `$5 + €3` follows the numeric addition path and keeps the left currency, yielding a dollar result of $8 without conversion. This is incorrect arithmetic metadata, not an exchange-rate feature request.
- **Suggested fix:** Reject mismatched explicit currency codes in additive/comparison/aggregate operations; preserve intended scalar-percentage rules and add small focused examples.
- **Effort:** S. **Safe to auto-fix?** No — confirm mixed/scalar currency semantics across arithmetic and aggregates.

### A1-20-cross-platform-payload

- **Category:** Dependencies. **Severity:** Low.
- **Location / evidence:** `product/AntiNotepad/AntiNotepad.csproj:20`: `Avalonia.Desktop`; `tools/install.ps1:109`: publish has no `-r`/RuntimeIdentifier. Existing manager Release output `build/a1-check/runtimes/` contains Linux `.so` and macOS `.dylib` payloads.
- **Impact / trigger:** Windows-only output carries native runtime assets for other platforms. Static inspection found 24 `.so` and one `.dylib`; WebAssembly packages are in supplied dependency results, but no `.wasm` file or browser runtime directory was found in this output. These are transitive packaging overhead, not independently upgradeable libraries.
- **Suggested fix:** For a chosen Windows architecture, evaluate RID-specific framework-dependent publish; verify needed WinRT/native assets remain. Keep package versions aligned with Avalonia.
- **Effort:** S. **Safe to auto-fix?** No — select supported Windows architectures first.

### A1-21-zero-caller-api

- **Category:** Code debt. **Severity:** Low.
- **Location / evidence:** `product/AntiNotepad/UI/Anim.cs:35`: `After`; `product/AntiNotepad/UI/Kit.cs:102,149`: `Segmented`, `Switch`; `product/AntiNotepad/Theme/Palette.cs:106-107`: `B`, `Brush`; `product/AntiNotepad/Model/Modes.cs:20`: `CodeLanguages`; `product/AntiNotepad/Model/Store.cs:32`: `IsLoaded`; `product/AntiNotepad/Platform/SingleInstance.cs:24,43`: `WasAbandoned` is assigned, never read. `product/AntiNotepad/Model/Store.cs:174`: `Store.Folder` also has no consumer (migration Result.Folder is a different symbol).
- **Impact / trigger:** Repo-wide symbol/reference searches find no callers/consumers for these members. Unused Kit controls duplicate Settings' active Seg/Toggle builders, and CodeLanguages duplicates the language registry without driving it. No unused entire type was established. No Settings property has zero member references outside Store.cs; JSON persistence is also an implicit consumer.
- **Suggested fix:** Remove unused nonserialized helpers/fields or document the intended consumer; use CodeColors.Languages as the language source. Do not remove serialized properties merely because serialization is implicit.
- **Effort:** S. **Safe to auto-fix?** Yes — these listed members are unconsumed local APIs; remove only after a focused reference check.

### A1-22-json-options-per-command

- **Category:** Inefficiency. **Severity:** Low.
- **Location / evidence:** `product/AntiNotepad/Editor/Completion.cs:180`: `new JsonSerializerOptions { WriteIndented = true }` per `/json`.
- **Impact / trigger:** CA1869 is real but command-triggered, not a render/typing hot path. Repeated commands rebuild serializer metadata/options.
- **Suggested fix:** Reuse one static readonly options instance.
- **Effort:** S. **Safe to auto-fix?** Yes — options are constant and do not depend on note/user state.

### A1-23-leftover-icon-and-fence-state

- **Category:** Code debt. **Severity:** Low.
- **Location / evidence:** `product/AntiNotepad/UI/Kit.cs:37-44`: dev icon-lab `public static bool Snap = true` is only read, never changed; `product/AntiNotepad/Editor/Structure.cs:24,31`: `Fence? open = null` is never assigned but checked.
- **Impact / trigger:** Icon Lab has been removed; an unwritten toggle and misleading icon-lab comment remain. Fence parsing carries redundant state while `openLine` is the real discriminator. These are small maintenance distractions, not a reason to remove approved snapping or scroll modes.
- **Suggested fix:** Inline always-on snapping, refresh GlyphIcon's faux-bold summary for current weight-zero uses, and remove the unused fence local/test.
- **Effort:** S. **Safe to auto-fix?** Yes — preserve current snapping and openLine behavior.

### A1-24-disabled-caret-still-ticks

- **Category:** Inefficiency. **Severity:** Low.
- **Location / evidence:** `product/AntiNotepad/Editor/Motion.cs:230`: GotFocus always `_blink.Start()`; `:235`: disabling CaretBlink merely returns from Tick.
- **Impact / trigger:** A focused editor with blinking disabled still wakes its dispatcher timer indefinitely until focus is lost. This is idle work, not a demonstrated leak.
- **Suggested fix:** Start/stop blinking according to focus and CaretBlink setting changes, keeping a steady visible caret.
- **Effort:** S. **Safe to auto-fix?** Yes — preserve steady caret visibility and apply changes while focused.

### A1-25-tray-empty-title-sentinel

- **Category:** Correctness. **Severity:** Low.
- **Location / evidence:** `product/AntiNotepad/Program.cs:116`: maps blank slots to string `"empty"`; `product/AntiNotepad/Platform/Tray.cs:31`: `IsEnabled = titles[i] != "empty"`.
- **Impact / trigger:** A filled slot whose actual title is exactly `empty` is disabled in the tray menu; keyboard slot access still works.
- **Suggested fix:** Pass explicit title/occupied state rather than use display text as a sentinel.
- **Effort:** S. **Safe to auto-fix?** Yes — separate state from text without changing menu design.

### A1-26-worker-effort-mismatch

- **Category:** Repo hygiene. **Severity:** Low.
- **Location / evidence:** `AGENTS.md:21`: workers `gpt-6-luna xhigh`; `docs/agents.md:13`: worker `gpt-6-luna high`; `.coord/tools/codex-run.sh:15`: `gpt-6-luna high`.
- **Impact / trigger:** Direct delegation and wrapper worker launches follow different effort contracts; the seed finding is verified. Director table's xhigh subagent wording does not fix the separate worker route.
- **Suggested fix:** Align the docs and executable worker route with the selected xhigh policy.
- **Effort:** S. **Safe to auto-fix?** Yes — AGENTS and this task explicitly choose xhigh.

### A1-27-tracked-session-snapshots

- **Category:** Repo hygiene. **Severity:** Low.
- **Location / evidence:** `.claude/atlas/0fc364ed-0bfc-4b86-b14a-cdf5aec44d80.json:1`: `"format":"conversation-atlas"`, `"sessionId"`, absolute `"root"`; the other three JSON files have the same session format at line 1. `.gitignore:1-5` ignores scratch/log/build outputs only.
- **Impact / trigger:** Four generated conversation snapshots contain prompts, session state and machine-specific paths. They are session files, not app source, create churn (one was already dirty), and do not belong in tracked product history.
- **Suggested fix:** Untrack these snapshots and ignore `.claude/atlas/` (or its JSON files). Ignore all `.claude/` only if it is confirmed to contain exclusively local state; do not hide future shared configuration by default.
- **Effort:** S. **Safe to auto-fix?** No — untracking/history policy is a manager decision; this audit performs no git mutation.

## Dependency and resource review

| Reference | Used / needed? | Evidence and decision |
|---|---|---|
| Avalonia.Desktop 12.1.4 | Yes | AntiNotepad.csproj:20; Program.cs:57-63 uses desktop/platform bootstrap and Win32 renderer options. Windows-specific publish trimming is the low-priority packaging finding, not a proposal to replace the UI framework. |
| Avalonia.Themes.Fluent 12.1.4 | Yes in current implementation | Program.cs:103 explicitly adds FluentTheme; :104 loads AvaloniaEdit's Fluent resource. Built-in TextBox, Button, ScrollViewer/ScrollBar and ToolTip templates still participate even though chrome is custom. Removing the theme without replacement templates is not a safe dependency cleanup. |
| Avalonia.AvaloniaEdit 12.0.0 | Yes | PadView.cs:69 onward constructs/uses TextEditor; structure/renderers consume its documents, folds, highlighting and visual lines. No first-party .NET/Avalonia control in this code supplies those capabilities. Version split is watch-only. |
| FluentSystemIcons-Regular.ttf | Yes | AntiNotepad.csproj:16 embeds it; Kit.cs:20 declares Glyph.Fluent; SettingsWindow.cs:162,189 and SettingsParts.cs:102,195,346 explicitly select that family. Ordinary glyphs default to Glyph.Font = Segoe Fluent Icons, with Segoe MDL2 fallback (Kit.cs:18,30). The I6 brief explicitly excludes Settings (:38), and docs/decisions.md:15-16 keeps the bundled Settings font. No contradiction or unused-font finding. |
| FluentSystemIcons-LICENSE.txt | Yes / shipped | MIT notice, Microsoft copyright at licence :1-3; csproj:17 copies to Licenses/FluentSystemIcons-LICENSE.txt, found in manager output. Font size: 2,894,360 bytes. Actual installed Windows font fallback and visual glyph coverage untested. |
| antinotepad.ico | Yes | csproj:9,14; AppInfo.IconUri; PadWindow.cs:41; Tray.cs:52; tools/make-icon.ps1:35 generates it. 4,730 bytes, embedded and executable icon. Locally generated artwork; no separate third-party icon licence identified as needed. Visual DPI quality untested. |

Kit.InputStyles intentionally overrides Fluent TextBox background/border/focus colors (Kit.cs:194-203), transparent plain-button template backgrounds/borders (:205-212), and ScrollBar opacity (:214-215). PadView.ClearBackgrounds clears editor template panel/border/presenter backgrounds (PadView.cs:141-150). These overrides do not replace all Fluent control templates. Existing local Windows OCR uses the Windows SDK (Platform/Ocr.cs), JSON uses System.Text.Json, and regex/filesystem/interop use .NET. No unnecessary independent third-party library or justified first-party replacement was established.

## SettingsWindow retention inventory

Code-reading inventory of potential roots; “internal graph” is not a leak by itself. The manager's observed `freed=False` / approximately 200 MB private memory remains an open runtime measurement. Private memory alone cannot identify a managed retained window or prove this particular trigger.

| Source / reference | Source location and quote | Lifetime / cleanup assessment |
|---|---|---|
| Resident pad's strong Settings field | PadWindow.Keys.cs:155,163-170: `_settingsWindow = window`; Closed sets it to null. | Expected while open; cleared on normal Closed. The Closed closure captures window and resident pad, but the handler is held by the window itself, not a separate static publisher. |
| Weak probe and trim timer | PadWindow.Keys.cs:165,177-203: `new WeakReference(window)`; timer stops at :190. | Weak reference does not root Settings. Resident timer reads weak probe and resident field. Forced GC/working-set trimming is locked behavior, not independently flagged. |
| Settings own handlers and controls | SettingsWindow.cs:62-71 (KeyDown, Deactivated, Closed), :165 (search), :200 (rail click); SettingsParts.cs:102-119,131-188 (local control callbacks). | Subscribers and controls belong to the window graph. Closed calls preview.Release; no external publisher found here. |
| Private preview countdown | MotionPreview.cs:108; PadView.cs:136; Timers.cs:26,30,85-94; release at PadView.cs:292-299. | **Concrete conditional dispatcher root**, A1-3-preview-timer-retains-settings. Running preview countdown is not stopped by close. |
| Swipe callbacks/timers | MotionPreview.cs:112-128: settings/pane/navigation/Moved closures capture preview; NoteSwipe.cs:59-70,93-94,126-137. | Detent settle stops on its first tick. Progress settle can remain active if frames stop with owed input: **suspected** A1-13-progress-swipe-close-retention. |
| Preview trace frames | MotionPreview.cs:137-139,143-175: scroll handler, attach/detach, `RequestAnimationFrame`; Release :184 sets sampling false. | Detach/release clears sampling; callback returns without rescheduling. Pending callback can temporarily hold preview. Framework disposal of already-queued callbacks was not inspected from library source. |
| Editor analysis/caret dispatcher timers | PadView.cs:135,295-297; Motion.cs:230-237,284-287. | Release stops analysis and custom blink timers. All caret event subscriptions are on preview-owned editor. Focus events after release/framework caret internals unverified; no extra proven external root. |
| SmoothScroll, arrival and settings transitions | PadView.cs:96,260,315-319; Motion.cs:149-155,273; Anim.cs:13-29; SettingsWindow.cs:214 onward uses Anim.Run. | Pending dispatcher/RAF closures can hold editor/control/window. Motion is finite under normal delivered frames; release does not explicitly cancel all queued framework callbacks. No additional permanent static root established. |
| Static sample notes | MotionPreview.cs:77-98: Note arrays/indexes. Private bind PadView.cs:239-242 bypasses DocumentFor's Note.Attach (:185). | Static samples retain sample strings and simple state, not a preview live-document delegate. The private document cache is cleared at PadView.cs:298; the currently bound document remains on Editor.Document (see next row). |
| Shared document caches | PadView.cs:174-188 (Recent/Owners/Documents and Note.Attach); private preview bypasses this path (:240-242). | Resident-note cache is static and bounded/evicted except dirty/on-screen entries. It does not normally store preview documents. Release leaves Editor.Document/Note/folding assigned inside the graph; no external FoldingManager retention verified. |
| Palette / Application events | PadWindow.cs:81: `ps.ColorValuesChanged += ... ApplyAppearance`; :63: static `Completion.OcrRequested +=`. | Both retain resident PadWindow, which calls into only its current Settings field. Closed field clears; no direct Settings subscription to Application or static palette events found. |
| Tray and named resident callbacks | Program.cs:114-118; Tray.cs:15,31-44; Program.cs:20,90-94; SingleInstance.cs:16-19,129. | Retain resident PadWindow or slot/store callbacks, not a closed Settings directly. Following the resident Settings field reaches null after normal close. |
| Static UI state | SettingsWindow.cs:23-31 (section metadata, selected section, Expanded); Kit.cs:18-20 (font families). | Contains strings/values/font resources, not windows. No static window cache found. |
| Posted focus/lost-focus callbacks | PadWindow.Keys.cs:235-240; PadView.cs:260,133. | Posts capture resident pad or preview controls transiently; do not reschedule a permanent loop by themselves. |

A dedicated runtime root/heap check should distinguish the confirmed started-preview-timer path from progress-swipe cancellation, queued framework frames, and native renderer/heap reservation. Audit did not change or run the existing dev settings-cycle probe.

## Dead-code, dev paths, culture and repo context

- All Settings properties (Store.cs:76-131) have member access outside Store.cs. No unused whole Editor/UI/core type was established. Zero-consumer members are enumerated in A1-21-zero-caller-api; write-only WasAbandoned and unchanged Snap are distinguished from zero callers. Reflection/serializer/library entrypoints are not treated as dead solely for lacking textual direct calls.
- Showcase is used by Completion.cs:44, Program.cs:56 and MotionPreview.cs:96. Only its startup seeding is dev-guarded (Showcase.cs:15-19); /showcase is a visible command, so it is not dead. MotionPreview is active Settings functionality (SettingsWindow.cs:35,58,533), not a leftover alternate lab.
- Alternate bar/scroll/swipe feels match docs/decisions.md:19-30 and remain deliberately selectable; no removal finding. No tracked IconLab type or --iconlab route remains. The stale Snap toggle/comment is the small surviving remnant.
- The settings-cycle probe is compiled in Release but guarded by Program.IsDev (PadWindow.Keys.cs:209). IsDev requires custom data plus DEV=1 (Program.cs:35-38). Capture F12 uses SHOTS alone (Keys.cs:20-31), so it is reachable in Release with that environment variable even without IsDev, despite its “Dev-only” comment. These documented opt-in diagnostics are inventory items, not default-user defects. Capture filesystem exceptions are not contained in that block; no permission/invalid-path probe was run. Renderer/cache/trim environment overrides remain live by design (Program.cs:19,61; PadWindow.cs:228).
- No outstanding TODO/HACK/FIXME marker was established; “todo” completion and UI labels are features. Regex fields in timers/elements/commands are static; static Regex.Match/Replace convenience calls are not automatically “new regex per call” findings because .NET caches them.
- Current runtime identity strings are AntiNotepad. Legacy Antipad strings in Store's v1 reader, DefaultDataFolderMigration, Native startup migration and SelfTest fixtures are intentional; old project name in physical checkout paths is not UI naming debt.
- I6/I7/I7b task/report paths and prior branch names are historical records, explicitly preserved by .coord/tasks/N1-antinotepad-identity.md:21-22. They should be marked/indexed as completed history if dispatch tooling starts reusing them, not silently rewritten as current instructions. N3's brief remains but its proposal is deliberately tag-only/on hold (STATUS.md:15-16; decisions.md:8-9); absent working-tree release-plan.md is not a broken active deliverable. No archive source/spike tree remains among the 78 paths.
- dev.sh delegates to dev.ps1 (dev.sh:29), so there are not two independent launch implementations to merge. dev.ps1 checks process image/start time and bounded test-data paths before stopping/cleaning (:145-236,:64-104); force-stopping only its own throwaway dev copy is documented. Uninstall refuses unsafe root directories and keeps data unless RemoveData (:79-120); safe shutdown still depends on A1-2-quit-after-save-failure.
- .gitattributes:1 enforces LF for shell scripts. Existing .gitignore covers normal build/obj/bin/scratch/log outputs. Apart from generated Atlas state, no concrete tracked build artifact or harmful attribute gap was established.

## Per-file checklist

“Reviewed” means read/source-traced or binary metadata/reference/licence review, not executed. Finding references include any directly implicated file, not only the primary location. Historical evidence is read as history.

| Path | Reviewed | Finding ids or nothing to report | One-line note |
|---|---|---|---|
| `.claude/atlas/0fc364ed-0bfc-4b86-b14a-cdf5aec44d80.json` | Yes | A1-27-tracked-session-snapshots | Parsed generated one-line Atlas snapshot; session/prompts/absolute paths are local state. |
| `.claude/atlas/2f45f362-d9fb-4b01-818e-4bd362de7768.json` | Yes | A1-27-tracked-session-snapshots | Parsed generated one-line Atlas snapshot; session/prompts/absolute paths are local state. |
| `.claude/atlas/4f4ae1d6-e998-4eb9-a1be-e100a230655b.json` | Yes | A1-27-tracked-session-snapshots | Parsed generated one-line Atlas snapshot; session/prompts/absolute paths are local state. |
| `.claude/atlas/966c1dc8-0742-4d57-af39-7b41c8396fd3.json` | Yes | A1-27-tracked-session-snapshots | Parsed generated one-line Atlas snapshot; session/prompts/absolute paths are local state. |
| `.coord/STATUS.md` | Yes | nothing to report | Current dispatch and memory open item read; preserved preexisting edit. |
| `.coord/reports/I6-win11-menu-toolbar-in-app.md` | Yes | nothing to report | Historical menu/corner result and build evidence read; not a current run command. |
| `.coord/reports/I6-win11-menu-toolbar-in-app.worker.codex.md` | Yes | nothing to report | Historical worker/director summary read; detailed companion report retained. |
| `.coord/reports/I7-menu-corner-artifacts.md` | Yes | nothing to report | Historical menu/corner result and build evidence read; not a current run command. |
| `.coord/reports/I7-menu-corner-artifacts.worker.codex.md` | Yes | nothing to report | Historical worker/director summary read; detailed companion report retained. |
| `.coord/reports/I7b-menu-corners-dwm.backup.codex.md` | Yes | nothing to report | Historical worker/director summary read; detailed companion report retained. |
| `.coord/reports/I7b-menu-corners-dwm.md` | Yes | nothing to report | Historical menu/corner result and build evidence read; not a current run command. |
| `.coord/reports/N1-antinotepad-identity.director.codex.md` | Yes | nothing to report | Historical worker/director summary read; detailed companion report retained. |
| `.coord/reports/N1-antinotepad-identity.md` | Yes | nothing to report | Historical rename/migration evidence and intentional old-name exceptions read. |
| `.coord/reports/N2-canonical-install-and-dev-badge.director.codex.md` | Yes | nothing to report | Historical worker/director summary read; detailed companion report retained. |
| `.coord/reports/N2-canonical-install-and-dev-badge.md` | Yes | nothing to report | Historical install/dev evidence and unverified live boundaries read. |
| `.coord/tasks/I6-win11-menu-toolbar-in-app.md` | Yes | nothing to report | Historical approved-menu brief; retired product path and deleted lab reference intentionally preserved. |
| `.coord/tasks/I7-menu-corner-artifacts.md` | Yes | nothing to report | Historical corner-fix brief; retired branch/product paths are historical context. |
| `.coord/tasks/I7b-menu-corners-dwm.md` | Yes | nothing to report | Historical corner-fix brief; retired branch/product paths are historical context. |
| `.coord/tasks/N1-antinotepad-identity.md` | Yes | nothing to report | Historical identity/install brief; prior branch and completion commands retained as evidence. |
| `.coord/tasks/N2-canonical-install-and-dev-badge.md` | Yes | nothing to report | Historical identity/install brief; prior branch and completion commands retained as evidence. |
| `.coord/tasks/N3-release-and-update-plan.md` | Yes | nothing to report | Historical on-hold proposal; missing working-tree release plan is intentional tag-only state. |
| `.coord/tools/codex-run.sh` | Yes | A1-26-worker-effort-mismatch | Routing, caps, failover and output paths read; effort mismatch verified. |
| `.gitattributes` | Yes | nothing to report | Shell LF rule present; no concrete additional attribute gap found. |
| `.gitignore` | Yes | A1-27-tracked-session-snapshots | Build/scratch/log ignores present; Atlas session state remains tracked. |
| `AGENTS.md` | Yes | A1-26-worker-effort-mismatch | Current product/workflow rules read; task write restriction overrides STATUS rule. |
| `CLAUDE.md` | Yes | nothing to report | Delegates to AGENTS.md. |
| `docs/agents.md` | Yes | A1-26-worker-effort-mismatch | Launch contract read; separate worker tier differs from AGENTS. |
| `docs/decisions.md` | Yes | nothing to report | Locked designs respected; retired archives/labs live in git history. |
| `docs/dev-loop.md` | Yes | nothing to report | Installed/dev isolation and deliberate probes documented; no app action taken. |
| `product/AntiNotepad/AntiNotepad.csproj` | Yes | A1-20-cross-platform-payload | All three dependencies and both binary resources used; licence copied. |
| `product/AntiNotepad/AppInfo.cs` | Yes | nothing to report | Generated identity and resource URI consumers checked. |
| `product/AntiNotepad/AppInfo.props` | Yes | nothing to report | Single identity/version source imported by project. |
| `product/AntiNotepad/Assets/Fonts/FluentSystemIcons-LICENSE.txt` | Yes | nothing to report | Microsoft MIT notice present; existing output contains licence. |
| `product/AntiNotepad/Assets/Fonts/FluentSystemIcons-Regular.ttf` | Yes | nothing to report | 2,894,360-byte embedded font; Settings explicitly uses Glyph.Fluent. |
| `product/AntiNotepad/Calc/Calculator.cs` | Yes | A1-6-calculator-unbounded-recursion, A1-19-mixed-currency-addition | Invariant numeric paths checked; recursion and mixed-currency risks. |
| `product/AntiNotepad/Editor/ChevronGutter.cs` | Yes | A1-10-render-path-allocation | Pointer invalidation and per-fold visible-line scans reviewed. |
| `product/AntiNotepad/Editor/Colorizers.cs` | Yes | A1-10-render-path-allocation | Markdown/math colorizers and token culture/brush usage reviewed. |
| `product/AntiNotepad/Editor/Completion.cs` | Yes | A1-22-json-options-per-command | Commands, popup lifecycle, transforms and JSON serializer options checked. |
| `product/AntiNotepad/Editor/Elements.cs` | Yes | nothing to report | Visible-element generators and static regexes reviewed. |
| `product/AntiNotepad/Editor/Motion.cs` | Yes | A1-24-disabled-caret-still-ticks | Scroll RAF stopping and custom caret lifecycle reviewed. |
| `product/AntiNotepad/Editor/PadView.cs` | Yes | A1-3-preview-timer-retains-settings, A1-4-list-marker-overflow, A1-5-timer-parser-can-throw, A1-7-stale-fence-cache, A1-8-whole-document-analysis, A1-10-render-path-allocation, A1-16-timer-history-never-pruned | Binding, caches, parsing, analysis and preview release reviewed. |
| `product/AntiNotepad/Editor/Renderers.cs` | Yes | A1-10-render-path-allocation | Paper, result, checkbox and divider drawing paths reviewed. |
| `product/AntiNotepad/Editor/Structure.cs` | Yes | A1-7-stale-fence-cache, A1-8-whole-document-analysis, A1-9-fence-copy-per-colored-line, A1-23-leftover-icon-and-fence-state | Folds, fence cache and shared highlighting helpers reviewed. |
| `product/AntiNotepad/Editor/TextCommands.cs` | Yes | nothing to report | Transforms referenced by Completion/SelfTest; regex fields static. |
| `product/AntiNotepad/Model/Modes.cs` | Yes | A1-21-zero-caller-api | Mode detection/labels used; duplicate unused language list. |
| `product/AntiNotepad/Model/Showcase.cs` | Yes | nothing to report | Used by /showcase and preview; auto-seeding guarded by IsDev. |
| `product/AntiNotepad/Model/Store.cs` | Yes | A1-1-recovery-deletes-bodies, A1-11-synchronous-note-search, A1-14-synchronous-save-and-log, A1-21-zero-caller-api | Save/recovery, lazy bodies, metadata, Settings references and legacy v1 read reviewed. |
| `product/AntiNotepad/Model/Timers.cs` | Yes | A1-3-preview-timer-retains-settings, A1-5-timer-parser-can-throw, A1-16-timer-history-never-pruned | Parsing, active-only tick lifecycle and retained entry dictionary reviewed. |
| `product/AntiNotepad/Model/Welcome.cs` | Yes | nothing to report | First-run seeding checks welcomed state and existing notes. |
| `product/AntiNotepad/Platform/DefaultDataFolderMigration.cs` | Yes | nothing to report | Legacy identity intentional; move/fallback paths and temp selftests read. |
| `product/AntiNotepad/Platform/GlobalHotkey.cs` | Yes | nothing to report | Registration, parsing, callback hook and unregister ownership read. |
| `product/AntiNotepad/Platform/Native.cs` | Yes | nothing to report | Win32 clipboard/lock ownership, startup identity and disposable hooks reviewed. |
| `product/AntiNotepad/Platform/Ocr.cs` | Yes | nothing to report | Local Windows OCR; stream/bitmap disposal and error mapping read. |
| `product/AntiNotepad/Platform/SingleInstance.cs` | Yes | A1-15-startup-quit-is-dropped, A1-21-zero-caller-api | Named objects, owner PID, listener and disposal read; startup callback cross-checked. |
| `product/AntiNotepad/Platform/Tray.cs` | Yes | A1-25-tray-empty-title-sentinel | Menu/icon lifetime read; title sentinel can disable a filled slot. |
| `product/AntiNotepad/Program.cs` | Yes | A1-14-synchronous-save-and-log, A1-15-startup-quit-is-dropped, A1-25-tray-empty-title-sentinel | Startup, migrations, logging, tray disposal and lifetime wiring reviewed. |
| `product/AntiNotepad/SelfTest.cs` | Yes | nothing to report | Existing checks read, including temporary migration/store fixtures; not executed. |
| `product/AntiNotepad/Theme/Palette.cs` | Yes | A1-21-zero-caller-api | Palette customization used; two unused brush helpers. |
| `product/AntiNotepad/UI/Anim.cs` | Yes | A1-21-zero-caller-api | Used frame tween helper and unused After reviewed. |
| `product/AntiNotepad/UI/HelpOverlay.cs` | Yes | nothing to report | Local overlay callbacks and focus post reviewed. |
| `product/AntiNotepad/UI/Kit.cs` | Yes | A1-10-render-path-allocation, A1-21-zero-caller-api, A1-23-leftover-icon-and-fence-state | Glyph families, Fluent template overrides and unused helpers checked. |
| `product/AntiNotepad/UI/MotionPreview.cs` | Yes | A1-3-preview-timer-retains-settings, A1-13-progress-swipe-close-retention | Private samples, trace, timer ownership and release graph checked. |
| `product/AntiNotepad/UI/NoteSwipe.cs` | Yes | A1-13-progress-swipe-close-retention | Approved detent/progress modes used; missing progress-close cancellation. |
| `product/AntiNotepad/UI/PadMenu.cs` | Yes | nothing to report | Current custom Popup and guarded DWM cleanup; no alternate IconLab implementation. |
| `product/AntiNotepad/UI/PadWindow.Keys.cs` | Yes | nothing to report | Settings strong reference clears on close; weak probe and guarded dev cycles reviewed. |
| `product/AntiNotepad/UI/PadWindow.Layout.cs` | Yes | nothing to report | Approved bars/chrome, timed callbacks and pager glyphs reviewed. |
| `product/AntiNotepad/UI/PadWindow.Navigation.cs` | Yes | nothing to report | Note moves/trash/split, clipboard capture, timers and OCR paths read. |
| `product/AntiNotepad/UI/PadWindow.cs` | Yes | A1-2-quit-after-save-failure, A1-14-synchronous-save-and-log | Persistence, resident lifetime, application events and hooks reviewed. |
| `product/AntiNotepad/UI/SearchOverlay.cs` | Yes | A1-11-synchronous-note-search | Body reads, search cards, trash and selection behavior reviewed. |
| `product/AntiNotepad/UI/SettingsParts.cs` | Yes | nothing to report | Custom row/control builders, glyph fonts and local callbacks reviewed. |
| `product/AntiNotepad/UI/SettingsWindow.cs` | Yes | A1-3-preview-timer-retains-settings, A1-12-settings-search-materializes-all | All sections, query rebuilding, parsing and Closed handler reviewed. |
| `product/AntiNotepad/antinotepad.ico` | Yes | nothing to report | 4,730-byte executable/resource/tray icon; generated locally by make-icon.ps1. |
| `tools/dev.ps1` | Yes | nothing to report | Owned PID/path/start-time checks, reparse-safe fresh data cleanup and launch isolation read. |
| `tools/dev.sh` | Yes | nothing to report | Argument wrapper delegates to dev.ps1; no duplicated launch implementation. |
| `tools/install.ps1` | Yes | A1-17-upgrade-has-no-rollback, A1-20-cross-platform-payload | Stage/quit/copy/selftest/startup sequence read; copy lacks rollback. |
| `tools/make-icon.ps1` | Yes | nothing to report | Locally generated multi-size ICO; short-lived drawing tool reviewed, not run. |
| `tools/press-hotkey.ps1` | Yes | A1-18-hotkey-helper-held-modifiers | Input order and missing guaranteed release checked; not run. |
| `tools/uninstall.ps1` | Yes | nothing to report | Graceful quit gate and bounded install/data removal guards reviewed; not run. |

