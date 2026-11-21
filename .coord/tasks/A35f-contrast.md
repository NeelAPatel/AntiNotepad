# A35f-contrast: text and controls that meet WCAG contrast in both themes

Item card (`docs/loop.md`). Batch 4 of `A35-ux-pass`; step 2 after [A35e-targets-keyboard](A35e-targets-keyboard.md); branch `fix/A35f-contrast`, created from the A35e tip when A35e builds.

Tier: Medium · theme-tokens (palette colors and control paints)
State: approved (builds after A35e)

## Request
Parent chat (A35-ux-pass), 2026-11-21: D-3, D-5, D-7, D-2, D-6, plus a live measure for D-1/D-4. Findings: `.coord/reports/A35-ux-pass-s3.md`, `-s3d.codex.md`.

## Findings
- **Now (computed from `Palette.cs`, WCAG):**
  - D-3: primary button text is white or `#1A1A1A` by a brightness test (`SettingsParts.cs:398`). White fails on Amber 2.93, Red 3.91, Pink 4.12, Blue 3.26, Teal 3.07, Green 3.03. `#1A1A1A` fails on Red 4.45, Pink 4.22. Choosing the better of white or near-black passes six accents; Red and Pink need black (about 5).
  - D-5: resting field border (Faint at .5) is 1.38 to 1.47 on every palette; Ink at .55 passes 3:1 on all eight. Paper focus ring (Accent at .9) is 2.57; Sage 3.38.
  - D-7, D-2, D-6: as in s3 (toast action 1.28 on Dark Ink; save-failure detail 3.5 to 4.4; Paper toggle/dot 1.25 to 1.38).
  - D-1/D-4: only a worst-case model exists. A real measure needs a white and a black wallpaper under the live Clear tint: the user's go-ahead for desktop control, or the user's own screenshots.
- **Where / Risk:** `Kit.cs` field styles reach every text box; palette-only changes are exempt from motion.

## Plan
- **After:** lab shows each fix before/after in light and dark. Foreground picked by WCAG for every accent; field border Ink at .55; focus ring falls back to Ink when Accent is under 3:1; toast action and failure detail use contrast-checked roles; toggle off-state and slot dot states reach 3:1 and keep the width cue.
- **Test plan:** a selftest sweeps every palette and accent for each pair above (the check that would have caught D-3).
- **Gates:** gate offline with `-Base fix/A35e-targets-keyboard`; Codex review.
- **Needs from you:** lab approval; the D-1/D-4 measure (go-ahead or screenshots); hand test.

## Files
Fixed after the lab; likely `Theme/Palette.cs`, `UI/Kit.cs`, `UI/SettingsParts.cs`, `UI/PadBar.cs`, `UI/PadWindow.Layout.cs`, `SelfTest/UI.cs`, `SelfTest/Contracts.cs`, `docs/design-scale.md`.

## Approve
User, 2026-11-21, after the lab: "lab is approved" (one Approve for both items).
