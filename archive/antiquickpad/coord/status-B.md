# Spike B checkpoint — T-003

Updated: 2026-10-17. Owner: spike-b-director.
Phase: Gate 1 PASS; full spike IMPLEMENTED + OFFLINE BUILD/SMOKE PASS; WAITING ON USER integrated runtime test.

## Milestones / evidence
- Toolchain: Rust/cargo 1.99.1 MSVC, CMake 4.4.4, VS2026 Community C++ tools. Manager clean pinned hello build 2m28s; no VS2022 fallback. Kit/component 0.7.3 and GPUI 0.3.9, default backend, Cargo.lock retained. No new dependencies or network this resume.
- Gate 1 PASS, explicitly accepted by manager: Logitech MX Master 3S (driver version unknown), no touchpad. Verified 279 NATIVE_WHEEL paired with 279 PUBLIC_SCROLL; 433 MOUSE_DRAG_MOVE; 11 press / 10 release; public/drag events editor_focus=true. No F-key labels; user/manager confirm all actions performed. Trace actually includes 120 horizontal and 159 vertical events, both directions and ±120/±240 raw deltas. Complete trace preserved as docs/spikes/B/input-gate1.tsv; gate1-summary.json contains totals/distribution. Optional touchpad and unavailable hi-res/inertia hardware untested.
- Spec 4b implementation: one normalized wheel step switches one page immediately with spring animation; further wheel events coalesce until 220ms quiet. Mouse drag on the blue page handle follows 1:1 between existing pages; distance >=0.35 page or velocity >=1 page/s selects, otherwise springs home. Velocity older than 150ms is ignored. Newest-edge drag rubber-bands at 0.48 and requires distance threshold to create; wheel beyond newest creates once per burst. Blank interior notes pruned, stable note IDs retained. Ordinary editor drag stays text selection.
- Three Luna high implementation workers owned src/model, src/editor and src/shell. Director integrated and reviewed; follow-up read-only review found no confirmed runtime-breaking source defect. Runtime remains unverified.
- Windows native tray + Ctrl+Alt+Space thread, exact same-process Zed::Window binding, retained hide/show, foreground restore request, tray Show/Hide/Quit, close-to-hide. Registration errors surface. GPUI-side focus restored after show. No app launch this resume.
- Transparent/Mica/Blur controls; pinned Blurred path uses Acrylic composition state 4. Reset prior DWM backdrop before switching paths. Chrome-free window, draggable title text. Actual material/text quality awaits visual check.
- Source editor: visible heading/emphasis/strike markers, app-owned fixture-oriented Markdown/Python scanner, clickable actual [ ]/[x] text using editor range geometry and undoable replace, soft wrap, explicit line numbers/folding. Tab/Shift+Tab use native editor actions. Wrapped checkbox undo/caret stability still needs physical verification.
- Autosave every 500ms when dirty plus hide/close/quit; stable IDs/current note persisted as length-prefixed UTF-8 in spikes/B/data/notes.bin (git-ignored). Temp+sync+Win32 atomic replace retains previous save on failure. Corrupt restore fails closed (autosave disabled). Smoke tests cover persistence round-trip/corruption.
- Full offline locked build passes; 15 cargo smoke tests pass. Final log/timing: docs/spikes/B/build-probe.log, build-probe-result.txt, smoke.log. Formatting and script syntax checked. Diagnostic/debug build; no runtime performance numbers fabricated.
- Prepared measure-spike.ps1: user launch-to-first-editable-frame observation (20ms polling), then working-set/private-memory sample after 10 idle seconds. Runtime HOTKEY/SHOW/SHOW_READY/EDITOR_REFOCUS/SHOW_EDITABLE_FRAME/HIDE records support later latency analysis. Measurements, screenshots, UI acceptance pending.
- Earlier sandbox shell launch/Computer Use rejection are historical; no retry this resume. Shared STATUS.md and Spike A untouched.

## Waiting on user — integrated spike test

1. In a normal interactive PowerShell at repository root run:
   ```powershell
   .\spikes\B\measure-spike.ps1
   ```
   Leave the app idle for 10 seconds after it appears so RAM can be sampled. If no window within 20 seconds, or the header says SHELL ERROR, stop and report it. The script writes docs/spikes/B/metrics-<pid>.json; runtime.tsv contains diagnostics. No F-key labels needed.
2. Confirm the fixture has colored heading levels, bold/italic/strike with visible markers, Python-colored fenced code, and line numbers. Click the long wrapped task's `[ ]` marker, then Ctrl+Z; verify one toggle/one undo and type a character to check caret location. Select multiple lines, Tab then Shift+Tab; check indentation. Try a heading/fence fold gutter and unfold it.
3. Wheel one notch each direction with >250ms pauses: one page per notch, animated. Spin a quick burst: only one page until quiet. Vertical wheel scroll and ordinary editor text-selection drag must still work.
4. Drag the **blue page handle** between existing notes: small slow move springs back; >35% width or a short fast release changes one page. At newest, a short pull springs back; a long leftward pull (roughly 75% width) rubber-bands then creates one blank. Wheel past newest also creates one blank per burst. Make a blank interior note, leave it, and check it is pruned. Drag the title text to move the window.
5. Click Transparent, Mica, Blur; note actual effect/fallback and text crispness against another window. These buttons request materials; source/API names do not prove the visual result.
6. Type a distinctive sentence in a note and wait for Saved. Click Hide, work in another app, Ctrl+Alt+Space to summon, and type immediately. Repeat show/hide about five times; check focus goes back to the prior app on hide and editor caret on show. Use tray right-click Show/Hide once. One process should remain while hidden.
7. Tray Quit (or header Quit), relaunch with `.\spikes\B\launch-spike.ps1`, and verify note text + selected page restored. Quit when finished. Reply “B full spike test complete” with failures and subjective animation/focus/material observations. Leave logs/metrics local for director analysis.

Stop here for this physical test. After return: analyse runtime trace/metrics; fix actual failures and update pass/fail matrix. No final stack recommendation or runtime performance result yet. Touchpad remains optional/untested.

## Local commit
Prior git add failed creating .git/index.lock (read-only .git in this session); no unchanged retry. Task is not runtime-complete. When reviewed in a git-writable session:
```powershell
git add -- spikes/B docs/spikes/B .coord/status-B.md .coord/reports/T-003.md
git commit -m "Implement Spike B carousel editor and resident Windows shell" -- spikes/B docs/spikes/B .coord/status-B.md .coord/reports/T-003.md
```

Final verification checkpoint: offline locked full build PASS in 6.1391557s incremental; cargo test PASS 15/15; cargo fmt --check PASS; all four PowerShell scripts parse cleanly. No B process launched or running at handoff; measurements remain pending user run. Worker smoke PDB removed from source folder. Stop for integrated runtime test above.
