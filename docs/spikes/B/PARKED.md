# Spike B (GPUI + GPUI Kit): PARKED 2026-10-17

Parked by the user's decision while comparing against spike A (user leans A). Not abandoned for technical failure; Codex usage limit hit mid-fix and the user chose to stop investing. Resume only if A fails or the user asks.

## What exists (committed; last unfinished edits committed as WIP)
- `spikes/B/`: Rust, GPUI snapshot 0.3.9 (`gpui-pre`) + gpui-kit 0.7.3, pinned, `Cargo.lock` kept, vendored Windows backend patch in `spikes/B/vendor/gpui-pre-windows` (direct manipulation/events changes).
- Builds: `cargo build --offline --locked` passes (5.7 s incremental, 2m28s clean on VS 2026). 15 cargo smoke tests passed at the last Director checkpoint. Env: `CARGO_HOME=spikes/B/.cargo-home`, `CARGO_TARGET_DIR=spikes/B/target`, `%USERPROFILE%\.cargo\bin` on PATH.
- Features implemented: notched-wheel page switching (one page per notch, coalescing), mouse-drag carousel with spring/rubber-band and new-note creation, decorated source markdown, wrapped clickable checkboxes, Tab/Shift+Tab, line numbers/folding, Python fence colors, autosave (`notes.bin`), tray + hotkey thread, material buttons (Transparent/Mica/Blur).
- Reports: `.coord/reports/T-003.md`, `docs/spikes/B/REPORT.md`, `USER-FEEDBACK.md`, `runtime.tsv`, input probe trace `input-probe.tsv`.

## Findings that matter for the decision
- Gate 1 (input): PASS for notched wheel (WM_MOUSEHWHEEL 120/notch, 279 events) and mouse drag with real press/release; touchpad finger-lift not proven (untested; no touchpad).
- User: much smoother animation than A with occasional stutter; nicer text rendering overall; GPU ~4-5% while swiping (no baseline captured).
- Transparency/Mica/Blur: effectively broken (also in A); de-prioritized.
- Toolchain friction: Rust/MSVC setup ok; crates need network, so the Codex sandbox cannot fetch (manager fetches, Directors build offline).

## Known open issues at parking
1. `SHELL ERROR: tray/hotkey unavailable`: tray and Ctrl+Alt+Space did not start on the user's machine. Diagnose from `docs/spikes/B/runtime.tsv` (A found Ctrl+Alt+Space collides on this machine, error 1409; try Ctrl+Alt+Shift+A).
2. Occasional animation stutter, not analysed.
3. Idle vs animating GPU/frame counts not measured. `spikes/B/src/perf.rs` and `shell/selfcheck.rs` are the Director's unfinished measurement scaffolding (compiles).
4. Text-rendering settings only partly recorded (a `TEXT_SETTINGS` log line was added).
5. Not measured: cold launch, hotkey latency, idle RAM (a `measure-spike.ps1` exists, user ran a partial pass; ~20 MB working set seen once in Task Manager).

## To resume
`codex exec resume 01a14851-bb52-7f90-98bf-36f7b9a3a5f9 -c 'windows.sandbox="unelevated"' --skip-git-repo-check -` with the open issues above (needs Codex usage reset, which was 3:12 AM on 2026-10-17). Or hand the issues to a Claude Sonnet session.

## Why GPUI could still win later
Smoothness and text rendering were better in the user's test. Cost: fewer ready-made theme/UI parts, younger toolkit, more hand-built editor behavior.
