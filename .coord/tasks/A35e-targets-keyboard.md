# A35e-targets-keyboard: bigger click targets and a keyboard path for every control (with A35f-contrast as step 2)

Item card (`docs/loop.md`). Batches 3 and 4 of `A35-ux-pass`, one lab. Step 2 is [A35f-contrast](A35f-contrast.md), stacked on this branch.

Tier: Medium · settings-sections (touches Settings controls, pad bar, recorder)
State: hand-tested
Branch: `fix/A35e-targets-keyboard` in `.claude/worktrees/fix/A35e-targets-keyboard` (from and back into `docs/A35-ux-pass`)

## Request
Parent chat (A35-ux-pass), 2026-11-21: batch 3 (B-5, A-2, A-5, B-4, B-1, B-2, A-3, B-7), deferred A-9, C-5, A-7. Findings: `.coord/reports/A35-ux-pass-s3.md`.

## Findings
- **Kind and why:** polish and keyboard access; under-sized targets and pointer-only controls break T1/T2/KB1.
- **Now (code, this tip):**
  - B-5: segments and text buttons are about 22 px tall, 2 px apart (`SettingsParts.cs:244`, `Kit.cs:201`).
  - A-2/A-5: unselected slot dots are 16 px wide buttons, 4 px apart; the selected one is 24 (`PadBar.cs:228`), so dots shift on selection.
  - B-4/B-1: palette chips and accent swatches are `Border`s with `PointerPressed` only (`SettingsParts.cs:471-508`): no Tab, no focus ring.
  - B-2: the recorder marks every key handled (`HotkeyRecorder.cs:151`); Clear is mouse-only.
  - A-3: no key moves focus between split panes.
  - B-7: `Show`/`AfterSet` rebuild the rail and page; focus is lost (`SettingsWindow.cs:158, 230`).
  - A-9: float bar Delete sits ~13 px from the arrows; C-5: the menu action row puts Delete beside Paste (`PadMenu.cs:116`); A-7: hidden bars give no cue.
  - `Kit.Plain` neutralises Fluent's `:focus-visible` (`Kit.cs:243`) and draws no ring of its own, so keyboard focus may be invisible on every Plain button. Confirmed in the live check.
- **Where / Risk:** `Kit.Plain` is used app-wide; a ring or size change reaches every button. Pad bar width at 320 px.
- **Source:** s3 report and its Codex reads.

## Plan
- **After:** lab `tools/playground/a35ef-lab/` (before/after, light and dark, 320 px) shows: 32 px segments/buttons with 4 px gaps; 24 px equal dot targets with a fixed 16 px dot inside (nothing shifts); 32 px swatch targets; chips and swatches as real buttons; one shared focus ring on `Plain`; recorder Backspace clears; F6 swaps pane focus; Delete moved away from Paste; Delete/Clear gap; an edge cue for hidden bars.
- **Commits:** (1) targets and sizes; (2) focus ring and chips/swatches as buttons; (3) recorder, F6, focus restore; (4) A-9, C-5, A-7.
- **Must not change:** motion; editor text position; show/hide speed; `docs/design-scale.md` contract (A35c).
- **Scope questions:** F6 as the default key; Backspace clears; the cue style (lab).
- **Test plan:** selftests: min target size and dot pitch from tokens; chips and swatches focusable; recorder Backspace; focus survives a page rebuild. Hand: Tab through every screen, 320 px.
- **Gates:** `tools/gate.ps1 -Task A35e-targets-keyboard -Offline -Base docs/A35-ux-pass`; tokens must not rise; Codex review.
- **Expected time:** over an hour; step 2 follows.
- **Needs from you:** the lab approval; the hand test.

## Files
Fixed after the lab; Codex may not touch others.
- `product/AntiNotepad/Theme/ChromeTokens.cs`
- `product/AntiNotepad/UI/Kit.cs`
- `product/AntiNotepad/UI/PadBar.cs`
- `product/AntiNotepad/UI/PadMenu.cs`
- `product/AntiNotepad/UI/HotkeyRecorder.cs`
- `product/AntiNotepad/UI/SettingsParts.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/UI/Shortcuts.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest/Bar.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
- `docs/shortcuts.md`
- `tools/design-tokens-baseline.tsv`

## Contracts
- New rows: target sizes, focusable chips, recorder Backspace, focus survives rebuild.

## Approve
User, 2026-11-21, after the lab (`tools/playground/a35ef-lab/`): "lab is approved". Defaults taken with it: F6 moves pane focus; Backspace clears in the recorder; hidden-bar cue is the 24x2 handle (a); menu row order Delete, Cut, Copy, Paste.

## For Codex
Two runs: `.coord/tasks/A35e-targets-keyboard.build-1.md`, `-build-2.md`. Claude commits between runs.

## Check
Gate PASS offline (base docs/A35-ux-pass) apart from scope on the lab files, which come off the branch at commit review. Codex review: rework, four bugs fixed (`.coord/logs/A35e-targets-keyboard.reviewer.last.md`, round 2). Rounds 3 and 4 fixed hand-test findings. A-3 (F6 pane focus) was dropped: the user said split view gets a rework later. Lab kept outside the repo.

## Hand test
User, 2026-11-21, blue dev copy, round 1: F6 "Didn't work. Split view will need to be carefully reworked later"; edge line "rounded + 2~4px away from the edge"; theme boxes equal width; expander hover zone; light palette not selectable in dark mode; Settings flashes on accent change; no focus ring on the side tab bar; ring lost after the shortcut recorder; float bar spacing and stray separator; Standard float bar poorly designed; "Shadow issues". Rounds 3 and 4 fixed these (float bar: "Type A is good, show the side items + separator when they exist").
User, 2026-11-21, after rounds 3-4: "but everything else works"; after the palette wrap fix: "Looks right"
