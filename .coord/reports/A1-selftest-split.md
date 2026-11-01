# A1-selftest-split: report

Built by a Claude Haiku sub-session (Codex paused). Role: builder. Review by the reviewer sub-session (`/review-task`).

## Change list
- Deleted `product/AntiNotepad/SelfTest.cs` (3,247 lines).
- Added `product/AntiNotepad/SelfTest/` with ten files, all under 600 lines: `Run.cs` (131), `Layers.cs` (543), `Layers.Scan.cs` (225),
  `Platform.cs` (342), `Editor.cs` (473), `Lists.cs` (262), `Notes.cs` (442), `Store.cs` (496), `App.cs` (248), `UI.cs` (148).
- `SelfTest/Contracts.cs` unchanged.
- `docs/coding.md`: "Planned splits" row for `SelfTest.cs` marked done (points to `SelfTest/`).
- `AGENTS.md`: "Code layout" names `SelfTest/` (selftest checks by area).
- `.coord/reports/A1-selftest-split.baseline.txt` (selftest output before the change), this report.

## How the move was done
- A script (kept in the sub-session scratch folder, not committed) copied exact line ranges from the old file, 17-3246 in 35 ranges
  mapped by the card's member map. Each member's start line was confirmed by hand; a member's leading `//` lines moved with it
  (only one case: the two comment lines above `SettingsSetterAllowances`, which went into `Layers.cs`).
- Each file: the same 10-line using block, `namespace AntiNotepad;`, `internal static partial class SelfTest` and braces.
  `Run.cs` alone keeps the class `/// <summary>`. Member order inside each file is the old order.
- Unused `using` lines were removed with `dotnet format style --diagnostics IDE0005 --include product/AntiNotepad/SelfTest/`
  (compiler-accurate). Removed per file: App (`AntiNotepad.Model`, `Avalonia.Platform`, `System.Reflection`); Editor (`Model`,
  `Theme`, `UI`, `Avalonia.Controls`, `Avalonia`, `System.Reflection`, `System.Reflection.Emit`); Layers.Scan (`System.Reflection`,
  `System.Reflection.Emit`); Layers (`Model`, `Platform`, `UI`, `System.Diagnostics`, `System.Reflection`, `System.Reflection.Emit`);
  Lists (`Model`); Notes (`Model`, `Platform`); Platform (`Model`, `Platform`); Run (`Model`, `Theme`); Store (`Model`, `Platform`); UI (`Model`).
  The build has 0 warnings and 0 errors after this.
- Line endings: the working tree in this worktree is CRLF (core.autocrlf=true, blobs are LF). New files are CRLF to match
  `Contracts.cs`. Every line of each new file has CRLF.

## Checks
1. Gate: `tools/gate.ps1 -Task A1-selftest-split` → **GATE: PASS** (Build 0 warning(s), 0 error(s); Selftest PASS; LAYERS and
   CONTRACTS 13 rows, 12 pass, 1 manual; Seam PASS; Format PASS with no problems on changed lines, nothing fixed by hand;
   Scope PASS, 13 changed files all allowed; LoopLint PASS; Size PASS). Gate report: `.coord/reports/A1-selftest-split.gate.md`.
2. `--selftest` output vs baseline: exit 0, `SELFTEST PASS`. Same 144 lines except one: the line
   `A18-search-speed timing: ...` carries wall-clock milliseconds. A second run of the same build changed it again
   (29.807 / 30.318 / 29.433 ms), so this is measurement noise, not a behavior change. All other lines are identical.
3. Sorted-lines check: the old file (`git show dev:product/AntiNotepad/SelfTest.cs`) and the ten new files, each with `using`,
   `namespace`, the class line, column-0 `{`/`}` and blank lines removed, sorted: 2,950 lines each, equal (`diff` silent).
   Trimmed-line comparison was not needed, since the format step found no problems on moved lines.

## Behavior changes
None expected and none observed: the code is the same text in new files; the selftest output matches apart from the timing line.

## Out-of-scope files
None. Changed: `product/AntiNotepad/SelfTest.cs` (deleted), the ten `SelfTest/` files, `AGENTS.md`, `docs/coding.md`,
`.coord/reports/A1-selftest-split.baseline.txt`, `.coord/reports/A1-selftest-split.md`, `.coord/tasks/A1-selftest-split.md`
(Build and Check sections).

## Noticed
- The card's Store.cs size estimate (500) is 496 in the file; other estimates were close.
- The `A18-search-speed` timing line in `--selftest` prints wall-clock values, so a byte diff of the selftest output never matches
  exactly across runs. A future output-diff check should skip that line.
- `build/A1-selftest-split-check` is a gitignored build output; it is removed after the review.

## Plan deviations
None. Every Plan step was done as written: ranges copied verbatim, ten files, wrapper per file, docs edits, gate, sorted-lines
check. Only the report heading was missing at first; added at review (round `A1-selftest-split`, no code change).
