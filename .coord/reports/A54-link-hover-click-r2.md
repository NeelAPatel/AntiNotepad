## Summary
A54-link-hover-click-r2 fixes the actual cursor defect: PadView.OnViewMoved overwrote GlyphElement's Hand with Ibeam after AvaloniaEdit's class handler. Hover now hit-tests glyph bounds directly, and shortened-link Ctrl+left presses dispatch in the TextView tunnel handler before selection, with an explicit handled-event guard against duplicate glyph dispatch. The prior live Ctrl+click failure's precise selection route is unconfirmed; this change removes that dependency.

## Change list
| file:line-range | what changed | which goal line it serves |
|---|---|---|
| product/AntiNotepad/Editor/Elements.cs:19-30,138-146 | Link marker and handled-event guard | Identify shortened links and prevent duplicate opens |
| product/AntiNotepad/Editor/PadView.cs:561-616 | Shared glyph bounds hit test, hover cursor and tunnel dispatch | Preserve glyph Hand, restore Ibeam outside, dispatch Ctrl+left before selection |
| product/AntiNotepad/SelfTest.cs:135-206 | Preserve round-1 checks; extend stubbed tunnel, duplicate, plain/right/missing/nonlink and wiring checks | Headless regression evidence without browser launch |
| product/AntiNotepad/SelfTest/Contracts.cs:68-74 | Retain round-1 shortened-link-opens row | Required behavior contract |

## Behavior changes
GlyphElement.IsLink defaults false and is true only for shortened URLs; it separates link dispatch from checkboxes. GlyphElement.OnPointerPressed ignores already handled events. PadView hover derives Hand from actual glyph bounds as well as existing chip bounds; tooltip behavior stays chip-specific. Ctrl+left on a shortened glyph is handled before selection and calls Pressed once; plain/right and nonlink presses retain the existing path. No setting, timer or persistent state changes.

## Out-of-scope files
This required report. Temporary rendering probe in build/A54-link-hover-click-probe (geometry only; no shown window or browser). STATUS remains unchanged because docs/loop.md says task branches never edit STATUS; manager records milestones centrally.

## Contracts
Retained shortened-link-opens. Automated portion now also verifies tunnel dispatch, duplicate suppression, plain/right/missing/nonlink rejection and shared hit-test wiring. Geometry and physical editor/browser behavior are separate evidence below and manual checks.

## Lens review
lens_reviewer: no proven findings; VERDICT: accept. Reviewed scope, callers, cursor/press coupling, new IsLink state and handled-event guard; independently ran the task selftest and git diff --check. Physical routing/browser remains a manual check.

## Checks run
- Debug build into build/A54-link-hover-click-check with AVALONIA_TELEMETRY_OPTOUT=1 and NuGetAudit=false: 0 warnings, 0 errors.
- --selftest: SHORTENED LINK all True; CONTRACTS: 9 rows, 8 pass, 1 manual; SELFTEST PASS.
- Temporary native-platform rendering probe: GEOMETRY PROBE PASS. Real shortened glyph hit; outside glyph/viewport/below-text misses; nonzero scroll offsets; wrapped glyph hit. TextView measured/arranged directly, no window shown. Synthetic detached pointer events returned (0,0), so the initial handler probe was discarded as unsuitable; it supplies no physical-routing or cursor-handler evidence. The selftest exercises dispatch directly with a stub opener.
- pwsh -NoProfile -File tools/gate.ps1 -Task A54-link-hover-click-r2 -Offline: GATE: PASS; build 0 warnings/errors; SELFTEST PASS; seam 0 violations. Warnings: offline audit disabled, round brief has no Files section (base Files checked), PadView/SelfTest size growth. Manager online gate pending.
- git diff --check: PASS.
- Gate cleaned its own build/data folders. Automatic approval review rejected recursive deletion of build/A54-link-hover-click-check and build/A54-link-hover-click-probe as blocked by policy, after absolute target validation under this worktree build directory. Both remain for manager cleanup.
- No normal AntiNotepad launch, quit, stop or browser open.

## Hand checks for the user
In the dev copy, enable Shorten links, paste https://example.com/long/path/over/thirtyfour/characters, and move the caret to another line. Check Hand over the shortened text, I-beam after leaving, Ctrl+left opens once, and plain click places the caret and expands the URL. Repeat after scrolling/wrapping; check checkbox Hand/toggle, result-chip Hand/copy, and short URLs unchanged.

