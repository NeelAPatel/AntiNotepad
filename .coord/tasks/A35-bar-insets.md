# A35-bar-insets: chrome design tokens, new bar geometry, and a pill-material lab

Role: director (gpt-6.1-sol medium). Delegate code work to gpt-6-luna xhigh subagents, one area at a time, and review
their diffs yourself before you finish.
Branch: `feat/A35-bar-insets` (this worktree). You cannot commit: leave changes in the working tree; the manager reviews and commits.
Product: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10). Read `AGENTS.md` and the top of `docs/decisions.md` first.

## Spec (user-approved, do not reinterpret)
Read `.coord/handoff/A35-bar-insets/approved-values.md` (the numbers) and open `.coord/handoff/A35-bar-insets/mockup.html`
(the approved concept; its `geo()`, `shadeCss()`, `pill()` and `topBar()` functions encode the geometry). All values are
app px (DIP). In short:
- Top bar, all three bar styles (docked, overlay, float): Hide button 32x32 at 4 from the top and right edges; both clusters
  centered on y 20; badges exactly 20 tall (so 10 from the top); leftmost item 10 from the left edge; docked bar row 40.
- Top bar backdrop in overlay and float: never solid. Blur behind the note text, tinted with the palette Canvas at 50%,
  blur radius 10, dense to 40, fully clear by 52, midpoint 50%. Fallback if blur is not possible: the feathered gradient
  (Canvas at 94% at the edge, 86% at 40, 50% point midway, 0% at 52).
- Text edge fade (`PadView.UpdateEdgeFade`): band 12 (was 40).
- Pager pill: 32 px buttons, 2 px inset on all four sides inside a 1 px border = 38 tall; pill corner radius 12; 4 from the
  bottom edge (docked, overlay); float: popup 8 below the window.
- Hover-fill corner radii: top bar icon buttons 8, pager arrow buttons 8, pager count 8, pager dots 9.
- Bottom bar per style:
  - Docked: the pager gets its own empty strip like the top bar row (4 + 38 + 4 = 46). Note text never shows in that strip,
    only the 12 px text fade above it.
  - Overlay: no full-width bottom shade any more. The pill alone floats over the text, wearing the window's material.
  - Float below: the pill popup wears the window's material.
- The pill's material is the editor's material (Settings > Material: acrylic | mica | solid). No separate pill setting.

## Part 1: a design system for chrome (required)
The user wants a design system, not per-control numbers. Add one token source, e.g. `product/AntiNotepad/UI/Chrome.cs`
(static class or record of named constants): edge inset (4), bar item inset (10), bar row height (40), icon button size (32),
hover radii, pill inset/border/radius/bottom/float gap, pager strip height (derived), text fade band, top backdrop stops,
blur radius and tint. Derived values are computed from tokens, never retyped. Replace the literals in
`UI/PadWindow.Layout.cs`, `Editor/PadView.cs` (`UpdateEdgeFade`), `UI/Kit.cs` (`IconButton`/`Plain` radius defaults as used by
the bar and pager), and the couplings that key off the old sizes: `Editor/Elements.cs` `TopGapGenerator.Gap` (30),
`_divider` bottom margin (52), `_toast` bottom margin (48), `_topLayer`/`_bottomLayer` heights, `TopZone`/`BottomZone`
where they assume the old bar sizes. Settings window and menus are out of scope unless they share these exact controls.

## Part 2: pill material lab (feasibility, best to worst)
Implement and compare, behind a dev-only lab switch, three ways to give the pill (overlay and float) its material:
1. **Acrylic (best)**: the pill is its own small native window (the float style already uses a non-overlay `Popup`) with real
   DWM acrylic (e.g. `TransparencyLevelHint` AcrylicBlur on the popup's TopLevel, or `DWMWA_SYSTEMBACKDROP_TYPE`
   transient-window), so Windows blurs the pad and its text behind it. In overlay it sits over the pad's bottom edge and must
   follow the pad when moved/resized, show/hide with it (including the app's own show fade), keep z-order just above the pad,
   never take focus or activation, never appear in Alt+Tab/taskbar. Report what corner radius is achievable (DWM offers 8 or
   4; the spec says 12) and whether the 1 px border and hover fills draw correctly on it.
   When Settings > Material is mica, test whether the popup can show Mica; if not, report what it falls back to.
2. **Blur behind (second)**: in-window blur of the note text under the pill (and the same technique for the top bar
   backdrop): e.g. a VisualBrush of the editor area with a `BlurEffect`, clipped/masked to the pill shape (and to the feathered
   top band), tinted with Canvas at 50%. Must update live while text scrolls and the user types.
3. **Solid (fallback)**: the pill uses the palette Surface fully opaque (today it is Surface at 86%).
Also test the top bar's blur-behind band (Part 2.2 technique) against the feathered-gradient fallback.

Lab switch: dev-only (only when `ANTINOTEPAD_DEV=1`), e.g. a "Lab" group at the bottom of Settings > UI with
"Pill material: Follow material / Acrylic window / Blur behind / Solid" and "Top backdrop: Blur behind / Feathered gradient".
Switching must apply live without a restart. The installed (non-dev) build must behave as: pill follows Settings > Material
using the best variant that works, top backdrop uses blur if it works, otherwise the gradient; leave that default wiring in
place but clearly marked so the losing variants can be deleted after the user picks.

## Hard constraints
- Do NOT change the pad window's frame styles, DWM attributes, corner/shadow handling or show/hide animation code paths
  (a past attempt broke corners, shadow and all three bar styles; see `docs/decisions.md` 2026-10-25). New popup windows may
  set their own DWM attributes.
- Speed of show/hide is the product's priority: measure it. Log, in the dev log via `Program.Log`, hotkey-to-focus ms (already
  logged) for each lab variant, plus the cost of one blur-behind frame (render ms) while scrolling and while typing.
- First-party/maintained libraries only; prefer none.
- US spelling in UI text. Match the surrounding code style and comment density.
- Never launch or kill AntiNotepad, never run `--quit`. Build into `build/A35-bar-insets-check`
  (`AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A35-bar-insets-check -p:AntiNotepadDev=true`),
  run `--selftest` from there with `ANTINOTEPAD_DATA` set to a folder under `build/`, then delete the check folder.
- Add one small `--selftest` check that the token-derived geometry holds (e.g. pill height = button + 2*inset + 2*border = 38,
  docked pager strip = 46, Hide button rect 4 px from top/right in a 480-wide layout if testable headless; keep it cheap).

## Done means
- 0 build warnings introduced, SELFTEST PASS.
- Report at `.coord/reports/A35-bar-insets.md` (in this worktree), short and structured:
  - files changed and the token list;
  - for each lab variant: works / partly / no, with the exact technique, limits (corner radius, border, focus, z-order,
    move/resize lag, Mica behavior), and measured costs (hotkey-to-focus ms, blur frame ms);
  - your recommendation for the shipped default per Material setting, and which code to delete once the user picks;
  - anything you could not verify without seeing the screen (the manager checks visuals in the dev copy).
