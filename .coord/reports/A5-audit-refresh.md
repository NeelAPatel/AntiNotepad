## Findings

A5-audit-refresh — 2026-10-23, branch `feat/A5-audit-refresh`. All 27 A1 findings rechecked: 20 Open, 7 Fixed; 5 new A5 findings, for 32 blocks total. No A1 severity changed; A1-7 retains the manager-corrected Medium level. Status describes current source, while severity retains the original level for fixed findings.

Four `gpt-6-luna xhigh` readers covered Editor, UI, core/resources, and tooling/repo files (9 + 13 + 21 + 48 = 91); director merged, checked the requested history/diff and verified quoted source. The five lenses were code debt, dependencies, inefficiency, correctness and repo hygiene. Historical briefs/reports were read as history, and source paths include preexisting working-tree edits. Findings describe conditional source risks unless explicit measured evidence is named.

### A1-1-recovery-deletes-bodies

- status: Open
- severity: High
- area: Your notes
- where: product/AntiNotepad/Model/Store.cs:243
- quote: `if (!live.Contains(Path.GetFileNameWithoutExtension(file))) File.Delete(file);`
- plain: If the note list is damaged, starting the app can permanently delete note text that still exists on disk.
- technical: Startup calls Normalize and RemoveOrphans even when no index candidate was recovered (Store.cs:195-196). Cleanup deletes every body not named by that metadata, so both failed recovery and an older backup can destroy recoverable text; a body written before a failed index commit is also vulnerable.
- fix: Preserve/quarantine unreferenced bodies; disable destructive cleanup in recovery, and commit/recover body/index generations together. Retain enough generations to recover from the backup index.
- effort: M
- auto-fix safe: no — recovery policy needed
- decision needed: Which recovery and retention policy should preserve unlisted note files?

### A1-2-quit-after-save-failure

- status: Open
- severity: High
- area: Your notes
- where: product/AntiNotepad/UI/PadWindow.cs:183
- quote: `QuitRequested?.Invoke();`
- plain: Quitting after a save failure can lose your latest changes.
- technical: Quit calls Save, then closes and invokes shutdown regardless of the result (PadWindow.cs:176-183). Save reports an I/O failure only through a toast and returns void at line 220, so unsaved edits disappear and a --quit client can observe successful process exit.
- fix: Return save success, refuse normal shutdown on failure, and make quit failure observable to the requesting process; offer retry/explicit discard behavior.
- effort: M
- auto-fix safe: no — shutdown failure behavior needed
- decision needed: Should failed saves block quitting until retry or explicit discard?

### A1-3-preview-timer-retains-settings

- status: Open
- severity: High
- area: Speed and memory
- where: product/AntiNotepad/Editor/PadView.cs:136
- quote: `_timers.Tick += () => { if (Mode == NoteMode.Timer) view.InvalidateLayer(KnownLayer.Selection); };`
- plain: A countdown started in the Settings preview can keep the closed window alive and keep using memory.
- technical: MotionPreview creates a private Timers instance (MotionPreview.cs:108); its dispatcher timer retains the PadView tick callback while a countdown is running. Closing Settings invokes Release, but PadView.cs:293-299 stops only analysis/caret and clears documents, leaving this conditional retention path until the countdown completes; no runtime leak was reproduced.
- fix: Give the privately owned preview Timers a stop/dispose path, and unsubscribe PadView's handler on release. Do not stop the resident pad's shared timers accidentally.
- effort: M
- auto-fix safe: no — timer ownership needs care
- decision needed: Should preview countdowns stop when Settings closes?

### A1-4-list-marker-overflow

- status: Open
- severity: High
- area: Crashes
- where: product/AntiNotepad/Editor/PadView.cs:597
- quote: `var own = int.Parse(m.Groups[3].Value);`
- plain: Pressing Enter or Tab beside an oversized numbered list entry can crash the app.
- technical: The list pattern accepts arbitrary digit counts, and Enter/Tab reach unchecked int.Parse in Renumber and ContinueList (PadView.cs:597,632). Oversized values throw, and incrementing Int32.MaxValue wraps; these UI exceptions are logged without being handled (Program.cs:31).
- fix: Use invariant bounded TryParse and overflow-safe increment; preserve the original marker or decline continuation when out of range.
- effort: S
- auto-fix safe: no — invalid marker behavior needed
- decision needed: Should oversized list numbers stay unchanged or stop automatic numbering?

### A1-5-timer-parser-can-throw

- status: Open
- severity: High
- area: Crashes
- where: product/AntiNotepad/Model/Timers.cs:57
- quote: `duration = TimeSpan.FromSeconds(total);`
- plain: Pasting a very large or unsupported timer value can crash the app.
- technical: The regex accepts Unicode digits and unbounded duration parts, but parsing and TimeSpan.FromSeconds can throw (Timers.cs:22-23,40-57). Analyse calls this parser (PadView.cs:354), and starting an otherwise representable duration can also exceed DateTime's range (Timers.cs:75,81); no crash was reproduced.
- fix: Restrict grammar to intended digits, use invariant TryParse, reject nonfinite/out-of-range totals, and validate duration against both TimeSpan and DateTime limits.
- effort: S
- auto-fix safe: no — valid duration contract needed
- decision needed: What maximum timer duration and invalid-input behavior should apply?

### A1-6-calculator-unbounded-recursion

- status: Open
- severity: High
- area: Crashes
- where: product/AntiNotepad/Calc/Calculator.cs:683
- quote: `if (IsOp(Cur, '+')) { Take(TokenKind.Operator); return ParseUnary(); }`
- plain: Pasting an extremely deeply nested calculation can crash the app.
- technical: Unary signs, powers, parentheses and nested functions recurse without a depth/work limit (Calculator.cs:682-739). Enough pasted nesting can exhaust the process stack during note analysis; the ordinary exception catch at Calculator.cs:1049 cannot recover a stack overflow.
- fix: Enforce a parser nesting/work limit with a normal invalid-expression result, or use an explicit parse stack.
- effort: M
- auto-fix safe: no — parser limit contract needed
- decision needed: What calculation nesting and work limit should apply?

### A5-1-body-read-failure-overwrite

- status: Open
- severity: High
- area: Your notes
- where: product/AntiNotepad/Model/Store.cs:178
- quote: `catch (Exception ex) { LastError = $"read {id}: {ex.Message}"; return ""; }`
- plain: A note that temporarily cannot be read can appear empty, and editing it can replace its original text.
- technical: ReadBody converts an I/O error into an ordinary empty string (Store.cs:175-178), which Note.Text caches and PadView.DocumentFor exposes as an editable document. If the user types and the read fault later clears, normal autosave writes that replacement over the original body (Store.cs:36-37,47,257); Save clears LastError, and this is distinct from A1-1's orphan deletion.
- fix: Distinguish unreadable bodies from valid empty text and prevent editing/saving that body until a successful read or explicit recovery choice. Preserve the original file and surface retry/recovery state.
- effort: M
- auto-fix safe: no — read failure recovery behavior needed
- decision needed: Should an unreadable note stay locked until retry, with explicit recovery or discard?

### A1-7-stale-fence-cache

- status: Open
- severity: Medium
- area: Speed and memory
- where: product/AntiNotepad/Editor/Structure.cs:176
- quote: `private readonly Dictionary<int, Cache> _cache = [];   // by fence open line`
- plain: Moving code blocks repeatedly in one note can leave old copies in memory until you switch notes.
- technical: Editing does not prune cache keys for vanished or shifted opening lines; only Bind resets the cache (PadView.cs:253). Retained entries include body text, documents and highlighters, with one entry per opening-line position used before reset; this corrects A1's unbounded-growth wording while retaining its manager-approved Medium severity.
- fix: Prune stale keys after each structure update and dispose/release obsolete highlighters as appropriate; prefer document-version/fence identity keys.
- effort: M
- auto-fix safe: no — highlighting invalidation needs care
- decision needed: none

### A1-8-whole-document-analysis

- status: Open
- severity: Medium
- area: Speed and memory
- where: product/AntiNotepad/Editor/PadView.cs:343
- quote: `var lines = new string[doc.LineCount];`
- plain: Typing in a large note can feel slow because the whole note is repeatedly checked.
- technical: On edits at up to 1,500 lines, Analyse copies every line and rebuilds structure/folds/math; longer notes defer the same work by 140 ms (PadView.cs:329-345). Blank-neighbor scans and descendant fold scans can be quadratic (Structure.cs:42-43,82-93), and line count alone does not bound a long single line; slowdown was not measured.
- fix: Compute blank-neighbor/indent ranges in linear passes, cache structure by changes, and budget analysis by text size/work as well as line count.
- effort: L
- auto-fix safe: no — folding and math semantics
- decision needed: none

### A1-9-fence-copy-per-colored-line

- status: Open
- severity: Medium
- area: Speed and memory
- where: product/AntiNotepad/Editor/Structure.cs:209
- quote: `var text = doc.GetText(from, to - from);`
- plain: Scrolling a large colored code block can repeatedly copy all of its text.
- technical: ColorizeLine calls Get for each painted fence line (Structure.cs:186). Get copies the full fence body before its cache comparison at line 210, so even cache hits scale with fence size multiplied by visible colored lines.
- fix: Validate body/version once per edit or coloring pass and reuse the cached inner document for its lines.
- effort: M
- auto-fix safe: no — edit invalidation needs care
- decision needed: none

### A1-10-render-path-allocation

- status: Open
- severity: Medium
- area: Speed and memory
- where: product/AntiNotepad/Editor/Renderers.cs:111
- quote: `var items = new List<`
- plain: Drawing answer bubbles and fold markers can make the app do extra work while you scroll.
- technical: Result chips construct a list, formatted text and brushes during drawing (Renderers.cs:111-121). Gutter drawing scans visible lines per fold and creates pens (ChevronGutter.cs:61-90), glyphs allocate text/brushes (Kit.cs:37), and fade updates create gradients (PadView.cs:280-288); no frame/allocation measurement was made.
- fix: Cache palette/style drawing resources and formatted text where stable, update existing fade stops, index visible folds, and invalidate only on meaningful changes.
- effort: M
- auto-fix safe: no — drawing cache keys need care
- decision needed: none

### A1-11-synchronous-note-search

- status: Open
- severity: Medium
- area: Speed and memory
- where: product/AntiNotepad/UI/SearchOverlay.cs:79
- quote: `terms.All(t => n.PeekText().Contains(t, StringComparison.CurrentCultureIgnoreCase))`
- plain: Typing a search can briefly freeze the window when many notes need reading.
- technical: TextChanged runs Refresh, whose per-term matching calls PeekText, and result cards read bodies again (SearchOverlay.cs:43,79,104,215). Unloaded bodies use File.ReadAllText (Store.cs:177), so a large collection or slow storage blocks the UI thread per query keystroke.
- fix: Debounce/cancel queries, load each body once per query, use a bounded search cache/index and background I/O with UI result publication.
- effort: M
- auto-fix safe: no — query ordering and cache lifetime
- decision needed: none

### A1-12-settings-search-materializes-all

- status: Open
- severity: Medium
- area: Speed and memory
- where: product/AntiNotepad/UI/SettingsWindow.cs:256
- quote: `var built = BuildSection(id, withPreview: false);`
- plain: Each letter typed in Settings search rebuilds controls for every Settings page.
- technical: Each query change calls ShowResults, which constructs every section before filtering matching rows (SettingsWindow.cs:165-168,254-260). The preview is excluded, but choices/icons and unmatched row controls are still repeatedly allocated.
- fix: Represent searchable rows as descriptors and filter before constructing controls; debounce if still necessary.
- effort: M
- auto-fix safe: no — live control callbacks need care
- decision needed: none

### A1-13-progress-swipe-close-retention

- status: Open
- severity: Medium
- area: Speed and memory
- where: product/AntiNotepad/UI/NoteSwipe.cs:131
- quote: `if (Math.Abs(_owed) > 0.5) return;     // still paying out the last clicks`
- plain: Closing Settings during a swipe may leave the closed preview waking up in the background.
- technical: ProgressSettle returns before timer.Stop while input is owed; MotionPreview.Release stops sampling/editor state but never cancels NoteSwipe (MotionPreview.cs:182-185). If closing prevents the queued animation frames from draining owed input, the settle timer keeps repeating and retaining the preview; actual closed-window frame delivery remains unverified.
- fix: Add explicit NoteSwipe cancellation/disposal on preview release/detach; clear owed state and stop both settle timers.
- effort: S
- auto-fix safe: no — detach and cancellation need verification
- decision needed: none

### A1-14-synchronous-save-and-log

- status: Open
- severity: Medium
- area: Speed and memory
- where: product/AntiNotepad/UI/PadWindow.cs:60
- quote: `_saveTimer.Tick += (_, _) => { _saveTimer.Stop(); Save(); };`
- plain: Saving a large note or using slow storage can briefly pause typing or showing the pad.
- technical: The 400 ms dispatcher callback runs synchronous store serialization and atomic file writes (Store.cs:249-272), also used on blur/hide/quit. Interaction logging performs synchronous file inspection and append (Program.cs:84-85), so slow storage or large bodies can block the UI thread; no timing was measured.
- fix: Capture consistent immutable save snapshots and serialize writes off the UI thread, retaining dirty/error state and awaiting the final save; buffer diagnostic logging.
- effort: L
- auto-fix safe: no — save ordering and durability
- decision needed: none

### A1-15-startup-quit-is-dropped

- status: Open
- severity: Medium
- area: Install and tools
- where: product/AntiNotepad/Program.cs:74
- quote: `if (_window is { } w) Dispatcher.UIThread.Post(w.Quit);`
- plain: An upgrade or uninstall can fail to close the app if it is still starting.
- technical: RequestQuit posts only when _window is already attached, whereas Attach replays only pending Show (Program.cs:72-75,90-94). A quit event consumed during startup is dropped, so the requesting process waits then fails and install/uninstall abort safely.
- fix: Latch a pending quit, honor it when attached or abort initialization safely; acknowledge save/shutdown result to clients.
- effort: M
- auto-fix safe: no — startup and save states need coordination
- decision needed: none

### A1-16-timer-history-never-pruned

- status: Open
- severity: Medium
- area: Speed and memory
- where: product/AntiNotepad/Model/Timers.cs:25
- quote: `private readonly Dictionary<string, Entry> _entries = [];`
- plain: Old timer entries can accumulate during a long session and slow timer updates.
- technical: Ensure only adds or replaces entries; no remove/clear path exists, and every active 250 ms tick scans dictionary values (Timers.cs:64-69,85-94). Timer keys include note ID plus trimmed line text (PadView.cs:412), so text changes and deleted/completed timers accumulate through a resident session.
- fix: Keep running timers according to a documented policy, prune inactive unreferenced entries and remove timers on final owner disposal.
- effort: M
- auto-fix safe: no — running timer policy needed
- decision needed: Should running timers survive changes to their text or note deletion?

### A1-17-upgrade-has-no-rollback

- status: Open
- severity: Medium
- area: Install and tools
- where: tools/install.ps1:132
- quote: `Get-ChildItem -LiteralPath $stageDirectory -Force | Copy-Item -Destination $installDirectory -Recurse -Force`
- plain: A failed upgrade can leave the installed app broken without restoring the previous version.
- technical: The installer overlays the existing directory before running installed selftest (install.ps1:130-137). A partial copy or failed check leaves mixed files without rollback, and obsolete files are not removed; this does not claim deletion of the separate notes directory.
- fix: Validate staged payload first, keep the previous install as a rollback generation, and switch directories/files with a recovery plan after confirmed graceful quit.
- effort: M
- auto-fix safe: no — upgrade recovery transaction needed
- decision needed: none

### A1-18-hotkey-helper-held-modifiers

- status: Fixed
- severity: Medium
- area: Install and tools
- where: tools/press-hotkey.ps1:3
- quote: `if ($Key -notmatch '\A[A-Za-z0-9]\z') {`
- plain: The hotkey helper now rejects bad keys before pressing anything and releases keys afterward.
- technical: The helper validates the key before input and converts it before pressing modifiers, then a finally block releases the main key and all modifiers (press-hotkey.ps1:3-19). This completes the original exceptional-release fix. The separate valid-key casing failure in A5-5-hotkey-case-depends-on-language occurs before input and does not reintroduce held modifiers.
- fix: None; the source fix is complete.
- effort: S
- auto-fix safe: yes — bounded validation and cleanup
- decision needed: none

### A1-19-mixed-currency-addition

- status: Open
- severity: Medium
- area: Your notes
- where: product/AntiNotepad/Calc/Calculator.cs:296
- quote: `res = new Q(l.V + sign * r.V, l.U ?? r.U, l.Cur ?? r.Cur, l.Pct && r.Pct);`
- plain: Adding dollars and euros can show a misleading total as though they were the same money.
- technical: TryAdd checks units but combines values and keeps a currency marker without comparing currencies, so $5 + €3 follows an $8 result path. Function aggregates also combine raw values and retain the first currency (Calculator.cs:857-875), while whole-note context uses TryAdd (Calculator.cs:1022-1025); these are one existing currency finding.
- fix: Reject mismatched explicit currency codes in additive/comparison/aggregate operations; preserve intended scalar-percentage rules and add small focused examples.
- effort: S
- auto-fix safe: no — currency semantics needed
- decision needed: Should different currencies be rejected, and how should plain numbers combine with money?

### A5-2-invalid-index-bypasses-backup

- status: Open
- severity: Medium
- area: Crashes
- where: product/AntiNotepad/Model/Store.cs:190
- quote: `Data = data; loaded = true; break;`
- plain: A damaged note-list file can stop the app from starting even when a usable backup exists.
- technical: A syntactically valid index such as one with Settings set to null is accepted and ends the candidate loop (Store.cs:189-190). Normalize then dereferences Settings/Stack/Trash outside that catch (Store.cs:195,224,227,233), so a null member crashes startup before trying the backup; this is a separate validation gap from A1-1's destructive cleanup.
- fix: Validate and normalize each candidate into a temporary complete model before selecting it. Reject structurally invalid candidates and try the backup while preserving all body files.
- effort: M
- auto-fix safe: no — recovery must preserve existing notes
- decision needed: none

### A5-3-ocr-result-follows-rebound-pane

- status: Open
- severity: Medium
- area: Your notes
- where: product/AntiNotepad/UI/PadWindow.Navigation.cs:346
- quote: `var doc = pad.Editor.Document;`
- plain: Switching notes while text recognition is running can put the recognized text into the wrong note.
- technical: OcrIntoPane awaits recognition at Navigation.cs:343 and then reads the pane's current document and caret at lines 346-347. Navigation rebinds the same pane while that await is pending, and a snip request also retains only the pane, so completion can modify a different note from the one that requested it.
- fix: Capture the intended note/document and insertion target before asynchronous work, and validate it on completion. Cancel or explicitly redirect if that note was deleted or the target is no longer valid.
- effort: M
- auto-fix safe: no — destination behavior needs a choice
- decision needed: Should recognition finish in the original note or cancel when you switch notes?

### A1-20-cross-platform-payload

- status: Open
- severity: Low
- area: Install and tools
- where: product/AntiNotepad/AntiNotepad.csproj:20
- quote: `<PackageReference Include="Avalonia.Desktop" Version="12.1.4" />`
- plain: The Windows app can include files for other operating systems, increasing its size.
- technical: The project still references Avalonia.Desktop and install publish has no runtime identifier (install.ps1:109). The prior A1 output inventory found Linux/macOS native files; current source preserves that packaging risk, but A5 did not publish or re-inventory an installed payload.
- fix: For a chosen Windows architecture, evaluate RID-specific framework-dependent publish; verify needed WinRT/native assets remain. Keep package versions aligned with Avalonia.
- effort: S
- auto-fix safe: no — supported architectures needed
- decision needed: Which Windows processor architectures must the installer support?

### A1-21-zero-caller-api

- status: Fixed
- severity: Low
- area: Repo housekeeping
- where: product/AntiNotepad/Model/Modes.cs:21
- quote: `public static (NoteMode Mode, int KeywordLength, string? Argument) Detect(string text)`
- plain: Unused helper code has been removed, making future maintenance simpler.
- technical: The diff removes Anim.After, Kit.Segmented/Switch, Palette.B/Brush, Modes.CodeLanguages, Note.IsLoaded, Store.Folder and SingleInstanceService.WasAbandoned. Current scoped source/reference checks confirm these nonserialized members are gone and the active language registry/serialized settings remain; no new regression was established.
- fix: None; the source fix is complete.
- effort: S
- auto-fix safe: yes — unconsumed local helpers removed
- decision needed: none

### A1-22-json-options-per-command

- status: Fixed
- severity: Low
- area: Speed and memory
- where: product/AntiNotepad/Editor/Completion.cs:16
- quote: `private static readonly JsonSerializerOptions PrettyJsonOptions = new() { WriteIndented = true };`
- plain: Formatting JSON now reuses its settings instead of recreating them each time.
- technical: Completion.cs:16 defines one static readonly options instance and line 182 passes it to Serialize. The per-command construction is gone; no new source regression was established.
- fix: None; the source fix is complete.
- effort: S
- auto-fix safe: yes — constant options reused
- decision needed: none

### A1-23-leftover-icon-and-fence-state

- status: Fixed
- severity: Low
- area: Repo housekeeping
- where: product/AntiNotepad/Editor/Structure.cs:30
- quote: `if (openLine == 0)`
- plain: Unused icon and code-block state has been removed without changing the intended behavior.
- technical: Structure.cs:24-31 now uses only openLine for fence state, preserving the prior condition's behavior. Kit.cs:39-42 snaps unconditionally and its summary describes actual weighting; the Snap toggle and stale icon-lab comment are gone, with no new regression established.
- fix: None; the source fix is complete.
- effort: S
- auto-fix safe: yes — equivalent behavior preserved
- decision needed: none

### A1-24-disabled-caret-still-ticks

- status: Fixed
- severity: Low
- area: Speed and memory
- where: product/AntiNotepad/Editor/Motion.cs:248
- quote: `if (_editor.TextArea.IsFocused && _pad.Settings.CaretBlink) _blink.Start();`
- plain: Turning caret blinking off now stops its timer and keeps the caret solid.
- technical: Motion.cs:246-250 starts blinking only when focused and enabled, otherwise stops the timer and sets a solid caret. Focus handlers and PadView.ApplyLook at line 156 call the update, covering live setting changes; no new source regression was established.
- fix: None; the source fix is complete.
- effort: S
- auto-fix safe: yes — timer follows focus and settings
- decision needed: none

### A1-25-tray-empty-title-sentinel

- status: Fixed
- severity: Low
- area: Your notes
- where: product/AntiNotepad/Platform/Tray.cs:32
- quote: `IsEnabled = slot.Occupied`
- plain: A note titled “empty” can now be opened from the tray menu.
- technical: Program.cs:116 supplies separate title/occupancy tuples, and Tray.cs:32 uses Occupied for both labeling and enabling. A filled slot titled empty stays enabled; no new source regression was established.
- fix: None; the source fix is complete.
- effort: S
- auto-fix safe: yes — state separated from title
- decision needed: none

### A1-26-worker-effort-mismatch

- status: Fixed
- severity: Low
- area: Repo housekeeping
- where: .coord/tools/codex-run.sh:15
- quote: `worker)   echo "gpt-6-luna xhigh ${CODEX_MAX_WORKER:-10}";;`
- plain: Worker launches now use the same effort setting as the project instructions.
- technical: The executable worker route and docs/agents.md:13 both use gpt-6-luna xhigh, matching AGENTS.md. This removes the prior high/xhigh mismatch; no new source regression was established.
- fix: None; the source fix is complete.
- effort: S
- auto-fix safe: yes — routing matches selected policy
- decision needed: none

### A1-27-tracked-session-snapshots

- status: Open
- severity: Low
- area: Repo housekeeping
- where: .claude/atlas/0fc364ed-0bfc-4b86-b14a-cdf5aec44d80.json:1
- quote: `"format":"conversation-atlas"`
- plain: Saved conversation files in the repository can expose prompts and personal machine paths when shared.
- technical: Four tracked .claude/atlas JSON snapshots contain conversation-atlas format/session state and absolute paths at line 1. The ignore file covers scratch/log/build outputs but not Atlas, so local conversation state keeps producing tracked churn and can travel with repository sharing.
- fix: Untrack these snapshots and ignore `.claude/atlas/` (or its JSON files). Ignore all `.claude/` only if it is confirmed to contain exclusively local state; do not hide future shared configuration by default.
- effort: S
- auto-fix safe: no — tracking and history policy needed
- decision needed: Should the four snapshots be untracked and future Atlas snapshots ignored?

### A5-4-worker-output-slug-unchecked

- status: Open
- severity: Low
- area: Install and tools
- where: .coord/tools/codex-run.sh:25
- quote: `log="$ROOT/.coord/logs/$task.$r.log"`
- plain: A mistyped task name containing folder steps can make worker logs or reports overwrite files in an unintended folder.
- technical: The task argument from line 8 is interpolated into both log and report paths without slug validation. A value containing ../ segments changes the destination of shell redirection and --output-last-message (lines 25,31); this is a local operator/tooling mistake risk, not a remote attack claim.
- fix: Reject task values containing separators or parent-directory segments and validate the documented ID-slug format before creating outputs. Keep resolved log/report destinations inside their intended directories.
- effort: S
- auto-fix safe: yes — documented task slugs are single path components
- decision needed: none

### A5-5-hotkey-case-depends-on-language

- status: Open
- severity: Low
- area: Install and tools
- where: tools/press-hotkey.ps1:11
- quote: `$vk = [byte][char]$Key.ToUpper()`
- plain: The hotkey helper can reject a valid lowercase i on a computer using Turkish language rules.
- technical: The ASCII validator accepts i, but parameterless ToUpper uses current culture; Turkish casing produces U+0130, which cannot convert to a byte. An isolated explicit-culture string/byte conversion confirmed that conversion failure without running the helper or sending keys; conversion occurs before any key-down, so A1-18 remains fixed.
- fix: Use ToUpperInvariant or explicit ASCII case conversion before virtual-key conversion.
- effort: S
- auto-fix safe: yes — input contract is already ASCII
- decision needed: none

## Fix verification

- A1-18-hotkey-helper-held-modifiers: Checked validation before conversion/input and the unconditional key/modifier releases in `finally` (`tools/press-hotkey.ps1:3-19`); the original stuck-key path is fixed. A5-5 identifies a separate casing failure before any key-down; helper not executed.
- A1-21-zero-caller-api: Checked the diff plus current definitions/references for all nine listed helpers/state members across Anim, Kit, Palette, Modes, Store and SingleInstance; each is removed, serialized settings remain, and no new source regression was established.
- A1-22-json-options-per-command: Checked `Completion.cs:16,182`: one static readonly `PrettyJsonOptions` instance is reused by `/json`; no per-command options construction or new source regression established.
- A1-23-leftover-icon-and-fence-state: Checked `Structure.cs:24-31` and `Kit.cs:23-42`: dead fence local/toggle removed, equivalent `openLine` logic and unconditional snapping preserved, summary corrected; no new source regression established.
- A1-24-disabled-caret-still-ticks: Checked `Motion.cs:230-250` and `PadView.cs:153-156`: focus and live settings updates gate the timer, disabled state stops it and restores a solid caret; no new source regression established.
- A1-25-tray-empty-title-sentinel: Checked `Program.cs:116` and `Tray.cs:29-33`: explicit occupancy drives enabled state, so a title of `empty` is valid; no new actionable regression established. Evaluating titles for all nine slots is bounded incidental work.
- A1-26-worker-effort-mismatch: Checked `.coord/tools/codex-run.sh:15`, `docs/agents.md:13` and `AGENTS.md:21`: all worker routes specify `gpt-6-luna xhigh`; no new regression established in that change. A5-4 is an independent existing wrapper-validation gap.

Manager-collected A5 evidence was read and quoted, never rerun: `.coord/scratch/A5/build-release.txt` says `Build succeeded.`, `0 Warning(s)`, `0 Error(s)`; `selftest.txt` says `SELFTEST PASS`, `exit=0`. `analyzers-dedup.txt` contains 50 warnings: CA1859 ×18, CA1305 ×17, CA1310 ×8, CA1822 ×3, and CA1862/CA1852/CA1720/CA1001 ×1 each. `packages.txt` lists no top-level updates and says `has no vulnerable packages given the current sources` and `has no deprecated packages given the current sources`; listed transitive updates are not independent upgrade instructions. `files.txt` supplies the ordered 91-file checklist.

Analyzer disposition: list/timer numeric parsing has concrete failure paths above; fixed literal Settings choices and human-facing date formatting do not establish additional corruption. Ordinal syntax comparisons, concrete return types, static members, sealing and naming suggestions are cleanup, not 50 separate findings. Normal quit explicitly disposes the tray, so CA1001 alone does not establish a leak. Current project references remain Avalonia.Desktop/Fluent 12.1.4 and AvaloniaEdit 12.0.0; their existing consumers remain active, and no first-party replacement need or unjustified direct dependency was established. The bundled Settings font and its Microsoft MIT notice are referenced and retained.

## Checklist

Review means source reading/tracing, or binary metadata/reference/license review; it does not mean execution. Fixed-finding IDs identify verified changes, including relevant historical briefs/reports.

| path | finding ids or - | note |
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

## Not covered

- No build, analyzer run, package query, selftest, app launch/quit/stop, installer, registry change, hotkey input, branch switch, merge or commit was performed. Only this report was written; preexisting STATUS, Atlas snapshot and untracked A5 brief were left untouched, overriding the general STATUS-update rule as required by this task.
- No crash, data-loss fault injection, disk-full/permission probe, save-failure shutdown, OCR/navigation race or leak reproduction was run. A1's ordinary Settings memory-cycle results are historical evidence, not a fresh A5 measurement; the progress-swipe retention trigger still depends on unverified framework frame delivery after close.
- No live allocation, latency, idle-wakeup or physical touchpad measurements, screen inspection, or installed-copy inspection was authorized. A1-20's native-payload inventory is explicitly prior evidence; current publish configuration was checked, but no new payload was produced or inventoried.
- Package health is bounded to the supplied manager scan and its configured sources; no online freshness check, transitive-version override or third-party/library-source audit was performed. Binary review checked the 2,894,360-byte font and 4,730-byte icon, consumers and license, not visual appearance, glyph coverage or DPI rendering.
- The only isolated expression check evaluated Turkish explicit-culture casing and byte conversion: lowercase `i` became U+0130 and byte conversion failed. It did not execute the hotkey helper, alter culture settings or send input. Other correctness conclusions are source-based.
- Symbol searches are lexical, not compiler-proven reachability. The five-lens sweep found no other supported standalone findings; ambiguous clipboard request-time destinations, slot-delete behavior, crafted local-index path scenarios and legacy-path-as-file cases were reviewed but not promoted as separate findings.

