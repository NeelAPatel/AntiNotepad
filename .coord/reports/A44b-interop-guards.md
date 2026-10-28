# A44b-interop-guards

Implemented on `reorg/A44b-interop-guards`. I did not make a commit. During final inspection, shared `HEAD` was `98d91f8` (`A44b-interop-guards: nest hotkey/taskbar externs in private Win32; P/Invoke and raw-type LAYERS rows; source guard script`), authored by Neel and co-authored by Claude. It contains the three product edits and new script; I left it untouched. This report remains untracked.

## Files and line counts

| File | Final lines | Diff vs `dev` |
|---|---:|---:|
| `product/AntiNotepad/Platform/GlobalHotkey.cs` | 140 | +7 / -4 |
| `product/AntiNotepad/Platform/TaskbarPresence.cs` | 577 | +15 / -14 |
| `product/AntiNotepad/SelfTest.cs` | 2,602 | +170 / -28 |
| `tools/check-platform-seam.ps1` | 242 | new |

The branch already contains `.coord/tasks/A44b-interop-guards.md` relative to `dev`; it was not changed. The implementation changes are limited to the three product files above and the new source guard. This report and the coordination status note are documentation.

## Changes

- `GlobalHotkey.RegisterHotKey` and `UnregisterHotKey` now live in private nested `GlobalHotkey.Win32`; the two call sites are qualified. `Marshal.GetLastWin32Error()` remains the immediately following statement after registration.
- `TaskbarProxy`'s four extern declarations now live in private nested `TaskbarProxy.Win32`; the six call sites, including the `_taskbarCreated` field initializer, are qualified. The initializer remains in its original field position. The native visibility bracket, click snapshot/generation order, and Input/Background posts are unchanged. No native method body or policy logic moved.
- The table-driven `LAYERS` scanner gained app-assembly P/Invoke-holder detection, direct raw-type rows, and the assembly P/Invoke ownership count. Its prior output lines and order are unchanged. `PLATFORM P/Invoke holders` checks by reflection that `GlobalHotkey.Win32`, `TaskbarProxy.Win32`, and `DesktopPlatform.Native` exist and are nested private; the two Win32 holders must also contain methods with the actual `MethodAttributes.PinvokeImpl` flag. It calls no native methods.
- `tools/check-platform-seam.ps1` scans every C# file under `product/AntiNotepad` outside `Platform/`, including `Program.cs`, `SelfTest.cs`, and generated `.cs` files. It ignores comments and literal text while scanning interpolation holes. It rejects `DllImport`, `LibraryImport`, `TryGetPlatformHandle`, `Win32Properties`, `DwmSetWindowAttribute`, `IntPtr`, `UIntPtr`, `nint`, and `nuint`.
- No package or project-file changes were made.

## New selftest output

All 89 prior `LAYERS` lines remain unchanged. The 13 added lines on the clean tree are:

```text
LAYERS UI->pinvoke-holder 0 refs
LAYERS Editor->pinvoke-holder 0 refs
LAYERS UI->Marshal 0 refs
LAYERS Editor->Marshal 0 refs
LAYERS UI->IntPtr 0 refs
LAYERS Editor->IntPtr 0 refs
LAYERS UI->UIntPtr 0 refs
LAYERS Editor->UIntPtr 0 refs
LAYERS UI->Win32Properties 0 refs
LAYERS Editor->Win32Properties 0 refs
LAYERS UI->Windows.* 0 refs
LAYERS Editor->Windows.* 0 refs
LAYERS pinvoke-outside-Platform 0
```

The capability line is:

```text
PLATFORM P/Invoke holders
```

Raw-type inspection found no direct UI/Editor declarations or IL type/member operands for `Marshal`, `IntPtr`, `UIntPtr`, `Win32Properties`, or a `Windows.*` type. A source search's only `Windows.` hit was prose in `UI/PadWindow.cs:432`, not a namespace reference. The raw rows use declared signatures (including base/interface types) and direct IL type/field/member declaring types; they do not attribute a framework method's external parameter signature to the UI/editor caller. The source guard separately catches lexical local-only pointer uses that reflection cannot see.

## Guard proofs

1. Added a temporary `DllImport` extern to `Model.A44bPInvokeProbe`. The selftest printed `LAYERS pinvoke-outside-Platform 1` and identified `AntiNotepad.Model.A44bPInvokeProbe.Probe`; the source guard failed at `product/AntiNotepad/Model/A44bPInvokeProbe.cs:5` on `DllImport`. Removed the probe.
2. Added a temporary UI method returning `System.IntPtr`. The selftest printed `LAYERS UI->IntPtr 1 refs` and failed with `AntiNotepad.UI.A44bRawProbe -> System.IntPtr`. Removed the probe.
3. Added a temporary UI P/Invoke holder and a UI caller. The selftest printed `LAYERS UI->pinvoke-holder 2 refs` and failed; the whole-assembly check also printed `LAYERS pinvoke-outside-Platform 1`. Removed the holder and caller.
4. A temporary UI `[DllImport]` failed the source guard at line 5. A separate local-only `System.IntPtr x = default` failed it at line 7. Each probe was removed before continuing.
5. A temporary fixture containing `IntPtr` in line/block/XML comments, normal/verbatim/raw strings, interpolated text, and the escaped `"\nint nint"` fixture passed with 0 violations. Adding `System.IntPtr` inside an interpolation hole failed at line 12. Removed the fixture.

No temporary probe remains. After the last probe, the clean tree was rebuilt and rechecked:

- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A44b-check`: 0 warnings, 0 errors.
- `build/A44b-check/AntiNotepad.exe --selftest`: `SELFTEST PASS`, all 102 `LAYERS` lines and both `PLATFORM` capability lines.
- `pwsh -NoProfile -File tools/check-platform-seam.ps1`: `PLATFORM-SEAM source guard: 0 violations in 54 files`.
- `git diff --check`: passed.

No application window was launched or stopped. The workspace tool policy rejected the recursive removal command for `build/A44b-check`; a later `Test-Path` check found that directory absent.
