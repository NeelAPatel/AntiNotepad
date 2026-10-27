# A43b-notes-session-r2: missing notes folder at startup must not crash (rework inside A43b-notes-session)

Role: worker (gpt-6-luna xhigh). **Implementation, small.** Product `product/AntiNotepad`. Branch `reorg/A43b-notes-session` (already
checked out; do not switch branches, do not commit: you cannot write to `.git`). Read `AGENTS.md` and `.coord/reports/A43b-notes-session.md`
first. Line numbers are from the current tree; re-read before editing.

## The bug (found in the user's hand test; the pre-A43b build 6b7b1f4 crashes identically, so it is existing, not caused by NotesSession)
Start with `location.json` in the app home pointing at a folder that does not exist (valid JSON, e.g.
`{"NotesFolder": "C:\\...\\MISSING\\"}`, run with `ANTINOTEPAD_DATA` = a throwaway folder). `Store.Load()` returns early with
`IsAvailable = false`, so `Store.Data` stays the default `new StoreData()`: `Stack` is empty and `Current` is -1. Then
`PadWindow..ctor -> BindPanes -> LeftNote` (`UI/PadWindow.Navigation.cs:17`, `Data.Stack[Data.Current]`) throws
`ArgumentOutOfRangeException` and the app dies at startup, so the read-only "Error: Notes folder not found ... Choose a folder in
Settings > Storage" hint in `PadView.Bind` is never reached.

## Intended behavior
With an unavailable store the app starts, shows the pad read-only with that hint (`PadView.Bind` already handles
`NotesLocationError`), Settings > Storage shows the error and "Choose folder", and nothing is written to or created in the missing
folder (saves already refuse while unavailable). Choosing a valid folder in Settings still moves/switches as today.

## What to do
1. Find how a normally loaded store guarantees a usable selection (see `Store.Normalize`, called near `Store.cs:577`: at least one stack note,
   `Current` in range). Give the **unavailable** path the same guarantee **in memory only**: after the store is marked unavailable /
   `Load` returns early, `Data.Stack` has one blank note and `Current` is valid. Prefer the smallest fix at the source (in `Store`,
   where both `MarkUnavailable` and the early `Load` return live) over guarding every reader; a guard in `LeftNote` alone is not
   enough unless you also verify every other reader of `Data.Stack[Data.Current]` / `Data.Current` (grep them) survives an unavailable store.
   Do not touch `NotesLocation`, `NotesSession`, `Program` bootstrap order, `Welcome`/`Showcase` seeding conditions, or settings policy.
2. The blank in-memory note must never reach disk: confirm `Save`/`SwitchFolder` paths while unavailable still refuse and that
   switching to a valid folder replaces `Data` with the loaded folder's data as it does today (a recovered `Data` must not inherit the
   placeholder note).
3. `SelfTest.cs`: add one capability line `UNAVAILABLE STORE startup` (print like the `NOTES SESSION move` line) covering, in a temp
   root: `new Store(<missing folder>, createDirectories: false)` + `Load()` gives `IsAvailable == false`, `Data.Stack.Count >= 1`,
   `Data.Current` in range, `Save()` fails or no-ops without creating the folder; and the same through the `MarkUnavailable(...)`
   constructor-fallback path used in `Program`. No UI/window construction in the selftest.
4. Keep all 89 existing `LAYERS` lines and every other selftest line unchanged.

## Rules
- Behavior-neutral for every available-store path. US spelling. Never launch or kill AntiNotepad (never `--quit`). Do not commit.
- Build: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A43b-r2-check` (0 warnings, 0 errors), then
  `build/A43b-r2-check/AntiNotepad.exe --selftest` must print `SELFTEST PASS` with all existing lines plus `UNAVAILABLE STORE startup`.
  Delete `build/A43b-r2-check` afterwards (if the tool refuses recursive removal, say so in the report).

## Done means
- Build 0 warnings 0 errors; `SELFTEST PASS`.
- Report `.coord/reports/A43b-notes-session-r2.md`: the root cause as you found it, files changed, why the placeholder cannot reach disk,
  the list of other readers of `Data.Current` / `Data.Stack[...]` you checked, selftest lines.
