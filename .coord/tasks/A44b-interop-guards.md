# A44b-interop-guards: nest remaining externs, extend LAYERS, add source guard (tier 2 of A44-platform-seam)

Role: worker (gpt-6-luna xhigh). **Implementation, behavior-neutral.** Product `product/AntiNotepad`. Branch
`reorg/A44b-interop-guards` (already checked out; do not switch branches, do not commit: you cannot write to `.git`).
Read first: `AGENTS.md`, `.coord/tasks/A44-platform-seam-plan.md` (its "r2 update" wins over r1), `.coord/reports/A44-platform-seam-impact-r2.md`,
`.coord/reports/A44a-native-facade.md`. A44a is merged: `Platform/DesktopPlatform.cs` exists, `Native` is private and nested in it.
Line numbers below are from dev `e0ea687`; re-grep before editing.

## Scope (A44b only)
1. **`Platform/GlobalHotkey.cs`** (externs at :135-136): move `RegisterHotKey` and `UnregisterHotKey` into a
   `private static class Win32` nested in `GlobalHotkey`; call sites (:60, :89) become `Win32.RegisterHotKey` / `Win32.UnregisterHotKey`.
   `Marshal.GetLastWin32Error()` stays on the very next statement after `RegisterHotKey` (:61). Nothing else changes.
2. **`Platform/TaskbarPresence.cs`**, type `TaskbarProxy` (externs at :565-575): move `ShowWindow`, `GetForegroundWindow`, `GetClassName`,
   `RegisterWindowMessage` into a `private static class Win32` nested in `TaskbarProxy`; call sites (:114, :212, :239, :397, :433, :494) become
   `Win32.X`. Move only the extern declarations. Leave exactly where they are: the `_taskbarCreated` initializer (its position among field
   initializers), the native-visibility bracket in `NativeShowWindow`, snapshot/generation ordering, the Input/Background posts. No new
   dispatch, no reordering.
3. **`SelfTest.cs`: extend the existing table-driven `LAYERS` scanner (`LayerRules`, `CreateLayerRules`, `CheckLayers`, `LayerRule`).
   Extend it; do not rewrite it.** Existing output lines must all remain unchanged. Add:
   - a target kind "P/Invoke holder" (any type, nested types included, that declares a method with `PinvokeImpl`; use the real attribute
     flag via reflection, not names) with rules `UI -> holder` and `Editor -> holder`. Output like `LAYERS UI->pinvoke-holder N refs`.
   - a whole-assembly check `LAYERS pinvoke-outside-Platform 0`: every `PinvokeImpl` method's declaring type must be in `AntiNotepad.Platform`
     (nested types too: use the top-level declaring type's namespace).
   - raw-type rows for UI and Editor: `Marshal`, `IntPtr`, `UIntPtr`, `Win32Properties` and the `Windows.*` namespaces (WinRT). **First check
     what UI/Editor legitimately use today** (the scanner sees signatures and IL operands; local-only types are not seen). If any real use
     exists, do not refactor: keep the row out or exempt it explicitly, and report each real hit (file, line, why) in the report.
   - a small capability line (e.g. `PLATFORM P/Invoke holders`): `GlobalHotkey.Win32` and `TaskbarProxy.Win32` are nested private,
     `DesktopPlatform.Native` stays nested private; reflection lookup failure fails. Do not call native functions.
4. **New `tools/check-platform-seam.ps1`** (PowerShell 7, no packages), run by the manager at the gate, not by the selftest. Scans
   `product/AntiNotepad/**/*.cs` **outside `Platform/`** (Program.cs and SelfTest.cs included) and fails (exit 1, prints file:line and the token)
   on any of: `DllImport`, `LibraryImport`, `TryGetPlatformHandle`, `Win32Properties`, `DwmSetWindowAttribute`, and `IntPtr` (the local-only
   case the IL scanner cannot see; also reject `UIntPtr`, `nint`, `nuint` if none exist today, else report). It must:
   - ignore comments (`//`, `/* */`, `///`) and string/char literals (regular, verbatim `@""`, raw `"""`, interpolated `$""`, `$@""`);
   - **but read code inside interpolation holes** (`$"{ ... IntPtr ... }"` is code);
   - not trip on `SelfTest.cs` fixture strings containing `\nint` (and `nint` inside strings in general);
   - print one summary line on pass (e.g. `PLATFORM-SEAM source guard: 0 violations in N files`). Pass on the current tree.
   Prefer a small hand-written lexer/state machine over regex-only stripping. Keep it readable; add a short header comment of what it checks.

## Must not change
No behavior change anywhere. No edits to hook install/dispose order, hotkey swap/rollback, taskbar logic, `DesktopPlatform`/`Native`, UI files,
`Program.cs`, `Tray`, `Ocr`. No packages.

## Rules
- Build: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A44b-check`: 0 errors, 0 warnings.
  Run `build/A44b-check/AntiNotepad.exe --selftest` there: `SELFTEST PASS` with all prior LAYERS lines plus the new ones. Delete
  `build/A44b-check` at the end (say so if refused).
- Never launch or kill AntiNotepad (never `--quit`, never `tools/dev.ps1`). Do not commit. US spelling.

## Prove each guard fails, then revert (rebuild to a clean state after the last one)
1. Temporary `[DllImport]` extern in a Model type: selftest `pinvoke-outside-Platform` fails; the script also fails.
2. Temporary `Marshal`/`IntPtr` use in a UI file that the IL scanner sees (e.g. a field or method signature): the raw-type row fails.
3. Temporary UI reference to a P/Invoke holder (a P/Invoke holder reachable from UI only for the proof, e.g. a temporary non-private holder):
   the `UI->pinvoke-holder` row fails.
4. Temporary `[DllImport]` (and a separate local-only `IntPtr x = default;`) in a UI file: `tools/check-platform-seam.ps1` fails on each.
5. Script false-positive checks: `IntPtr` in a comment, in a normal string, in a raw string and in an interpolated string's text part must
   NOT fail; `IntPtr` inside an interpolation hole MUST fail.
Record every outcome in the report; confirm the tree is clean of the temporary violations.

## Done means
- Build 0 warnings 0 errors; `SELFTEST PASS`; `pwsh -NoProfile -File tools/check-platform-seam.ps1` exits 0.
- `git diff dev --stat` limited to `Platform/GlobalHotkey.cs`, `Platform/TaskbarPresence.cs`, `SelfTest.cs`, new `tools/check-platform-seam.ps1`.
- Report `.coord/reports/A44b-interop-guards.md`: files and line counts, new LAYERS/PLATFORM lines (full text), what UI/Editor
  legitimately use of the raw types, every proof outcome, confirmation that only externs moved and `GetLastWin32Error` placement is intact.
