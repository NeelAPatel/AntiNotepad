# A31-list-default-type: Settings choice for what the list keyword makes

Item card (`docs/loop.md`). Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB.

Tier: Medium · editor-feature
State: approved
Branch: `feat/A31-list-default-type` in `.claude/worktrees/feat/A31-list-default-type`

## Request
User via Master 7, 2026-11-19: a Settings choice for what `list` creates, plus direct keywords `checklist`, `bullet`/`bulletlist`, `numberlist`. Constraints: do not touch save/shutdown code (A30 open); no slowdown of typing or show/hide; Codex offline only; follow `.claude/rules/coding.md`.

## Findings
Own scout (about 6 files).
- `Model/Modes.cs`: `list`, `todo` are keywords for `NoteMode.List`; `Modes.Detect` is static (callers: Note, Store, Store.Search, NoteAnalysis, PadView).
- The checkbox is hard-coded only in `Editor/ListEditing.Continue` (plain line in List mode gets `\n[ ] `) and `Editor/Completion.cs:152` (Enter on `list`/`todo`). Prefixed lines continue in their own style.
- `ListEditing.ListKind` and `ConvertLine` exist on dev (A32); `TextCommands.AsList` still takes strings.
- Settings pattern: `SaveDelayMs` (normalizing setter), `SettingsService.cs` table, `SettingsWindow.cs`/`SettingsParts.cs`, round trip at `SelfTest/Store.cs:519`.

## Plan
- **After:** Reuse `ListEditing.ListKind {Checklist, Bullet, Numbered}` (added by A32, on dev); do not add a second enum. `ModeKeyword` gets an optional kind: `list` follows setting `ListDefault` (bullet | check | number, default **bullet**, unknown falls back to bullet); `todo`, `checklist` = check; `bullet`, `bulletlist` = bullet; `numberlist` = number. All stay `NoteMode.List`. `Modes.ListKindOf(firstLine, default)` feeds `ListEditing.Continue` and `Completion`. Popup lists all new words; `list` row reads "Your default: <type>".
- **Must not change:** save/shutdown code; prefixed lines; other keywords. Known change: plain lines in existing `list` notes continue as bullets.
- **Settings control:** "New list lines" pills (Bullets, Checkboxes, Numbers) in the Editor group, per approved lab `tools/playground/a31-list-default-lab.html`.
- **Test plan:** selftests: default, invalid value, round trip, each keyword's kind, `todo`, continuation per kind. Hand: change the setting, type `list`, new words, popup text.
- **Gates:** gate + Codex review; hand test in the slot dev copy.

## Files
- `product/AntiNotepad/Model/Modes.cs`
- `product/AntiNotepad/Model/Settings.cs`
- `product/AntiNotepad/Model/SettingsService.cs`
- `product/AntiNotepad/Editor/ListEditing.cs`
- `product/AntiNotepad/Editor/Completion.cs`
- `product/AntiNotepad/Editor/TextCommands.cs`
- `product/AntiNotepad/Editor/DocumentCommands.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/UI/SettingsParts.cs`
- `product/AntiNotepad/SelfTest/Lists.cs`
- `product/AntiNotepad/SelfTest/Store.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
- `tools/playground/a31-list-default-lab.html`

## Contracts
- Add row: `list` follows `ListDefault`; `todo` always checkboxes.

## Approve
User, 2026-11-19: "Approve" (decisions verbatim: "bullet list should be default, with settings option to change what list invokes by default."; "Stay checklist" for `todo`; "Yes show all. the help text should indicate default is <a type> but yes the popup is a good idea"; lab approved with "(Settings)" removed from the popup text).

## For Codex
Role: terra, a feature across a few files. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then the Files.
- Build exactly "After"; no drive-by changes. Build into `build/A31-list-default-type-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, delete the folder. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A31-list-default-type -Offline`.
- Never launch AntiNotepad or edit STATUS or this card. Offline checks only.
- Done means: the Plan's tests exist and pass; build 0 warnings, `SELFTEST PASS`, gate passes.

Report `.coord/reports/A31-list-default-type.md`: 1 Summary; 2 Change list; 3 Behavior changes; 4 Out-of-scope files; 5 Plan deviations; 6 Contracts; 7 Checks run; 8 Hand checks.

## Hold
- Held for A29 (user, 2026-11-19: "Wait for A29 merge"); A29 merged at 3e5bc0a, hold over. Check Settings layout and keys against `docs/shortcuts.md`.

## Check
- Gate (r2, `-Offline`): PASS; plain run only failed on NuGet audit warnings (env). Codex review: no findings, VERDICT accept.
- Within plan: all Plan "After" items. Justified: four extra files (DocumentCommands, PadView, UI.cs, Design.cs) only carry the setting or update field count and design hash; now in Files. Justified: Model keeps a string kind mapped to `ListEditing.ListKind` (layer boundary, no second enum).
- Known behavior change, as approved: plain lines in existing `list` notes continue as bullets.
- Noticed: gate Scope reads one path per Files bullet; the card template could say so (Loop item, not filed).
- Misses: found at gate Scope and Format; cause `code`/card format; caught by gate; lesson: one path per Files bullet.
- Commit review: 11 commits folded to 3 (card and lab; code; reports and check); gate PASS at tip, catch-up RETEST: no.

## Hand test
Dev copy (blue slot, A31 worktree build). User, 2026-11-20, verbatim: "All pass"
