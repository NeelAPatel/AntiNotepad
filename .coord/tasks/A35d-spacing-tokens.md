# A35d-spacing-tokens: every spacing, size, stroke and corner value comes from a named token

Item card (`docs/loop.md`).

Tier: Medium · settings-sections (corner override restyles Fluent controls app-wide; Settings rows move)
State: hand test
Branch: `refactor/A35d-spacing-tokens` in `.claude/worktrees/refactor/A35d-spacing-tokens` (from and back into `docs/A35-ux-pass`)

## Request
User via the A35-ux-pass chat, 2026-11-20: "kick off anything else you want". Batch 5 of the A35-ux-pass audit: spacing tokens and value snaps.

## Findings
- **Kind and why:** polish; spacing is only a document, so nothing stops drift.
- **Now:** `Theme/` has no spacing tokens; 97 on-scale and 39 off-scale spacing literals, 20 sizes off the 4 grid; pens 1.1/1.4/1.8; radius 9 (`Renderers.cs:101`); about 60 buttons get Fluent's radius 3, not `Shape.Control` 4 (R-1); `PagerDotButtonHeight` is 32, doc says 20; 10 tokens missing from `docs/design-scale.md`. Scanner counts `0` in `new Thickness(...)` (114 hits), misses pens and drawn radii.
- **Where / Risk:** 16 product files, `Theme/`, Fluent setup. Risk: the corner override reaches every Fluent control reading `ControlCornerRadius`.
- **Source:** `.coord/reports/A35-ux-pass-s1.md`, `-s3.md` (Codex audits done in A35-ux-pass).

## Plan
- **After:** `Theme/Space.cs` (Nudge 2, Xs 4, S 8, M 12, L 16, Xl 24, Xxl 32) and stroke tokens (Thin 1, Thick 2); `ChromeTokens` spacing values alias steps; named size tokens for on-grid sizes; rows in [the snap list](../reports/A35d-spacing-tokens-snaps.md) applied as snap, derive or exception; one shared Fluent setup sets `ControlCornerRadius` to `Shape.Control`, used by the app and both selftest setups; scanner skips `0`, counts pens and drawn radii, self-check fixtures for each; baseline lowered; `design-scale.md` gets the 10 tokens, `PagerDotButtonHeight` 32, the new exceptions.
- **Commits:** (1) tokens and on-scale migration, `DesignHash` byte-identical; (2) scanner and baseline; (3) snaps, R-1, strokes, radius 9, new `DesignHash`; (4) doc drift.
- **Expected time:** over an hour (waits for A35b).
- **Must not change:** editor text position; toggle knob geometry; window show/hide speed; the design contract doc (A35c-design-contract).
- **Scope questions:** which snap-list rows to apply (asked at Approve).
- **Test plan:** commit 1 keeps the pinned design hash; commit 3 dump diff matches the snap list; scanner self-check. Hand: Settings (nav, rows, dropdown, swatches, corners), search, help, completion, save-failure card, fold highlight.
- **Gates:** `tools/gate.ps1 -Task A35d-spacing-tokens -Offline` (build, selftest and hash, format, tokens ratchet); Codex review.
- **Needs from you:** Approve the snap list; the hand test.

## Files
- `product/AntiNotepad/Theme/Space.cs`
- `product/AntiNotepad/Theme/Shape.cs`
- `product/AntiNotepad/Theme/Typography.cs`
- `product/AntiNotepad/Theme/ChromeTokens.cs`
- `product/AntiNotepad/Program.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/Editor/Completion.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/Editor/Renderers.cs`
- `product/AntiNotepad/Platform/TaskbarPresence.cs`
- `product/AntiNotepad/UI/HelpOverlay.cs`
- `product/AntiNotepad/UI/HotkeyRecorder.cs`
- `product/AntiNotepad/UI/Kit.cs`
- `product/AntiNotepad/UI/MotionPreview.cs`
- `product/AntiNotepad/UI/NoteSwipe.cs`
- `product/AntiNotepad/UI/PadBar.cs`
- `product/AntiNotepad/UI/PadMenu.cs`
- `product/AntiNotepad/UI/PadWindow.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/UI/SearchOverlay.cs`
- `product/AntiNotepad/UI/SettingsParts.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `tools/check-design-tokens.ps1`
- `tools/design-tokens-baseline.tsv`
- `docs/design-scale.md`

## Contracts
- Design snapshot hash (`SelfTest/Design.cs`): unchanged by commit 1, re-pinned once by commit 3.

## Approve
User, 2026-11-21, after the lab (`tools/playground/a35d-lab/`): "Approve"

## For Codex
Four runs, one step each: `.coord/tasks/A35d-spacing-tokens.build.md`. Claude commits between steps.

## Check

## Hand test
