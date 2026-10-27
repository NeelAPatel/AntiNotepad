# A43-app-context-impact

Read-only source analysis, 2026-10-25; `reorg/A43-app-context`, HEAD `b54b47660b4ba8d7a4103b39a6b79c13f105cbf1`.
All code paths below are relative to `product/AntiNotepad/`; line numbers describe this HEAD, before A39-motion-out-of-ui.
No build, selftest, app launch, stop, or installed-copy access. Proposed signatures/rules are recommendations, not implemented or runtime-verified.
Only this report was written. Initial status already contained untracked `.coord/tasks/A43-app-context-impact.md`; preserved.
The task's report-only constraint overrides the usual STATUS update requirement. Two bounded read-only workers checked threads/statics and sibling overlap.

## Usage baseline

Reproducible search: `rg -n 'Program\.' product/AntiNotepad --glob '*.cs'`; count tokens with `rg -o 'Program\.\w+' ...`.
73 occurrences on 70 matching lines, in 8 files; 69 occurrences outside Program.cs (7 files). No `using static Program` aliases found.
Counts are lexical occurrences, not runtime executions; `PadWindow.cs:21` contains two Store tokens; TaskbarPresence.cs:49 and PadWindow.cs:414 each contain two different tokens.

| File | Qualified reads/calls: member = lines (occurrence count) | Total |
|---|---|---:|
| Editor/PadView.cs | NotesLocationError=275 (1); DataFolder=280 (1) | 2 |
| Platform/TaskbarPresence.cs | IsDev=49,56,64,143,165,174,186,202,203,235,240,244,248,277,297,322,367,381,385,389,400,405,416,425,426,441,444,529 (28); Log=49 (1) | 29 |
| Program.cs, App class | Store=344; Attach=360; StartHidden=361; StartupNotice=363 (1 each) | 4 |
| UI/PadWindow.cs | Store=21 twice (2); IsDev=72,84,322,336,341,344,414 (7); Log=144,252,257,414 (4); FlushLogs=259,274 (2) | 15 |
| UI/PadWindow.Keys.cs | IsDev=35,197,209 (3); Log=39,200,214 (3) | 6 |
| UI/PadWindow.Layout.cs | IsDev=139,140 (2) | 2 |
| UI/SearchOverlay.cs | Store=108 (1) | 1 |
| UI/SettingsWindow.cs | SoftwareRendering=366 (1); DataFolder=578,613,653,673,754 (5); NotesLocationError=587,602,603 (3); AppHome=635,636,674 (3); Store=666 (1); MoveNotes=678 (1) | 14 |

| Qualified member | Before |
|---|---:|
| IsDev | 40 |
| Log | 8 |
| DataFolder | 6 |
| Store | 5 |
| NotesLocationError | 4 |
| AppHome | 3 |
| FlushLogs | 2 |
| Attach / StartHidden / StartupNotice / SoftwareRendering / MoveNotes | 1 each |
| Other Program members | 0 |

Program's own unqualified accesses are additional to those totals. Complete static-member inventory follows; R=read/call, W=assignment/initialization, all locations in Program.cs unless qualified above.
Property backing fields/accessors are compiler-generated manifestations of these properties, not separate service state.

| Member (declaration) | Writers / initialization | Internal readers/callers; external readers above |
|---|---|---|
| DataFolder (15) | default empty; 82; move 184 | R89,92,93,100,171,181 |
| AppHome (17) | default empty; 68,81 | R76,77 twice,78,80 twice,138,142,181 |
| NotesLocationError (18) | default null; 83,94,100,186 | R100,169,171 |
| StartupNotice (19) | default null; 84,95 | no unqualified R |
| StartHidden (20) | default false; 61 | no unqualified R |
| IsDev (21) | default false; 58 | R104 |
| SoftwareRendering (22) | default false; 111 | R112 |
| Store (23) | default null despite `null!`; 89 or 92 | R96,98,99,100 twice,101,103,104,106,111,169 twice,174,176,183,186,187 |
| GpuCacheBytes (24) | computed on demand from environment; default 8 MiB | R109; no stored value |
| _window (25) | default null; Attach:194 | R120,129 |
| _pendingRequests (26) | singleton latch at type initialization | RequestShow:118, RequestQuit:127, Attach:195; mutates latch internals |
| Main (29) | method; no writers | executable entry point, not a source call |
| RunApplication (37) | method | Main:33 |
| RequestShow (116), RequestQuit (125) | methods | passed to SingleInstanceService:62; listener invokes callbacks (Platform/SingleInstance.cs:128). `RequestQuit` at Program.cs:60 is the different SingleInstanceService method |
| Log (134) | method | 43,49,69,70 (delegate),85,86,106,188; delegate invoked by Startup.MigrateLegacy (Platform/Native.cs:155-166) |
| _logGate (150) | readonly lock object at type initialization | locks 140,156,163 |
| _logger (151) | default null; lazily assigned 142 under _logGate | R143,156,163 under gate; never reset, including disposal |
| FlushLogs (153) | method | ProcessExit:32; exception handlers:44,50 |
| DisposeLogs (160) | method | Main finally:34 |
| MoveNotes (167) | method | only SettingsWindow.cs:678 |
| Attach (192) | method | only App:360 |

## Constants versus runtime state

| Item | Destination / rationale |
|---|---|
| Existing Program members | **None is a pure product constant to move to AppInfo.** Paths, notices, renderer selection, Store and IsDev are runtime state; others are methods/coordination objects. |
| AppInfo.IsDevBuild | Keep existing assembly/build identity (`AppInfo.cs:13,25`); sourced from generated assembly metadata, not the launch environment. DisplayName/icon use it at 26,61. It is build identity, although the C# accessor is not a literal `const`. |
| Program.IsDev | Put readonly `bool IsDev` on passed AppRuntime. Code: Program.cs:54-59 requires nonempty ANTINOTEPAD_DATA **and** ANTINOTEPAD_DEV exactly `1`. Data override alone isolates an instance but does not enable dev UI. Never substitute IsDevBuild or add static AppInfo.IsDev. |
| Product names, environment key construction, IDs, resource/log names | Already AppInfo.cs:24-68. Keep there. Runtime **selected** instanceId remains a local in Program.cs:59. |
| GPU cache default 8 MiB / conversion factor (Program.cs:24) | Optional named immutable values on AppInfo only if useful; no required move. Environment-derived GpuCacheBytes remains local bootstrap configuration, not AppInfo runtime state. |
| StartHidden, StartupNotice, SoftwareRendering | Readonly context values; SoftwareRendering records actual startup renderer, not the user's later desired Material. |
| DataFolder, NotesLocationError | Privately mutable, UI-owned notes-location state; they must change together after Store.SwitchFolder. |
| AppHome, Store reference, logging service | Readonly after bootstrap; Store's content/folder and logger internals remain mutable. |

## Initialization and later changes

| Order / evidence | What becomes valid | Earlier read / constraints |
|---|---|---|
| Program type initialization:15-26,150-151 | empty paths, null notices/Store/window/logger, false flags; latch and log gate exist | `null!` does not allocate Store. Store.Data dereference fails; Log silently returns on empty AppHome (138); Flush/Dispose are null-safe. |
| Main:31 | exact single argument `--selftest` returns SelfTest.Run immediately | No RunApplication, ProcessExit subscription, logging disposal, env parsing or context initialization. |
| Main:32-34; RunApplication:39-51 | ProcessExit + process/UI exception handlers | Handler can run before folders exist: best-effort no-op log, safe flush. |
| RunApplication:54-63 | custom-data predicate, runtime IsDev, selected instanceId; exact `--quit` then returns; StartHidden set only afterward | Identity must be selected **before** quit. Nonprimary returns before any folder resolution/Store. IsDev can be true with Store null on these short paths. |
| 66-70 | migration result sets first AppHome; migration logging/legacy startup migration may create logger | Do not eagerly create directories/logger on selftest, quit, or secondary-launch path. Logger captures path on first Log:142, not on every write. |
| 72-86 | notes resolution replaces AppHome and sets DataFolder/error/notice; catch produces error resolution | Null error before this stage is not evidence of usable storage. NotesLocation.Resolve preserves app-home identity while following notes pointer (Platform/NotesLocation.cs:17-55). |
| 89-106 | Store allocated (fallback on constructor failure), unavailable marked, loaded, error reconciled, welcome/dev seeding only if available | Store reference exists before loading completes; prevent UI exposure until load/seeding complete. Failed store still deliberately passed to UI. |
| 107-113 | GPU cache configured; actual SoftwareRendering chosen from RENDER override or loaded Material; Avalonia begins | Early false SoftwareRendering could lie about actual renderer. |
| App:344-363 | store captured; PadWindow, tray, shutdown handler created; hooks installed; Attach; show or hidden prewarm; startup toast | PadWindow constructor reads settings and binds panes (UI/PadWindow.cs:40-60); requires loaded store and resolved folder/error state. Attach drains requests after hooks exist. |
| MoveNotes:169-189 | guard; synchronous Store.Save; NotesLocation.Move; same Store.SwitchFolder; DataFolder rewritten; error updated; warning logged | Success return can coexist with newly unavailable Store:186-189. StartupNotice, AppHome, IsDev, renderer and Store identity do not change. |

Construction/capture audit (all qualified readers accounted for in first table):

| Reader | Capture versus live behavior / stale risk |
|---|---|
| App:344-358 -> PadWindow:38-40, tray slot delegates | Caches Store reference, not its folder; safe because MoveNotes switches same Store. Replacing Store on move would stale pad and tray. |
| PadWindow.cs:21 -> Timers | Timers/delegate allocated at field initialization, but lambda reads Program.Store twice **when called**; no Store snapshot today. Use injected same Store with live `.Data.Stack/Slots` lookup, not a captured list (Model/Timers.cs:34, lifecycle callback use). |
| SearchOverlay.cs:108-114 | Each StartSearch reads live Store, captures search context; background request intentionally snapshots body folder/cache epoch. Existing running search need not follow a move; next search gets new folder. |
| PadView.cs:265-284 | Bind caches error/hint/read-only state; same Note early return at 267 prevents refresh. Reads live folder/error only on an actual bind; snapshotting these in constructor adds a new stale-state bug. MotionPreview also uses this Bind path despite Private documents (UI/MotionPreview.cs:110,118). |
| SettingsWindow.cs:578,587,602-603 | Values rendered/captured when storage rows are built; Refresh after move:691 rebuilds them. No constructor-time global snapshot. |
| SettingsWindow.cs:613-636 | Recovered path/count/description captured per page build; AppHome description captured there; open-app-home action reads live at invocation:636. |
| SettingsWindow.cs:653,666,673-678,754 | Picker continuation/move/open read live values; oldFolder/appHome deliberately snapshot before moving for backup reporting. |
| SettingsWindow.cs:366 | Reads startup renderer every restart-hint computation; desired settings can change, actual renderer cannot. |
| PadWindow.Layout.cs:139-140 | Dev badge materialized at layout build; other IsDev readers in Keys/PadWindow/TaskbarPresence read live at event/call time. Runtime IsDev never changes after initialization, so readonly injection is safe. |
| App:361,363 | StartHidden and notice consumed once; not ongoing settings. |
| All Log/Flush callsites | Live logger access today; first logger creation caches log path. Keep one lifetime owner and stable app-home log location after notes move. |

## Threads and synchronization

| Path | Thread / Program state / synchronization |
|---|---|
| A30 save | PadWindow.QueueSave/Save use injected `_store`; Model/Store.cs:734-749 starts Task.Run(ProcessSaveQueueAsync). Worker takes batches/completions under `_saveGate` (760-800,896-903), writes captured snapshot outside lock (783-794,851-893). No worker Program access. Captures folder/data/bodies at 797-823; Data.All enumeration precedes lock:799 and worker publishes LastError, so the gate is not a general lock on Store/Data. Keep snapshot/UI ownership assumptions. |
| Search bodies | UI captures Program.Store/context (SearchOverlay.cs:102-114); worker Task.Run at 124 reads captured context, not Program. Store.CreateSearchContext locks cache Gate:410-412; cache read/invalidation/epoch operations lock at 450-481,504-510; file read outside lock, ReadCount uses Interlocked. Moving folder must retain cache invalidation/epoch behavior (SwitchFolder:948-960). |
| BackgroundLog | Program.Log reads AppHome before gate, initializes/captures _logger under _logGate:138-144; Flush/Dispose capture same reference under gate:153-165. Worker Task.Run:215 consumes only own fixed path/queue (248-267), never Program. Its _gate guards writes/disposal/queue barriers (218-246,270-283); Flush waits barrier/worker outside lock. |
| Process/crash handlers | ProcessExit:32 can flush off UI thread; AppDomain.UnhandledException:41-45 can log/flush on failing thread. Avalonia handler:47-51 is UI. No general locks on Program paths/flags/Store; normal startup publishes them before UI starts. Do not make handlers depend on UI dispatcher. |
| Single instance | Platform/SingleInstance.cs:48-49 dedicated background Thread, callback invocation:121-130. Program RequestShow/Quit touch latch and _window (116-131); latch lock:298-320 serializes readiness/pending flags; Attach assigns window before entering that gate (194-198), dispatches via UIThread.Post. Service Dispose requires owner thread and joins listener (133-147). |
| Session end / taskbar | App shutdown requested:349-352 calls UI PrepareForSessionEnd; PadWindow.cs:239-259 synchronously saves and flushes logs. Taskbar native/UI callbacks read IsDev and call Log; no evidence of an additional managed worker for those callbacks. No extra lock around dev diagnostics. |

## Selftest dependence

No `Program.*` read in SelfTest.cs, nor in the reached Store/search/BackgroundLog/pure UI-helper paths found by source tracing. Main:31 returns before all normal initialization; if inspected, Program.Store is null, paths empty, notices null, flags false (even when environment requests dev). They are not exercised indirectly merely because Program's type was initialized.
SelfTest constructs temporary Stores and standalone BackgroundLog (1126), runs search workers (550,555,587), calls pure TaskbarPresence decisions (673-704), PadView pure helpers, and SettingsWindow.SearchMatches (728-730); no window construction/PadView.Bind path found. The search fixture saves/restores Note.LoadBody/InvalidateSearchBody (522-523,614-615), separate hidden globals.
Injecting runtime only into application construction keeps existing checks equivalent; it does **not** add coverage of Program's context wiring, real folder errors, dev routing or GUI shutdown. If BackgroundLog moves files, preserve its constructible test seam. Suggested small new capability check: move temporary notes via AppRuntime, assert same Store reference plus updated folder/error; keep it explicit and isolated, not normal startup from selftest. No tests run in this audit.

## Other static state (scope boundary)

Default A43 scope is Program dependencies only; these remain candidates, except diagnostics state may move mechanically when eliminating Program reads. R/W lists give owning methods and external callsites; no synchronization unless noted.

| State / declaration | Writers / readers | Scope |
|---|---|---|
| Anim.Reduced, UI/Anim.cs:8 (A39 moves file) | W PadWindow.cs:43,506 from Settings; R Anim.Run:15, Editor/Motion.cs:58,75,273, SettingsWindow.cs:235, PadWindow.Layout.cs:324,332. UI-owned, no lock. | Out; runtime settings global, not a constant. |
| PadView.Documents / Recent / Owners, Editor/PadView.cs:31,198-199 | RW DocumentFor:201-215; removals Forget:222-225/Evict:232-242; R CachedDocuments:247. Callers Navigation.cs:45,136,198-205,310,383; SearchOverlay.cs:362; PadWindow.cs:371,391,408; count diagnostics Keys:39/PadWindow:414. No lock; UI cache retains documents/undo/owners. | Out; do not scope per window accidentally. CachedDocuments is derived count, not separate storage. |
| Note.LoadBody / InvalidateSearchBody, Model/Store.cs:133,140 | W Store ctor:389-390 and SwitchFolder:959-960; R Note.ReadBody:185-190 and InvalidateSearchCache:207-210, called by note mutations. SelfTest save/restore:522-523,614-615. No lock; latest Store wins. | Out; passing Store does not solve multi-Store interference. |
| Search caches/revisions | SearchBodyCache:30-36 belongs to Store; per-note revision:137,207-210; context caches folder/epoch:14-26. Gate and Interlocked as thread table. | Not static; retain lifecycle on move. |
| CodeColors.Disabled, Editor/Structure.cs:275 | W PadWindow.cs:515 from DisabledLanguages; R Structure.cs:281 and Completion.cs:69. No lock. | Out; static settings-derived reader. |
| CodeColors._mappedFor, Structure.cs:283 | RW Definition:291; definition from global HighlightingManager.Instance:289, Map mutates definition:292,297 onward. Marker does not skip Map. | Out; shared highlighting state. |
| Completion.OcrRequested / _open, Editor/Completion.cs:20,50 | Event W subscription PadWindow.cs:64, invocation command:45; no unsubscribe found. Popup R IsOpen:51/Show:55, W close/open:94-95; callers PadView.cs:686, PadWindow.Keys.cs:43. UI-owned. | Out; event/popup can retain pad/window. |
| SettingsWindow._section / Expanded, UI/SettingsWindow.cs:32-33 | Section R68,120,134,178,215; W228. Expanded R UI/SettingsParts.cs:127; Add/Remove:150. UI lifetime memory across window reopening. | Out. |
| MotionPreview._editorSamples/_feelSamples / _editorIndex/_feelIndex, UI/MotionPreview.cs:77-78 | Arrays lazily initialized/read Samples:95-97; indices RW Index:99, navigation:116-117, R trace:172 and bind:197. Shared sample note content/index survives preview disposal. | Out. |
| SessionEnd.IsEnding, Platform/SessionEnd.cs:8 | RW Begin:10-14, called PadWindow.cs:241; R PadWindow.cs:71,83,90,125,213,227,364,369,375,383; TaskbarPresence.cs:136,142,198,267. No lock; UI/message-loop assumption. | Out; preserve process lifetime. |
| TaskbarPresence._padActionAt/_padActionCause, Platform/TaskbarPresence.cs:18-19 | W TracePadAction:55-59; R PadActionContext:62-65; PadWindow show/hide calls:126-222 plus traces. No lock. TraceStartedAt:17 fixed timestamp feeds SinceTraceStart:52 and Trace:49. | Out as redesign; may move intact into single TaskbarTrace instance to remove global service reads. |
| PadWindow.TrimDelaySeconds, UI/PadWindow.cs:398 | Static env read TRIM_SECONDS, captured by instance _trim initializer:397. | Out; tuning flag, not persisted settings. |
| Startup.IsEnabled / Command, Platform/Native.cs:134-151 | IsEnabled reads Run registry live:143; SetEnabled writes/deletes:150-151; MigrateLegacy:155-170. Command uses SpecialFolder.LocalApplicationData + AppInfo; SettingsWindow.cs:504-505 reads/toggles. | Out; external platform state/static accessor, not in-memory settings singleton. |
| GlobalHotkey constructor env access, Platform/GlobalHotkey.cs:39 | Reads HOTKEY only when fromEnvironment; DefaultGesture:16 get-only record. | Out; static default immutable, environment resolution currently constructor-local. |
| Static readonly tables, not actively mutated | Modes.Keywords (Model/Modes.cs:10, read Detect:21 onward); CodeColors.Languages/Aliases (Structure.cs:256, Find:277, Completion:69); Completion.Commands (22, used completion building); SettingsWindow.Sections (25, navigation); HelpOverlay.Sheet (13, help rows); PadMenu.ActionLabels (23, action row); Colorizers.RainbowLight/Dark (112-113, colorizing). No table-content writes found beyond initializers. | Out; readonly references technically expose mutable collections; different from active globals above. |
| Other fixed reusable statics | Regex instances in Timers/PadView/TextCommands/Elements/Colorizers; JSON options in Store/NotesLocation/Completion; fonts in Kit/PadMenu; welcome/showcase strings, MenuEntry.Separator and design constants. No runtime settings owner/cache writes found. | Out; reusable configuration/data, not launch context. |

No additional static persisted-Settings singleton found in Model/Editor/UI/Platform. BCL/Avalonia global services (Application.Current, HighlightingManager.Instance, clipboard, registry) remain outside Program-only A43; a context sweep does not isolate them.

## Proposed shape and signatures

Use internal `AppRuntime` in the root namespace (avoids collision with System.AppContext), constructed locally by Program after Store load and renderer choice, before Avalonia creates App. No container, singleton accessor or `AppRuntime.Current`.

| Member / owner | Contract |
|---|---|
| AppRuntime ctor | `(Store store, string appHome, string dataFolder, string? notesLocationError, string? startupNotice, bool startHidden, bool isDev, bool softwareRendering, RuntimeLog log)`; get-only Store/AppHome/StartupNotice/StartHidden/IsDev/SoftwareRendering/Log. |
| Notes state | DataFolder + NotesLocationError private setters, updated only by `bool MoveNotes(string target, out string message)` on UI thread; do not duplicate a separately mutable Store reference. A readonly reference to a small location-state object can narrow what Editor sees. |
| RuntimeLog | Tiny lifetime service exposing `Write(string)`, `Flush()`, `Dispose()`; owns existing BackgroundLog/gate behavior. For bootstrap, retain empty-home no-op and lazy creation, update provisional home as resolution proceeds, freeze after bootstrap. Never retarget an already-created logger. |
| Program bootstrap | Own local logger before registering handlers (no-op until home known); handlers capture it rather than a runtime that does not yet exist. Set home at existing assignments:68,81. Construct AppRuntime only on primary application path. |
| App construction | Add `internal App(AppRuntime runtime, Action<PadWindow> attach)` (public App has internal dependency types); Program supplies runtime and Attach delegate through AppBuilder's application factory. Exact Avalonia 12 factory overload requires implementation-time compile verification. App does not read Program directly. |
| PadWindow | `PadWindow(Store)` -> `PadWindow(AppRuntime runtime)`; store readonly alias allowed. Move `_timers` initialization into ctor after assigning store; lambda closes over injected store and checks its current Data each invocation. |
| SettingsWindow | `SettingsWindow(PadWindow pad)` -> `SettingsWindow(PadWindow pad, AppRuntime runtime)` at current seam; A40-settings-change-event should replace pad coupling with its settings owner/callbacks before finalizing this signature. Inject runtime independently of that owner. |
| SearchOverlay | `(PadWindow window, Palette palette, string query, bool trash)` -> add `Store store`; replace Program.Store with `_store`, keep search snapshot timing. Full runtime is unnecessary. |
| PadView | `(Palette, Settings, Timers)` -> add `AppRuntime runtime` (or narrow location-state reference). Bind reads live runtime DataFolder/error; do not precompute constructor strings. |
| MotionPreview | `(Palette, Settings)` -> add same runtime/location state and forward to its private PadView. Preserve existing error behavior for preview as well. SettingsWindow.cs:65 constructs preview. |
| TaskbarPresence | Retain static pure decision functions for selftests. Replace static Trace/TracePadAction/PadActionContext with an instance diagnostic helper `TaskbarTrace(bool isDev, Action<string> log)` in the same file, created once per pad. Pass helper to proxy; move its cause/timestamp state together, preserving lifetime. No static Configure(context). |
| TaskbarProxy | `(Window pad, string title, WindowIcon? icon, Action<bool,string> taskbarClick)` -> add `bool isDev, TaskbarTrace trace`; use readonly bool for all 25 proxy IsDev reads. TraceProxyMessage is already an instance method (320); pure static window-description helpers need no service access. |
| Timers | **No signature change**: existing `Timers(Func<string,bool>? isLive=null)` (Model/Timers.cs:34) is already the seam. Fix its caller only. |
| Tray | **No signature change**: existing action/slot callbacks suffice (Platform/Tray.cs:12). App closures use injected same store. |
| Logging in other classes | PadWindow uses runtime.Log.Write/Flush; TaskbarTrace receives one allocated Write delegate; bootstrap Startup.MigrateLegacy receives it too. No need for ILog or a global forwarding shim. |

Existing methods without signature changes: PadView.Bind, SettingsWindow.MoveNotes/OpenNotesFolder/Material hint, PadWindow lifecycle methods; their implementations access injected state. All PadWindow partials share the runtime field; factory callsites in Layout/Keys and MotionPreview change.
Keep in Program: internal static type identity, Main/RunApplication, environment/argument parsing, local selected instanceId, exact selftest/quit short circuits, SingleInstanceService lifetime, RequestShow/RequestQuit, latch/window Attach coordination, final log disposal. Remove public runtime accessors and Log/Flush forwarding once all callers are injected.
MoveNotes moves verbatim into AppRuntime initially: preserve precondition/message order, the second Store.Save after SettingsWindow's `_pad.Save`, NotesLocation.Move backup/pointer semantics, Store.SwitchFolder before location-state assignment, warning logging and success-with-unavailable behavior. Do not replace Store or add settings-change propagation in this task.

## Risks and concrete failure scenarios

| Risk | Concrete failure / preservation rule |
|---|---|
| Startup order | Publishing context before Store.Load gives PadWindow default settings/empty data and wrong renderer (Program.cs:89-113; UI/PadWindow.cs:40-60). Construct only after loading and renderer decision. |
| Instance / quit isolation | Computing quit identity from IsDev instead of usesCustomData sends DATA-only quit to installed instance (Program.cs:54-62). Preserve two distinct predicates and exact argument matching. All custom folders currently share DevInstanceId; do not introduce path-hashed identities. |
| Dev/build identity | Replacing runtime IsDev with IsDevBuild can seed showcase or expose dev controls without the DATA+DEV opt-in (Program.cs:104; AppInfo.cs:25-26). |
| Unavailable notes | Dropping MarkUnavailable/error reconciliation can recreate/write an unavailable folder or enable editing blank substitutes (Program.cs:89-100; PadView.cs:275-284). Keep failed Store available as an object and error live. Choosing a folder currently still hits MoveNotes guard; do not silently implement recovery behavior here. |
| Move / stale captures | Constructor-captured DataFolder leaves storage/open/recovered actions pointing at old folder; replacing Store strands tray/timers. Preserve same Store identity and page refresh (SettingsWindow.cs:673-691). |
| Session-end durability | Disposing log before PrepareForSessionEnd loses final save outcome; omitting Store.Flush on Quit anyway can terminate queued writes (PadWindow.cs:239-282). Preserve save -> log -> Flush ordering and ProcessExit flush + Main finally disposal. |
| Early/racing crashes | Exception handler capturing unassigned context throws while reporting original crash; logger disposal racing writes must stay best effort (Program.cs:41-50,134-165). Capture lifetime log service immediately, keep its locks/no-throw behavior. |
| Single-instance startup race | Dispatching before Attach reaches null window; holding a new context lock across dispatcher or disk waits can deadlock listener/UI (Program.cs:116-131,192-198; latch:294-322). Retain latch publication order and post dispatch. |
| Selftest | Requiring real runtime construction to call pure helper tests introduces folders/Avalonia into early selftest path (Program.cs:31). Preserve plain helper entry points; use explicit temporary runtime only for focused new capability checks. |
| Hotkey cost | Creating context/delegates or looking up services on each show/hide adds allocation/work (PadWindow.cs:115-155). Allocate once at composition/constructor time; readonly field/bool reads only. No new IO, locks or events on hotkey path. Performance remains unmeasured here. |
| Dev/installed filesystem split | Resolving Documents/default folder before custom override/primary check touches real user data on a secondary or dev launch (Program.cs:62-78). Keep resolve/migration guards and createDirectories condition identical. |

## Guard rules for A39-motion-out-of-ui LAYERS

A39 branch currently contains its brief, not implemented scanner/product changes; baseline SelfTest.cs contains no LAYERS code. These are additions to its **planned** table. Program is recognizable as `internal static class AntiNotepad.Program` (Program.cs:12), not top-level compiler-generated statements.

| Rule / kind | Today | After A43-app-context |
|---|---|---|
| Exact type-target: `any application type except Program and its nested/compiler-generated types -> AntiNotepad.Program` forbidden | Fails: seven external files plus App in Program.cs; namespace-root exclusion would incorrectly hide App. | Holds if App receives runtime + Attach delegate. Match declaring-type ancestry for Program-owned closures, not all root types. |
| Simpler pairs `Editor -> type Program`, `UI -> type Program`, `Platform -> type Program` | Fail (usage table). Model/Calc/Theme -> Program have zero source references today. | All hold, but alone miss App and new namespaces; prefer universal exact-type rule. |
| `Editor -> UI`; `Model -> UI/Editor`; `Theme -> UI/Editor`; `Platform -> UI/Editor`; `Calc -> any other AntiNotepad namespace` | Editor->UI fails today (PadView.cs UI imports/menu/motion; A39 brief enumerates). Other pairs have no source dependencies in inspected namespaces today; IL confirmation not run. | Preserve A39's rules after its move; context must not depend on UI types in Editor-facing signatures. |
| New static-runtime-access rule | Program's runtime getters violate it. IsDevBuild is sanctioned build metadata, not runtime flag. | Reject static fields/getters returning AppRuntime, Store or location services and runtime launch flags outside explicit composition-root allowlist; no Current/Instance accessor. |
| Runtime flag source provenance | Program.cs:58 is current sanctioned DATA+DEV evaluation. Other environment reads exist for independent tuning knobs. | Require DATA/DEV reads only in Program; forbid new AppInfo runtime getters. Combine IL call checks for Environment.GetEnvironmentVariable plus constant key/name checks; a name-only IsDev check misses renamed aliases. |

Scanner must cover signatures, base/interfaces, fields/properties/events, generic arguments, nested/lambda types and IL resolved field/method/type tokens (A39 brief:43-50). Add exact type predicates alongside namespace pairs. Static shape checks need member metadata; environment provenance needs IL/constants or a small source audit, not just namespace edges. Reflection/string-built lookups cannot be proved absent by ordinary token scan; document that bound. Prove a deliberate temporary violation fails when implementing, then remove it.

## Overlap and order

Minimum proposed existing-code set: **10 files**: Program.cs; Editor/PadView.cs; Platform/TaskbarPresence.cs; UI/PadWindow.cs; UI/PadWindow.Keys.cs; UI/PadWindow.Layout.cs; UI/SearchOverlay.cs; UI/SettingsWindow.cs; UI/MotionPreview.cs; SelfTest.cs. **2 new files**: AppRuntime.cs, RuntimeLog.cs (BackgroundLog may move with RuntimeLog). Total **12 product paths**; AppInfo.cs need not change. Documentation/status/report paths are additional, outside this product count.

| Sibling / inspected ref | Actual product diff now / brief-based overlap with A43 |
|---|---|
| A39-motion-out-of-ui, reorg/A39-motion-out-of-ui @8164c33 | No product diff; 2 coordination files changed vs dev. Brief plans Anim/Chrome/MenuEntry moves, Editor/Motion.cs, PadView.cs, Elements.cs, PadWindow.Layout.cs, PadMenu.cs, SelfTest.cs and Anim callers. **3 definite A43 intersections**: PadView, Layout, SelfTest; PadWindow, SettingsWindow, MotionPreview likely additional callers. Exact final file count unavailable. |
| A40-settings-change-event, reorg/A40-settings-change-event @4689a14 | Only impact brief committed; 0 product files. Brief:10-18 names Store.cs, PadWindow.Keys.cs, SettingsWindow.cs; **2 explicit intersections** Keys/SettingsWindow, plus propagation likely PadWindow/MotionPreview. No implemented signature to adopt yet. |
| A41-padwindow-split, reorg/A41-padwindow-split @b54b476 | 0 ahead files, no brief available in branch tree. PadWindow partial overlap inferred from task name only; exact paths/count unverified. |
| A42-padview-commands, reorg/A42-padview-commands @4e47778 | Only impact brief committed; 0 product files. Brief:11-18,27-30,43-45 targets PadView.cs, TextCommands.cs/new helpers, SelfTest.cs and callers. **2 explicit intersections** PadView/SelfTest. |
| A44-platform-seam, reorg/A44-platform-seam @b54b476 | 0 ahead files; no brief in tree; no matching claude/a44-platform-seam-* ref found. TaskbarPresence/Program overlap is plausible, not verified. Exact paths/count unavailable. |

Recommended integration: **A39-motion-out-of-ui first**, then A42-padview-commands and A41-padwindow-split structural work, A40-settings-change-event ownership work, A44-platform-seam once brief exists, then A43-app-context injection sweep over settled constructors. A41/A44 relative placement remains provisional until their real briefs exist. Alternatively land A43 immediately after A39 as a shared signature baseline and require siblings to rebase onto it; do not independently invent runtime/static substitutes on each branch.
Success measures: zero Program references outside Program-owned composition code (including App); all 73 qualified tokens above removed if App uses the Attach delegate; all runtime accessors removed; no new static runtime service accessor; A39 rules plus new exact-type rule pass. Recount against merged baseline since sibling changes can move lines/files. Report-only audit cannot establish runtime neutrality, quit routing, session-end durability or latency; those remain implementation gates using isolated dev/test data.
