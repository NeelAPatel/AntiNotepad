# A27-recovered-folder-always-open: the Recovered notes Open button always works

Item card (`docs/loop.md`). Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB.

Tier: Medium · notes-folder write (was Low)
State: hand-tested
Branch: `fix/A27-recovered-folder-always-open` in `.claude/worktrees/fix/A27-recovered-folder-always-open`

## Request
Master 6, 2026-11-18, on the user's go (first noted by the user 2026-10-27, old A51): the "Recovered notes" row in Settings always has a working Open button, and the recovered folder always exists, even when empty. Done: folder exists when the row shows; Open works on an empty folder; row text still says Empty / N files; selftest and gate pass.

## Findings
Own scout, dev 8056d53; the note still applies.
- **Now:** `UI/SettingsWindow.cs` (~607-628) returns the Open button only when `recoveredCount is > 0`. The folder is created only in `Store.RemoveOrphans`.
- **Trap:** `Directory.CreateDirectory` is recursive, so a blind create under a missing notes folder would recreate it. Ensure only when the notes folder exists.

## Plan
- **After:** `NotesLocation.EnsureRecovered(dataFolder)` creates `<dataFolder>\recovered` only if `dataFolder` is an existing directory; returns whether the folder exists; catches IO errors (false). Settings calls it when the row is built, and the Open button always shows. Text stays Empty / N files / Unavailable.
- **Must not change:** `Store.cs`, `Note.cs`, `PadWindow*.cs` (A21b); no startup write; row layout.
- **Test plan:** selftest case in `SelfTest/Notes.cs`: creates in an existing folder, idempotent, creates nothing (parent stays absent) when the notes folder is missing. Hand: slot Settings > Storage shows Open beside Empty; click opens the folder.
- **Gates:** gate (build, selftest, format, tokens) + Codex review. Needs from you: the one hand check.

## Files
- `product/AntiNotepad/Platform/NotesLocation.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/SelfTest/Notes.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs` (the contract row)
- `product/AntiNotepad/SelfTest/Design.cs` (design hash: the Open button now shows in Storage; proved by a dump diff with only added button lines)

## Contracts
- Add one `CONTRACTS` row for the ensure rule if the file has a natural place; keep existing rows passing.

## Approve
User, 2026-11-18: "Approve"

## For Codex
Role: terra, because it reasons across three files with one guard rule. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then the Files; re-read before editing.
- Build exactly the Plan's "After" for the Files above; no drive-by changes. If the Findings are wrong, stop and report that.
- Build into `build/A27-recovered-folder-always-open-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, delete the folder. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A27-recovered-folder-always-open -Offline`.
- Never launch or kill AntiNotepad, never run `--quit`, never edit `.coord/STATUS.md` or this card. Offline checks only.
- Done means: the Open button is returned unconditionally; the ensure rule and its selftest exist and pass; build 0 warnings, `SELFTEST PASS`, gate passes.

Report `.coord/reports/A27-recovered-folder-always-open.md`, short: 1 Summary; 2 Change list (`file:lines | what | Plan line`); 3 Behavior changes; 4 Out-of-scope files; 5 Plan deviations; 6 Contracts; 7 Lens review; 8 Checks run; 9 Hand checks for the user.

## Check
Item chat, 2026-11-18. Gate (-Offline): PASS after round 1 (selftest design-hash fix below). Codex lens review: accept, no proven findings.
- **Within plan:** `EnsureRecovered`, always-on Open, selftest cases, contract row `recovered-folder-ensure`.
- **Justified:** `SelfTest/Design.cs` hash (the Storage page now has an Open button when empty). Proof: a dump from the same code with only that line reverted hashes to dev's old value; the diff against the new dump is added Open-button lines in every theme, nothing else. `Contracts.cs` was in the Plan text, now in Files.
- **Noticed:** none. **Misses:** card Files list lacked `Design.cs` (found at gate, cause: design snapshot covers Settings pages; should have caught: Plan; lesson: Settings row changes list `Design.cs`).

## Hand test
User, 2026-11-18 (Blue slot, test data): saw Open beside Empty (screenshot); "yes it opens the correct folders". Build checked: recovered folder exists on disk.
