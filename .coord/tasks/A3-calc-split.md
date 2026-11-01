# A3-calc-split: split Calc/Calculator.cs by job, verbatim

Item card (`docs/loop.md`). Codex builds (pause lifted by the user 2026-11-01), run by the builder sub-session.

Tier: Low · pure-move (rules: `docs/loop.md`)
Branch: `refactor/A3-calc-split` in `.claude/worktrees/refactor/A3-calc-split` (created at Approve)

## Request
2026-11-01, user: "okay this idea is approved. start a2 d1 and a3 in parallel. during merge back, we'll merge a2 first
then a3 and finally d1". A3 = `idea: calc-split` (Era 1 goal: clean up and standardize the app code).

## Findings
- **Kind and why:** process (cleanup). `product/AntiNotepad/Calc/Calculator.cs` is 1,383 lines; its own section markers
  name the jobs: public types (1-22), units and words (27-159), dates (160-278), values and formatting (279-360), lexer
  (361-549), parser (550-944), line eval and public API (945-1136), the calc self test (1137-1383).
- **Now:** `public static class Calculator` (line 23, not partial), namespace `AntiNotepad.Calc`.
- **Where:** only `Calc/Calculator.cs`, plus the docs below. LAYERS rule for Calc is by namespace
  (`SelfTest/Layers.cs:47`), so no LAYERS edit.
- **Risk:** a changed line in the move; the shared `private static readonly` fields at the top must stay in one file.
- **Source:** master scout.

## Plan
- **After:** seven files in `Calc/`, namespace unchanged, `Calculator` made `public static partial class`:
  `Calculator.cs` (public types, shared fields, line eval and public API), `Calculator.Units.cs` (units, words),
  `Calculator.Dates.cs`, `Calculator.Values.cs` (values, formatting), `Calculator.Lexer.cs`, `Calculator.Parser.cs`,
  `Calculator.SelfTest.cs`. Each under 600 lines. Only edited line: the `partial` keyword.
- **Docs (for A2 and A3 both):** `docs/coding.md` "Planned splits": the `Model/Store.cs` and `Calc/Calculator.cs` rows
  marked done; `AGENTS.md` code layout names `Model/Search/` and the `Calc/` parts. Written after A2 merges (catch-up
  from dev first), since A2 merges first (user's order).
- **Expected time:** about 60 minutes, plus the wait for A2.
- **Must not change:** any behavior, results or formatting of calc output, namespaces, `SelfTest/`.
- **Scope questions:** the "over about 400 lines of product code means lettered steps" rule: this moves about 1,380
  lines verbatim; proposed as one run (the sorted-lines check proves no line changed). Approving says yes to that.
- **Test plan, code:** `--selftest` output equal to dev's (timing lines excepted), including the calc self test;
  sorted-lines check (old vs new, minus `using`, namespace, class line and braces). No new `CONTRACTS` row.
- **Test plan, UI:** none (not visible).
- **Gates:** `tools/gate.ps1 -Task A3-calc-split` plus the two checks above.
- **Needs from you:** approve.

## Files
- `product/AntiNotepad/Calc/Calculator.cs`
- `product/AntiNotepad/Calc/Calculator.Units.cs`
- `product/AntiNotepad/Calc/Calculator.Dates.cs`
- `product/AntiNotepad/Calc/Calculator.Values.cs`
- `product/AntiNotepad/Calc/Calculator.Lexer.cs`
- `product/AntiNotepad/Calc/Calculator.Parser.cs`
- `product/AntiNotepad/Calc/Calculator.SelfTest.cs`
- `docs/coding.md`
- `AGENTS.md`
- `.coord/tasks/A3-calc-split.md`
- `.coord/reports/A3-calc-split.md`
- `.coord/reports/A3-calc-split.baseline.txt`

## Contracts
None touched; all rows keep passing.

## Approve
2026-11-01, user: "Approve all three" (plans as written: one run each for A2 and A3, not lettered; D1 pictures as SVG).

## Build
2026-11-01, Codex `quick` (297s, exit 0; log in main `.coord/logs/A3-calc-split.quick.log`, report `.coord/reports/A3-calc-split.md`). Seven files in `Calc/`, all under 600 lines. Builder then ran `dotnet format whitespace` on the Dates, Lexer, Parser and SelfTest parts because the gate's Format step counts every line of a new file as changed. Catch-up with dev (A2-store-split, 268bb6f) clean, `RETEST: no`. Builder edited the `docs/coding.md` "Planned splits" rows (Store and Calculator, done) and the `AGENTS.md` code layout.

## For Codex
Role: quick (`docs/agents.md`), because this is a verbatim line-range move with one edited keyword. Product `product/AntiNotepad`.
The branch is already checked out: never switch, merge or commit (you cannot write to `.git`). Read `AGENTS.md`, then
`product/AntiNotepad/Calc/Calculator.cs`. Line numbers are from the current tree; re-read the section markers before cutting.

Constraints:
- Split `Calc/Calculator.cs` into the seven files under Plan "After" by copying line ranges with a script (PowerShell or
  Python), never by retyping. Each new part file gets the same `using`s it needs, `namespace AntiNotepad.Calc;` (or the
  existing namespace form), and `public static partial class Calculator { ... }` around its moved block.
- The only edited line is the `Calculator` class line, which becomes `public static partial class Calculator`. Shared
  `private static readonly` fields stay in `Calculator.cs`. Namespace unchanged. No other product code, no `SelfTest/`.
- Code files only in this run: do NOT edit `docs/coding.md` or `AGENTS.md` (the builder session does, after A2 merges).
- Each resulting file stays under 600 lines.
- Build into `build/A3-calc-split-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that folder.
- Run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A3-calc-split -Offline`.
- Never launch or kill AntiNotepad. Never run `--quit`. Never edit `.coord/STATUS.md` or this card.
- Compare against `.coord/reports/A3-calc-split.baseline.txt` (selftest before the change).

Done means (offline checks only):
- `Calc/` holds the seven files; `Calculator` is `public static partial class`; nothing else changed in the moved lines.
- Build 0 warnings; `--selftest` prints `SELFTEST PASS` with output equal to the baseline except wall-clock timing lines
  (including the calc self test); `tools/gate.ps1 -Task A3-calc-split -Offline` passes; every file under 600 lines.

Report: `.coord/reports/A3-calc-split.md`, short, with exactly these sections: 1. **Summary**; 2. **Change list**
(`file:line-range | what changed | Plan line`); 3. **Behavior changes** (None only if true); 4. **Out-of-scope files**;
5. **Plan deviations**; 6. **Contracts**; 7. **Lens review** (run the checklist yourself and say so); 8. **Checks run**;
9. **Hand checks for the user** (None, not visible).

## Check
Gate (final run after this commit): PASS, recorded in the review reply. Selftest equals the baseline except the A18 timing line.
- Seven-file split, namespace unchanged, `partial` keyword the only edited class line: within plan.
- Format pass on moved code (indentation fixes; a few multi-label `case` and dictionary lines split one per line): justified. The gate would otherwise fail, and a whitespace-stripped character count old vs new differs only by the six added `{ }` pairs. The sorted-lines check passed before the pass, not after (lines were split).
- Docs edits (`docs/coding.md`, `AGENTS.md`) by the builder, a few lines, after A2 landed: within plan.
- Behavior changes: None. Out-of-scope files: None.
Noticed: `tools/gate.ps1` Format treats every line of a new file as changed, so a pure move into new files can't pass without reformatting (proposal for the loop chat: baseline new files against the file they were split from). Codex's offline gate did not run Format.
Misses: found at online gate; cause env/gate design; caught by the online gate; the offline gate should have run Format; lesson: for splits, run `dotnet format whitespace` on the new parts before the gate.

## Hand test
Not visible: no hand test needed.
