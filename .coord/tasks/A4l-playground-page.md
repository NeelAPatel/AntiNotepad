# A4l-playground-page: a local page that redraws the notepad and lets the user tune every new token live

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build.

Tier: Medium · side tool (does not ship), design preview for the token scale (step 2 of 2 for the token playground)
Branch: `chore/A4l-playground-page` in `.claude/worktrees/chore/A4l-playground-page` (cut by hand off
`feat/A4-design-tokens` at Approve; squashed back into the umbrella, not dev)

## Request
- 2026-11-04, user: "then draw all of them out accurately on an website. Have scrollable pane of adjuster cards for all
  new tokens: <token name>: number box/slider/dropdown. Cards are grouped by section and logic. Each card will have a
  rough list of all the tokens it replaced after the simplification. The Notepad viewer stays open and sticky, on a
  zoomable canvas that updates based on the values given on the adjusters."
- 2026-11-04, user answers: "Local page"; "HTML redraw"; views "Pad + top bar + pager, Search, menu, toast, Overlay modes
  and themes"; output "the adjuster is to preview in the html --> export to Live would be nice where i give you the new
  values i want to try and you make a copy of that app for me".
- 2026-11-04, user: "Yes, start there" (recommended scale as the starting values); "Yes, do both" (squash A4k, plan this).
- 2026-11-04, user, after round 1: "Excellent start. add the remaining tokens and images and graphics. One thing i'd add is
  some eye button next to the token that flashes the exact thing that is being adjusted For example i couldn't figure what
  Corner.Xs targets".
- 2026-11-04, user, after round 2: "Compare overlay + Save".
- 2026-11-04, user, add-on to round 3: "any chance we can show 'markers' on these sliders for the design system stops? Right
  now its by ones and no markers, but what if i want to see "whats the next closest recommended notch point" i.e 4, 8, 16
  etc whatever we have confirmed in our design system for each of these new simplified tokens".
- 2026-11-04, user, round 4 pick: "Search, menu, toast".
- 2026-11-04, user, round 5 pick: "Overlay modes + light".

## Findings
- **Kind and why:** side tool for a design decision; the user wants to see and tune the scale before any app change.
- **Inputs, all in the umbrella:** `docs/playground/tokens.json` (52 scale tokens: group, name, today, recommended,
  alternative, control, min, max, step, replaces; 154 role rows with their step and a `locked` flag for ChromeTokens),
  `docs/playground/measure.md` (every box in client DIPs with px and code sources; window 506 x 643 client, small 354 x
  490), `docs/playground/shots/` (49 captures, the `-2x` ones are clean 2x renders), `docs/design-scale.md`.
- **Serving pattern to copy:** `tools/roadmap-serve.ps1` (HttpListener on localhost:47811, `-Background`, `-Open`,
  `-Stop`, health URL) and `tools/roadmap/index.html` (one file, inline CSS/JS, light/dark tokens). A page opened from
  `file://` cannot fetch the JSON, so it needs the small server.
- **Measured quirks the redraw must pick a side on:** Solid puts the bar 4 DIP higher than Acrylic/Mica (use Acrylic's
  20.5 centre, note it); focus and current rings are 2 DIP on screen; the F12 renders have empty pager and toast text
  (use the grabs for those).
- **Risk:** a redraw that looks right but is off by a few px, so the user tunes against a wrong picture. Mitigation: an
  "overlay today's screenshot" switch (onion skin) on the canvas, and a check that the docked view lines up within 1 px
  at 100%.
- **Source:** master scout.

## Plan
- **After:**
  1. **Server** `tools/playground-serve.ps1`: like the roadmap server (localhost **47812**, `-Background`, `-Open`,
     `-Stop`, health). Serves `tools/playground/` and read-only `docs/playground/` (tokens, measurements, shots), plus one
     write: `POST /picks` saves the current values to `docs/playground/picks.json`.
  2. **Page** `tools/playground/index.html`, one file, no outside libraries:
     - **Canvas (left, sticky):** the notepad redrawn in HTML/CSS from `measure.md`, every size, gap, radius, stroke,
       font, alpha and duration bound to a token. Zoom 50 to 400 % (buttons and Ctrl+wheel), pan, 1:1 reset. Switches:
       view, theme (dark/light), backdrop look (Acrylic/Mica/Solid, approximated), window size (default/small),
       hover/focus state, **overlay today's screenshot** (opacity slider) to compare, and a **play** button for show/hide
       and toast motion.
     - **Adjusters (right, scrolls):** one card per section (corners, text size, weight, icons, stroke, shadow, motion,
       transparency, spacing). Each card has one row per new token: `<token name>: number box + slider` or a dropdown,
       with today's value shown, and a "replaces" list of the old tokens it absorbed. Per card: reset to Today /
       Recommended / Alternative. Top of the pane: the same three as presets for everything, and a **top-bar lock**
       switch (on: the ChromeTokens rows stay at today's values; off: they follow the scale).
     - **Export:** "Save picks" (writes `picks.json` via the server; copy-to-clipboard fallback). That file is what the
       user hands over for the lab copy of the app (next step, scale-lab).
  3. **Views:** pad docked with top bar and pager (empty and with notes, badges, timer chip), then search (field,
     results, current), right-click menu, toast; overlay and float modes; dark and light.
  4. One line in `docs/dev-loop.md` on how to open it.
- **Rounds (user, 2026-11-04: start with a simple one-pager MVP, then work up; more deployments allowed).** The full
  "After" above is the end goal, reached in small rounds on this branch. Each round is committed, pushed, shown to the
  user, and the next round is picked from their feedback.
  - **Round 1, MVP (about 1 hour):** server (`-Background`, `-Open`, `-Stop`); one page with the canvas on the left
    (sticky, zoom 50 to 400 %, 1:1 reset) showing **the docked pad, dark, default size: card, top bar (badges, 5 icon
    buttons with hover), pager pill (arrows, count, separator, dots), a few lines of note text**; adjuster pane on the
    right with **four cards: Corners, Text size, Spacing, Stroke** (each token: number box + slider, today's value,
    "replaces" list); presets **Today / Recommended** for everything. No export, onion skin, lock, other views or themes
    yet.
  - **Later rounds (order set by the user after each one):** Alternative preset and per-card resets; the remaining cards
    (weight, icons, shadow, transparency, motion); onion-skin compare; top-bar lock switch; Save picks; search, menu,
    toast; overlay and float modes; light theme; small window; motion play.
- **Who:** a Claude sub-session (Sonnet, high effort) using the built-in browser to check its page, not Codex: Codex's
  sandbox cannot run a server or a browser.
- **Must not change:** any product file, token, test or the DESIGN hash; `tokens.json`, `measure.md` and the shots
  (read only; a wrong measurement is reported, not edited).
- **Scope questions:** answered at Approve: rounds, MVP first.
- **Test plan, code:** None (side tool). Round 1, checked in the built-in browser: the page loads from the server; each
  of the four cards moves the canvas; the Today preset, compared side by side (onion skin comes later), lines up with `pad-stack-docked-solid-dark-default-2x.png` within
  1 px at 100 % (bar centre per the Acrylic note); `-Stop` frees the port. Later rounds add their own checks (Save writes
  `picks.json`, onion skin aligns).
- **Test plan, UI:** you open the page, flip Today / Recommended, try a few sliders, and say if the picture reads as the
  app.
- **Gates:** Scope (only the files below), Format.
- **Needs from you:** Approve with the cut answer; then the look check.

## Files
- `tools/playground-serve.ps1`
- `tools/playground/index.html`
- `docs/playground/picks.json`
- `docs/dev-loop.md`

## Contracts
- None.

## Approve
- 2026-11-04, user: "start with a simple one pager MVP for some main basic controls. then work your way up. No need to
  bulk everything in two deployments. more deployments to this are allowed."

## Check

## Hand test
