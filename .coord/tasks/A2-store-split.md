# A2-store-split: split Model/Store.cs by job, verbatim

Item card (`docs/loop.md`). Codex builds (pause lifted by the user 2026-11-01), run by the builder sub-session.

Tier: Low · pure-move (rules: `docs/loop.md`; Store is a High-tier area, but nothing in it changes)
Branch: `refactor/A2-store-split` in `.claude/worktrees/refactor/A2-store-split` (created at Approve)

## Request
2026-11-01, user: "okay this idea is approved. start a2 d1 and a3 in parallel. during merge back, we'll merge a2 first
then a3 and finally d1". A2 = `idea: store-split` (Era 1 goal: clean up and standardize the app code).

## Findings
- **Kind and why:** process (cleanup). `product/AntiNotepad/Model/Store.cs` is 1,006 lines with four jobs: the search
  types (lines 6-129), `Note` (130-266), `Settings` (267-335), `StoreData` (336-353) and `Store` (354-1006), whose search
  members (`CreateSearchContext` about line 412 through `MakeSearchSnippet`) are a fifth job inside it.
- **Now:** one file, namespace `AntiNotepad.Model`; `Store` is `public sealed class` (not partial).
- **Where:** only `Model/Store.cs`. LAYERS rules match by namespace (`SelfTest/Layers.cs:45-47`), so no LAYERS edit while
  the namespace stays. No caller changes.
- **Risk:** a changed line in the move; `Store` save/load paths are High-tier code, so any edit beyond the move is out of
  scope.
- **Source:** master scout.

## Plan
- **After:** five files, all in namespace `AntiNotepad.Model` (folder is not namespace, so callers stay untouched):
  `Model/Search/SearchTypes.cs` (lines 6-129), `Model/Search/Store.Search.cs` (the `Store` search members, as
  `partial class Store`), `Model/Note.cs`, `Model/Settings.cs`, `Model/Store.cs` (`StoreData` and the rest of `Store`).
  The only edited line: `public sealed class Store` becomes `public sealed partial class Store`. Each file under 600 lines.
- **Docs:** none on this branch. A3-calc-split updates both `docs/coding.md` rows and `AGENTS.md` (user's merge order:
  A2, A3, D1), so the three parallel items share no files.
- **Expected time:** about 45-60 minutes (A1 took about 50 minutes with the same checks).
- **Must not change:** any behavior, any caller, namespaces, `SelfTest/`.
- **Scope questions:** the "over about 400 lines of product code means lettered steps" rule: this moves about 1,000
  lines verbatim; proposed as one run (the sorted-lines check proves no line changed). Approving says yes to that.
- **Test plan, code:** `--selftest` output equal to dev's (timing lines such as `A18-search-speed timing` excepted);
  sorted-lines check (old file vs new files, minus `using`, namespace, the `partial` class line and braces). No new
  `CONTRACTS` row.
- **Test plan, UI:** none (not visible).
- **Gates:** `tools/gate.ps1 -Task A2-store-split` (build, selftest, LAYERS, CONTRACTS, scope, size) plus the two checks
  above.
- **Needs from you:** approve.

## Files
- `product/AntiNotepad/Model/Store.cs`
- `product/AntiNotepad/Model/Note.cs`
- `product/AntiNotepad/Model/Settings.cs`
- `product/AntiNotepad/Model/Search/SearchTypes.cs`
- `product/AntiNotepad/Model/Search/Store.Search.cs`
- `.coord/tasks/A2-store-split.md`
- `.coord/reports/A2-store-split.md`
- `.coord/reports/A2-store-split.baseline.txt`

## Contracts
None touched; all rows keep passing.

## Approve
2026-11-01, user: "Approve all three" (plans as written: one run each for A2 and A3, not lettered; D1 pictures as SVG).

## For Codex
Role: quick (`docs/agents.md`), because this is a verbatim line-range move with one edited line. Product `product/AntiNotepad`. The
branch is already checked out: never switch, merge or commit (you cannot write to `.git`). Read `AGENTS.md`, then
`product/AntiNotepad/Model/Store.cs`. Line numbers are from the current tree; re-read before editing.

Constraints:
- Build exactly the Plan's "After" for the Files above: split `Model/Store.cs` into the five files. No drive-by refactors,
  renames or edits.
- Copy line ranges with a script (keep a copy of the original `Store.cs` outside the tree first), never retype or reflow.
  Each new file gets the original `using` lines it needs and `namespace AntiNotepad.Model;` in the same form the original
  uses; moved blocks stay byte-identical (including line endings and the file's BOM state).
- The only edited line: `public sealed class Store` becomes `public sealed partial class Store` (in both `Store.cs` and
  `Store.Search.cs`). The namespace stays `AntiNotepad.Model` in every file.
- Touch only the Files above. Do not edit `SelfTest/`, `docs/coding.md` or `AGENTS.md`.
- If the line ranges in Findings do not match the file, find the true boundaries; if the plan cannot work, stop and report.
- Build into `build/A2-store-split-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A2-store-split -Offline`; the manager
  re-runs it online.
- Never launch or kill AntiNotepad. Never run `--quit`. Never edit `.coord/STATUS.md` or this card.

Done means (offline checks only):
- Five files exist as in the Plan's "After"; each under 600 lines; `Model/Store.cs` no longer holds the moved types.
- Sorted lines of the old `Store.cs` equal the sorted lines of the five new files, ignoring `using`, `namespace`, class
  declaration lines, braces and blank lines.
- Build 0 warnings, 0 errors; `--selftest` prints `SELFTEST PASS`; the offline gate passes.

Report: `.coord/reports/A2-store-split.md`, short, with exactly these sections:
1. **Summary**: two or three sentences.
2. **Change list**: one row per new/changed file: `file:line-range | what moved | which Plan line it serves`.
3. **Behavior changes**: "None" only if true.
4. **Out-of-scope files**: file and why, or "None".
5. **Plan deviations**: anything done differently from the Plan, and why, or "None".
6. **Contracts**: rows added or changed, or "None".
7. **Lens review**: run the checklist yourself and say so.
8. **Checks run**: build, selftest, gate result lines.
9. **Hand checks for the user**: "None (not visible)".

## Build
Round 1, Codex `quick` (gpt-6-luna medium), run 2026-11-01 from the builder sub-session; log `.coord/logs/A2-store-split.quick.log`, report `.coord/reports/A2-store-split.md`. Commit c271437.

## Check
- Gate (online, `tools/gate.ps1 -Task A2-store-split`): GATE: PASS (Seam, Format, Scope 5 files, Size, selftest, LAYERS, CONTRACTS 13 rows / 12 pass / 1 manual).
- `--selftest` vs baseline (`.coord/reports/A2-store-split.baseline.txt`): identical apart from ms timing text; SELFTEST PASS; build 0 warnings, 0 errors.
- Sorted-lines check (`dev:Model/Store.cs` vs the five new files, minus using/namespace/Store class lines/braces/blanks): 670 = 670 lines, no difference.
- Only edited line: `public sealed class Store` to `public sealed partial class Store`; namespace stays `AntiNotepad.Model` in all files; each file under 600 lines (556 / 146 / 73 / 123 / 128).
- Bounds: Request (split Store, approved plan) vs Plan vs done: within plan. Behavior changes None; Out-of-scope files None; Plan deviations None. The search cache field moved with the search members: within plan (search job).
- Tier Low, so no Codex or design review. Contracts: none added. Docs untouched (A3 owns them).
- Noticed: none. Misses: None.
- Verdict: accepted.

## Hand test
Skipped: not visible (pure move).
