# A22-wrap-hanging-indent-r2: format gate, result chips on wrapped lines, review fixes

Rework round of `A22-wrap-hanging-indent` (`docs/loop.md`). Read the item card `.coord/tasks/A22-wrap-hanging-indent.md` first: its Plan, Files, Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: High (same as the card)
Role: terra, because known fixes across a few files, no unclear cause

## Why this round
Review (2026-11-17) found:
1. MEDIUM, Format gate FAILS: `SelfTest/Editor.cs` lines ~199-245, 46 whitespace problems on changed lines (one-line triple `foreach`). The r1 report said format passed; it did not. Fix with `dotnet format --include product/AntiNotepad/SelfTest/Editor.cs` or nest the loops normally.
2. MEDIUM, result chips can overlap text on wrapped lines: `Editor/Renderers.cs` ~line 265-268 uses `row.WidthIncludingTrailingWhitespace` of the last row as `TextEnd`; that ignores the continuation indent. Use the public X helper (`visual.GetTextLineVisualXPosition(row, end)`) so TextEnd includes the indent. Add a selftest check: a chip line whose last row is a continuation has TextEnd past the indented text end.
3. LOW, `Rendering/TextView.cs` ~line 1247 (inline object arrange) omits the indent: patch it with an `// AntiNotepad patch A22:` marker, listed in `PATCHES.md`, and the drift contract updated.
4. LOW, perf (show/hide speed is the priority): `WrapIndent.GetTextOffset` allocates and runs a regex for every visual line built. Run it only when the first row ends before the line end (the line actually wraps).
5. LOW, add selftest cases: a tab in the prefix, a folded line, an indent over half the width (falls back to none, no crash). If tab stops restart at the row origin and that is wrong in the layout, report it and do not widen the patch.

## Change to the plan
None.

## Files
- `product/AntiNotepad/Editor/Renderers.cs`
- `product/AntiNotepad/Editor/WrapIndent.cs`
- `product/AntiNotepad/SelfTest/Editor.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `third_party/AvaloniaEdit/Rendering/TextView.cs`
- `third_party/AvaloniaEdit/PATCHES.md`

## Done means
Offline checks only (`docs/agents.md`).
- Findings 1-5 fixed (or 5 reported), format clean on changed lines, chip check passes.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A22-wrap-hanging-indent-r2 -Offline` passes except the known Scope quirk (directory entry vs exact paths); run it with `pwsh`, not Windows PowerShell. Remove `build/A22-check` if policy allows; else say so.
- Never touch `PadWindow*`, Settings or Store files; do not edit STATUS or the cards; never launch the app.

Report: `.coord/reports/A22-wrap-hanging-indent-r2.md`, in the card's report format.
