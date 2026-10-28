# A44a-native-facade: DesktopPlatform facade, Native becomes private (tier 1 of A44-platform-seam)

Role: worker (gpt-6-luna xhigh). **Implementation, behavior-neutral refactor.** Product `product/AntiNotepad`. Branch
`reorg/A44a-native-facade` (already checked out; do not switch branches, do not commit: you cannot write to `.git`).
Read first: `AGENTS.md`, `.coord/tasks/A44-platform-seam-plan.md` (its "r2 update" wins over the r1 text) and
`.coord/reports/A44-platform-seam-impact-r2.md` (tables "Every outside Native call" and "Exact callable Platform type surface").
Line numbers are from the plan; re-read and re-grep before editing.

## Scope (A44a only)
1. **Add `Platform/DesktopPlatform.cs`**: `internal static partial class DesktopPlatform` with nine one-line forwards, same
   signatures as today's `Native`: `CursorPosition`, `ListenClipboard`, `ClipboardIsPrivate`, `Chime`, `TrimWorkingSet`,
   `ConfigurePopupWindow`, `Flash` (unused today; keep, add no caller), `OpenUrl`, `StartScreenSnip`. Synchronous, no new
   await/post/event/cache/allocation.
2. **`Platform/Native.cs`**: nest `Native` as a `private static class Native` inside a second `DesktopPlatform` partial. Bodies,
   DllImports, `Disposer`/`FlashInfo`/`NativePoint` unchanged (inner members need `internal` visibility so the outer partial can
   call them; do not make `Native` itself assembly-visible). `Startup` stays at namespace scope in the same file, untouched.
3. **Substitute the 11 `Native.` call sites in 7 files** (`UI/PadWindow.cs` x3, `UI/PadWindow.Keys.cs`, `UI/PadWindow.Layout.cs`,
   `UI/PadWindow.Navigation.cs` x3, `UI/PadMenu.cs`, `UI/PadWindow.TopBlur.cs`, `Editor/Elements.cs` as `Platform.Native.OpenUrl`
   -> `Platform.DesktopPlatform.OpenUrl`). Name substitution only: same position, same call order.
4. **`SelfTest.cs`**: retarget the four `ExactTo: typeof(Native)` rows (MathNotes, ListEditing, NoteAnalysis, ClipboardCapturePolicy,
   around lines 555, 570, 586, 598) to `typeof(DesktopPlatform)`. Keep their separate whole-Platform rules. Update the printed
   label text only if it names Native. All 89 LAYERS outputs must remain.
5. **Small capability check** in `SelfTest.cs` (one new line, e.g. `PLATFORM DesktopPlatform surface`): `DesktopPlatform` exposes the
   nine expected static members with the expected signatures; the nested `Native` exists (found via reflection with
   `BindingFlags.NonPublic`) and is not accessible from outside (`IsNestedPrivate`). Missing type fails. Do not expose `Native` and
   do not call any native function from the selftest.

## Out of scope (A44b / A44c)
No extern moves in `GlobalHotkey` or `TaskbarProxy`, no P/Invoke-holder or raw-type LAYERS rules, no source-guard script, no
`OpenFolder`, no shell or Process changes, no metrics changes, no `Startup`/`Tray`/`Ocr`/`SingleInstance` changes.

## Must not change
- `ConfigurePopupWindow`: copy the body verbatim; it stays popup-only. The two callers (`PadMenu.cs`, `PadWindow.TopBlur.cs`) keep
  their event/post/guard order exactly; add no pad or Settings use. No new window-style code anywhere.
- Call order, the clipboard privacy order (`ClipboardIsPrivate` before the text read), `ListenClipboard` zero-HWND no-op and
  hook add/dispose order, the hotkey/clipboard install order in `PadWindow`, all timers (hidden trim 45 s, Settings 1.5 s, after GC).
- `Marshal.GetLastWin32Error` placement and every other Platform file not listed above.

## Rules
- Build: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A44a-check`: 0 errors, 0 warnings, no packages.
  Then `build/A44a-check/AntiNotepad.exe --selftest` must print `SELFTEST PASS` with all existing lines plus the new one. Delete
  `build/A44a-check` afterwards (say so in the report if removal is refused).
- Never launch or kill AntiNotepad (never `--quit`). Do not commit. US spelling in UI text (none expected).
- `grep -rn "Native\." product/AntiNotepad --include=*.cs` outside `Platform/` must find nothing (SelfTest string/label hits count; remove them).

## Prove the guards (then revert both, confirm the tree is clean of them)
1. Temporarily add a `Native.CursorPosition()` (or `Platform.Native...`) reference in a UI file: the build must fail to compile.
2. Temporarily make one retargeted pure helper (e.g. `MathNotes`) reference `DesktopPlatform` (a harmless call to
   `DesktopPlatform.Chime` is not to be executed; an unused method body suffices): `--selftest` must fail on that LAYERS row.
Record both outcomes in the report, then revert both and rebuild once more to confirm 0 warnings and `SELFTEST PASS`.

## Done means
- Build 0 warnings 0 errors; `SELFTEST PASS`.
- `git diff dev --stat` limited to `Platform/DesktopPlatform.cs` (new), `Platform/Native.cs`, the 7 caller files, `SelfTest.cs`.
- Report `.coord/reports/A44a-native-facade.md`: files changed, final grep result, the 89 LAYERS lines count, the new capability line,
  both guard proofs, confirmation that `ConfigurePopupWindow` is verbatim and nothing else moved.
