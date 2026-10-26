# A40-settings-change-event-impact

Read-only source audit, 2026-10-25. Branch `reorg/A40-settings-change-event`, HEAD `4689a14`, product `product/AntiNotepad`. All product references below are relative to that directory. No build, selftest, app launch, or installed-copy access. Only this report was written; the task's explicit write restriction overrides the normal STATUS update rule.

## Impact radius and finding

Recommend a small settings owner with a batched `Changed` event and explicit change flags. Keep `Settings` a JSON POCO. An event alone cannot enforce ownership while callers retain a mutable reference: guard direct setters and mutable-list edits, and preserve specialized navigation, hotkey validation, persistence, and focus behavior.

There are **56 serialized mutable properties**, not 55 (`Model/Store.cs:269-325`), plus the replaceable `StoreData.Settings` property (`:345`). Settings touch 20 source files including Store and SelfTest. The detailed per-file read/write inventory is below; counts mean static source occurrences, never runtime invocations. Object transfer, serializer-wide operations, list mutation and initializers are counted separately to avoid calling a whole-object read 56 independent reads.

Context read: `feat/A38-mvvm-audit:docs/A38-mvvm-audit.md` and `reorg/A39-motion-out-of-ui:.coord/tasks/A39-motion-out-of-ui.md`. A39's branch resolved to `8164c331`; its current `SelfTest.cs` contains no LAYERS implementation. Proposed rules below target the **brief's** table-driven type/IL-token mechanism, assuming A39 lands first. Current line references use this checkout's pre-move paths.

## Coupling map and propagation

| Owner/caller | Reads/writes | Reaction and boundary |
|---|---|---|
| `Store` | Owns Data, JSON load, normalize, clone/save | No change event. Load replaces Data/settings; snapshots clone settings (`Model/Store.cs:528-618,797-835`). |
| `PadWindow` | Dynamic `Settings => _store.Data.Settings` | Appearance, focus, geometry, hotkeys, taskbar, timers; many specialized write paths bypass SettingsChanged (`UI/PadWindow.cs:36`). |
| `SettingsWindow` + `SettingsParts` | Dynamic `S => _pad.Settings`; 51 direct setter sites plus language-list edits | `Set(Action)` and `Store(Action)` manually call the pad; Set also updates its own preview/page (`UI/SettingsWindow.cs:49,125-134,514`; `UI/SettingsParts.cs:363`). |
| `PadView` / editor helpers | Retained settings reference; delegates resolve that reference | ApplyLook replaces reference, resets caret, font/margins/highlighting/redraw; render/input paths poll other properties (`Editor/PadView.cs:64,82,110,175-195`). |
| `MotionPreview` | Retained `_s`, retained PadView | Updated from SettingsWindow; shares global motion/code-language state, owns separate notes/timers (`UI/MotionPreview.cs:91,103-136,201-205`). |
| `NoteSwipe`, `SmoothScroll` | `Func<Settings>` | Read at input/tick time; no settings notification (`UI/NoteSwipe.cs:15,44,145-147`; `Editor/Motion.cs:29-40,54-177`). |
| `PadMenu`, `SearchOverlay` | Menu receives reference; search reads pad dynamically | Menu options read during menu construction; SearchGrid mutates then reopens search (`UI/PadMenu.cs:25-79`; `UI/SearchOverlay.cs:37,389-392`). |
| `Program`, `Welcome` | Startup Material; Welcomed read/write | Before UI exists, no propagation expected (`Program.cs:88-112`; `Model/Welcome.cs:8-17`). |

### Central chain and static call counts

- `PadWindow.SettingsChanged`: **3 call sites**: SettingsWindow.Set `UI/SettingsWindow.cs:128`, Store helper `:514`, reset/undo local Apply `UI/PadWindow.Keys.cs:258`. Order: `Topmost` → ensure taskbar proxy unless never → `ApplyTaskbarSetting` → `ApplyAppearance` → `Edited` (`UI/PadWindow.Keys.cs:264-270`). It does **not** register hotkeys or bind panes.
- `ApplyAppearance`: **3 call sites**: constructor `UI/PadWindow.cs:59`, posted system color-values change `:99`, SettingsChanged `UI/PadWindow.Keys.cs:269`. Order at `UI/PadWindow.cs:505-524`: main palette → Anim.Reduced → theme/material/tint/scale → global disabled-language hash → left/right ApplyLook → split background → conditional input styles → RebuildChrome → ApplyBars → ApplyTopBlur → reopen overlay → settings SyncLook.
- `PadView.ApplyLook`: **8 call sites**: own constructor `Editor/PadView.cs:159`; main/split appearance `UI/PadWindow.cs:516,517`; zoom twice `UI/PadWindow.Keys.cs:105`; reset twice `:255`; preview `UI/MotionPreview.cs:204`. `SmoothCaret.SettingsChanged` has **1 call site**, `Editor/PadView.cs:178`; it resets blink/position (`Editor/Motion.cs:240-249`).
- `MotionPreview.ApplyLook`: **3 call sites**, its constructor `UI/MotionPreview.cs:136` and settings Restyle/Set `UI/SettingsWindow.cs:117,131`. SettingsWindow's own ApplyLook has **4 sites**, constructor/Restyle/Set/Build `:66,116,130,145`; SyncLook is called once from pad appearance (`UI/PadWindow.cs:524`), Refresh once from reset (`UI/PadWindow.Keys.cs:259`).
- `ApplyTaskbarSetting`: **6 sites** in `UI/PadWindow.cs:108,134,191,205,221` and `UI/PadWindow.Keys.cs:268`. No setting changes at the five initialization/show/hide sites: these are necessary visibility reactions. Constructor and OS theme changes likewise propagate without a Settings mutation.
- Palette resolution: main constructor and appearance (`UI/PadWindow.cs:44,505`); split palette `:501,517,518` (potentially **two resolutions per appearance**, one right-pane palette and one background input). Palette base/accent swatches also build in Settings (`UI/SettingsWindow.cs:299,301,322`; `UI/SettingsParts.cs:366`). No background palette worker.

### Secondary propagation / polling sites

| Helper | Call sites (definitions excluded) |
|---|---|
| RebuildChrome (2) | `UI/PadWindow.cs:520`; `UI/PadWindow.Navigation.cs:182` |
| ApplyBars (1), ApplyTopBlur (3) | Bars: `UI/PadWindow.cs:521`; blur: `UI/PadWindow.cs:522`, `UI/PadWindow.Layout.cs:279`, `UI/PadWindow.TopBlur.cs:89` |
| EnsureTaskbarProxy (2) | `UI/PadWindow.cs:105`; `UI/PadWindow.Keys.cs:267` |
| ReopenOverlay (1) | `UI/PadWindow.cs:523`; reconstructs current search/help/trash content (`UI/PadWindow.Keys.cs:139-152`) |
| ApplyMargins (3), ApplyCodeHighlighting (2) | Margins: `Editor/PadView.cs:135,192,877`; highlighting: `:193,530` |
| UpdateEdgeFade (6) | `Editor/PadView.cs:130,131,133,134,409,900`; scroll/document/visual-line/size/margin changes re-read EdgeFade |
| StyleFoldingMargin (1) | `Editor/PadView.cs:296`, note Bind only |
| WakeChrome (6), SleepChrome (4) | Wake: `UI/PadWindow.cs:67,85,154`, `UI/PadWindow.Layout.cs:104,105`, `UI/PadWindow.Keys.cs:121`; sleep: `UI/PadWindow.cs:68,357`, `UI/PadWindow.Layout.cs:106`, `UI/PadWindow.Keys.cs:59` |

### All property groups: who reacts and when

| Properties (56 total; disjoint groups) | Existing reaction |
|---|---|
| Theme, LightPalette, DarkPalette, Accent, Tone, SplitPalette (6) | Set → full manual chain; palettes/styles/chrome/editors/settings restyled. OS theme/high contrast independently invokes appearance. Palette picks may also change Theme in the **same** Set batch (`UI/SettingsWindow.cs:298-302`; `UI/PadWindow.cs:485-524`). |
| Material, Tint, UiScale (3) | Full manual chain: transparency hint guarded by previous material, tint/scaler/blur updated, Settings own look/preview refreshed. GPU/software renderer changes **only next process start** (`Program.cs:111-112`; `UI/SettingsWindow.cs:362-367`). |
| Font, FontSize, TextWidth, LineSpacing, Paper, PaperStrength, Bubbles (7) | Set → appearance → editor ApplyLook/margins/redraw; paper/bubbles read during drawing (`Editor/PadView.cs:175-195,884-906`; `Editor/Renderers.cs:109-122,250-278,395`). Zoom has its own shorter path. |
| InlineMath, ShortenLinks, CleanMarkdown, IndentGuides, Folding, DisabledLanguages (6) | Full chain redraws and reapplies highlighting/global disabled-language hash (`UI/PadWindow.cs:515`; `Editor/PadView.cs:193-194,920`). Links/markdown/guides poll in rendering. **Gap:** InlineMath analysis updates only on Analyse (`:517`); ApplyLook does not Analyse. Folding install/uninstall only on note Bind (`:287-294`), so the manual call does not fully apply these settings immediately. |
| CaretStyle, CaretBlink, CaretGlide (3) | Full chain → each PadView's caret SettingsChanged; focus/tick/render/move also poll (`Editor/Motion.cs:235-313`). |
| ReduceMotion, Scrolling, LinesPerNotch, ScrollDistance, ScrollBoost, FreeFriction, Swipe, SwipeLength, SwipeFiresAt (9) | Set does full chain (and preview). Anim.Reduced updated first; scroll/swipe read settings on subsequent input/ticks (`Editor/Motion.cs:54-177`; `UI/NoteSwipe.cs:44,145-147`). No general cancel/restart of in-flight motion. |
| Chrome, FadeAfterMs, Bars, EdgeFade, MenuActionRow, MenuColorIcons (6) | Full chain rebuilds/reparents bars, margins, blur, chrome timing. Chrome sleep reads at `UI/PadWindow.Layout.cs:342-349`; edge fade on render (`Editor/PadView.cs:317`). Menu flags read when next menu is built (`UI/PadMenu.cs:62-79`); no explicit restyle of an already open PadMenu. |
| Hotkey, NewNoteHotkey (2) | Settings editor registers **before** mutation; only successful registration calls Store helper (`UI/SettingsWindow.cs:475-479`). Reset explicitly registers after replacement (`UI/PadWindow.Keys.cs:254-258`); startup InstallHooks reads them (`UI/PadWindow.cs:106-107`). SettingsChanged alone does not register. Tray tooltip follows HotkeyStatusChanged (`Program.cs:358`; `UI/PadWindow.cs:440`). |
| FreshNoteAfterMinutes, HideOnBlur, DimWhenPinned, OpenAt, Pinned, Taskbar (6) | All Set → manual chain. Pinned/Taskbar apply immediately. Others poll at next show, deactivation or delayed focus check (`UI/PadWindow.cs:78-79,129,161-175,339`; `UI/PadWindow.Keys.cs:235-239`). Changing dim/hide does not itself synthesize focus loss. TogglePin uses its own Topmost/chrome/save path (`UI/PadWindow.Navigation.cs:178-184`). Tray always exists; Taskbar never is not a tray-disable choice (`Program.cs:355-359`). |
| Width, Height, X, Y (4) | RememberBounds writes actual window state and only restarts save timer (`UI/PadWindow.cs:417-422`). Read initially (`:45-47`); reset preserves them. Generic appearance does not apply stored bounds back to window. |
| Split, SplitSlot (2) | Navigation mutations explicitly resize/bind panes; BindPanes may repair a same-slot conflict. No SettingsChanged (`UI/PadWindow.Navigation.cs:21-32,146-175`); setters alone do not update pane binding. |
| SearchGrid (1) | Toggle → reopen search, **no Edited/save request** (`UI/SearchOverlay.cs:389-392`). Persisted only when another save captures it. |
| Welcomed (1) | Startup seed writes once; reset forces true. No live UI notification required (`Model/Welcome.cs:10-11`; `UI/PadWindow.Keys.cs:248`). |

Other settings-like state is outside the POCO: startup registration reads/writes HKCU synchronously (`UI/SettingsWindow.cs:504-505`; `Platform/Native.cs:128-170`); notes-folder pointer/migration below. A settings event must not pretend these are JSON properties.

## SettingsWindow member coupling (both partial files)

Counts are syntactic member uses, including property reads and method-group references. `_pad` here is the SettingsWindow field, not MotionPreview's separately named PadView field.

| Target/member | Count | Source |
|---|---:|---|
| PadWindow.Settings | 1 | `UI/SettingsWindow.cs:49` |
| PadWindow.CurrentPalette | 6 | `UI/SettingsWindow.cs:54,96,97,110,129` (twice at 129) |
| PadWindow.Icon | 1 | `UI/SettingsWindow.cs:56` |
| PadWindow.SettingsLostFocus | 2 | `UI/SettingsWindow.cs:77,78` |
| PadWindow.SettingsChanged | 2 | `UI/SettingsWindow.cs:128,514` |
| PadWindow.HotkeyStatus / RegisterHotkey | 1 each | `UI/SettingsWindow.cs:475` |
| PadWindow.NewNoteHotkeyStatus / RegisterNewNoteHotkey | 1 each | `UI/SettingsWindow.cs:476,479` |
| PadWindow.Toast | 3 | `UI/SettingsWindow.cs:505,760,765` |
| PadWindow.ShowResident / OpenHelp | 1 each | `UI/SettingsWindow.cs:568` |
| PadWindow.ResetSettings / Save | 1 each | `UI/SettingsWindow.cs:569,665` |
| MotionPreview constructor | 1 | `UI/SettingsWindow.cs:65` |
| MotionPreview.ApplyLook | 2 | `UI/SettingsWindow.cs:117,131` |
| MotionPreview.Release / FocusEditor / Parent / UseSamples | 1 each | `UI/SettingsWindow.cs:78,105,771,772` |
| PadMenu | 0 | Neither Settings partial references it directly. Preview editor builds its own menu through PadView (`Editor/PadView.cs:754`). |
| Anim.Reduced / Run / EaseOut | 1 each | `UI/SettingsWindow.cs:235,240,240` |
| Kit.Material / InputStyles / IconButton / Toggles | 1 each | `UI/SettingsWindow.cs:86,88,156,413` |
| Kit.Text | 14 | `UI/SettingsWindow.cs:152,202,289,524,530,545,582`; `UI/SettingsParts.cs:134,182,201,226,246,266,313` |
| Kit.Plain | 8 | `UI/SettingsWindow.cs:203`; `UI/SettingsParts.cs:143,184,217,232,248,271,317` |
| Glyph.Close / Fluent | 1 / 5 | `UI/SettingsWindow.cs:156,169,201`; `UI/SettingsParts.cs:132,229,407` |
| GlyphIcon constructor | 5 | `UI/SettingsWindow.cs:169,201`; `UI/SettingsParts.cs:132,229,407` |
| WindowDecorationProperties.SetElementRole | 1 | `UI/SettingsWindow.cs:149` |

PadWindow member uses total **23** (14 distinct members). SettingsWindow additionally depends on Editor.CodeColors.Languages (4 uses, `UI/SettingsWindow.cs:402,412` twice, `:413`), Theme.Palette, root Program storage/runtime state, Startup, and standard Avalonia controls. Its private controls/pictures/helpers are members of the same partial class, not independent service dependencies. SettingsParts holds all 3 of its settings reads and its 1 setter through S; no extra PadWindow member reference.

## Store lifecycle, identity and threads

- **First load/defaults:** Store initially allocates Data and its Settings (`Model/Store.cs:345,374`). Program loads before Avalonia (`Program.cs:88-112`). Load drains writer first; forced/unavailable folder returns without replacement (`Model/Store.cs:530-537`). Tries index then `.bak`, rejecting incomplete/null Settings (`:543-578`). Successful JSON deserialize **replaces Data and Settings** (`:548-549`); failing readable candidates/legacy yields new StoreData (`:567`). Missing JSON members retain POCO initializer defaults; explicit null Settings invalidates the candidate.
- **Migration:** v1 deserializes StoreData, migrates embedded note bodies, replaces Data (`Model/Store.cs:585-595`), normalizes, and saves after load (`:574`). Normalize changes Font alias and clamps SplitSlot **in place**, without notifications (`:611,618`). No generic reflection mutation (`GetProperties`/`PropertyInfo.SetValue`) was found in product; implicit setters are System.Text.Json's load/undo paths.
- **Reset is not in Store:** PadWindow.ResetSettings serializes old settings for undo, makes fresh defaults preserving Width/Height/X/Y/Split/SplitSlot and forcing Welcomed true; Apply replaces `Data.Settings`, updates both pane references, registers both hotkeys, runs central chain, refreshes settings (`UI/PadWindow.Keys.cs:245-259`). Undo deserializes a **new** Settings and repeats Apply. Pinned and SearchGrid are reset. No ResetSettings API exists on Store.
- **Who can retain old instances:** PadView.Settings (`Editor/PadView.cs:64,82,177`), MotionPreview._s and its editor (`UI/MotionPreview.cs:91,105,203-204`), plus already-created menu/build call arguments. PadWindow.Settings and SettingsWindow.S are dynamic getters. SmoothScroll's delegate reads PadView.Settings and NoteSwipe's delegates read pad/preview dynamically. Reset explicitly refreshes retained panes/preview; a later bare Store.Load after constructing UI would replace Data without any notification/rebind. No normal runtime reload after UI construction was found (`Program.cs:99`). Save snapshots intentionally keep separate historical clones.
- **Save layers:** pad's UI DispatcherTimer is a **400 ms trailing debounce**, reset by Edited/RememberBounds; deactivate/hide can queue sooner (`UI/PadWindow.cs:22,61,73,217,353-368,417-422`). QueueSave flushes editor analysis first, then captures a snapshot synchronously before returning its task (`Model/Store.cs:648-651,797-835`). CloneData copies metadata and Settings.CopyForSave; MemberwiseClone plus separate DisabledLanguages list (`:327-331,834`).
- **Writer:** `_saveGate` protects pending batches/body revisions/completions; Task.Run starts a single writer (`Model/Store.cs:734-749`). It waits up to **50 ms per pending batch**, not a restarted trailing debounce; replacements replace the batch snapshot (`:745,771,943`). It removes pending batch under lock, writes outside lock, serializes **snapshot.Data.Settings**, and queues completion (`:775-793,873`). New batches can follow an active write. Only disk work is offloaded; settings copy remains on caller thread.
- **Sync Save/Flush:** Save captures latest state, forces batch now and waits; Flush drains without a fresh snapshot (`Model/Store.cs:660-718`). Unavailable saves are successful no-ops; failure/dirty retention differs from a successful durable write (`:648-695,721-731,851-893`). Atomic index replace/backup is `:971-976`. Completed writes only clear dirty bodies matching captured revision (`:896-913`); they do not replace current Settings.
- **Folder move:** Settings first calls pad.Save, then Program.MoveNotes calls Store.Save again (`UI/SettingsWindow.cs:663-680`; `Program.cs:174-183`). NotesLocation.Move copies/verifies/updates pointer and handles backup (`Platform/NotesLocation.cs:81-124,187-249`). Store.SwitchFolder drains, changes paths/cache/availability only; it **does not Load or replace Data/Settings** (`Model/Store.cs:948-961`). No settings event is needed merely for changing the storage location, but storage UI/error state must still refresh.
- **Thread contract:** startup main thread loads/seeds/settings renderer read; subsequently Avalonia UI event handlers, DispatcherTimers, animation frames and posted OS/hotkey callbacks read/write live Settings (`Program.cs:99-111,121,130`; `UI/PadWindow.cs:61-99,438`; `Editor/Motion.cs:217-248`). No live-settings read on background save writer: it serializes detached clone. `_saveGate` does **not** synchronize arbitrary POCO setters/list edits with snapshot capture; safety depends on UI-thread mutation. CompleteQueuedSaveAsync captures calling synchronization context when present (`Model/Store.cs:753-757`); headless callers may complete on a pool thread, where completion applies note metadata, not live Settings. A new owner should enforce UI-thread updates or serialize mutation/capture explicitly; never invoke UI subscribers from writer callbacks.

## Risks and hidden behavior

1. **Partial propagation already exists.** Full manual notification still leaves Folding until note Bind and InlineMath until analysis, as above (`Editor/PadView.cs:175-195,287-294,517`). Bind returns early for the same Note (`:267`), so merely requesting the same note does not recreate folding. Treat this as identified behavior, not permission to silently fix it during a behavior-neutral extraction.
2. **Re-entrancy and duplicate work:** Set → pad.SettingsChanged → ApplyAppearance → settings.SyncLook → Restyle when palette differs, before Set resumes. Set then applies preview/look again because SyncLook has already updated `_p` (`UI/SettingsWindow.cs:94-98,125-134`; `UI/PadWindow.cs:524`). Preserve page/query/scroll behavior (`UI/SettingsWindow.cs:118-121,132-134`). A naive event subscriber per setter can expose intermediate LightPalette/Theme combinations (`:300,302`) and recur through pane-binding conflict repair (`UI/PadWindow.Navigation.cs:27`). Queue nested updates, then emit one ordered follow-up batch; do not recursively dispatch while a batch is active.
3. **Observer order matters:** set Anim.Reduced and disabled-language global state before updating panes/preview; palette must be ready before their look; hotkey validation before commit; taskbar/Topmost before chrome/save (`UI/PadWindow.cs:505-524`; `UI/SettingsWindow.cs:475-479`; `UI/PadWindow.Keys.cs:266-270`). Use ordered owner effects or a precomputed presentation snapshot; arbitrary subscription order is insufficient.
4. **Fast show/hide must stay fast:** position/size notifications write X/Y/Width/Height (`UI/PadWindow.cs:97-98,417-422`); OpenAt positioning happens on show (`:132,159-176`). Broadcasting full appearance here would allocate palettes, brushes, FontFamily, disabled-language sets, rebuild chrome, redraw both panes/reopen overlays, and potentially fire geometry again (`:505-524`; `Editor/PadView.cs:175-195`). Filter geometry/persistence-only updates; never route visibility/taskbar reactions through an all-settings event. Same-value/no-op batches should emit nothing.
5. **Motion semantics are mixed:** Anim.Run tests Reduced only on start; an already running frame closure does not recheck (`UI/Anim.cs:12-31`). SmoothScroll reads Reduced on input/line mode, caret on move (`Editor/Motion.cs:58,75,273`); swipe still handles input and settlement with settings polling (`UI/NoteSwipe.cs:44,145-156`). Bar reveal checks Reduced explicitly (`UI/PadWindow.Layout.cs:324,332`); FadeWindow deliberately passes `decorative:false` (`UI/PadWindow.cs:302`) and still animates. A new event should not accidentally cancel all in-flight motion or promise immediate cancellation. A39 relocates Anim; semantics should remain.
6. **Hidden/open windows:** pad stays alive hidden; SettingsChanged applies appearance regardless of visibility (`UI/PadWindow.Keys.cs:264-270`). OpenSettings first shows pad (`:158-174`). Pad deactivation saves but skips hide/dim while settings exists (`UI/PadWindow.cs:73-79`); settings close/loss posts a fresh focus check (`UI/PadWindow.Keys.cs:235-239`). Keep these focus commands outside settings ownership. Unsubscribe preview/settings handlers on close to preserve collection/trim (`UI/PadWindow.Keys.cs:185-200`; `UI/MotionPreview.cs:183-198`).
7. **Live preview is shared behavior:** preview keeps sample documents and its own timers, moves between Editor/Feel pages, ApplyLook restyles in place (`UI/MotionPreview.cs:103-136,183-205`; `UI/SettingsWindow.cs:65,117,131`). No preview-only Settings copy: changes are real/persisted, and animation/global disabled languages are shared. UI-scale affects pad scaler; Settings uses its own fixed-density look (`UI/PadWindow.cs:513-514`; `UI/SettingsWindow.cs:89-90`).
8. **Missed central calls versus intentional specialized paths:** zoom, geometry, split/slot navigation, TogglePin, SearchGrid, Welcome and normalization do not call SettingsChanged (`UI/PadWindow.Keys.cs:102-107`; `UI/PadWindow.cs:417-422`; `UI/PadWindow.Navigation.cs:27,149-184`; `UI/SearchOverlay.cs:391`; `Model/Welcome.cs:11`; `Model/Store.cs:611,618`). Most have explicit local reactions; SearchGrid has no save scheduling, and CycleSplitSlot lacks Edited (`UI/PadWindow.Navigation.cs:169-175`). Do not add full appearance blindly. Conversely Set/Store always propagate even assigning the same value (`UI/SettingsWindow.cs:125-134,514`); system color events/init/show/hide legitimately propagate with **no setting change**.
9. **Lists and serializer setters bypass simple setter guards:** Remove/Add inside DisabledLanguages and snapshot deep-copy are essential (`UI/SettingsWindow.cs:414`; `Model/Store.cs:329-330`). Reflection/JSON bypass source-only grep; loading/reset should publish at most one whole replacement after normalization, not one event per deserialized property.
10. **Rebuild lifecycle:** Build creates a fresh 150 ms search debounce timer and a Closed handler on every rebuild (`UI/SettingsWindow.cs:165-181`); it does not stop the previous timer during rebuild. Restyle restores a query (starting its timer) and immediately builds results (`:118-121`). More broad Changed callbacks can multiply these rebuilds. Preview detach stops sampling/swipe (`UI/MotionPreview.cs:140-141`), but a queued frame has only `_sampling`, no generation token (`:163-176`); a detach/reattach before it runs could resume an old chain. These are source-derived risks, not reproduced failures.

## Design options and recommendation

| Option | POCO and batch handling | Tradeoff |
|---|---|---|
| **A. SettingsService owner (recommended)** | Holds current plain Settings; `Update(Action<Settings>)` captures before/after values (including a copied language list), diffs once and emits `Changed(flags, propertyNames?)` once after outermost batch. `Replace` handles reset/load and refreshes all retained references. Save scheduling is attached once at composition root. | Explicit seam for settings UI, pad/editor effects and tests. Use flags for Appearance/EditorAnalysis/Motion/Hotkeys/WindowState/Geometry/Search/Persistence; property names optional diagnostics. Diffing 56 scalar/list properties on actual updates is small; do not serialize JSON for every comparison. Enforce mutation at owner and guard exemptions. |
| B. Store.Changed only | Store wraps Data.Settings with same Update/Replace batching and diff; Settings stays unchanged. | Smaller extraction, but Store mixes disk persistence with live settings protocol. Settings UI can receive the owner interface rather than PadWindow; subscribers still need property categories. Whole-object replacement and UI-thread dispatch remain required. A parameterless Changed over-invalidates unless observers maintain their own snapshots. |
| C. INotifyPropertyChanged-style setters | Putting events in Settings itself violates the requested plain-POCO goal; instead wrap it in a notifying facade with forwarding setters and Begin/EndUpdate, emitting one aggregate Changed per batch. JSON reads/writes only underlying POCO. | More duplicated surface for 56 properties; callers can still bypass wrapper/list mutation. Per-property notifications during JSON construction are avoided only with facade or explicit suppression. No MVVM package/bindings needed, but little benefit over A here. |

Recommended A: SettingsWindow accepts a small owner plus explicit host commands/services for hotkey validation, toast/reset-undo, storage and focus/show. A settings event **alone cannot remove all PadWindow coupling**: those are separate app/window capabilities. Presentation palette should be supplied by an appearance source, icon by app identity, not obtained through PadWindow. Do not introduce a namespace-wide UI→UI prohibition; Kit/preview/menu composition is valid. Keep hotkey reject/rollback transactional, keep reset/undo preserved fields, and separate persistence-only geometry from look changes. Do not make every note save originate from SettingsService: notes still require Store.Save/QueueSave independent of settings changes.

## Proposed LAYERS / selftests (after A39)

- Extend A39's rule table to accept exact source types as well as namespace prefixes: `UI.SettingsWindow` **including all partials and generated closures** → `UI.PadWindow` forbidden. Also `Model.SettingsService` → UI/Editor forbidden; preserve A39's Model→UI/Editor rules. A generic `Action` carrying UI behavior is fine; method signatures/fields/IL tokens must not retain PadWindow.
- Extend the same IL scan with a **member rule table**: resolve calls to `Settings.set_*` and `StoreData.set_Settings`; allow owner methods, owner-generated closures, JSON/default construction and focused SelfTest fixtures only. Report offending method+property. Avoid a blanket selftest namespace exemption in production checks. Today callers contain mutation lambdas, so a lexical ban would reject Update's lambda too: design a closed writer facade, or narrowly validate mutations occur in callbacks passed to owner.Update. A39's simple type-edge checker alone cannot prove that control-flow property.
- Guard mutating language-list calls (or change owner API to `SetLanguageEnabled`), since getter + List.Add/Remove has no Settings setter token. JSON reflection is outside the app IL token scanner; supplement with a whitelist of deserialize/replace entrypoints and round-trip tests.
- For saving, forbid **SettingsWindow/SettingsParts → Store.Save/QueueSave/Flush and PadWindow.Save** member calls; settings changes request persistence through owner/composition root. Do **not** globally forbid PadWindow/Program saves: quit/session end, note edits and folder-move gates need them. If only owner may schedule settings persistence, distinguish that API from generic note-store persistence and guard the former.
- Small behavior check: attach two fake subscribers; Update changing FontSize and Theme emits exactly **one** event with both properties and final values; no-op emits zero; nested Update emits once; list mutation is detected; Replace/reset emits once and Current identity is the new object. A handler requesting another update receives one subsequent batch, never recursive partial state. Verify detached subscriber is not called. Keep these headless and owner-only.
- Targeted regression checks: snapshot language list remains detached (retain existing A30 check); failed hotkey transaction retains saved value and emits no committed change; geometry-only update does not invoke fake appearance subscriber. Add analysis/folding capability checks if fixes are separately authorized. Prove new architecture rules with a temporary violation and restore it during implementation, as A39 requires; no such mutation/test was performed in this read-only task.

## Existing selftests touching settings

| Check / source | Assertion relevant to settings |
|---|---|
| Run store round trip (`SelfTest.cs:48-60`) | Default StoreData/settings serialize with notes and recover backup; no property-by-property notification assertion. |
| DefaultDataMigrationSelfTest (`SelfTest.cs:81-225`) | Moves a literal `settings.json` fixture with default-folder migration; it does not deserialize Settings. |
| NotesLocationChecks (`SelfTest.cs:288-422`, especially `:374-377`) | Null Settings invalidates index and falls back to valid backup; storage move/recovery checks. |
| NoteSwipeChecks (`SelfTest.cs:450-486`) | `new Settings { Swipe = "progress" }`; no-frame cancellation and bounded stalled settlement. |
| WelcomeSlotsSelfTest (`SelfTest.cs:489-515`) | Seed/preserve/idempotence via Welcomed, and defaults Scrolling=rate, Swipe=progress. |
| TaskbarChecks (`SelfTest.cs:655-706`) | Default/empty JSON Taskbar=never; three values round-trip through StoreData; visibility/activation rules. |
| SettingsSearchMatchingChecks (`SelfTest.cs:726-733`) | Case-insensitive settings search, strips internal prefix, null query matches; no settings instance mutation. |
| BackgroundSaveAndLogChecks (`SelfTest.cs:976-1149`, especially `:1038-1050`) | Queued settings snapshot preserves pre-queue language list despite subsequent live Add; other checks cover 40-save coalescing, latest synchronous snapshot, flush/failure/unavailable/move recovery. |

No existing Changed/subscription/one-batch check. All assertions above were read, **not executed**.

## Uncertainties

No live timing/cost, physical motion, focus/taskbar, renderer switch or hidden-window behavior was measured; costs and gaps above are source-derived. A39's finished implementation and its eventual line numbers/rule API are unavailable on the inspected branch. Product has no runtime assertion preventing non-UI-thread setters, so UI confinement is a call-path finding, not a type-system guarantee.

## Detailed reads/writes inventory

Generated from named Settings receivers and manually checked aliases. `R`/`W` count individual property access expressions (multiple accesses on one line count separately); `W` excludes declarations/default initializers and whole-object operations, recorded separately below. List mutator accesses are R to fetch the list plus the listed content mutations. This inventory covers all 56 properties, including those only initialized or consumed during startup.

### Counts per file

| File | Explicit property R | Explicit property W |
|---|---:|---:|
| `Editor/Elements.cs` | 4 | 0 |
| `Editor/Motion.cs` | 13 | 0 |
| `Editor/PadView.cs` | 15 | 0 |
| `Editor/Renderers.cs` | 7 | 0 |
| `Editor/Structure.cs` | 1 | 0 |
| `Model/Store.cs` | 4 | 3 |
| `Model/Welcome.cs` | 1 | 1 |
| `Program.cs` | 1 | 0 |
| `SelfTest.cs` | 7 | 1 |
| `UI/MotionPreview.cs` | 1 | 0 |
| `UI/NoteSwipe.cs` | 3 | 0 |
| `UI/PadMenu.cs` | 4 | 0 |
| `UI/PadWindow.cs` | 43 | 4 |
| `UI/PadWindow.Keys.cs` | 14 | 1 |
| `UI/PadWindow.Layout.cs` | 20 | 0 |
| `UI/PadWindow.Navigation.cs` | 18 | 5 |
| `UI/PadWindow.TopBlur.cs` | 4 | 0 |
| `UI/SearchOverlay.cs` | 2 | 1 |
| `UI/SettingsParts.cs` | 3 | 1 |
| `UI/SettingsWindow.cs` | 74 | 50 |
| **Total** | **239** | **67** |

Additional explicit initializer assignments: reset has **7** at `UI/PadWindow.Keys.cs:248`; SelfTest has **2** at `SelfTest.cs:454,663`. List-content mutation sites: **2** in `UI/SettingsWindow.cs:414` (Remove and Add), **1** in `SelfTest.cs:1043` (Add). Whole-instance Settings setter sites: declaration default `Model/Store.cs:345`, snapshot initializer `:834`, reset/undo replacement `UI/PadWindow.Keys.cs:254`, Taskbar test StoreData initializer `SelfTest.cs:663`, plus JSON deserialization. All 56 defaults are established by each POCO construction/JSON missing-member path; these implicit/default writes are not multiplied into source-access totals.

### Counts per property group

Same direct-property convention as file totals; initializer column is additional explicit assignments, not implicit POCO defaults.

| Group | Properties | R | W | Extra initializer W |
|---|---:|---:|---:|---:|
| Palette | 6 | 22 | 8 | 0 |
| Window appearance | 3 | 21 | 3 | 0 |
| Text and paper | 7 | 26 | 9 | 0 |
| Writing aids | 6 | 23 | 7 | 0 |
| Caret | 3 | 9 | 4 | 0 |
| Motion | 9 | 33 | 10 | 1 |
| Chrome and menu | 6 | 33 | 6 | 0 |
| Hotkeys | 2 | 4 | 2 | 0 |
| Window policy | 6 | 34 | 7 | 1 |
| Geometry | 4 | 9 | 4 | 4 |
| Split | 2 | 22 | 5 | 2 |
| Search | 1 | 2 | 1 | 0 |
| Onboarding | 1 | 1 | 1 | 1 |

### Every explicit property access, by file

Notation: `Property R line,line; W line`. Repeated line numbers are distinct occurrences. Prefix each line number with the file named in its bullet.

- **`Editor/Elements.cs`**: `ShortenLinks R 123,133,135`; `CleanMarkdown R 179`.
- **`Editor/Motion.cs`**: `Scrolling R 54`; `LinesPerNotch R 71`; `ScrollDistance R 86,112,125`; `ScrollBoost R 107,122`; `FreeFriction R 177`; `CaretBlink R 235,248`; `CaretStyle R 262,313`; `CaretGlide R 273`.
- **`Editor/PadView.cs`**: `Font R 180`; `FontSize R 181,189,891`; `LineSpacing R 191,191`; `Folding R 294`; `EdgeFade R 317`; `InlineMath R 517`; `ShortenLinks R 647`; `CleanMarkdown R 647`; `TextWidth R 888,893`; `Bars R 897,905`.
- **`Editor/Renderers.cs`**: `Paper R 109`; `PaperStrength R 115`; `Bars R 122`; `Font R 250`; `FontSize R 252,395`; `Bubbles R 278`.
- **`Editor/Structure.cs`**: `IndentGuides R 456`.
- **`Model/Store.cs`**: `DisabledLanguages R 330,330; W 330`; `Font R 611; W 611`; `SplitSlot W 618; R 618`.
- **`Model/Welcome.cs`**: `Welcomed R 10; W 11`.
- **`Program.cs`**: `Material R 111`.
- **`SelfTest.cs`**: `Scrolling R 512`; `Swipe R 512`; `Taskbar R 659,660,664`; `DisabledLanguages W 1041; R 1043,1049`.
- **`UI/MotionPreview.cs`**: `Material R 205`.
- **`UI/NoteSwipe.cs`**: `Swipe R 44`; `SwipeLength R 146`; `SwipeFiresAt R 147`.
- **`UI/PadMenu.cs`**: `MenuActionRow R 62,71`; `MenuColorIcons R 64,79`.
- **`UI/PadWindow.cs`**: `ReduceMotion R 43,175,506`; `Width R 45; W 421`; `Split R 45,421`; `Height R 45; W 421`; `X R 47; W 420`; `Y R 47; W 420`; `Pinned R 53`; `HideOnBlur R 78,339`; `DimWhenPinned R 79`; `Taskbar R 105,311`; `Hotkey R 106`; `NewNoteHotkey R 107`; `FreshNoteAfterMinutes R 129,129`; `OpenAt R 144,161,163,167,175`; `Theme R 485`; `DarkPalette R 498`; `LightPalette R 498`; `Accent R 498,501`; `Tone R 498,501`; `SplitPalette R 501,501,518`; `Material R 509,509,509,511,518`; `Tint R 511,518`; `UiScale R 513`; `DisabledLanguages R 515`.
- **`UI/PadWindow.Keys.cs`**: `FontSize W 104; R 104,106`; `HideOnBlur R 238`; `DimWhenPinned R 239`; `Width R 248`; `Height R 248`; `X R 248`; `Y R 248`; `Split R 248`; `SplitSlot R 248`; `Hotkey R 256`; `NewNoteHotkey R 257`; `Pinned R 266`; `Taskbar R 267`.
- **`UI/PadWindow.Layout.cs`**: `Split R 122,210`; `Pinned R 123,144,145,145`; `Material R 154`; `SplitSlot R 210`; `Bars R 235,240,241,253,254,259,267,296,317`; `Chrome R 342,349`; `FadeAfterMs R 342`.
- **`UI/PadWindow.Navigation.cs`**: `SplitSlot R 18,27,27,115,171; W 27,155,173`; `Split R 24,30,31,31,32,115,149,151,373; W 149`; `Width R 148`; `Pinned W 180; R 180,181,183`.
- **`UI/PadWindow.TopBlur.cs`**: `Bars R 144,149,202,267`.
- **`UI/SearchOverlay.cs`**: `SearchGrid R 37,391; W 391`.
- **`UI/SettingsParts.cs`**: `Accent R 355; W 363`; `DarkPalette R 366`; `LightPalette R 366`.
- **`UI/SettingsWindow.cs`**: `Material R 86,86,86,90,332,337,365; W 332`; `Tint R 90,338; W 338`; `Theme R 298,300,302; W 298,300,302`; `LightPalette R 299,300; W 300`; `DarkPalette R 301,302; W 302`; `CaretStyle R 310,311; W 310,311`; `CaretBlink R 316; W 316`; `CaretGlide R 317; W 317`; `SplitPalette R 322; W 322`; `UiScale R 340; W 340`; `Chrome R 344,345,346; W 345`; `FadeAfterMs R 347; W 347`; `Bars R 348,354; W 354`; `EdgeFade R 355; W 355`; `MenuActionRow R 356; W 356`; `MenuColorIcons R 357; W 357`; `DimWhenPinned R 358; W 358`; `Font R 376; W 376`; `FontSize R 378; W 378`; `TextWidth R 380; W 380`; `LineSpacing R 382; W 382`; `Paper R 389,395,397; W 389`; `Tone R 394; W 394`; `PaperStrength R 398; W 398`; `DisabledLanguages R 402,414,414,414`; `InlineMath R 404; W 404`; `Bubbles R 406; W 406`; `CleanMarkdown R 407; W 407`; `IndentGuides R 408; W 408`; `Folding R 409; W 409`; `ShortenLinks R 410; W 410`; `Scrolling R 423,423,424,446; W 424`; `Swipe R 431,432,433,434,436; W 433`; `SwipeLength R 435; W 435`; `SwipeFiresAt R 437; W 437`; `ReduceMotion R 442; W 442`; `LinesPerNotch R 448; W 448`; `ScrollDistance R 451,456; W 451,456`; `FreeFriction R 452; W 452`; `ScrollBoost R 457; W 457`; `Hotkey W 475`; `NewNoteHotkey W 479`; `OpenAt R 487,491; W 487`; `Taskbar R 493; W 493`; `FreshNoteAfterMinutes R 495; W 495`; `Pinned R 500; W 500`; `HideOnBlur R 501; W 501`.

### Schema/default initializer sites

`Theme:269`, `LightPalette:270`, `DarkPalette:271`, `Paper:272`, `PaperStrength:273`, `Material:274`, `Tint:275`, `FontSize:276`, `TextWidth:277`, `LineSpacing:278`, `Bubbles:279`, `UiScale:280`, `Font:281`, `Hotkey:282`, `NewNoteHotkey:283`, `FreshNoteAfterMinutes:284`, `HideOnBlur:285`, `DimWhenPinned:286`, `Accent:287`, `Tone:288`, `SplitPalette:289`, `OpenAt:290`, `ReduceMotion:291`, `Scrolling:292`, `LinesPerNotch:294`, `ScrollDistance:295`, `ScrollBoost:296`, `FreeFriction:297`, `SwipeLength:298`, `SwipeFiresAt:299`, `Swipe:300`, `Chrome:301`, `FadeAfterMs:302`, `Bars:303`, `EdgeFade:304`, `MenuActionRow:305`, `MenuColorIcons:306`, `CaretStyle:307`, `CaretBlink:308`, `CaretGlide:309`, `InlineMath:310`, `ShortenLinks:311`, `CleanMarkdown:312`, `IndentGuides:313`, `Folding:314`, `DisabledLanguages:315`, `Pinned:316`, `Taskbar:317`, `Width:318`, `Height:319`, `X:320`, `Y:321`, `SplitSlot:322`, `Split:323`, `SearchGrid:324`, `Welcomed:325`. All references in this paragraph are `Model/Store.cs`.

### Whole-object / reference paths (in addition to property inventory)

- `Model/Store.cs:329-331` clone; `:345,374` allocation; `:548-549,567,586,593` deserialize/replace; `:577` validity read; `:834` clone/transfer; `:873` JSON snapshot read. JSON load can set any of the schema properties; reset undo serializes/deserializes all at `UI/PadWindow.Keys.cs:247,250`.
- `Editor/PadView.cs:64,82,110,159,175-178,754` retained property, constructor/ApplyLook transfer, getter delegate, context-menu argument. `UI/PadWindow.cs:36,516-517`, `UI/PadWindow.Layout.cs:113`, `UI/PadWindow.Keys.cs:105,247-255` dynamic getter/argument transfers. `UI/PadWindow.Navigation.cs:277` delegates to current instance.
- `UI/SettingsWindow.cs:49,65,117,131` getter and preview arguments; `UI/MotionPreview.cs:91,103-110,114,136,201-204` retained reference, constructor/preview apply, swipe getter; `UI/NoteSwipe.cs:15,145` Func and local alias; `Editor/Motion.cs:29,38-40,106,121` Func and local aliases; `UI/PadMenu.cs:25,28,58` parameter/transfer.
- `SelfTest.cs:374,377` null JSON/validity; `:454,456-457,512,659-664` test instances/delegates/JSON transfer; `:1041-1049` snapshot fixture. No singleton/static Settings instance exists: `Program.Store.Data.Settings.Material` at `Program.cs:111` is the direct global access; `Program.Store` is passed to PadWindow at `:344-345`.

