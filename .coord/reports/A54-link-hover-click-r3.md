## Summary
A54-link-hover-click-r3 makes plain-text URLs Ctrl+left-clickable. The cause was that r2 hit-tested only shortened GlyphElements; short URLs, expanded caret-line URLs and Shorten links off produce no glyph. PlainUrlAt now maps viewport coordinates through scroll offset and GetPositionFloor, checks actual text/viewport bounds, and matches the existing LinkGenerator.Url regex on the document line.

## Change list
| file:line-range | what changed | which goal line it serves |
|---|---|---|
| product/AntiNotepad/Editor/PadView.cs:561-587 | Tunnel dispatch for plain URLs; Control-only Hand for plain text | Goals 2-3; preserves glyph/chip paths |
| product/AntiNotepad/Editor/PadView.cs:609-638 | Bounds-aware URL hit test and handled-once dispatch with supplied opener | Goals 1-2; shared regex, editing clicks unhandled |
| product/AntiNotepad/SelfTest.cs:6,179-227,250-259 | Windowless real layout with generator, short/expanded/off hits, outside misses, stubbed once/plain/right/missing checks and wiring | Goal 5; no browser |
| product/AntiNotepad/SelfTest/Contracts.cs:69-74 | Existing single row now promises any URL | Goal 5 |

## Behavior changes
OnViewPressed opens any plain URL on Ctrl+left through DesktopPlatform.OpenUrl, handles before opening and rejects repeat handled events. OnViewMoved adds Hand for plain URLs only with Control. New internal stateless PlainUrlAt returns a URL only inside plain text matches, excluding glyphs and points beyond text/viewport bounds; TryPressUrl accepts an opener delegate and leaves non-Control/right/missing presses unhandled. Shortened glyphs keep their always-on Hand and existing dispatch. No generator, color, settings, chip, checkbox, timing or persisted-state changes. The selftest initializes Avalonia windowless platform/layout services in its final contract check, without starting an app lifetime or showing a window.

## Out-of-scope files
This required report. STATUS unchanged per docs/loop.md task-branch rule; manager records milestones centrally. Existing untracked director output reports were left untouched.

## Contracts
Updated shortened-link-opens in place to cover any URL. Automated evidence includes real formatted pointer-point hit testing for short/caret-line/off URLs and outside misses, stubbed Ctrl+left once, plain/right/missing rejection, prior glyph checks and handler wiring. Physical pointer routing/cursor/browser behavior remains manual.

## Lens review
lens_reviewer: no proven findings; VERDICT: accept. Behavior changes match the diff; all helper callers checked, glyph/chip paths intact, no persistent state added. Reviewer independently ran the check build selftest (PASS) and git diff --check (PASS). Live pointer/cursor/browser behavior and wrapped/scrolled plain-text URL interaction remain manual.

## Checks run
- Debug build into build/A54-link-hover-click-check, AVALONIA_TELEMETRY_OPTOUT=1, NuGetAudit=false: 0 warnings, 0 errors.
- --selftest: ANY URL short/expanded/off=True/True/True; outside=True; once/rejected=True/True; CONTRACTS: 9 rows, 8 pass, 1 manual; SELFTEST PASS.
- pwsh -NoProfile -File tools/gate.ps1 -Task A54-link-hover-click-r3 -Offline: GATE: PASS; build 0 warnings/errors, SELFTEST PASS, seam 0 violations. Expected warnings: offline audit disabled, round briefs inherit base Files, PadView/SelfTest size growth.
- git diff --check: PASS. Automatic approval review rejected the recursive deletion command for build/A54-link-hover-click-check and build/A54-link-hover-click-probe as blocked by policy before execution. The check folder remains for manager cleanup; the probe folder is already absent. The gate cleaned its own output/data folders.
- No normal app launch/quit/stop, installed-copy access or browser open. Windowless layout does not prove physical routing, live cursor changes, browser launch or wrapped/scrolled interaction.

## Hand checks for the user
In the dev copy, test https://google.com and a long URL: shortened off the caret line, expanded on its line, and with Shorten links off. Ctrl+left must open once; plain click must place the caret; right-click must retain editing behavior. Plain text shows Hand only while moving with Control; shortened glyphs always show Hand. Leave the URL to restore I-beam. Repeat with wrapping/scrolling, and verify checkbox Hand/toggle and result-chip Hand/copy.

