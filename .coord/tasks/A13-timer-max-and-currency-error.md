# A13-timer-max-and-currency-error

You are a worker (gpt-6-luna xhigh). Repo: this folder, a git worktree on branch `feat/A13-timer-max-and-currency-error`. Never switch
branch, merge or commit (.git is read-only for you). App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10). Read `AGENTS.md`.
Context: A9-input-limits-and-timers added a 99-day timer limit and made mixed currencies an error. The user tested them. Match the
surrounding style. First-party only.

## 1. Timer maximum is 999h 59m 59s
User: "`999h59m59s` should be the maximum allowed."
- In `Model/Timers.cs`, the limit becomes **999 h 59 m 59 s** (3,599,999 seconds), not 99 days. `999h59m59s` and `999:59:59` are
  valid timers; `1000h`, `999h60m` (it totals more than the max) and anything longer are not.
- The clock form needs up to 3 hour digits: `(\d{1,2}):` becomes `([0-9]{1,3}):`.
- The remaining time must display correctly for long timers: check `Timers.Format` for anything above 99 h.

## 2. Mixed currencies show an error
User test: in math mode `$5 + €3 =` shows nothing, while `$5 + $3` shows `$8`. The approved behavior is "show an error instead of
a total".
- In math mode, and in inline `=` lines in plain notes, a line that fails **only** because of mixed currencies shows a small
  error chip reading `mixed currencies`, in the palette's `Error` color, where the result chip would be.
- Other invalid lines stay silent, as today. Find how `LineEval` / `CalcResult` and `ChipFor` (in `Editor/PadView.cs`) and
  `Editor/Renderers.cs` carry and draw results, and add the smallest way to carry that one error reason.
- The chip is not clickable to copy (or copies nothing).

## Files in scope
`Model/Timers.cs`, `Calc/Calculator.cs`, `Editor/PadView.cs` (`ChipFor` and the math or inline result code only), `Editor/Renderers.cs`,
`SelfTest.cs`: update `InputLimitChecks`, changing 99/100 days to 999h59m59s vs 1000h, and add `$5 + €3` giving the
mixed-currency reason. Other workers are editing PadView's hint, list and completion code, Settings and OCR in parallel: keep
PadView edits minimal.

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a13-check`: 0 errors, 0 warnings. `--selftest`
  PASS. Never launch or kill AntiNotepad.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A13-timer-max-and-currency-error.md`: what changed, where, and the selftest result.
