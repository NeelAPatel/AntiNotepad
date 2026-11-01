# A3-calc-split report

## 1. Summary
Split `Calculator.cs` into the seven planned partial-class files. Product behavior and calculator class-body lines are unchanged.

## 2. Change list
- `Calc/Calculator.cs:23, 25-26, 945-1136` | made the class partial; retained shared fields and line evaluation/public API | Plan: `Calculator.cs` (public types, shared fields, line eval and public API)
- `Calc/Calculator.Units.cs:1-140` | moved units and words | Plan: `Calculator.Units.cs`
- `Calc/Calculator.Dates.cs:1-126` | moved dates | Plan: `Calculator.Dates.cs`
- `Calc/Calculator.Values.cs:1-89` | moved values and formatting | Plan: `Calculator.Values.cs`
- `Calc/Calculator.Lexer.cs:1-196` | moved lexer | Plan: `Calculator.Lexer.cs`
- `Calc/Calculator.Parser.cs:1-402` | moved parser | Plan: `Calculator.Parser.cs`
- `Calc/Calculator.SelfTest.cs:1-253` | moved calc self-test | Plan: `Calculator.SelfTest.cs`

## 3. Behavior changes
None.

## 4. Out-of-scope files
None changed.

## 5. Plan deviations
Builder (after the run): `tools/gate.ps1` Format treats every line of a new file as changed, so the moved code's older format problems failed the online gate. `dotnet format whitespace` was run on the Dates, Lexer, Parser and SelfTest parts (indentation fixes, and a few multi-label `case`/dictionary lines split one per line; the odd +6 line growth). No tokens changed: a whitespace-stripped character count of old vs new matches except the 6 added `{ }` pairs of the part files. The sorted-lines check passed before the format pass. Gate PASS after.
Original note: Documentation edits remain for the post-A2 catch-up as planned.

## 6. Contracts
None touched; all 13 contract rows retain their baseline result (12 pass, 1 manual).

## 7. Lens review
Ran the lens-review checklist on the full diff. Scope is limited to the approved moves and `partial` keyword; no public signature or shared field changed, no caller changes are needed, and no new layering or runtime coupling was introduced. No findings. Verdict: accept.

## 8. Checks run
- Sorted-lines check: PASS, 1,358 original class-body lines preserved exactly as a multiset.
- Release build to `build/A3-calc-split-check`: PASS, 0 warnings, 0 errors.
- `--selftest`: PASS; output matches `.coord/reports/A3-calc-split.baseline.txt` excluding timing lines, including calculator self-test.
- `tools/gate.ps1 -Task A3-calc-split -Offline`: PASS. Offline NuGet audit warning only; gate cleaned its task build/data folders.
- All seven calculator files are under 600 lines.

## 9. Hand checks for the user
None, not visible.
