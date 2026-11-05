# A4b-design-dump-r2: implement the dump (round 1 stopped at investigation)

Rework round of `A4b-design-dump` (`docs/loop.md`). Read the item card `.coord/tasks/A4b-design-dump.md` first: its Plan, Files,
Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Low (same as the card)
Role: terra, because it is the same job and needs the same care

## Why this round
Round 1 wrote only a report: no code. It raised three worries; here are the decisions so you can build now.
- Handlers on `Completion.OcrRequested` / `PlatformSettings.ColorValuesChanged`, canceled close, `Quit` path: ignore them.
  Never call `Close()`, `Quit` or `Show()`. Build the windows, read values, drop the references. Hidden, never-shown
  windows left behind are acceptable: `--selftest` and `--design-dump` are short-lived processes that exit afterwards.
  Run the DESIGN check as the LAST selftest check, so leftovers cannot touch other checks.
- Process isolation: not needed. Build both dump passes in the same process.
- `PadMenu.Show` opens a popup; do not call it. Its `private static Border BuildMenu(IReadOnlyList<MenuEntry>, Palette,
  Settings, Action)` is reachable from SelfTest by reflection (`BindingFlags.NonPublic | BindingFlags.Static`). Use that,
  with a small sample entry list copied from how `PadWindow` builds its menu (read it; do not change product code). If
  reflection proves impossible, leave the menu out of the dump and say so under Plan deviations; do NOT edit `UI/`.
- If after a real attempt a window truly cannot be constructed offline without a product change, stop and report exactly
  what failed (exception text), still without editing `UI/`, `Editor/` or `Theme/`.

## Change to the plan
None.

## Files
None added beyond the card's Files.

## Done means
Offline checks only.
- As the card's "Done means": `--design-dump <file>` works for all 8 palettes (two runs byte-identical); `--selftest`
  prints `DESIGN dump <hash> <line count>`; the one-value-change failure proof is done once and reverted.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A4b-design-dump-r2 -Offline` passes.

Report: `.coord/reports/A4b-design-dump-r2.md`, in the card's report format.
