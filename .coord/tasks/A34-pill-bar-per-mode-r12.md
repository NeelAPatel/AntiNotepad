# A34-pill-bar-per-mode-r12: bar snapshot must not read slot bodies

Role: terra at CODEX_EFFORT=medium (one proven finding, known place; docs/agents.md). Work only in this worktree. Never commit, never launch AntiNotepad, no ports. Set `AVALONIA_TELEMETRY_OPTOUT=1`. Build into `build/A34-check`, run `--selftest` there; delete the folder after if the policy allows, otherwise say so.
Read `.coord/reports/A34-pill-bar-per-mode.review2.md` finding 1.

## Why this round
Lens review of r10 and r11 returned `VERDICT: rework`, one proven major finding: `BarContextFor` (`UI/PadWindow.Bar.cs`) builds `BarSlotContext(note.IsEmpty, note.Title)` for all nine slots on every `UpdateChrome`. `Note.Title` can read the note body from disk for slots whose title is not cached, and `UpdateChrome` runs on stack navigation and in `CloseOverlay` on the hide path (`HideResident`), where show/hide speed is the app's priority. Before A34, titles were read only when the slot tooltips were built.

## Do
1. Make the slot title lazy: `BarSlotContext` carries `IsEmpty` and a `Func<string> Title` (or an equivalent lazy value) that is evaluated only when `PadBar` builds the slot dots' tooltips, which only happens when the bar shows slot content (view kind Slot). A stack view must not call any slot's `Title`. Keep tooltip text identical to today ("Slot i · title" / "empty").
2. Check `note.IsEmpty` is cheap (no body read for a never-loaded slot); if it is not, use the cheapest equivalent flag the model already exposes and say which.
3. Self-test in `SelfTest/Bar.cs`: building a bar context and rebuilding a PadBar in stack view invokes zero title resolvers (counter in a fake `BarSlotContext`); in slot view the tooltips contain the titles.
4. No other change. The design dump must not change; if it does, stop and report (do not edit `SelfTest/Design.cs`).

## Done means (offline only)
Build 0 warnings, `--selftest` passes, `pwsh -NoProfile -File tools/gate.ps1 -Task A34-pill-bar-per-mode -Offline` passes. Report `.coord/reports/A34-pill-bar-per-mode-r12.md` with sections Change list, Behavior changes, Plan deviations, Gate result. Final message 4 lines max.
