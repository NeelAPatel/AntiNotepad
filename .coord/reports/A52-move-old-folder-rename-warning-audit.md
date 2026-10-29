# A52-move-old-folder-rename-warning-audit

Read-only impact audit, 2026-10-29. Branch confirmed: `feat/A52-move-old-folder-rename-warning`. Product references below are relative to `product/AntiNotepad/`. No product edits, app launches, installed-data access, switches, merges or commits. STATUS left unchanged under this audit's reports-only rule.

## 1. Root cause

**Most likely cause: a trailing directory separator makes the backup destination a child of the source.** `Platform/NotesLocation.cs:86` uses `Path.GetFullPath`, which preserves a trailing separator. `KeepOldCopy` derives the parent from that untrimmed path at line 212, but trims only when deriving the leaf at line 213. For `P\test targetA\`, the destination becomes `P\test targetA\test targetA (moved …)` instead of its intended sibling. `Directory.Move` at line 218 then fails with the reported error.

Settings receives the picker URI's `Path.LocalPath` and calls only `GetFullPath` (`UI/SettingsWindow.cs:669-672`); the pointer and session retain this form (`Platform/NotesLocation.cs:68`, `NotesSession.cs:40`). That is a plausible entry route. The original user's picker result was not captured, so its exact path form remains unverified.

Console probe linked the **unchanged production `NotesLocation.cs`**, with only an `AppInfo.DataFolderName` stub; .NET SDK 10.0.403, runtime 10.0.14, Windows build 26340. Fixture data lived entirely under `build/A52-audit-probe`. Results:

| Input / condition | Production `Move` result | Direct rename evidence |
|---|---|---|
| Plain absolute path containing spaces | Success; old absent; backup exists; pointer correct | No warning |
| Trailing `\` | Success; old remains; backup absent; pointer correct | `IOException`, `0x80070057`, `The parameter is incorrect.` |
| Trailing `/` | Same failure | Same exception and HRESULT |
| Relative path, no trailing separator | Success; backup exists | No warning |
| 368-character source path | Success; backup exists | No warning |
| Process current directory = source, no trailing separator | Move succeeds; rename fails | `0x80070020`, file in use |
| Source index held open with `FileShare.Read` | Move succeeds; rename fails | `0x80070005`, access denied |
| Trim ending separator **before** deriving parent | Old absent; sibling backup exists | Rename succeeds |

The trailing-separator failures required **no store, watcher or open source handle**. Locks and working directory are therefore unnecessary to reproduce this incident; the tested lock cases have different errors. Spaces, ordinary relative input and the tested long path are ruled out as independent causes, not every possible filesystem/path restriction. No product reference to `FileSystemWatcher`, `CurrentDirectory` or `SetCurrentDirectory` was found. External locks remain legitimate failure cases that must still produce a truthful message.

## 2. Files and tier

**High is correct:** the fix touches the notes folder, move outcome and session state (`docs/loop.md`, Tiers). Keep the listed five-file scope; no additional production files are needed.

| Brief file | Expected responsibility |
|---|---|
| `Platform/NotesLocation.cs` | Local, root-preserving separator handling in `KeepOldCopy`; return actual retention outcome/path and a user-safe failure reason. |
| `NotesSession.cs` | Carry that outcome to Settings while retaining current save/switch/state behavior and diagnostic logging. |
| `UI/SettingsWindow.cs` | Use the reported outcome instead of guessing from directory differences; render one accurate result. |
| `SelfTest.cs` | Reproduction and focused success/failure checks with isolated folders. |
| `SelfTest/Contracts.cs` | Register `notes-move-renames-old-folder`; retain the four named existing contracts. |

Out-of-scope product files: none proposed. Audit-only throwaway probe was permitted. Its fixture folders were deleted by the probe's `finally` block, but the source/build artifacts remain under `build/A52-audit-probe`: automatic approval review rejected both recursive cleanup and a narrower file-by-file cleanup as “blocked by policy.” Manager cleanup is required. No full product build, selftest or implementation gate was run; console evidence does not establish live GUI behavior.

## 3. Callers and shared state

| Function / owner | Callers, state and lifetime |
|---|---|
| `NotesLocation.Move` | Production: first-run migration from `Resolve` (`Platform/NotesLocation.cs:36`) and `NotesSession.MoveNotes` (`NotesSession.cs:38`). Existing selftests exercise both routes indirectly. |
| `KeepOldCopy` | Sole caller: `Move`, **after** verified copying and `WritePointer` (`Platform/NotesLocation.cs:132-134`). Non-app-home rename failure is caught and returned as a warning; the move stays successful. App-home delegates to `BackupAppHomeData` (line 211). |
| `NotesSession.MoveNotes` | Production: Settings (`UI/SettingsWindow.cs:699`); tests: `SelfTest.cs:1699,1719,1728,1743`. Saves first, then moves, then switches the existing Store and updates `DataFolder`/availability; warning is also logged (`NotesSession.cs:24-47`). |
| Store / save writer | Keeps path strings, model, settings service and tasks, not a persistent directory stream (`Model/Store.cs:361-392`). Synchronous `Save` waits for its write completion (`688-700`); `SwitchFolder` drains saves, changes paths and clears search cache (`975-988`). Preserve Store/Data/SettingsService identity. |
| Search | Captures the old body path/cache epoch (`Model/Store.cs:411-414`), performs background searches (`UI/SearchOverlay.cs:118-125`), reads through bounded `File.ReadAllText` (`Model/Store.cs:477`). A read can transiently overlap a move; there is no persistent folder handle. Switching invalidates cache entries, not already captured path strings (`506-512,988`). |
| Recovery / enumerators | Orphan recovery drains saves, materializes enumeration via `ToList`, then moves files (`Model/Store.cs:653-669`). Move data enumeration is also materialized (`Platform/NotesLocation.cs:155-171`). Settings recovery counts are synchronous (`UI/SettingsWindow.cs:633-639,736-739`). No persistent recovery watcher found. |
| Settings result | Reads session paths, independently counts note data, snapshots a presumed backup parent, then searches for a new matching directory (`UI/SettingsWindow.cs:694-711,741-776`). `NewBackupDirectory` returns only a leaf name; on failure line 710 substitutes the original leaf and still claims “kept as.” Line 711 classifies the session string using a `StartsWith` test. Storage renders success and notice separately (`605-608`). |

Settings' `backupRoot` repeats the untrimmed `GetDirectoryName` issue (line 697). Fixing only the platform rename leaves Settings scanning the vanished old folder and falling back to an incorrect backup name. An explicit result avoids this second failure and directory-scan races.

## 4. Risks and invariants

- Keep the copy/hash/count verification, target conflict checks, cleanup/rollback and pointer-write order unchanged (`Platform/NotesLocation.cs:83-145`). A retention failure after pointer commit must remain a successful migration; never delete the valid target or switch the session back.
- Derive parent and leaf from one locally normalized source path. Preserve roots with `Path.TrimEndingDirectorySeparator`; consider equivalent trailing forms in the app-home comparison so app home cannot accidentally enter the rename branch. Leave `BackupAppHomeData` behavior intact. Do not expand into A68-notes-path-dedupe or relax same/nested/index refusals.
- Report the actual retained path for both ordinary rename and app-home backup. Preserve the full exception in diagnostics if useful, but map rename failures to plain reasons (name collision, permission/access, in use, other failure) for the UI. Do not promise a specific cause when the error code is ambiguous.
- Preserve save-before-copy, object identity, post-move edit saving, unavailable-store handling and cache invalidation. No Store disposal, watcher shutdown or new search lifecycle machinery is justified by this reproduction.
- A single failure sentence must replace the contradictory success/notice pair, name the unchanged old path, and make clear the notes moved. Successful retention must name its actual destination. Avoid using string prefixes or filesystem guesses as outcome data.

## 5. Hand checks in the dev copy

User runs these with throwaway folders only, through the documented dev instance:

1. Move out of initial dev app home; confirm the app-home backup still works and its displayed path exists.
2. Pick `test targetA`, migrate there, then pick sibling `B` and migrate again. Confirm `test targetA` disappears, its timestamped sibling contains the old files, and Settings names that exact backup once. Repeat with folder-picker paths ending in a separator where reproducible.
3. Edit/save after the second move; verify `B` changes, the backup does not, and restarting the dev copy follows its pointer to `B` with the edit intact.
4. Force a backup-name collision or hold a disposable source file open without delete sharing. Confirm notes still move, the original old folder remains, and Settings shows one plain sentence with that path and a useful reason, without raw exception text or a false backup claim.
5. Try same-folder, nested-folder and target-with-`index.json` choices. Confirm refusals and unchanged note content/location. Open search and recovered-folder details after moving to check they use the new location.

## 6. Recommendation on handling

**One task, narrower implementation within the existing scope; no split needed.** Add the trailing-separator cause and explicit retention outcome requirement to the brief. Replace the mandatory open-store failure reproduction: an open Store is not the demonstrated cause. Reproduce with a **non-app-home source ending in `\` and `/`**; keeping the Store alive is a useful integration condition, not the trigger. Existing `NotesMoveChecks` begins with `AppHome == source` (`SelfTest.cs:1698`), so its successful initial move never exercises this rename branch.

Require the new contract to cover trimmed and trailing forms, a real non-app-home rename, retained file contents, a deterministic retention failure (e.g. backup-name collision), successful pointer/session switching on that failure, and the displayed outcome data. Retain the existing post-move identity/save and refusal checks. Check app-home equivalence with a trailing separator without changing its backup algorithm. The implementer should run the brief's build/selftest/offline gate and lens review; the manager then applies the High-tier fresh review, design review and user hand-test gates.
