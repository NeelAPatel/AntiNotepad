# A4j-scale-proposal: pick a few values per category and map every control kind onto them, on paper

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build.

Tier: Medium · design choice, on paper only (the lab build comes next)
Branch: `docs/A4j-scale-proposal` in `.claude/worktrees/docs/A4j-scale-proposal` (cut by hand off `feat/A4-design-tokens`
at Approve; squashed back into the umbrella, not dev)

## Request
- 2026-11-03, user: "so instead of using 7~10 different numbers we reduce it to maybe 5 numbers which follow a
  pattern/theme and consistency, with a few exceptiosn here and there"
- 2026-11-03, user, on the four-step path (systems study, scale proposal, lab build, apply): "Yes, this path."
- 2026-11-03, user, on "Squash the study and plan the scale proposal?": "Yes, do both"

## Findings
- **Kind and why:** polish (design system), step 2 of the path.
- **Inputs, all in the umbrella:** `docs/design-systems.md` (tiers, step-count aims: radius 5, type 5, weight 2, stroke 2,
  elevation 2, motion 4, alpha 5, icon 3, spacing 6), `docs/design-fluent.md` (Windows 11 values and the patterns we
  break), `.coord/reports/A4g-control-groups-map.md` (15 control kinds, where each disagrees), today's tokens in
  `product/AntiNotepad/Theme/` (Typography, Shape, Paint, ChromeTokens, Palette) and `Motion/MotionTokens.cs`.
- **Earlier scope:** on 2026-11-01 the user kept spacing and thickness out of design-tokens ("ratchet only"). The new
  goal ("5 numbers ... pattern") may bring spacing back: asked at Approve.
- **Lock:** `docs/decisions.md`, "Bar insets and chrome tokens" fixes top bar and pager numbers (bar 40, buttons 32,
  badges 20, hover radii 8/9, pill 42 tall). Any scale step that changes them needs the user to lift that lock.
- **Risk:** a scale that reads well on paper but changes the feel (corners 7 to 4 is the biggest visible change). The
  lab build exists to catch that; this step changes nothing in the app.
- **Source:** master scout.

## Plan
- **After:** `docs/design-scale.md`:
  1. **Per category** (corners, text size, weight, icon size, stroke, shadow, motion, transparency, spacing: margins,
     padding and gaps, read from the XAML and code): the scale steps (name, value), the **recommended** scale and **one alternative** (for example
     Windows-like corners 4/8 vs a softer 6/10), each with a one-line reason tied to Fluent or the systems study.
  2. **Roles > step:** every existing role token (`Typography.*`, `Shape.*`, `Paint.*`, `MotionTokens.*`) and each
     ChromeTokens value mapped to one step; today's value, new value, change (+/- px or ms).
  3. **Controls:** one row per control kind from the A4g map (15): which role it uses for radius, font, padding, fill,
     border, shadow; each A4g disagreement resolved to one role or named as an exception.
  4. **Exceptions:** each literal kept, with why (for example pager dots 7 px).
  5. **Visible changes, biggest first:** what the user will see in the lab build, so the look test knows where to
     look.
  6. **Lock:** the ChromeTokens values that would change, as one question to lift or keep the lock.
- **Who:** a Claude sub-session (Sonnet, high effort), not Codex: this is design judgement across three docs with no
  code, and the Codex map needed four rounds. Offline, no web.
- **Expected time:** about 45 minutes.
- **Must not change:** any product file, token or test.
- **Scope questions:** is spacing in (6-step 4 px grid) or still out? Answered at Approve: in.
- **Test plan, code:** None (no code).
- **Test plan, UI:** None. You read the doc and pick per category (recommended or alternative).
- **Gates:** Scope (only the doc changed), Format.
- **Needs from you:** Approve with the spacing answer; then your picks per category and the lock answer.

## Files
- `docs/design-scale.md`

## Contracts
- None.

## Approve
- 2026-11-03, user: "Approve, with spacing" (spacing is in: a ~6-step 4 px grid, mapped like the other categories)

## Check
- 2026-11-03: doc `docs/design-scale.md` (1f58bb6, 337 lines); gate PASS (Scope: only the doc; Format). Master read it in
  full: 9 categories with recommended and alternative, every role and ChromeTokens value mapped, 15 controls, 13
  exceptions, visible changes, one lock question.

## Hand test
- 2026-11-04, user, on "Use the recommended scale as the playground's starting values?": "Yes, start there"
- 2026-11-04, user, asked for a token playground before approving the scale (local page; HTML redraw; pad + bar + pager,
  search/menu/toast, modes and themes; output: preview in HTML, then a lab copy of the app from the values they give).
  Promised as the next design-tokens step.
- Lock question (ChromeTokens): open; to be answered after the playground shows both.
