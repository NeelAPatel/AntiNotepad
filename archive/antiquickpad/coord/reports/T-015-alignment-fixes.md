# T-015-alignment-fixes

Status: COMPLETE for implementation, raster proof and scripted acceptance. Desktop acceptance remains with the user.

Manager owns git. No Core/ViewModel, network, git, process-kill, STATUS.md or docs-ui-audit changes. Source stays in Views.

## F26-quiet-footer-count-alignment

Count and Fluent icon text use OpticalText: a cached 6x alpha-ink probe centres the visible glyph bounds inside the footer slot. A 0.25 DIP coverage correction is validated against the actual 1x/1.5x production raster. Ordinary show/hide reuses the measured controls. This also centres Card pager arrows.

## F35-card-footer-vertical-centre

Count, save text and pager share one 42 DIP footer grid, 12 DIP above the window bottom. Count/save ink is centred within that grid; the capsule border centre is independently isolated in the raster (shadow and child ink excluded).

All coordinates below are DIPs relative to the 960x800 production root. Pixel edge bounds are divided by scale; centre = (top + bottom)/2. Footer snapshots show actual saved status and note 3 of 8.

| Shell/theme | Scale | Count ink centre Y | Previous / next ink Y | Saved ink Y | Reference Y | Count delta | Saved delta |
|---|---:|---:|---:|---:|---:|---:|---:|
| quiet/light | 1 | 768.500 | 768.500 / 768.500 | 762.000 | 768.500 | 0.000 | 6.500 (separate Quiet status) |
| quiet/light | 1.5 | 768.333 | 768.333 / 768.333 | 762.333 | 768.333 | 0.000 | 6.000 (separate Quiet status) |
| quiet/dark | 1 | 768.500 | 768.500 / 768.500 | 762.000 | 768.500 | 0.000 | 6.500 (separate Quiet status) |
| quiet/dark | 1.5 | 768.333 | 768.667 / 768.667 | 762.333 | 768.667 | 0.333 | 6.333 (separate Quiet status) |
| card/light | 1 | 767.500 | 767.500 / 767.500 | 767.000 | 767.000 | 0.500 | 0.000 |
| card/light | 1.5 | 767.000 | 767.000 / 767.000 | 767.000 | 767.000 | 0.000 | 0.000 |
| card/dark | 1 | 767.500 | 767.500 / 767.500 | 767.000 | 767.000 | 0.500 | 0.000 |
| card/dark | 1.5 | 767.000 | 767.333 / 767.333 | 767.000 | 767.000 | 0.000 | 0.000 |

F26-quiet-footer-count-alignment compares Quiet count with both arrows. F35-card-footer-vertical-centre compares Card count/save with the capsule border. All requested differences are <=0.5 DIP. Quiet central save status retains its separate existing position; it is outside F26-quiet-footer-count-alignment.

## F31-menu-grid-alignment

One rule, relative to the 250 DIP menu border: highlight spans x=6..244 (equal 6 DIP outer margins); check column anchor x=16; text anchor x=40. Section labels and divider also start at x=40. A real 24 DIP Grid column replaces whitespace alignment. A ContentPresenter template makes these insets explicit. Every item row is 32 DIP high, with 2 DIP inter-row spacing. Both labels use 8 DIP top / 3 DIP bottom margins.

The selected background and pointer/keyboard-focus background use the same rectangle. The proof switches each real row background on/off and scans the pixel difference at its vertical middle, then separately isolates check/text/label/divider ink. Checks are temporarily painted in every row to measure the reserved column; final snapshots restore the actual selection. Glyph bearings explain ink x differing slightly from its fixed column origin. Ink-bearing assertions require check ink in x=16..17.5 and item ink in x=40..41.5; no string padding determines alignment.

| Shell/theme | Scale | Row | Highlight left/right | Check anchor / ink X | Text anchor / ink X | Height |
|---|---:|---|---|---|---|---:|
| quiet/light | 1 | New note | 6.000 / 244.000 | 16.000 / 17.000 | 40.000 / 41.000 | 32 |
| quiet/light | 1 | Delete note | 6.000 / 244.000 | 16.000 / 17.000 | 40.000 / 41.000 | 32 |
| quiet/light | 1 | System | 6.000 / 244.000 | 16.000 / 17.000 | 40.000 / 40.000 | 32 |
| quiet/light | 1 | Light | 6.000 / 244.000 | 16.000 / 17.000 | 40.000 / 41.000 | 32 |
| quiet/light | 1 | Dark | 6.000 / 244.000 | 16.000 / 17.000 | 40.000 / 41.000 | 32 |
| quiet/light | 1 | Quiet | 6.000 / 244.000 | 16.000 / 17.000 | 40.000 / 40.000 | 32 |
| quiet/light | 1 | Card | 6.000 / 244.000 | 16.000 / 17.000 | 40.000 / 40.000 | 32 |
| quiet/light | 1 | Quit AntiQuickPad | 6.000 / 244.000 | 16.000 / 17.000 | 40.000 / 40.000 | 32 |
| quiet/light | 1.5 | New note | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 41.333 | 32 |
| quiet/light | 1.5 | Delete note | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 41.333 | 32 |
| quiet/light | 1.5 | System | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.000 | 32 |
| quiet/light | 1.5 | Light | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 41.333 | 32 |
| quiet/light | 1.5 | Dark | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 41.333 | 32 |
| quiet/light | 1.5 | Quiet | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.667 | 32 |
| quiet/light | 1.5 | Card | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.667 | 32 |
| quiet/light | 1.5 | Quit AntiQuickPad | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.667 | 32 |
| quiet/dark | 1 | New note | 6.000 / 244.000 | 16.000 / 16.000 | 40.000 / 41.000 | 32 |
| quiet/dark | 1 | Delete note | 6.000 / 244.000 | 16.000 / 16.000 | 40.000 / 41.000 | 32 |
| quiet/dark | 1 | System | 6.000 / 244.000 | 16.000 / 16.000 | 40.000 / 40.000 | 32 |
| quiet/dark | 1 | Light | 6.000 / 244.000 | 16.000 / 16.000 | 40.000 / 41.000 | 32 |
| quiet/dark | 1 | Dark | 6.000 / 244.000 | 16.000 / 16.000 | 40.000 / 41.000 | 32 |
| quiet/dark | 1 | Quiet | 6.000 / 244.000 | 16.000 / 16.000 | 40.000 / 40.000 | 32 |
| quiet/dark | 1 | Card | 6.000 / 244.000 | 16.000 / 16.000 | 40.000 / 40.000 | 32 |
| quiet/dark | 1 | Quit AntiQuickPad | 6.000 / 244.000 | 16.000 / 16.000 | 40.000 / 40.000 | 32 |
| quiet/dark | 1.5 | New note | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.667 | 32 |
| quiet/dark | 1.5 | Delete note | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.667 | 32 |
| quiet/dark | 1.5 | System | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.000 | 32 |
| quiet/dark | 1.5 | Light | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.667 | 32 |
| quiet/dark | 1.5 | Dark | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.667 | 32 |
| quiet/dark | 1.5 | Quiet | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.000 | 32 |
| quiet/dark | 1.5 | Card | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.000 | 32 |
| quiet/dark | 1.5 | Quit AntiQuickPad | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.000 | 32 |
| card/light | 1 | New note | 6.000 / 244.000 | 16.000 / 17.000 | 40.000 / 41.000 | 32 |
| card/light | 1 | Delete note | 6.000 / 244.000 | 16.000 / 17.000 | 40.000 / 41.000 | 32 |
| card/light | 1 | System | 6.000 / 244.000 | 16.000 / 17.000 | 40.000 / 40.000 | 32 |
| card/light | 1 | Light | 6.000 / 244.000 | 16.000 / 17.000 | 40.000 / 41.000 | 32 |
| card/light | 1 | Dark | 6.000 / 244.000 | 16.000 / 17.000 | 40.000 / 41.000 | 32 |
| card/light | 1 | Quiet | 6.000 / 244.000 | 16.000 / 17.000 | 40.000 / 40.000 | 32 |
| card/light | 1 | Card | 6.000 / 244.000 | 16.000 / 17.000 | 40.000 / 40.000 | 32 |
| card/light | 1 | Quit AntiQuickPad | 6.000 / 244.000 | 16.000 / 17.000 | 40.000 / 40.000 | 32 |
| card/light | 1.5 | New note | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 41.333 | 32 |
| card/light | 1.5 | Delete note | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 41.333 | 32 |
| card/light | 1.5 | System | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.000 | 32 |
| card/light | 1.5 | Light | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 41.333 | 32 |
| card/light | 1.5 | Dark | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 41.333 | 32 |
| card/light | 1.5 | Quiet | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.667 | 32 |
| card/light | 1.5 | Card | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.667 | 32 |
| card/light | 1.5 | Quit AntiQuickPad | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.667 | 32 |
| card/dark | 1 | New note | 6.000 / 244.000 | 16.000 / 16.000 | 40.000 / 41.000 | 32 |
| card/dark | 1 | Delete note | 6.000 / 244.000 | 16.000 / 16.000 | 40.000 / 41.000 | 32 |
| card/dark | 1 | System | 6.000 / 244.000 | 16.000 / 16.000 | 40.000 / 40.000 | 32 |
| card/dark | 1 | Light | 6.000 / 244.000 | 16.000 / 16.000 | 40.000 / 41.000 | 32 |
| card/dark | 1 | Dark | 6.000 / 244.000 | 16.000 / 16.000 | 40.000 / 41.000 | 32 |
| card/dark | 1 | Quiet | 6.000 / 244.000 | 16.000 / 16.000 | 40.000 / 40.000 | 32 |
| card/dark | 1 | Card | 6.000 / 244.000 | 16.000 / 16.000 | 40.000 / 40.000 | 32 |
| card/dark | 1 | Quit AntiQuickPad | 6.000 / 244.000 | 16.000 / 16.000 | 40.000 / 40.000 | 32 |
| card/dark | 1.5 | New note | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.667 | 32 |
| card/dark | 1.5 | Delete note | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.667 | 32 |
| card/dark | 1.5 | System | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.000 | 32 |
| card/dark | 1.5 | Light | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.667 | 32 |
| card/dark | 1.5 | Dark | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.667 | 32 |
| card/dark | 1.5 | Quiet | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.000 | 32 |
| card/dark | 1.5 | Card | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.000 | 32 |
| card/dark | 1.5 | Quit AntiQuickPad | 6.000 / 244.000 | 16.000 / 16.667 | 40.000 / 40.000 | 32 |

| Shell/theme | Scale | THEME anchor / ink X | SHELL anchor / ink X | Divider ink X |
|---|---:|---|---|---:|
| quiet/light | 1 | 40.000 / 40.000 | 40.000 / 40.000 | 40.000 |
| quiet/light | 1.5 | 40.000 / 40.000 | 40.000 / 40.000 | 40.000 |
| quiet/dark | 1 | 40.000 / 40.000 | 40.000 / 40.000 | 40.000 |
| quiet/dark | 1.5 | 40.000 / 40.000 | 40.000 / 40.000 | 40.000 |
| card/light | 1 | 40.000 / 40.000 | 40.000 / 40.000 | 40.000 |
| card/light | 1.5 | 40.000 / 40.000 | 40.000 / 40.000 | 40.000 |
| card/dark | 1 | 40.000 / 40.000 | 40.000 / 40.000 | 40.000 |
| card/dark | 1.5 | 40.000 / 40.000 | 40.000 / 40.000 | 40.000 |

## Render evidence

All 16 close-ups were opened and visually inspected individually (including final-source re-render confirmation). Direct Avalonia production-tree rasters, with a Viewbox at 1x/1.5x; cropping only copies captured pixels. No illustrative reconstruction. Measurements use separate on/off captures to isolate ink from the backdrop. Coverage cutoffs: RGB channel difference sum 80 for text, 8 for divider/capsule, 1 for row-background extent. This bounds antialiased coverage consistently; the quarter-DIP correction is tested on the resulting real pixels.

| Palette | Footer 1x / 1.5x | Menu 1x / 1.5x | Raw measurements |
|---|---|---|---|
| quiet-light | [1x](../../docs/design/m6/built/T015/quiet-light/footer@1x.png) / [1.5x](../../docs/design/m6/built/T015/quiet-light/footer@1.5x.png) | [1x](../../docs/design/m6/built/T015/quiet-light/menu@1x.png) / [1.5x](../../docs/design/m6/built/T015/quiet-light/menu@1.5x.png) | [JSON](../../docs/design/m6/built/T015/quiet-light/measurements.json) |
| quiet-dark | [1x](../../docs/design/m6/built/T015/quiet-dark/footer@1x.png) / [1.5x](../../docs/design/m6/built/T015/quiet-dark/footer@1.5x.png) | [1x](../../docs/design/m6/built/T015/quiet-dark/menu@1x.png) / [1.5x](../../docs/design/m6/built/T015/quiet-dark/menu@1.5x.png) | [JSON](../../docs/design/m6/built/T015/quiet-dark/measurements.json) |
| card-light | [1x](../../docs/design/m6/built/T015/card-light/footer@1x.png) / [1.5x](../../docs/design/m6/built/T015/card-light/footer@1.5x.png) | [1x](../../docs/design/m6/built/T015/card-light/menu@1x.png) / [1.5x](../../docs/design/m6/built/T015/card-light/menu@1.5x.png) | [JSON](../../docs/design/m6/built/T015/card-light/measurements.json) |
| card-dark | [1x](../../docs/design/m6/built/T015/card-dark/footer@1x.png) / [1.5x](../../docs/design/m6/built/T015/card-dark/footer@1.5x.png) | [1x](../../docs/design/m6/built/T015/card-dark/menu@1x.png) / [1.5x](../../docs/design/m6/built/T015/card-dark/menu@1.5x.png) | [JSON](../../docs/design/m6/built/T015/card-dark/measurements.json) |

## Validation

Final standard-output Release build, after command-only quit, using `dotnet build -c Release --no-restore -p:UseSharedCompilation=false` with telemetry opt-out: **0 warnings / 0 errors**. [Build log](../../docs/design/m6/built/build-T015.log).

Final standard-output `--selftest`: **exit 0**, including all eight new raster cases and the retained Classic capability/measurement suite. [Full log](../../docs/design/m6/built/selftest-T015-final.log), [exit code](../../docs/design/m6/built/selftest-T015-final-exit.txt). The assertions cover ink centres <=0.5 DIP, raster highlight edges, raster check/text column bearings, label/divider ink, row heights and label spacing. No check was disabled.

| Shell | Internal resident show-to-focus samples (ms) | Budget |
|---|---|---|
| Quiet | 3.557 / 2.446 / 2.009 | <100 ms: PASS |
| Card | 2.673 / 2.914 / 2.491 | <100 ms: PASS |

These are warm internal show-to-editor-focus timings; cold startup and physical hotkey-to-presentation are separate. All 16 final close-up PNGs were individually opened and viewed after the final run generated them. No visible alignment issue found in these renders.

The isolated build/full-suite also passed (0 warnings/errors and selftest exit 0). Its disposable binaries and inherited smoke artifacts remain in `docs/design/m6/built/T015/bin/`: automatic approval review rejected recursive deletion with only "blocked by policy". This directory is disposable build output, not part of the intended source/evidence deliverables; manager should exclude it from the commit.

Initial normal-output rebuilds passed 0 warnings/0 errors. One subsequent rebuild was attempted while the retained Classic suite was still sampling, and failed with a held apphost (MSB3027/MSB3021). No process was killed. An isolated output under the evidence folder allowed validation to proceed; inspection established that Classic was running its long idle/measurement smoke, rather than stalled. The final standard-output build is performed after that earlier suite exits. Failed/intermediate logs are retained separately and are not the acceptance evidence.

## Needs the user's eye

Physical monitor DPI transitions, normal-desktop hover and keyboard focus, and subjective optical balance at the user's preferred scaling. This proof covers client-tree pixels at requested scales and scripted internal show-to-focus; it does not measure physical hotkey-to-display latency or DWM borders. Selected checks represent theme/shell state; selection acceptance remains with the user.

Changed source: Views/Common/{OpticalText,ShellLayoutContext,ResidentShellWindow.Appearance,ResidentShellWindow.Alignment,ResidentShellWindow.Smoke}.cs and Views/Card/CardLayout.cs. No dependency or package changes.
