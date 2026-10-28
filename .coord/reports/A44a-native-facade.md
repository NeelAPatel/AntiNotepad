# A44a-native-facade

Implemented the behavior-neutral `DesktopPlatform` facade on `reorg/A44a-native-facade`. Codex made no commit. During verification, the shared branch advanced to `8d8fbc2` (A44a code commit `01c01c6`, followed by its status update).

## Product files changed

- `product/AntiNotepad/Platform/DesktopPlatform.cs` (new): nine synchronous one-line forwards, including unused `Flash`.
- `product/AntiNotepad/Platform/Native.cs`: `Native` is now a private nested class in the second `DesktopPlatform` partial. Native method bodies, P/Invokes, `Disposer`, `FlashInfo`, and `NativePoint` remain in place; only the nine forwarded methods changed from public to internal visibility. `Startup` remains at namespace scope.
- `product/AntiNotepad/UI/PadWindow.cs`
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/UI/PadMenu.cs`
- `product/AntiNotepad/UI/PadWindow.TopBlur.cs`
- `product/AntiNotepad/Editor/Elements.cs`
- `product/AntiNotepad/SelfTest.cs`

The 11 outside calls are name substitutions only. The four exact helper rules now target `DesktopPlatform`; the four whole-Platform rules remain. The capability check validates all nine static signatures and verifies that reflected nested `Native` exists and has `IsNestedPrivate`; it calls no native functions.

## Verification

- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A44a-check`: succeeded with 0 warnings and 0 errors; restore was already up to date and no package was added.
- `build/A44a-check/AntiNotepad.exe --selftest`: exit 0. The Windows executable did not expose a captured console stream in this PowerShell run. `dotnet build/A44a-check/AntiNotepad.dll --selftest` printed `SELFTEST PASS`, all 89 `LAYERS` lines, and `PLATFORM DesktopPlatform surface`.
- Final grep for `Native\.` outside `Platform/`: 0 matches. No temporary guard probe remains.
- `ConfigurePopupWindow`'s method body compares verbatim with the pre-change `dev` version. The namespace-level `Startup` section is unchanged.
- Cleanup: the requested recursive removal of `build/A44a-check` was rejected by the workspace tool policy; a final inspection found the directory absent.

## Guard proofs

1. Temporarily changed the UI layout call to `DesktopPlatform.Native.CursorPosition()`. Build failed with CS0122: `DesktopPlatform.Native` is inaccessible due to its protection level. Restored the file byte-for-byte.
2. Temporarily added an unused `MathNotes` method calling `DesktopPlatform.Chime()`. Build succeeded; selftest failed with `MathNotes->Platform 1 refs` and `MathNotes->DesktopPlatform 1 refs`, including the retargeted exact row. Restored the file byte-for-byte.

After both restorations, the final build and selftest passed again. `ConfigurePopupWindow` remains popup-only, its two callers retain their event/post/guard order, and no other platform bodies, styles, timers, hook order, clipboard order, or caller order moved.

The product implementation changes are limited to the ten files listed above. The branch already contains A44 plan/impact artifacts ahead of `dev`, and the coordination-required `.coord/STATUS.md` update is committed separately. `DesktopPlatform.cs` is in the A44a code commit; this report remains untracked and unstaged.
