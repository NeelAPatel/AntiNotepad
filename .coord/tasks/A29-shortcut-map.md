# A29-shortcut-map: one shortcut table, a Settings page to record keys

Item card (`docs/loop.md`). Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB (detail in the files named).

Tier: Medium · editor-feature (new setting, key dispatch)
State: building
Branch: `feat/A29-shortcut-map` in `.claude/worktrees/feat/A29-shortcut-map`

## Request
User via Master 7, 2026-11-19: "turn this into a work item to set up proper shortcuts for the app since it was done relatively randomly". Then: "Make a localhost shortcut manager where i can record keystrokes to set shortcuts and clear the ones that arent needed... by default only the basics will have shortcuts ready ... we dont use ctrl alt shift 1~9/F1~F9 (dev/worktree copies)". Lab: `tools/playground/a29-shortcuts-lab.html`. Export decides defaults; "REMOVE" means no default key, still bindable (user, 2026-11-19: "it wont have a shortcut set but if they decide to add one, it will work").

## Findings
Full inventory: `.coord/research/A29-key-inventory.md`; Codex audit: `.coord/research/A29-key-audit.md`.
- Keys decided in `PadWindow.Keys.cs` (window, tunnel), `PadView.OnKeyDown` (fold, Ctrl+Enter), `SearchOverlay` (grid, trash); help, menu labels and tooltips are separate hand lists that drifted.
- The window uses `HasFlag`, so several keys also fire with extra modifiers. The map must match exact modifiers.
- Alt+Shift+arrows shadow AvaloniaEdit rectangular selection: not used as defaults.

## Plan
**After:** `UI/Shortcuts.cs` holds one table (id, name, section, scope, default keys). Settings stores only changes (id to key text; "" = cleared). One lookup (key + modifiers) feeds the window, editor and search handlers; help, app menu and tooltips read the same map. A Settings page **Shortcuts** lists actions: Record (reuse `HotkeyRecorder` rules for in-window keys), Clear, Reset row, Reset all; a key owned by another action asks to take it. Rules (same as the lab's `blocked()`): refuse text-editing keys, Windows keys, Ctrl+Alt+Shift+1..9, F1..F9 and S. `docs/shortcuts.md` lists scheme and table.
**Defaults (the export):** New note Ctrl+N; Next / Previous note Alt+Right / Alt+Left; Search Ctrl+F; Help F1; Keep on top Ctrl+T; Split Ctrl+\; Text size Ctrl++ / Ctrl+-; Copy whole note Ctrl+Shift+C; OCR Ctrl+Shift+O; Checkbox or timer Ctrl+Enter; Fold Ctrl+Shift+[ and ]; Quit Ctrl+Shift+Q. **No default key (still bindable):** Settings, First note, Latest note, Move note earlier / later, Delete note, Next / Previous alias, Search aliases, Grid, Trash, Back to the stack, Open slot 1-9, Keep note in slot 1-9 (a digit binding stands for the family 1-9). Esc stays fixed (closes overlays, hides). Fixed and listed read-only: Enter, Tab, Ctrl+[ ], paste, undo, Ctrl+Home, Ctrl+click.
**Search key toggles (user, 2026-11-19):** the Search key opens the global search and, pressed again, closes it (like the toolbar button, `ToggleSearch`); current-note search is a later feature, no plan. Built in r3 after r2.
**Must not change:** global hotkeys and their Settings rows; save/shutdown (A30); typing speed (one dictionary lookup per key, no allocation); A31/A32 files.
**Tests:** unique default keys per scope; defaults pass the block rules; reserved keys refused; every default key fires its action through `OnWindowKeyDown`; exact modifiers (Ctrl+Shift+, does not open Settings); save, load, clear, reset round trip and invalid or duplicate saved values fall back; help rows equal the map; docs table equals the map. One CONTRACTS row. **Gates:** gate + Codex review; hand test in the slot dev copy.
**Needs from you:** hand test: record, clear, reset, restart; conflict prompt; F1 help lists live keys.

## Files
- `product/AntiNotepad/UI/Shortcuts.cs`
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/HelpOverlay.cs`
- `product/AntiNotepad/UI/SearchOverlay.cs`
- `product/AntiNotepad/UI/HotkeyRecorder.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/UI/SettingsParts.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/Model/Settings.cs`
- `product/AntiNotepad/Model/SettingsService.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest/Design.cs` (pinned design hash: Settings and Help gain rows)
- `product/AntiNotepad/UI/PadWindow.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs` (slot toast names the real key)
- `product/AntiNotepad/Model/Welcome.cs` (example note no longer names Ctrl+Enter)
- `product/AntiNotepad/Model/Showcase.cs`
- `tools/playground/a29-shortcuts-lab.html`
- `docs/shortcuts.md`
- `AGENTS.md`

## Contracts
- new row `shortcut-map`: one key, one action per scope; defaults avoid text-editing, Windows and dev-slot keys.

## Approve
User, 2026-11-19: "Approved but here's my idea: ..." (see Request); basics "Use my list"; lab export then "yes thats the idea. it wont have a shortcut set but if they decide to add one, it will work".

## For Codex
Role: worker (gpt-6-luna xhigh), because many files, shared state and key dispatch. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, `.claude/rules/coding.md`, the Files, the lab page's `blocked()` and `WARN` rules and `.coord/research/A29-key-inventory.md`; re-read before editing.
- Build exactly the Plan's "After" for the Files above; no drive-by changes. If the Findings are wrong, stop and report that.
- Build into `build/A29-shortcut-map-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, delete the folder. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A29-shortcut-map -Offline`.
- Never launch or kill AntiNotepad, never run `--quit`, never edit `.coord/STATUS.md` or this card. Offline only.
- Done means: every default works as listed, nothing else changed for the user today except removed default keys; tests pass; build 0 warnings, `SELFTEST PASS`, gate passes.
- Run `lens_reviewer` on your diff and fix what it proves.

Report `.coord/reports/A29-shortcut-map.md`: 1 Summary; 2 Change list; 3 Behavior changes (keys removed from defaults); 4 Out-of-scope files; 5 Plan deviations; 6 Contracts; 7 Lens review; 8 Checks run; 9 Hand checks.

## Check
- Gate PASS at the branch tip (build 0 warnings, SELFTEST PASS, tokens, format, scope). Codex review 1: rework (5 findings, fixed in r2); re-review: one narrow case left (garbage saved Search value while New note holds Ctrl+F), justified: not reachable from Settings, r2 brief allowed it.
- Within plan: all defaults from the export; Search key toggles (user, r3); Help toggles on F1, `Ctrl++` label, no default for Checkbox or timer, keyboard glyph (user hand test, r4); example notes say click the box (r5).
- Removed defaults versus dev: see `.coord/reports/A29-shortcut-map.review.md` ("Removed named defaults"). Alt+Shift+arrows now free for rectangular selection.
- Misses: Codex worker run 1 timed out on a hung selftest (found at env: gate/run overlap), never wrote its report (cause `env`); Help row icon and Ctrl+Enter default were hand-test finds (cause `plan`: lab export kept Ctrl+Enter, user wanted none).
- Noticed: `tools/files-overlap.ps1` returns a misleading "No card" error for an empty `## Files` section (small T fix).

## Hand test
User, 2026-11-19: "Passed" (F1 toggles Help, Ctrl++ label, Checkbox or timer unbound, keyboard icon, Search toggle, Settings record/clear/reset/restart).
