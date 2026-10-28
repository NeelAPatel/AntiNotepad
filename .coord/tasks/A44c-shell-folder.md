# A44c-shell-folder: move the Explorer launch behind DesktopPlatform, guard UI/Editor against Process.Start (tier 3 of A44-platform-seam)

Role: worker (gpt-6-luna xhigh). **Implementation, behavior-neutral.** Product `product/AntiNotepad`. Branch
`reorg/A44c-shell-folder` (already checked out; do not switch branches, do not commit: you cannot write to `.git`).
Read first: `AGENTS.md`, `.coord/tasks/A44-platform-seam-plan.md` (its "r2 update" and "Recommendation on handling" win over r1),
`.coord/reports/A44-platform-seam-impact-r2.md` (A44c rows), `.coord/reports/A44b-interop-guards.md`.
A44a and A44b are merged: `Platform/DesktopPlatform.cs` exists (facade over private nested `Native`), `LAYERS` rows and
`tools/check-platform-seam.ps1` exist. Line numbers are from dev `e9ff65b`; re-grep before editing.

## Scope (A44c only)
1. **`Platform/DesktopPlatform.cs` / `Platform/Native.cs`**: add `internal static void OpenFolder(string path)` on `DesktopPlatform`
   (facade, same style as `OpenUrl`), implemented in `Native` (or in `DesktopPlatform` itself if that fits the file better).
   It does exactly what `SettingsWindow.OpenFolder` does today after validation: `ProcessStartInfo("explorer.exe") { UseShellExecute = true }`,
   `ArgumentList.Add(path)`, `Process.Start`. No catch, no `Directory.Exists`, no toast inside it; exceptions propagate.
2. **`UI/SettingsWindow.cs`** (`OpenFolder`, ~:799-809): keep the `Directory.Exists` check, the try/catch and both toasts exactly as they are;
   replace only the three launch lines with `DesktopPlatform.OpenFolder(path)`. Drop `using System.Diagnostics;` only if nothing else
   in the file needs it. Re-grep all of `UI/` and `Editor/` for `Process.Start`, `ProcessStartInfo` and `explorer`; today
   `SettingsWindow` is the only hit outside `Platform/` (`Native.cs` OpenUrl/StartScreenSnip are already in Platform).
3. **Leave alone**: dev memory metrics (`Process.GetCurrentProcess` in `UI/PadWindow.Keys.cs`). The user chose to skip them; the new
   guard must not flag them (target `Process.Start` members only, not the `Process` or `ProcessStartInfo` types).
4. **Guard** (extend A44b's work, do not rewrite it):
   - `SelfTest.cs`: a new LAYERS-style row per layer (`UI` and `Editor`) that fails on any call/callvirt/newobj-free IL reference to a
     method named `Start` declared on `System.Diagnostics.Process` (use the existing `ScanMethod` `inspectCall` hook like
     `CheckSettingsSetters`, or the closest fit to the existing row structure). Output lines like `LAYERS UI->Process.Start 0` and
     `LAYERS Editor->Process.Start 0`. Existing output lines must remain unchanged. Add `OpenFolder` (`void`, `string`) to the
     `DesktopPlatformSurfaceChecks` expected list.
   - `tools/check-platform-seam.ps1`: add `Process.Start` (and `ProcessStartInfo`) to the masked-source token scan outside `Platform/`
     only if it does not trip on `Process.GetCurrentProcess` or the SelfTest strings; otherwise leave the script alone and say why.
     Keep its pass line format.
5. **Prove the guard**: temporarily add a `Process.Start(...)` call in a UI file, build, confirm the selftest `LAYERS` row (and the
   script, if extended) fails, then revert it completely. Say in the report what you saw. Final tree must contain no probe.

## Must not change
No behavior change: same process, arguments, toasts, validation order. Settings, Store, notes-location code, `PadWindow.Keys.cs` metrics.
Hotkey and taskbar code. No new packages. US spelling.

## Rules
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A44c-check`: 0 errors, 0 warnings. Run
  `build/A44c-check/AntiNotepad.exe --selftest`: `SELFTEST PASS`. Run `pwsh -NoProfile -File tools/check-platform-seam.ps1`: passes.
  Then delete `build/A44c-check`.
- Never launch, stop or kill AntiNotepad; never run `tools/dev.ps1`, `tools/install.ps1` or `--quit`.
- Do not commit. Do not touch `.coord/STATUS.md`.

## Done means
Build clean, selftest PASS, seam script passes, guard proven then reverted, `build/A44c-check` removed.
Report at `.coord/reports/A44c-shell-folder.md`: files changed, the new LAYERS lines, the guard proof, and anything left out.
