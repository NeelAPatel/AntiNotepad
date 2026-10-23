# A9-input-limits-and-timers

You are a worker (gpt-6-luna xhigh). Repo: this folder, a git worktree on branch `feat/A9-input-limits-and-timers`. Never switch
branch, merge or commit (.git is read-only for you). App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10).
Read first: `AGENTS.md`, `docs/audit/A1-codebase-audit.md` (A1-3, A1-4, A1-5, A1-6, A1-16, A1-19, decisions 3, 5, 6). Match the
surrounding style. First-party only. Keep every change minimal.

## Approved decisions to implement
1. **List numbers (A1-4).** A numbered marker with more than 9 digits stops auto-numbering: Enter and Tab behave as on a plain line,
   and the renumber pass leaves it as it is. This covers both `int.Parse` sites in `Editor/PadView.cs` (~:597 renumber, ~:631
   continuation). Use `[0-9]` instead of `\d` in that regex. No exception can escape.
2. **Timers (A1-5).**
   - Only ASCII digits (`[0-9]`, not `\d`) in `Model/Timers.cs`.
   - Use invariant `TryParse`.
   - A duration above **99 days** or non-finite is **not a timer**: `TryParse` returns false.
   - Start and toggle must not overflow `DateTime.Now + duration`.
3. **Calculator depth (A1-6).** Nesting deeper than **64** (parentheses and chained unary signs) makes that line a normal
   "too complex" invalid result: whatever the calculator shows for an invalid line, plus that message if it has a message slot.
   No recursion beyond the limit, and no stack overflow. `Calc/Calculator.cs`.
4. **Mixed currencies (A1-19).** Adding or subtracting two amounts with **different explicit currencies** (for example `$5 + €3`) is
   an error, not a total. Check comparisons and aggregates (sum, total, average, and so on) that use the same add path. A plain
   number plus money keeps today's behavior. Same currency is unchanged.
5. **Preview timers stop with Settings (A1-3).** The Settings preview (`UI/MotionPreview.cs` creates its own `new Timers()`) must
   stop its timer and drop the PadView tick subscription when the preview is released, so a closed Settings window is not kept
   alive. Only the preview's own `Timers` stops: the resident pad's shared timers never do.
6. **Timer identity and pruning (A1-16).**
   - A running timer keeps running when only its **label** text is edited, so the key must not include the label. Base it on
     note id, the timer's duration text and its ordinal among the timer lines.
   - A timer whose line is deleted, or whose note is deleted, **stops and is removed**.
   - For deleted notes, check live note ids, for example by having `Timers` ask a `Func<string, bool> isLive` or prune against the
     note's current timer keys after each analysis. Don't edit `UI/PadWindow.Navigation.cs`, which another worker owns.
   - Completed or stale entries are pruned, so `_entries` can't grow without bound.

## Files in scope
`Editor/PadView.cs` (list and timer code only), `Model/Timers.cs`, `Calc/Calculator.cs`, `UI/MotionPreview.cs`, `UI/PadWindow.cs` (at most
the one line that constructs or wires the shared `Timers`, if needed), `SelfTest.cs` (add a method `InputLimitChecks` at the END
with one call line in Run). Other workers are editing PadView's hint and bind code, Store, Settings, PadWindow's quit and the OCR
code in parallel: keep your PadView edits local.

## Selftest
- a 12-digit list marker doesn't throw and doesn't continue;
- `999999999999999999999h` and Arabic-Indic digits are not timers;
- 100 days is not a timer and 99 days is;
- 65-deep parentheses give an invalid result, not a crash;
- `$5 + €3` is an error, while `$5 + $3` = `$8` and `$5 + 3` keeps today's result;
- the timer key is unchanged when the label changes;
- a removed timer line drops its entry.

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a9-check`: 0 errors, 0 warnings. `--selftest`
  PASS. Delete `build/a9-check` if you can. Never launch or kill AntiNotepad.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A9-input-limits-and-timers.md`: per finding, what changed and where; the timer key design; the
  build and selftest.
