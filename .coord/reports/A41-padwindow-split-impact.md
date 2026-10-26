# A41-padwindow-split-impact

Read-only source analysis, 2026-10-25, branch `reorg/A41-padwindow-split`, HEAD `b54b47660b4ba8d7a4103b39a6b79c13f105cbf1` (read with git). No build, selftest, app launch, app termination, or live UI check was performed. The only authored file is this report. Initial `git status --short` already contained `?? .coord/tasks/A41-padwindow-split-impact.md`; that pre-existing brief is preserved. The task-specific report-only instruction overrides the routine STATUS update rule (`.coord/tasks/A41-padwindow-split-impact.md:4-5`; `AGENTS.md:18`).

All product references below are relative to `product/AntiNotepad/`. Shorthand: **W** = `UI/PadWindow.cs`, **L** = `UI/PadWindow.Layout.cs`, **N** = `UI/PadWindow.Navigation.cs`, **K** = `UI/PadWindow.Keys.cs`, **B** = `UI/PadWindow.TopBlur.cs`. Thus `W:123-156` means `product/AntiNotepad/UI/PadWindow.cs:123-156`. Recommendations are proposals grounded in the cited current implementation, not claims about unlanded branches.

Recommendation: extract hotkey registration policy first; then separate save scheduling from its window adapter; then isolate clipboard policy with its OCR priority preserved; leave taskbar/focus orchestration and pane/blur mechanics in the window initially. Navigation needs a model-only transition core, not a move of the existing partial wholesale (`W:430-481`, `W:353-423`, `N:292-315`, `W:69-85`, `N:21-35`, `B:139-185`). Preserve the real window's frame styles and stable stand-in: the locked history explicitly records the A32/A34 corner/shadow/bar failures and the A20/A22 taskbar contract (`docs/decisions.md:17-27`).

## 1. Responsibility map

The five partials total 2,014 lines in this snapshot: W 532, L 490, N 400, K 272, B 320 (last closing braces at `W:532`, `L:490`, `N:400`, `K:272`, `B:320`). There is no dynamic pane list: two permanent PadViews and one mutable active reference are created once (`W:29-30`, `L:74-79`).

| Responsibility / range | Owned state | Members and work | Dependencies / boundary |
|---|---|---|---|
| Construction and app facade, `W:20-109` | `_store`, `_palette`, `_left`, `_right`, `_active`; `QuitRequested`, status/event; `Data`, `Settings` | Constructor assembles layout/appearance/bindings, attaches handlers; `InstallHooks` | Store; Program.Store for timer liveness; PadView; native hooks; every partial. Preserve constructor order (`W:40-65`). |
| Resident presentation, `W:113-223`, `W:285-305` | `_hiddenAt`, `_fade`; inherited visibility/activation/opacity/position/state | ToggleResident overloads, ShowResident overloads, PlaceForOpen, Prewarm, ShowQuietly, HideResident overloads, PlayShow, FadeWindow, FocusActive | Fresh-note policy, taskbar, overlay, saves, trim, chrome, Native cursor, screens, Anim. Quiet show deliberately does not activate (`W:197-206`). |
| Close, quit, session end, `W:87-95`, `W:225-283` | `_quitting`; SessionEnd shared static | Quit, PrepareForSessionEnd, RetryQuit, QuitAnyway, CompleteQuit | Pane flush, Store Save/Flush, log Flush, Settings close, hook/proxy disposal, lifetime callback. Shutdown is a separate no-prompt path (`Program.cs:349-354`). |
| Taskbar and blur coordination, `W:69-85`, `W:308-349` | `_taskbarProxy`, `_blurHideGeneration` | ApplyTaskbarSetting, EnsureTaskbarProxy, OnTaskbarProxyClick, QueueHideOnBlur, CancelQueuedHideOnBlur | Platform.TaskbarProxy already owns native state; window owns visibility/active/topmost/overlay/Settings checks. Proxy consumes deactivation before Settings early return (`W:75-78`). |
| Save scheduling and persistence UI, `W:22`, `W:61`, `W:353-393` | `_saveTimer`, `_quitting`; KeepDocuments=6 | Edited, QueueSave, Save overloads | Pane Flush; Store async queue/sync gate; document Evict; toast/chrome. Store captures live text synchronously before background writer (`Model/Store.cs:648-673`, `Model/Store.cs:797-834`). |
| Hidden-memory and bounds policy, `W:395-426` | `_trim`, env-derived TrimDelaySeconds; Settings geometry; `_sizingForSplit` from N | TrimWhileHidden, RememberBounds, OnAnyScreen | Visibility, screens, pane note cache, Store.RemoveOrphans, forced GC/finalizers, Native.TrimWorkingSet, logging. Bounds only stored for visible Normal state outside split resizing (`W:419-422`). |
| Global registration policy, `W:26-34`, `W:430-481` | `_hotkey`, `_newNoteHotkey`, HotkeyStatus, HotkeyStatusChanged, NewNoteHotkeyStatus | RegisterHotkey, SwapHotkey, RegisterNewNoteHotkey | GlobalHotkey parser/registrar; environment default; UI dispatch to toggle or show+new; Toast on format failure. Disposal on quit (`W:277-278`). |
| Appearance/theme, `W:485-531` | `_palette`, `_appliedMaterial`, `_stylesPalette`; Anim.Reduced | IsDark, SystemHighContrast, ResolvePalette, SplitPalette, ApplyAppearance, CurrentPalette | System color settings, Theme.Palette, Settings, Kit, both PadViews, chrome/bars/blur, overlays/Settings. Material/style caches suppress flashes (`W:508-524`). |
| Layout and pane event wiring, `L:19-129` | `_tint`, `_scaler`, `_content`, `_top`, `_badges`, `_meta`, `_icons`, `_panes`, `_divider`, `_pill`, `_toast`, `_overlayHost`, `_topLayer`, `_bottomLayer`, `_floatPager` | BuildLayout, NewPane | Creates two PadViews with shared Timers, wires edit/mode/toast/copy/focus and AppMenu delegates to window commands (`L:111-129`). |
| Metadata/pager, `L:133-226` | `_saved`; control children rebuilt | RebuildChrome, UpdateChrome, SavedText, RefreshSaved, Chip, BuildPill | Palette, Program.IsDev badge, active PadView mode/note, timer liveness, Data stack/slots, Settings split, navigation/menu commands. |
| Bar reveal/fade, `L:36-49`, `L:235-368` | `_barsMode`, `_topShown`, `_bottomShown`, `_pointerY`, `_barsSleep`, `_chromeSleep`, `_chromeAwake`, `_chromeAnim` | Sliding, BottomZone, ApplyBars, OverlayPointer, BarsSleepTick, SlideBar, FloatPager, WakeChrome, SleepChrome, FadeChrome | Actual control parentage, popup, Native cursor, Anim, overlay state, saved label and blur. Window-only geometry/render work. |
| Toast/undo and save-failure prompt, `L:50-57`, `L:372-481` | `_toastTimer`, `_toastAction`, `_undoArmed`; `_saveFailureOverlay/Card/Reason/RetryButton/QuitButton/RetryAction` | Toast, HideToast, ShowSaveFailurePrompt, OnSaveFailureKeyDown, OnSaveFailurePointerPressed | Overlay/focus, palette, Anim, retry/quit callbacks; Ctrl+Z window routing and edit event clear undo (`K:55`, `K:79`, `L:114`). Retry button click captures initial `retry`; Enter uses mutable `_saveFailureRetryAction` (`L:407`, `L:423`, `L:460`). Preserve rather than redesign incidentally. |
| Pane binding and navigation/stack, `N:14-176`, `N:188-279`; bump `L:484-488` | `_noteSwipe`, `_sizingForSplit`; Data.Current/ViewSlot/Stack/Slots/Trash, Settings.Split/SplitSlot; `_active` | LeftNote, RightNote, BindPanes, DropIfAbandoned, Navigate, NewNote, OpenFreshNote, ShowStack, OpenSlot, PromoteToSlot, ToggleSplit, CycleSplitSlot, DeleteCurrent, RestoreFromTrash, MoveCurrent, SearchChangedData, JumpTo, OnWheel, Bump | PadView Flush/Bind/DocumentFor/Forget/Arrive; geometry/screens, focus, overlay, chrome, Edited, toast undo. NoteSwipe accepts host/current-pane/settings/navigate callbacks, not PadWindow type (`N:277`; `UI/NoteSwipe.cs:15`). |
| Pinning, `N:178-185` | Settings.Pinned, inherited Topmost | TogglePin | RebuildChrome, toast, Edited, deactivation dim/hide policy (`W:78-79`). |
| Clipboard capture, `N:283-315`, hook `W:107` | `_ownClipboard`, `_lastCaptured`, `_clipboardHook`; writes `_hiddenAt` | CopyText, OnClipboardChanged | Avalonia Clipboard; Native private formats; Store data/mode timestamps; PadView.DocumentFor; Edited. OCR image handling is checked before text capture/privacy gate (`N:294-305`). |
| Timer attention glue, `W:21`, `W:63`, `N:319-326` | Shared `_timers`; collection itself owns countdown entries | OnTimerFinished | Native.Chime, ShowQuietly, Toast, UpdateChrome; model countdown is already separate (`Model/Timers.cs:27-37`, `Model/Timers.cs:186-199`). |
| OCR/snipping, `N:330-399` | OcrTarget record, `_awaitingSnip`, `_snipPane`, `_snipTarget` | RunOcr, OcrIntoPane, CaptureOcrTarget, OcrIntoTarget | Completion.OcrRequested; Native.StartScreenSnip, Ocr clipboard image APIs, original note/document/caret, current pane membership, show/activate/focus, cache and toast. Captures original destination across awaits (`N:338-339`, `N:347-398`). |
| Window key routing/zoom/dev capture, `K:11-108` | `_galleryShot`; uses undo/overlay/active state | OnWindowKeyDown, Zoom | Completion popup first refusal of Esc; global note/pane/window commands, active Editor text/OCR; Settings.FontSize; screenshot env path and Program logging (`K:20-43`, `K:72-107`). |
| Overlay hosting, `K:11-13`, `K:112-153`, `K:242` | `_overlay` / derived `_overlayOpen` | ShowOverlay, IsOverlay, CloseOverlay, ReopenOverlay, OpenSearch, OpenHelp | Control ownership, pane Flush/opacity, pill, chrome, animation, focus; SearchOverlay and HelpOverlay hold PadWindow references (`UI/SearchOverlay.cs:16`, `UI/HelpOverlay.cs:24-29`). |
| Settings lifecycle and replacement, `K:155-271` | `_settingsWindow`, `_settingsGone`, `_settingsTrimTimer` | OpenSettings, CloseSettings, NewSettingsTrim, CycleSettingsForDev, SettingsLostFocus, ResetSettings with local Apply, SettingsChanged | Window creation/activation, preview release, delayed GC/Native trim, Program dev diagnostics, Settings replacement, registration, taskbar/appearance/Edited. Replacement means cached Settings references can become stale (`K:254-259`). |
| Top-bar sampled blur/popup native decoration, `B:13-319` | All `_topBlur*` fields: visuals/tint/group; attach/scheduled flags; content/top/left/right transform refs; window/content/layer/pane/transform handlers; edited/scroll/line handlers (`B:13-24`) | InitializeTopBlur, InitializeTopBlurVisuals, OnTopBlurPaneChanged, ObserveTopBlurTransform, UpdateTopBlurVisibility, Attach/DetachTopBlurSourceObservers, ApplyTopBlur, UpdateTopBlurBackdropVisibility, TopBlurMask, RefreshTopBlurSources, RefreshTopBlurSourcesAfterLayout, SetFreshTopBlurSource, UpdateTopBlurMasks, OnFloatPagerOpened, OnTopBlurWindowClosed; nested TopBlurVisual.Background/Render | Actual visual tree and coordinate roots, pane editors/scroll/lines/transforms/opacity, bars, palette, Render dispatch, fresh VisualBrush allocations, PadView.SetTopBackdropMask; Native.ConfigurePopupWindow only for float popup (`B:227-301`). |

## 2. Callers

### Outside PadWindow

| Caller | Exact PadWindow access / sites |
|---|---|
| Program/App startup and IPC | PadWindow constructor `Program.cs:345`; PrepareForSessionEnd `:351`; QuitRequested assignment `:354`; ShowResident `:121`, `:196`, `:355`, `:357`, `:361`; Quit `:130`, `:197`, `:355`; NewNote/OpenSettings `:355`; OpenSlot `:357`; HotkeyStatusChanged subscription `:358`; InstallHooks `:359`; Prewarm `:362`; Toast `:363`; Attach stores PadWindow `:192-197`. |
| Tray | No typed PadWindow reference. Receives window delegates at `Program.cs:355-357`; Show native-menu item `Platform/Tray.cs:16`, New note `:17`, slots `:33`, Settings `:40`, Quit `:42`, left click shows `:44`. Left click does not toggle. |
| SettingsWindow | Settings getter `UI/SettingsWindow.cs:49`; palette `:54`, `:96-97`, `:110`, `:129`; inherited Icon `:56`; SettingsLostFocus `:77-78`; SettingsChanged `:128`, `:514`; HotkeyStatus/RegisterHotkey `:475`; NewNoteHotkeyStatus `:476`, RegisterNewNoteHotkey `:479`; Toast `:505`, `:760`, `:765`; ShowResident/OpenHelp `:568`; ResetSettings `:569`; Save `:665`. |
| SearchOverlay | Settings.SearchGrid `UI/SearchOverlay.cs:37`, mutation `:391`; Data `:73`, `:96`, `:266`, `:269`, `:273`, `:349`; CloseOverlay `:74`, `:340`; IsOverlay `:88`, `:104`, `:125`, `:134`, `:379`; SearchChangedData `:274`, `:378`, `:383`; inherited Bounds `:282`; RestoreFromTrash/Toast `:340`; JumpTo `:341`; Toast `:353`, `:368`; OpenSearch `:392`, `:395`. Search also bypasses window through Program.Store (`:108`) and directly mutates stack/trash/current (`:266-274`, `:349-383`). |
| HelpOverlay | Constructor receives window; header close delegate `UI/HelpOverlay.cs:24-29`. |
| NoteSwipe | No typed PadWindow reference: constructed by window with Navigate method group and functions for live Settings/current pane (`N:277`); calls navigate after resetting transform/opacity (`UI/NoteSwipe.cs:210-220`). Preview creates another independent swipe (`UI/MotionPreview.cs:85`, `UI/MotionPreview.cs:111-123`). |
| PadView | No typed PadWindow reference. Events/properties exposed at `Editor/PadView.cs:71-78` are wired in `L:114-128`. Editor menu invokes AppMenu and combines actions (`Editor/PadView.cs:749-754`); standard context-menu Copy calls Editor.Copy directly (`Editor/PadView.cs:745`), while chip-copy alone emits the custom Copy event (`Editor/PadView.cs:630`), so that standard copy bypasses CopyText/`_ownClipboard`; focus raises FocusedPane (`Editor/PadView.cs:139`); document edit raises Edited (`Editor/PadView.cs:488`). |
| PadMenu | No typed PadWindow reference. Invokes supplied MenuEntry.Click actions; window supplies NewNote/OpenSearch/ToggleSplit/TogglePin/OpenSettings/HideResident (`L:118-127`; `UI/PadMenu.cs:14`, `UI/PadMenu.cs:127`, `UI/PadMenu.cs:165`). |
| Platform hooks/proxy | GlobalHotkey invokes Action<long> at `Platform/GlobalHotkey.cs:74`, callbacks installed at `W:438`, `W:476`; native clipboard Action at `Platform/Native.cs:25` -> `W:107`; TaskbarProxy Action<bool,string> at `Platform/TaskbarPresence.cs:446` -> `W:317-328`. Proxy holds base Window, not PadWindow (`Platform/TaskbarPresence.cs:93`, `Platform/TaskbarPresence.cs:115`). |
| Completion / model timers | Static Completion.OcrRequested subscription -> RunOcr (`W:64`; producer `Editor/Completion.cs:20`, `Editor/Completion.cs:45`); Timers.Finished -> OnTimerFinished (`W:63`; `Model/Timers.cs:196`). |
| SelfTest | No PadWindow construction/reference in current `SelfTest.cs`; it exercises taskbar policy, close policy, background Store saves/logs, PadView static analysis and NoteSwipe separately (`SelfTest.cs:73-75`, `SelfTest.cs:451-478`, `SelfTest.cs:655-724`, `SelfTest.cs:976-1144`). Passing those cannot prove window choreography. |

### Inside PadWindow: member call-site index

The following generated index records each method declaration in the five partials and all same-name occurrences in those partials outside its declaration signatures (including same-line wrapper calls). It includes method groups/event wiring and calls in lambdas/local functions; overloaded methods share a row. It is a source cross-check, not compiler symbol analysis; qualified calls on other objects and text literals are excluded. The responsibility map and external table disambiguate ownership. Event and framework dispatch sites are identified separately above; source subscriptions do not imply that a handler has run. Properties/state are covered by sections 1 and 3; nested Render is framework-dispatched (`B:318`).

| Member | Declaration(s) | Inside-partial calls / method-group references |
|---|---|---|
| `PadWindow` | W:38 | No internal construction; external construction at Program.cs:345. |
| `InstallHooks` | W:103 | No direct occurrence; see external/framework dispatch above. |
| `ToggleResident` | W:113; W:115 | W:113 (public wrapper calls private overload), 438 |
| `ShowResident` | W:121; W:123 | W:118, 121 (wrapper), 230, 328, 476; N:362; K:161 |
| `PlaceForOpen` | W:159 | W:131 |
| `Prewarm` | W:181 | No direct occurrence; see external/framework dispatch above. |
| `ShowQuietly` | W:197 | N:323 |
| `HideResident` | W:209; W:211 | W:93, 117, 209 (wrapper), 326, 345; L:126, 147; K:47, 238 |
| `Quit` | W:225 | K:96 |
| `PrepareForSessionEnd` | W:239 | W:89 |
| `RetryQuit` | W:262 | W:231, 265 |
| `QuitAnyway` | W:268 | W:231, 265 |
| `CompleteQuit` | W:270 | W:235, 264, 268 |
| `PlayShow` | W:285 | W:153 |
| `FadeWindow` | W:297 | W:79, 85; K:239 |
| `FocusActive` | W:305 | W:96, 136, 141; N:35; K:136 |
| `ApplyTaskbarSetting` | W:308 | W:108, 134, 191, 205, 221; K:268 |
| `EnsureTaskbarProxy` | W:316 | W:105; K:267 |
| `OnTaskbarProxyClick` | W:319 | W:317 |
| `QueueHideOnBlur` | W:333 | W:78 |
| `CancelQueuedHideOnBlur` | W:349 | W:85, 127, 201, 215, 247, 323 |
| `Edited` | W:353 | L:114; N:74, 89, 109, 123, 142, 165, 184, 199, 200, 213, 230, 242, 249, 265, 314, 387; K:107, 270 |
| `Save` | W:360; W:379 | W:228, 264, 360 (public wrapper calls private overload) |
| `QueueSave` | W:362 | W:61, 73, 217 |
| `TrimWhileHidden` | W:404 | W:62 |
| `RememberBounds` | W:417 | W:97, 98 |
| `OnAnyScreen` | W:425 | W:47 |
| `RegisterHotkey` | W:430 | W:106; K:256 |
| `SwapHotkey` | W:450 | W:437, 475 |
| `RegisterNewNoteHotkey` | W:469 | W:107; K:257 |
| `ResolvePalette` | W:496 | W:44, 505 |
| `SplitPalette` | W:501 | W:517, 518 |
| `ApplyAppearance` | W:503 | W:59, 99; K:269 |
| `BuildLayout` | L:59 | W:58 |
| `NewPane` | L:111 | L:74, 75 |
| `RebuildChrome` | L:133 | W:520; N:182 |
| `UpdateChrome` | L:159 | W:356; L:115, 128, 155; N:34, 241, 325, 396; K:135 |
| `SavedText` | L:172 | L:167, 177 |
| `RefreshSaved` | L:175 | L:316, 341 |
| `Chip` | L:180 | L:139, 164, 166 |
| `BuildPill` | L:189 | L:169 |
| `ApplyBars` | L:238 | W:521 |
| `OverlayPointer` | L:283 | W:67 |
| `BarsSleepTick` | L:299 | L:98 |
| `SlideBar` | L:312 | L:269, 270, 291, 292, 308, 309 |
| `FloatPager` | L:329 | L:317 |
| `WakeChrome` | L:337 | W:67, 85, 154; L:104, 105; K:121 |
| `SleepChrome` | L:345 | W:68, 357; L:106; K:59 |
| `FadeChrome` | L:357 | L:341, 354 |
| `Toast` | L:372 | W:370, 375, 387, 443, 478; L:116; N:72, 73, 132, 141, 183, 199, 212, 324, 340, 366, 369, 388, 397; K:93, 106, 250 |
| `HideToast` | L:395 | L:107, 384; K:55, 79 |
| `ShowSaveFailurePrompt` | L:401 | W:231, 265 |
| `OnSaveFailureKeyDown` | L:453 | L:61 |
| `OnSaveFailurePointerPressed` | L:474 | L:62 |
| `Bump` | L:484 | N:59, 62, 238 |
| `BindPanes` | N:21 | W:60; N:70, 88, 99, 108, 115, 122, 140, 164, 174, 210, 229, 248, 264 |
| `DropIfAbandoned` | N:39 | N:68, 85, 121, 226, 260 |
| `Navigate` | N:51 | L:194, 204; N:277; K:64, 65, 83, 84 |
| `NewNote` | N:77 | W:476; L:120; K:82 |
| `OpenFreshNote` | N:93 | W:129 |
| `ShowStack` | N:102 | L:201; N:54, 116; K:85 |
| `OpenSlot` | N:112 | L:219; N:256; K:76 |
| `PromoteToSlot` | N:127 | K:76 |
| `ToggleSplit` | N:145 | L:122, 142; K:89 |
| `CycleSplitSlot` | N:168 | N:53 |
| `TogglePin` | N:178 | L:123, 144; K:90 |
| `DeleteCurrent` | N:188 | K:86 |
| `RestoreFromTrash` | N:216 | N:212 |
| `MoveCurrent` | N:234 | K:66, 67 |
| `SearchChangedData` | N:246 | No direct occurrence; see external/framework dispatch above. |
| `JumpTo` | N:252 | No direct occurrence; see external/framework dispatch above. |
| `OnWheel` | N:269 | W:66 |
| `CopyText` | N:286 | L:117; K:93 |
| `OnClipboardChanged` | N:292 | W:107 |
| `OnTimerFinished` | N:319 | W:63 |
| `OcrTarget` (record type) | N:330 | Constructed at N:356; storage/return/parameter uses N:334, 352, 359. |
| `RunOcr` | N:336 | W:64; K:92 |
| `OcrIntoPane` | N:345 | N:297 |
| `CaptureOcrTarget` | N:352 | N:338, 347 |
| `OcrIntoTarget` | N:359 | N:349 |
| `OnWindowKeyDown` | K:15 | W:65 |
| `Zoom` | K:102 | K:94, 95 |
| `ShowOverlay` | K:112 | L:449; K:151, 242 |
| `IsOverlay` | K:125 | W:74; L:404, 450, 455, 476 |
| `CloseOverlay` | K:127 | W:74, 216; L:479; N:256, 263; K:47 |
| `ReopenOverlay` | K:139 | W:523 |
| `OpenSearch` | K:148 | L:121, 141, 201; K:54, 87, 143 |
| `OpenSettings` | K:158 | L:125, 146; K:53, 88, 219 |
| `CloseSettings` | K:175 | W:276; K:225 |
| `NewSettingsTrim` | K:185 | K:179 |
| `CycleSettingsForDev` | K:207 | W:96 |
| `SettingsLostFocus` | K:235 | No direct occurrence; see external/framework dispatch above. |
| `OpenHelp` | K:242 | K:58, 91, 144 |
| `ResetSettings` | K:245 | No direct occurrence; see external/framework dispatch above. |
| `SettingsChanged` | K:264 | K:258 |
| `InitializeTopBlur` | B:26 | L:108 |
| `InitializeTopBlurVisuals` | B:93 | L:89 |
| `OnTopBlurPaneChanged` | B:117 | B:64, 65 |
| `ObserveTopBlurTransform` | B:131 | B:52, 58, 70, 71, 121, 165, 166 |
| `UpdateTopBlurVisibility` | B:139 | B:47, 90 |
| `AttachTopBlurSourceObservers` | B:154 | B:143 |
| `DetachTopBlurSourceObservers` | B:170 | B:148, 306 |
| `ApplyTopBlur` | B:187 | W:522; L:279; B:89 |
| `UpdateTopBlurBackdropVisibility` | B:199 | B:62, 126, 151, 195 |
| `TopBlurMask` | B:214 | B:193 |
| `RefreshTopBlurSources` | B:227 | B:28, 32, 37, 42, 75, 77, 78, 82, 83, 84, 85, 86, 87, 127, 136, 196, 211 |
| `RefreshTopBlurSourcesAfterLayout` | B:234 | B:231 |
| `SetFreshTopBlurSource` | B:247 | B:242, 244 |
| `UpdateTopBlurMasks` | B:264 | B:31, 36, 41, 59, 74, 122, 210 |
| `OnFloatPagerOpened` | B:291 | B:80 |
| `OnTopBlurWindowClosed` | B:304 | B:81 |
| `Render` | B:318 | No direct occurrence; see external/framework dispatch above. |

### Property, event and constant access supplement

| Member / declaration | Readers, writers or subscribers |
|---|---|
| Data / Settings (`W:35-36`) | All window model/setting operations in sections 1/3; direct external readers/mutators are SettingsWindow's S getter (`UI/SettingsWindow.cs:49`) and SearchOverlay (`UI/SearchOverlay.cs:37`, `:73`, `:96`, `:266-274`, `:349-392`). Data.Settings replacement is `K:254`; it is not a fixed constructor Settings snapshot. |
| QuitRequested (`W:32`) | Set by App `Program.cs:354`; invoked by CompleteQuit `W:282`. |
| HotkeyStatus / HotkeyStatusChanged (`W:33-34`) | Written/emitted `W:439-440`; Settings reads `UI/SettingsWindow.cs:475`; Program subscribes `Program.cs:358`. |
| NewNoteHotkeyStatus (`W:481`) | Settings reads `UI/SettingsWindow.cs:476`; it derives from current registration slot rather than stored setting. |
| IsDark / SystemHighContrast (`W:485-494`) | ResolvePalette reads both `W:496-498`; SplitPalette reads IsDark `W:501`. |
| CurrentPalette (`W:531`) | SettingsWindow constructor/restyle/sync checks `UI/SettingsWindow.cs:54`, `:96-97`, `:110`, `:129`. |
| LeftNote / RightNote (`N:17-18`) | BindPanes `N:23`, `N:28`; LeftNote also NewNote `N:80`, OpenSlot `N:118`, RestoreFromTrash `N:221`, JumpTo `N:255`. |
| Sliding / BottomZone (`L:235`, `L:296`) | Sliding read by pill exit `L:99`, ApplyBars `L:261`, OverlayPointer `L:285`, WakeChrome `L:339`, SleepChrome `L:348`; BottomZone by OverlayPointer `L:290` and BarsSleepTick `L:309`. |
| _overlayOpen (`K:13`) | Deactivation/focus/blur `W:78`, `W:305`, `W:339`; pointer/chrome `L:285`, `L:349`; wheel `N:271`; key routing `K:47`, `K:50`; SettingsLostFocus `K:238`. |
| _settingsTrim (`K:179`) | Settings Closed starts/stops it `K:170`; getter constructs through NewSettingsTrim only on first use. |
| TrimDelaySeconds / KeepDocuments (`W:398`, `W:395`) | Trim timer initializer `W:397`; successful normal async/sync save eviction `W:371`, `W:391`. |
| TopBlurVisual.Background / Render (`B:316-318`) | SetFreshTopBlurSource writes Background `B:255-260`, then InvalidateVisual `B:261`; framework Render reads it `B:318`. |

The save coordinator also must not become the exclusive owner of Store persistence: Settings folder move first calls `_pad.Save()`, checks LastError, then Program.MoveNotes calls Store.Save again before moving/switching folders (`UI/SettingsWindow.cs:663-678`; `Program.cs:167-183`). Store.Load legacy migration calls Save directly (`Model/Store.cs:574`). Preserve those backend entry paths and their error gates while extracting the window debounce (`W:353-393`).

## 3. Shared mutable state

| State | Writers / readers spanning responsibilities | Extraction constraint |
|---|---|---|
| `_left`, `_right`, `_active` | Created `L:74-79`; focus changes active `L:128`; BindPanes corrects it `N:32`; navigation selects it `N:87`, `N:107`, `N:115-120`, `N:160-162`, `N:228`, `N:262`; OCR selects captured pane `N:395`; keys read editor `K:92-93`; save/trim use both notes `W:365-391`, `W:408`; blur observes both `B:154-184`. | Pass current pane/notes at operation time. There is no stable “current note” field independent of selection and pane focus (`N:17-18`, `N:190-192`). |
| `Data` and replaceable `Settings` | Data facade `W:35-36`; navigation mutates stack/slots/trash/current/view (`N:39-265`); search also mutates them (`UI/SearchOverlay.cs:266-274`, `UI/SearchOverlay.cs:349-383`); settings reset replaces object (`K:254`); geometry writes `W:420-422`; pin/split/zoom write `N:149-163`, `N:180`, `K:104`. | A40 events must survive replacement; do not capture old Settings or duplicate selection state. A navigation extraction needs search reconciliation, not just key callers. |
| `_hiddenAt` | Initialized `W:25`; set by prewarm/hide `W:190`, `W:218`; hidden clipboard append refreshes it `N:313`; show reads it for fresh-note threshold `W:128-129`. | Clipboard must signal hidden activity to owner without taking visibility/fresh-note policy. |
| `_overlay`, `_overlayOpen`, prompt refs | Host writes `K:115`, `K:130`; focus gate `W:305`; blur guard `W:78`, `W:339`; save prompt deactivation closes overlay `W:74`; wheel gate `N:271`; chrome sleep gate `L:349`; prompt key/pointer gate `L:455`, `L:476`. | Overlay install must precede posted query/prompt focus; clear before focusing editor (`K:127-136`). |
| `_settingsWindow` | Set before Show `K:164-172`; Closed clears conditionally `K:168`; pad Deactivated early return `W:77`; queued blur checks `W:339`; SettingsLostFocus checks active window `K:237`; appearance sync `W:524`; quit closes `W:276`. | Keep assignment, close handlers and delayed active-state check together. |
| `_blurHideGeneration`, proxy suppression and generations | Window cancellation increments `W:349`; queue captures/checks `W:335-345`; activate/show/hide/taskbar/session-end cancel (`W:85`, `W:127`, `W:201`, `W:215`, `W:247`, `W:323`); proxy one-shot state `Platform/TaskbarPresence.cs:153-157`, `:220-227`. | Do not replace synchronous cancellation with asynchronously delivered Changed events. |
| `_saveTimer`, `_quitting`, SessionEnd.IsEnding | Edits/bounds restart `W:355`, `W:422`; tick stops/enqueues `W:61`; session end stops/sets global gate `W:241-247`; quit marks flag only after flush `W:273-275`; async save checks before/after await (`W:364`, `W:369`). | Scheduler alone is insufficient: preserve synchronous final snapshot, drain and UI suppression. |
| Cached documents / note live text | Navigation forgets or edits cache `N:45`, `N:136`, `N:198-205`; clipboard/OCR append `N:310-312`, `N:383-386`; saves evict `W:371`, `W:391`; hidden trim evicts `W:408`; document text callback attached to note (`Editor/PadView.cs:201-215`). | Keep cache operations on UI thread and dirty/on-screen exclusions (`Editor/PadView.cs:232-244`). |
| `_palette`, `_barsMode`, `_topShown/_bottomShown`, transforms | Appearance rebuilds `W:505-523`; bars reparent/move/opacity `L:238-334`; active pane arrival/bump/swipe transform `N:33`, `L:486-488`, `UI/NoteSwipe.cs:176-220`; blur observes transforms and bottom popup visibility (`B:26-90`, `B:139-150`). | Blur belongs to window visual tree; avoid new event ownership or duplicate animation state. |
| `_undoArmed`, `_toastAction`, prompt retry action | Toast writes `L:377-378`; HideToast clears `L:397`; edit clears undo `L:114`; key router invokes `K:55`, `K:79`; deletion closures change model/cache (`N:199`, `N:212`); prompt refresh/key path `L:407`, `L:460`. | Model transition may return undo data; window still owns toast lifetime and key priority. |
| `_timers` | Shared by both PadViews `L:113`; model liveness uses Program.Store `W:21`; chrome reads AnyRunning `L:168`; finished callback shows pad `N:319-325`. | Countdown state is already separate; attention/focus adapter remains window work. |
| Clipboard/OCR state across awaits | Own copy set before await `N:288-289`; last capture `N:309`; pending snip set `N:338-339`, cleared before OCR await `N:296-297`; captured document/offset used after image read `N:347-398`. | Preserve image-first priority, original note identity and deletion guard; no new concurrency serialization without an explicit behavior decision. |

## 4. Timers and dispatcher use

### PadWindow-owned timers

| Timer / owner | Interval and registration | Start / restart | Stop / completion |
|---|---|---|---|
| Save debounce | 400 ms `W:22`; tick `W:61` | Edited `W:355`; RememberBounds `W:422` | Tick stops then QueueSave; session end `W:245`. Save/HideResident do not explicitly stop this timer (`W:211-222`, `W:379-392`). It is a trailing debounce, not a continuously running loop. |
| Hidden trim | Env `TRIM_SECONDS`, default 45 s `W:397-398`; tick wired `W:62` | Prewarm background hide `W:190`; hide restarts `W:220` | Tick stops itself, exits if visible (`W:404-407`); session end `W:246`. Show does not stop it; visibility check avoids trimming a shown pad. |
| Bars sleep | 450 ms `L:45`; tick wired `L:98` | Pill exit `L:99`; pointer leaves reveal zone `L:293`; sliding SleepChrome(soon) `L:348` | BarsSleepTick stops then queries real cursor/pointer and hides individual bars (`L:299-309`). |
| Chrome sleep | Initial 1600 ms `L:46`; tick `L:106` | WakeChrome sets max(300, FadeAfterMs), starts unless held (`L:340-342`); soon SleepChrome restarts `L:351` | WakeChrome stops before state check; tick stops then SleepChrome(false) (`L:106`, `L:340`). |
| Toast | Initial 2600 ms `L:47`; tick `L:107` | Toast replaces interval: action 7000, brief 1300, otherwise 2400 ms, restarts (`L:390-392`) | Tick stops then HideToast; manual HideToast clears action/undo and fades, without stopping timer (`L:395-398`). |
| Settings trim (lazy) | 1.5 s `K:178-187` | Settings Closed handler stops/restarts (`K:166-170`) | Tick stops; if a new Settings window exists, returns; otherwise forced collection and working-set trim (`K:188-200`). |

Only save and hidden-trim timers are explicitly stopped by PrepareForSessionEnd; CompleteQuit explicitly disposes hooks/proxy but does not stop every UI timer. This is existing behavior, not a recommendation to broaden teardown during extraction (`W:239-259`, `W:270-282`).

### Every explicit PadWindow UIThread post

| Site | Priority / purpose | Guard / ordering |
|---|---|---|
| `W:99` | Default; system color change -> ApplyAppearance | Constructor event subscription; no explicit unsubscribe here. |
| `W:107` | Default; clipboard native update -> async OnClipboardChanged | Posted work before clipboard/OCR access; hook disposed on quit `W:279`. |
| `W:139-146` | Input; second FocusActive and hotkey latency log | Scheduled after synchronous Show/Activate/Focus, before animation setup in source order; FocusActive gates visible/overlay (`W:305`). |
| `W:186-193` | Background; invisible prewarm -> Hide/reset flags/start trim/taskbar | No generation guard; must not introduce another dispatch layer. |
| `W:337-346` | Background; deferred hide-on-blur | Generation plus active/visible/Settings/topmost/overlay/settings-policy recheck. |
| `W:438` | Default; global toggle callback carrying original timestamp | One post from native hotkey -> ToggleResident. |
| `W:476` | Default; second global hotkey -> ShowResident then NewNote | Preserve callback order and distinct registration id. |
| `K:235-240` | Background; Settings deactivation/close -> conditional hide/dim | Rechecks pad active and Settings active; does not share blur-generation token. |
| `L:450` | Default; focus Retry button after installing save-failure overlay | Checks overlay identity first. |
| `B:231` | Render; coalesced RefreshTopBlurSourcesAfterLayout | `_topBlurRefreshScheduled` flag, visible/group-visible guards before and after posting (`B:227-244`). |
| `B:293-301` | Loaded; float pager native popup/theme setup | Rechecks popup open, top-level exists, not pad and not Window; then Native.ConfigurePopupWindow. |

Async save continuation implicitly resumes on its captured UI context because QueueSave does not use ConfigureAwait(false); synchronous pane flush/snapshot happens before awaiting. OCR and clipboard routines also await from UI callbacks; do not move whole routines onto Task.Run (`W:362-376`, `N:286-315`, `N:345-398`; background writer itself is `Model/Store.cs:748-790`).

### Product-wide adjacent scheduling (not owned by the extracted controller)

| Owner | Scheduling and lifetime evidence |
|---|---|
| Model.Timers | 250 ms dispatcher tick (`Model/Timers.cs:29`, `:37`); Sync starts while AnyRunning, stops otherwise (`:184`); Stop clears entries (`:169-179`); OnTick fires Finished then resynchronizes (`:186-199`). |
| NoteSwipe | Lazy detent settle 130 ms; input restarts, tick self-stops (`UI/NoteSwipe.cs:61-75`). Lazy progress settle 260 ms; input restarts, waits while owed motion is fresh, drops stalled debt at 1000 ms then stops (`:21-22`, `:109-110`, `:154-173`). Fire/Cancel stop both (`:210-228`); RequestAnimationFrame uses weak refs/generation (`:113-139`). No PadWindow hide call to Cancel in current lifecycle (`W:211-222`). |
| PadView / SmoothCaret | PadView analysis debounce 140 ms (`Editor/PadView.cs:491`), long-document edit restarts (`:464-479`), short edit stops (`:484`), Bind stops (`:271`), Release stops (`:425`), tick stops (`:148-151`). Template clear-background post Loaded (`:146`); bind scroll restoration post Loaded with bind token (`:305`). Caret blink 530 ms; LostFocus stops, UpdateBlink starts only focused+enabled; Stop stops (`Editor/Motion.cs:217`, `:231`, `:246-249`, `:302`). |
| Anim / scroll / caret frames | Anim.Run allocates handle, calls initial step, schedules frames until duration/cancel; reduced decorative/no top-level completes immediately (`UI/Anim.cs:12-29`). Scroll frames start through motion methods and stop on thresholds/no top-level (`Editor/Motion.cs:139-204`); caret frames stop with animating flag and draw progress (`Editor/Motion.cs:266-302`). |
| TaskbarProxy | Background post resets native deactivation snapshot (`Platform/TaskbarPresence.cs:222-227`); Input post delivers one click decision, checks disposal/presence generation, then Background releases pending flag (`:435-453`). These posts are part of the focus contract, not general-purpose scheduling to consolidate. |
| Program | IPC show/quit and Attach dispatch default UI posts (`Program.cs:121`, `:130`, `:196-197`); unhandled UI exception event is not a post (`:47-52`). BackgroundLog worker is Task.Run with explicit Flush (`:215`, `:230-268`). |
| SearchOverlay | 125 ms query debounce; restarts on query, tick stops, detached stops+disposes search generation (`UI/SearchOverlay.cs:42-66`); StartSearch stops (`:105`). Default constructor initial search and query focus posts (`:86-92`); Task.Run search then UI InvokeAsync success/error with generation/overlay guard (`:122-135`). |
| SettingsWindow / HelpOverlay | Settings findPause 150 ms; text change stops then restarts if nonblank, tick/Closed stop (`UI/SettingsWindow.cs:173-181`); scroll restore Loaded (`:233`); hotkey editor focus/select-all default post (`:557`). Help initial Focus default post (`UI/HelpOverlay.cs:49`). |
| MotionPreview | RequestAnimationFrame sampling until motion quiet for trace span+100 ms (`UI/MotionPreview.cs:163-177`); Release clears sampling, cancels swipe, releases PadView, stops Timers (`:184-190`). |
| Store | Background writer coalesces for 50 ms or FlushRequested, then writes immutable captured snapshot (`Model/Store.cs:356`, `:748-790`); this is Task.Delay, not DispatcherTimer. |
| Dev Settings cycle | Await delays 1500, 300, six times 1000, then 2500 ms per close; gated by Program.IsDev and env (`K:207-228`). This is diagnostic asynchronous UI work, not a recurring product save/trim timer. |

## 5. Focus, activation and hide-on-blur ordering

These are source-order sequences. Avalonia/Win32 can synchronously raise lifecycle events inside Show/Activate/Hide; code alone does not establish the complete OS message interleaving (`W:69-96`; `Platform/TaskbarPresence.cs:196-250`).

| Trigger | Exact sequence to retain |
|---|---|
| Normal show | Session-end gate -> trace -> cancel queued blur -> compute away/fresh-note condition -> optional OpenFreshNote (bind without focus) -> remember previous visibility -> optional placement -> Normal -> Show -> taskbar presence -> Activate -> synchronous FocusActive -> Input-priority second focus/log -> dropdown animation or first-show content fade -> WakeChrome -> trace (`W:123-155`; `N:93-99`). FocusActive is no-op under overlay or while hidden (`W:305`). |
| Activated | Session-end gate -> dev trace -> cancel blur -> WakeChrome -> FadeWindow(1) (`W:81-85`). This must invalidate a previously queued Background hide before it can run. |
| Hide | Session-end gate -> trace -> cancel blur -> CloseOverlay (restores pane/pill/chrome, focuses while still visible) -> QueueSave (flushes both panes and captures Store snapshot before await) -> record hidden time -> Hide -> restart hidden trim -> taskbar presence -> trace (`W:211-222`, `K:127-136`, `W:362-371`, `Model/Store.cs:648-651`). Do not await disk completion before Hide. |
| Deactivated | Session-end gate -> trace -> QueueSave -> dismiss save-failure overlay if current -> consume proxy-transfer suppression -> if Settings exists return -> if HideOnBlur, not Topmost, no overlay, visible, not transfer: queue Background hide -> otherwise if pinned+DimWhenPinned fade to .72 (`W:69-79`). Queued hide rechecks generation and current conditions (`W:333-346`). |
| Hidden startup/prewarm | Program constructs window -> tray/status subscription -> InstallHooks -> Attach -> Prewarm (`Program.cs:345-362`). Prewarm trace -> Opacity=0, ShowActivated=false -> Show -> Background post -> Hide -> Opacity=1, ShowActivated=true, hidden time, trim.Start -> taskbar presence -> trace (`W:181-193`). Opened handler still runs FocusActive/CycleSettingsForDev (`W:96`); invisibility/no-activation intent does not prove absence of all focus events. |
| Toggle hotkey | WM_HOTKEY sets handled/captures timestamp -> callback posts -> if visible+active HideResident("hotkey-toggle"), else ShowResident(timestamp,"hotkey-toggle") (`Platform/GlobalHotkey.cs:69-75`; `W:438`; `W:115-118`). Visible inactive pad must be focused, not hidden. |
| New-note hotkey | Separate id 0x4151, environment disabled -> post -> ShowResident -> NewNote (flush left, select/create stack note, active left, bind+arrival+focus, Edited) (`W:469-476`; `N:77-89`). Keep show-before-new order, including possible fresh-note policy execution inside show. |
| Taskbar button | Proxy snapshots native foreground before Avalonia activation; recognizes direct/recent loss to taskbar/proxy/null within 500 ms; suppresses duplicate post-hide fallback for 1 s; selects hide only for visible+foreground; captures presence generation; Input dispatch -> window callback cancels queued blur -> if the decision is hide, HideResident only if still visible (already hidden is a no-op); only a non-hide decision calls ShowResident -> proxy pending flag clears at Background (`Platform/TaskbarPresence.cs:15-45`, `:196-227`, `:265-315`, `:372-453`; `W:319-328`). Do not add another post/event queue between native decision and cancellation/action. |
| Tray click / menu | Tray left click always show; Show menu likewise; New note show then NewNote; slot show then OpenSlot only if not already selected; Settings uses OpenSettings; Quit uses Quit (`Platform/Tray.cs:15-44`; `Program.cs:355-357`). Tray does not implement taskbar toggle semantics. |
| Settings open | If pad hidden ShowResident -> if existing Settings Activate+return -> construct Settings -> assign `_settingsWindow` and weak ref -> register Closed clear/trim handler -> Show separate window (`K:158-172`). Assignment before Show makes pad Deactivated return without hide/dim (`W:77`). Constructor hooks Settings Deactivated/Closed callbacks (`UI/SettingsWindow.cs:77-78`). |
| Settings close / lose focus | Settings Deactivated posts SettingsLostFocus; Closed releases preview then posts it again (`UI/SettingsWindow.cs:77-78`); owner's later Closed handler clears reference iff same instance and restarts trim (`K:166-170`). Background callbacks recheck pad/Settings active, then hide if unpinned+blur+visible+no overlay, otherwise dim if pinned (`K:235-240`). No automatic Activate/FocusActive is issued by CloseSettings (`K:175`). |
| Overlay open/close | ShowOverlay flushes panes, sets `_overlay`, installs control, shows host, hides pane ink/pill, holds chrome, animates (`K:112-123`); OpenSearch then posts query focus (`K:148-152`; `UI/SearchOverlay.cs:92`). Close clears overlay/host before restoring pane/pill/chrome and FocusActive (`K:127-136`). Esc defers to Completion popup first (`K:43-47`). |
| Timer completion | Chime -> ShowQuietly (if hidden: cancel blur, ShowActivated=false, Show, restore ShowActivated=true, taskbar) -> Toast -> chrome update; no Activate or editor Focus call in this method (`N:319-325`; `W:197-206`). |
| Session end / ordinary close | ShutdownRequested -> PrepareForSessionEnd -> e.Cancel=false (`Program.cs:349-352`); Begin once, stop save/trim, cancel blur, flush left/right, synchronous Store.Save+LastError check, log, FlushLogs finally (`W:239-259`). Closing OSShutdown calls same method then cancellation policy permits session/quit closes; ordinary close cancels and hides (`W:87-94`; `Platform/SessionEnd.cs:10-18`). |

## 6. Show/hide hot path and extraction budget

**Common show path:** `GlobalHotkey.WndProc` -> Action<long> -> one default UI post -> ToggleResident -> ShowResident -> SessionEnd gate/trace/cancel -> away calculation -> PlaceForOpen only when hidden -> WindowState.Normal -> Show -> synchronous visibility observers / possible Opened+Activated handlers -> ApplyTaskbarSetting -> Activate -> FocusActive -> `_active.FocusText` -> `Editor.TextArea.Focus` -> enqueue Input focus confirmation -> PlayShow or dropdown -> WakeChrome (`Platform/GlobalHotkey.cs:69-75`; `W:438`, `W:115-155`, `W:305`; `Editor/PadView.cs:453`). Show may synchronously trigger blur-source attach/visibility updates and schedule one Render refresh (`B:45-47`, `B:139-168`, `B:199-244`). Actual render/focus completion timing requires Windows measurement, not inferred call counts.

**Conditional show work:** threshold-triggered OpenFreshNote may allocate a Note and bind panes/update chrome (including UI controls/document load) before Show (`W:129`; `N:93-99`; `Editor/PadView.cs:201-215`, `Editor/PadView.cs:265-310`; `L:159-225`). Non-remember placement reads native cursor/screens and updates Position, which can trigger RememberBounds (`W:159-177`, `W:97`, `W:417-422`). Taskbar synchronization only calls an existing proxy; creation is in InstallHooks/SettingsChanged, not ApplyTaskbarSetting (`W:105`, `W:308-317`; `K:267`).

**Hide path:** same native callback/post -> ToggleResident -> HideResident -> cancel -> CloseOverlay -> QueueSave -> both pane Flush -> synchronous Store snapshot/queue -> hidden timestamp -> Hide (blur observers detach; float popup closes) -> restart trim -> ApplyTaskbarSetting (`W:117`, `W:211-222`, `W:362-371`; `Model/Store.cs:648-651`, `Model/Store.cs:797-834`; `B:139-150`, `B:170-184`). Hide has no fade-out wait. QueueSave is async void but everything before its first incomplete await executes before Hide, so the path is not allocation-free (`W:362-368`).

**Existing allocations/indirection:** callback closures/default post and focus-confirmation closure; cause/trace/log strings (some gated by dev); PlayShow TranslateTransform/Anim handle/closures; conditional fresh Note/PadView document and chrome rebuild; Store snapshot arrays/dictionaries/cloned settings, save completion tasks; `[_left.Note,_right.Note]` on eviction; blur refresh creates new VisualBrush by design (`W:139-151`, `W:285-302`, `W:368-371`; `UI/Anim.cs:12-28`; `Model/Store.cs:797-834`; `B:247-260`). Do not claim zero-allocation toggling today.

**Proposed extraction cost constraints:** construct owned collaborators once before input registration; pre-bind callbacks; retain original native timestamp; invoke window actions directly from the existing UI callback; no new dispatcher hop, awaited event, per-toggle subscriber construction, lazy controller/proxy initialization, service resolution, snapshot of all settings, or synchronous save on show. A single direct collaborator method call is acceptable as a design proposal, but latency neutrality needs the current `hotkey-to-focus ms/sync/focused` log plus manual perceived responsiveness before/after; the log measures callback-to-focus, not physical keypress-to-photon (`W:138-146`; `Platform/GlobalHotkey.cs:74`). Keep save snapshot cost where it is; moving it to a worker would violate live editor affinity (`Editor/PadView.cs:208`; `Model/Store.cs:808`).

## 7. Extractability and narrow contracts

SAFE means the proposed bounded portion is suitable for extraction; it does not assert the current whole responsibility has no window dependencies. Proposed types/names are illustrative. Keep them internal and window-owned; no service locator or app-wide replacement state (`W:20-30`, `L:74-79`).

| Candidate | Verdict and evidence | Proposed boundary |
|---|---|---|
| Hotkey registration/swap/status | **SAFE**, smallest initial extraction. Native hook already separate; window portion is two slots, swap rollback, status and callbacks; Settings/Program consumers are explicit (`W:430-481`; `UI/SettingsWindow.cs:475-479`; `Program.cs:358`). | `PadHotkeys.RegisterToggle(string?)`, `RegisterNewNote(string?)`, `Status`, `NewNoteStatus`, `StatusChanged`, `Dispose`. Inject registration factory, default-gesture/env lookup, direct `Action<long> toggle`, `Action newNoteRequested`, parse-failure notification. Native adapter may take base Window; registration policy may not know PadWindow or UI. Factory enables no-HWND tests. Keep exactly one existing UI post in adapter/composition, not one in both layers (`W:438`, `W:476`). |
| Save debounce / queue coordinator | **SAFE for scheduling/gating**, **ENTANGLED for pane/cache/prompt work** (`W:353-393`, `W:239-282`). | `SaveCoordinator.MarkChanged()`, `Queue()`, `SaveNow()`, `Drain()`, `BeginEnding()`; inject scheduler and persistence operations. Window adapter synchronously flushes panes before persistence snapshot; errors/success callbacks return to UI and obtain *current* protected note pair for eviction. Window retains Edited's chrome/sleep side effects and all prompt/quit/lifetime sequencing. Keep Store queue/coalescing implementation intact (`Model/Store.cs:648-790`). No extra task around QueueSave. |
| Clipboard watcher | **ENTANGLED as currently written**: OCR priority, private-format filtering, live cached documents, hidden time, multiple awaits (`N:292-315`). **SAFE subpart**: own-copy/dedup/normalization policy. | Policy: `RecordOwnCopy(text)`, `TryPrepareCapture(text, existingText, isPrivate) -> entry?`; coordinator: `CopyAsync`, `OnChangedAsync`, injected clipboard/private-format adapters, target selector, append callback, `Captured` callback for Edited/hidden-activity. Keep original async read ordering and image-first routing in one owner; no PadView/Window/UI in policy. Native listener remains disposable host adapter (`Platform/Native.cs:19-30`). Do not silently add semaphore/late target reselection. |
| Note navigation/stack | **ENTANGLED**, needs smaller model core. Shares selection with SearchOverlay; Flush/Forget/DocumentFor, pane focus/geometry, undo, animation and save/chrome (`N:21-279`; `UI/SearchOverlay.cs:266-274`, `:349-383`). | Model `NoteNavigation` takes live StoreData/accessor and returns a small transition result: selected note(s), direction/bump, removed identities, confirmation/undo data. Methods Navigate/New/SelectSlot/Promote/Restore/Reorder after owner flush. No Avalonia.Controls, PadView, document cache, UI types or callbacks exposing a Window. Keep BindPanes, ToggleSplit geometry, Bump, OnWheel, FocusActive and toast closures in window. First extract only one operation with parity table, then expand. Avoid event-driven rebind races; caller applies transition synchronously (`N:55-74`, `N:145-175`). Preserve current persistence-trigger differences: OpenFreshNote and CycleSplitSlot do not call Edited themselves, unlike Navigate/NewNote; adding automatic notifications could change save timing (`N:93-99`, `N:168-175`, `N:74`, `N:89`). |
| Taskbar presence glue | **ENTANGLED; defer**. Native proxy is already isolated and typed to base Window; remaining glue lies directly in activation/hide races (`W:69-85`, `W:308-349`; `Platform/TaskbarPresence.cs:93-115`, `:435-453`). | If later needed: thin `TaskbarPresenceOwner.Apply(mode, visible)`, `ConsumeDeactivationToProxy()`, direct `Action<bool,string>` callback, Dispose. It may wrap existing proxy, but must not own pad show/hide/blur flags, decide hide a second time, alter native styles or introduce another dispatch. Current five glue methods are cheap enough to leave. |
| Hidden trim / Settings trim | **LEAVE in A41 initially**. Visibility, GC timing, on-screen notes, orphan drain, weak Settings refs and Native seam overlap (`W:404-414`; `K:185-203`). | Later a scheduler can request a window-owned trim only after rechecking visibility/Settings. No GC/trim or orphan sweep in show/hide directly. |
| Timer attention | **LEAVE** the three UI actions; countdown state already belongs to Model.Timers (`N:319-325`; `Model/Timers.cs:27-37`). | Replace Program.Store liveness through A43 context/accessor; no new timer controller just to move a few lines. |
| OCR | **ENTANGLED; leave unless clipboard extraction needs a narrow handshake**. Destination capture, document identity, deletion and focus are intertwined (`N:330-398`). | Optional image-request callback returning whether update was consumed; retain captured destination and 120 s predicate. Broad OCR redesign would overlap A42/A44. |
| Layout, keys, pane binding/split sizing, appearance, blur, resident focus/activation and overlays | **LEAVE** as true window/editor adapter concerns (`L:59-129`, `L:238-368`; `K:15-153`; `N:21-35`, `N:145-175`; `W:123-223`, `W:503-524`; `B:26-312`). | Reuse A39 relocated primitives and A42 commands; do not relocate controls to model namespace just to reduce PadWindow line count. |

## 8. Recommended extraction order

These are proposed implementation substeps, not work executed here. Each ends with an isolated check-output build, `--selftest`, and applicable manual gate before proceeding; the existing dev procedure defines zero warnings/errors and SELFTEST PASS (`docs/dev-loop.md:76-79`). The manager owns commits; no git changes were made (`docs/agents.md:26-29`).

1. **A41-padwindow-split-hotkeys:** after A39 lands, add the registration policy and fake-factory capability checks, preserve PadWindow facade/status event and existing dispatch callbacks. Move only `_hotkey/_newNoteHotkey` ownership and SwapHotkey/register logic; verify same-key no-op, rollback, disable and disposal. Build/selftest; manager tests live rebinding/conflict and first hidden-start hotkey (`W:430-481`, `W:103-108`, `W:277-278`).
2. **A41-padwindow-split-save-scheduler:** extract just 400 ms debounce with fake scheduler; keep Edited chrome/sleep, pane flush and Store calls in owner callbacks. Then move common queue/final-save gates only if the ordering is explicit and tests remain small. Build/selftest after each move; gate edit-hide-immediate-show, forced save failure/retry and final close (`W:61`, `W:239-282`, `W:353-393`). Do not move Store writer internals (`Model/Store.cs:734-790`).
3. **A41-padwindow-split-clipboard-policy:** extract dedup and normalization first; retain image-first listener routing and append/hidden-activity callback. Build/selftest with fake clipboard and destination; manual private clipboard, own-copy and snip-then-navigate checks (`N:286-315`, `N:330-398`). Do not add a new asynchronous queue as an incidental cleanup.
4. **A41-padwindow-split-navigation-core:** coordinate A42 ownership first, then move pure stack selection/reorder one operation at a time. Add transition tables, leave flush/rebind/animation/cache effects explicit; expand to abandoned blanks/slots/delete/restore only after search's direct mutations have a reconciliation contract. Build/selftest per operation group, manually gate split right-pane navigation and search undo (`N:39-265`; `UI/SearchOverlay.cs:266-274`, `:349-383`).
5. **A41-padwindow-split-presence-adapter (optional/deferred):** only if meaningful window code remains removable, wrap proxy lifetime/apply behind an eager-owned adapter without touching native message logic or deactivation order. Build/selftest taskbar truth tables and manually test all modes/focus click sequences. It is reasonable to finish A41 without this step (`W:69-85`, `W:308-349`; `Platform/TaskbarPresence.cs:396-453`).

Do not combine these into a broad renaming/folder/style cleanup. Behavior-neutral review can compare each narrow body/call sequence against the cited originals; the native frame decision explicitly forbids another style experiment under this reorganization (`docs/decisions.md:17-23`).

## 9. Silent-regression risks and symptoms

| Risk | Mechanism / evidence | Observable symptom and gate |
|---|---|---|
| Focus and late blur | A show/activate must invalidate Background hide; FocusActive ignores overlays (`W:85`, `W:127`, `W:305`, `W:333-349`). | Pad flashes then disappears, hotkey shows but typing goes elsewhere, search loses query focus. Manually test quick toggle/focus transfer; no-window generation checks are supplementary. |
| Prewarm | Opacity and ShowActivated are temporarily changed; hide is unguarded Background work (`W:181-193`); Opened also focuses (`W:96`). | First hotkey invisible, first keystroke lost, taskbar button appears during startup, or late prewarm hides newly shown pad. Exercise immediate input during hidden startup; extraction must not add another post. |
| A22-A25 stand-in | Real pad ShowInTaskbar=false; stable minimized proxy, native early focus snapshots, one-shot suppression, 500 ms/1 s policies and dispatcher priorities (`W:52`, `W:75`, `W:308-349`; `Platform/TaskbarPresence.cs:15-45`, `:115-157`, `:196-227`, `:396-453`). | Double button, focused click hides then reopens, inactive click hides, hidden Always button stops working. Test Never/While visible/Always, visible active/inactive/hidden, fast repeated clicks and Explorer button recreation; policy tests alone do not exercise HWND behavior. |
| A30 background saving | QueueSave captures snapshot before await; completion only evicts after success; dirty/revision acknowledgment prevents clearing newer edits (`W:362-391`; `Model/Store.cs:797-822`, `Model/Store.cs:896-916`). | Last characters disappear, stale caret/scroll restored, newer edit marked clean, responsiveness stalls on disk. Check burst+queued final save fixtures plus manual large-note hide/reopen. |
| A26 shutdown | ShutdownRequested handles ApplicationShutdown ambiguity before Closing; Begin is once-only; failure log no prompt; resident close policy must permit both windows (`Program.cs:346-352`; `W:239-259`; `Platform/SessionEnd.cs:10-18`; `Platform/TaskbarPresence.cs:134-138`). | Sign-out blocked, app respawns/hides instead of closing, last edit lost, prompt appears during shutdown. Test policy/recorded fake call order without window; actual restart/sign-out still manual and previously unresolved (`.coord/STATUS.md:34`). |
| Quit/retry/error UI | Quit gates on synchronous Save; failure shows pad/prompt; QuitAnyway still drains writer before marking quitting/disposal (`W:225-282`; `L:401-480`). | Quit silently loses failed save, app exits before queued write, retry edits act on stale overlay. Fake failure gate plus dev failure prompt/keyboard/pointer test. |
| Split panes | Both panes flush for split/save; selection may be right; never bind same slot twice; sizing flag protects stored width (`N:21-35`, `N:51-54`, `N:112-123`, `N:145-175`; `W:419-422`). | Wrong note changes, duplicated slot side by side, right-pane edits lost, window doubles/halves repeatedly or width drifts. Model selection tests plus manual resize/split/navigation/save. |
| Cache/undo/trash | Notes attach live editor document readers; blank drop and deletion Forget; slot clear uses document text; eviction excludes dirty/on-screen (`Editor/PadView.cs:201-244`; `N:39-47`, `N:188-230`). | Undo text/history lost, blank notes accumulate, clipboard/OCR updates wrong cache, restored selection wrong. Transition tables and dev editor undo across swipes/cache trimming. |
| Search mutates behind navigation | Search reorder/delete/undo changes Data and invokes SearchChangedData; rebind avoids focus while overlay exists (`UI/SearchOverlay.cs:266-274`, `:349-383`; `N:246-249`). | Search selection jumps, invalid Data.Current, stale editor after deleting current result. Reconcile tests and manual multi-delete/reorder/restore. |
| Clipboard privacy/OCR | Image check precedes private text filter; own/last/substring suppression, latest clipboard-mode target; original snip destination retained (`N:294-312`, `N:338-398`). | Secret text captured, self-copy echo, repeated bullets, OCR inserted in newly selected note instead of original, deleted target resurrected. Fake text policy and captured-target tests; real clipboard/snipping manually. |
| Top-bar blur | Visibility attaches/detaches observers, per-pane transforms/masks, one Render refresh, fresh VisualBrush, float popup Loaded native setup (`B:139-185`, `B:227-301`). | Frozen text under bar, double ink/sharp text, blank/opaque top band, wrong split sample, orphan float pager, growing hidden work. Manual three bar styles, both palettes/materials, split/swipe/scale/hide-show. Keep this partial out of extraction. |
| Native frame/styles | BorderOnly+extended client area; A32/A34 reverted because frame changes broke corners/shadow/bars (`W:49-52`; `docs/decisions.md:17-23`). | Square corners, heavy border, missing shadow, broken drag/resize or bars. Require unchanged style setup and manual visual gate; layer tests cannot prove DWM appearance. |
| Hidden/Settings trims | Delayed forced compacting GC and Native.TrimWorkingSet; hidden trim does not run while visible; no immediate trim on show/hide (`W:397-414`; `K:185-203`). | Slow next show from premature paging, Settings retained, GC stalls while editing. Preserve timer delays/visibility checks and compare dev diagnostics. |
| Settings event migration | Whole Settings replacement and direct writer paths outside SettingsWindow exist (`K:254-258`, `N:149`, `N:180`, `K:104`, `W:420-422`). | New collaborator reads stale settings; duplicate save/rebuild/registration on one change; open Settings causes pad to hide. Coordinate A40 subscriptions and test replace/reset/undo. |

## 10. Proposed guards

No guard was added or run in this read-only task. Prefer small capability tables and fakes over constructing a Window; existing SelfTest already uses policy/store fixtures rather than PadWindow (`SelfTest.cs:655-724`, `SelfTest.cs:976-1144`).

| Proposed selftest | Without a window? | Cases / basis |
|---|---|---|
| Hotkey swap table | Yes, fake registrar | Same registered label no dispose/create; dispose old before create; refused new restores old; failed restore leaves empty; invalid parse reports false; new-note blank disables; independent slots/dispose once. Preserve existing textual label semantics rather than normalize modifier order incidentally (`W:430-481`; `Platform/GlobalHotkey.cs:97-118`). |
| Debounce / final-save order | Yes, fake clock/scheduler and recorder | Repeated edits restart 400 ms once; bounds request also restarts; tick stops before queue; ending prevents normal queue completion UI; final sequence flush-left, flush-right, synchronous save, log flush once. Quit-anyway still drains. Keep UI chrome callbacks separate (`W:61`, `W:239-282`, `W:353-393`, `W:422`). |
| Store capability checks | Yes, existing temp fixtures | Retain burst coalescing, final live snapshot, copied settings, failure/retry/unavailable folder and flush/logger cases; do not duplicate them in coordinator tests (`SelfTest.cs:976-1144`; `Model/Store.cs:648-790`). Add only a missing coordinator boundary contract. |
| Clipboard policy table | Yes | Null/blank, own text, repeat last, trimmed substring already present, CRLF/multiline bullet indentation, private formats, latest modified clipboard note; record own text before SetTextAsync; hidden activity notification only after append. Fake image update proves snip route consumes before text path (`N:286-315`). |
| Navigation transitions | Yes, new model core | Bump at start/empty tail; create at nonblank end; reuse trailing blank; abandoned blank drops without losing arriving identity; slots 1..9, slot toggle->stack, right-pane cycle excludes left slot; promote occupied/empty; delete last stack leaves one; restore index bounds; reorder edges; search deletion reconciliation. Assert selected identities/current indices and effects, not UI animation internals (`N:39-279`). |
| OCR destination contract | Yes if captured-target policy is separated; document fixture without Window if needed | Capture original note/doc/offset, navigate before result, delete before result, hidden target append vs same-doc insert; do not create a fake PadWindow hierarchy solely to test OCR (`N:330-398`). |
| Taskbar / resident-close tables | Yes, existing | Preserve ShouldShow modes, focused/hidden click, null counterpart within grace, duplicate fallback, all close reasons. New generation/scheduler fake can cover dispatch invalidation, but does not validate native hooks (`SelfTest.cs:655-724`; `Platform/TaskbarPresence.cs:435-453`). |
| Window visual/focus behavior | **Manual dev copy** | All section 9 focus/prewarm/taskbar/Settings/three bars/frame/split/OCR and perceived latency checks; actual Windows sign-out/restart with last-second edit. Build/selftest cannot attest these (`W:69-96`, `W:123-223`; `Program.cs:346-352`; `B:139-301`). |

### Table-driven LAYERS proposal after A39

Use fully qualified type/namespace rules and explicit source allowlists; reject fully qualified references as well as `using` imports. Check generic arguments, fields/properties/events, method signatures and member references if using metadata/IL; source-only regex is a cheap supplement and misses aliases/reflection. A39's final implementation is not in this snapshot; current cross-layer primitives still reside in UI (`UI/Anim.cs:3-6`; `UI/Chrome.cs:1-5`; `UI/PadMenu.cs:11-16`; `Editor/PadView.cs:52`, `:78`).

| Rule | Proposed allowed / forbidden references | Current rationale |
|---|---|---|
| PadWindow inbound allowlist | Only Program/App composition and UI.SettingsWindow/SearchOverlay/HelpOverlay may type-reference PadWindow initially; extracted Navigation, save coordinator, clipboard policy, hotkey policy, Model/Calc/Editor/Theme/Platform may not. | Existing inbound references listed in section 2; base Window in GlobalHotkey/TaskbarProxy is intentional (`Platform/GlobalHotkey.cs:18`, `:34`; `Platform/TaskbarPresence.cs:93`, `:115`). Do not confuse `OnPadWindowMessage` name with a PadWindow type dependency. |
| Navigation core | Allow Model and small result/identity contracts; forbid `Avalonia.Controls`, `AvaloniaEdit`, PadView, Native, Program and `AntiNotepad.UI`. | Current mixed operations demonstrate why simply moving N into a different directory is insufficient (`N:21-35`, `N:145-175`, `N:198`, `N:277`). |
| Extracted classes | No `AntiNotepad.UI` or PadWindow; no UI callback signature exposing Control/PadView. Keep any actual host Window adapter in presentation/platform composition with explicit allowlist. | Existing hooks take Window but policy need not; menu/motion primitives should use A39's eventual non-UI types (`W:438`, `N:310`; `Editor/PadView.cs:78`). |
| Save coordinator | Forbid Window/Control/PadView, Native, Program, direct GC/file/Win32 operations; allow injected scheduler/store contract. Store owns disk writer; additional Store entry paths remain Program.MoveNotes (`Program.cs:174`) and legacy Load migration (`Model/Store.cs:574`), rather than routing every backend call through a window coordinator. | UI pane/cache/notification work must remain adapter callbacks (`W:362-391`; `Model/Store.cs:734-790`). |
| Clipboard policy | Forbid UI, Window/PadView, Native/Ocr and dispatcher; adapters may call platform clipboard and OCR handshake. | Policy can normalize/filter; owner must append to live document and apply hidden activity (`N:294-314`). |
| Hotkey policy | Forbid UI, PadWindow, direct show/hide and Program statics; platform registrar may retain base Window and native hooks. | RegisterHotkey policy currently mixes these through factory closures (`W:430-478`). |
| Program and native seams | Ban new Program/Native references in extracted core now; stage existing UI Native ban with A44, Program ban with A43. Do not globally fail unchanged callers before the parallel tasks land. | Existing usages in section 11; source baseline must distinguish agreed debt from newly introduced violations (`W:21`, `W:162`, `K:196`, `B:300`). |
| Foundation layers | Preserve A39's landed rules for Model/Calc/Theme/Editor and moved Anim/ChromeTokens/MenuEntry. Avoid blanket “no Avalonia in Model” until current Timers and SessionEnd design is addressed. | Model.Timers intentionally uses Avalonia.Threading today (`Model/Timers.cs:3`, `:29`); it is outside a model-only navigation core restriction. |

Proposed cheap read-only grep checks (run from repo root; all are future checks, not claims of zero matches now):

```powershell
rg -n '\bPadWindow\b' product/AntiNotepad --glob '*.cs'
rg -n 'Program\.|Native\.|Dispatcher|Avalonia\.Controls|AvaloniaEdit|PadView|AntiNotepad\.UI' <extracted-core-files>
rg -n 'ShowInTaskbar|WindowDecorations|ExtendClientArea|SetWindowLong|DwmSetWindowAttribute' product/AntiNotepad
rg -n 'DispatcherTimer|Dispatcher\.UIThread|RequestAnimationFrame|Task\.Run|Task\.Delay' product/AntiNotepad --glob '*.cs'
rg -n 'SettingsChanged\(|RegisterHotkey\(|RegisterNewNoteHotkey\(|ApplyTaskbarSetting\(' product/AntiNotepad --glob '*.cs'
rg -n 'PrepareForSessionEnd|ShutdownRequested|ShouldCancelClose|\.Flush\(|QueueSave\(' product/AntiNotepad --glob '*.cs'
git diff --check
git status --short
```

Review diffs of matches against this report; grep presence alone cannot enforce call ordering, distinguish PadView.SettingsChanged from PadWindow.SettingsChanged, or bind overloads (`Editor/Motion.cs:240`; `K:264`; `W:360`, `W:379`). For implementation gates use isolated output and manager-run dev checks specified by `docs/agents.md:26-29` and `docs/dev-loop.md:76-79`; this analysis deliberately ran neither.

## 11. Overlap with parallel tasks

These are exact current conflict surfaces; future branches were not supplied. A39 is assumed to land first per brief, but current snapshot still has UI primitives, so its future paths/usings cannot be confirmed (`.coord/tasks/A41-padwindow-split-impact.md:18-21`; `UI/Anim.cs:3`, `UI/PadMenu.cs:11-16`).

| Task | Exact PadWindow members/lines potentially touched | Integration agreement |
|---|---|---|
| A39-motion-out-of-ui: Anim | Constructor `W:43`; ShowResident dropdown `W:151`; PlayShow `W:290`; `_fade`/FadeWindow `W:294-302`; ApplyAppearance `W:506`; `_chromeAnim` `L:49`; SlideBar `L:324-325`; FloatPager `L:332-334`; FadeChrome `L:363`; Toast `L:389`; HideToast `L:398`; Bump `L:488`; ShowOverlay `K:122`. | Accept landed namespaces/types first, preserve animation semantics and decorative flags. A41 should not move these window bodies. |
| A39-motion-out-of-ui: ChromeTokens | Layout field initializers `L:21`, `L:24`, `L:26-30`, `L:34-35`, popup `L:42`; RebuildChrome `L:148-151`; UpdateChrome `L:167-168`; Chip `L:182-186`; BuildPill `L:192-218`; ApplyBars `L:253`, `L:256`, `L:264`, `L:274`; OverlayPointer `L:289`; BottomZone `L:296`; BarsSleepTick `L:308`; InitializeTopBlurVisuals `B:99`, `B:106`; ApplyTopBlur `B:194`; TopBlurMask `B:220-223`. | Mostly imports/type resolution after relocation. Leave locked measurements unchanged (`docs/decisions.md:7-16`). |
| A39-motion-out-of-ui: MenuEntry / LAYERS | NewPane AppMenu separator `L:124` and target-typed entries `L:118-127`; new type's signature flows through PadView.AppMenu (`Editor/PadView.cs:78`). No direct LAYERS member currently in PadWindow. | Extend A39 table for new core/controller types instead of a second competing architecture test. |
| A40-settings-change-event | `Settings` facade `W:36`; ApplyAppearance `W:503-524`; ResetSettings/local Apply `K:245-259`; SettingsChanged whole method `K:264-270`; registrations `W:430-481`; InstallHooks `W:105-108`; potential event wiring in constructor `W:38-99` and teardown `W:270-282`. Direct writes that need a notification policy: RememberBounds `W:420-422`, BindPanes slot repair `N:27`, Split settings `N:149-163`, CycleSplitSlot `N:173`, TogglePin `N:180`, Zoom `K:104`; SearchGrid is written outside the partial at `UI/SearchOverlay.cs:391-392`. Note/selection mutations in `N:39-265` are StoreData changes, not Settings changes, and should not automatically become Settings.Changed events. | Keep window reaction (Topmost -> ensure/apply presence -> appearance -> Edited) in same order; avoid duplicate registration/persistence when one change fires. Settings replacement must detach/reattach or use stable owner event (`K:254-258`). A40 definite call replacement outside window: SettingsWindow `:128`, `:514`. Exact event owner/payload is uncertain. |
| A42-padview-commands | NewPane event/AppMenu wiring `L:113-128`; window keyboard reading `.Editor.Text` and RunOcr target `K:92-93`; zoom ApplyLook `K:105`; BindPanes `N:23-35`; navigation Flush/Forget/DocumentFor/Arrive throughout `N:45`, `N:55`, `N:79`, `N:105`, `N:117`, `N:130`, `N:136`, `N:147`, `N:170-175`, `N:191-205`, `N:220`, `N:254`; clipboard DocumentFor `N:310-312`; OCR CaptureOcrTarget/OcrIntoTarget `N:352-398`; FocusActive `W:305`; session flush `W:248-249`; QueueSave/Save cache adapter `W:365-391`; trim `W:408`; Bump `L:486-488`; TopBlur raw editor accesses `B:159-162`, `B:242-244`, `B:278-279`. | Commands may reduce active-editor reads and cache plumbing, but geometry/blur sampling needs presentation access. A42 brief not present: these are dependency contact points, not an assertion it will edit every line. Assign cache/append/flush command ownership before navigation or clipboard extraction. |
| A43-app-context | `_timers` initializer `W:21`; Deactivated/Activated dev logging `W:72`, `W:84`; ShowResident log `W:144`; PrepareForSessionEnd logs/flush `W:252-259`; CompleteQuit log flush `W:274`; taskbar callback/blur diagnostics `W:322`, `W:336`, `W:341`, `W:344`; TrimWhileHidden diagnostics `W:414`; RebuildChrome dev badge `L:139-140`; OnWindowKeyDown memory probe `K:35-39`; NewSettingsTrim diagnostics `K:197-200`; CycleSettingsForDev `K:209`, `K:214`. | Inject Store/context/liveness/logger into eager-owned collaborators; do not copy Program statics into extracted core. Program composition changes `Program.cs:344-360`; no direct Program reference in N or B today. AppInfo is a separate identity helper, not automatically part of this task. |
| A44-platform-seam | InstallHooks clipboard listener `W:107`; PlaceForOpen cursor `W:162`; TrimWhileHidden `W:413`; BarsSleepTick cursor `L:305`; OnClipboardChanged privacy `N:303`; OnTimerFinished chime `N:321`; RunOcr snip `N:341`; NewSettingsTrim trim `K:196`; OnFloatPagerOpened decoration `B:300`. Related platform types (not `Native.*`): GlobalHotkey creation `W:438`, `W:476`; TaskbarProxy creation `W:317`; SessionEnd gates/closing `W:71-90`, `W:125`, `W:213`, `W:227`, `W:241`, `W:364-383`; Ocr calls `N:294`, `N:361`. | A44 owns replacement adapters/interfaces for native calls; A41 supplies callbacks/ownership, not a duplicate Native facade. Distinguish explicit Native call scope from broader platform dependencies; no reason to migrate frame styles. |

### Verification performed

Read-only inventory/call-site searches covered every C# file under `product/AntiNotepad`; method declarations in all five partials were cross-checked against the index above. Reference endpoints were checked against current file lengths. These checks validate this report's indexing, not runtime behavior (`W:18`; `L:17`; `N:12`; `K:9`; `B:11`). Final git status is recorded below; no build or selftest was run.

Final read-only commands: `git diff --check` reported no tracked diff errors; `git diff --name-only` was empty. `git status --short --untracked-files=all`:

```text
?? .coord/reports/A41-padwindow-split-impact.md
?? .coord/tasks/A41-padwindow-split-impact.md
```

The second line was present before this task. Only the report was added by the analysis; no claim is made that the worktree contains only one untracked file. This follows the report-only restriction rather than deleting the brief (`.coord/tasks/A41-padwindow-split-impact.md:4-5`, `:47-48`).

## Uncertain

- Full Win32/Avalonia event interleaving, actual focus attainment, popup activation, DWM corners/shadow, first-show flicker and latency cannot be confirmed from source alone; source describes intent and guards (`W:69-96`, `W:123-223`; `Platform/TaskbarPresence.cs:196-250`, `:396-453`; `docs/decisions.md:17-27`).
- The stand-in likewise relies on native handle creation through TryGetPlatformHandle and native ShowWindow, without `_window.Show()` in its constructor/presence methods. Whether the intended runtime creates that handle at this point needs a live taskbar check; do not rewrite the lifetime based on this uncertainty alone (`Platform/TaskbarPresence.cs:115-151`, `:160-194`).
- Hidden startup hooks depend on a valid native handle before visible Show. GlobalHotkey retries on Opened; clipboard listener simply returns a no-op disposer for zero handle. Whether that happens in this runtime was not tested (`Platform/GlobalHotkey.cs:41-55`; `Platform/Native.cs:19-30`; `Program.cs:359-362`).
- A prewarm Background callback has no generation cancellation; rapid user show before that callback is a plausible ordering hazard, not an observed failure. Preserve existing behavior and include a manual check (`W:181-193`, `W:123-146`).
- Actual Windows restart/sign-out saving remained listed as not live-checked in STATUS; this analysis cannot close it (`.coord/STATUS.md:34`; `Program.cs:346-352`; `W:239-259`).
- A39/A40/A42/A43/A44 final APIs, namespaces and exact edits are unavailable here. Section 11 maps current dependencies and distinguishes definite brief-driven changes from possible contact surfaces (`.coord/tasks/A41-padwindow-split-impact.md:18-21`; `K:264-270`; `Editor/PadView.cs:71-78`).
- Clipboard/OCR overlapping notifications can interleave across awaits; code selects text target before reading text and saves `_lastCaptured` after awaiting, with no serialization. No runtime race was demonstrated; adding one would need a deliberate behavior decision (`N:292-315`).
- Windows private-format/image/OCR behavior and shell taskbar messages require real platform integration; pure policy tables validate only decisions (`Platform/Native.cs:35-54`; `N:294-305`; `Platform/TaskbarPresence.cs:265-315`; `SelfTest.cs:655-724`).
- Ordinary quit does not explicitly unsubscribe the static Completion.OcrRequested handler or stop/cancel every owned timer/swipe, and color-change/size handlers include anonymous subscriptions. The process normally exits through lifetime callback; repeated in-process window construction/disposal is not proven safe and should not become an implicit new supported lifecycle (`W:63-64`, `W:99`, `W:270-282`; `N:277`; `B:72-81`, `B:304-311`).
- Final git-status exclusivity has a pre-existing exception: the task brief was untracked before analysis. It was not removed to manufacture a clean result; only this report was authored (`.coord/tasks/A41-padwindow-split-impact.md:4-5`, `:47-48`; initial/final git status captured by the analysis tools).
