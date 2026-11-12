# A9-pinned-accent-ring: accent-colored ring around the window while pinned

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build; Codex reads the whole card and
follows "For Codex". Codex never edits this file; Check and Hand test are filled by the sub-session afterwards.

Tier: Medium · visible design, lab-first (rules: `docs/loop.md`)
Branch: `feat/A9-pinned-accent-ring` in `.claude/worktrees/feat/A9-pinned-accent-ring` (created at Approve)

## Request
2026-10-29 (user): "When the window is pinned (Ctrl+T), draw an accent-colored ring 2-4 px thick around the window's edge, so pinned is visible at a glance." Open then: exact thickness, whether it follows the rounded corners, theme accent or a fixed color, a fade on pin/unpin, how it works with the frameless window and its shadow.
2026-11-09 (user): confirmed this scope; numbered A9. Runs in its own worktree beside A10-hotkey-recorder and A11-settings-thread-count; each dev copy shows its own branch and launch time in the dev badge.

## Findings
- **Kind and why:** feature, visible design (lab-first); pinned state today is only a filled pin icon.
- **Now:** pin state is `Settings.Pinned` (`Model/Settings.cs:55`), toggled by `TogglePin` (`UI/PadWindow.Navigation.cs:183`, Ctrl+T at `UI/PadWindow.Keys.cs:93`). It sets `Topmost` (`UI/PadWindow.cs:79`, `PadWindow.Keys.cs:275`). The only visual cue is the toolbar pin filled with the accent (`UI/PadWindow.Layout.cs:204-206`). "Dim when pinned" fades the window to 0.72 (`PadWindow.Keys.cs:248`).
- **Where:** `UI/PadWindow.Layout.cs` (window chrome and badges), `Theme/` palettes (accent) and tokens (`ChromeTokens`/`Paint`/`Shape`), `Motion/` if a fade is chosen, `SelfTest/` for token and state checks.
- **Risk:** the ring must not steal clicks, resize hit areas or text space, and must not slow show/hide. Window corner radius and shadow come from the window chrome; not yet traced (Plan step 1).
- **Source:** master scout (grep only).

## Plan
- **Lab picks (user, 2026-11-09, lab `tools/playground/a9-ring-lab.html`, commit 350b542 and follow-up):** 4 px thick, rounded to the window outline (`Shape.Card`, Md 8), theme accent (`_palette.Accent`), 250 ms fade on pin and unpin, opacity .9 (`Paint.FocusBorder`).
- **After:** a pinned window shows a 4 px accent ring along its edge; unpinned shows none. The ring fades in and out over 250 ms (instant with Reduce motion).
- **Expected time:** about 1 hour (step A9b-pinned-ring-build; step A9a-pinned-ring-lab is done).
- **Must not change:** show/hide speed, window size, text area, shadow, the pin icon, Dim when pinned, hit testing.
- **Named exception:** 4 px is not a Stroke step (Thick is 2); it gets its own token `ChromeTokens.PinRingThickness`.
- **Test plan, code:** token values in `Theme/` and `Motion/` (Tokens gate); a selftest check that the ring's target opacity is 0 unpinned and `Paint.FocusBorder` pinned, and its brush is the palette accent.
- **Test plan, UI:** Ctrl+T shows the ring and a toast; Ctrl+T again fades it away; both themes; a different accent in the theme maker changes the ring; split view; drag and resize still work; toggle quickly (no flicker or stuck ring); Reduce motion on makes it instant; the ring does not cover the top-bar icons' clicks.
- **Gates:** build 0 warnings, selftest, Tokens, Format.
- **Needs from you:** hand test.

## Files
Only these files may change (exact paths; `tools/gate.ps1` reads this list).
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/PadWindow.cs`
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/Theme/ChromeTokens.cs`
- `product/AntiNotepad/Motion/MotionTokens.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`

## Contracts
- Pinned window shows the ring; unpinned does not (new row in `SelfTest/Contracts.cs`).

## Approve
2026-11-09 (user): "Approve all three". Scope as planned: lab step A9a first, build step A9b after the user approves the lab.
2026-11-09 (user), lab picks: "4px, Rounded Md8, Theme Accent, 250 ms Fade on pin and unpin." Treated as the lab approval.

## For Codex
Role: director (the director's launch order for this item; a Medium change across seven files).
Build the pinned accent ring in `product/AntiNotepad`. Follow `docs/coding.md`; offline checks only (build, `--selftest`, gate); never launch the app.
1. `Theme/ChromeTokens.cs`: add `PinRingThickness = 4` (comment: a named exception to the Stroke scale, 4 px). `Motion/MotionTokens.cs`: add `PinRingFade = 250`. Reuse `Shape.Card` (radius 8) and `Paint.FocusBorder` (.9 opacity); no new literals elsewhere.
2. `UI/PadWindow.Layout.cs`: add a field `_pinRing` (a `Border`, `IsHitTestVisible = false`, `Opacity = 0`, `BorderThickness = new Thickness(ChromeTokens.PinRingThickness)`, `CornerRadius = new CornerRadius(Shape.Card)`). In `BuildLayout`, add it to `root` last (above the chrome layers) so it paints over everything but takes no input. It must not change layout: the Border fills the grid cell and has no margin or padding effect on siblings.
3. `UI/PadWindow.cs` `ApplyAppearance` (or a small `ApplyPinRing(bool animate)` method placed in `PadWindow.Layout.cs`): set `_pinRing.BorderBrush = new SolidColorBrush(_palette.Accent)`, so a changed accent or theme recolors it. Add `internal static double PinRingOpacity(bool pinned) => pinned ? Paint.FocusBorder : 0;` for the selftest.
4. Fade: `ApplyPinRing(animate)` runs `Anim.Run(_pinRing, MotionTokens.PinRingFade, t => _pinRing.Opacity = from + (to - from) * t)` from the current opacity to `PinRingOpacity(Settings.Pinned)`; keep the `Anim.Handle` in a field and cancel it first so quick toggles never stick. Skip the animation (set the value) when the target already equals the current opacity, and on first build.
5. `UI/PadWindow.Keys.cs` `HandleSettingsChanged`: next to `Topmost = Settings.Pinned;` call `ApplyPinRing(animate: true)`; the constructor/first `ApplyAppearance` sets the initial state without animation. Do not touch the pin icon, `DimWhenPinned` or the toast.
6. `SelfTest/UI.cs`: small check that `PinRingOpacity(false) == 0`, `PinRingOpacity(true) == Paint.FocusBorder`, and that a built pad's `_pinRing` is not hit-test visible (use whatever pad-construction pattern the neighbouring UI checks use; keep it a capability check). `SelfTest/Contracts.cs`: add a contract row "pinned-accent-ring" (promise: pinned window shows the accent ring, unpinned does not) with that check and a manual line for the UI test.
Done means (offline only): build with 0 warnings into `build/A9-check`, `--selftest` passes there, `tools/gate.ps1 -Offline` passes (Scope, Tokens, Format), then delete `build/A9-check`. Write a short report to `.coord/reports/A9-pinned-accent-ring.md` with sections Changed, Checks run, Not checked (anything needing the screen).
Report any file you needed beyond the Files list instead of editing it.

## Check
- Gate (round r2): PASS (Build 0 warnings, Selftest with 16 contract rows, Seam, Tokens, Format, Scope). Size WARN: `PadWindow.Layout.cs` 606 -> 636 lines, same chrome-layout job, no split planned.
- Codex review (`.coord/reports/A9-pinned-accent-ring-r2.review.md`): VERDICT accept, no proven findings.
- Plan vs done: ring 4 px, Shape.Card radius, palette accent, Paint.FocusBorder opacity, 250 ms cancellable fade, click-through, instant under Reduce motion: **within plan**. Pin icon, Dim when pinned, toast, shadow, window size: untouched.
- `SelfTest/Design.cs` hash refresh and `tools/playground/a9-ring-lab.html` in scope: **justified** (the ring adds one Border row per palette; the lab page is the approved A9a deliverable); both authorized by the r2 brief.
- Coding standard: new/touched files have header comments; no gap found.
- Misses: round 1 selftest hang. Found at: first Codex run's gate. Cause: `test` (new check ran Avalonia setup differently from the Design dump and stalled a later contract). Caught by: the gate selftest. Earlier catch: a rule that new UI selftests copy the Design.cs setup. Lesson: a PadWindow selftest must install the Fluent theme and use a loaded, seeded store, and run last among contracts.
- Noticed: the ring check must stay last in `ContractsChecks` (shared Avalonia state); worth a general fix someday (proposal only).

## Hand test
2026-11-09 (user): "everything works, but clicking the pin button specifically also triggers the Toolbar / Pager's Fade while Typing which is not expected."
Triage: the ring works. The chrome fade is existing behavior, not from A9: a Pinned change carries the Appearance and Persistence flags, so `HandleSettingsChanged` calls `Edited()`, which calls `SleepChrome(soon: false)` (the same fade typing causes). A9 only added the ring update beside `Topmost`. Awaiting the user's call: fix here as round r3 or log as a separate idea.
2026-11-09 (user), re-test after round 3: "All checks pass"
Round 3 (`A9-pinned-accent-ring-r3`) made pinning, by button, menu or Ctrl+T, leave the toolbar and pager alone; the user's scope change is in its brief. Check for r3: gate PASS (Codex run), diff read by the builder instead of a second Codex review (small change inside the accepted design).
