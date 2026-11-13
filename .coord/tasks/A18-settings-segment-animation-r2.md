# A18-settings-segment-animation-r2: keep the glide when Settings rebuilds its rows

Rework round of `A18-settings-segment-animation` (`docs/loop.md`). Read the item card `.coord/tasks/A18-settings-segment-animation.md` first: its Plan, Files, Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Medium, same as the card.
Role: terra, because it is one targeted change in `Seg()` plus a selftest check.

## Why this round
Round 1 works for selectors that do not rebuild the page, but the item chat found two cases where the glide is lost. (1) Selectors set with `Set(..., rows: true)` (Bars, Show, Scrolling style, Swipe style, and others) rebuild the whole page through `Show(...)`, which creates a new `Seg` already on the chosen option, so nothing glides. (2) Changing the Theme calls `Restyle()` (palette change), which rebuilds every control, so the System / Light / Dark selector, the user's headline example, also snaps. The user approved the sliding pill and hover fade (2026-11-12, "yes with hover"); a glide that gets cut on rebuild is not what they approved.

## Change to the plan
Keep the highlight position across a rebuild. Suggested design (change it if you find something simpler that behaves the same): the `SettingsWindow` keeps a small field holding, per selector signature (the option values joined, for example `"system|light|dark"`), the last chosen value, the highlight bounds and `Environment.TickCount64`. When a new `Seg` is built and the stored entry has the same signature, was written within the last 1000 ms and has a different chosen value, the new highlight starts at the stored bounds (not at the new target) and glides to the new target once laid out, with the same 167 ms `Win11Enter` motion. Otherwise it starts on the chosen option as now. Update the entry when the highlight moves or lands. The hover fade and the reduced-motion snap stay as built. Reduced motion (find how `Anim` or other code checks it; round 1 may already do it) must still snap.

## Files
- `product/AntiNotepad/UI/SettingsParts.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
- `tools/playground/a18-segment-lab.html`

## Done means
Offline checks only (`docs/agents.md`).
- A rebuilt selector whose stored entry is fresh and differs starts at the old bounds; with no entry or a stale one it starts on the chosen option. Cover both with a `--selftest` check on the pure decision (no UI needed).
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A18-settings-segment-animation-r2 -Offline` passes. If the design snapshot hash changes, update it. Delete `build/A18-settings-segment-animation-check` and `-gate`/`-gate-data` folders only if the sandbox allows it; otherwise say so.

Report: `.coord/reports/A18-settings-segment-animation-r2.md`, in the card's report format.
