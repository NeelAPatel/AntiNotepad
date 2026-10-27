# A43b-notes-session-impact

Read-only re-audit, 2026-10-26; branch `reorg/A43-app-context`, HEAD `646cf7e`, includes dev `ab4d353` and A43a merge `5245a62`.
Paths below are relative to `product/AntiNotepad/` unless prefixed with `docs/`, `tools/` or `.coord/`.
Read AGENTS, STATUS, revision-2 plan/impact, A43a brief/report, dev-loop and agents instructions; one bounded read-only worker checked guards.
Source/API-doc findings verified against this tree; proposed signatures, guard results and all runtime/hand checks remain **unverified**.
No build, selftest, app launch, process stop, `--quit`, installed-data inspection or git mutation. Only this report is written; STATUS exception follows the explicit report-only instruction.

## Recommendation on handling

| Decision | Recommendation / reason |
|---|---|
| Proceed? | **Yes, keep A43b-notes-session as one behavior-neutral task.** A43a removed logging/launch complexity; remaining scope is coherent service injection plus live location ownership. No broad AppRuntime or DI container. |
| Split? | Optional `A43b1-explicit-services` (App/Store/service/timer/search injection) then `A43b2-notes-session` (location/move/UI/guards/docs). The first removes only 6 tokens and leaves the risky 15-token location seam; two startup gates add cost. Prefer one task with these as review stages, one final hand gate. |
| Settings route | Inject NotesSession directly into SettingsWindow; retain existing ISettingsHost for UI actions/Save. Fewer forwarding members, no Store exposed through the host, preview receives the session only as a read-only location view. |
| Wording conflict | Plan:31-32,57-60 keeps Program's window/latch/Attach wiring; scope says delete **all** runtime statics. Recommend delete all runtime **service/location** statics (14-19,133), retaining confined bootstrap `_window`/`_pendingRequests` and methods. Literal all-state removal additionally requires local window/latch + captured local RequestShow/RequestQuit/Attach functions; do not silently claim that happened. It is small but unnecessary lifecycle churn. |
| Drop / leave alone | No RuntimeLog/TaskbarTrace rewrite; no Store/NotesLocation/settings-policy change; no refresh-on-same-note fix, search cancellation redesign, recovery behavior change, install/scripts/migration/native/frame work, other singleton cleanup. |
| A44-platform-seam | Afterwards, re-audit its branch against merged A43b. Shared existing files: PadWindow.cs/Keys/Layout, SettingsWindow.cs, SelfTest.cs. TaskbarPresence was A43a overlap, requires no A43b edit. Preserve new scanner behavior when A44 extends it. |

## 1. Remaining qualified Program uses: 21 tokens on 20 lines, five files

All are reads/calls, zero qualified writes. No using-static/alias access found. UI-thread labels are source callback paths, not measured thread traces.

| Member | File:line (each token) | Live vs cached / thread |
|---|---|---|
| Store (2) | UI/PadWindow.cs:23,23 | Lambda resolves global Store and current Data.Stack/Data.Slots whenever called; Timers UI lifetime (Model/Timers.cs:29-37,127-132). |
| Store | UI/SearchOverlay.cs:108 | Live lookup per StartSearch on UI; local Store then metadata/context snapshot; worker consumes context, not Program. |
| Store | Program.cs:233 (App) | One startup lookup into local `store`; UI initialization; tray captures this same reference. |
| SettingsService (2) | Program.cs:234,236 (App) | Startup read for ctor and single Changed subscription; UI initialization. |
| DataFolder | Editor/PadView.cs:279 | Fresh at effective Bind, then copied into hint; UI. |
| NotesLocationError | Editor/PadView.cs:274 | Fresh at effective Bind, then local/hint/read-only state; UI. |
| DataFolder | UI/SettingsWindow.cs:594 | Fresh when NotesFolderDetails factory builds controls; displayed string thereafter; UI. |
| NotesLocationError (3) | UI/SettingsWindow.cs:603,618,619 | Fresh details/error/action construction; controls cache resulting text/actions until rebuild; UI. |
| DataFolder | UI/SettingsWindow.cs:629 | Captured recovered path per StoragePage construction; lazy count/description closures; UI. |
| AppHome (2) | UI/SettingsWindow.cs:651,652 | Row description snapshot at build; Open callback reads home when clicked; home stable after startup; UI. |
| DataFolder | UI/SettingsWindow.cs:669 | Live comparison after awaited folder picker; UI continuation expected. |
| Store | UI/SettingsWindow.cs:682 | Live LastError check immediately after host Save; UI. |
| DataFolder / AppHome | UI/SettingsWindow.cs:689,690 | Intentional pre-move `oldFolder`/`appHome` snapshots; UI. |
| MoveNotes | UI/SettingsWindow.cs:694 | Synchronous operation on UI, no dispatch or await. |
| DataFolder | UI/SettingsWindow.cs:770 | OpenNotesFolder reads current folder at click time; UI. |

Counts: DataFolder=6, NotesLocationError=4, AppHome=3, Store=5, SettingsService=2, MoveNotes=1; 18 tokens outside Program.cs, 3 inside its separate App class.

| Program still owns | Evidence / disposition |
|---|---|
| Five service/path properties | DataFolder:14, AppHome:16, NotesLocationError:17, Store:18, SettingsService:19; initialized empty/null/null!, assigned in bootstrap; path/error also changed by MoveNotes. Replace with locals/session. |
| Private log bridge | `_runtimeLog`:133 assigned once:36, used only warning log:156. Remove with MoveNotes extraction; RuntimeLog lifetime remains local in Main:28-31. |
| Bootstrap state/methods | `_window`:21, `_pendingRequests`:22; Main:25, RunApplication:34, RequestShow:115, RequestQuit:124, MoveNotes:135, Attach:160. GPU_CACHE_MB computed getter:20 is runtime env access, not stored service/constant. |
| Not Program statics | AppLaunch is a root record:212-213; launch is local:107. `_launch` is readonly App instance field:217, assigned:220; `_tray` App instance field:218. StartupRequestLatch:170 is a separate instance type. No new singleton beyond `_runtimeLog` bridge. |
| A43a seam delta | Log/dev/hidden/notice/renderer/Attach reads removed from consumers; AppLaunch and factory:107-108,212-220; pad receives log/dev/renderer:234; Settings renderer already explicit. Platform/TaskbarPresence has zero Program references. RuntimeLog owns BackgroundLog. |

## 2. Composition order and session placement

| Stage | Current evidence | Constraint for A43b |
|---|---|---|
| Early exits / lifetime | Sole selftest:27 precedes logger/hooks; logger:28, ProcessExit flush:29, finally dispose:31; crash callbacks capture log:39-46. | Keep outside NotesSession; no folder work for selftest. |
| Instance before paths | DATA/DEV/id:51-55; sole quit:56; hidden:57; SingleInstance:58; nonprimary return:59. | Session construction must remain after primary ownership. |
| Provisional home | DefaultDataFolderMigration.Resolve:63; first AppHome assignment:64 + SetHome:65; migration log:66; normal-only Startup migration:67. | Preserve early logging destination even if later resolution throws. |
| Final paths | custom/normal Resolve:73-75, exception fallback:77; **second** AppHome assignment:78 + SetHome:79; folder/error/notice:80-82. | AppHome may normalize/change; session takes final home, not first assignment. Logger never retargets an already-created writer (RuntimeLog.cs:24-25). |
| Store fallback | Construct:87 with `createDirectories: usesCustomData && notes.Error is null`; catch:90 constructs same path with false, records error:91-93. | Preserve two-stage attempt; fallback constructor can itself throw (no enclosing recovery). Never manufacture a writable alternate store or replace unavailable folder with app home. |
| Settings and load | Alias Store.SettingsService:95; unavailable mark:96; Load:97; reconcile missing error:98; seed/showcase only available:99-103; log LastError:104. | Store ctor creates sole SettingsService (Model/Store.cs:382); Load replaces its Current, not service identity (:552,573,602). Session starts after reconciled state. |
| Renderer + factory | Renderer selection from loaded service:106; launch:107; Configure<App>(() => new App(launch)):108; backend config:110-111; lifetime:112. | Build session after:106 and beside AppLaunch; inject both into App through existing factory. NotesSession is not launch options and should not own AppLaunch/log disposal/window callbacks. |
| App completion | Explicit shutdown:232; window:234; Changed:236; session-end:240-244; QuitRequested:245; tray:246-248; tooltip:249; hooks:250; Attach:251; show/prewarm:252-253; notice:254. | Same order, one Store/service instance, one Changed subscription. No second Store/SettingsService in App or session. |

Avalonia question is resolved: current factory call is already present, A43a report records a successful build, and local `C:/Users/Neel/.nuget/packages/avalonia/12.1.4/lib/net8.0/Avalonia.Controls.xml:24748` declares `Configure<TApp>(Func<TApp>)`. No new overload/fallback/static handoff needed. No build was run here.

## 3. MoveNotes contract: preserve verbatim sequencing

| Step | Evidence / exact behavior to preserve |
|---|---|
| Session guard | Program.cs:137-140: null/unavailable Store OR non-null location error -> false, current error/unavailable-folder message. Keep even though injected Store is non-null. |
| Save before filesystem move | :142-147 Store.Save false -> false with exact save-error text. This is in addition to host Save; do not coalesce the two saves. Store.Save synchronously captures/drains (:674-693). |
| Move | :149 calls NotesLocation.Move(AppHome, DataFolder, target); :150 failure message and return, no session state change. Filesystem helper normalizes/guards:Platform/NotesLocation.cs:83-103, copies/verifies:110-130, writes pointer:132, keeps old copy:133, returns success/warning:134; cleanup on failure:136-142. |
| Switch then publish | Program.cs:151 Store.SwitchFolder first; :152 folder, :153-154 error derived from Store.IsAvailable, :155 message, :156 warning log, :157 true. Store.SwitchFolder drains, updates paths/availability/error/body delegates/cache (Model/Store.cs:961-974), retaining Data and SettingsService. No new Load or Store. |
| Success with unavailable store | A successful Move followed by unavailable SwitchFolder still returns true, publishes target + unavailable error, and chooses warning first, otherwise unavailable message. Preserve; no rollback/error conversion. This race path is source-verified, not reproduced. |
| Host preflight | UI/SettingsWindow.cs:681 calls `_host.Save()` and ignores its bool; :682 checks Store.LastError, sets Couldn't save error and Refresh on failure. Host Save -> PadWindow.Save:406-419 flushes both panes, saves and evicts documents (unless SessionEnd.IsEnding); preserve the UI flush before session's second Save. |
| Intentional before snapshots | SettingsWindow.cs:689-693 captures oldFolder, appHome, fileCount, backupRoot and directory set. Backup root = appHome if same path, otherwise old folder's parent. These must remain **before** move. |
| Result display | :694-699 failure: error + clear notice + Refresh, pending destination retained. :702 finds new backup using old snapshots; :703-706 clears pending/error, sets old-copy success text and warning notice; :707 Refresh. Preserve strings/count/fallback behavior, including unavailable success. |
| Thread | Production move stays synchronous on UI; no Task.Run/await/extra lock. Save captures live model/editor callbacks; settings owner thread recorded at Model/SettingsService.cs:109 and checked:266-267. Session construction is on Main STA before Avalonia; UI runs that owner path. Background writer drains internally; session itself is not thread-safe. |

Folder/error update together means coherent after the synchronous call on its owner thread; current two property assignments are not a cross-thread atomic transaction. Do not introduce a concurrency promise or state-change event in this refactor.

## 4. Stale-capture inventory

| Reader | Existing lifetime / preservation requirement |
|---|---|
| PadView.Bind | Editor/PadView.cs:266 same-reference early return occurs **before** location read:274. Error/path are captured into hint/read-only/document selection:274-285 until a different note binds. Inject live view, read inside Bind; do not pass constructor strings or force rebinding as an unrequested fix. |
| MotionPreview | UI/MotionPreview.cs:110 private PadView still reads global availability despite Private=true; Private only changes document cache after unavailable check (PadView.cs:283-285). Forward same live view, not an always-healthy preview fixture. Bind on swipe:117 and UseSamples:193-197; ApplyLook:201-204 does not refresh location for same note. |
| Settings rows | Details get current view when factories execute:594-619; recovered path/count/description cached per StoragePage build:629-650; home description built once:651, Open live:652. Refresh -> Restyle -> Build:114-127 rebuilds these after move. Keep captured recovered path per build; OpenNotesFolder:770 live. |
| Settings move | Old path/home/count/before set are deliberately captured:689-693 for post-move backup text; retain. Picker comparison after await:669 must use live folder. Local `_notesError` precedence over location error:603 remains. |
| PadWindow timers | UI/PadWindow.cs:23 currently reads Program.Store each callback; move initializer into ctor after `_store` assignment:51, capture Store reference only. Access `store.Data.Stack/Slots` inside lambda each time, never capture Data/lists. Timers predicate cleanup:Model/Timers.cs:127-132 on UI paths. |
| Tray closures | App Program.cs:233,246-248 already capture stable Store, read current Data/Slots/ViewSlot at invocation. Preserve; no captured slot list or new Store. |
| SearchOverlay | StartSearch:106-110 snapshots source/metadata/current context on UI each request; context stores body-folder + cache epoch (Store.cs:411-414). Worker:SearchOverlay.cs:122 reads snapshot path; UI applies generation/overlay guarded:123-126. Inject stable Store but recreate context per search, never cache initial context in ctor. |
| Existing in-flight search | SwitchFolder clears cache epoch (:974); a worker may still hold old folder. No new move-driven cancellation exists. Preserve existing behavior; check fresh searches after move, label pre-move in-flight results an existing risk. |
| Other caches | Store body delegates rebind on SwitchFolder:972-973; Store.Data/service identity retained. PadView document caches, Note.LoadBody/InvalidateSearchBody statics, Settings/preview samples and Anim.Reduced remain out of scope. No other qualified Program readers found. |

## 5. Isolation, arguments and documentation

| Contract / evidence | Exact future gate (disposable dev fixtures only; not executed) |
|---|---|
| Sole `--selftest`, Program.cs:27 | Invoke sole selftest in isolated output with DATA pointing to a nonexistent temp child; assert child not created, no normal bootstrap/instance signal. Combined args are intentionally not the shortcut. Existing checks:SelfTest.cs:77,100-110. |
| usesCustomData vs IsDev, :51-56 | Source predicate matrix: DATA absent/DEV absent -> normal id,false; DATA absent/DEV=1 -> normal id,false; DATA present/DEV absent -> dev id,false; DATA present/DEV=1 -> dev id,true. Test parsing without actually launching or quitting normal identity. Nonempty includes whitespace; do not change predicate during refactor. |
| One dev identity | Both custom cases use AppInfo.DevInstanceId regardless of folder path; two different fixture DATA roots do **not** create independent dev instances. Quit all dev fixtures manually between roots; do not infer isolation per folder. |
| Single instance/latch | Program.cs:58-59,115-130,160-165; StartupRequestLatch:177-208. User: rapid same-DATA second launch shows primary, only one dev tray; repeat hidden primary. Listener callbacks on background thread (SingleInstance.cs:48,128), post window actions to UI; latch buffers before attach. Keep quit/show ordering and mutex disposal on creating thread (:136-137). |
| `--quit` | Sole quit returns before paths/lock construction (:56); SingleInstance.RequestQuit only opens existing objects (:72-104). User-only future test with explicit absolute disposable DATA, DEV unset then 1: only dev PID exits. Verify executable path/start-time/fixture home first; never issue no-DATA quit. This audit sends no signal. |
| `--hidden` | Contains predicate:57; attach first:251 then Prewarm:253, no ShowResident. Test rapid secondary while primary prewarms; preserve pending requests. |
| Custom migration/path | DefaultDataFolderMigration.cs:18 bypasses legacy migration; Program.cs:67 skips registry migration and :69 skips Documents lookup; NotesLocation.Resolve custom branch:19-25 follows own pointer. Preserve final AppHome normalization and log SetHome calls. |
| Missing/invalid pointer target | NotesLocation.cs:60-76 yields error without creating target; Store construction false when notes.Error non-null (Program.cs:87,90), MarkUnavailable:96; Store.Load:530-538 protects unavailable. Confirm absent target remains absent and editing is read-only; don't accidentally use default `createDirectories=true`. |
| Existing capability evidence | SelfTest.cs:985-1053 migration scenarios; :1185-1318 resolution/move/recovery; custom pointer follow:1303-1308; :1321 onward latch; :1517 identity; :1606 close; :1873 save/log. These are source checks, **not run here**, and do not prove end-to-end Program quit routing. |
| Mix-up detection | Before fixture edits verify executable is task/dev output, DATA is absolute disposable home, dev script PID path/start time checks (tools/dev.ps1:150-215), runtime dev marker+hotkey D when enabled (:315-317), Storage app home and notes folder both in intended fixture roots; log remains in home after notes move; home/location.json targets fixture notes. Badge alone insufficient: DATA-only deliberately has no badge. Restart with same DATA and verify unique fixture text survives in moved target. |

Exact replacement for `docs/dev-loop.md:11` dev Notes folder cell:
`build/testdata` initially; after an in-app move, the folder selected by `build/testdata/location.json` (`ANTINOTEPAD_DATA` bypasses Documents and first-run migration, and follows its own pointer).
Also fix the related false sentence at `docs/dev-loop.md:46`: replace “`ANTINOTEPAD_DATA` bypasses this startup resolution for dev and selftest fixtures.” with “`ANTINOTEPAD_DATA` uses the custom folder as app home, bypasses Documents and first-run migration, and follows that home's `location.json` after an in-app move.” Neither documentation edit is made here.

## 6. Session-end/log lifetime

| Path | Ordering / A43b implication |
|---|---|
| PrepareForSessionEnd | UI/PadWindow.cs:264 one SessionEnd.Begin; :268-270 stop save/trim/cancel hide; :271-272 flush panes; :274 synchronous Store.Save + LastError; :275-280 log result/failure; finally log.Flush:282. Retain exact Store/log identities and order. |
| OS integration | App Program.cs:240-244 ShutdownRequested calls prepare and accepts shutdown; PadWindow.cs:112 Closing fallback also calls it; idempotent SessionEnd.Begin handles duplicates. Explicit Quit does not raise ShutdownRequested (:237-239). |
| Quit path | PadWindow.cs:250-258 Save then CompleteQuit; retry:285-288; Quit anyway:291. CompleteQuit:296 Store.Flush then :297 log.Flush before disposing hooks/proxy/close/callback:298-304. |
| Main/ProcessExit | Program.cs:28-31 owns RuntimeLog, ProcessExit captures same instance and Flushes, finally Dispose after lifetime returns. Crash handlers:39-46 capture same log. RuntimeLog.Flush/Dispose:33-50 swallow errors; BackgroundLog.Dispose:123-135 drains queue, Flush:83-98 waits even when disposed. ProcessExit may flush an already-disposed writer safely. |
| Scope | Removing `_runtimeLog` only replaces move-warning sink with session's injected sink; session must neither dispose logger nor own session-end flush. A30 OS sign-out/restart final-edit check is still pending (.coord/STATUS.md:71), ordinary Quit does not prove it. |

## 7. Proposed minimal shape and every caller

| Type/member | Proposed signature / caller changes |
|---|---|
| New NotesSession.cs (root namespace) | `internal sealed class NotesSession : INotesLocation`; ctor `(Store store, string appHome, string dataFolder, string? notesLocationError, Action<string> log)`; get-only Store/AppHome, private-set DataFolder/NotesLocationError; private readonly warning sink; `bool MoveNotes(string target, out string message)` body moved verbatim except sink. No static Current/Instance, new Store, settings creation, filesystem resolution, disposal, or async work. |
| New read-only contract | `Model/INotesLocation.cs`: `internal interface INotesLocation { string DataFolder { get; } string? NotesLocationError { get; } }`. Editor needs neither AppHome, Store, MoveNotes nor UI interface. Session implements; keep root-to-Platform filesystem dependency explicit, not a new Model-to-UI/Platform dependency. |
| Program.RunApplication | Service/path values become locals; after Load/reconciliation/seeding/renderer (:95-106), construct session using same Store/final home/error and log.Write. Remove five static properties + `_runtimeLog` + MoveNotes. Keep early bootstrap/latch policy per recommendation. |
| App | Preferred `internal App(AppLaunch launch, Store store, SettingsService settingsService, NotesSession notes)` with readonly fields; factory:108 passes locals. AppLaunch record unchanged, session built beside it. Replace App's three qualified reads:233-236; ctor forwards same references; Changed subscription uses injected service. Alternative extending AppLaunch with services works but mixes launch flags with live state for little benefit. |
| PadWindow | Append `NotesSession notes` to existing `(Store, SettingsService, RuntimeLog, bool isDev, bool softwareRendering)`; sole caller App:234. Store session readonly; `_timers = new(id => store.Data.Stack.Any(...) || store.Data.Slots.Any(...))` after assignments, before layout. Existing Store/service params already explicit; do not replace them with service lookups. |
| PadView | Append `INotesLocation location` to `(Palette, Settings, Timers)`; readonly view, Bind reads it at existing point. Both callers:PadWindow.Layout.cs:115 and MotionPreview.cs:110. |
| MotionPreview | Append `INotesLocation location` to `(Palette, Settings)`; forward to private PadView; sole caller SettingsWindow.cs:71. No preview behavior override. |
| SettingsWindow | Append `NotesSession notes` to `(SettingsService, ISettingsHost, bool softwareRendering)`; sole caller PadWindow.Keys.cs:169. Store readonly; replace paths/error/MoveNotes via session and LastError via `notes.Store.LastError`; pass view to MotionPreview. Existing `_host.Save()` unchanged. ISettingsHost.cs unchanged. |
| Host alternative | Extend ISettingsHost with live location/home/LastError + MoveNotes and forward through PadWindow:513-523; same Settings ctor. More host surface; must still pass narrow view to preview. Choose only if keeping storage entirely behind UI host is preferred; do not implement both routes. |
| SearchOverlay | Add Store to `(PadWindow, Palette, string query, bool trash)`, readonly field; sole caller PadWindow.Keys.cs:156 passes `_store`; StartSearch:108 uses field each invocation. No SettingsService param needed: GridView/updates already through window (:37,391). |
| Unchanged signatures | Timers(Func<string,bool>?) and preview's parameterless Timers, Tray callbacks, AppLaunch, RuntimeLog, TaskbarProxy/Trace, Store/NotesLocation/SettingsService. `SettingsWindow.MoveNotes` and session MoveNotes retain out-message shape. |

## 8. LAYERS rules and focused capability

Current `SelfTest.cs:116-124` LayerRule has From/To/AnyAntiNotepadNamespace/ExactFrom/ExactTo. Table:512-593 has **79 rows**, plus settings-setter output:663 = **80 LAYERS lines**, not 80 table entries. Existing source selection already scans all matching rule sources (:621-643); exact/nested matching:708-728; AddType recursion:827-845; operand resolution:922-947.

| Existing table form (do not duplicate existing rows) | Now / after A43b |
|---|---|
| `new("Platform", "Program", ExactTo: typeof(Program)),` | Already present:583, holds after A43a by source inspection. |
| `new("Model", "Program", ExactTo: typeof(Program)),` | Already present:584, holds now. |
| `new("RuntimeLog", "Program", ExactFrom: typeof(RuntimeLog), ExactTo: typeof(Program)),` | Already present:588, holds after A43a; preserve its other four rows:585-589 too. |
| `new("Editor", "Program", ExactTo: typeof(Program)),` | Add; fails now on PadView, holds after location injection. |
| `new("UI", "Program", ExactTo: typeof(Program)),` | Add; fails now on pad/search/settings, holds after A43b. |
| `new("App", "Program", ExactFrom: typeof(App), ExactTo: typeof(Program)),` | Add; App is a root type, not UI namespace; fails now, holds after constructor injection. |
| `new("*except Program", "Program", ExactTo: typeof(Program), AnyApplicationTypeExceptTarget: true),` | Add final universal rule; absent today. It also catches newly introduced root types outside the preceding namespace rows. |

| Guard detail | Small required implementation / limits |
|---|---|
| Universal selector | Append bool `AnyApplicationTypeExceptTarget=false` to LayerRule; MatchesSource selects all same-assembly application types outside ExactTo/its nested ancestry when set (reuse IsExactOrNested). Match target with existing ExactTo. `From="*"` alone would only name a nonexistent namespace. Include generated closure/state-machine types; do not exempt App because it shares Program.cs. |
| SelfTest metadata | Table already contains `typeof(Program)` tokens. Allow **only** `OpCodes.Ldtoken`, InlineTok resolving exactly Program, in SelfTest's rule initializer. Current initializer compiles into SelfTest.TypeInitializer; safer small refinement is extract only rule construction into named `CreateLayerRules()` and scope allowance there. If using .cctor instead, explicitly document allowance is method-scoped, not per-assignment dataflow. Reject Program field/method access even in that method and reject Program ldtoken elsewhere. Apply before AddType(:941-944), before HashSet loses operand provenance; never blanket-exempt SelfTest. |
| Avoid self-inflicted violation | Universal/scanner helpers should use `rule.ExactTo` or the rule-initializer-supplied target, not add new `typeof(Program)` operands outside allowed initializer. Other metadata types (NotesSession/Store) do not need this Program-specific exemption. |
| Static service exposure | Separate reflection check, since type-edge sets erase static-ness: scan declared static fields and static properties/getters for Store, SettingsService, RuntimeLog, NotesSession, outside Program ancestry per plan; include private/compiler backing fields, dedupe diagnostics. Local temp instances/instance fields pass. For final service-static removal also explicitly assert Program has none of these four. Typed check does not catch object/delegate/dictionary-hidden locators or static factory methods; do not overclaim. No general static ban. |
| DATA/DEV provenance | Source inventory confines env reads to Program.cs:51,54. Plan's additional provenance guard must distinguish DATA/DEV keys from allowed HOTKEY/RENDER/tuning reads; reflection/string-built access remains bounded. No large source parser project needed; keep essential universal rule + static exposure + move capability focused. |
| Guard gate later | Preserve all 80 outputs, add rows/checks; manager demonstrates violations in App, Editor and a new root type fail, including a selftest Program method/field read and static service field/property, then removes probes. Metadata initializer must pass; no guard was executed here. |

| New capability: `NOTES SESSION move` | Assertions in one disposable temp-root test (future implementation) |
|---|---|
| Real happy path | Construct Store+service on calling thread, Load/add distinctive note, session(home=source, folder=source,error=null), capture references. Move to empty sibling via real MoveNotes, assert true, same Store/Data/SettingsService references, session.DataFolder == Store.Folder == target, error null and availability true, pointer follows target; save again through original Store and verify body in target. Flush before fixture cleanup. |
| Coherent failure | Same session attempts occupied/nested target: false; folder/error/Store identity unchanged, pointer still target. Separate unavailable/error fixture verifies guard false and no target creation. These assert state after synchronous return; they do not prove cross-thread atomicity. |
| Error publication | Constructor given a reconciled unavailable error retains that error/folder pair. Successful-move/unavailable-switch race is not deterministically covered without adding a seam; keep it a source invariant and hand risk rather than adding injectable filesystem machinery for this task. |

## 9. User hand gate: dev fixtures only, all pending

| Risk | Concrete check after implementation |
|---|---|
| Isolation/startup | Verify fixture home/notes/log/pointer and dev executable path first; restart saved theme/material/split and Settings change/save/reopen. Actual renderer hint still reflects launch choice. |
| Move + identity | Unique note + slot + running timer; move to empty sibling; Storage path/Open/recovered + backup text correct, search/tray/timer still work; edit/save/restart reload moved text while log remains in original app home. Repeat move once more to catch captured initial path. |
| Refusal/unavailable | Occupied/nested target refuses without pointer/path change. Restart fixture with missing pointer target: absent folder stays absent, read-only hint correct, Settings error/actions and preview preserve existing behavior. Failed Save prevents move; warning success still displayed as notice. |
| Stale views/search | Switch notes after move; same-note binding remains existing behavior. Open Settings preview and fresh search after move; verify no constructor-captured old folder. Any in-flight old search limitation must be reported, not silently fixed. |
| Hidden/single instance/quit | Hidden primary + rapid secondary; one dev tray and correct show. Explicit fixture DATA with DEV unset then 1, user-triggered quit only affects dev; installed copy untouched. Do not test normal identity routing by launching/quitting it. |
| Session-end (A30 still pending) | Type unique final edit immediately before user-controlled sign-out/restart, reopen fixture, verify exact edit + `session-end save=ok` in home log. Ordinary Quit/retry/quit-anyway fixture checks only prove their own drain path. |
| Regression surface | Hotkey/taskbar hide/show, tray slots and Settings previews unchanged. No new performance claim: show/hide timing, OS shutdown, race behavior and installed behavior are unverified by this source audit. |

## 10. Files / overlap (current logical lines and bytes)

| Existing path | Lines | Bytes | A43b touch |
|---|---:|---:|---|
| Program.cs | 258 | 12,261 | Local composition, service statics removal, App ctor/forwarding. |
| UI/PadWindow.cs | 524 | 24,166 | Session field/ctor and timer initialization. |
| UI/PadWindow.Keys.cs | 285 | 13,208 | Search/Settings construction. |
| UI/PadWindow.Layout.cs | 493 | 28,209 | PadView view argument. |
| UI/SettingsWindow.cs | 799 | 44,494 | Session reads/move + preview view. |
| UI/SearchOverlay.cs | 396 | 18,930 | Store field/ctor/use. |
| Editor/PadView.cs | 864 | 41,353 | Read-only view injection and Bind. |
| UI/MotionPreview.cs | 299 | 13,623 | View forwarding. |
| SelfTest.cs | 2,049 | 130,257 | Rules, narrow scanner extension, static exposure, move capability. |
| UI/ISettingsHost.cs | 20 | 581 | No edit with direct-session recommendation; alternative host route only. |
| docs/dev-loop.md | 92 | 8,379 | Pointer wording. |

New: root NotesSession.cs + Model/INotesLocation.cs. RuntimeLog.cs (137 lines/3,965 bytes) and TaskbarPresence.cs need no changes.
Nine required existing product files + documentation; optional host makes ten. Preserve Store, SettingsService, NotesLocation, AppInfo, Timers, Tray, hotkeys, clipboard/math/analysis and native bodies.
A44 plan read via `git show reorg/A44-platform-seam:.coord/tasks/A44-platform-seam-plan.md`: tiers remain native facade/extern nesting/optional managed OS calls. Its Editor-only scanner premise and Program-in-GlobalHotkey/Tray claims are stale; current scanner selects all matching sources and those files have no Program tokens. Re-baseline, serialize SelfTest edits and carry universal/static guards forward; do not broaden A43b into A44.

Completion: report only (203 lines). No implementation or runtime verification performed; initial worktree was clean; final `git status --short --untracked-files=all` shows only `?? .coord/reports/A43b-notes-session-impact.md`, and tracked `git diff --stat` is empty.
