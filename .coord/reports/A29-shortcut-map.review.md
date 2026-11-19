# A29-shortcut-map lens review

Reviewed HEAD `973f281aa8da5c1fed19d703b2deef4b45d83503` using `git diff dev...HEAD -- product docs AGENTS.md`, the card, gatefix/gate reports, research inventory/audit, coding rules, changed code and affected callers. The missing implementer report is reconstructed below.

## Findings (proven)

1. **[major] product/AntiNotepad/UI/Shortcuts.cs:210 — Shift+Backspace can replace character deletion.** The recorder produces `Shift+Backspace`, and BlockedReason returns null. A New note override with that value resolves to `ShortcutRoute { Pad = NewNote, Search = None }`. The window consumes it before the editor (`PadWindow.Keys.cs:43`), so it creates a note instead of deleting a character. AvaloniaEdit explicitly binds Shift+Backspace to Backspace (`third_party/AvaloniaEdit/Editing/EditingCommandHandler.cs:70`). Probe: `BLOCK Shift+Backspace: ALLOWED`; `EDIT new-note=Shift+Backspace; route=True: ShortcutRoute { Pad = NewNote, Search = None }`. Smallest fix: reserve Shift+Backspace and add a capability check. The lab also omitted this case, but the card's text-editing reservation and supplied audit establish the intended behavior.

2. **[major] product/AntiNotepad/UI/Shortcuts.cs:98 — Duplicate/bad saved values can remove Search instead of restoring defaults.** Refresh removes all overridden defaults before validating overrides in table order. With `new-note = Ctrl+F` and `search = Ctrl+F`, New note takes the vacant Ctrl+F; Search's duplicate and fallback Ctrl+F are both rejected at lines 110–111. Search ends unassigned and Ctrl+N disappears. Probe: `DUPLICATE FALLBACK new-note=Ctrl+F; search=[]`. Replacing Search's saved value with `garbage` gives the same failure: `FALLBACK new-note=Ctrl+F; search=[]; Ctrl+F=True: ShortcutRoute { Pad = NewNote, Search = None }`. Smallest fix: resolve invalid/duplicate override sets together before committing routes, preserving fallback defaults as well as valid swaps and explicit clears. Add both paired-override cases to the selftest.

3. **[minor] product/AntiNotepad/UI/PadWindow.Navigation.cs:170 — Slot promotion advertises a removed default.** DispatchShortcut(KeepSlot) calls PromoteToSlot, which ends with `Toast($"Kept in slot {slot} · Ctrl+{slot} opens it")`. The default map's Open slot label is empty, and the probe finds all 36 Ctrl/Ctrl+Shift top-row/numpad slot routes absent. Binding Keep slot alone therefore advertises an unassigned key; rebinding Open slot leaves the toast wrong too. This is an omitted dependent caller outside the submitted diff. Smallest fix: read the effective Open slot binding and omit the hint when unassigned; add this dependent file to rework scope.

4. **[minor] product/AntiNotepad/UI/PadWindow.Keys.cs:15 — Dispatch interception is a product test hook.** ShortcutDispatchOverride runs before every real action at line 84. Its only assigning caller is `SelfTest/UI.cs:278`, which returns true to bypass command bodies. This violates lens checklist item 9 (test hooks outside product classes) and limits what the dispatch check proves. Smallest fix: remove the product override and check safe action outcomes or use test-owned observation.

5. **[minor] product/AntiNotepad/UI/SettingsWindow.cs:505 — New non-trivial methods lack required summaries.** ShortcutsPage, ShortcutControl, ApplyShortcut, SaveShortcut, ResetShortcut, ResetAllShortcuts and ConfirmShortcutTake lack `///` summaries. Collision/routing helpers in `Shortcuts.cs:271`, `:280`, `:294`, `:302` also lack them. Add concise purpose summaries to meet the on-touch coding standard. Touched file headers are present; no new history comments were found.

## Behavior changes check

Report's list vs the diff: **missing** (main implementer report absent). Gatefix documents formatting/token fixes only. Reconstructed changes and deviations follow.

## Change list and behavior changes

- Adds a 30-action table, pad/search scopes, exact parsing, digit families, override validation, allocation-free routes, Help rows and a checked documentation table.
- Adds Settings.ShortcutOverrides, detached save copies and a SettingsService dictionary-content comparison/change flag. Empty string clears; removing an override resets. Existing store persistence serializes the setting.
- Moves fold/unfold and checkbox/timer dispatch from PadView to the window; their new editor entry points have only window-dispatch callers. Search grid/trash also use the window route. Fixed Search controls now use exact modifiers.
- Adds Settings Shortcuts Change/Record, Save, Clear, row Reset, Reset all and same-scope conflict takeover. Accepted takeover clears the previous owner in the same Settings update. Existing System global-hotkey rows remain unchanged.
- Reads configurable Help, app-menu, toolbar, pager and Search-button labels from the map; adds docs/shortcuts.md and the AGENTS layout mention. The slot toast is still stale. Fixed Esc labels remain literal.
- Adds the shortcut contract/selftest, updates Settings' property count and pins the changed Help/Settings design hash.

**Removed named defaults vs dev:** Ctrl+P/K (Search aliases); Ctrl+PageUp/Down (note aliases); Ctrl+, (Settings); Ctrl+0, Ctrl+NumPad0 and Ctrl+tilde (stack); Ctrl+1–9 and Ctrl+NumPad1–9 (open slots); Ctrl+Shift+1–9 and Ctrl+Shift+NumPad1–9 (keep slots); Alt+Shift+Up/Down (reorder); Ctrl+Shift+Backspace (delete note); Ctrl+/ and Ctrl+Shift+/ (old question-key Help forms); Ctrl+G in Search (grid); Ctrl+T in Search (trash). Ctrl+T still pins outside overlays. First/latest actions are newly bindable and had no explicit dev defaults.

Accidental supersets also stop firing: Shift+F1; Alt+Shift+Left/Right; extra Alt on old Ctrl actions; extra Shift where the old cases allowed it (Settings, split, stack, Help and zoom-down). Modified Esc no longer hides/closes through the window. Fold and checkbox/timer now require exact modifiers. Search arrows/Enter/select/delete no longer accept broad supersets. Ctrl+plus deliberately accepts the physical Ctrl+Shift+OEM-plus equivalent; Add/Subtract/OemPipe normalize to their physical families.

The probe found every named non-digit removed route absent and 36/36 slot digit routes absent. Window source has no old command-switch fallback; Search's Ctrl+G/T handlers and the editor's direct fold/checkbox handlers are removed. Ctrl+T resolves to Pad KeepOnTop with Search None; the overlay branch does not execute Pad pin. Released keys can still perform ordinary editor operations.

## Scope and plan check

- **In plan:** all 16 submitted product/docs/AGENTS files serve the approved map, routing, Settings, labels or checks. No unrelated icons, styles, renames or defaults found. Gatefix's token substitutions preserve spacing values. SettingsParts.cs and the lab required no implementation edit.
- **Unlisted deviations:** product dispatch hook; omitted dependent toast; incomplete editing reservations and multi-override fallback. Fixed Search modifier restrictions and modified Esc are additional behavior changes disclosed above. Slot tooltips now use the digit-family label rather than the individual digit. No main report disclosed these changes.
- **Must not change:** Platform/global registrations and their System Settings rows have no diff. Save/shutdown bodies, scheduler and session-end code have no diff; the new setting uses existing notification/persistence paths. A31/A32-specific implementation areas are untouched. Shared PadView edits stay in approved shortcut entry points/handler removal, away from list keywords, conversion and context menus.
- **Contracts/callers:** optional HotkeyRecorder callbacks preserve the existing global-recorder callers; only ShortcutControl supplies the new policy. SettingsService's new flag is consumed by PadWindow.HandleSettingsChanged and included in Replace/All, whose count check was updated. ShortcutMap callers are window dispatch/refresh, Settings controls, Help/menu/tooltips and selftests. New editor entry points are called only by DispatchShortcut. No Model/Editor dependency on UI was introduced.
- **Typing:** one window TryGet after fixed Escape handling; a value-type gesture and one dictionary lookup. No parsing, strings, enumeration or new text-input handler on normal typing. Zero-allocation selftest passes. The hook check runs only for matched actions.
- **State/timing/cleanup/failure:** map rebuilds are synchronous on the UI/Settings path; dictionary changes compare contents and save copies detach. Ordinary clear/reset/load cases pass. The conflict dialog saves only after acceptance. No additional proven latch, stale deferred input, resource-growth, save-failure or shutdown regression was found.

## Unproven risks

- Visible recording, conflict-dialog cancel/take, restart persistence and live Help/tooltip refresh require the card's hand checks; no windows were shown.
- Independent dispatch probes intercept action bodies; they do not execute OCR, clipboard, quit, fold or checkbox/timer side effects.
- Live typing latency was not measured; source inspection and allocation checks support only the stated hot-path properties.

## Checks run

- Offline build: NuGet.Config with no sources; telemetry/audit disabled; output/intermediates inside build/A29-shortcut-map-review. **Build succeeded; 0 warnings; 0 errors; 9.33 seconds.**
- Isolated DLL `--selftest`, DATA/TEMP/TMP in the review folder: **CONTRACTS: 36 rows, 35 pass, 1 manual; SELFTEST PASS.** Includes Help/docs parity, save/load/clear/reset, detached copies, dictionary notifications and allocation checks. First GUI-apphost invocation produced no captured output; the DLL invocation provided the verified result.
- Independent compiled reflection probe against the built assembly, without showing PadWindow: **DEFAULT WINDOW DISPATCH exactly once=15/15**. Each event was handled and observed exactly one expected action.
- Extra-modifier variants: **40/40 unbound**, excluding physical Ctrl+plus Shift alias. Removed named non-digit routes absent; **36/36** digit routes unbound. Ctrl+Shift+comma absent; Ctrl+T maps to Pad pin only.
- **18/18** reserved Ctrl+Alt+Shift digit/function chords refused; Windows-modifier recorder text null; Alt+F4/Ctrl+Z refused. Shift+Backspace incorrectly allowed.
- Duplicate and malformed paired overrides reproduce missing Search fallback (finding 2).
- Source checks cover window/editor double handling, remaining labels, coding summaries and scope. Supplied gate is PASS; it was read, not rerun.
- Every build/selftest/probe process completed within three minutes. Product files/git metadata were not edited; no resident AntiNotepad was launched or killed. Review build folder removed after recording results.

VERDICT: rework
