# A4h-fluent-study: set Windows 11 Fluent sizes beside ours and name the patterns we break

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build.

Tier: Low · notes only, no code
Branch: `docs/A4h-fluent-study` in `.claude/worktrees/docs/A4h-fluent-study` (cut by hand off `feat/A4-design-tokens` at
Approve; squashed back into the umbrella, not dev)

## Request
- 2026-11-02, user: "we'll take time to examine design language and principles followed by microsoft for their sizes and
  observe patterns. for example certain things are always even numbers."
- 2026-11-02, user, on "Squash A4f into the umbrella, then plan control-groups and fluent-study?": "Yes, do both"

## Findings
- **Kind and why:** process (design research); our sizes grew one control at a time, so there is no stated rule for
  them. Fluent gives one to compare against.
- **Now:** our values live in `Theme/Typography.cs` (sizes 11, 11.5, 12, 12.5, 13, 13.5, 14, 15, 17, 18, 26),
  `Theme/Shape.cs` (radii 1, 5, 7, 8, 10, 12), `Theme/ChromeTokens.cs` (bar 40, buttons 32, badges 20, pill inset 4,
  pager dots 7, border 1.2), `Theme/Paint.cs` and `Motion/MotionTokens.cs` (80 to 200 ms). Odd and half sizes are
  common (11.5, 12.5, 13.5, 5, 7, 7 px dots, 1.2 px border).
- **Where:** no product file changes.
- **Risk:** Fluent numbers quoted from memory instead of the source. Every Fluent value needs its Microsoft page.
- **Source:** master scout.

## Plan
- **After:** `docs/design-fluent.md`, one table per area (type ramp, spacing grid, control sizes, corner radii,
  borders and strokes, elevation and shadow, motion durations and easing, materials and tint ranges): Fluent value |
  ours | token | match / near / breaks. Then a short "patterns" list (for example "sizes on a 4 px grid", "type sizes are
  even", "two radii only: 4 controls, 8 overlays") with which of ours break each, and a list of candidate changes for
  design-questions. No recommendations are applied.
- **Who:** a Claude sub-session with web access (Sonnet medium), not Codex: Codex runs offline. It reads Microsoft
  Learn's Windows 11 design pages (typography, geometry, spacing and sizing, layering and elevation, motion, materials)
  and the Fluent 2 site, and cites the URL beside every Fluent value.
- **Expected time:** about 30 minutes.
- **Must not change:** any product file, token or test.
- **Scope questions:** None.
- **Test plan, code:** None (no code).
- **Test plan, UI:** None (not visible). You read the doc.
- **Gates:** Scope (only the doc changed), Format.
- **Needs from you:** Approve now; read the doc when it lands.

## Files
- `docs/design-fluent.md`

## Contracts
- None.

## Approve
2026-11-02, user: "Approve both"

## Check

## Hand test
None (doc only). 2026-11-03, user on the reshaped design-system path: "Yes, this path." The doc is input to the scale
proposal; its candidates move there.
