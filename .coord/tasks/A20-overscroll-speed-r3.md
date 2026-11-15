# A20-overscroll-speed-r3: remove the vertical overscroll

Rework round of `A20-overscroll-speed` (`docs/loop.md`). Read the item card `.coord/tasks/A20-overscroll-speed.md` first: its Files, Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Medium, same as the card
Role: worker, because it unwinds shared timing state in `SmoothScroll` and `EdgeBand` without touching the horizontal feel

## Why this round
User hand test of the round 2 dev copy, 2026-11-15, verbatim: "hm. its fine remove the vertical overscroll logic. let it stay as windows 11 style smooth scrolling vertically".

## Change to the plan
Plan "After" changes: vertical scrolling returns to exactly how it behaved on `dev` before this item (Lines, Free, Rate-matched, Wheel-driven glide inside the note; the ends just stop; touchpad follows 1:1). No vertical overscroll, no carry-over, no `SetPull` / `VisualPull` / vertical edge state. Kept as built: the horizontal Follow-through feel (NoteSwipe, PadView swipe, preview), the Settings > Feel names and one-option Horizontal Scroll Style, defaults Rate-matched (vertical) and Follow-through (horizontal), Reduce motion. Start by comparing `product/AntiNotepad/Editor/Motion.cs` with `git show dev:product/AntiNotepad/Editor/Motion.cs`; restore its scrolling code from dev (keep only a doc-comment tweak if wanted), then delete what only the vertical overscroll used from `EdgeBand.cs` (keep what the horizontal swipe uses), `MotionTokens.cs` (vertical nudge and carry-over cutoff tokens), `SelfTest/ScrollFeel.cs`, and the contract rows `vertical-edge-pull-springs-home` and `vertical-scroll-carries-momentum-to-edge` in `SelfTest/Contracts.cs`. Update `docs/scroll-feel.md` (vertical section: unchanged glide, no overscroll; note this was the user's call on 2026-11-15) and `docs/decisions.md`. Reduce motion subtitle in Settings: say overscroll only if it still applies to the horizontal pull.

## Files
- `product/AntiNotepad/Editor/Motion.cs`
- `product/AntiNotepad/Motion/EdgeBand.cs`
- `product/AntiNotepad/Motion/MotionTokens.cs`
- `product/AntiNotepad/SelfTest/ScrollFeel.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest/Run.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/App.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `docs/scroll-feel.md`
- `docs/decisions.md`

## Done means
Offline checks only (`docs/agents.md`).
- `Editor/Motion.cs` scrolling behaves as on `dev` (diff against dev shows no vertical edge code); no unused vertical-edge symbols or tokens remain.
- Horizontal contract `horizontal-edge-pull-no-jump` and every horizontal check still pass.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A20-overscroll-speed-r3 -Offline` passes.

Report: `.coord/reports/A20-overscroll-speed-r3.md`, in the card's report format.
