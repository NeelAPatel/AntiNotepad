# Feedback-fix verification — Spike A

2026-10-17. Current build: `spikes/A/bin/verify/retest/InputProbe.dll`. Status: fixes built and self-checked; short physical retest pending.

## Changes

1. Shift+Tab now removes up to four leading spaces or one tab from the current line with only a caret, and from each selected line for a selection. A shared handler/helper maps caret and selection offsets after edits. The window intercepts Tab only when an editor has keyboard focus, before focus navigation. Multi-line edits use one document update/undo group. Regression checks cover caret-only, inside-indent clamping, tab prefixes, selections and undo.
2. Wheel input is a page request per spec 4b. First horizontal delta immediately starts a 220 ms cubic slide; repeated burst deltas do not restart its clock or queue additional pages. A 140 ms idle gap ends a burst only after the active slide completes. Reversal retargets from the actual displayed position. Newest-edge creation commits once at burst completion; drag remains pointer-bound with real release and rubber band.
3. Checkbox hover/click use one scroll-aware hit test. Marker hover sets Hand on editor/TextArea/TextView and a `Toggle task checkbox` tooltip; moving away restores IBeam. Physical cursor/tooltip perception needs the retest.
4. Registered chord is now Ctrl+Alt+Shift+A, shown in the persistent footer and tray tooltip; if registration fails, both say unavailable and point to tray. Tray click/menu always show/focus, even when already visible. Hide remains a toolbar action and Escape. Footer cues survive material/folding changes and wrap within the window.

## Verification

- [Offline build](build-feedback.txt): zero warnings/errors, final build 22.43 s. No restore/network/package change. Isolated retest output avoids the older running build lock.
- [12 smoke checks](smoke-feedback.txt): 12/12 pass, 57.6 ms. Tests exercise shared key helper and burst state, not physical key/wheel input.
- [Runtime self-check](ui-feedback-log.txt): template/layout/focus and insert/undo pass; tray-show handler restores the same editor with focus, initializationCount=1. Actual tray notification delivery is not injected.
- [Write-process log](persistence-write-log.txt) / [new-process read log](persistence-read-log.txt): actual SpikeWindow document edit/autosave, process exit, relaunch through normal load path, exact marker restored. Both exit 0. Isolated temporary data; user notes untouched. Wall durations 1240.0 ms write process, 1237.0 ms read process (full lifecycle, not launch-to-edit latency). Machine-readable record: [persistence-feedback.json](persistence-feedback.json).
- [Framework preview](render-feedback.png) inspected: readable fixture, visible Hide, persistent footer reopen cue. This render excludes Windows compositor backdrops.

## Timing / memory

Debug framework-dependent build, Windows 10.0.26340 / .NET 10.0.13, 96 DPI. Single observations, not a cold-cache benchmark.

| Measure | Result and scope |
|---|---|
| Main entry → window Opened | 1882.4 ms UI run; 927.8 / 957.4 ms persistence processes |
| Main entry → reported editor focus | 2070.5 ms UI run; excludes dotnet host before Main and first-paint/typing proof |
| Hide dispatch | 3.19 ms; save runs before timed Hide; no desktop paint/disappearance observation |
| Show request → reported focus | 3.2 ms through same tray-show handler; show method dispatch 1.712 ms; excludes physical click/hotkey delivery and compositor latency |
| Hidden 1-second sample | Private bytes 116,822,016 (~111.4 MiB), working set 161,976,320 (~154.5 MiB); not long-idle steady state |
| Physical hotkey → visible/focused | Still requires user; registration error 1409 in this run because chord is occupied (older app remains running) |

## Short retest

Close/Exit the older spike before launching the retest DLL so it releases Ctrl+Alt+Shift+A. Follow [RETEST.md](RETEST.md): caret/selection Shift+Tab+undo; notch/burst page slides+drag; checkbox hover+click; Hide → tray reopen → hotkey reopen. Return four PASS/FAIL notes and newest spike log. This retest decides perceived smoothness/affordance and actual physical shortcut/tray delivery.
