# A52-move-old-folder-rename-warning-audit: read-only impact audit

Role: director (gpt-6.1-sol medium). Product `product/AntiNotepad`.
Work in `.claude/worktrees/A52-move-old-folder-rename-warning` (branch `feat/A52-move-old-folder-rename-warning`), read-only.
Never switch, merge or commit. Never launch or kill AntiNotepad; never touch the user's real notes or `%LOCALAPPDATA%`.

## What to audit
The brief `.coord/tasks/A52-move-old-folder-rename-warning.md`. Read it first. No edits outside `.coord/reports/`.
You may write a throwaway console probe or selftest copy under `build/A52-audit-probe` (delete it after) to reproduce
`Directory.Move` failing with "The parameter is incorrect" on a temp folder.

## Report: `.coord/reports/A52-move-old-folder-rename-warning-audit.md`
Short, tables where useful:
1. **Root cause**: most likely cause with evidence (`file:line`, probe output); other suspects ruled out and how.
2. **Files and tier**: confirm or correct the brief's Files list and the High tier.
3. **Callers and shared state**: who calls `NotesLocation.Move`, `KeepOldCopy`, `NotesSession.MoveNotes`; what holds the
   old folder open (store, watchers, search, recovery); what the Settings message code reads.
4. **Risks**: riskiest parts of the fix, and what must not change.
5. **Hand checks** for the user in the dev copy.
6. **Recommendation on handling**: scope as written, narrower or split; anything to add to the brief.
