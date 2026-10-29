## Summary
A54-link-hover-click's reported failure was not reproduced: the unchanged product generated a shortened GlyphElement, showed Hand in a routed move, and dispatched Ctrl+left once to a stub opener. Actual live cause remains unconfirmed; no product behavior fix is claimed. The probe confirms preserved left-button/Control data and a handled cursor event; exact-package source places glyph dispatch before normal bubble selection. Full-editor physical routing is still unverified.

## Change list
| file:line-range | what changed | which goal line it serves |
|---|---|---|
| product/AntiNotepad/SelfTest.cs:135-187 | Headless glyph press checks with stub callback, plus IL checks of generator/cursor wiring | Automate what selftest can prove without opening a browser |
| product/AntiNotepad/SelfTest/Contracts.cs:68-74 | Added shortened-link-opens and live manual note | Required regression contract |
| .coord/reports/A54-link-hover-click.md:1-end | Investigation, evidence and remaining hand checks | Required report |

## Behavior changes
Selftest adds SHORTENED LINK output and one CONTRACTS row (9 rows, 8 pass, 1 manual). No product method, shared state, timing, setting, or default changed. The selftest stub checks glyph dispatch only: it does not prove the production URL delegate, rendering, browser launch, or actual caret placement.

## Out-of-scope files
.coord/reports/A54-link-hover-click.md: required report. Temporary rendering probe under build/A54-link-hover-click-probe. Both that folder and build/A54-link-hover-click-check remain: automatic approval review rejected recursive cleanup as blocked by policy, including a retry with verified literal absolute paths. STATUS unchanged: docs/loop.md says task branches never edit STATUS.

## Contracts
Added shortened-link-opens: "A shortened link shows a hand cursor and opens on Ctrl+click; a plain click places the caret." Headless portion exercises left/Control event preservation, one callback, plain event remaining unhandled and right-button rejection, and inspects the presence of generator/cursor setter calls (not their values or branch eligibility). Manual note covers the actual live promise.

## Lens review
lens_reviewer: no proven findings; VERDICT: accept for the test/report diff. Clarified its two evidence limits: the probe exercises TextView directly without editor templates or a visual TextArea route, and IL setter checks prove presence only. Live defect remains unresolved.

## Checks run
- Debug build to build/A54-link-hover-click-check with AVALONIA_TELEMETRY_OPTOUT=1 and NuGetAudit=false: 0 warnings, 0 errors.
- --selftest: SHORTENED LINK all True; CONTRACTS: 9 rows, 8 pass, 1 manual; SELFTEST PASS.
- Temporary native-platform rendering probe, no window shown: PadView constructed with its LinkGenerator; only TextView measured/arranged directly, without editor templates or a visual TextArea route; URL https://example.com/long/path/over/thirtyfour/characters on line 1, caret on line 2; real rendered glyph, Pressed replaced by stub. Baseline output: glyph=AntiNotepad.Editor.GlyphElement; move handled=True cursor=Hand; pressed left=True mods=Control; press handled=True opens=1. This probes synthetic TextView callbacks, not physical hit testing, full-editor routing, selection/focus ordering, or the browser.
- pwsh -NoProfile -File tools/gate.ps1 -Task A54-link-hover-click -Offline: GATE: PASS; build 0 warnings/errors; SELFTEST PASS; seam 0 violations; scope PASS. Expected offline audit warning; size warning: SelfTest.cs +54 lines. Manager online gate pending.
- Gate removed its own task build/data folders. Requested check/probe cleanup was rejected by automatic approval review (blocked by policy); manager cleanup required.
- No AntiNotepad normal launch, --quit, process stop, or browser open.

## Hand checks for the user
In the dev copy, enable Shorten links and paste https://example.com/long/path/over/thirtyfour/characters on a line; put the caret on another line and verify the URL is visibly shortened. Hover it and leave (Hand then I-beam), Ctrl+left-click once (one browser open), then plain-click (caret placement and full URL). Check result-chip hover/copy and a short URL such as https://example.com stays plain colored text. If the visibly shortened case fails, capture which step fails and whether scrolling/wrapping changes it; this task remains awaiting a live reproduction, not a claimed fix.


