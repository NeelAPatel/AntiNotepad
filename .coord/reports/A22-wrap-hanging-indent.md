# A22-wrap-hanging-indent

## Summary
A22a probe stopped: AvaloniaEdit 12.0.0 and Avalonia 12.1.4 expose no bounded product-only hook that can apply hanging indentation to wrapping and keep drawing, caret, selection, and hit-testing geometry together. The approved plan requires stopping if a dependency patch is the only route, so A22b was not built.

## Change list
- Added this probe report.
- No product code changed. The temporary `build/A22-check` probe was removed.

## Behavior changes
None. The existing wrap behavior remains.

## Out-of-scope files
No product files, `PadWindow*`, Settings, Store, STATUS, or item card files were changed.

## Plan deviations
A22b implementation, selftests, CONTRACTS row, product build, selftest run, and task gate were skipped because the A22a integration gate did not pass. No dependency patch was attempted.

## Contracts
No new contract row: no behavior change shipped. The requested `wrap-indent-text-column` contract remains unverified.

## Checks run
- Built and ran a temporary reflection probe under `build/A22-check` with `AVALONIA_TELEMETRY_OPTOUT=1`: 0 warnings, 0 errors. It loaded AvaloniaEdit 12.0.0 against Avalonia 12.1.4.
- Confirmed `TextView.CreateParagraphProperties` is private and nonvirtual; `VisualLine` is sealed and its position methods are nonvirtual; `VisualLineTextParagraphProperties` is internal and sealed. `TextView.GetPosition` and `GetVisualPosition` are nonvirtual.
- No app launch, screenshot, product build, selftest, or task gate was run.

## Probe result (options tried)
- Existing `WordWrapIndentation`/`InheritWordWrapIndentation` configuration cannot fix this: the audit shows AvaloniaEdit never enables its internal calculation, and Avalonia's formatter ignores paragraph `Indent`.
- A `TextView` drawing override or background renderer can only shift painted pixels; it cannot change the sealed `VisualLine` layout/position mapping used by caret, selection, and hit-testing.
- Existing line transformers and element generators can alter text runs, but provide no paragraph-formatting hook for continuation width and shared geometry.
- The remaining coordinated route crosses AvaloniaEdit's private paragraph construction and Avalonia's formatter. That is a dependency patch, outside the approved probe gate.

## Hand checks for the user
Pending an accepted integration and dev build. Compare before/after screenshots of the supplied nested-bullet example at a narrow width; then check levels 1–3, top-level bullets, numbered items, checkboxes, plain indented and unindented lines, multiple widths and fonts, plus caret movement, selection, and click-to-caret hit-testing. No screenshot was produced because no app build was made or launched.
