# A21-save-delay-setting-r2: fix the two failing selftest lines

Rework round of `A21-save-delay-setting` (`docs/loop.md`). Read the card `.coord/tasks/A21-save-delay-setting.md` first; its Plan, Files, Contracts and "For Codex" still apply unless this file says otherwise.

Tier: High (same)
Role: worker, because the cause is shared test state across selftest checks.

## Why this round
Item chat ran `tools/gate.ps1 -Task A21-save-delay-setting -Offline`: selftest finishes (no hang) but `SELFTEST FAIL`:
1. `contract shortened-link-opens: shortened link: Setup was already called on one of AppBuilder instances`. Suspect: your new UI check in `SelfTest/UI.cs` (~line 19 of the diff) calls `AppBuilder.Configure<Application>().UsePlatformDetect().SetupWithoutStarting()` when `Application.Current is null`, so a later check sets Avalonia up twice. Find how neighbouring checks create the app/window (reuse that helper) and remove the second setup. Prove cause by running the selftest on dev's build if needed.
2. `DESIGN ... FAIL expected 183c64c8...`, got `7c2c7648...`. The new "Saving" Settings group changes the dump. Run `--design-dump <file>` on this build and on dev's build (`build/` folder only), diff them, confirm the ONLY change is the new Saving group/row, then update the expected hash in `SelfTest/Design.cs`. If the diff shows anything else, stop and report it.

## Change to the plan
None.

## Files
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest/Store.cs`

## Done means
Offline checks only.
- Both failures gone; no product behavior change.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A21-save-delay-setting-r2 -Offline` passes (run it yourself; if a test process hangs, tell me the PID and path, do not wait).

Report: `.coord/reports/A21-save-delay-setting-r2.md`, in the card's report format.
