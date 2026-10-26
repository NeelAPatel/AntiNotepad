# A41-padwindow-split-impact: READ-ONLY impact analysis of the PadWindow god class

Role: director (gpt-6.1-sol medium; delegate reading to luna xhigh subagents if useful). Branch `reorg/A41-padwindow-split`
(this worktree, off dev). **READ-ONLY: do not edit, create, move or delete any file except your report
`.coord/reports/A41-padwindow-split-impact.md`.** Do not build into the tree, do not launch or kill AntiNotepad.
Product: `product/AntiNotepad`. Read `AGENTS.md`, `docs/decisions.md` (taskbar stand-in A20/A22-A25, session end A26, A30
background save, native-window history A32/A34) first.

## Context
`UI/PadWindow.cs`, `.Layout.cs`, `.Navigation.cs`, `.Keys.cs`, `.TopBlur.cs` (about 2.0k lines) is a god class. It owns window
show/hide/prewarm/fade, layout and bars, keys, note navigation and swipe, clipboard watching, top-bar blur, global hotkeys,
taskbar stand-in glue, timers, the 400 ms save loop, working-set trim and split panes. Everything reaches into it. The
planned task `A41-padwindow-split` will pull non-window responsibilities (candidates: note navigation/stack, clipboard
watcher, save loop, hotkey registration, taskbar presence glue) into small classes the window owns and talks to through
narrow interfaces/events, leaving only true window concerns in the partials. It must be behavior-neutral and must not slow
show/hide. Window behavior is fragile (A32/A34 broke corners, shadow, bar styles and were reverted).
Parallel tasks to account for: `A39-motion-out-of-ui` (moves `Anim`, `ChromeTokens`, `MenuEntry` out of `UI`, adds a
table-driven `LAYERS` selftest; assume it lands first), `A40-settings-change-event` (replaces hand-called
`SettingsChanged()` with a `Changed` event), `A42-padview-commands`, `A43-app-context` (`Program` statics),
`A44-platform-seam` (`Native.*` calls out of UI).

## Analyze (all of `product/AntiNotepad`, not only UI)
1. **Responsibility map**: for each responsibility inside the PadWindow partials list: what it does, file and line range, the
   fields/state it owns, its members, and its dependencies (other responsibilities, `Program`, `Store`, `Native`, `PadView`).
2. **Callers**: who calls each member, inside PadWindow and outside (`Program.cs`, `Tray`, `SettingsWindow`, `SearchOverlay`,
   `NoteSwipe`, `PadView`, `PadMenu`, selftests). Give the call sites (file:line).
3. **Shared mutable state** read or written by more than one responsibility (flags, current note, pane list, timers).
4. **Timers and dispatcher use**: every timer/`Dispatcher.UIThread` post, its interval, owner, start/stop points.
5. **Focus, activation and hide-on-blur ordering**: the exact sequence on show, hide, deactivate, prewarm, hotkey press,
   taskbar-button click, tray click, settings open/close. What must stay in order.
6. **Hot path for show/hide**: every call from hotkey press to visible-and-focused and to hidden; flag anything an extraction
   would add to it (allocations, indirection, events, lazy init).
7. **Extractability**: per candidate responsibility say SAFE (self-contained, few callers), ENTANGLED (shares state with window
   concerns; say what), or LEAVE. Propose the narrow interface/events for each safe one.
8. **Recommended extraction order**, smallest risk first, as sub-steps that each build and pass `--selftest` alone.
9. **Risks**: focus, activation, hide-on-blur, prewarm, taskbar stand-in (A22-A25), A30 background save, Windows session end
   (A26), split panes, top-bar blur, anything where a silent regression is likely and how it would show up.
10. **Guards**: proposed `--selftest` checks and table-driven `LAYERS` rules (e.g. which types may not reference `PadWindow`,
    `Navigation` must not reference `Avalonia.Controls`, extracted classes must not reference `UI`), plus grep checks,
    that would catch regressions cheaply. Say which can be written without a window and which need the dev copy by hand.
11. **Overlap** with A39, A40, A42, A43, A44: list the exact members/lines in the PadWindow partials each of them touches.

## Report
`.coord/reports/A41-padwindow-split-impact.md`, with sections matching 1-11, tables where it helps, file:line references,
and a final "uncertain" list of things you could not confirm from the code alone.

## Done means
Only the report file exists as a change (`git status` shows just it). Every claim carries a file:line.
