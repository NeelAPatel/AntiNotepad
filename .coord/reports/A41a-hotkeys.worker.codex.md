Implemented `A41a-hotkeys` on `reorg/A41a-hotkeys`; no commit was made.

- Added `HotkeyRegistrar`, its fakeable registration seam, and the `GlobalHotkey` adapter in [HotkeyRegistrar.cs](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/product/AntiNotepad/Platform/HotkeyRegistrar.cs>). Updated [PadWindow.cs](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/product/AntiNotepad/UI/PadWindow.cs>) with thin forwards and injected actions, and added fake-seam checks plus three `LAYERS` rows in [SelfTest.cs](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/product/AntiNotepad/SelfTest.cs>).
- Build passed with **0 warnings and 0 errors**. Selftest passed with `HOTKEYS swap`, all three new `LAYERS` rows at 0 references, and `SELFTEST PASS`.
- Dispatcher Post count stayed **11**; frame-style grep was unchanged. The temporary build output was removed. The required report is [A41a-hotkeys.md](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.coord/reports/A41a-hotkeys.md>).

One brief detail conflicted with preserving the existing status logic: a successful rollback reports the restored gesture; the unavailable error appears when the slot ends empty. I preserved and tested that behavior. Real Windows registration and focus behavior remains for the dev-copy hand check.

