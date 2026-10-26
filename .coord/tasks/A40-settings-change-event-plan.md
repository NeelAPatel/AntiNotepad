# A40-settings-change-event-plan

Status: PLAN ONLY. This session is a read-only investigator; implementation starts later from a separate flow, after
A39-motion-out-of-ui is merged. Evidence: `.coord/reports/A40-settings-change-event-impact.md` (Codex director, spot-checked
against code by the manager: call sites, ResetSettings, Set/Store helper, SearchGrid, Folding-on-Bind claims all confirmed).
Line numbers are from dev b54b476 and will move after A39 (re-verify at start).

## What
`Settings` (`Model/Store.cs:269-325`, **56** serialized mutable properties, held as `StoreData.Settings`) has no change
notification. Today a change is: mutate, then hand-call `PadWindow.SettingsChanged()` (`UI/PadWindow.Keys.cs:264`; 3 call sites:
`UI/SettingsWindow.cs:128` in `Set(...)`, `:514` in `Store(...)`, `UI/PadWindow.Keys.cs:258` in reset/undo). That chain does
Topmost -> taskbar proxy -> `ApplyTaskbarSetting` -> `ApplyAppearance` (palette, `Anim.Reduced`, theme/material/tint/scale,
`PadView.ApplyLook` on both panes, chrome, bars, blur, overlay reopen, settings `SyncLook`) -> `Edited()` (save timer).
`SettingsWindow` + `SettingsParts` write 51 properties directly (about 74 reads, 50 writes) and use 14 distinct `PadWindow`
members (23 uses): `Settings`, `CurrentPalette` (6), `Icon`, `SettingsLostFocus` (2), `SettingsChanged` (2), `HotkeyStatus` /
`RegisterHotkey`, `NewNoteHotkeyStatus` / `RegisterNewNoteHotkey`, `Toast` (3), `ShowResident`, `OpenHelp`, `ResetSettings`, `Save`.
Settings are read in 20 files (239 explicit property reads, 67 writes outside the schema defaults).

## Why
Removes the "mutated a setting, forgot the call, UI stale until later" class of bug, and makes `SettingsWindow` independent of
`PadWindow`. Concrete examples in today's code:
- `UI/SearchOverlay.cs:391` flips `SearchGrid` and only reopens search: no `Edited()`, so the change persists only if some other
  save happens to run.
- `UI/PadWindow.Navigation.cs:155,173` writes `SplitSlot` and `:149` `Split`; `CycleSplitSlot` (`:169-175`) has no `Edited()`.
- `UI/PadWindow.Keys.cs:104` (zoom) writes `FontSize` and runs its own shorter path instead of the shared one.
- Even "full" propagation is partial: `Folding` is applied only on note Bind (`Editor/PadView.cs:294`) and `InlineMath` only on
  Analyse, so the manual call does not make them take effect immediately.
- `Set`/`Store` always propagate even for same-value assignments; OS color change, show/hide and construction call the same
  chain with no setting change at all.

## How
**Design (recommended option A of the report): a `SettingsService`** in `Model/` (no UI/Editor references) that owns the current
plain `Settings` instance. `Settings` stays a JSON POCO, unchanged shape.
1. API: `Current` (the live POCO; identity changes only on `Replace`), `Update(Action<Settings>, ...)` which runs the mutation,
   diffs before/after (scalar compare plus a copied `DisabledLanguages` list; no JSON serialization per change), and raises
   **one** `Changed(SettingsChange)` after the outermost batch. `Replace(Settings)` for reset/undo/load raises once with
   all-flags. Nested `Update` from a handler is queued and emitted as one follow-up batch, never recursively. No-op batch
   emits nothing. UI-thread only (assert/documented); the A30 background writer keeps serializing a detached clone.
2. `SettingsChange` carries a flags enum (Appearance, Palette, EditorLook, Analysis, Motion, Caret, Chrome, Hotkeys, WindowPolicy,
   Taskbar, Geometry, Split, Search, Persistence) plus the changed property names for diagnostics. Geometry-only changes
   (X/Y/Width/Height, written on every move/resize in `UI/PadWindow.cs:417-422`) and Split/Search changes must **not** trigger the
   appearance chain (show/hide speed is the priority).
3. Subscribers react on their own, in an explicit order that the report found matters: `PadWindow` (one handler that keeps the
   existing order: Topmost, taskbar, appearance incl. `Anim.Reduced` and disabled-language hash before pane look, then `Edited`),
   `PadView`/`SmoothCaret` via the pad handler (panes keep `ApplyLook`), `SettingsWindow` (own look/preview refresh),
   `MotionPreview`. Order is kept by one ordered handler in `PadWindow`, not by arbitrary subscription order.
4. `Store` stays the persistence owner; `Data.Settings` becomes the service's `Current` (reset/undo go through `Replace`, which
   also updates retained references in `PadView`, `MotionPreview` and re-registers hotkeys as `ResetSettings` does today).
   Save scheduling for a settings change is attached once at the composition root (listens to `Changed` with Persistence), so
   `SearchGrid`/`SplitSlot` get saved too (this is a deliberate, small behavior fix; call it out in the commit).
5. `SettingsWindow` takes the service plus a small host interface (palette source, `Toast`, hotkey register/status with its
   validate-before-commit transaction, `ShowResident`/`OpenHelp`, `Save`, `ResetSettings`, `SettingsLostFocus`, icon) instead of
   `PadWindow`. `Set`/`Store` helpers call `service.Update`; the hand-called `SettingsChanged()` and its 3 sites go away.
   Hotkey registration stays "register first, commit only on success".
6. Migrate callers: the ~67 direct writes either go through `Update` or are on the documented exemption list (see guard).
   Specialized paths (zoom, pin toggle, window bounds, split navigation) switch to `Update` with their precise flags.
7. Guards (A39's table-driven `LAYERS` mechanism, extended): exact-type rules in addition to namespace rules; member-token
   rules for `Settings.set_*`.

**Stays unchanged**: `Settings` POCO shape and JSON schema/file; MVVM rejected (A38); no new package; startup registration
(HKCU) and the notes-folder pointer are not JSON settings and are not folded into the event; focus/hide behavior
(`SettingsLostFocus`, deactivate handling) remains in the pad as host commands; no visible behavior change except the
persistence fixes named above.

**Why option A**: an explicit owner with batch + flags fits a code-built UI (no bindings), keeps disk persistence in `Store`,
and gives tests one seam. B (event on `Store`) mixes live-settings protocol with disk I/O; C (notifying facade with 56 forwarding
setters) duplicates the surface and still leaves list/JSON bypasses.

## Success looks like
- `grep -rn "SettingsChanged()" product/AntiNotepad` finds no hand call (the old method gone; the event raised only by the service).
- `grep -rn "PadWindow" product/AntiNotepad/UI/SettingsWindow*.cs UI/SettingsParts.cs` finds nothing (host interface only).
- `Settings` property setters outside the allowed set are zero (see guard); `DisabledLanguages` mutated only via the owner.
- Selftest output keeps `SELFTEST PASS` and adds, one line each:
  `LAYERS SettingsWindow->PadWindow 0 refs`, `LAYERS SettingsService->UI/Editor 0 refs`, `LAYERS Settings.set_* outside owner 0`
  (allowing owner methods and their `Update` lambdas, JSON/default construction, SelfTest fixtures only), and
  `SETTINGS Changed` behavior check: two fake subscribers; `Update` changing FontSize+Theme fires exactly once with both names;
  no-op fires 0; nested `Update` fires once more, not recursively; list edit detected; `Replace` fires once and `Current`
  identity is the new object; detached subscriber not called; geometry-only update does not reach a fake appearance subscriber.
  Plus existing checks (A30 snapshot-list-detached, round trips, Taskbar) still pass.
- Each new guard proven to fail on a deliberate violation (temporary violation, selftest red naming it, then removed).
- Build: 0 warnings, 0 errors.
- Live checks by the user in the dev copy (`tools/dev.ps1`), by hand: change theme/palette/accent/material/tint/UI scale and
  see pad and Settings preview update at once; font, size, width, spacing, paper, bubbles; caret style/blink/glide; reduce motion;
  bars/chrome/edge fade; Pinned and Taskbar apply immediately; hotkey change accepted and a bad/in-use hotkey rejected without
  being saved; Reset to defaults and Undo from the toast; search grid toggle survives a restart; zoom; drag/resize the pad and
  restart (bounds kept); show/hide with the hotkey feels as fast as before; Settings open while the pad is hidden/shown.

## Impact radius
- Settings read in 20 files; 239 property reads, 67 writes; 3 `SettingsChanged` call sites; 8 `PadView.ApplyLook` call sites
  (1 `SmoothCaret.SettingsChanged`); 3 `MotionPreview.ApplyLook`; 4 `SettingsWindow.ApplyLook`; 3 `ApplyAppearance`.
- Files likely touched: new `Model/SettingsService.cs` (+ change type); `Model/Store.cs` (ownership/Replace/load, in-place Normalize
  paths `:611,618` fall under owner); `UI/PadWindow.cs`, `.Keys.cs`, `.Navigation.cs`, `.Layout.cs` (subscribe, route writes);
  `UI/SettingsWindow.cs`, `UI/SettingsParts.cs` (host interface, drop `PadWindow`); `UI/MotionPreview.cs`; `UI/SearchOverlay.cs`;
  `Editor/PadView.cs` and `Editor/Motion.cs` (only if they subscribe themselves; otherwise untouched); `Program.cs` (composition);
  `SelfTest.cs` (new rules and checks). Roughly 10-14 files; no change to `Calc`, `Theme`, `Platform`.

## Risks
1. **Ordering**: appearance effects have a required order (Anim.Reduced and disabled-language hash before pane look; palette before
   look; hotkey validated before commit; taskbar/Topmost before chrome/save). Arbitrary subscriber order would break this -> one
   ordered handler.
2. **Re-entrancy**: `Set` -> chain -> `SyncLook` -> `Restyle` runs before `Set` resumes; palette picks change Theme and palette in one batch
   (`UI/SettingsWindow.cs:298-302`). Needs batching and queued nested updates.
3. **Show/hide speed**: geometry writes and visibility-driven taskbar reactions must not go through an all-settings event;
   no-op and same-value updates emit nothing.
4. **Partial propagation today** (Folding, InlineMath, SearchGrid/SplitSlot save): the refactor is behavior-neutral by default; fixing
   these is a separate, flagged decision. Only the missing-persistence fix is proposed.
5. **Identity**: `PadView.Settings`, `MotionPreview._s` retain references; `Replace` must refresh them (reset/undo do today).
6. **Threading**: no live-settings read on the background writer, but `_saveGate` does not protect POCO setters; UI-thread-only update
   rule needs an assertion. Never raise `Changed` from writer callbacks.
7. **Guards cannot prove everything**: A39's type-edge scan cannot see JSON reflection or `List.Add` on `DisabledLanguages` (no setter
   token); use owner API (`SetLanguageEnabled`) plus a whitelist of deserialize/replace entry points.
8. Rebuild churn in Settings (150 ms search timer and Closed handler per `Build`); broader callbacks could multiply rebuilds.
   Keep the `rows` flag semantics.
9. Hotkey and Reset/Undo transactions must keep their preserved fields (Width/Height/X/Y/Split/SplitSlot, Welcomed forced true).

## Depends on / conflicts with
- **A39-motion-out-of-ui (first, hard dependency)**: provides the `LAYERS` table this plan extends and moves `Anim`/`ChromeTokens`/
  `MenuEntry`; `UI/SettingsWindow.cs` and `PadWindow.cs` call sites will shift (re-verify all line numbers). Needs the rule table to
  support exact types and member-token rules (small extension, in this task).
- **A41-padwindow-split (conflict, high)**: moves logic out of `PadWindow` (navigation, save loop, hotkeys); both touch
  `PadWindow*.cs`, `SettingsChanged`, `RegisterHotkey`, `Toast`. Run A40 before A41 as ordered; A41 then moves the subscriber
  handler with the rest.
- **A42-padview-commands (low)**: `PadView` text commands; only overlaps if `PadView` subscribes directly.
- **A43-app-context (medium)**: replaces `Program.Store`/statics; the composition root of `SettingsService` should be created in the
  place A43 introduces, or A40 creates it minimally and A43 relocates it.
- **A44-platform-seam (low)**: hotkey/Win32 behind `Platform`; the host interface's hotkey members may later move there.
- Order: A39, A40, A41, A42, A43, A44.
