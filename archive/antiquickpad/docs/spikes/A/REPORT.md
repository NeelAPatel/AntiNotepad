# Spike A report — T-002

Updated 2026-10-17. **Working prototype with user-confirmed core capabilities; feedback fixes built and awaiting short retest.**

Current launch: `dotnet .\spikes\A\bin\verify\retest\InputProbe.dll`. Exit the older copy first to free Ctrl+Alt+Shift+A. Latest details/evidence: [FEEDBACK-FIXES.md](FEEDBACK-FIXES.md); short user steps: [RETEST.md](RETEST.md).

## Results

| Spec capability | Result |
|---|---|
| Editor focus / typing | PASS user hands-on; missing AvaloniaEdit theme and TextArea focus path repaired earlier. Runtime focus/layout remains PASS. |
| Source-preserving Markdown / Python fence styling | PASS user hands-on: heading colors, bold/italic/strike with visible markers, Python keywords/strings. Local bounded regex transformer, not a full parser. |
| Clickable checkbox / undo / caret | Click PASS user (rough affordance). New shared hit-test hover Hand cursor/tooltip implemented; caret/undo helper PASS smoke. Hover requires short retest. |
| Tab / Shift+Tab | Tab previously worked; Shift+Tab bug fixed in caret-only and selection paths, including tab/space prefixes, caret mapping and grouped undo. Shared regression checks PASS; physical Shift+Tab retest pending. |
| Mouse wheel carousel | User navigation PASS, perceived hitchiness reported. Replaced per-notch offset/idle snap with immediate 220 ms page slide, coalesced bursts and continuous reversal retarget. One edge creation per burst. Model PASS; perceived smoothness needs retest. |
| Drag / release / rubber band | Dedicated grab strip remains pointer-bound with capture/release, distance/velocity target selection, bounded edge resistance and new-note creation. Model checks PASS; physical drag/cancel behavior needs acceptance. |
| Blank-note pruning / wrapping / vertical scroll | Wrapping observed; creation prunes blank interiors and retains newest blank. Vertical deltas remain editor-owned. Pruning model PASS; physical selection/scroll arbitration not newly claimed. |
| Resident frameless + tray / global hotkey | Built. Same editor/window and focus restored through tray show handler; one initialization PASS runtime. Footer+tray state explicitly show registered Ctrl+Alt+Shift+A or unavailable. Older running copy occupies the chord in latest check; actual tray/hotkey physical delivery needs retest. Hide/Exit stay reachable. |
| Transparent / Acrylic / Mica and text clarity | PASS user hands-on on all three. Runtime ActualTransparencyLevel also matched requests. |
| Autosave / restore on relaunch | PASS automated real-process test: document edit → autosave → exit → new SpikeWindow loads exact marker, isolated data. Content/order persist; current index and undo history do not. |
| Folding / line numbers | PASS user hands-on; folding gutter differs from user's envisioned interaction. Fold ranges are a one-shot probe, not maintained live. |
| Small smoke checks | Latest 12/12 PASS, 57.6 ms; offline build zero warnings/errors. Runtime reopen/template/edit+undo PASS. |
| Hardware | Logitech MX Master 3S per user, driver unknown. Original probe: native ±120 / Avalonia ∓1 notches; hi-res/inertial drivers and touchpad optional/untested. |

The manager-recorded hands-on results are [USER-FEEDBACK.md](USER-FEEDBACK.md). The old 120 px wheel step and finger-lift gate no longer describe the current requirement; spec 4b page switching controls wheel behavior.

## Measurements / evidence

Latest Debug run: Main→Opened 1882.4 ms, Main→reported editor focus 2070.5 ms (dotnet host time and compositor paint excluded). Show request→reported focus 3.2 ms; Hide dispatch 3.19 ms. Hidden 1-second sample: private ~111.4 MiB, working set ~154.5 MiB. These are single scripted observations, not physical hotkey-to-paint or cold-cache benchmarks. Full scopes and process-pair durations are in [FEEDBACK-FIXES.md](FEEDBACK-FIXES.md).

[Offline build](build-feedback.txt), [smoke output](smoke-feedback.txt), [UI log](ui-feedback-log.txt), [write log](persistence-write-log.txt), [read log](persistence-read-log.txt), [render preview](render-feedback.png). Preview is framework-rendered; user-reported desktop material readability comes from hands-on feedback, not that preview.

## Verdict and effort

Would I build the real app on Avalonia? **Yes, provisionally, after the short interaction retest.** User testing now supports the editor/material basics and the actual autosave process pair works. Final comparison with B and a production performance decision remain manager-owned. The remaining physical checks are wheel animation feel, Shift+Tab routing, hover and tray/hotkey delivery.

Production work still includes persistence recovery/debounce (spike currently saves synchronously), a robust Markdown/Python parser or maintained grammar path, folding refresh and meaningful cold/hot latency benchmarking. Budget roughly half a day to a day for feedback tuning, then several days for a production-shaped follow-up before committing to speed claims; this is an uncertain planning estimate, not measured effort.

Scope: one Luna high worker implemented feedback fixes exclusively in spikes/A; Director reviewed/integrated and ran verification. No system software/new dependencies/network restore, no B/shared STATUS edits. Current coordination checkpoint is .coord/status-A.md. Local commit remains blocked by .git write denial in this sandbox; explicit manager commit paths are recorded there. Director stopped for the requested short interactive retest.
