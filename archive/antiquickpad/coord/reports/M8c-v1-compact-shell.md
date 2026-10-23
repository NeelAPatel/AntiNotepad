# M8c-v1-compact-shell

Completed 2026-10-21. All four sub-steps passed their gates and have source snapshots. No git commands, network, packages, process killing, --measure, controller changes, or ViewModel/Core/Contract changes. STATUS/TRACKER were left to the manager because the task's explicit write allowlist excludes them; no feedback item status/priority was changed.

## M8c-v1-compact-shell / c1-retire-card-views

Deleted Views/Card/CardLayout.cs and its folder. Removed the Card layout switch, decorator/stride branches, palette branches, menu placement branch and Card-only alignment expectations from Views/Common. Kept the deliberate M8b-retire-card-function legacy-settings migration and rejected-command smoke checks: these guard retirement and are not Card rendering paths. Design/Card fixtures are outside the write allowlist and remain.

No new tokens, transparency changes, or pixel expectation changes in this sub-step. All 28 Quiet light/dark PNGs (14 per theme) are byte-identical to the pre-edit capture. Comparison: `.coord/scratch/M8c/c1-render-comparison.txt`.

Gate: graceful quit 0; Release build 0, 0 warnings/0 errors; selftest 0, BOUNDARY PASS (249 types), CONFORMANCE quiet PASS (23 commands), both themes' 1x/1.5x raster proofs and heading/checkbox/strike/guide proofs PASS; retained Classic PASS. Render exits both 0. Logs: `c1-{quit,build,selftest,light,dark}.log` under `.coord/scratch/M8c/`. Renders: `c1-light/`, `c1-dark/` (960 x 800 baseline). Snapshot: `c1/`, marker `c1.done`.

## M8c-v1-compact-shell / c2-window-material

Resident window defaults changed 960 x 800 -> 480 x 560 DIP; minimum 600 x 480 -> 360 x 420. CenterScreen remains the first-run placement; the existing M8b-retire-card-function version migration ignores pre-overhaul bounds, while current-version user bounds still restore. BorderOnly and extended client area remain; titlebar height hint 58 -> 38. No persistence or bounds-restoration logic changed.

New Look tokens and regional token changes (ARGB colours):

| Token | Default / old | Quiet light | Quiet dark |
| --- | --- | --- | --- |
| Look.Material (new) | solid (retained for other consumers) | acrylic | acrylic |
| Look.TintAlpha (new) | 1 (retained for other consumers) | .90 | .90 |
| Look.CodePanel | light #FFEEF0EB / dark #FF19221D | #99EEF0EB (60% alpha) | #9919221D (60% alpha) |
| ShellPalette.ActiveLine -> Look.ActiveLine | light #FFEAF0E9 / dark #FF304537 | #66EAF0E9 (40% alpha) | #66304537 (40% alpha) |

Look.Background and Canvas RGB stay light #FAFAF7 / dark #202823. TransparencyLevelHint is ordered AcrylicBlur, Mica, None; TransparencyBackgroundFallback is the opaque themed Canvas. One non-hit-testable Border behind all content paints Canvas with Look.TintAlpha. Local code-panel and active-row regions use the translucent tokens above; checkbox/guide/text colours retain their previous meanings.

Transparent layers: Window.Background; root Grid.Background; outer frame/page/viewport/host surfaces (no fill); editor and TextArea.Background; instance template descendant Panel, Border, TemplatedControl (including ScrollViewer), and ContentPresenter backgrounds; carousel current/preview editor surfaces and grab strip. EditorPresentation clears template layers on Loaded and after template application, including theme/editor rebuilds and preview editors. TextView has no full-area Background property in this version. The More menu deliberately uses an opaque surface and shadow; it is not a second full-window tint.

Proof changes: theme assertions now expect transparent editor background plus the correct opaque fallback, rather than an opaque editor; simulated system-theme changes compare fallback Canvas rather than editor background. Existing glyph-only RTB readers assume an opaque control surface: the smoke temporarily sets only the editor background to Look.Background during checkbox/strike/guide fixtures, then restores transparency. Their thresholds and semantic checks are unchanged. Renderer fixtures explicitly use 960 x 800 test-window space so long single-line fixtures do not wrap in the compact shell; production compact geometry is separately checked in the final sub-step.

First gate selftest failed on a concrete SolidColorBrush cast of Brushes.Transparent (immutable brush). One fix changed the assertion to ISolidColorBrush. The retry passed; original failure is preserved in `c2-attempt1-selftest.log`. No restore needed.

Final gate: quit 0; Release build 0, 0 warnings/0 errors; selftest 0, BOUNDARY PASS (250 types), CONFORMANCE quiet PASS (23 commands), all raster/renderer proofs and Classic PASS; both renders 0. Logs: `c2-{quit,build,selftest,light,dark}.log`. Renders `c2-light/`, `c2-dark/` still use the original 960 x 800 render harness (changed in the next sub-step); resident defaults are compact. Snapshot `c2/`, marker `c2.done`.

## M8c-v1-compact-shell / c3-compact-layout

Replaced the title, collection label, separated footers and delete icon with the V1 composition. Top actions are Pin, New note, More, Hide (Segoe Fluent Icons); all use ShellLayoutContext.Button and existing IDs. An always-present full-width transparent DragRegion remains independent of bar opacity/hit testing. The existing Minimize toolbar control is placed in a small toolbar row inside More, preserving the exact declared route without changing the function contract. Existing New/Delete, theme choices, Quiet shell choice and Quit menu entries remain, with solid surface and shadow.

Bottom presentation is 36 DIP high, 10 DIP above the bottom, with Previous / PositionText / PagerDots / Next, SaveLineText (SaveLineFailed accent), and Escape hint. Dot buttons retain note.select and their projected index. Selected dot width 20 -> 14; inactive button width 8 -> 5 (ink stays 5); spacing 7 -> 3. Save text trims to available width with a tooltip for its full value. Position/save font 12 -> 11; Escape font 11 -> 10. Buttons retain 32 x 30 slots.

Look.FontSize is 17 -> 14 DIP in both themes; EditorFont and CodeFont remain Cascadia Mono, Consolas; heading sizes/colours are unchanged. Hint is one muted 14 DIP mono line replacing the former 34/17/13 DIP promotional block. Editor viewport ends 56 DIP above the bottom, so its final lines cannot sit behind the bottom presentation. The initial compact placement treated the fold gutter as part of the inset; the final mock comparison in c4 below corrects that to the mock's editor inset plus gutter. The same single tint and transparent layers from c2 remain.

Production render harness changed 960 x 800 -> 480 x 560, including its 1.5x output (720 x 840). All six states and menu/fold closeups remain. c3 keeps bars visible/dimmed until the next sub-step adds reveal.

Gate: quit 0; Release build 0, 0 warnings/0 errors; selftest 0, BOUNDARY PASS (250 types), CONFORMANCE quiet PASS (23 commands), all raster/renderer proofs and Classic PASS; renders both 0. Logs `c3-{quit,build,selftest,light,dark}.log`; renders `c3-light/`, `c3-dark/`; snapshot `c3/`, marker `c3.done`. No gate fixes.

## M8c-v1-compact-shell / c4-hover-reveal

QuietBarReveal owns only presentation state. Reveal predicate = pointer over root OR keyboard focus within a bar OR More menu visible. Pointer/menu/focus events refresh the predicate; a view-owned DispatcherTimer supplies the 400 ms leave linger, then opacity fades out over 150 ms. Reveal fades in over 120 ms. Returning before timeout cancels the hide. Hidden bars have opacity 0 and IsHitTestVisible=false; their controls remain keyboard focusable, and the independent native DragRegion remains hit-testable. Menu-open and keyboard-focus states retain the bars even without pointer presence.

The reveal object is disposed before appearance rebuild, removing handlers and stopping the linger timer. Visual detachment stops its timer; temporary render/proof reparenting preserves the state object. CaptureState forces deterministic stills without real pointer input; ProbePointer supplies a synthetic input for the hidden smoke. Main render opacity .65 -> 0; revealed/menu/empty states use 1. Focused bar buttons and pager dots have a visible accent border ring. Save tooltip binding was corrected to observe the current text.

Final comparison to the picked PNG: editor inset is 24 DIP outside the 24 DIP folding gutter (first text x=48). Page margin is now (24,38,24,56); the existing grab strip occupies the 12 DIP gap before the editor (Look.GrabHeight 15 -> 12, both themes). Removed only its decorative dots for chrome-free idle presentation; the drag target, cursor, tooltip and gesture handlers remain. Empty hint is at (48,50), exactly the first text origin. This replaces the old hint position (112,145) and the c3 intermediate text origin (24,65).

No reduced-motion flag is already exposed by the Views code or the current Avalonia IPlatformSettings API (theme colours only, no animation preference). Timings remain fixed; no platform API, setting or controller behavior was added.

Added small presentation proofs: idle zero opacity/no hit testing; pointer reveal; linger; menu retention; keyboard focus/ring; fade-out; material and regional alpha tokens; footer save/dots still laid out at 480 and 360 DIP; long note's final line above the pill; exact hint/text origin; active-line renderer-on/off pixels confined to its row. Final active-row proof: light changed=8136, dark changed=8128, outside=0. Existing alignment proof now runs at 480 x 560, at 1x and 1.5x, while legacy isolated glyph fixtures retain sufficient width. No renderer proof thresholds were loosened.

Final gate after mock-placement refinement: quit 0; Release build 0, 0 warnings/0 errors; selftest 0, BOUNDARY PASS (256 types), CONFORMANCE quiet PASS (23 commands), REVEAL PASS, PRESENTATION PASS both themes, all alignment/heading/checkbox/strike/guide/active-row proofs and retained Classic PASS. The unchanged documented window.move route-kind KNOWN-GAP diagnostic still precedes conformance PASS. Hidden-window foreground checks PASS. Both renders exit 0. No gate failure in c4; the first successful c4 gate is preserved as `c4-before-fidelity-{build,selftest}.log`, followed by the successful final gate.

Logs `c4-{quit,build,selftest,light,dark}.log`; final renders `c4-light/`, `c4-dark/`; snapshot `c4/`, marker `c4.done`; final source hashes `c4-sha256.json`. Visually inspected final light revealed/empty, dark idle, and the prior light menu and dark revealed states. RTB captures cannot show the native Acrylic backdrop.

## Deliberate proof deltas (old -> final)

These are all changed fixed expectations and the principal font/layout-derived raster values. Existing proofs derive glyph positions from the actual typeface and geometry; no hardcoded golden coordinate or tolerance was replaced merely to make a mismatch pass.

| Expectation / observation | Old -> final | Reason / retained intent |
| --- | --- | --- |
| Editor theme background assertion | opaque palette background -> transparent editor + opaque themed fallback | Glass must remain unobscured; theme changes still verified |
| System-theme comparison | editor RGB -> fallback RGB (#FAFAF7 -> #202823) | Editor stays transparent in both themes |
| Main capture bar opacity | .65 -> 0 | Requested idle reveal policy; hover still 1 |
| Compact hint/text origin | legacy hint (112,145) -> shared (48,50); c3 intermediate (24,65) -> (48,50) | Match 24 DIP editor inset + 24 DIP gutter; align hint with text |
| Body/row metrics | font 17 -> 14; checkbox rows 22.917 -> 18.873 DIP | V1 text size; checkbox/plain-row displacement remains 0 |
| Four-space guide raster x, editor-local | 63,103,143 -> 56,89,122 for levels 1,2,3 | Font-derived four-space stops; same depth/blank/tab/code checks |
| Strike affected rows, editor-local | 10,11,12 -> 8,9,10 | Smaller body font; max thickness remains 3px, allowed rows <=4, >=2px ink, coverage delta <=.10, outside-band changes 0 |
| Footer 1x y centres | count/prev/next 768.5 -> count 532, prev/next 532.5; save 768 -> 532.5 | Compact geometry; count/reference tolerance remains .5 DIP |
| Footer 1.5x y centres | count/save 768.333 -> 532.333; light arrows 768.333 -> 532.333; dark arrows 768.667 -> 532.667 | Same .5 DIP alignment tolerance at both scales |
| Six heading checks | unchanged font/sizes/colours | All six levels still verified; heading tokens preserved |
| Menu bearings/highlight/row expectations | unchanged | Check/text x=16/40, highlight edges=6/244, row=32 DIP; all raster checks PASS |
| Active row | added renderer-on/off proof, changed pixels >0 and outside=0 | Proves translucent row still paints only its intended band |

Checkbox size, stroke, check shape and check-centre tolerances are unchanged. Guide off-setting equality and active-block outside-pixel equality remain exact. Classic expected values remain unchanged. Regional alpha token old/new values are fully listed in c2 above.

## Function tasks and differences from V1

No new function task is required for this UI: existing commands, projections, persistence, navigation and editor behavior were sufficient. No requested function work was built. An optional future reduced-motion platform projection would need the function/platform owner if desired; it is not present now.

Differences: Minimize is retained as a toolbar row inside More because its declared toolbar route is required; top icon order follows this brief (Pin first). Save is text-only and Escape uses 10 DIP to keep both legible in the minimum-width footer; the mock's save checkmark is omitted. Failed/long save text ellipsizes with the full message in its tooltip, retaining failure accent. Native resize borders/corners and the real material are platform-rendered, so RTB and synthetic-backdrop mock images are not equivalent glass evidence. Top actions occupy the requested 38 DIP strip directly, without the mock's extra 10 DIP outer top offset. Grab presentation is invisible but its existing gesture remains reachable.

Manager follow-up: live Acrylic/light/tint-90 visual check, physical hover/focus/menu/drag/resize/scroll check, and the approved --measure. These were not run here. No claim of physical behavior or performance validation.

## Artifacts and scope verification

All paths below are relative to `.coord/scratch/M8c/`: `start/` is the before-edit source backup; `c1/` through `c4/` are each passing src/AntiQuickPad snapshot excluding bin/obj; each has a `.done` sibling. Each render directory contains main, bars-hover, menu-open, carousel, fold-hover and empty at 1x/1.5x plus closeups. `align/quiet-light/` and `align/quiet-dark/` contain final compact 1x/1.5x alignment crops and measurements.json. Existing renderer proof PNGs remain in the isolated selftest roots printed in each selftest log.

`changed-source-files.txt` lists 14 changed source paths, all inside the allowlist. Source comparison against start verified every protected source file unchanged; final c4 snapshot bytes equal current source, with hashes recorded. ResidentShellWindow.cs changed only window dimensions/minimums/titlebar-height properties. README has only an appended Built section. No rollback or failed marker was necessary.
