## Summary
Implementation is not accepted: Esc-routing blocker awaiting scope clarification. Replaced typed shortcut entry with a key-press recorder for both Settings rows. Save applies explicitly; Cancel and focus loss discard the candidate, while optional Clear removes the new-note shortcut.

## Change list
file:line-range | what changed | which Plan line it serves
--- | --- | ---
product/AntiNotepad/UI/HotkeyRecorder.cs:1-225 | Recorder, key mapping, focus handling, key caps and actions | All After recording states; separate file; shared button factory
product/AntiNotepad/UI/SettingsWindow.cs:498-499 | Both rows use HotkeyControl | Keep row text and call shape
product/AntiNotepad/UI/SettingsWindow.cs:539-541 | Replace old editor with forwarder | Remove typed box and extract recorder
product/AntiNotepad/SelfTest/UI.cs:15-52 | Supported-key round trips, modifier order, rejected keys and commands | Code test plan
product/AntiNotepad/SelfTest/Contracts.cs:22-27 | Add recorder contract | One CONTRACTS row

## Behavior changes
Both shortcut rows record keys instead of accepting typed text. New per-recorder state holds the active capture session and candidate text; release retains it, a new supported key replaces it, modifiers alone disable Save, and unsupported keys preserve it with an error. Save or unmodified Enter applies; Cancel or focus leaving the control discards it. Esc is intended to cancel and modified Esc to record, but the existing window tunnel handler currently intercepts both; see Lens review. Focus changes among the recorder actions do not cancel. Optional Clear applies empty text. Failure keeps the session open and delegates the unchanged rollback behavior to the host. The pure mapper returns canonical modified supported-key text or null; command classification distinguishes unmodified Esc/Enter from recordable modified variants. No registration APIs, defaults or persisted text format changed. Dispatcher posts focus after attachment and checks focus loss after internal button focus changes.

## Out-of-scope files
No implementation edits outside the four allowed product files. This requested report and generated gate report are coordination artifacts. The default dev-relative gate also sees tools/lab/A10a-hotkey-recorder-lab.html from the already-committed approved lab; it was not edited by this run.

## Plan deviations
The recorder owns a ContentControl exposed through View, preserving the idle logical tree and pinned design snapshot. No user-facing deviation. Work stopped at the proven Esc-routing blocker as instructed when findings prove incomplete; fixing it needs a SettingsWindow edit outside the permitted block. Default dev-relative gate also includes the prior approved lab commit.

## Contracts
Added hotkey-recorder-round-trip: all 66 supported main keys with Ctrl+Alt round-trip through the unchanged parser; every other Avalonia key returns null; command and modifier-order checks pass.

## Lens review
VERDICT: rework. Proven major at UI/SettingsWindow.cs:77-83: its ancestor tunnel handler consumes Escape before the recorder; Esc closes Settings or clears search, and Ctrl+Esc cannot be recorded. The smallest fix needs an edit to that handler, outside the explicitly permitted block in SettingsWindow.cs. User scope-extension question pending; no fix applied.

## Checks run
Build: succeeded, 0 warnings, 0 errors (NuGet audit disabled for offline check).
Selftest: CONTRACTS: 16 rows, 15 pass, 1 manual; SELFTEST PASS; original design hash preserved.
Final exact offline gate: Build PASS (0 warnings), Selftest PASS, Seam PASS, Tokens PASS, Format PASS, Size PASS; Scope FAIL solely for the already-committed approved lab relative to dev; GATE: FAIL. Supplemental offline gate with -Base HEAD: GATE: PASS, all four implementation files in scope. This does not resolve the lens blocker or replace the failed default-base gate. Automatic approval review rejected cleanup of build/A10-hotkey-recorder-check twice (blocked by policy); folder remains, no further attempts made. git diff --check passed.

## Hand checks for the user
- In both rows and light/dark themes: Change shows static waiting dot, disabled Save and outlined Cancel; only the optional row shows Clear.
- Ctrl+Alt+K fills accent caps live, release retains them, Save applies, and the new global shortcut works. Also check unmodified Enter applies.
- Esc and Cancel restore old caps. Moving focus outside the recorder cancels; clicking its actions works.
- Hold only Ctrl: dashed ghost key cap and disabled Save. Press an arrow/punctuation key: error hint, existing caps unchanged.
- Try modified Esc/Enter, Tab and Win combos. OS interception and registration remain hand-test territory; use a delivered but occupied combo to test refusal, red message and old shortcut still working.
- Optional Clear shows None and disables its global binding; repeat Change/Cancel/Save/Clear sessions.
