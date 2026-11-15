# A20-overscroll-speed-r2: carry scroll momentum into the overscroll

Rework round of `A20-overscroll-speed` (`docs/loop.md`). Read the item card `.coord/tasks/A20-overscroll-speed.md` first: its Plan, Files, Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Medium, same as the card
Role: worker, because it changes shared timing state across the four vertical styles and the edge spring

## Why this round
User hand test, 2026-11-15, verbatim: "there small issue where the momentum of scrolling on the note does not smoothly carry over into the overscroll. its like two distinct animations".
Cause (`Editor/Motion.cs` `PhysicsStep`, `GlideStep`): when Coast, Spring or Fly reaches the top or bottom it zeroes its velocity at the boundary and stops; the overscroll starts only on the next input event. So a glide that arrives at the end with speed just stops, and a separate pull plays if you keep rolling.

## Change to the plan
Plan "After" gains: when a vertical glide, coast or fly reaches the top or bottom still moving (outward speed v in px/s), the leftover speed goes into the same edge band as a pull that is already moving, with no jump in position or speed. Concretely: start the `EdgeBand` in its returning state at shown = 0 with velocity = v (the critically damped spring then overshoots into the band and returns; peak about v / (omega * e), about 2.5 percent of v in px at period 420 ms), instead of zeroing the velocity. Add the small `EdgeBand` entry point this needs (for example `StartMoving(double velocity, bool wheel)`), keep the existing hold and spring numbers in `MotionTokens`. Lines (exponential glide) uses its last-frame speed (distance moved over dt) at arrival. Reduce motion: no carry-over (unchanged bypass). Wheel input arriving during the carry-over behaves as it does now during a return (`CurrentPull` then push). Speeds below the existing stop thresholds (15, 5, 4 px/s) do not start an overscroll.

## Files
- `product/AntiNotepad/Editor/Motion.cs`
- `product/AntiNotepad/Motion/EdgeBand.cs`
- `product/AntiNotepad/Motion/MotionTokens.cs`
- `product/AntiNotepad/SelfTest/ScrollFeel.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `docs/scroll-feel.md`

## Done means
Offline checks only (`docs/agents.md`).
- A coast (Free), spring (Rate-matched) and fly (Wheel-driven) that reaches an end at speed v starts an edge return with that velocity; position and speed are continuous across the handoff (selftest: no one-frame jump over N px, speed step small); a slow arrival below the thresholds does not overscroll.
- A new contract row for the carry-over; the two A20 rows still pass.
- `docs/scroll-feel.md` vertical section states the carry-over rule.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A20-overscroll-speed-r2 -Offline` passes.

Report: `.coord/reports/A20-overscroll-speed-r2.md`, in the card's report format.
