# A54-link-hover-click-r3: every URL is Ctrl+clickable, not only shortened ones

Role: director (gpt-6.1-sol medium). Product `product/AntiNotepad`. Branch `feat/A54-link-hover-click` (checked out; never switch/commit).
Tier: Low. Read `AGENTS.md`, the base brief `.coord/tasks/A54-link-hover-click.md` (Files list, constraints, report format apply),
the r2 brief and `.coord/reports/A54-link-hover-click-r2.md`.
Report: `.coord/reports/A54-link-hover-click-r3.md` (same sections). Gate: `-Task A54-link-hover-click-r3 -Offline`.
Files: same four as the base brief.

## Why
User hand test (2026-10-29): after r2, Ctrl+click and the hand cursor work on a SHORTENED link only. Short URLs (<= 34 chars, e.g.
`https://google.com`), any URL while the caret is on its line (shown in full), and any URL when Shorten links is off are plain text,
so they are not clickable. User decision: "do it in A54" - every URL in the note must be Ctrl+clickable.

## Do
1. Add an order-independent URL hit test in `PadView` for plain (non-glyph) URL text: map the pointer point to a document offset
   (e.g. `TextView.GetPosition(point + ScrollOffset)` / `GetPositionFloor`, mind that the pointer must be over the text, not past
   line end) and match `LinkGenerator.Url` on that line; return the URL when the offset lies inside a match. Reuse the existing
   regex; do not duplicate it.
2. `OnViewPressed` (Tunnel): Ctrl+left press over a plain URL opens it (`Platform.DesktopPlatform.OpenUrl`), marks `e.Handled`, opens
   once. Keep the shortened-glyph path from r2. Plain click (no Ctrl) stays unhandled so it places the caret.
3. `OnViewMoved`: show the Hand over a plain URL only while Control is held (`e.KeyModifiers`), so plain-text URLs do not look
   clickable when editing. Shortened glyphs keep their always-on Hand. Chips and I-beam behavior unchanged.
4. Short URLs must still render as plain colored text (no generator change). Checkbox and chip behavior unchanged.
5. Extend the selftest headlessly: the URL hit test returns the URL for a point inside a short URL, inside an expanded caret-line URL
   and with ShortenLinks off, and null outside; Ctrl+left on it calls a stubbed opener once; plain/right/no-Ctrl do not. Never open a
   browser from the selftest (stub the opener or test only the helper). Update the `shortened-link-opens` contract row text to cover
   "any URL" (rename only if needed; keep one row).
6. Cleanup: delete build/A54-link-hover-click-check and -probe if your sandbox allows; otherwise say so.

## Done means
Build 0 warnings, SELFTEST PASS, gate passes, report states the approach and what is not proven live.
