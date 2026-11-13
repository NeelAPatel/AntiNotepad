# A17-settings-toggle-animation: animate Settings toggle switches

Item card (`docs/loop.md`). Claude fills Request to Approve before any build; Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB.

Tier: Low - ui-motion, one shared control
State: approved
Branch: `feat/A17-settings-toggle-animation` in `.claude/worktrees/feat/A17-settings-toggle-animation` (created at Approve)

## Request
2026-11-12, user (pasted list, Animations, Settings), verbatim: "Settings toggle switches change with no transition; add one"

Then: "start 1 and 2 in parallel" (with A18-settings-segment-animation; the user also asked for these two first on the list). Reuse the Windows 11 motion values in `docs/decisions.md` (167 ms in, 83 ms out, `Win11Enter`/`Win11Exit`, tokens in `Motion/MotionTokens.cs`) rather than inventing timings; the user cannot judge motion from numbers, so show it in a lab page (`tools/playground/`, served on localhost) or the dev copy. Settings controls live under `product/AntiNotepad/UI/Settings/` and `UI/Kit`.

## Findings
- **Kind and why:** polish; the user's animation list (2026-11-12).
- **Now:** one shared `Toggle(on, set, name)` in `UI/SettingsParts.cs` (~line 214): 40x20 track, 12 px knob, On/Off word. `Paint()` sets knob side, colors, track fill and border at once, so a click snaps. About 15 uses in `SettingsWindow.cs`. `Kit.Toggles` (language chips) and `Seg` (A18) are other controls.
- **Where / Risk:** only `Toggle`; risk is the knob staying inside the track and Reduce motion (`Anim.Reduced`) snapping.
- **Source:** own scout; lab `tools/playground/a17-toggle-lab.html`, option E, from Microsoft's WinUI ToggleSwitch template.

## Plan
- **After:** Windows 11 toggle. Knob 12 px at rest, 14 px hover, 17x14 px pressed (each 83 ms, Win11Enter); on click the knob glides to the other side (167 ms, Win11Enter) and the track fill and outline cross-fade (83 ms). On/Off word unchanged. Reduce motion snaps (no hover/press resize either). Right edge of the on-knob stays at 36 px of the track (left = 36 - width); off-knob left = 3.
- **Expected time:** under an hour.
- **Must not change:** click behavior, `set()` timing, accessibility name, colors, `Seg`, `Kit.Toggles`.
- **Scope questions:** None (option E chosen).
- **Test plan:** `--selftest` passes. Hand check in dev copy: every Settings page, hover, press, release, rapid double click, Reduce motion on and off.
- **Gates:** `tools/gate.ps1`: build, format, tokens (new sizes and durations go in `Motion/MotionTokens.cs`), selftest.
- **Needs from you:** hand test.

## Files
- `product/AntiNotepad/UI/SettingsParts.cs`
- `product/AntiNotepad/Motion/MotionTokens.cs`
- `product/AntiNotepad/SelfTest/Design.cs` (design snapshot hash only; dump diff confined to the toggle track)

## Contracts
None new.

## Approve
2026-11-12, user: "Approve" (option E, Windows 11 toggle).

## For Codex
Role: terra, normal UI feature in one method plus tokens. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then the Files; re-read before editing.
- Build exactly the Plan's "After" for the Files above; no drive-by changes. Use `Win11Enter`, `Win11Fast`, `Win11Faster` from `Motion/MotionTokens.cs`; add named tokens for 12/14/17 sizes. Look at how other animations in `Motion/` run (`Anim`) and reuse them. If the Findings are wrong, stop and report that.
- Build into `build/A17-settings-toggle-animation-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, delete the folder. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A17-settings-toggle-animation -Offline`.
- Never launch or kill AntiNotepad, never run `--quit`, never edit `.coord/STATUS.md` or this card. Offline checks only.
- Done means: toggle behaves as in "After"; build 0 warnings, `SELFTEST PASS`, gate passes.

Report `.coord/reports/A17-settings-toggle-animation.md`, short: 1 Summary; 2 Change list; 3 Behavior changes; 4 Out-of-scope files; 5 Plan deviations; 6 Contracts; 7 Lens review; 8 Checks run; 9 Hand checks for the user.

## Check
(pending)

## Hand test
(pending)
