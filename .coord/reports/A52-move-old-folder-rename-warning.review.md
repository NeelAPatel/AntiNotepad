# A52-move-old-folder-rename-warning lens review

## Findings (proven)
None.

## Behavior changes check
Report's list vs the diff: matches.

- `NotesLocation.Move` callers: `Resolve`, `NotesSession.MoveNotes`, and the new selftest. Failed moves still return an error; successful moves always carry `OldCopy`. `Resolve` still forwards only a retention warning. Copy, verification, pointer writing, rollback, and move refusals are unchanged.
- `NotesSession.MoveNotes` callers: Settings and all five selftest call sites are updated. Preflight failures leave the new out result null; move failures return their result without switching the store. Successful moves preserve the store-switch order, availability state, and retention-warning logging.
- `KeepOldCopy` and `BackupAppHomeData` are called only by the move/retention path. Parent and leaf now derive from the same trimmed source, app-home comparison trims both paths, and retention results supply the exact backup or a plain failure category. App-home backup operations are preserved.
- Settings consumes the successful result once, removes backup discovery and duplicate counting, and reads availability explicitly. Removed helpers have no remaining callers.
- No new timers, subscriptions, deferred work, shared mutable state, or upward layer dependencies. The new contract covers trimmed and trailing source forms, content preservation, app-home equivalence, collision retention failure, pointer/session switching, and subsequent saves.

## Scope check
In scope. All five changed product files are allowed by the brief; no unrelated hunks. `git diff dev...HEAD -- product` is empty; implementation changes are in `git diff -- product`.

## Unproven risks
- Live Settings rendering and OS-level access-denied/in-use retention failures were not exercised; the automated failure case uses a backup-name collision.

## Checks run
- Read the brief, implementer report, gate report, skill, changed hunks and surrounding code; searched callers and result consumers.
- Fresh Debug build into `build/A52-move-old-folder-rename-warning-review`, with `AVALONIA_TELEMETRY_OPTOUT=1` and `NuGetAudit=false`: build succeeded, 0 warnings, 0 errors.
- Review executable `--selftest`: exit 0; `CONTRACTS: 7 rows, 6 pass, 1 manual`; `SELFTEST PASS`; layer checks passed.
- `tools/check-platform-seam.ps1`: exit 0; `PLATFORM-SEAM source guard: 0 violations in 49 files`.
- `git diff --check`: passed.
- Cleanup attempted after verifying the exact absolute review output path was inside the workspace. Command runner rejected `Remove-Item` with `blocked by policy`, so `build/A52-move-old-folder-rename-warning-review` remains for manager cleanup. No product source edits, branch changes, commits, GUI launches, or installed-copy operations.
- STATUS left unchanged under `docs/loop.md`: feature branches never edit STATUS; the manager maintains it on dev.

VERDICT: accept
