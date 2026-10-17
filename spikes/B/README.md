# Spike B — GPUI + GPUI Kit

Disposable full capability spike. Kit 0.7.3 / GPUI 0.3.9, default backend, retained lockfile. No additional crates. Gate 1 accepted for Logitech MX Master 3S notched wheel + mouse drag; touchpad optional/untested.

Build and smoke checks (offline):
```powershell
.\spikes\B\build.ps1
$env:PATH = "$env:USERPROFILE\.cargo\bin;$env:PATH"
$env:CARGO_HOME = Join-Path $PWD 'spikes/B/.cargo-home'
$env:CARGO_TARGET_DIR = Join-Path $PWD 'spikes/B/target'
cargo test --offline --locked --manifest-path spikes/B/Cargo.toml
```

Launch from a normal interactive shell (with launch/memory measurement):
```powershell
.\spikes\B\measure-spike.ps1
```
Or launch without measurement: `.\spikes\B\launch-spike.ps1`.

- Ctrl+Alt+Space toggles the retained window. Tray right-click: Show / Hide / Quit.
- Horizontal wheel: one page per notch/quick burst, animated. Burst quiet period: 220ms.
- Blue page handle: 1:1 mouse drag; short/slow springs back; distance/velocity selects next. At newest edge, rubber-band pull beyond threshold creates a blank note. Ordinary editor drags select text.
- Click task marker `[ ]` / `[x]` in source; Ctrl+Z undoes the text edit. Source markers remain visible. Editor Tab/Shift+Tab indent selection.
- Transparent/Mica/Blur buttons request materials; title text drags the window. Blur maps to the pinned backend's Acrylic composition path.
- Notes save locally to `spikes/B/data/notes.bin` (git-ignored), including stable IDs and active page. Corrupt restore disables autosave to preserve the existing file.

Exact user validation steps: ../../.coord/status-B.md. Runtime log: ../../docs/spikes/B/runtime.tsv; measured JSON: ../../docs/spikes/B/metrics-<pid>.json. Original Gate 1 evidence: input-gate1.tsv and gate1-summary.json. Old probe source is archived as src/input-probe.rs.disabled; launch-probe.ps1 now aliases the full-spike launcher to protect gate evidence.

Build/model smoke checks do not prove UI behavior. Full runtime, materials, editor interaction, focus, latency, RAM and visual checks remain pending. App-owned Markdown/Python scanner is fixture-oriented, not a complete language parser. Diagnostic append I/O and debug build affect performance comparisons. Do not use this spike as production storage.
