# A21-save-delay-setting-audit: findings for A21-save-delay-setting

Role: director (gpt-6.1-sol medium), read-only. Product `product/AntiNotepad`. Main checkout on `dev`: never switch, merge, commit or write files. Never launch or kill AntiNotepad.
Read `AGENTS.md` and the card `.coord/tasks/A21-save-delay-setting.md` (Request). Follow `.coord/templates/findings.md` sections 1-7.

## What the item chat already found (verify, correct, do not just repeat)
- `UI/PadWindow.cs:30` `_saveTimer` fixed 400 ms; `ScheduleSave()` (:386) stops/starts it; every `Edited()` calls it; Tick -> `QueueSave()`.
- Immediate saves: `Deactivated` (:100), `HideResident` (:244), `Quit`/`Save()`, `PrepareForSessionEnd` (:266, sign-out/shutdown). `Navigate` etc. only call `Edited()` (restart timer), no save.
- `Model/Store.cs` `QueueSave` adds a 50 ms coalesce window; `ApplyCompletedSaves` clears `Note.Dirty`.
- "Saved <ago>" label (`UI/PadWindow.Layout.cs:275`) uses `Note.Modified`, which `Note.Touched` sets on every edit, not at save time.

## Extra questions
A. Complete list of paths that save or must save with a pending edit (hide, deactivate, quit, session end, note switch, new note, folder change, settings window open, Windows shutdown/sign-out, crash). Which lack a flush today and would matter at 5/10/30 s?
B. Is `ApplyCompletedSaves` reached after the async `QueueSave` path, so `Dirty` clears? Where would a `SavedAt` time (set when a body is durable) be set, and when does the label refresh?
C. Who else reads `Note.Modified` (sort, search, persistence format)? Is it safe to leave it as "last edit"?
D. Any other timer or debounce on the save path (settings file save uses ScheduleSave too: should it follow the new delay)?
E. Existing selftests for Store save/flush that a new check can sit beside (file, helper).
F. Tier: confirm High.

End with `READY: yes` or `READY: no - <what is missing>`.
