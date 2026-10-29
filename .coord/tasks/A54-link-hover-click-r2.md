# A54-link-hover-click-r2: fix the hand cursor override and make Ctrl+click independent of AvaloniaEdit routing

Role: director (gpt-6.1-sol medium). Product `product/AntiNotepad`. Branch `feat/A54-link-hover-click` (checked out; never switch/commit).
Tier: Low. Read `AGENTS.md`, the base brief `.coord/tasks/A54-link-hover-click.md` (Files, constraints, report format apply) and the round-1 report.
Report: `.coord/reports/A54-link-hover-click-r2.md` (same sections). Gate: `-Task A54-link-hover-click-r2 -Offline`.

## Why rework
Round 1 added only tests and did not find the cause; the user-visible bug stands. Manager's code reading found a real defect:
`PadView.OnViewMoved` (`Editor/PadView.cs` ~576) assigns `view.Cursor = Ibeam` on every PointerMoved when not over a chip.
It is an instance handler on the TextView, so it runs AFTER AvaloniaEdit's class handler that calls `GlyphElement.OnQueryCursor`
(`Editor/Elements.cs:20`), overwriting the Hand with Ibeam. This also affects checkbox glyphs. Ctrl+click not firing in the real
editor is likely the same family (routing/ordering in the real TextArea); round 1's TextView-only probe cannot see it.

## Do
1. Fix the cursor: `OnViewMoved` must not overwrite the cursor when the pointer is over a `GlyphElement` with `Hand = true`
   (hit-test the visual line elements under the pointer, e.g. `view.GetVisualLineFromVisualTop` + element at the x/offset, or
   record a "hand over" flag set by `GlyphElement.OnQueryCursor` and consumed by `OnViewMoved` - pick the simplest that is
   order-independent). Chips keep Hand; elsewhere Ibeam as today; leaving a link restores Ibeam.
2. Make Ctrl+left-click robust: in `OnViewPressed` (Tunnel, runs first), if the left button is pressed with Control and the
   pointer is over a shortened-link `GlyphElement`, invoke its `Pressed` and mark `e.Handled`, so it no longer depends on
   AvaloniaEdit's selection handler order. Plain click must still place the caret (do not handle it). Link opens exactly once
   (guard against `GlyphElement.OnPointerPressed` also firing: handled event must stop it).
3. Keep the round-1 selftest and contract row; extend the selftest where headless-provable (e.g. the hit-test helper returns the
   link glyph for a point over it and null outside; opener stubbed, never open a browser). Update the contract row if needed.
4. Files: same four as the base brief. Cleanup: delete `build/A54-link-hover-click-check` and `build/A54-link-hover-click-probe`
   if your sandbox allows; otherwise say so (manager will).

## Done means
Build 0 warnings, SELFTEST PASS, gate passes, report written with the actual cause stated.
