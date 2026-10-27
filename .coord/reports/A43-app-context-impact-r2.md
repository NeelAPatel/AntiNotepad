# A43-app-context-impact-r2

Read-only source audit, 2026-10-26. Branch `reorg/A43-app-context`, HEAD `5f55472` (latest dev `756f139` merged by `902ee36`).
Paths/lines below are relative to `product/AntiNotepad/` unless stated otherwise. Round 1 = `.coord/reports/A43-app-context-impact.md`, baseline `b54b476`.
Read AGENTS.md, STATUS, the plan, round 1, dev-loop and agents instructions. One bounded read-only worker checked isolation/lifetimes.
Only this report is authorized for writing; STATUS/plan remain unchanged. No build, selftest, launch, stop, `--quit`, or installed-copy inspection was performed.
Source findings are verified; proposed signatures, IL outcomes, performance and all hand checks are unverified until implementation gates.

## Recount: 73 -> 75

Search: `rg -n 'Program\.' product/AntiNotepad --glob '*.cs'`, count each `Program\.\w+` match separately.
75 qualified tokens on 72 lines in 8 files; 69 outside Program.cs, unchanged. All 6 in Program.cs belong to the separate `App` class.
No using-static/alias access found. Counts are lexical calls/reads, not executions or distinct IL edges. Qualified writes: **zero**.

| Member | Round 1 | Now | Delta |
|---|---:|---:|---:|
| IsDev | 40 | 40 | 0 |
| Log | 8 | 8 | 0 |
| DataFolder | 6 | 6 | 0 |
| Store | 5 | 5 | 0 |
| NotesLocationError | 4 | 4 | 0 |
| AppHome | 3 | 3 | 0 |
| FlushLogs | 2 | 2 | 0 |
| **SettingsService (new A40)** | **0** | **2** | **+2** |
| Attach / StartHidden / StartupNotice / SoftwareRendering / MoveNotes | 1 each | 1 each | 0 |
| Total | 73 | 75 | +2 |

| File | All qualified readers/calls: member = lines | Round 1 -> now |
|---|---|---:|
| Editor/PadView.cs | NotesLocationError=274; DataFolder=279 | 2 -> 2 |
| Platform/TaskbarPresence.cs | IsDev=49,56,64,143,165,174,186,202,203,235,240,244,248,277,297,322,367,381,385,389,400,405,416,425,426,441,444,529; Log=49 | 29 -> 29 |
| Program.cs (`App`) | Store=346; **SettingsService=347,349**; Attach=364; StartHidden=365; StartupNotice=367 | 4 -> 6 |
| UI/PadWindow.cs | Store=23 twice; IsDev=87,99,336,350,355,358,433; Log=159,267,272,433; FlushLogs=274,289 | 15 -> 15 |
| UI/PadWindow.Keys.cs | IsDev=37,203,215; Log=41,206,220 | 6 -> 6 |
| UI/PadWindow.Layout.cs | IsDev=142,143 | 2 -> 2 |
| UI/SearchOverlay.cs | Store=108 | 1 -> 1 |
| UI/SettingsWindow.cs | SoftwareRendering=380; DataFolder=592,627,667,687,768; NotesLocationError=601,616,617; AppHome=649,650,688; Store=680; MoveNotes=692 | 14 -> 14 |

Complete Program static inventory follows; W includes initialization, R includes calls/delegate capture. All evidence in Program.cs; external readers are above.
Compiler-generated property backing fields/accessors are not additional owners. Arguments and local variables are not static state.

| Member / declaration | Writers or initialization | Unqualified readers/callers |
|---|---|---|
| DataFolder:15 | empty default; 83,186 | 90,93,94,102,173,183 |
| AppHome:17 | empty default; 69,82 | 77,78 twice,79,81 twice,140,144,183 |
| NotesLocationError:18 | null; 84,95,102,188 | 102,171,173 |
| StartupNotice:19 | null; 85,96 | none |
| StartHidden:20 | false; 62 | none |
| IsDev:21 | false; 59 | 106 |
| SoftwareRendering:22 | false; 113 | 114 |
| Store:23 | null!; 90 or 93 | 97,99,100,101,102 twice,103,105,106,108,171 twice,176,178,185,188,189 |
| **SettingsService:24** | **null!; 99 aliases Store.SettingsService** | **105,113** |
| GpuCacheBytes:25 | computed env getter, default 8 MiB; no stored value | 111 |
| _window:26 | null; 196 | 122,131 |
| _pendingRequests:27 | readonly new latch | 120,129,197; mutable latch state:291-325 |
| _logGate:152 | readonly new lock | 142,158,165 |
| _logger:153 | null; lazy assignment 144 under lock, never reset | 145,158,165 |
| Main:30 / RunApplication:38 | methods, no writers | entry point / 34 |
| RequestShow:118 / RequestQuit:127 | methods | callbacks passed at 63; line 61 calls SingleInstanceService.RequestQuit instead |
| Log:136 | method | 44,50,70,71 (delegate),86,87,108,190 |
| FlushLogs:155 / DisposeLogs:162 | methods | 33,45,51 / 35 |
| MoveNotes:169 / Attach:194 | methods | only qualified callers above |

## Current composition and prior injections

| Stage | Current order / evidence | A43 implication |
|---|---|---|
| Main | exact sole `--selftest` returns first:32; ProcessExit flush registration:33; RunApplication:34; finally dispose:35 | Keep selftest independent of real bootstrap. Create/capture lifetime logger before installing handlers if removing statics. |
| RunApplication early | telemetry/crash hooks:40-51; DATA/DEV/instance id:56-60; sole `--quit`:61; hidden:62; instance ownership:63-64 | Do not move folder resolution before primary check or quit routing. |
| Paths | migration:68; provisional AppHome:69; normal-only registry migration:71; Documents skipped for custom:73; resolution/fallback:77-87 | Early logging can precede final notes resolution. Preserve provisional-home/no-op/lazy logging behavior. |
| Store/settings | Store constructed:90/93; service alias:99; unavailable:100; Load:101; error reconcile:102; welcome + showcase:105-106 | **Store creates** SettingsService in Model/Store.cs:382, with replacement callback updating current Data.Settings. Program does not create a second service. |
| Renderer | builder:109-111; RENDER override or service.Current.Material:113; software option:114; start lifetime:115 | Build injected launch dependencies only after loaded settings and renderer selection. |
| App | Initialize styles:335-338; explicit shutdown:345; pad:347; **one Changed subscription:349**; OS shutdown handler:353-357; QuitRequested:358; tray:359-362; hooks:363; Attach:364; show/prewarm:365-366; notice:367; base:369 | Keep event subscription in composition root; no second subscription or new service on window reopen. App is not Program-owned merely because it shares its file. |
| A40-settings-change-event | Store.SettingsService get-only:375; replacement on Load paths:552,573,602; SettingsService.Current/Changed and owner-thread check in Model/SettingsService.cs; SettingsWindow(SettingsService,ISettingsHost):54 | Pass existing service explicitly; runtime paths/logging are not settings. UI/ISettingsHost.cs:6-20 is already the host-action seam; PadWindow implements it:505-515. |
| A41a-hotkeys | PadWindow.cs:50-55 injects two registration factories, posted toggle/new-note actions and toast; InstallHooks:118-123 registers; CompleteQuit:292 disposes | HotkeyRegistrar has **no Program dependency**. Platform/HotkeyRegistrar.cs:75 reads HOTKEY env fallback, then GlobalHotkey.DefaultGesture, not Program.IsDev/Store. Keep this seam intact. |
| A41b-clipboard-policy | PadWindow.cs:49 creates per-pad policy; Navigation.cs:287-313 supplies text/own-copy/current body; privacy check:306 precedes text eligibility/decision | Stateful instance for dedupe + pure FormatAppend (Editor/ClipboardCapturePolicy.cs:3-34); no runtime/context injection required. |
| A42a-math-notes | Editor/MathNotes.cs:12,42,49 takes strings/lines; ListEditing.cs:8-12 supplies regex/number helper; NoteAnalysis.cs:148-159 calls math helper | Explicit values/pure helpers; do not feed them launch context. |
| A42b-note-analysis-file | PadView.cs:37 owns NoteAnalysis; NoteAnalysis.Update:78-79 takes TextDocument, range, inlineMath, note id, timers | File move, no new service locator. PadView smaller (864 lines); its 2 Program reads remain in Bind:274-279. |
| A39-motion-out-of-ui | Motion/Anim.cs:6-8 still has mutable Reduced; Theme/ChromeTokens.cs owns tokens; PadView.ShowMenu:77,696 supplied by PadWindow.Layout.cs:120 | Preserve namespace boundaries; no Editor -> UI host interface. |

New Program static state since round 1: **SettingsService only**, an alias of the Store-owned instance. A40's static property/flags tables are metadata (SettingsService.cs), A42a's ListPrefix is a reusable regex; A41 adds instance policy/registrar state and A42b moves existing analysis state. No new runtime singleton was found in these extracted helpers. Existing Anim.Reduced, editor document caches, Note.LoadBody/InvalidateSearchBody, session-end state, preview samples and Settings section memory remain outside A43.

## Constants versus runtime; preferred ownership

| Program member(s) | Re-decided classification | Recommended owner |
|---|---|---|
| IsDev | Runtime launch predicate: nonempty DATA **and** DEV exactly `1` (Program.cs:57-59) | Readonly launch bool passed to pad/taskbar trace/proxy. Never AppInfo.IsDevBuild, which reads assembly metadata (AppInfo.cs:13,25). |
| StartHidden / StartupNotice / SoftwareRendering | Runtime launch decisions/result, fixed after startup | App receives hidden/notice values; Settings host can expose actual renderer bool for restart hint. |
| AppHome | Resolved runtime path, assigned twice during bootstrap; stable during notes move | Logger owns resolved logging destination; notes-location service keeps home for pointer/move and Storage display. |
| DataFolder / NotesLocationError | Runtime mutable location state | Small live NotesSession/location service; error and folder must update together after SwitchFolder. Prefer read-only location interface for Editor. |
| Store / SettingsService | Runtime services; stable identities, mutable contents | Explicit Store and its existing SettingsService, not two independent owners. |
| Log / FlushLogs / DisposeLogs; _logger / _logGate | Runtime behavior + lifetime/synchronization state | RuntimeLog wrapping BackgroundLog; Program owns disposal, consumers get instance/delegate. |
| MoveNotes | Stateful operation over live store/location and filesystem | NotesSession; move body verbatim first. |
| GpuCacheBytes | Runtime env getter, **not constant** | Local bootstrap getter/value; no reason to inject into UI. |
| _window / _pendingRequests; Attach / RequestShow / RequestQuit | Composition/lifecycle state and coordination | Keep confined to Program initially; pass attach delegate to App. |
| Main / RunApplication | Bootstrap behavior, not constants or data | Program. |

**Recommendation: do not add the broad round-1 AppRuntime.** Smaller pieces now fit the established seams: explicit Store/SettingsService, RuntimeLog, launch values, and live notes-location state. A40's SettingsService should not absorb filesystem paths, logger, renderer or dev identity: its diff/event/save contract concerns persisted Settings only. ISettingsHost can carry Settings-only host operations/renderer fact (UI seam); it must not be passed into Editor or Platform. A small aggregate local to App/Program is optional constructor convenience, but no global Current/Instance and no broad context flowing into every PadView.

## Shape and split recommendation

| Task / order | Scope and shape | Risk / gate |
|---|---|---|
| **A43a-runtime-log-and-launch** first | Extract RuntimeLog; inject log + isDev into pad and instance TaskbarTrace; pass bool/trace to TaskbarProxy; inject hidden/notice/renderer facts and attach callback into App/UI. Keep Store/location/MoveNotes statics temporarily. | Medium-high: crash/exit log lifetime and taskbar trace lifetime. No native activation/style/policy rewrite. Hand-check show/hide/taskbar/hidden and log paths. Partial Program guard only for members removed in this task. |
| **A43b-notes-session** second | Explicit Store/SettingsService into App/pad; timer closure over injected Store; SearchOverlay Store; NotesSession owns same Store + AppHome + live folder/error + verbatim MoveNotes; read-only location view to editor/preview; Settings host forwards move/save error/path operations or inject NotesSession directly. Remove remaining runtime statics. | High: isolation, stale folder captures, move/save/error sequencing. Gate temp-folder move capability, restart pointer resolution, unavailable-folder protection, session-end hand check. |
| Skip | DI container; settings redesign; broad static cleanup; A42c/A42d list work; OCR/save scheduler extraction; clipboard/hotkey redesign; install scripts; migration policy changes; frame styles; runtime IsDev in AppInfo. | Keep scope structural. A44-platform-seam follows both tasks. |

| Type / current signature (evidence) | Proposed final change versus round 1 |
|---|---|
| PadWindow(Store, SettingsService), UI/PadWindow.cs:45 | Keep explicit services; add narrow launch/log/location dependencies. Move `_timers` initializer:23 into ctor after `_store` assignment. Simpler than replacing everything with AppRuntime; some constructor width remains. |
| SettingsWindow(SettingsService, ISettingsHost), UI/SettingsWindow.cs:54 | Already decoupled from PadWindow. Prefer extend host with narrowly named storage/renderer operations and read-only location, keeping service parameter. Alternative explicit NotesSession + renderer bool avoids expanding host. Preserve `_host.Save()` then LastError check:679-680. |
| PadView(Palette, Settings, Timers), Editor/PadView.cs:79 | Add live read-only location view; no Store/log/isDev/full runtime. Bind reads fresh error/folder; same-note short circuit:266 remains unchanged. |
| SearchOverlay(PadWindow, Palette, string, bool), UI/SearchOverlay.cs:39 | Add Store; retain existing settings Update through window:391. No broader decoupling needed here. |
| MotionPreview(Palette, Settings), UI/MotionPreview.cs:103 | Add/forward location view into private PadView:110; do not silently change unavailable-folder behavior for preview. |
| TaskbarProxy(Window, string, WindowIcon?, Action<bool,string>), Platform/TaskbarPresence.cs:115 | Add readonly bool isDev + TaskbarTrace; static LossTimestampText:461 becomes trace-aware too. Keep pure static decisions and native callback order. |
| App (public parameterless today), Program.cs:331 | Internal injected ctor/factory with explicit services/launch values/attach. Avalonia factory overload **not compiled/verified here**; verify local package API at implementation. Avoid static App service handoff. |
| Timers / Tray / HotkeyRegistrar | No signature change needed. Existing callbacks/factories suffice; Tray closures retain same Store. |

Round-1 direction/order still holds: composition root -> explicit dependencies, after A42b and before A44. Its broad AppRuntime signature and obsolete SettingsWindow(PadWindow) baseline should not be copied. Its suggested alternate ordering that put A44 first is superseded by the current plan/user sequence.

## Isolation path and exact future checks

| Path / evidence | Behavior to preserve / checks that catch a mix-up |
|---|---|
| Program.cs:32 | `args is ["--selftest"]` returns before hooks/env/instance/folders. Combined arguments are not this shortcut. Future isolated test: sole selftest with hostile DATA fixture path must not create that path or contact an instance. |
| Program.cs:56-61; AppInfo.cs:55-56 | Nonempty DATA alone selects `AntiNotepad-dev`; IsDev is independent. Matrix: DATA absent + DEV=1 => normal id, IsDev false; DATA set + DEV absent/0 => dev id, false; DATA set + DEV=1 => dev id, true. Test parsing as pure capability without launching a no-DATA app. Empty DATA counts absent; all custom paths share one dev id (no per-path instance). |
| Program.cs:61-64 | Sole `--quit` routes by usesCustomData before hidden/primary/folders; secondary startup asks existing instance to show and returns. Future manual quit only with absolute disposable DATA explicitly set; installed copy stays running (user observation). Never test installed quit routing by sending a no-DATA quit. |
| Program.cs:62,363-366; Platform/SingleInstance.cs:44-51 | `--hidden` is Contains, not sole-arg match; still installs hooks, attaches latch and prewarms. A second launch, even another `--hidden`, signals show unconditionally. User dev check: hidden primary has no initial resident pad; second hidden launch reveals it. Preserve this existing behavior. |
| DefaultDataFolderMigration.cs:19-40; Program.cs:68-73 | Custom bypasses legacy default/registry migration and Documents lookup. Normal primary migrates LocalAppData/Antipad -> AntiNotepad with old-folder fallback on failure, then resolves Documents. Use synthetic localAppData/Documents roots only for capability checks. |
| NotesLocation.cs:19-25,60-76 | Custom absolute home follows **its own location.json** after MoveNotes; absent pointer means home==notes. It does not consult installed pointer/Documents. Invalid/missing target returns error, does not create target. Check custom fixture containing pointer to second disposable folder, then missing target and malformed pointer. |
| NotesLocation.cs:28-56; Program.cs:77-102 | Normal resolution follows pointer or migrates app-home notes/creates Documents default; app home/log stays separate. Store createDirectories is exactly `usesCustomData && notes.Error is null`; preserve fallback MarkUnavailable and post-Load error reconcile. |
| Program.cs:169-191; Store.cs:961-974 | Guard unavailable -> Save -> NotesLocation.Move -> SwitchFolder -> folder/error update -> warning log. NotesLocation.cs:110-142 copies, verifies hashes/count, writes pointer then retains old copy; failure cleans new files. Same Store/SettingsService identity; successful copy can still leave Store unavailable. Check identity, folder/error, pointer, backup, new save destination; collision/pointer-write failure must retain old usable data. |
| Existing selftests | SelfTest.cs:852-940 migration success/existing/custom/failure; :943-986 and :1390-1415 identity; :1059-1192 notes resolution/recovery including custom pointer:1177-1182; :1195 onward latch; :1751 onward save/log. Inspect/run after implementation in isolated output; **not run here**. They do not currently test Program's DATA/DEV -> quit-routing predicate end to end. |

Refactor-sensitive portions: dependency construction, App callback capture, logger home/lifetime, Store identity, live location reads, MoveNotes ownership. Keep actual env/argument parsing, instance id, migration implementation, pointer format, native event/mutex behavior and install/dev scripts unchanged.
Documentation discrepancy: docs/dev-loop.md:11 says custom DATA bypasses the pointer; current NotesLocation.cs:19-25 and SelfTest.cs:1177-1182 prove it follows its own pointer. Correct that wording in later authorized documentation work, not this report-only audit.
Additional isolation gate: user launches through tools/dev.ps1, confirms dev hotkey D/badge, Storage app-home and notes paths both lie in intended disposable roots (after move they differ), log remains in custom home, restart reloads moved fixture. Badge alone is insufficient: DATA-only runs are intentionally unbadged. No installed-data mutation/inspection is needed by this audit.

## Highest risks and user hand checks (future, dev fixtures only)

| Risk / evidence | Concrete failure scenario | Hand check |
|---|---|---|
| Startup ordering, Program.cs:99-114,347-349 | New service created before Load or second service after Load gives defaults, wrong renderer or missing Changed effects. | Restart fixture with saved theme/material/split; verify same state and Settings change/save/reopen; renderer restart hint matches actual startup mode. |
| Single instance/latch, Program.cs:118-132,194-199,289-328 | Request arrives before attach and is dropped, or new locking blocks UI startup. | Two rapid launches with same custom DATA: one dev resident/tray; second shows existing pad. Include hidden primary and rapid secondary. |
| Quit identity, Program.cs:57-64 | Basing instance id on IsDev sends DATA-only quit to normal instance. | User sets disposable DATA, clears DEV, requests dev quit; only dev exits. Repeat DEV=1. Observe installed stays running; never issue no-DATA quit. |
| Location/move, Program.cs:169-191; SettingsWindow.cs:677-705 | Cached path/error stays old; replaced Store strands timers/tray; failure updates pointer too early. | Edit fixture, move to sibling empty folder, check path/Open/recovered, tray slots/search/timer, save and restart. Reject occupied/nested target; unavailable pointer stays read-only and missing target remains absent. |
| Session end, PadWindow.cs:254-274,285-296; Program.cs:353-357 | Logger disposed before save outcome, or async pending writes abandoned. | In user-controlled dev session, type unique final edit then sign out/restart; reopen fixture and check exact edit + `session-end save=ok` in custom log. Still unverified; do not claim ordinary Quit proves OS shutdown. |
| Crash/exit log, Program.cs:33-51,136-166 | Exception callback captures unassigned runtime or logger silently moves from initial home to notes folder. | Check hotkey/session-end lines remain in app-home log before/after notes move; early-crash path needs focused synthetic capability check, not a destructive manual crash of working copies. |
| Taskbar trace, TaskbarPresence.cs:17-65,461 | One trace per method or changed timestamp origin loses click-cause correlation; changed callback order affects activation. | Repeated taskbar hide/show, hotkey show, blur hide and hidden startup; verify no bounce/focus regression and continuous trace timestamps/cause. No new hot-path allocation/IO/lock; timing unmeasured. |

## LAYERS additions in current table shape

Current table: SelfTest.cs:108-116 defines From/To/AnyAntiNotepadNamespace/ExactFrom/ExactTo; :395-466 has **72 rows**, plus Settings setter output:537 = **73 LAYERS lines**. Capability outputs SETTINGS/HOTKEYS/CLIPBOARD/MATH NOTES/LIST EDITING are separate (:24-40).
Scanner already selects every matching source (:496), scans members/IL (:502-517), supports exact types plus nested ancestry (:582-599); A44's old claim of Editor-only source scanning is stale.
These are proposed product code snippets, not edits or executed guards:

```csharp
new("Editor", "Program", ExactTo: typeof(Program)),
new("UI", "Program", ExactTo: typeof(Program)),
new("Platform", "Program", ExactTo: typeof(Program)),
new("Model", "Program", ExactTo: typeof(Program)),
new("App", "Program", ExactFrom: typeof(App), ExactTo: typeof(Program)),
```

| Row(s) above | Holds today by source inspection | Final expectation |
|---|---|---|
| Editor / UI / Platform -> Program | No / no / no (recount) | Yes after A43b-notes-session; Platform can pass after A43a-runtime-log-and-launch. |
| Model -> Program | Yes; no source references | Preserve. |
| App -> Program | No, 6 tokens | Yes after A43b-notes-session. |

Universal rule needs a new source selector; existing From="*" would match namespace AntiNotepad.* literally, not all sources.
Proposed minimal extension to LayerRule: `bool AnyApplicationTypeExceptTarget = false`; MatchesSource selects all application types outside ExactTo's declaring-type ancestry when set. Use current IsExactOrNested helper; do not exempt the whole root namespace/file.

```csharp
new("*except Program", "Program", ExactTo: typeof(Program), AnyApplicationTypeExceptTarget: true),
```

**Metadata caveat:** literal "only Program may reference Program" also rejects SelfTest's existing `typeof(Program)` rows (:405,415,430,446,458) and the new rows. Add a narrow allowance for Program `ldtoken` operands in SelfTest's rule-table initializer only; continue rejecting Program field/property/method access in SelfTest and other methods. This needs operand-aware handling before AddType collapses references, not a blanket SelfTest exemption. Today universal rule fails on real readers regardless; deliberate violations in App, an Editor method and a newly added root type must fail after implementation, while metadata declarations pass.
Additional guards are **separate kinds**, not expressible with the current type-edge table: static field/property exposing Store/SettingsService/RuntimeLog/NotesSession outside Program (allow test-only local instances, not singleton access); DATA/DEV env-source provenance confined to Program (do not ban HOTKEY/RENDER/tuning env reads). Source/IL operand checks required; reflection/string-built keys remain a documented limit. New types do not exist today; static service guard otherwise holds outside Program. DATA/DEV source provenance holds by inspected source, not an executed IL check. Avoid a large scanner project: exact rows + final universal rule and one focused move capability are the essential A43 gate.

## File overlap and current sizes

Logical line counts from Get-Content; bytes from filesystem. Sizes describe current HEAD, not generated/build output.

| Existing product path | Lines | Bytes | Expected A43 work |
|---|---:|---:|---|
| Program.cs | 371 | 15,500 | Both tasks: bootstrap, App, remove services; BackgroundLog may move verbatim. |
| Platform/TaskbarPresence.cs | 556 | 26,777 | A43a-runtime-log-and-launch trace/bool only. |
| UI/PadWindow.cs | 516 | 24,005 | Both: ctor, timers, trace/log, lifecycle. |
| UI/PadWindow.Keys.cs | 285 | 13,212 | Both: log/dev reads and Search/Settings construction. |
| UI/PadWindow.Layout.cs | 493 | 28,223 | Both: badge bool, PadView location. |
| UI/SettingsWindow.cs | 797 | 44,381 | Both: renderer and storage/preview reads. |
| UI/ISettingsHost.cs | 20 | 581 | Both if preferred host extension chosen; optional with explicit NotesSession/renderer injection. |
| UI/SearchOverlay.cs | 396 | 18,930 | A43b-notes-session Store. |
| Editor/PadView.cs | 864 | 41,353 | A43b-notes-session location ctor/Bind. |
| UI/MotionPreview.cs | 299 | 13,623 | A43b-notes-session forwards live location. |
| SelfTest.cs | 1,923 | 123,286 | Both: staged guards/capabilities; serialize edits. |

10 required existing product files + optional ISettingsHost; new RuntimeLog.cs, NotesSession.cs and optional separate read-only location contract (size N/A). TaskbarTrace can stay in TaskbarPresence.cs. No AppRuntime.cs required. AppInfo/SettingsService/Store/NotesLocation/HotkeyRegistrar/MathNotes/ListEditing/NoteAnalysis should need no product edits under this scope.
A44-platform-seam branch `1cb4e33` has a plan/report (read via git show), unlike round 1's unavailable brief. Its plan targets Native facade, GlobalHotkey/TaskbarProxy P/Invoke nesting, optional Settings OpenFolder/dev metrics, and LAYERS. Direct A43 overlaps: TaskbarPresence.cs, PadWindow.cs/Keys/Layout, SettingsWindow.cs, SelfTest.cs; Program bootstrap is explicitly to remain unchanged there. Re-audit A44 after both A43 tasks: its old line numbers, Editor-only scanner claim and references to Program in GlobalHotkey/Tray are not current evidence. Do not fold A44 native-seam work into A43.

Completion verification: initial git status clean; final `git status --short --untracked-files=all` shows only `?? .coord/reports/A43-app-context-impact-r2.md`. No changes to plan/STATUS/product/.git; no build or app commands executed.
