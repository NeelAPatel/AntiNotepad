# A4k-playground-measure: measure the dev copy to the pixel, as input for the token playground

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build.

Tier: Medium · measurement by computer control, no product change (step 1 of 2 for the token playground)
Branch: `docs/A4k-playground-measure` in `.claude/worktrees/docs/A4k-playground-measure` (cut by hand off
`feat/A4-design-tokens` at Approve; squashed back into the umbrella, not dev)

## Request
- 2026-11-04, user: "i want you to show me the recommended scale of changes: I want you to use computer control, take a
  screenshot of the current notepad in multiple dimensions and states, highlight some controls, change its overlay modes,
  and compile a series of screenshots that essentially give you pixel level dimension of the notepad and its gaps,
  heights, widths, etc both visually and in code. then draw all of them out accurately on an website. Have scrollable pane
  of adjuster cards for all new tokens: <token name>: number box/slider/dropdown. Cards are grouped by section and logic.
  Each card will have a rough list of all the tokens it replaced after the simplification. The Notepad viewer stays open
  and sticky, on a zoomable canvas that updates based on the values given on the adjusters. Can you promise me that as
  part of design-tokens development?"
- 2026-11-04, user answers: page "Local page"; fidelity "HTML redraw"; views "Pad + top bar + pager, Search, menu, toast,
  Overlay modes and themes" (Settings window not in v1); output "the adjuster is to preview in the html --> export to Live
  would be nice where i give you the new values i want to try and you make a copy of that app for me".
- 2026-11-04, user: "Yes, start there" (the recommended scale is the playground's starting values); "Yes, do both"
  (squash A4j, plan the playground).

## Findings
- **Kind and why:** polish (design system tooling). The playground needs exact pixel facts; today's tokens and XAML give
  the numbers, but not how they add up on screen (stacked margins, derived heights, glyph ink).
- **Now:** `tools/dev.ps1` builds and launches the dev copy (`build/dev`, data `build/testdata`, hotkey
  Ctrl+Alt+Shift+D, dev badge). `ANTINOTEPAD_SHOTS=build/shots` is set but no product code reads it. The design dump
  (`SelfTest/Design.cs`) records 13 style properties per control (font, radius, opacity, brushes, margin, padding), no
  bounds. Tokens: `Theme/` (Typography, Shape, Paint, ChromeTokens, Palette), `Motion/MotionTokens.cs`; the scale and
  its mapping: `docs/design-scale.md` (A4j).
- **Where:** dev copy only, plus one new folder of captures and one measurements doc. No product file.
- **Risk:** computer control on the wrong window. The installed copy (`%LOCALAPPDATA%\Programs\AntiNotepad`) runs at the
  same time; it must never be typed into, resized or stopped. Mitigation: dev copy only (title `AntiNotepad (dev)`, dev
  badge, its own hotkey), path check before any stop. Second risk: screenshots at a scaled DPI; record the display scale
  and convert to DIPs.
- **Source:** master scout.

## Plan
The playground is two steps: **A4k-playground-measure** (this card) and then **A4l-playground-page** (the local web
page: sticky zoomable canvas, adjuster cards, starts from the recommended scale, a lock on/off switch for the top bar
values). A4l is planned after this one lands, from its measurements.

- **After:**
  1. **Captures** in `docs/playground/shots/` (PNG, full scale, named `<view>-<state>-<theme>-<size>.png`), from the dev
     copy by computer control:
     - views: pad empty and with notes (plain, list, timer badge, math), top bar shown and hidden, pager (pill, dots,
       count, hover on each), search (card, results, current), right-click menu, a dropdown popup, a toast;
     - themes: dark and light; each overlay and docked mode Settings offers; two window sizes (default and small);
     - highlights: hover and focus states of the bar icons, pager parts and menu rows.
  2. **Measurements** in `docs/playground/measure.md`: per view, every box that the redraw needs (x, y, width, height in
     DIPs, at the recorded display scale), gaps and paddings between them, corner radii, line thickness, font size and
     weight, each with **two sources**: the pixels measured on the capture and the token or XAML value it comes from (or
     "derived: a + b"). A mismatch between the two is listed, not fixed.
  3. **Token sheet** `docs/playground/tokens.json`: every token the page will adjust, from `docs/design-scale.md`: the
     new token (group, name, today, recommended, alternative, control kind: number box, slider or dropdown, min and max)
     and the old tokens it replaced. The page in A4l reads this file.
- **Who:** a Claude sub-session (Sonnet, high effort) with computer control, not Codex: Codex has no screen and no
  browser. Computer control is allowed for this step on the dev copy only (user, 2026-11-04: "I want you to use computer
  control"); the app may ask you to approve screen access once.
- **Expected time:** about 1.5 hours (computer control is slow: about 40 captures plus the measuring). A4l is another
  2 to 3 hours, planned separately.
- **Must not change:** any product file, token, test or the DESIGN hash; the installed copy and its data.
- **Scope questions:** None (answered 2026-11-04).
- **Test plan, code:** None (no code). The token sheet must list every token named in `docs/design-scale.md` sections 3.1
  and 3.2 (the session checks the count).
- **Test plan, UI:** None for you here; you see the captures in the doc.
- **Gates:** Scope (only `docs/playground/` and this card), Format.
- **Needs from you:** Approve; approve screen access once when asked.

## Files
- `docs/playground/measure.md`
- `docs/playground/tokens.json`
- `docs/playground/shots/dropdown-tint-popup-light-default.png`
- `docs/playground/shots/menu-contextmenu-dark-default.png`
- `docs/playground/shots/menu-contextmenu-hover-row-dark-default.png`
- `docs/playground/shots/menu-contextmenu-hover-row-solid-dark-default.png`
- `docs/playground/shots/pad-slot1-markdown-dark-default.png`
- `docs/playground/shots/pad-slot2-math-dark-default.png`
- `docs/playground/shots/pad-slot3-checklist-dark-default.png`
- `docs/playground/shots/pad-slot4-timer-running-dark-small.png`
- `docs/playground/shots/pad-slot4-timerdone-solid-dark-default-2x.png`
- `docs/playground/shots/pad-slot4-timers-dark-default.png`
- `docs/playground/shots/pad-slot5-bar-dark-default.png`
- `docs/playground/shots/pad-slot5-nobar-dark-default.png`
- `docs/playground/shots/pad-slot6-empty-dark-default.png`
- `docs/playground/shots/pad-slot6-plain-dark-default.png`
- `docs/playground/shots/pad-slot7-list-dark-default.png`
- `docs/playground/shots/pad-stack-docked-solid-dark-default-2x.png`
- `docs/playground/shots/pad-stack-docked-solid-dark-default.png`
- `docs/playground/shots/pad-stack-focus-bar-solid-dark-default-2x.png`
- `docs/playground/shots/pad-stack-focus-bar-solid-dark-default.png`
- `docs/playground/shots/pad-stack-hover-pagernext-solid-dark-default.png`
- `docs/playground/shots/pad-stack1-current-dark-default.png`
- `docs/playground/shots/pad-stack1-docked-light-default.png`
- `docs/playground/shots/pad-stack1-docked-mica-dark-default.png`
- `docs/playground/shots/pad-stack1-docked-solid-dark-default.png`
- `docs/playground/shots/pad-stack1-docked-solid-dark-small.png`
- `docs/playground/shots/pad-stack1-empty-dark-default.png`
- `docs/playground/shots/pad-stack1-float-bar-dark-default.png`
- `docs/playground/shots/pad-stack1-float-bar-light-default.png`
- `docs/playground/shots/pad-stack1-float-light-default.png`
- `docs/playground/shots/pad-stack1-float-pager-light-default.png`
- `docs/playground/shots/pad-stack1-hover-pagercount-dark-default.png`
- `docs/playground/shots/pad-stack1-hover-pagerdot-dark-default.png`
- `docs/playground/shots/pad-stack1-hover-pagerprev-dark-default.png`
- `docs/playground/shots/pad-stack1-hover-search-dark-default.png`
- `docs/playground/shots/pad-stack1-hover-settings-dark-default.png`
- `docs/playground/shots/pad-stack1-overlay-bar-dark-default.png`
- `docs/playground/shots/pad-stack1-overlay-bar-light-default.png`
- `docs/playground/shots/pad-stack1-overlay-dark-default.png`
- `docs/playground/shots/pad-stack1-overlay-light-default.png`
- `docs/playground/shots/pad-stack1-overlay-pager-light-default.png`
- `docs/playground/shots/pad-stack2-math-dark-default.png`
- `docs/playground/shots/search-current2-dark-default.png`
- `docs/playground/shots/search-focus-toolbutton-dark-default.png`
- `docs/playground/shots/search-results-dark-default.png`
- `docs/playground/shots/search-results-solid-dark-default-2x.png`
- `docs/playground/shots/search-results-solid-dark-default.png`
- `docs/playground/shots/toast-copied-dark-default.png`
- `docs/playground/shots/toast-copied-solid-dark-default-2x.png`
- `docs/playground/shots/toast-copied-solid-dark-default.png`

## Contracts
- None.

## Approve
- 2026-11-04, user: "Approve"
- 2026-11-04, user, screen access (relayed by the master): "retry the capture, i will approve"; then in this session
  the user allowed it and added: "I allow, but also ensure you're capturing multiple pages of the saved notes section
  and multiple pages of the regular section too." (slots 1-7 and stack pages 1/2 and 2/2 are captured)

## Check

## Hand test
