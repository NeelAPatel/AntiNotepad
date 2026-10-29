# A52-move-old-folder-rename-warning: rename the old notes folder after a move, and show the right message

Role: director (gpt-6.1-sol medium), workers gpt-6-luna xhigh. Product `product/AntiNotepad`.
Branch: `feat/A52-move-old-folder-rename-warning` in `.claude/worktrees/A52-move-old-folder-rename-warning` (already checked
out; never switch, merge or commit: you cannot write to `.git`).
Tier: High (notes folder and move path; rules: `docs/loop.md`)
Read first: `AGENTS.md`, then the files below. Line numbers are from the current tree; re-read before editing.

## Goal
User report (2026-10-27): moving notes from `test targetA` to `B` in Settings logs "Notes are now in the new folder, but the
old folder could not be renamed and remains at <old>: The parameter is incorrect." The notes move fine. Wanted:
1. Find why `Directory.Move(currentFolder, backup)` in `KeepOldCopy` (`Platform/NotesLocation.cs:207-222`) fails, and fix
   it so the old folder is renamed to `<leaf> (moved yyyy-MM-dd HHmm)` as designed. Suspects to rule in or out with
   evidence: an open handle or watcher on the old folder (store, search, recovery), path form (trailing separator, long or
   relative path), the app's working directory.
2. The Settings result message tells the truth: today `UI/SettingsWindow.cs:710` says "The old folder was kept as …" even
   when the rename failed, beside the failure notice. When the rename succeeds, say where the old copy is; when it fails,
   one plain sentence with the old path and the reason, no raw exception text.
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
- Reproduce first: a selftest that moves a temp folder while the store (or whatever holds the folder) is open, showing the
  failure before the fix.
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
