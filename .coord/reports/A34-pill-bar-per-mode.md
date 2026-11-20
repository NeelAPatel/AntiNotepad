# A34-pill-bar-per-mode implementation report

## Changed files
- `product/AntiNotepad/UI/PadWindow.Bar.cs` (new), `PadWindow.Layout.cs`, `PadWindow.Navigation.cs`, `PadWindow.Keys.cs`, `PadWindow.TopBlur.cs`: three persistent bar containers, active-pane pager, swap and action controls, and whole-bar placement/visibility.
- `product/AntiNotepad/UI/Kit.cs`, `SettingsWindow.cs`, `Shortcuts.cs`; `Model/Settings.cs`; `Theme/ChromeTokens.cs`; `docs/shortcuts.md`: hover colors, settings, shortcut and design values.
- `product/AntiNotepad/SelfTest/Bar.cs` (new) and `SelfTest/UI.cs`: bar decisions, defaults/round trip, slot target, debounce, container parent and hit testing checks.

## What I decided
- In split view, if the preferred swap target is already displayed in the right pane, select the first filled available slot, or the first available empty slot, so swap leaves the right pane unchanged. Otherwise remember the last left slot, falling back to first filled or slot 1.
- New settings use the existing SettingsService fallback, which supplies appearance and persistence notifications for unmapped settings.
- The lens reviewer found focus and last-slot issues during review; both were fixed. It also requested real-container and fade/hit-test checks, which were added after review.
- Behavior changes: the bottom bar follows the active pane, exposes Delete and New note per settings, and swap changes the left view; the three new settings and unbound shortcut are persisted through existing settings mechanisms.

## Selftest output summary
- Offline local-cache restore and build: passed, zero warnings and errors.
- Selftest: **FAIL**, solely the pinned design dump. All 36 automated contract rows passed, one manual row remains; layer checks report zero violations. Expected hash `b31e3ae5432abe120b395b96834d526f3f4dd3af79d6fa0f65c2fb8ffc338955`; deterministic actual hash `6693fbbd9f7f302e0a6b4ffc2de92499d15515a8cfed556b532f96dadc3a96ee` (18,048 lines).

## Gate result
**FAIL**: Selftest failed on the pinned design snapshot. Build, Seam, Tokens, Format, Scope and Size passed. See `.coord/reports/A34-pill-bar-per-mode.gate.md`. Lens review returned accept with a test-coverage gap; the requested real-control parenting and hit-test checks were added afterward and pass in the selftest contract run.

## Not verified
Visual appearance, hover colors, reveal timing, 320 px fit, and split behavior require the manager's hand test in the slot dev copy. No app was launched for hand testing.

**Scope request:** The approved Settings UI changes alter the pinned design snapshot, whose expected hash is in `product/AntiNotepad/SelfTest/Design.cs`. That file is outside `## Files`, so it was not changed. Please add it to the authorized scope so the design dump can be reviewed and its expected hash updated, then rerun the selftest and gate. The item is not ready for hand test or acceptance until these pass.
