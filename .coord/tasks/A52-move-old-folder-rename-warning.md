# A52-move-old-folder-rename-warning: rename the old notes folder after a move, and show the right message

Role: director (gpt-6.1-sol medium), workers gpt-6-luna xhigh. Product `product/AntiNotepad`.
Branch: `feat/A52-move-old-folder-rename-warning` in `.claude/worktrees/A52-move-old-folder-rename-warning` (already checked
out; never switch, merge or commit: you cannot write to `.git`).
Tier: High (notes folder and move path; rules: `docs/loop.md`)
Read first: `AGENTS.md`, then the files below. Line numbers are from the current tree; re-read before editing.

## Goal
User report (2026-10-27): moving notes from `test targetA` to `B` in Settings logs "Notes are now in the new folder, but the
old folder could not be renamed and remains at <old>: The parameter is incorrect." The notes move fine. Wanted:
1. Root cause (A52-move-old-folder-rename-warning-audit, probe-verified): a trailing separator on the source path.
   `KeepOldCopy` (`Platform/NotesLocation.cs:207-222`) takes the parent from the untrimmed path (line 212) but trims only
   for the leaf (213), so `P\test targetA\` gets the backup `P\test targetA\test targetA (moved …)`, a child of itself, and
   `Directory.Move` fails with 0x80070057. Fix: derive parent and leaf from one path trimmed with
   `Path.TrimEndingDirectorySeparator` (roots preserved), and make the app-home comparison treat trailing forms as equal.
   No store, watcher or handle changes: an open store is not the cause.
2. `Move` returns the actual outcome explicitly: where the old copy was kept (rename or app-home backup), or that it stayed
   in place and a plain reason (name collision, access denied, in use, other). `NotesSession.MoveNotes` passes it on.
3. Settings uses that outcome instead of scanning folders (`NewBackupDirectory`, the `backupRoot` at
   `UI/SettingsWindow.cs:697` has the same untrimmed-path bug) or `StartsWith` on the message (line 711). Success: name the
   exact backup once. Failure: one plain sentence that the notes moved, the old folder stays at <path>, and why; no raw
   exception text and no "kept as" claim. Today line 710 says "The old folder was kept as …" even when the rename failed.
Must not change: the copy/verify/rollback logic in `Move`, the pointer write, the app-home backup path
(`BackupAppHomeData`), move refusals (same or nested, target has `index.json`), and the notes themselves.

## Files
Only these files may change. Touching any other file needs a line under "Out-of-scope files" in the report.
- `product/AntiNotepad/Platform/NotesLocation.cs`
- `product/AntiNotepad/NotesSession.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/SelfTest.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`

## Contracts
Existing rows to keep passing: `store-unavailable-save-noop`, `store-folder-returns`, `startup-unavailable-store`,
`settings-changed-notifies`.
New row (this fix's promise): `notes-move-renames-old-folder` — after a successful move of a non-app-home folder, the old
folder no longer exists at its old path and a `<leaf> (moved …)` folder holds its files; if the rename cannot happen, the
move still succeeds and the result message names the old path.

## Constraints
- Smallest change that meets the goal. No drive-by refactors; the shared path rule is A68-notes-path-dedupe, not this item.
- Reproduce first: a selftest that moves a non-app-home temp source given with a trailing `\` and with `/`, failing
  before the fix. (Existing `NotesMoveChecks` starts from app home, `SelfTest.cs:1698`, so it never reaches this branch.)
- The new contract covers: trimmed and trailing forms, a real non-app-home rename with file contents kept, a forced
  retention failure (backup-name collision) where the move and pointer/session switch still succeed, app-home equivalence
  with a trailing separator, and the outcome data Settings displays.
- Build into `build/A52-move-old-folder-rename-warning-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there,
  then delete that folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A52-move-old-folder-rename-warning -Offline`.
- Never launch or kill AntiNotepad. Never run `--quit`. Never touch the user's real notes folder or `%LOCALAPPDATA%`.

## Done means
- The selftest that reproduced the failure now passes; `CONTRACTS` has `notes-move-renames-old-folder` passing.
- Settings shows one correct message for both outcomes.
- Build 0 warnings, `SELFTEST PASS`, the gate passes.

## Before you report (director only)
Spawn the `lens_reviewer` agent on your diff (it reads `.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why
not.

## Report: `.coord/reports/A52-move-old-folder-rename-warning.md`
Keep it short. Use exactly these sections:
1. **Summary**: two or three sentences, including the root cause with evidence.
2. **Change list**: `file:line-range | what changed | which goal line it serves`.
3. **Behavior changes**: every changed result, message, timing or shared state. "None" only if true.
4. **Out-of-scope files**: file and why, or "None".
5. **Contracts**: rows added or changed.
6. **Lens review**: findings and what you did with each.
7. **Checks run**: build, selftest, gate result lines.
8. **Hand checks for the user**: what to try in the dev copy.
