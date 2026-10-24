# A13-timer-max-and-currency-error

- `Model/Timers.cs`: capped durations at 3,599,999 seconds, accepted clock timers with three hour digits, and allowed compact duration parts such as `999h59m59s`.
- `Calc/Calculator.cs`: added a `MixedCurrencies` evaluation reason for currency mismatches in expressions, aggregates, and note totals; exposed it for inline `=` evaluation.
- `Editor/PadView.cs`: displayed `mixed currencies` in the palette Error color for math and inline results. Error chips do not copy or show a clickable cursor. Other invalid expressions remain silent.
- `SelfTest.cs`: covered maximum and over-limit timers, long timer formatting, and the mixed-currency reason for math and inline expressions.

Verification: Release build succeeded with 0 warnings and 0 errors; `--selftest` passed. `NuGetAudit=false` was set in the build environment because the restricted network could not reach NuGet's vulnerability metadata service.

The build output remains in `build/a13-check`; the environment blocked recursive removal of that folder.
