# A35e-targets-keyboard: build run 1

## 1. Summary
Implemented the requested targets, selectable chips and swatches, and shared Plain focus adorner. Verification is incomplete.

## 2. Change list
Updated chrome tokens, pager layout, Settings controls, Plain, and UI contract checks.

## 3. Behavior changes
Segments, toggle pills and text buttons are 32 px high; dot targets have a fixed 24 px pitch; accent targets are 32 px; chips and swatches are buttons with selected accessible names. Plain uses an Ink focus adorner.

## 4. Out-of-scope files
No product files outside the card list were changed. The gate wrote its standard report; its scope check lists 14 existing lab files relative to the requested base.

## 5. Plan deviations
The old bar selftest assumes a flat pager and the design snapshot hash is stale. Their fixes require SelfTest/Bar.cs and SelfTest/Design.cs, both outside the card's fixed Files list. The environment rejected recursive removal of the requested build folder; the failing gate also retained its build and data folders.

## 6. Contracts
Added a target and keyboard contract. It passes in the gate run; the focus adorner hook is checked, not live keyboard/pointer rendering.

## 7. Lens review
Not run; this run did not request delegation and verification remains incomplete.

## 8. Checks run
Offline Release build passed with zero warnings; tokens and format passed. Selftest and gate failed on the existing bar tree assumption and design snapshot hash; scope also failed on the lab files. Seam passed. See the gate report for details.

## 9. Hand checks for the user
After automated checks are repaired, Tab, Space and Enter through chips and swatches; check keyboard and pointer focus rings, dot stability, and pager fit at 320 px in both themes.

# Run 2

## 1. Summary
Implemented the requested recorder, pane shortcut, Settings focus, bar gap, menu order and edge cues. Gate remains blocked by scope and cleanup.

## 2. Change list
Added F6 to the shortcut registry and documentation; restored Settings focus; added bar cue and separator tokens; refreshed the design snapshot.

## 3. Behavior changes
Backspace clears optional recordings; F6 changes split pane focus; Settings rebuilds select the matching accessible name or first control; Delete leads the menu row; hidden sliding bars show edge handles.

## 4. Out-of-scope files
No files outside the card's Files list were edited, except this requested report and the gate's generated report. The gate identifies 14 existing lab files relative to the requested base.

## 5. Plan deviations
The gate cannot pass scope without changing the card or base. The environment rejected recursive deletion of the requested build directory; the failed gate retained its own build and data folders.

## 6. Contracts
Added recorder, shortcut/menu and focus-selection contracts. The focus-selection test covers matching and fallback; actual focus transfer and visual cues require hand verification.

## 7. Lens review
Not run; no delegation requested.

## 8. Checks run
Release build: zero warnings. Selftest: pass. Offline gate: build, selftest, seam, tokens and format pass; scope fails on 14 existing lab files. Size warnings for UI.cs and SettingsWindow.cs.

## 9. Hand checks for the user
Check F6 both ways in split and single view; recorder optional/required Backspace; rail and changing Settings options with Tab; gap and edge cues in both themes, modes and reduced motion at 320 px.

# Round 2

## 1. Summary
Fixed the four review findings for Settings focus and bar reveal cancellation.

## 2. Change list
Shared focus capture and restoration across palette, page and rail rebuilds; retained and canceled top, bottom and float-fade animation handles.

## 3. Behavior changes
Focus follows its automation name or unnamed control index; the first control is chosen only when nothing was focused. Rail focus survives leaving search, and old reveal frames cannot overwrite a mode reset.

## 4. Out-of-scope files
Only the requested product files and report were edited; the gate wrote its standard report. Scope flags 14 existing lab files relative to the base.

## 5. Plan deviations
None. The design dump did not change, so its pin was not updated.

## 6. Contracts
Added named, unnamed, no-focus and missing-target checks; tested cancellation through ApplyBars and updated contract rows.

## 7. Lens review
No new independent review was run; this round addresses the four findings in the supplied review.

## 8. Checks run
Release build: zero warnings. Selftest: pass. Offline gate: build, selftest, seam, tokens and format pass; scope fails on the 14 existing lab files. Gate retained its build and data folders and reported size warnings.

## 9. Hand checks for the user
Keyboard-change theme, palette and accent; reset an individual shortcut; exit search with the current rail item; switch bar modes during reveal and check handles, including float and reduced motion.

# Round 3

## 1. Summary
Removed split-pane F6 and revised the handle, palette choices, Settings focus, recorder, theme transition, and expander face. Visual behavior still needs hand verification.

## 2. Change list
Added the inset and equal chip-width tokens, rebuilt both palette picks, inset the shared focus adorner, restored keyboard-visible focus, and added a snapshot fade over restyles. The recorder skips the host in Tab order and returns focus to Change, including after the shortcuts page rebuilds.

## 3. Behavior changes
The 24 x 2 handle is inset 3 px; palette chips have equal widths. Palette and accent restyles retain scroll and fade the previous image unless reduced motion is enabled.

## 4. Out-of-scope files
No additional product files were edited. The gate reports 16 existing lab files against the requested base and wrote its standard report.

## 5. Findings and deviations
Plain's adorner extended outside button bounds; clipped card contents can cut it off. The rail shares that adorner, but without a live visual check its exact clipping or adorner-layer cause is not proven. The adorner is now inside the button. Pointer and keyboard rendering, selected rail items, and every Plain button still need a hand test. A recorder focus check after cancel could not be established without an attached interactive window; the selftest checks the idle host and the restoration code is unverified visually.

Other Set pickers without rows rebuild: segments for Theme, Slot dots, Line spacing, List default, Paper strength, Scrolling lines/distance/coast/boost, Taskbar; dropdowns for Split palette, Tint, UI scale, Fade delay, Font, Font size, Text width, Tone, Answer position, Fresh note delay and Save delay. Segments repaint themselves; dropdowns update their displayed label, but their popup selection is built from the original value. These were not changed.

## 6. Contracts
Selftests check equal chip widths, selected palette after a picker rebuild, idle recorder host, F6 default removal, and handle inset. They do not prove rendered shape, actual keyboard focus after cancel, palette row text, or transitions. The design dump changed deterministically and its pin was updated.

## 7. Expander cause
The row's padding contained a negatively margined Plain face while the row border remained outside it; the face did not cover the full row. The face now owns the padding and receives the card-end radius.

## 8. Checks run
Offline gate: build, selftest, seam, tokens and format pass; scope fails only on 16 lab files. Gate retained its build and data folders. See the gate report for size warnings.

## 9. Hand checks for the user
Check handle roundness and inset; palette selection and scroll in System-dark; all Settings focus rings and recorder Change-to-Reset navigation; rapid palette/accent switching and reduced motion; expander hover at card ends, including collapsed rows.

# Round 4

## 1. Summary
Implemented float option A and space for the pill shadow.

## 2. Change list
Moved separators between groups, tied them to visible side controls, and added symmetric shadow-reach padding to the popup.

## 3. Behavior changes
Visible side controls have 8 px on either side of their separator; Slots never reserves New. The popup's vertical offset compensates for its padding and bar fading remains on the pill.

## 4. Out-of-scope files
The gate flags 16 existing lab files against the requested base; it also wrote its standard report.

## 5. Plan deviations
None. The design dump changed with the bar visual tree and its deterministic hash was re-pinned.

## 6. Contracts
Selftests cover float group combinations, separator margins, pager targets, shadow reach and popup padding. Native window pointer pass-through is not established by these checks.

## 7. Lens review
No independent review was requested or run.

## 8. Checks run
Release build to build/A35e-targets-keyboard-check: zero warnings; selftest passed. Offline gate passed build, selftest, seam, tokens and format; scope failed only for 16 lab files. Size warnings remain.

## 9. Hand checks for the user
On screen, check Standard and Slots with each side setting, disabled controls, 320 px fit, shadow and fade in both themes, and pointer input through the padding over the pad.
