# A18-settings-segment-animation: animate selector-style Settings controls

Item card (`docs/loop.md`). Budget: 4 KB.

Tier: Medium · ui-motion
State: approved
Branch: `feat/A18-settings-segment-animation` in `.claude/worktrees/feat/A18-settings-segment-animation` (created at Approve)

## Request
2026-11-12, user (pasted list, Animations, Settings), verbatim: "A transition on System / Light / Dark buttons — not just system/light/dark im saying selectors of that style. this was just an example. Perhaps this should be a html web demo entirely with the current stack to see what other ways we can implement these type of settings options."

Then: "start 1 and 2 in parallel". Scope: every selector-style (segmented) control in Settings, not only System/Light/Dark. The user floated an HTML lab of several selector styles to compare; show options in a lab page before building. Reuse the Windows 11 motion values in `docs/decisions.md` (167 ms in, 83 ms out, `Win11Enter`/`Win11Exit`, tokens in `Motion/MotionTokens.cs`) rather than inventing timings; the user cannot judge motion from numbers, so show it in a lab page (`tools/playground/`, served on localhost) or the dev copy. Settings controls live under `product/AntiNotepad/UI/Settings/` and `UI/Kit`.

## Findings
One shared control: `Seg()` in `UI/SettingsParts.cs` (~180) builds every selector as pills in a WrapPanel; chosen pill gets an accent wash, no motion. 15 call sites in `UI/SettingsWindow.cs` (Theme, Show chrome, Bars, Line spacing, Paper strength, Scrolling style, Swipe style/length/fires-at, Lines per notch, Push, Coast, Distance, Boost, Taskbar). Hover repaint via `Kit.Plain`.
Not segmented but selector-like, each its own code: `Tile()` picture ring (caret, material, open-at), `PaletteChips`, `AccentDots`, `Dropdown`, rail `BuildRail`. Out of scope unless the user wants them; ask after the lab.
Settings rebuilds rows on `Set(rows:true)`, which would drop a running animation: the pill must persist or animate before the rebuild.

## Plan
After: picking an option glides one highlight to it (user approved style 2 Sliding pill, 2026-11-12, plus hover wash fading 167 ms in / 83 ms out on these pills; lab: `tools/playground/a18-segment-lab.html`, served at http://127.0.0.1:47830/a18-segment-lab.html), 167 ms enter curve; reduced motion snaps. Change `Seg()` only, so all 15 selectors animate at once.
Must not change: option values, Set()/pick order, hover wash, keyboard/automation names, other Settings controls, A17 toggle files.
Test plan: --selftest for the pill target maths; gate; hand test in a dev copy (rapid clicks, Settings rebuild, narrow window).
Needs from you: hand test. Approved by the user 2026-11-12 ("yes with hover"). Hover fade is pills only; shared Kit.Plain stays untouched unless Codex proves it must change (tell the user first).

## Files
- `product/AntiNotepad/UI/SettingsParts.cs`
- `product/AntiNotepad/Motion/MotionTokens.cs`
- `product/AntiNotepad/SelfTest/Design.cs` (only if the design snapshot hash changes)

## Contracts
None new.

## Approve
2026-11-12, user: "yes with hover" (style 2 Sliding pill plus hover fade on the pill selectors).

## For Codex
Role: terra, a UI feature in one control plus tokens. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then the Files; re-read before editing.
- Build exactly the Plan's "After" in `Seg()` (`UI/SettingsParts.cs`): one accent-wash highlight glides (left and width) from the old option to the new one in `Win11Fast` with `Win11Enter`; hover wash on each pill fades in `Win11Fast`/`Win11Enter`, out `Win11Faster`/`Win11Exit`; reduced motion snaps (find how other motion checks it). Pills have uneven widths, so animate by measured bounds. Keep pick order (`chosen`, Paint, `pick`) and automation names. Settings rebuilds rows on `Set(rows:true)`: a rebuilt control starts with the highlight already on the chosen option (no replay); rapid clicks retarget from the current position. Use `Anim`/`Motion` helpers; add named tokens to `Motion/MotionTokens.cs` only if needed. Do not edit shared `Kit.Plain`; if it must change, stop and report. Add a `--selftest` check for the target-bounds maths. Reference: `tools/playground/a18-segment-lab.html` style 2.
- Build into `build/A18-settings-segment-animation-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, delete the folder. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A18-settings-segment-animation -Offline`.
- Never launch or kill AntiNotepad, never run `--quit`, never edit STATUS or this card. Offline checks only.
- Done means: selectors behave as in After; build 0 warnings, `SELFTEST PASS`, gate passes.