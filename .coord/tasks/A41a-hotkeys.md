# A41a-hotkeys: move global-hotkey registration into one testable class

Role: worker (gpt-6-luna xhigh). **Implementation.** Product `product/AntiNotepad`. Read `AGENTS.md`, then the plan
`.coord/tasks/A41-padwindow-split-plan.md` (sections "What", "How", "Success", "Risks", `A41a-hotkeys` parts only) and
`.coord/reports/A41-padwindow-split-impact-r2.md`. Branch `reorg/A41a-hotkeys` (already checked out; do not switch branches,
do not commit: you cannot write to `.git`). Behavior-neutral refactor. A41b-clipboard-policy, the save scheduler and
navigation are OUT of scope.

## Files in scope
- New `Platform/HotkeyRegistrar.cs` (name may differ, keep it in `Platform/`): the class, the seam interface, the adapter.
- `UI/PadWindow.cs`: hotkey region (fields at ~29, status/event at ~35-36, `CompleteQuit` disposal ~281-282, region ~439-492,
  `ISettingsHost` forwards ~546-549), plus the constructor/`InstallHooks` wiring.
- `SelfTest.cs`: the `HOTKEYS swap` check and new `LAYERS` rows.
- Nothing else. `Program.cs`, `SettingsWindow`, `ISettingsHost`, `SearchOverlay`, `PadView`, `Store`, `SettingsService` unchanged.

## Goal
One class owns both hotkey slots (show/hide, id 0x4150 with the dev env override; new-note, id 0x4151), their status strings,
`HotkeyStatusChanged`, the swap/rollback logic and disposal, behind a small fakeable registration seam. The window keeps thin
forwards. No behavior change, no timing change.

## Work steps
1. Seam: an interface (e.g. `IHotkeyRegistration : IDisposable`) with `GestureLabel`, `IsRegistered`, `RegistrationErrorMessage`.
   An adapter wraps the sealed `GlobalHotkey` (registers against the Window in its constructor, so the adapter is built by a
   factory delegate supplied by `PadWindow`).
2. The class (constructor takes factory delegates and injected actions; no `Window`, `PadWindow`, `PadView`, `Program` types):
   - Owns slots `_hotkey` / `_newNoteHotkey`, `HotkeyStatus`, `NewNoteHotkeyStatus` ("off" when empty), `HotkeyStatusChanged`.
   - `RegisterHotkey(string?)` and `RegisterNewNoteHotkey(string?)` move verbatim, including: the no-gesture dev-override/default
     lookup, `GlobalHotkey.HotkeyGesture.Parse`, status text `"{label} unavailable: {error}"` when the slot ends empty, the
     `HotkeyStatusChanged` raise, blank new-note gesture = dispose + null + return true, `FormatException` -> injected
     `Action<string>` toast + return false.
   - `SwapHotkey` verbatim logic: same label while registered = no-op (true); release old FIRST; new registered -> keep;
     refused -> read error text, dispose new, re-create old by its previous label, keep it if registered else dispose; return false.
   - `Dispose()` disposes both slots (each once), used by `CompleteQuit`.
   - The toggle and new-note actions are injected as delegates built in `PadWindow` that already contain the existing
     `Dispatcher.UIThread.Post(...)` bodies: toggle posts `ToggleResident(pressed)` with the original timestamp; new-note posts
     `{ ShowResident(); NewNote(); }`. Move those two `Post` calls' text unchanged into the delegates in `PadWindow`; total
     `Dispatcher.UIThread.Post` count over PadWindow*.cs + the new file must equal today's (11 in PadWindow*.cs).
3. `PadWindow`: create the class in the constructor BEFORE `InstallHooks` (no lazy init). Keep `internal bool RegisterHotkey`,
   `RegisterNewNoteHotkey`, `NewNoteHotkeyStatus`, `HotkeyStatus`, `HotkeyStatusChanged` as thin forwards (they satisfy
   `ISettingsHost`; `Program.cs:~362` subscribes to `HotkeyStatusChanged`, so the forward must re-raise or expose the same event
   without breaking that subscription). `InstallHooks` order unchanged (hotkey, new-note hotkey, then clipboard hook).
   `CompleteQuit` disposes via the class at the same position.
4. Guards in `SelfTest.cs`:
   - `HOTKEYS swap` on a fake seam (no window, no Win32). Cases: same label while registered = no-op, nothing created or
     disposed; swap releases old BEFORE creating new (record call order); refused new restores old (old label registered
     again, returns false, error text captured into status); refused new AND failed restore leaves slot empty; blank new-note
     gesture disables and disposes; each slot disposed exactly once on `Dispose()` (and double Dispose does not double dispose);
     unparsable gesture calls the toast and returns false without touching the slot; `HotkeyStatusChanged` raised on each
     show/hide registration. Print one line `HOTKEYS swap ...` in the same style as the `LAYERS`/`SETTINGS` lines; failures go
     to the same failure list so `SELFTEST PASS` depends on it.
   - New `LAYERS` rows in the existing table: the new class must not reference `PadWindow`, `PadView`, `Program`
     (use `ExactFrom` of the new type, `ExactTo` of those types; follow the existing row style). Existing rows unchanged.
5. Build and check as below.

## Rules
- Do not touch: `ShowResident`, `HideResident`, `Prewarm`, `FocusActive`, `QueueHideOnBlur`, `PrepareForSessionEnd`, the
  `Deactivated` handler, frame styles (`ShowInTaskbar|WindowDecorations|ExtendClientArea|SetWindowLong|DwmSetWindowAttribute`),
  `HandleSettingsChanged`, constructor order of anything else.
- No subscription to `SettingsService.Changed`. No new dispatcher hop, timer, task or lock. No lazy init. Never cache the
  `Settings` object; read via `Settings`/`SettingsService.Current` at call time only if needed.
- `ResetSettings` keeps ignoring registration return values; do not "fix" it.
- The new class takes values/delegates, never `Program.*`. US spelling in UI text; match surrounding comment density.
- No new library. Do not launch or kill AntiNotepad. Do not commit.
- Build: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A41a-codex-check` (0 warnings), then
  `build/A41a-codex-check/AntiNotepad.exe --selftest` must print `SELFTEST PASS` with all existing `LAYERS`/`SETTINGS` lines plus
  the new ones. Delete `build/A41a-codex-check` afterwards.

## Done means
- Build 0 warnings 0 errors; `SELFTEST PASS` including `HOTKEYS swap` and the new `LAYERS` rows.
- `git diff` of the untouchable members above is empty; frame-style grep unchanged; Post count unchanged.
- Short report at `.coord/reports/A41a-hotkeys.md`: files changed, new type names, the Post count before/after, the selftest
  output lines, any deviation from this brief and why. Do not prove the guards fail yourself; the manager does that.
