# Spike B report — T-003

Updated: 2026-10-17. Owner: spike-b-director.
**Gate 1 PASS on Logitech MX Master 3S. Remaining spike implemented and build/smoke-checked; integrated physical validation pending.**

Would I build the real app on this? **Conditional only.** The current hardware input gate passes and the pinned stack compiles, but a stack decision still needs the integrated editor, animation, materials, shell/focus and performance checks below. No completed runtime capability or performance claim is inferred from compilation.

## Confirmed evidence

Manager's clean hello build: **2m28s**. Rust/cargo 1.99.1 MSVC, CMake 4.4.4 and VS2026 Community C++ tools compiled the debug spike; no VS2022 fallback needed. Kit/component 0.7.3, GPUI 0.3.9, default backend and Cargo.lock retained. No new crate, lockfile dependency change, installation or network use this resume. [Final incremental build timing](build-probe-result.txt), [build log](build-probe.log), [smoke log](smoke.log).

Final incremental full build: **6.1391557s** (debug). Rust formatting and four PowerShell script syntax checks passed.

**15/15 cargo smoke checks pass** for notebook/gesture capabilities: stable notes/defaults, coalesced fractional/burst wheel paging, fresh burst after quiet, newest-note creation, direct drag following, distance/velocity selection, stale velocity/no-motion rejection, spring-back/settling, newest-edge resistance and creation threshold, blank pruning/index remapping, Unicode/current-page save/restore and corrupt/truncated input rejection. These are model/persistence tests, not GUI tests. Source files carry `# spike` comments; no exhaustive suite added.

Gate 1 user trace: **279 NATIVE_WHEEL + 279 PUBLIC_SCROLL**, **433 MOUSE_DRAG_MOVE**, **11 presses / 10 releases**. Public and drag events have editor_focus=true. No F-key labels were pressed; manager explicitly accepts the gate with the user's confirmation of completed actions. Mouse: **Logitech MX Master 3S**, driver version **unknown**.

The trace is more varied than the manager's shorthand: **120 horizontal / 159 vertical native wheel events**, raw ±120 and occasional ±240, public horizontal line deltas ±3/±6 with normalized notch steps ±1/±2; vertical rows have dx=0. The characteristic +120 WM_MOUSEHWHEEL notch maps to public dx=-3 and notch_steps_x=-1. The 11/10 press/release imbalance is recorded rather than treated as proof every cancellation path is delivered. [Original gate trace](input-gate1.tsv), [counts/distribution](gate1-summary.json).

## Behavior and acceptance matrix

Spec 4b supersedes earlier wheel-release interpretation: **wheel switches pages; mouse drag supplies direct manipulation**. The implementation normalizes line deltas using Windows scroll chars, advances one page immediately, then coalesces further events until **220ms quiet**. One wheel burst can create at most one new note at the newest edge. Optional pixel input has a provisional 40px/step scaling; smooth wheels/driver inertia are **untested**, not validated by notched-wheel evidence.

Mouse drag uses a dedicated blue page handle so editor drags retain text selection. Between existing notes it follows pointer displacement 1:1; release uses **0.35 page distance or 1 page/s velocity**, ignoring velocity older than **150ms**. At newest edge, displacement is resisted by **0.48**, and creation requires the distance threshold even for a fast flick. Spring integration is damped and frame-driven. Blank interior notes are pruned; stable IDs preserve retained editor entities and per-note editing state.

| Spec item | Implementation/evidence | Runtime result |
|---|---|---|
| Input Gate 1: notched wheel + mouse press/drag/release | User trace + manager acceptance above | **PASS** for current hardware |
| Wheel page switch, animated/coalesced, bounded newest creation | Model + 15-check suite | **PENDING** integrated feel/direction check |
| Direct drag, distance/velocity, spring, rubber band, note creation/pruning | Dedicated page handle + model checks | **PENDING** visual/arbitration check |
| Wrap, vertical scrolling, text selection | Persistent Kit Editor; horizontal wheel capture only | **PENDING** full-app check |
| Tray/global hotkey, resident frameless window | Native Shell_NotifyIcon/RegisterHotKey; own message thread; close hides; exact HWND binding | **PENDING** registration/focus/show-hide check |
| Transparent/Mica/blur/Acrylic and text crispness | GPUI material controls; old DWM backdrop reset | **PENDING** visual result/fallback |
| Heading colors by level; bold/italic/strike, markers visible | App-owned source scanner; six heading styles | **PENDING** rendering check |
| Clickable task in wrapped Markdown; undo/caret stable | Actual marker hit via editor range_to_bounds; undoable same-length replace, click caret restored | **PENDING** physical click/undo/caret check |
| Tab/Shift+Tab selection indentation | Native editor action bindings | **PENDING** selection check |
| Python fence highlight, rest Markdown | Small app-owned lexical scanner; no new grammar crate | **PENDING** visual result; fixture-oriented parser |
| Autosave/restore | 500ms dirty save + hide/close/quit; length-prefixed UTF-8; temp/sync/atomic replacement; corrupt restore disables saving | Model round-trip **PASS**; app relaunch **PENDING** |
| Folding/line-number wishlist | Explicitly enabled; heading/fence FoldRange supplied | **PENDING** gutter behavior |
| Cold/editable, hotkey/focus, hide, RAM | Instrumented + measurement script | **PENDING** user run |
| Touchpad lifecycle; smooth/hi-res wheel/inertia | Historical source audit and diagnostic seam only | **OPTIONAL / UNTESTED** |

The Markdown/Python scanner is a bounded spike implementation, not complete CommonMark or Python grammar coverage. Task clicks act on canonical source marker characters rather than a separate preview/widget; marker geometry comes from the editor's own wrapped layout. Physical stability remains to be verified.

## Native implementation and limits

Exact restored Windows source is gpui-pre-windows 0.3.9 / Zed revision 93d745668805029476e6d7fbb19652116fed51de; [source audit](input-source-audit.md), [small diagnostic patch](native-diagnostic.patch). Cache unchanged; vendored Apache license retained. The stock touchpad Ended follows READY/interrupted inertia, which remains optional evidence after the hardware gate change.

Shell uses first-party Win32 APIs via std FFI, no extra crate. Tray/hotkey thread handles native show/hide and sends events to GPUI for focus/save/quit. Main polls at 50ms even while hidden; animations request render frames separately. Registration collisions/errors are logged and surfaced; exact same-process `Zed::Window` title binding avoids helper windows. Foreground restrictions, hidden timer liveness, tray menu behavior and retained focus need a real user run.

Pinned Windows source maps Blurred to composition state 4 (Acrylic path), Transparent to state 2, and Mica to DWM main-window backdrop. These are requests; Windows settings/driver behavior may change their visible effect. Actual material status is not exposed as a confirmed success result by the GPUI setter.

Local notes: `spikes/B/data/notes.bin` (git-ignored), stable IDs/active page. Prior saved file is never deleted before successful replacement. Restore signature/size/UTF-8/ID checks fail closed; the UI disables autosave if existing data cannot be restored. No note text is written to diagnostic logs.

## Measurements and next physical test

Run from an interactive user PowerShell:
```powershell
.\spikes\B\measure-spike.ps1
```

The script observes process launch to FIRST_EDITABLE_FRAME (20ms polling overhead), then samples working-set/private memory after 10 seconds idle into `docs/spikes/B/metrics-<pid>.json`. Main also records main-entry-to-frame time and editor logical focus. `runtime.tsv` records hotkey receipt, native show/hide call durations, native visible+foreground confirmation, GPUI editor refocus and next editable frame. Derive hotkey-to-frame from that sequence after user toggles. Poll cadence, debug build and diagnostic I/O affect these measurements. No runtime timings or RAM values exist yet for the full app.

Follow the consolidated test in [.coord/status-B.md](../../../.coord/status-B.md). Launch-only alternative: `spikes/B/launch-spike.ps1`; legacy launch-probe.ps1 now delegates there to protect the archived gate trace. No Computer Use or launch attempt made this resume. No screenshot is claimed; visual verification remains pending.

Remaining effort estimate (planning judgment): one user validation pass, then ~0.5–2 days if integration defects appear. A substantial editor/input rewrite would change the verdict. Lead reviewed integration and a read-only Luna follow-up found no confirmed source-level runtime blocker; neither review proves user-visible behavior.

Task is paused at required interactive validation, not complete. Both requested reports and status are updated. Local commit remains blocked by prior read-only .git/index.lock failure; no unchanged retry. Shared STATUS.md and all Spike A files untouched.

