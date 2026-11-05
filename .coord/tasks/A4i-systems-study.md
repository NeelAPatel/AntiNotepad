# A4i-systems-study: how real design systems build and organize their tokens

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build.

Tier: Low · notes only, no code
Branch: `docs/A4i-systems-study` in `.claude/worktrees/docs/A4i-systems-study` (cut by hand off `feat/A4-design-tokens` at
Approve; squashed back into the umbrella, not dev)

## Request
- 2026-11-03, user: "my intention here is to learn from microsoft's fluent and maybe some others to understand how design
  systems are built and organized --> map it to our controls and control groups. so instead of using 7~10 different
  numbers we reduce it to maybe 5 numbers which follow a pattern/theme and consistency, with a few exceptiosn here and
  there"
- 2026-11-03, user, on the four-step path (systems study, scale proposal, lab build, apply): "Yes, this path. We wont be
  merging to dev until i say so"

## Findings
- **Kind and why:** process (design research); step 1 of the design-system path. The scale proposal needs a stated
  method for choosing a few values and mapping controls to them.
- **Now:** `docs/design-fluent.md` (A4h-fluent-study) compares Fluent's numbers with ours, but not how systems are
  organized. `.coord/reports/A4g-control-groups-map.md` lists 15 control kinds and where each disagrees. Our tokens are
  one tier only (role names such as `Typography.Caption`, `Shape.Control`), with no scale under them and no control tier
  over them.
- **Where:** no product file changes.
- **Risk:** describing a system from memory. Every claim needs its source page.
- **Source:** master scout.

## Plan
- **After:** `docs/design-systems.md`, short and scannable:
  1. **Tiers:** how each system layers tokens (Fluent 2 global > alias > control; Material 3 reference > system >
     component; Apple HIG and one more web system, for example IBM Carbon or GitHub Primer), with names and one
     example chain each.
  2. **How scales are chosen:** base grid (4 / 8 px), type ramps (fixed list or ratio), radius sets, alpha and
     elevation steps, motion buckets; how many steps each system keeps per category.
  3. **Exceptions:** how each system allows a one-off (control-level override, documented exception) without breaking
     the scale.
  4. **Naming:** how tokens are named per tier (size words, numbers, t-shirt sizes).
  5. **Template for us:** a three-tier shape for AntiNotepad (scale > role > control) with our 15 control kinds from the
     A4g map as the control tier, and a per-category step count to aim for. No values chosen: that is scale-proposal.
- **Who:** a Claude sub-session with web access (Sonnet medium), not Codex: Codex runs offline. It cites the URL beside
  every claim.
- **Expected time:** about 30 minutes.
- **Must not change:** any product file, token or test.
- **Scope questions:** None.
- **Test plan, code:** None (no code).
- **Test plan, UI:** None (not visible). You read the doc.
- **Gates:** Scope (only the doc changed), Format.
- **Needs from you:** Approve now; read the doc when it lands.

## Files
- `docs/design-systems.md`

## Contracts
- None.

## Approve
2026-11-03, user: "Approve"

## Check

## Hand test
None (doc only). 2026-11-03, user on "Squash the study and plan the scale proposal?": "Yes, do both".
