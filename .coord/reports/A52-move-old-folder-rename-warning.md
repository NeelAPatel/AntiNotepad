## Summary
A trailing separator made KeepOldCopy derive a parent inside the source while deriving its leaf from a trimmed path. Before the fix, both backslash and slash selftest cases copied successfully but left the source in place with "The parameter is incorrect"; the fix derives both parts from Path.TrimEndingDirectorySeparator and returns the actual retention outcome.

## Change list
- product/AntiNotepad/Platform/NotesLocation.cs:8-34,158-160,234-290 | explicit old-copy path/failure/message, consistent source trim and app-home equality; existing app-home backup naming and operations preserved | goals 1-2.
- product/AntiNotepad/NotesSession.cs:24-49 | pass the move result to callers, use its retention message, preserve switching and warning logging | goal 2.
- product/AntiNotepad/UI/SettingsWindow.cs:684-707,733-736 | display the outcome once, use explicit availability notice, remove backup discovery and redundant file count | goal 3.
- product/AntiNotepad/SelfTest.cs:1699-1743,1766-1893 | adapt session calls; reproduce trimmed/backslash/slash rename and verify paths, content, app-home equivalence, collision outcome and subsequent target saves | goals 1-3 and Contracts.
- product/AntiNotepad/SelfTest/Contracts.cs:51-56 | register notes-move-renames-old-folder | Contracts.

## Behavior changes
- Non-app-home paths with a trailing separator now rename to the correct sibling; app-home trailing forms use the existing app-home data backup.
- Successful Move results include OriginalFolder, exact BackupFolder (or null when no old data), and nullable failure category; NotesSession.MoveNotes adds an out result (null on preflight failure).
- Success text includes the verified copied file count and full backup path once; retention failure text says notes moved, names the original path and plain reason without raw exception text or a kept-as claim. Startup notices and session warning logs use that plain failure text too.
- Settings no longer enumerates files/directories to infer count/backup and no longer parses message prefixes. Target-unavailable notices use NotesLocationError explicitly.
- Copy/verify/rollback, pointer writing, move refusals, note contents, and store switching order are unchanged; no timers or shared mutable state added.

## Out-of-scope files
- .coord/reports/A52-move-old-folder-rename-warning.md: required task report.
- .coord/reports/A52-move-old-folder-rename-warning.gate.md: generated required gate evidence (ignored).
STATUS is unchanged because docs/loop.md reserves STATUS edits for the manager on dev; this feature branch does not edit it.

## Contracts
Added notes-move-renames-old-folder: trimmed and both trailing forms, preserved backup/target contents, exact success messages, trailing app-home equivalence, forced name collision with exact plain failure message, pointer/session/store switch and later target save. All existing rows retained: CONTRACTS: 7 rows, 6 pass, 1 manual.

## Lens review
Director spawned lens_reviewer using the project skill. VERDICT: accept; no proven findings or unrelated hunks. Reviewer checked changed callers, retention/failure paths, preserved move operations and final contract coverage; no fixes required. Live Settings rendering remains for the dev-copy hand check.

## Checks run
- Pre-fix: build 0 warnings, 0 errors; SELFTEST FAIL on both trailing separator cases, with notes copied and source unrenamed.
- Final task-check build (AVALONIA_TELEMETRY_OPTOUT=1, NuGetAudit=false): 0 warnings, 0 errors; SELFTEST PASS; CONTRACTS: 7 rows, 6 pass, 1 manual.
- pwsh -NoProfile -File tools/gate.ps1 -Task A52-move-old-folder-rename-warning -Offline: GATE: PASS; build 0 warnings/errors, selftest exit 0, seam 0 violations, scope all five product files allowed. Size WARN: SelfTest.cs 2637 -> 2763 lines; Offline WARN: NuGet audit disabled for this build only.
- git diff --check: pass. Gate build/data output removed. Separate build/A52-move-old-folder-rename-warning-check output remains: command runner rejected exact-path, workspace-verified cleanup with "blocked by policy" (no further reason supplied).
- GUI/user hand checks not run; no application launch other than --selftest, no quit/kill, no installed notes accessed. Existing selftests use isolated OS-temp fixtures; the new contract uses an isolated build fixture.

## Hand checks for the user
1. In the dev copy only, move disposable notes from test targetA to B; confirm the old folder becomes a sibling test targetA (moved ...), contains the original files, and Settings names that exact backup once.
2. Force a collision by precreating the timestamped sibling backup name, then move; confirm notes load/save in B, the old folder remains, and one sentence names its path and says the backup name already exists, with no kept-as claim.
3. Start with disposable app-home notes and move them; confirm Settings names the existing backup-before-move-* folder inside app home, and later edits save in the new folder.

## Manager verdict
Tier High. Gate PASS (SELFTEST PASS, CONTRACTS 7 rows, 6 pass, 1 manual; seam 0). Codex review: accept, no proven findings. Design review: accept-with-followup. Size WARN on SelfTest.cs (2637 -> 2763) accepted: the new contract needs its fixtures there.
Follow-ups (not blocking): A69-move-result-single-message (Settings rebuilds the success text instead of using the session message, and relies on a non-null OldCopy); C21-selftest-notes-move-split (move the notes-move checks out of SelfTest.cs); A68-notes-path-dedupe should add the KeepOldCopy comparison to its scope.
