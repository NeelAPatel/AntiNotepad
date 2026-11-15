# A20-overscroll-speed: one smooth feel for swipe, scroll and overscroll

Tier: Medium - ui-motion (replaces the Feel scroll and switch feels)
State: approved
Branch: `feat/A20-overscroll-speed` in `.claude/worktrees/feat/A20-overscroll-speed` (created at Approve)

## Request
2026-11-12 (user): "Overscroll feels faster than the page-swipe animations between notes; match them"; 2026-11-13: "theres a point in the overscroll where it simply snaps back".
2026-11-15 (user): "lock in horizontal parameters. Save the parameters to a .md. take the overscroll logic in the new version and apply it to Rate Matched. Settings > Feel: Scroll style -> rename to Vertical Scroll Style; add Horizontal Scroll Style with only one option, the new logic. These are the new default settings. Make this change in the app." and "the overscroll logic should apply everywhere too".

## Findings
- **Now:** the edge `NoteSwipe.Fire()` clears the pull at once then a 7 px `Bump` runs (the snap). Switching has two feels (Detent, Progress), vertical `SmoothScroll` (`Editor/Motion.cs`) has no overscroll.
- **Where / Risk:** `NoteSwipe` (pad and `MotionPreview`), `SmoothScroll` (pad editor, Settings page), Settings > Feel. Riskiest: vertical pull on TextEditor and ScrollViewer without breaking selection or caret scroll.
- **Source:** lab `tools/playground/a20-styles-lab.html`, `a20-compare.html`; locked values `docs/scroll-feel.md`.

## Plan
- **After:** horizontal: one feel, "Follow-through" (replaces Detent and Progress), exactly the `docs/scroll-feel.md` horizontal table; edge pull holds then springs home, no snap. Vertical: all four styles keep their own glide but gain the same overscroll at the top and bottom (band, hold, spring). Settings > Feel: "Scroll style" is "Vertical Scroll Style"; new "Horizontal Scroll Style" with one option; defaults Rate-matched and Follow-through. Preview shows both.
- **Must not change:** each vertical style's glide inside a note, caret and selection scrolling, Reduce motion (no glides or overscroll), the installed copy.
- **Test plan:** selftests: horizontal commit by projection, no one-frame jump at an edge, `notchGain` scaling, vertical band and spring return, Reduce motion off-path. Hand: dev copy, wheel and touchpad, first and last note, top and bottom of a long note, Settings preview.
- **Needs from you:** hand test.

## Files
- `product/AntiNotepad/UI/NoteSwipe.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/MotionPreview.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/Editor/Motion.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/Motion/MotionTokens.cs`
- `product/AntiNotepad/Motion/EdgeBand.cs` (new: band, spring, notch gain)
- `product/AntiNotepad/Model/Settings.cs`
- `product/AntiNotepad/SelfTest/ScrollFeel.cs` (new)
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest/Run.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/App.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
- `docs/scroll-feel.md`
- `docs/decisions.md`
- `tools/playground/a20-styles-lab.html`
- `tools/playground/a20-compare.html`

## Contracts
Add rows: edge pull never jumps in one frame; vertical overscroll holds then springs home.

## Approve
2026-11-15 (user): "proceed. Since we're setting rate-matched as default across the app, the overscroll logic should apply everywhere too".

## For Codex
Role: worker, because shared timing state across four surfaces. Never switch, merge or commit. Read `AGENTS.md`, `docs/scroll-feel.md` (the spec), the lab engine functions it names, then the Files.
- Build exactly the Plan's "After"; port the lab's behavior, not its page. Design numbers go in `Motion/` tokens.
- Build into `build/A20-overscroll-speed-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, delete the folder. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A20-overscroll-speed -Offline`.
- Never launch or kill AntiNotepad; never edit STATUS or this card. Offline checks only.
- Done means: Plan "After" holds; selftests exist and pass; build 0 warnings, `SELFTEST PASS`, gate passes.

Report `.coord/reports/A20-overscroll-speed.md`, nine short sections per `.coord/templates/item.md`.

## Check
2026-11-15, item chat: gate PASS at the branch tip (`tools/gate.ps1 -Offline`; build 0 warnings, SELFTEST PASS, 20 contract rows 19 pass 1 manual, tokens, format, scope all allowed; Size warn: PadView.cs 943->996). Bounds: within plan. Skimmed vertical edge code (`EdgeBand`, `SmoothScroll`): matches the spec, Reduce motion bypasses. Noticed: Settings key `Swipe` kept to store Follow-through (compat). Misses: none. Not run live; hand test next.

## Hand test
(after the build)
