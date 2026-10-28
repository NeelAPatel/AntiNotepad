# A44-platform-seam-impact-r2

Read-only delta audit, 2026-10-27. Branch `reorg/A44-platform-seam`, HEAD `0db2e8a` (merge of dev `1e89288`); r1 baseline `b54b476`. Paths/line numbers below refer to current `product/AntiNotepad/` unless prefixed otherwise. Only this report was written. The report-only instruction overrides `AGENTS.md`'s STATUS-update rule; neither STATUS nor the plan was edited. The pre-existing untracked `.coord/tasks/A44-platform-seam-impact-r2.md` was left alone. No build, selftest, negative probe, app launch/stop, installed-copy access, or runtime measurement was performed. Thread labels are inferred from source.

## (a) What A44 still needs / no longer needs

| Finding | Disposition / evidence |
|---|---|
| Stateless native implementation is still assembly-visible | Tier 1 still useful: `Native` is internal, nine callable operations, 11 outside calls in the same seven files (`Platform/Native.cs:10-96`; caller table below). Make implementation private behind `DesktopPlatform`; retain synchronous forwards. |
| Native declarations still sit on service types | Tier 2 still useful for the strong guard: `TaskbarProxy` remains directly referenced by UI and declares four imports (`UI/PadWindow.cs:35,342`; `Platform/TaskbarPresence.cs:565-575`). `GlobalHotkey` declares two, but UI no longer directly references it (`Platform/HotkeyRegistrar.cs:14-28`; `UI/PadWindow.cs:61-66`). Its extern nesting is consistency/future-proofing, no longer necessary to clear today's UI dependency count. |
| Managed shell/process code remains in UI | Optional tier 3 remains: explorer launch `UI/SettingsWindow.cs:799-808`; two process samples `UI/PadWindow.Keys.cs:205-206,219-220`. Recommend folder launch as a separately gated optional task; defer metrics. |
| No scanner generalization required | Landed scanner already scans every matching rule source; universal Program rule brings in every assembly type except Program and its nested types (`SelfTest.cs:621,643-677,743-749`). Add interop checks to it; do not rewrite it. |
| No app-context or hotkey-policy extraction required | A40 owns settings changes (`Model/SettingsService.cs:105-269`); A41 owns hotkey swap/status policy (`Platform/HotkeyRegistrar.cs:32-120`); A43 owns log/launch/location context (`RuntimeLog.cs:6-51`; `Program.cs:182-183`; `NotesSession.cs:6-111`). Preserve these seams. |
| New compile dependency in SelfTest | Four existing exact-target rows name `typeof(Native)` (`SelfTest.cs:555,570,586,598`). Private nesting breaks them even before adding guards. Retarget those redundant helper rules to `DesktopPlatform` (the helpers already also forbid the whole Platform namespace). |
| Source/IL enforcement still missing for interop | Existing guards enforce architecture, settings setters, static service exposure and DATA/DEV reads, but no import ownership, P/Invoke-holder target, raw interop, or general UI shell ban (`SelfTest.cs:133-139,532-704,754-833`). |

## (b) Fresh inventory and counts

Counts are source occurrences/declarations, not executed calls or transitive dependency counts. `Native.` count deliberately excludes bare `typeof(Native)` so it compares with r1. Generated `obj/` output is not part of the checked-in product source inventory.

| Measure | r1 (`b54b476`) | Current (`1e89288` in HEAD) | Proposed final target |
|---|---:|---:|---|
| `Native.` outside Platform | 11, seven files | **11**, same seven files | 0 after tier 1 |
| P/Invoke-declaring types directly referenced by UI/Editor | GlobalHotkey, TaskbarProxy, Native (3) | **TaskbarProxy, Native (2)**; GlobalHotkey is now behind GlobalHotkeyRegistration | 0 after tiers 1+2 |
| DllImport declarations | 20 / three files | **20 / three files** | 20, unchanged |
| Native.cs / GlobalHotkey.cs / TaskbarPresence.cs imports | 14 / 2 / 4 | **14 / 2 / 4**, at `:108-121` / `:135-136` / `:565-575` | Same files/counts, private declaring holders |
| LibraryImport declarations | 0 | **0** | 0 (guard must support future ones) |
| Imports / actual raw interop outside Platform | 0 / 0 | **0 / 0**; backend setup exception is managed Avalonia configuration (`Program.cs:108`) | 0 / 0 |
| Outside Process.Start / GetCurrentProcess | 1 / 2 | **1 / 2**, `SettingsWindow:806`, `Keys:205,219` | 0 / 2 with folder-only tier 3; 0 / 0 only if metrics also moved |
| `Program.` tokens outside Program.cs | r1 not quantified here | **0** (fresh source search); type metadata `typeof(Program)` intentionally remains in SelfTest | Preserve 0 |
| LAYERS outputs | r1 discussed an Editor-only working-tree scanner | **88 rule rows + one settings-setter output = 89** (`SelfTest.cs:534-621,698`) | Retain existing coverage, add rows/checks below |

### Raw native/Windows implementation touchpoints

`U` UI dispatcher/input, `W` native WndProc callback on window thread, `T` UI timer, `M` initial STA/main thread, `B` background worker, `S` selftest caller. “Keep” means preserve ownership and ordering; moving an unchanged synchronous call through a forward is permitted.

| Location file:line | Member(s) | Purpose | Thread | Handle | Timing sensitivity | Can-move? |
|---|---|---|---|---|---|---|
| `Platform/Native.cs:19-30,109-110` | ListenClipboard; Add/RemoveClipboardFormatListener; Win32Properties callback/add/remove | Clipboard notification hook | U/W | Captures pad HWND once; zero returns no-op; returned Disposer owns removal | Hook added before listener; disposal removes listener before hook; no retry/create/show | Yes, forward only; UI retains token |
| `Platform/Native.cs:33,107-108` | CursorPosition, NativePoint, GetCursorPos | Physical screen cursor | U/T | No retained handle | Fresh query before positioning/containment | Yes, no cache/post |
| `Platform/Native.cs:36-54,113-119` | ClipboardIsPrivate; RegisterClipboardFormat, IsClipboardFormatAvailable, OpenClipboard, GetClipboardData, GlobalLock/Unlock, CloseClipboard; Marshal.ReadInt32 | Password/history format checks | U | Null clipboard owner; borrowed HGLOBAL/pointer scoped to call | Privacy before text await; finally unlock/close; open failure false, lock failure true, exception true | Yes, preserve full body/failure policy |
| `Platform/Native.cs:57-59,112` | TrimWorkingSet, Process.GetCurrentProcess/Handle, SetProcessWorkingSetSize(nint,nint) | Return unused pages | T | Process wrapper scoped with using | Only after existing GC sequences | Yes, schedule/GC stay in UI |
| `Platform/Native.cs:62,111` | Chime, MessageBeep | Information beep | U/T | None | Before quiet timer show | Yes |
| `Platform/Native.cs:13-16,65-77,121-124` | ConfigurePopupWindow, SetDwmWindowAttribute, DwmSetWindowAttribute | DWM corner attr 33=1, border attr 34=0xFFFFFFFE | U | Resolve popup HWND each call, zero returns | Popup exists first; corners then border; ignore failures | Yes, popup-only; no pad/Settings application |
| `Platform/Native.cs:80-85,105-106,120` | Flash, FlashInfo, Marshal.SizeOf, FlashWindowEx | Flash window | Uncalled | Borrowed HWND; scoped struct | Flags 3\|0xC/count 3; no current caller | Preserve unused operation, add no caller |
| `Platform/Native.cs:88-96` | OpenUrl / StartScreenSnip; Process.Start | http(s) / ms-screenclip shell | U | Process result not retained | URL validates, launch errors propagate; snip errors swallowed | Yes, unchanged |
| `Platform/GlobalHotkey.cs:21,34-92,135-136` | ctor/Register/WndProc/Dispose; TryGetPlatformHandle; Win32Properties hooks; RegisterHotKey/UnregisterHotKey; Marshal.GetLastWin32Error | WM_HOTKEY registration/delivery | U/W | Saved HWND; Opened retry and Closed cleanup | Hook before registration; last error immediately at :61 after :60; id check and Stopwatch callback; unregister before hook removal | Keep service; move only extern declarations into nested Win32 |
| `Platform/TaskbarPresence.cs:114-170,180-214` | TaskbarProxy ctor; RegisterWindowMessage; ShowMinimized/Hide; Handle/PadHandle; NativeShowWindow | Stable minimized stand-in, hooks on proxy/pad | U/W | Own proxy Window; borrow pad; handles queried live | Register TaskbarCreated at field initialization; state/generation before native show/hide; reentrancy bracket | Keep machine; extern qualification only |
| `Platform/TaskbarPresence.cs:216-337` | OnPadWindowMessage/GetCounterpart/OnProxyWindowMessage | WM_ACTIVATE/SHOWWINDOW/NCACTIVATE/ACTIVATEAPP, TaskbarCreated, WINDOWPOSCHANGING, MOUSEACTIVATE, SC_RESTORE | W | Borrowed hwnd/wParam/lParam and foreground identities | Snapshot/consume before focus changes; synchronous message handling; shutdown short-circuit | Keep |
| `Platform/TaskbarPresence.cs:340-482,529-531` | TraceProxyMessage; snapshot/fallback/QueueTaskbarClick; ClickSnapshot(IntPtr Foreground) | Classify foreground and choose hide/show | W, posted U | HWND as identity, no ownership | 500ms loss / 1000ms snapshot/post-hide windows; Input callback then Background reset; generation invalidation | Keep, no extra dispatch |
| `Platform/TaskbarPresence.cs:484-514,533-543` | FormatHwnd/DescribeWindow/GetWindowClass/TryGetWindowPosFlags; WindowPos; Marshal.PtrToStructure | Class query, message pointer decode | W | lParam valid during message only; WindowPos has Hwnd/HwndInsertAfter | Never postpone pointer dereference | Keep under Platform |
| `Platform/TaskbarPresence.cs:545-575` | Dispose; ShowWindow/GetForegroundWindow/GetClassName/RegisterWindowMessage imports | Remove hooks/close stand-in | U | Remove both hooks while windows live, close owned proxy | Hide then unhook then close | Keep; private nested extern holder only |
| `Platform/Native.cs:128-169` | Startup; Environment.GetFolderPath; Registry.CurrentUser/OpenSubKey/CreateSubKey/SetValue/DeleteValue | HKCU Run/canonical installed exe | M/U | RegistryKey using-scoped | Write new value before legacy delete; custom-data bypass in Program | Keep existing facade |
| `Platform/Ocr.cs:8-33` | ClipboardHasImage / ReadClipboardImage; Windows.ApplicationModel.DataTransfer, BitmapDecoder, SoftwareBitmap, OcrEngine | WinRT image/OCR | U async entry/resumption | Scoped stream/bitmap, no HWND API exposed | Do not Task.Run; preserve clipboard read/await order | Keep existing facade |
| `Platform/SingleInstance.cs:25-69,73-148` | ctor/ObjectStem/RequestQuit/Listen/Dispose; OperatingSystem, WindowsIdentity, Environment, Process.GetProcessById | User/session named mutex/events/PID map, quit protocol | M owns mutex; B listener; M client | Mutex, events, mapping/accessors, cancellation wait handle, Process scoped | Identity before quit; 5s deadline; primary Dispose must use owner thread | Keep whole service |
| `Platform/HotkeyRegistrar.cs:6-120` | IHotkeyRegistration, GlobalHotkeyRegistration, HotkeyRegistrar; env override | Adapter + two registration slots/swap/status policy | U; S fake checks | Adapter owns GlobalHotkey; registrar owns slots | Parse before swap; release/create/refusal disposal/rollback; show then new-note disposal | Keep; already extracted |
| `Platform/Tray.cs:8-50` | Tray ctor/Tooltip/Dispose | Avalonia TrayIcon/native menu callbacks | U/menu callback | Owned TrayIcon | Subscribe status before hook installation | Keep existing facade |
| `Platform/SessionEnd.cs:6-18` | IsEnding/Begin/ShouldCancelClose | Shutdown latch / pure close policy | U/S | None | Begin before synchronous flush | Keep |
| `Platform/DefaultDataFolderMigration.cs:5-56`; `Platform/NotesLocation.cs:6-269` | Resolve/Move/ReadPointer/WritePointer + records | Filesystem migration/location/pointer | M/U/S | Scoped streams, no raw handles | Lock before load/migration; verify copies before pointer, old-copy backup after commit | Keep; NotesSession now sequences live move |

All actual DllImport/Marshal/IntPtr/nint/TryGetPlatformHandle/WndProc/Win32Properties/StructLayout/DWM touchpoints are grouped above; only the first three implementation files declare or directly use raw native interop. `SelfTest.cs:2008,2040` contain `\\nint` in code fixture strings, not nint declarations. No LibraryImport exists. No raw interop was added in Editor/Model/Theme.

### Exact callable Platform type surface

All 27 declared Platform types are accounted for: 20 assembly-callable types below and seven private helper types after the table. Assembly-callable surface on internal types is listed too. Positional record properties and normal synthesized record equality/deconstruction follow their declarations; private helper types have no outside surface.

| Type / location | Explicit callable surface |
|---|---|
| Native, internal static, `Platform/Native.cs:10-96` | `IDisposable ListenClipboard(Window,Action)`; `PixelPoint CursorPosition()`; `bool ClipboardIsPrivate()`; `void TrimWorkingSet()`, `Chime()`, `ConfigurePopupWindow(TopLevel)`, `Flash(Window)`, `OpenUrl(string)`, `StartScreenSnip()` |
| Startup, internal static, `Platform/Native.cs:128-169` | `bool IsEnabled { get; }`; `void SetEnabled(bool)`; `void MigrateLegacy(Action<string>? logError=null)` |
| GlobalHotkey, public sealed, `Platform/GlobalHotkey.cs:9-95` | `DefaultGesture`; `IsRegistered`/`RegistrationError` private-set getters; `RegistrationErrorMessage`, `GestureLabel`; ctor `(Window,Action<long>,string? gesture=null,int id=0x4150,bool fromEnvironment=true)`; `Dispose()` |
| GlobalHotkey.HotkeyGesture, public readonly record struct, `Platform/GlobalHotkey.cs:95-118` | `(string Label,uint Modifiers,int VirtualKey)`; static `Parse(string)` |
| IHotkeyRegistration, internal interface, `Platform/HotkeyRegistrar.cs:6-11` | IDisposable; `string GestureLabel`, `bool IsRegistered`, `string? RegistrationErrorMessage` getters |
| GlobalHotkeyRegistration, internal sealed, `Platform/HotkeyRegistrar.cs:14-28` | ctor same parameters/defaults as GlobalHotkey; three interface getters; `Dispose()` |
| HotkeyRegistrar, internal sealed, `Platform/HotkeyRegistrar.cs:32-120` | ctor `(Func<string,Action<long>,IHotkeyRegistration> makeHotkey, same makeNewNoteHotkey, Action<long> toggleAction, Action newNoteAction, Action<string> toast)`; `HotkeyStatus`, `NewNoteHotkeyStatus`; event `Action<string>? HotkeyStatusChanged`; `bool RegisterHotkey(string?)`, `RegisterNewNoteHotkey(string?)`; `Dispose()` |
| TaskbarPresence, internal static, `Platform/TaskbarPresence.cs:13-43` | `bool ShouldShow(string?,bool)`; `ShouldHideOnClick(bool,bool)`; `ShouldSuppressPostHideActivation(bool decisionConsumed,bool lastDecisionWasHide,bool freshClickStart,bool explicitRestore,long hideDecisionAt,long now)`; `WasPadRecentlyForeground(long lastForegroundLoss,long now,TaskbarActivationCounterpart)` |
| TaskbarTrace, internal sealed, `Platform/TaskbarPresence.cs:47-83` | ctor `(bool isDev,Action<string> log)`; `bool IsDev { get; }`; `Write(string)`, `double SinceTraceStart(long)`, `TracePadAction(string cause,string phase,bool visible)`, `string PadActionContext()` |
| TaskbarActivationCounterpart, internal enum, `Platform/TaskbarPresence.cs:85-91` | Other, None, Taskbar, Proxy |
| TaskbarProxy, internal sealed, `Platform/TaskbarPresence.cs:94-575` | ctor `(Window pad,string title,WindowIcon? icon,Action<bool,string> taskbarClick,bool isDev,TaskbarTrace trace)`; `bool ConsumePadDeactivationToProxy()`; `ShowMinimized()`, `Hide()`, `Dispose()` |
| Tray, internal sealed, `Platform/Tray.cs:8-50` | ctor `(Action show,Action newNote,Action settings,Action quit,Func<IReadOnlyList<(string Title,bool Occupied)>>? slotTitles=null,Action<int>? openSlot=null)`; `string Tooltip { set; }`; `Dispose()` |
| Ocr, internal static, `Platform/Ocr.cs:8-33` | `Task<bool> ClipboardHasImage()`; `Task<(string? Text,string? Error)> ReadClipboardImage()` |
| SessionEnd, internal static, `Platform/SessionEnd.cs:6-18` | `bool IsEnding { get; private set; }`; `bool Begin()`; `bool ShouldCancelClose(WindowCloseReason,bool exiting,bool sessionEnding)` |
| SingleInstanceService, public sealed, `Platform/SingleInstance.cs:10-148` | ctor `(Action onSecondLaunch,string? applicationId=null,Action? onQuit=null)`; `bool IsPrimary { get; }`; static `int RequestQuit(string? applicationId=null)`; `Dispose()` |
| DataFolderResolution, internal readonly record struct, `Platform/DefaultDataFolderMigration.cs:5` | `(string Folder,string? Error,bool Migrated)` |
| DefaultDataFolderMigration, internal static, `Platform/DefaultDataFolderMigration.cs:8-56` | const `LegacyFolderName`; `DataFolderResolution Resolve(string localAppData,string? customFolder,Action<string,string>? moveDirectory=null)` |
| NotesLocationResolution, internal sealed record, `Platform/NotesLocation.cs:6` | `(string AppHome,string NotesFolder,string? Error,string? Notice=null,bool Custom=false)` |
| NotesMoveResult, internal sealed record, `Platform/NotesLocation.cs:8` | `(bool Success,string Folder,string? Error=null,string? Warning=null,int FileCount=0,long ByteCount=0)` |
| NotesLocation, internal static, `Platform/NotesLocation.cs:11-269` | const `PointerFileName`; `NotesLocationResolution Resolve(string appHome,string documents,string? customFolder=null)`; `NotesMoveResult Move(string appHome,string currentFolder,string targetFolder)`; `string? ReadPointer(string appHome)`; **now internal** `void WritePointer(string appHome,string notesFolder)` (`:190`) |

Private type inventory: `Native.Disposer`, `FlashInfo`, `NativePoint` (`Platform/Native.cs:99-107`); `TaskbarProxy.ClickSnapshot`, `WindowPos` (`Platform/TaskbarPresence.cs:529-543`); `DefaultDataFolderMigration.PathKind` (`:56`); `NotesLocation.LocationPointer` (`:269`). No public raw handle surface. The runtime purpose/thread/handle/timing/move decision for each accessible type is in the preceding implementation table; records/enums/interfaces carry values/contracts only and should remain beside their owning service.

### Every outside Native call (all 11)

| Location file:line | Member | Purpose | Thread | Handle | Timing sensitivity | Can-move? |
|---|---|---|---|---|---|---|
| `UI/PadWindow.cs:133` | ListenClipboard | Hold notification token | U -> posted U | Pad HWND captured by implementation | Proxy -> show hotkey -> new-note hotkey -> clipboard -> presence | Yes, name substitution only |
| `UI/PadWindow.cs:188` | CursorPosition | Open-at-mouse/screen selection | U | None | Query before show/position, physical pixels | Yes, synchronous |
| `UI/PadWindow.cs:443` | TrimWorkingSet | Hidden trim | T | Scoped process inside helper | 45s default; stop/check visibility/evict/remove-orphans/compact/GC/finalizers first | Yes, only leaf call |
| `UI/PadWindow.Keys.cs:202` | TrimWorkingSet | Closed-Settings trim | T | Scoped process inside helper | 1.5s, abort if Settings open; GC/finalizers/GC first, sample after | Yes, only leaf call |
| `UI/PadWindow.Layout.cs:308` | CursorPosition | Pointer-inside chrome sleep | U/T | None | Current position/physical containment | Yes, no cache |
| `UI/PadWindow.Navigation.cs:306` | ClipboardIsPrivate | Exclude secrets | U async | Scoped native clipboard | After snip branch/target check, **before** TryGetTextAsync | Yes, unchanged order |
| `UI/PadWindow.Navigation.cs:322` | Chime | Timer finished sound | U/T | None | Before ShowQuietly/toast | Yes |
| `UI/PadWindow.Navigation.cs:342` | StartScreenSnip | Launch snip | U | None | Saved target/caret, timestamp/pane and toast already set | Yes, same catch policy |
| `UI/PadMenu.cs:40` | ConfigurePopupWindow | Popup native border/corner suppression | U Opened | Fresh TopLevel host | Transparency/background -> configure -> content.Focus | Yes, keep event/location |
| `UI/PadWindow.TopBlur.cs:301` | ConfigurePopupWindow | Float pager host | U Loaded post | Fresh non-Window popup | IsOpen/not-pad/not-Window checks -> theme/transparency -> configure | Yes, keep post/guards |
| `Editor/Elements.cs:142` | Platform.Native.OpenUrl | Ctrl+click browser | U input | None | Mark handled first; http(s) validation/throw behavior in Native | Yes, qualified DesktopPlatform replacement |

### Other outside Platform references and managed OS calls

Rows group repeated use of the same contract; none requires a new forwarding layer. Test/metadata references are listed separately from runtime calls.

| Location file:line | Member/type | Purpose | Thread | Handle | Timing sensitivity | Can-move? |
|---|---|---|---|---|---|---|
| `UI/PadWindow.cs:34,40-44,61-66,132-133,303,462-464` | HotkeyRegistrar; GlobalHotkeyRegistration; status/event/register/dispose | Two callbacks/registrations with IDs 0x4150/0x4151 | U -> posted U | Registrar owns adapters, adapters own native registration | Factories defer construction until register; preserve posted actions, status subscription, disposal order | Keep existing boundary; no direct GlobalHotkey/HotkeyGesture UI dependency remains |
| `UI/PadWindow.cs:28,59,98,110,152,181,209,214,218,226,232,240,248,342,347,361,366,369` | TaskbarTrace field/ctor/Write/TracePadAction/PadActionContext; passed to proxy | Dev causal trace | U/posted U | None | Shared per-window trace context and Stopwatch origin; do not allocate replacement on each call | Keep; log delegate is RuntimeLog.Write |
| `UI/PadWindow.cs:35,101,131,305,333-353`; `UI/PadWindow.Keys.cs:275-276` | TaskbarProxy field/ctor/Consume/ShowMinimized/Hide/Dispose; TaskbarPresence.ShouldShow | Presence/activation policy | U/W-driven U | UI owns proxy lifetime | Consume transfer before blur; lazy creation before ApplyTaskbarSetting; unhook before pad close | Keep services; imports may nest |
| `UI/PadWindow.cs:97,109,116,151,239,253,267,394,399,405,413` | SessionEnd.IsEnding/ShouldCancelClose/Begin | Guard UI/save/shutdown | U | None | Begin before synchronous flush; no async replacement | Keep |
| `UI/PadWindow.Navigation.cs:297,362` | Ocr.ClipboardHasImage/ReadClipboardImage | Snip capture/recognition | U async | Scoped WinRT resources | 120s image branch before normal text; recognize saved target and revalidate note/document | Keep existing Ocr boundary |
| `UI/SettingsWindow.cs:523-524` | Startup.IsEnabled/SetEnabled | Startup toggle | U | Scoped registry inside service | Existing read catch/write Toast retained | Keep |
| `Program.cs:48,50-51` | SingleInstanceService.RequestQuit/ctor/IsPrimary/using Dispose | CLI/instance lifetime | M; B signal -> UI Post `:112-135` | Named objects owned here | Choose custom-data identity first; Dispose on initial thread | Keep composition |
| `Program.cs:55-59` | DefaultDataFolderMigration.Resolve, inferred DataFolderResolution fields; Startup.MigrateLegacy(log.Write) | Data/registry migration | M | Scoped managed I/O | Primary lock before migration, custom-data skips Run-key migration | Keep |
| `Program.cs:62,65-76,87-88` | NotesLocationResolution type/ctor/with/getters; NotesLocation.Resolve | Startup path/error/notice | M | None raw | Resolve before Store construction/load and AppLaunch | Keep composition |
| `NotesSession.cs:38-45,86` | NotesLocation.Move/WritePointer; inferred NotesMoveResult getters | Live relocation/recovery | U | Scoped files | Save -> move -> SwitchFolder -> publish location/error; next-start recovery writes pointer only | Keep new coordinator; do not route through DesktopPlatform |
| `Program.cs:191,223-228` | Tray field/ctor/Tooltip/Dispose | Tray lifetime/menu/status | U | Owned TrayIcon | Tray + tooltip subscription before InstallHooks; InstallHooks before Attach/show/prewarm | Keep |
| `UI/SettingsWindow.cs:799-808` | ProcessStartInfo, ArgumentList, Process.Start | explorer folder launch | U | Process result unused | Directory.Exists and Toast catch stay in UI | Shell body can move synchronously to OpenFolder |
| `UI/PadWindow.Keys.cs:205-206,219-220` | Process.GetCurrentProcess, PrivateMemorySize64/WorkingSet64/Threads.Count | Dev memory samples | T/U async local Mem | using-scoped Process | Sample only under _isDev; first after trim; preserve measurement order if moved | Optional metrics leaf only; defer |
| `Program.cs:13,29,43,46,54,61,102` | Environment.GetEnvironmentVariable(GPU_CACHE_MB/DATA/DEV/RENDER), SetEnvironmentVariable(telemetry), GetFolderPath(LocalApplicationData/MyDocuments) | Backend/identity config | M | None | DATA/DEV decisions before quit; rendering before backend start | Must stay composition; existing DATA/DEV confinement |
| `UI/PadWindow.cs:428`; `UI/PadWindow.Keys.cs:22,215` | Environment.GetEnvironmentVariable(TRIM_SECONDS/SHOTS/DEBUG_SETTINGS_CYCLES) | Dev/timer tuning | U/T | None | Preserve defaults/read time; field initializer reads trim delay, key event reads shots | Keep managed config; no blanket Environment ban |
| `Model/SettingsService.cs:109,266` | Environment.CurrentManagedThreadId | New owner-thread enforcement | Constructor/update caller | No OS handle | Capture once, assert before settings changes | Must stay service invariant |
| `Editor/Motion.cs:94,126,195`; `UI/NoteSwipe.cs:95,125,130,158`; `SelfTest.cs:1732` | Environment.TickCount64 | Motion/debounce/test clock | U/frame/T/S | None | Read at current motion step | Keep managed clock |
| `RuntimeLog.cs:117` | Environment.NewLine | Append log line | B | Managed file writer | After timestamp captured in queueing caller `:77-78` | Keep; not platform leakage |

Outside Platform test/metadata references (including inferred result-record property use in the cited scenario spans):

| Location file:line | Member/type | Purpose | Thread | Handle | Timing sensitivity | Can-move? |
|---|---|---|---|---|---|---|
| `SelfTest.cs:153,178,197,202-297,542-544` | IHotkeyRegistration contract, HotkeyRegistrar constructor/register/status/event/Dispose; exact rule metadata | Fake swap/refusal/rollback/idempotent-disposal checks | S | Fake registrations only | Ordered dispose/create assertions | Keep test access; no native registration invoked |
| `SelfTest.cs:456-473` | TaskbarTrace ctor/Write/TracePadAction/PadActionContext | Trace prefix/suppression/context capabilities | S | None | Stopwatch comparisons, same trace instance | Keep |
| `SelfTest.cs:555,570,586,598` | Native exact-target metadata | Pure helper dependency bans | S table construction | None | Must compile when holder becomes private | Retarget to DesktopPlatform, keep namespace bans |
| `SelfTest.cs:1122,1131-1135,1146,1152-1155,1164,1171-1174,1182,1186-1192` | DefaultDataFolderMigration.LegacyFolderName/Resolve; DataFolderResolution inferred Folder/Error/Migrated | Temp-root migration/custom/failure cases | S | Managed fixture files only | Injected move failure, no live folders | Keep |
| `SelfTest.cs:1331,1334,1346,1349,1364-1365,1372-1373,1435,1442-1443,1483,1496,1502,1512,1568,1577,1582,1590,1592,1598,1600,1607,1610` | NotesLocation.Resolve/ReadPointer/WritePointer/PointerFileName; NotesLocationResolution inferred properties | Temp-root resolution/move/session/recovery capabilities | S | Scoped fixture files | Pointer assertions after successful move/recovery and refused operations | Keep; no desktop facade access needed |
| `SelfTest.cs:1938,1941-1945,1949-1954,1956-1961,1965-1969` | TaskbarPresence four policy methods; TaskbarActivationCounterpart values (`:1949-1954,1956,1959`) | Pure presence/click/loss/suppression truth tables | S | No HWND | 500ms/1000ms policy bounds | Keep; does not establish native behavior |
| `SelfTest.cs:1981,1983-1985` | SessionEnd.ShouldCancelClose | Pure close policy | S | None | Does not call Begin or mutate shutdown state | Keep |

Outside namespace imports (not executable calls): `Program.cs:6`, `NotesSession.cs:2`, `SelfTest.cs:2`, `UI/PadWindow.cs:13`, `UI/PadWindow.Keys.cs:7`, `UI/PadWindow.Layout.cs:14`, `UI/PadWindow.Navigation.cs:8`, `UI/PadWindow.TopBlur.cs:7`, `UI/PadMenu.cs:9`, `UI/SettingsWindow.cs:14`; Editor uses the fully qualified call at `Editor/Elements.cs:142`. Avalonia.Platform imports are unrelated to AntiNotepad.Platform. No other outside Platform type references were found.

No outside Registry calls, WindowsIdentity calls, OperatingSystem checks, or Windows.* APIs were found. `GlobalHotkey`, `Tray`, and `TaskbarPresence` do **not** call Program.IsDev/Log: the first two still use AppInfo, while TaskbarProxy receives bool/TaskbarTrace (`Platform/GlobalHotkey.cs:39`; `Platform/Tray.cs:14`; `Platform/TaskbarPresence.cs:133-143`). AppLaunch is an `AntiNotepad` record carrying Log/IsDev/etc, **not** a Platform type and not a new dependency of GlobalHotkey/Tray (`Program.cs:182-183,212`).

Framework scope remains explicit: Avalonia clipboard text (`UI/PadWindow.Navigation.cs:289-313`), Screens/window lifecycle (`UI/PadWindow.cs:73-78,149-248`), material/theme (`:472-477`), popup host lookup (`UI/PadMenu.cs:34-42`; `UI/PadWindow.TopBlur.cs:292-302`), render scaling/frame callbacks, and AvaloniaEdit editing remain in views. Model file persistence remains in Store; new NotesSession recovery uses managed Directory/Path (`NotesSession.cs:60-86,97-108`). The A42 helpers use text/document/math logic, no OS API (`Editor/MathNotes.cs:1-54`; `ListEditing.cs:1-19`; `NoteAnalysis.cs:1-272`); Theme has no new OS touchpoint (`Theme/ChromeTokens.cs:1-98`). A39 moved `Anim` to `Motion/Anim.cs:3-6`, `ChromeTokens` to `Theme/ChromeTokens.cs:1-4`, and menu contracts to `Editor/MenuEntry.cs:1-14`; the old UI ownership must not be restored.

## (c) Delta table vs r1 plan

Plan references below mean `.coord/tasks/A44-platform-seam-plan.md`; r1 report references mean `.coord/reports/A44-platform-seam-impact.md`. STILL TRUE refers to behavior/scope, not its obsolete line number. NEW identifies an addition needed in the refreshed implementation brief.

| r1 claim / plan location | Verdict | Current evidence / correction |
|---|---|---|
| What tier 1, nine operations, private Native, Startup stays namespace-level (`:9-18`) | STILL TRUE | `Platform/Native.cs:10-169`; all 11 calls in table (b). NEW: fix four exact Native table tokens as part of this step (`SelfTest.cs:555,570,586,598`). |
| Seven caller files / 11 calls (`:15-18`) | STILL TRUE | Exact owners/count unchanged, only lines shifted; Elements is now `:142`, menus `:40,301`, main window `:133,188,443`. |
| Tier 2 needed for direct GlobalHotkey + TaskbarProxy UI uses (`:20-25`) | STALE in part | TaskbarProxy still direct; GlobalHotkey now behind adapter (`UI/PadWindow.cs:61-66`; `Platform/HotkeyRegistrar.cs:14-28`). Extern-only nesting still valid for both (`GlobalHotkey.cs:135-136`; `TaskbarPresence.cs:565-575`). |
| Tier 3 folder + two diagnostics bodies (`:27-32`) | STILL TRUE | `SettingsWindow.cs:799-808`; `Keys.cs:205-206,219-220`; preserve catches/sample timing. Folder-only recommendation still reasonable. |
| Kept GlobalHotkey / TaskbarProxy / TaskbarPresence policy+trace (`:34-36`) | STALE surface, STILL TRUE ownership | HotkeyRegistrar now owns slots; TaskbarTrace split out; proxy ctor gained bool/trace (`HotkeyRegistrar.cs:32-120`; `TaskbarPresence.cs:47-83,133`). Do not wrap these again. |
| Kept Tray / Startup / Ocr / SingleInstanceService / SessionEnd / NotesLocation / DefaultDataFolderMigration (`:34-36`) | STILL TRUE | Current surfaces in (b); new public-to-assembly WritePointer (`NotesLocation.cs:190`) used by NotesSession (`:86`), not a reason for static desktop forwarding. |
| Why: internal Native permits arbitrary same-assembly references (`:40`) | STILL TRUE | `Native.cs:10`; four pure-helper exact bans restrict those helpers only (`SelfTest.cs:555,570,586,598`). |
| Why: popup-only contract/DWM risk (`:41-43`) | STILL TRUE | `Native.cs:65-77`; both UI popup calls remain; `docs/decisions.md:18-23` forbids frame-style retry outside lab. Facade alone cannot guarantee correct popup argument. |
| Why: Editor Native.OpenUrl (`:44`) | STILL TRUE | `Editor/Elements.cs:142`; A39 removed UI dependency, not this allowed Platform call. |
| Why: no automated Win32-in-Platform guard (`:45-46`) | STILL TRUE, bounded | No interop ownership/raw-API predicate in current table/scanner (`SelfTest.cs:133-139,532-704`). Architecture guards now much broader than r1. |
| How 1 rebaseline and new A41 owners (`:49-51`) | STALE premise, rebaseline DONE | HEAD merge includes dev `1e89288`; only hotkey policy/clipboard pure rules moved. Native callers remain at old files, table (b). No additional merge needed in this audit. |
| How 2 forwards/private nesting/no new scheduling (`:52-54`) | STILL TRUE + NEW dependency | Add four SelfTest row retargets in same compiling unit (`:555,570,586,598`); otherwise follow exactly. |
| How 3 extern nesting/error adjacency (`:55-56`) | STILL TRUE | `GlobalHotkey.cs:60-61,135-136`; TaskbarProxy declarations now `:565-575`, including initializer call `:114`. |
| How 4 optional managed bodies (`:57`) | STILL TRUE | Folder/metrics still outside; separate optional gate proposed in (g). |
| How 5 broaden Editor-only scanner (`:59-61`) | STALE | Already all matching sources (`SelfTest.cs:651,743-749`) plus universal except-Program rule (`:621`). Do not broaden/rebuild again. |
| How 5 add holder target / import ownership / source guard / capability (`:62-70`) | STILL TRUE additions; rationale corrected | Current table lacks these kinds (`:133-139,836-842`). Reflection **can** see resolved platform-handle method declaring types in IL (`:1060-1083`); source guard is complementary, not because method use is intrinsically invisible. |
| How 6 deliberate violations/build/selftest (`:72-74`) | STILL TRUE future gate | Scanner already emits failures/nonzero Run result (`SelfTest.cs:19-22,129-130,692-703`); no new guard or probe executed here. |
| How 7 commits/status/no lifecycle changes (`:75`) | STILL TRUE implementation workflow, not this audit | Manager commits per `docs/agents.md:23-26`; report-only audit exception above. |
| Unchanged show/hide/prewarm (`:77-81`) | STILL TRUE, updated locations | `PadWindow.cs:149-181,207-219,237-248`; focus/post/animation and delayed trim preserved. |
| Unchanged hook install/dispose (`:78`) | STILL TRUE sequence, STALE owner | `PadWindow.cs:129-135,296-307`; `_hotkeys.Dispose()` now handles show then new-note (`HotkeyRegistrar.cs:113-118`). |
| Unchanged hotkey rollback (`:78`; r1 report c) | STILL TRUE, STALE location | In `HotkeyRegistrar.cs:81-97`, not PadWindow; callbacks still posted by UI factory composition (`PadWindow.cs:61-66`). |
| Unchanged popup configure/trim/STA privacy/mutex/backend (`:79-81`) | STILL TRUE | `PadMenu.cs:34-42`; `TopBlur.cs:292-302`; `PadWindow.cs:427-444`; `Keys.cs:191-220`; `Navigation.cs:295-315,337-399`; `SingleInstance.cs:133-147`; `Program.cs:108`. |
| Success counts/zero direct declaring-type refs (`:83-96`) | STALE before list, STILL TRUE target | Current before set has 2 types, not 3. Folder-only leaves two GetCurrentProcess calls; don't claim full managed seam unless metrics also moved (b). |
| Success preserve LAYERS and hand checks (`:94-105`) | STILL TRUE, scanner baseline STALE | Preserve 89 current outputs (four Native labels intentionally renamed), add new ones. Hand tests still required, scoped in (g). |
| Impact radius 9/12/13 files, SelfTest only added in tier 2 (`:107-110`) | STALE | Tier 1 already needs SelfTest due four typeof(Native) rows; current estimate (h). Program/Model/Theme/Calc/runtime coordinators need no edits. |
| Risk 1 popup DWM/frame drift (`:113-114`) | STILL TRUE | `Native.cs:65-77`; `PadWindow.cs:75-78`; `docs/decisions.md:18-23`. |
| Risk 2 taskbar extra dispatch (`:115-116`) | STILL TRUE | `TaskbarPresence.cs:209-214,372-482`; preserve native reentrancy/generation/Input/Background decisions. |
| Risk 3 hook order / zero HWND (`:117`) | STILL TRUE | `Native.cs:19-30`; `PadWindow.cs:129-135,303-306`. |
| Risk 4 last error / registration order (`:118`) | STILL TRUE | `GlobalHotkey.cs:60-61`; `HotkeyRegistrar.cs:85-95`. |
| Risk 5 privacy / WinRT workers (`:119`) | STILL TRUE | `Navigation.cs:297-311,337-399`; `Ocr.cs:10-33`. Policy extraction did not move privacy or OS access. |
| Risk 6 guard too strict/lax and Editor-only scanner (`:120-121`) | STALE scanner premise; STILL TRUE boundary concern | Namespace bans are directional; direct holder detection must not mark enclosing facades as import holders merely because nested Win32 declares imports (`SelfTest.cs:836-849`). |
| Risk 7 A41 moves callers (`:122`) | STALE except hotkeys | Native caller list unchanged; only `HotkeyRegistrar` and `ClipboardCapturePolicy` landed (`PadWindow.cs:61-66,133`; `Navigation.cs:287-315`). |
| Risk 8 shell catches (`:123`) | STILL TRUE | `Native.cs:88-96`; `SettingsWindow.cs:801-808`. |
| r1 report risks 6/7/8/10: shutdown, stale popup, trim, shell/startup | STILL TRUE | `PadWindow.cs:265-285`; `SingleInstance.cs:133-147`; `TopBlur.cs:294-301`; `PadWindow.cs:434-444`; `Keys.cs:193-206`; `Native.cs:88-96,133-136`. |
| Conflicts A39: pending ownership moves (`:126-127`) | STALE status | Moves landed at `Motion/Anim.cs:3`; `Theme/ChromeTokens.cs:1`; `Editor/MenuEntry.cs:1`. Keep imports/current scanner. |
| Conflicts A40: future settings propagation (`:128-129`) | STALE status | SettingsService + ordered Changed handler landed (`Program.cs:213-214`; `PadWindow.Keys.cs:269-284`). A44 must not take subscriptions/registration into facade. |
| Conflicts A41: clipboard watcher/taskbar/trim all extracted (`:130-132`) | STALE | Those remain UI; hotkeys extracted, clipboard pure rules only (`PadWindow.cs:129-135,333-353,427-444`; `Navigation.cs:287-399`). |
| Conflicts A42: commands future / shared SelfTest (`:133`) | STALE status, STILL TRUE isolation | MathNotes/ListEditing/NoteAnalysis landed (`Editor/PadView.cs:38,459,702-782`); Elements remains independent. Four exact Native guard rows are new overlap. |
| Conflicts A43: Program.IsDev/Log in GlobalHotkey/Tray/taskbar (`:134-135`) | STALE | GlobalHotkey/Tray unchanged since baseline and use AppInfo; TaskbarTrace receives log/bool; UI gets AppLaunch values (`GlobalHotkey.cs:39`; `Tray.cs:14`; `TaskbarPresence.cs:56,133`; `Program.cs:182-183,212`). No runtime-context work for A44. |
| Conflicts order / serialize SelfTest (`:136-137`; r1 report f's alternative A44-before-A41 order) | STALE historical order, STILL TRUE serialization | A39-A43b already in dev; work only from current owners. Current SelfTest has universal and exact-type guards absent in r1 (`:534-621`); preserve them. |
| Needs user: tier 3/name (`:139-141`) | STILL TRUE optional scope choice | Recommend DesktopPlatform, tiers 1+2, optional folder-only A44c. This is a proposal; implementation is not authorized by this audit. |
| New A43b notes boundary | NEW | NotesSession sequences Move/WritePointer (`:38-45,86`), Model/Editor see INotesLocation (`Model/INotesLocation.cs:3-7`; `Editor/PadView.cs:33,80,275`). Keep it separate from desktop interop. |

## (d) Landed LAYERS capabilities

Single implementation file: `SelfTest.cs` (no split SelfTest file found). Table shape is `LayerRule(From,To,AnyAntiNotepadNamespace=false,ExactFrom=null,ExactTo=null,AnyApplicationTypeExceptTarget=false)` (`:133-139`). Counts below sum to 88 rows; the setter-owner result is the 89th output.

| Rule source(s) | Existing targets / count | Evidence |
|---|---|---|
| Editor; Model; Calc; Theme; Platform | Editor->UI; Model->UI/Editor; Calc->any other app namespace; Theme->UI/Editor; Platform->UI/Editor: 8 rows | `SelfTest.cs:534-541` |
| HotkeyRegistrar exact + nested | PadWindow/PadView/Program: 3 | `:542-544` |
| ClipboardCapturePolicy exact + nested | UI/Platform/Motion, PadWindow/PadView/Program/Native, Window/Control/Dispatcher/TextDocument: 11 | `:545-547,595-602` |
| MathNotes exact + nested | UI/Platform/Motion/Theme; PadWindow/PadView/Program/Native; Window/Control/Dispatcher/TextDocument/Timers/Settings/SettingsService: 15 | `:548-562` |
| ListEditing exact + nested | Same 15 plus CheckboxGenerator: 16 | `:563-578` |
| NoteAnalysis exact + nested | PadView; UI/Platform/Motion/Theme; PadWindow/Program/Native; Window/Control/Dispatcher/Settings/SettingsService/TextEditor/TextArea/TextView: 16 | `:579-594` |
| Platform, Model | Program: 2 | `:603-604` |
| RuntimeLog exact + nested | UI/Editor/Platform/Program/Avalonia.Application: 5; label Avalonia means exact Application, not whole framework | `:605-609` |
| SettingsWindow; SettingsService | SettingsWindow->PadWindow; SettingsService->UI/Editor: 3 | `:610-612` |
| Editor, UI, App | Program: 3 | `:613-615` |
| Model, Editor; INotesLocation | Model/Editor->NotesSession; INotesLocation->UI/Editor/Platform: 5 | `:616-620` |
| Every assembly type except Program + its nested types | Exact Program + nested: 1; no namespace restriction on source assembly filter | `:621,743-749` |

| Capability | Exact extent / gap |
|---|---|
| Source/target semantics | Namespace+subnamespace matching uses AntiNotepad prefix; exact source and target include nested/compiler-generated descendants (`:743-749,836-849,945-949`). Any-other-app-namespace supports Calc; universal rule excludes only Program tree. No generic arbitrary predicate or P/Invoke target kind exists yet. |
| Selection / signatures | Assembly.GetTypes, then any matching rule source (`:643-677`); type/base/interfaces, fields, property/indexer types, events, constructors/static initializer/methods, return/parameter types/generic constraints (`:657-677,951-986`). Generated closures/async state machines appear as nested assembly types, so their fields and MoveNext IL are included. |
| Type recursion / IL | Element types, generic definitions/arguments/constraints, function-pointer signature types (`:951-968`); resolve InlineType/InlineMethod/InlineField/InlineTok with generic context; referenced method declaration/parameter/return/generic types (`:1055-1103`). Direct dependency graph only, not recursive callee body expansion. Thus UI->adapter does not become UI->GlobalHotkey transitively. |
| Output / failure | Unique source-target type pairs, sorted; not call counts (`:683-694`). Partial type load, reflection failures, unknown/truncated/unsupported IL and resolution errors become failures (`:643-647,680,702-703,1007-1088`); final Run returns 1 on failures (`:129-130`). |
| Intentional metadata exception | Only Program ldtoken in SelfTest.CreateLayerRules is allowed, not all SelfTest access (`:674-677,1078-1079`). Retain it when adding guard metadata. |
| Existing setter ownership guard | Whole assembly app callers, settings setter calls inspected through ScanMethod callback; allowances for service/type initializers and named methods/lambdas (`:624-635,707-740`). Emits `LAYERS Settings.set_* outside owner N` (`:698`). |
| Additional guards, not source-text guards | Static service fields/properties for exact Store/SettingsService/RuntimeLog/NotesSession types (`:754-783`); DATA/DEV env-call and ldstr co-occurrence confinement with required Program observations (`:786-833`). Neither reads C# files. Static guard explicitly excludes object/delegate/dictionary locators; env key guard is not dataflow tracing. |
| No interop source enforcement today | No C# source lexer/read/fail-on-match guard for DllImport/LibraryImport/native handle access in SelfTest or a dedicated tools guard found. Existing identity/string checks do not provide that guarantee (`:786-833,1055-1083`). |
| Scanner limits relevant to A44 | No inspection of method-body local-variable signatures, exception catch types, custom attributes, or resolved InlineSig signatures (`:989-1092`). InlineSig operand is skipped by size, not decoded. A local-only unused IntPtr may evade current signature/IL scanning; source guard must catch it. String-built reflection/dynamic lookup is outside the type graph. |
| Existing negative-proof hooks | ScanMethod exposes `inspectCall`, `allowedProgramTypeToken`, `inspectString` (`:971-974`); diagnostics/failure exit support manager-inserted probes. Fake registrations/capability checks exercise hotkey policy (`:151-307`), not interop guard mutation. Historical manager probes are recorded in `.coord/reports/A40-settings-change-event.md:52-54` (SettingsWindow->PadWindow, SettingsService->UI/Editor) and `A40-settings-change-event-r2.md:34-38` (setter). These are historical evidence, not rerun here. No dedicated interop probe switch/fixture exists. |

There is **no blanket UI/Editor -> Platform or Native ban**. Pure helper exact-source rows already forbid Platform (and redundant Native); those do not cover LinkGenerator or UI. Existing Platform->UI/Editor rows express the reverse direction (`SelfTest.cs:540-541,546,549,564,581,555,570,586,598`). A44 adds ownership and raw-interop restrictions, not source-scan generalization.

## (e) Facade and guard proposal

Proposed implementation contract (not implemented): `internal static partial class AntiNotepad.Platform.DesktopPlatform`, nine synchronous forwards with unchanged Native bodies:

```csharp
internal static PixelPoint CursorPosition();
internal static IDisposable ListenClipboard(Window window, Action changed);
internal static bool ClipboardIsPrivate();
internal static void Chime();
internal static void TrimWorkingSet();
internal static void ConfigurePopupWindow(TopLevel popup);
internal static void Flash(Window window); // no new caller
internal static void OpenUrl(string url);
internal static void StartScreenSnip();
// Optional A44c-shell-folder:
internal static void OpenFolder(string path); // explorer.exe, ArgumentList.Add, no catch here
// Deferred unless separately selected:
internal static (long PrivateBytes, long WorkingSetBytes, int ThreadCount)
    ReadCurrentProcessMetrics();
```

Place forwards in new `Platform/DesktopPlatform.cs`; wrap current Native body in a second DesktopPlatform partial in `Platform/Native.cs`, with `private static class Native`; Startup remains a namespace-level sibling. Keep Native's private helpers/externs inside Native. In GlobalHotkey and TaskbarProxy, nest **only externs** in a `private static class Win32`, qualify existing calls (including TaskbarCreated initializer), preserve message structs/Marshal calls/lifetimes. Private holder methods need enclosing-callable member visibility (e.g. `internal static extern` inside the private holder); do not assume an outer type can call private members of its nested type. Do not make holders assembly-visible. Existing public service surfaces remain unchanged (inventory b).

Retarget the four exact helper->Native rows to helper->DesktopPlatform with `ExactTo: typeof(DesktopPlatform)`; keep their separate whole-Platform rules. Do not expose private Native merely for testing or add an accessor. Reflection can locate nested Native for the capability check, with a missing-type failure (`SelfTest.cs:555,570,586,598`).

| Guard addition compatible with existing scanner | Exact proposed enforcement / negative proof for future implementation gate |
|---|---|
| New LayerRule target kind: P/Invoke-declaring type | Add optional `bool PInvokeDeclaringType=false` (or equivalent small target enum), extend MatchesRule before namespace fallback. Add `new("UI","pinvoke-holder",PInvokeDeclaringType:true)` and Editor equivalent. Predicate: **DeclaredOnly** methods on target have `MethodAttributes.PinvokeImpl` (optionally DllImport attribute corroboration). Never recursively mark enclosing facades merely because nested Win32 has imports. Generated LibraryImport actual extern holders are caught. Negative: temporarily expose a holder and add unused UI and Editor calls, including an async/lambda path; selftest names both types and fails, then remove. Before holder extraction current UI->TaskbarProxy/Native must be detected. |
| Whole-assembly import ownership | Inspect all assembly methods, including Program and nested/generated types, outside the source-selection filter; every PinvokeImpl declaring type must be in Platform/subnamespace. Emit `LAYERS pinvoke-outside-Platform N`; partial load/inspection failure fails. Negative: uncalled Model DllImport; then LibraryImport on partial Model type. Generated imported methods must fail ownership, source declaration must also fail source guard. |
| Exact raw-type targets using existing ExactTo | UI and Editor rows to `Marshal`, `IntPtr`, `UIntPtr`, `Avalonia.Controls.Win32Properties` (8 rows); keep allowed facade/window types. An external-namespace-prefix predicate can additionally reject Windows.* API targets in UI/Editor (2 rows), without misusing existing InNamespace, which prepends AntiNotepad. Negative: UI Marshal.SizeOf<int>(), unused IntPtr field/signature, then WinRT API reference in unused async method. Existing fields/signatures/IL scanner catches these; local-only native type needs source guard. |
| Fail-on-match source guard | Add one small repo check (prefer `tools/check-platform-seam.ps1`, invoked explicitly by manager gate; do not make installed selftest depend on repo source). Scan tracked product .cs **plus newly added product source** at the gate, exclude generated obj/bin only. Detect actual tokens/qualified attributes outside Platform: DllImport/DllImportAttribute/LibraryImport/LibraryImportAttribute, Marshal, IntPtr/UIntPtr/nint/nuint, TryGetPlatformHandle, Win32Properties, WndProc hook access, DWM entry points. Ignore comments/string literal text, but inspect interpolation expressions; report file:line, fail on violation or unreadable/missing source. Permit only exact `typeof(...)` raw-type metadata in SelfTest.CreateLayerRules/the named facade capability check; do not blanket-exempt SelfTest or permit actual calls/fields there. Document lexer limits/aliases; IL rules cover qualified type aliases that survive compilation. Negative: unused UI nint local and uncalled import; escaped fixture `\\nint`, comments and literal token names must pass; actual SelfTest Marshal call must still fail. A raw grep printed for review is not enforcement. |
| Minimal capability check | Expected nine facade signatures, internal facade, private nested Native and private Win32 holders; no facade-visible IntPtr/UIntPtr/SafeHandle parameters/results. Do not call shell/registry/listeners/windows. Negative: missing/renamed method fails compile/signature check; changing holder privacy fails reflection check. Reflection lookup failure itself fails. |
| Optional managed shell guard | If folder-only A44c chosen, use ScanMethod.inspectCall to reject Process.Start in UI/Editor; leave GetCurrentProcess allowed/documented. If metrics also selected, add exact UI/Editor->Process and ProcessStartInfo bans. Registry/RegistryKey/WindowsIdentity can have exact-type rules independently (currently zero outside Platform). Negative: restore unused explorer Process.Start body; fail then revert. Do not impose Process-type ban while metrics intentionally stay. |

Do not add UI/Editor->all Platform, global Environment, Avalonia clipboard/window/Screens, Dispatcher/frame callback, general File/Directory, or all OS-clock bans. These would reject intended framework/persistence/composition behavior already inventoried. Retain every universal Program/NotesSession/static-service/DATA-DEV/settings guard. New rows should have deterministic labels and counts, e.g. `LAYERS UI->pinvoke-holder 0 refs`; new source script should print its own clear pass/failure line. A44's new guards may name target metadata in SelfTest without violating UI/Editor rules; do not suppress the universal Program rule to accommodate them.

## (f) Risks ranked

| Rank | Risk / preservation rule | Evidence |
|---|---|---|
| 1 critical | Wrong popup argument/frame-style edits lose pad corners/shadow/bar geometry. Forwarding is structural; do not change native style bodies, add pad/Settings calls, or weaken popup guards. | `Native.cs:65-77`; `PadWindow.cs:75-78`; `TopBlur.cs:296-301`; `docs/decisions.md:18-23` |
| 2 high | Taskbar ordering drift: extra posting/allocation/context recreation changes foreground evidence or duplicate suppression. Nest externs only; preserve initializer, native-visibility bracket, snapshot/generation/Input/Background ordering. | `TaskbarPresence.cs:114,209-214,372-482`; `PadWindow.cs:101,333-353` |
| 3 high | Hook/registration lifetime drift: listener zero-HWND no-op has no retry; adapter factories must remain lazy; registrar must dispose before clipboard/proxy/pad. Last-error capture must remain directly after RegisterHotKey. | `Native.cs:19-30`; `GlobalHotkey.cs:48-65`; `PadWindow.cs:61-66,129-135,303-306` |
| 4 high, A41 split boundary | Moving policy across new seams changes behavior: RecordOwnCopy **before** SetTextAsync; privacy **before** TryGetTextAsync; CheckText after await; Decide updates dedupe state before document insertion; keep policy per-window. Registrar: preserve parse/normalized no-op/release/create/refusal dispose/rollback/status event, plus existing UI posts. These are new owner-boundary hazards, not demonstrated new defects. | `Navigation.cs:287-315`; `ClipboardCapturePolicy.cs:16-34`; `HotkeyRegistrar.cs:61-118`; `PadWindow.cs:61-66`; `Program.cs:227-229` |
| 5 high | OCR/snip target/STA drift: retain saved note/document/caret before shell and 120s image path; await on UI, revalidate target after recognition, do not insert into whatever pane is current. | `Navigation.cs:297-301,337-399`; `Ocr.cs:10-33` |
| 6 medium | Guard false positives/holes: old typeof(Native) fails compile; recursively labeling outer holders rejects facades; scanner does not inspect local-only native types; text grep can mistake fixtures or ignore interpolation code. Preserve all 89 outputs and source/IL complementary coverage. | `SelfTest.cs:555,570,586,598,698,951-1092`; guard proposal (e) |
| 7 medium | Hidden trim/Settings sampling moves into hot toggle path or measurements shift. Keep both timers/GC sequences and _isDev checks at callers, no caching process samples. | `PadWindow.cs:237-247,427-444`; `Keys.cs:191-234` |
| 8 medium | Shell catch/quoting drift: URL throws, snip swallows, folder uses ArgumentList and UI Toast; preserve each independently. | `Native.cs:88-96`; `SettingsWindow.cs:799-808` |
| 9 high if accidentally expanded; outside change scope | Shutdown/session/mutex or NotesSession save/move ordering lost by broad facade/runtime cleanup. Touch none of these bodies. | `PadWindow.cs:265-285`; `SingleInstance.cs:133-147`; `NotesSession.cs:24-46,49-94`; `Program.cs:218-231` |

## (g) Handling recommendation

Keep tiers 1+2, split into two tasks with hand gates; optional folder shell task third. Do not expand A44 into a new desktop runtime or repeat A39-A43. All scopes below are proposed, not performed.

| Order / task | Exact scope | Gate / user's hand checks in isolated dev copy |
|---|---|---|
| 1. **A44a-native-facade** | New DesktopPlatform nine forwards; private nested Native, Startup left alone; 11 caller substitutions in seven files; four SelfTest exact-target retargets + small facade visibility/signature capability. No extern moves in other services, no hotkey-policy/shell/metrics changes. | Manager build 0 warnings/errors + selftest, source diff/lifecycle review. User: hidden start and clipboard capture/private clipboard; fast hotkey toggles/new-note/focus and mouse/right-monitor placement; timer chime, Ctrl+click URL, snip/OCR with pane switch/deleted target; context menu + float pager; corners/shadow/bars in Docked/Overlay/Float; hidden and Settings-close trim still delayed. |
| 2. **A44b-interop-guards** | Nest GlobalHotkey/TaskbarProxy externs and qualify callers/initializer; add P/Invoke target predicate + 2 rows, whole-assembly import ownership, UI/Editor raw-type/Windows target rows, source guard and its manager gate; retain all landed rules. No service lifetime/state/dispatch changes. | Manager build/selftest plus deliberate unused import/holder/UI Marshal/field/local/async violations, remove probes, then clean pass; source guard comments/strings/interpolation checks. User: hotkey same/change/conflict rollback/disable/re-enable/new-note; tray status, hidden startup; taskbar Never/While visible/Always, rapid taskbar hides/shows, blur transfer, Explorer restart/button recovery; quit/save/restart in dev only. |
| 3. **A44c-shell-folder** (optional) | Move only explorer ProcessStartInfo/ArgumentList/Start to DesktopPlatform.OpenFolder; validation/catch/Toast remain in SettingsWindow. Add UI/Editor Process.Start member guard. Defer ReadCurrentProcessMetrics and both samples. | Manager build/selftest + unused Process.Start negative proof. User: Open notes/recovered folder, path with spaces, unavailable-folder Toast, move location then Open uses current folder. No installed-copy upgrade. |

A44a-native-facade must precede strong guard enforcement so today's UI Native references can clear; A44b-interop-guards can demonstrate its predicate against pre-extraction TaskbarProxy before nesting. Keep guard probes confined to disposable implementation work and remove them before final review. A44c-shell-folder is not a prerequisite for native seam enforcement. If the user prefers only two gates, bundle folder-only code into A44a-native-facade and explicitly keep its member guard for A44b-interop-guards; do not silently include metrics.

## (h) Estimate and verification limits

Changed-line estimates are planning ranges (added+deleted), exclude report/docs and wholesale reindent churn; they are not measured implementation diffs. Counts include newly created product files.

| Scope | Product files / other files | Approximate changed lines |
|---|---|---:|
| A44a-native-facade | **10 product**: seven callers + Native.cs + new DesktopPlatform.cs + SelfTest.cs (r1's 9 omitted the newly required guard-row edits) | 80-150: 11 substitutions, nine forwards, nesting, four row retargets/capability |
| A44b-interop-guards | **3 product**: GlobalHotkey.cs, TaskbarPresence.cs, SelfTest.cs; **1 new tools script** | 130-230 product; 70-150 script depending lexical guard implementation |
| Required tiers 1+2 cumulative | **12 distinct product files + 1 tools script** | **280-530** total; no packages/project changes expected |
| Optional A44c-shell-folder | **3 product**: SettingsWindow.cs, DesktopPlatform.cs, SelfTest.cs | 25-50 additional; cumulative **13 distinct product + 1 script** |
| Deferred metrics move | Keys/DesktopPlatform/SelfTest already in cumulative set | 25-45 additional if selected; no extra product file |

Main estimate uncertainty is source guard tokenization and complementary IL checks, not 11 call renames. Existing scanner extension should be narrow (`SelfTest.cs:133-139,836-842,971-974`); no fresh architecture scanner is needed. No changed behavior or measurable startup/toggle-speed benefit is claimed.

Unverified: compilation/selftest on this branch, new guard failure proofs, actual HWND availability at hidden initialization, STA/runtime clipboard delivery/private-manager behavior, OCR integration, Explorer restart/taskbar activation, shutdown flush, exact timer/memory/performance behavior, and all window visuals. Prior STATUS/report gate claims were not rerun and are not evidence of a current live pass. Source inspection establishes current dependencies and ordering only.
