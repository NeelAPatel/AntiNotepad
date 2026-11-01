# A1-selftest-split: split SelfTest.cs into a SelfTest/ folder by area, verbatim

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build. Codex is paused; the
builder sub-session implements (Haiku, user 2026-11-01). Check and Hand test are filled by the sub-session afterwards.

Tier: Low · pure-move (rules: `docs/loop.md`)
Branch: `refactor/A1-selftest-split` in `.claude/worktrees/refactor/A1-selftest-split` (created at Approve)

## Request
2026-11-01, user: "Start A1, replace codex subagents with haiku" (A1 = `idea: selftest-split`, proposed by Master 2 on
2026-10-30: split `SelfTest.cs` into a `SelfTest/` folder by area, verbatim). Era 1 goal (user): clean up the app's code
and standardize things.

## Findings
- **Kind and why:** process (cleanup). `product/AntiNotepad/SelfTest.cs` is 3,247 lines holding about 50 independent
  checks plus the LAYERS rules; `docs/coding.md` "Planned splits" names it first, and every later split (store, calc)
  edits the LAYERS rows inside it.
- **Now:** one file, `internal static partial class SelfTest` (SelfTest.cs:15); the only other part is
  `SelfTest/Contracts.cs`. `Run()` (lines 17-139) calls every check. No `Compile` items in the csproj (SDK globbing), no
  enforced code-style analyzers.
- **Where:** only `SelfTest.cs` moves. Live references to the path: `docs/coding.md:78` (splits table),
  `.coord/templates/item.md:34`, `tools/catch-up.ps1:12` and `tools/files-overlap.ps1:11` (hot-file lists; both also list
  `SelfTest/*.cs`, so they keep working with no edit).
- **Risk:** a line changed during the move (caught by the sorted-lines check); the gate's format step reads every moved
  line as changed, so old format problems may surface.
- **Source:** master scout.

Member map (start lines; each runs to the next):
| New file | Members (SelfTest.cs lines) | About |
|---|---|---|
| `SelfTest/Run.cs` | `Run` 17-139 | 125 |
| `SelfTest/Layers.cs` | layer records 333-360; `CreateLayerRules` through `IsExactOrNested` 982-1485 (incl. DesktopPlatformSurface, PInvokeHolder, StaticServiceExposure, EnvironmentReadConfinement) | 550 |
| `SelfTest/Layers.Scan.cs` | `InNamespace` through `HasBytes` 1579-1796 (IL reference scan) | 230 |
| `SelfTest/Platform.cs` | hotkey fakes 361-399, `HotkeySwapChecks`, `ClipboardPolicyChecks`, `TaskbarTraceChecks`, `TaskbarChecks`, `SessionEndCloseChecks` | 350 |
| `SelfTest/Editor.cs` | `ShortenedLinkChecks`, `SplitDividerFullHeightChecks`, `SplitPaneClippingChecks`, `MathNotesChecks`, `FenceCacheChecks`, `IncrementalAnalysisChecks` | 475 |
| `SelfTest/Lists.cs` | `ListEditingChecks`, `ListCommandsChecks`, `CheckboxIndentChecks` | 270 |
| `SelfTest/Notes.cs` | `NotesLocationChecks`, `NotesMoveChecks`, `NotesMoveRenameChecks`, `PathEquals`, `NotesUseFolderChecks` | 450 |
| `SelfTest/Store.cs` | `DefaultDataMigrationSelfTest`, `InputLimitChecks`, `UnavailableStoreStartupChecks`, `SearchSpeedChecks`, `BackgroundSaveAndLogChecks` | 500 |
| `SelfTest/App.cs` | `RuntimeLogChecks`, `AppInfoSelfTest`, `StartupRequestLatchChecks`, `WelcomeSlotsSelfTest`, `DevBadgeBuildStampChecks`, `DevIdentitySelfTest` | 255 |
| `SelfTest/UI.cs` | `SettingsChangedChecks`, `SettingsSearchMatchingChecks`, `NoteSwipeChecks` | 155 |

## Plan
- **After:** `SelfTest.cs` is gone; the ten files above plus `Contracts.cs` make up `SelfTest`, each under 600 lines,
  each with only the `using` lines it needs and the same namespace and class line. Method bodies, order within a file
  and comments are byte-for-byte the old ones. `docs/coding.md` marks the row done; `AGENTS.md` code layout names
  `SelfTest/`.
- **How:** the builder moves the code with a script that copies line ranges (no retyping), then fixes `using` lines by
  compiling.
- **Expected time:** about 45-60 minutes request to merge (no Haiku pure-move rows in `usage.tsv` yet; Loop items of
  similar size took 30-90 min).
- **Must not change:** any check's logic or output; the selftest output; any file outside Files.
- **Scope questions:** none open (see "Needs from you").
- **Test plan, code:** `--selftest` output identical to dev's (same lines, same pass count; diffed); sorted-lines check:
  the old file's lines sorted equal the new files' lines sorted, apart from `using`, `namespace`, class and brace lines.
  No new `CONTRACTS` row (no bug, no new promise).
- **Test plan, UI:** none (not visible).
- **Gates:** `tools/gate.ps1 -Task A1-selftest-split` (build 0 warnings and selftest catch a broken move; LAYERS and
  CONTRACTS catch a dropped check; scope catches files outside the list; size catches a file over 600); plus the two
  checks above, which prove the move is verbatim.
- **Needs from you:** approve. If the format step flags old problems on moved lines, the builder fixes whitespace only
  (the sorted-lines check then compares trimmed lines) and lists them in the report.

## Files
- `product/AntiNotepad/SelfTest.cs` (deleted)
- `product/AntiNotepad/SelfTest/Run.cs`
- `product/AntiNotepad/SelfTest/Layers.cs`
- `product/AntiNotepad/SelfTest/Layers.Scan.cs`
- `product/AntiNotepad/SelfTest/Platform.cs`
- `product/AntiNotepad/SelfTest/Editor.cs`
- `product/AntiNotepad/SelfTest/Lists.cs`
- `product/AntiNotepad/SelfTest/Notes.cs`
- `product/AntiNotepad/SelfTest/Store.cs`
- `product/AntiNotepad/SelfTest/App.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `AGENTS.md`
- `docs/coding.md`
- `.coord/tasks/A1-selftest-split.md`
- `.coord/reports/A1-selftest-split.md`

## Contracts
None touched; all rows in `product/AntiNotepad/SelfTest/Contracts.cs` keep passing.

## Approve
2026-11-01, user: "approved" (plan as written; builder on Haiku).

## Build
2026-11-01, builder (Claude Haiku sub-session, Codex paused): split done as planned, verbatim (sorted-lines check equal, 2,950
lines each). Gate PASS; selftest PASS, same output as the baseline except the timing line in `A18-search-speed`. Report:
`.coord/reports/A1-selftest-split.md`.

## Check
- Gate: PASS (build 0 warnings, 0 errors; selftest PASS; LAYERS and CONTRACTS 12 pass, 1 manual; format: no problems on
  changed lines; scope: all changed files allowed; loop-lint PASS; size PASS). Report `.coord/reports/A1-selftest-split.gate.md`.
- Codex review: not required (Low). Design review: not required (Low). Contracts: none touched; none to confirm.
- Within plan: the ten `SelfTest/` files, deletion of `SelfTest.cs`, `docs/coding.md` row, `AGENTS.md` code layout line.
- Justified: `.coord/reports/A1-selftest-split.baseline.txt` is not in Files, but the Build step 1 requires it. Nothing for the user to decide.
- Justified: the `A18-search-speed timing` line of `--selftest` differs between runs (wall-clock ms; three runs of the same build
  differ). Not a behavior change; all other 143 output lines match the baseline.
- Within plan, added at review: the report lacked the `## Plan deviations` heading (step 1 check). Heading added with the word
  "None"; no code change.
- Comments and formatting follow `docs/coding.md`: comments moved verbatim; no format problems.
- Noticed (proposal, one line to the master): make the selftest output diff skip the timing lines, as `idea:` in STATUS.
- Result: accepted.
- Misses:
  - found at: review step 1 (report check); cause: report heading missing from the build report; caught by: review step 1;
    should have been caught by: builder self-check against the report sections before commit; lesson: fill the report from the
    template's headings before committing.

## Hand test
Not visible (pure move, no UI change): no hand test needed.
