# A44-platform-seam: plan (investigation only, nothing implemented)

Branch `reorg/A44-platform-seam`. Source: Codex director impact report `.coord/reports/A44-platform-seam-impact.md`
(spot-checked 2026-10-25 against dev `b54b476`: call sites, DllImport counts, Native visibility, SettingsWindow/PadWindow.Keys
shell and process calls all match). Assumes A39 to A43 land first, in the user's order; A44 is last.
Implementation is a separate flow; this session is an investigator only.

## r2 update (2026-10-27, on dev `1e89288`, after A39 to A43b landed): read this first
Source: `.coord/reports/A44-platform-seam-impact-r2.md` (spot-checked: 11 `Native.` calls in the same 7 files, 4 `typeof(Native)` rows
at `SelfTest.cs:555,570,586,598`, UI still holds `TaskbarProxy` at `PadWindow.cs:35`, no `Program.` use in `Platform/`). The sections
below keep the r1 text; where r2 differs, r2 wins:
- **Still needed:** tier 1 (11 `Native.` outside Platform, unchanged count), tier 2 for `TaskbarProxy` (UI still references it and it
  declares 4 imports), the interop guards. Optional tier 3 folder launch (`SettingsWindow.cs:799-808`).
- **Stale / no longer needed:** (1) "generalize A39's Editor-only scanner": the landed `LAYERS` scanner is a table-driven IL scan
  over every rule source (88 rows + 1 setter output = 89 lines); extend it, do not rewrite it. (2) `GlobalHotkey`/`Tray`/
  `TaskbarPresence` using `Program.*`: none now (`RuntimeLog`, `TaskbarTrace`, `AppLaunch`; a universal rule keeps `Program.` out).
  (3) `GlobalHotkey` extern nesting is no longer needed to clear a UI dependency: UI now goes through `HotkeyRegistrar` and its
  adapter (`Platform/HotkeyRegistrar.cs`); nest anyway only for consistency. (4) P/Invoke holders referenced by UI/Editor are now 2
  (`TaskbarProxy`, `Native`), was 3.
- **New:** private-nesting `Native` breaks four existing exact-target rows (`ExactTo: typeof(Native)` for MathNotes, ListEditing,
  NoteAnalysis, ClipboardCapturePolicy): retarget them to `DesktopPlatform` (they already also forbid the whole Platform namespace).
  Product files for tier 1 are 10, not 9 (SelfTest edit). The scanner does not inspect local-variable types, catch types,
  attributes or `InlineSig`, so the source guard must catch a local-only `IntPtr`. Source guard goes in a small repo script
  (`tools/check-platform-seam.ps1`, run at the manager gate), not in the installed selftest.
- **Split into sub-tasks:** see "Recommendation on handling" at the end.

## What
**Tier 1 (required): one thin `Platform` facade for the stateless Win32 helpers.**
- New `Platform/DesktopPlatform.cs`: `internal static partial class DesktopPlatform` with forwards, same signatures and bodies as
  today's `Native`: `CursorPosition`, `ListenClipboard`, `ClipboardIsPrivate`, `Chime`, `TrimWorkingSet`, `ConfigurePopupWindow`,
  `Flash` (unused today, keep, add no caller), `OpenUrl`, `StartScreenSnip`.
- `Native` (in `Platform/Native.cs`, currently `internal static class`, line 10) becomes `private static class Native` nested in
  `DesktopPlatform`. UI/Editor can then no longer name it (compile error). `Startup` stays at namespace scope in the same file.
- Replace the 11 outside call sites (at dev; A39/A41 move some, re-grep `Native\.` at start): `UI/PadWindow.cs` (ListenClipboard,
  CursorPosition, TrimWorkingSet), `UI/PadWindow.Keys.cs` (TrimWorkingSet), `UI/PadWindow.Layout.cs` (CursorPosition),
  `UI/PadWindow.Navigation.cs` (ClipboardIsPrivate, Chime, StartScreenSnip), `UI/PadMenu.cs` and `UI/PadWindow.TopBlur.cs`
  (ConfigurePopupWindow), `Editor/Elements.cs` (OpenUrl, written `Platform.Native.OpenUrl`).

**Tier 2 (required for the strong guard): move P/Invoke declarations out of the stateful services' public types.**
- `Platform/GlobalHotkey.cs:135-136` (RegisterHotKey, UnregisterHotKey) and `Platform/TaskbarPresence.cs:545-555` (ShowWindow,
  GetForegroundWindow, GetClassName, RegisterWindowMessage) go into a `private static class Win32` nested in the same type; call
  sites become `Win32.X`. Nothing else in those files changes (guards, hooks, `GetLastWin32Error` placement, `Marshal` use).
  Without this the rule "UI/Editor never reference a type that declares a P/Invoke" would reject the legitimate use of
  `GlobalHotkey`/`TaskbarProxy` from `PadWindow`.

**Tier 3 (optional, user decides): two managed OS bodies still in UI.**
- `UI/SettingsWindow.cs:758-765` `OpenFolder`: move only the `explorer.exe` launch to `DesktopPlatform.OpenFolder(string)`; the
  Directory.Exists check, try/catch and Toast stay in UI.
- `UI/PadWindow.Keys.cs:199,213` dev memory sampling (`Process.GetCurrentProcess`, PrivateMemory/WorkingSet/Threads) to
  `DesktopPlatform.ReadCurrentProcessMetrics()`. Dev diagnostics only; skipping it costs nothing.
- Recommendation: OpenFolder yes; skip the metrics move unless the guard should ban `Process` in UI outright.

**Kept as is (already good facades, no change):** `GlobalHotkey`, `TaskbarProxy` + `TaskbarPresence` policy/trace, `Tray`,
`Startup`, `Ocr`, `SingleInstanceService`, `SessionEnd`, `NotesLocation`, `DefaultDataFolderMigration`. Do not wrap them in
static forwards: they own handles and lifetimes.

## Why
UI code holds Win32 knowledge it should not, so an OS-level change rippling into UI is invisible until much later.
- `Native` is `internal` in a single assembly, so nothing stops any file calling it or adding a new `Native.X` from UI.
- Example: `UI/PadWindow.TopBlur.cs:300` and `UI/PadMenu.cs:45` both call `ConfigurePopupWindow`, which writes DWM corner/border
  attributes. Its popup-only contract (never on the pad or Settings; A32/A34 lost corners and shadow exactly by styling the
  pad) is recorded only in a comment. A caller one line away could break it, and the user finds out by eye days later.
- Example: `Editor/Elements.cs:143` calls `Platform.Native.OpenUrl` from the Editor, the layer A39 is making UI-free.
- There is no automated check that Win32 stays in `Platform`. After A44, a P/Invoke or `Native` ref from UI/Editor fails
  `--selftest` instead of surfacing later.

## How
1. Re-baseline: merge latest `dev` (after A39 to A43 are in), re-run `grep -rn "Native\." product/AntiNotepad --include=*.cs`,
   re-check A41's new owner classes (clipboard watcher, hotkey/taskbar glue) and update the call-site list; refresh this plan
   if the count or files moved.
2. Tier 1: add `Platform/DesktopPlatform.cs` (`partial`; forwards, one line each). Edit `Platform/Native.cs` only to nest `Native`
   inside `DesktopPlatform` (private) with unchanged bodies and `DllImport`s. Substitute call sites. Same call order, no new
   await/post/event/cache/allocation.
3. Tier 2: nest the externs in `GlobalHotkey` and `TaskbarProxy` (`private static class Win32`) and qualify the calls. Keep
   `Marshal.GetLastWin32Error` immediately after `RegisterHotKey`.
4. Tier 3 (if chosen): the two bodies above.
5. Selftest, extending A39's table-driven `LAYERS` (`SelfTest.CheckLayers`, `LayerRules`):
   - A39 only scans types whose namespace is `Editor` (`InNamespace(t.Namespace, "Editor")`), so its Model/Theme/Platform/Calc
     rules currently pass vacuously. Generalize the source selection to "every type whose namespace is any rule's From". Check
     that existing rules still report 0 refs; report any real violation found rather than refactoring for it.
   - Add a target kind "P/Invoke declaring type" (types with any method flagged `PinvokeImpl`, nested types included) with rules
     `UI -> PInvoke-holder` and `Editor -> PInvoke-holder`; output lines like `LAYERS UI->pinvoke-holder N refs`.
   - Add a whole-assembly check: every method with `PinvokeImpl` has a declaring type in `AntiNotepad.Platform`
     (`LAYERS pinvoke-outside-Platform 0`).
   - Add a source-level guard (selftest reads `product/AntiNotepad/**/*.cs`, or a tiny `tools` script): no `DllImport`,
     `LibraryImport`, `TryGetPlatformHandle`, `Win32Properties`, `DwmSetWindowAttribute` outside `Platform/`; strip comments and
     strings (SelfTest's embedded code strings contain `\nint`). Needed because reflection cannot see `TryGetPlatformHandle`
     use and may not see `LibraryImport` declarations.
   - Small capability check: `DesktopPlatform` exposes the expected members; `Native` is not accessible from outside.
   - Banning `Native` by name for UI/Editor is automatic (private nested), no rule needed.
6. Gate: build 0 warnings, `--selftest` PASS with the new lines, and prove each guard fails on a deliberate violation (temp
   `[DllImport]` in a Model type; temp `Marshal.SizeOf` or `IntPtr` field in UI; temp UI call into an exposed P/Invoke holder;
   temp restore of `explorer.exe` Process.Start in UI if tier 3), then revert.
7. Commits: code, then status, separately (project rule). No change to window frame styles, show/hide, hook order or timers.

**Unchanged (do not touch):** `ShowResident`/`HideResident` sequence (`UI/PadWindow.cs:123-155,211-222`), install/dispose order of
hotkeys, clipboard hook and taskbar proxy (`:105-108,277-282`), hotkey swap/rollback (`:450-465`), popup configure ordering
(menu: transparency, configure, Focus after `Opened`; float pager: `Loaded` post, then guards, then configure), trim schedules
(hidden 45 s timer, Settings 1.5 s timer, after GC), OCR/WinRT clipboard on the UI thread, the clipboard privacy check before the
text read, mutex disposal on its owner thread, `Program.cs` Avalonia backend setup.

## Success looks like
Measure on the branch before and after:

| Check | Before (dev b54b476) | After |
|---|---|---|
| `Native.` refs outside `Platform/` (grep `Native\.`, excluding `Platform/`) | 11 (PadWindow 3, Keys 1, Layout 1, Navigation 3, PadMenu 1, TopBlur 1, Elements 1, the last as `Platform.Native.OpenUrl`) | 0 |
| `DllImport`/`LibraryImport` outside `Platform/` | 0 | 0 |
| P/Invoke declaring types referenced by UI or Editor | `GlobalHotkey`, `TaskbarProxy`, `Native` | 0 |
| `Process.Start`/`GetCurrentProcess` outside Platform | 3 | 0 if tier 3 is taken |

- Selftest prints (names indicative): `LAYERS UI->pinvoke-holder 0 refs`, `LAYERS Editor->pinvoke-holder 0 refs`,
  `LAYERS pinvoke-outside-Platform 0`, the source guard line, plus all A39 `LAYERS` lines still 0 with the broadened scan.
- Each guard demonstrably fails on a deliberate violation, then passes after revert (recorded in the report).
- Build 0 warnings 0 errors; `--selftest` PASS; `git diff dev --stat` limited to `Platform/*`, the ~7 caller files, `SelfTest.cs`.
- Estimate: 9 files for tier 1, 12 with the tier 2 guard, 13 with tier 3; about 200-390 changed lines.
- Live checks by the user by hand in the dev copy (`tools/dev.ps1`), since source and selftest cannot prove these:
  1. Hotkey summon and hide, fast repeated toggles, new-note hotkey; open at mouse position on the right monitor.
  2. Copy something while the pad is hidden (clipboard capture works); copy from a password manager: nothing captured.
  3. Right-click menu: rounded panel, no system border/shadow; float-pager popup as before; overlay top-bar blur unchanged.
  4. Timer notification chime; Ctrl+click on a link opens the browser; screen-snip OCR command still launches.
  5. Taskbar modes Never / While visible / Always: button click hides/shows; Explorer restart (button returns).
  6. Settings: "Open folder" works and errors toast; closing Settings still trims memory as before.
  7. Window corners, shadow and bars unchanged in Docked/Overlay/Float.

## Impact radius
Direct: `Platform/Native.cs`, new `Platform/DesktopPlatform.cs`, `Platform/GlobalHotkey.cs`, `Platform/TaskbarPresence.cs` (extern
nesting only), the call-site files above, `SelfTest.cs`. Runtime: none expected (forwarders, same order). Model, Theme, Calc,
`Program.cs`, `Tray`, `Ocr`, `SingleInstance`, `SessionEnd`, `NotesLocation` untouched.

## Risks (ranked, from the impact report)
1. Critical: DWM/frame-style change leaking to the pad or Settings (A32/A34). Mitigation: `ConfigurePopupWindow` stays popup-only,
   body copied verbatim; diff review rejects any new window-style code.
2. High: extra dispatch or reordered work on the taskbar activation path (A22-A25) breaks foreground evidence. Mitigation: tier 2
   moves externs only, no `TaskbarProxy` logic.
3. High: hook install/dispose reordering loses clipboard/hotkey events; `ListenClipboard` zero-HWND no-op must stay.
4. High: `GetLastWin32Error` must follow `RegisterHotKey` immediately (tier 2 qualification must not insert calls between).
5. High: moving the privacy check after the clipboard text read, or WinRT/OCR onto a worker thread.
6. Medium: guards too strict or too lax. A blanket `UI -> Platform` ban would reject legitimate services; the source guard must
   ignore comments/strings; A39's scan covers Editor types only until generalized.
7. Medium: A41 moves the call sites; A44 after A41 applies the wrappers to the new owner classes (re-grep).
8. Medium: tier 3 behavior drift (URL launch throws, snip swallows, folder errors are toasted by UI): keep the catches in place.

## Depends on / conflicts with A39-A43
- **A39-motion-out-of-ui** (committed on its branch, `14b08f9`; lands first): provides `LayerRules` and the IL scanner A44 extends.
  File overlap: `SelfTest.cs`, `PadMenu.cs`, `PadWindow*.cs`, `Elements.cs` (imports). Hard dependency for the guards.
- **A40-settings-change-event**: may touch `PadWindow.Keys.cs` settings handling (taskbar/hotkey reaction) and the `SettingsWindow.cs`
  startup toggle; A44 only edits the trim call (`Keys.cs:196`) and OpenFolder. Small textual conflicts.
- **A41-padwindow-split**: highest overlap. It moves clipboard watching, hotkeys, taskbar glue and trim out of `PadWindow`
  (call sites at `PadWindow.cs:105-108,162,413`, `Navigation.cs:303,321,341`). In the agreed order A41 lands before A44, so A44
  substitutes in the new owners; this plan lists members, not files, for that reason. Do not start A44 before A41 is merged.
- **A42-padview-commands**: mostly independent (`Editor/Elements.cs:143` is not a text command); shared `SelfTest.cs` edits only.
- **A43-app-context**: `Program.cs`, and `Program.IsDev/Log` used inside `TaskbarPresence`/`GlobalHotkey`/`Tray`; A44 must not
  change those signatures. If A43 passes a context into Platform types, tier 2 nesting still applies unchanged.
- Serialize all `SelfTest.cs` edits; expect textual conflicts there and keep both sides.
- Order (user's): A39, A40, A41, A42, A43, A44. After A43, re-baseline per How step 1.

## Needs the user
- Include tier 3? Recommended: OpenFolder yes, dev metrics no.
- Facade name: `DesktopPlatform` (proposed) or another.

## Recommendation on handling (r2)
**Scope:** tiers 1 + 2 (facade for the stateless Win32 helpers, then the interop guards). Optional tier 3 limited to the Settings
folder launch. Skip the dev memory-metrics move. Do not add a desktop runtime, do not touch hotkey/taskbar/shutdown/notes logic
(A40 to A43b already own those seams).

**Split into 3 sub-tasks, in this order, one commit each, each gated and hand-checked before the next:**
| Task | Scope | Size |
|---|---|---|
| `A44a-native-facade` | `DesktopPlatform` with 9 forwards; `Native` private nested; 11 caller substitutions in 7 files; retarget the 4 `typeof(Native)` selftest rows; small capability check. No extern moves, no shell, no metrics. | 10 product files, ~80-150 lines |
| `A44b-interop-guards` | Nest externs in `GlobalHotkey` and `TaskbarProxy` (`private static class Win32`); add to `LAYERS`: P/Invoke-holder target for UI and Editor, whole-assembly import ownership (`pinvoke-outside-Platform`), raw-type rows (`Marshal`, `IntPtr`, `UIntPtr`, `Win32Properties`, `Windows.*`); add `tools/check-platform-seam.ps1` source guard. Prove each guard fails on a deliberate violation, then revert. | 3 product files + 1 script, ~130-230 + 70-150 lines |
| `A44c-shell-folder` (optional) | `DesktopPlatform.OpenFolder`; validation, catch and Toast stay in `SettingsWindow`; add a UI/Editor `Process.Start` member guard. | 3 files, ~25-50 lines |
A44a must land before A44b can clear today's UI references; A44b can first show the P/Invoke-holder rule failing against the
pre-extraction `TaskbarProxy`. If the user wants only two gates: fold the folder launch into A44a and keep its guard for A44b.
Cumulative: 12 product files + 1 script, about 280-530 lines. No packages.

**Riskiest parts (in order):**
1. Popup styling (`ConfigurePopupWindow`): body must be copied verbatim and stay popup-only; any pad/Settings use brings back the
   A32/A34 corner/shadow loss.
2. `TaskbarProxy` extern nesting (A44b): externs only; the TaskbarCreated initializer, native-visibility bracket, snapshot/generation
   ordering and Input/Background posts must not move.
3. Clipboard hook lifetime and privacy order (`ListenClipboard` zero-HWND no-op; `ClipboardIsPrivate` before the text read; own-copy
   marker before `SetTextAsync`), and OCR/snip target capture on the UI thread.
4. Guard quality: private nesting must not make the guards reject legitimate facades; the source guard must ignore comments and
   strings (SelfTest fixture strings contain `\nint`) but read interpolation code; the IL scanner misses local-only types.
5. `typeof(Native)` rows: a compile break if not retargeted in A44a.

**User's hand checks (dev copy via `tools/dev.ps1`, never the installed copy):**
- After `A44a-native-facade`: hidden start and clipboard capture; copy from a password manager is not captured; rapid hotkey
  toggles, new-note hotkey, opening at the mouse on the right monitor; timer chime; Ctrl+click on a link; screen snip and OCR with a
  pane switch and a deleted target; right-click menu and float pager popup look as before; corners, shadow and bars in Docked/
  Overlay/Float; hidden-trim and Settings-close trim still delayed.
- After `A44b-interop-guards`: hotkey change/conflict (rollback)/disable/re-enable, new-note hotkey; tray status; hidden startup;
  taskbar Never / While visible / Always, rapid taskbar clicks, click-away blur transfer, Explorer restart (button returns); quit,
  save and restart.
- After `A44c-shell-folder` (if chosen): Open notes folder and recovered folder, a path with spaces, the unavailable-folder toast,
  move location then Open uses the current folder.

**Decision for the user:** include `A44c-shell-folder` (recommended yes) and keep the dev metrics move out (recommended); confirm the
facade name `DesktopPlatform`; confirm the 3-way split versus 2 gates.
