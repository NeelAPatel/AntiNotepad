# M8a — Quiet variant renders

These are real Avalonia windows built from the production `EditorFactory.Create` with the Quiet light/dark palette, a single 90% canvas tint layer, transparent editor/template layers, and Acrylic. Each variant has idle and forced-hover states; the hover state makes the top and bottom routes visible without depending on a live pointer. The manager runs the screen capture outside the Codex sandbox, so the PNGs and `render-manifest.json` are produced by the command in the handoff below.

**V1-compact-pill** is 480 × 560 DIP with 14 DIP body text and 24 DIP editor insets. Its icon-only top hover bar and small bottom count/dot pill are the closest AntiNote-like answer. It trades writing room for a very quiet footprint and would need a function task for hover-reveal timing plus fade-out when inactive before the UI build.

**V2-roomy-calm** is 760 × 680 DIP with 15 DIP body text and 48 DIP insets. Full-width top and bottom bars use hairline rules and keep the navigation, pager, position and save line in a generous rhythm. It trades compactness for readability and would need a function task for the bar reveal/fade policy and whether the bars remain available while the window is inactive.

**V3-paper-grid** is 480 × 560 DIP with 14 DIP body text and 24 DIP insets. It keeps V1's compact floating bars and adds a sparse dotted paper texture above the tint, with transparent space between the dots. It trades some visual restraint for a tactile writing cue and would need a function task for paper-texture preference and reduced-motion/contrast behavior.

**V4-edge-strips** is 560 × 640 DIP with 14 DIP body text and 32 DIP insets. Full-width top and bottom edge strips carry the title/date and count/save state, while the editor remains the dominant surface. It trades overlay subtlety for persistent context and would need a function task for strip fade timing, inactive-window dimming and date-state wording.

**V5-bare** is 520 × 600 DIP with 14 DIP body text and 28 DIP insets. Idle shows only a faint `3 / 8` at the bottom-right; forced hover overlays compact bars on the text edges with every Quiet route. It trades discoverability for the least visual furniture and would need a function task for reliable hover reveal, keyboard discoverability and an auto-hide timeout.

The synthetic backdrop is a borderless, topmost, non-activating Avalonia window filled with a fixed generated composition of warm/cool colour fields, high-contrast shapes, lines and large text. Every variant is also topmost and is captured only after at least 650 ms of settling. Native GDI capture uses the physical window rectangle plus a 24-pixel margin, asserts that the rectangle is fully inside the backdrop, records the actual pixel size and DPI scale, and compares V1 tint 0% against tint 100% before choosing inactive or activated capture. The glass check uses the same pixel-difference measurement as the backdrop-through proof.

The backdrop is synthetic and does not predict a user's actual wallpaper, monitor arrangement, accessibility scaling or GPU behavior. Hover is forced for the design stills; no live pointer timing is claimed. The chosen variant remains a design candidate until its real shell integration and the behavior tasks above are completed.

Re-render one variant from `src/AntiQuickPad`:

```powershell
dotnet bin/Release/net10.0/AntiQuickPad.dll --render-variants ../../docs/design/m8 --only V1-compact-pill
```

## Built

M8c-v1-compact-shell built the selected V1-compact-pill as the production Quiet shell on 2026-10-21. Resident defaults are 480 x 560 DIP (minimum 360 x 420), with AcrylicBlur -> Mica -> None fallback, one 90% Quiet canvas tint, and transparent editor/template/carousel surfaces. Light and dark are implemented. Existing theme preferences and current-version saved bounds still apply.

The 38 DIP top strip retains native dragging while its Pin / New note / More / Hide controls reveal on pointer presence, bar keyboard focus, or an open More menu. The 36 DIP bottom presentation shows navigation, position, selectable pager dots, save status and Escape hint. Reveal is view-only: 120 ms in, 400 ms leave linger, 150 ms out. The existing Minimize toolbar control lives inside More to preserve its declared route. Editor text is 14 DIP mono; the 24 DIP editor inset surrounds the existing 24 DIP folding gutter. The empty hint aligns with the first text line; the editor viewport ends above the footer.

All four sub-step build/selftest/render gates passed, including conformance, ownership boundary, alignment and renderer pixel proofs. Final hidden renders: `.coord/scratch/M8c/c4-light/` and `c4-dark/` (idle=`main`, revealed=`bars-hover`, plus menu/carousel/fold/empty states, each at 1x and 1.5x). These RTB images do not establish live Acrylic appearance. No --measure was run. Physical hover/drag/focus, live material quality, and performance remain for the manager/user. No reduced-motion flag is exposed by the view's current Avalonia platform-settings API; the timings remain fixed.

Full implementation, proof deltas, gate evidence and remaining differences: `.coord/reports/M8c-v1-compact-shell.md`. This Built section supersedes the earlier candidate text's suggestion that hover presentation requires a function task; no new command, setting, persistence or controller behavior was needed.
