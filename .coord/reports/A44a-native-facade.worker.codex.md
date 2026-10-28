Implemented **A44a-native-facade**. `DesktopPlatform` now provides nine synchronous forwards, `Native` is private and nested, the 11 callers were retargeted, and the four exact `LAYERS` rules now target `DesktopPlatform`. The capability check verifies the signatures and private nesting without calling native code.

The build passed with **0 warnings and 0 errors**. The Windows executable’s selftest exited 0 but exposed no captured console output; running the same build’s DLL through `dotnet` printed **`SELFTEST PASS`**, all **89 `LAYERS` lines**, and **`PLATFORM DesktopPlatform surface`**.

Both guard proofs passed: private `Native` access failed compilation with CS0122, and a temporary `MathNotes` call failed both the `MathNotes->Platform` and `MathNotes->DesktopPlatform` rows. Both edits were restored. The final grep found **0 `Native.` references outside `Platform/`**. `ConfigurePopupWindow`’s body matches the pre-change version verbatim, and `Startup` is unchanged.

The product-scoped `git diff dev --stat -- product/AntiNotepad` contains exactly the ten requested product files. The unscoped diff also includes A44 plan and impact documents already on the branch. The cleanup command was blocked by workspace policy, but a final check found `build/A44a-check` absent. The full report is [A44a-native-facade.md](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.coord/reports/A44a-native-facade.md>).

I did not commit. At final inspection, the shared branch was at `8d8fbc2`, with the A44a code and status commits already present; the report remains untracked.

