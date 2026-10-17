# Decisions log (append-only)

- 2026-10-17: Two spikes first, A = Avalonia + AvaloniaEdit, B = GPUI + GPUI Kit. C = Tauri + CodeMirror 6 only as backup if A/B fail badly.
- 2026-10-17: Not a 1:1 AntiNote copy; take functionality, choose our own methods. Least-resistance library choices.
- 2026-10-17: Code notes: fenced blocks first, then first-line `python:`; drop `code: py` unless trivial. Checkboxes via `- [ ]`. Folding and line numbers are wishlist, not blockers.
- 2026-10-17: Carousel is a must: direct manipulation drag-to-snap, position bound to the gesture.
- 2026-10-17: Codex first, Claude manages. Directors run headless (`codex exec`) with files for state; Claude helpers are visible desktop sessions.
- 2026-10-17: Luna worker = `gpt-6-luna` high (no 6.1 Luna exists locally).
- 2026-10-17: T-001 reviewed by manager: GO spike A, conditional GO spike B, both gated first on a precision-touchpad finger-lift probe (neither framework proven to expose release vs inertia). WinUI/WPF custom editor and WinUIEdit: NO-GO. C stays backup. Directors run all milestones in one turn (ending a turn leaves them idle).
- 2026-10-17: Dispatch route = Codex app sessions + codex queue (codex exec sandbox fails here). One named app session per Director: spike-a-director, spike-b-director.
- 2026-10-17: User has no touchpad, only a mouse with a horizontal scroll wheel. Carousel gate redefined: horizontal wheel deltas + mouse drag are primary; release = idle gap after last wheel delta; touchpad finger-lift probe becomes optional. Spike A/B Directors to be resumed with this change.
- 2026-10-17: Mouse = Logitech MX Master 3S. Horizontal wheel switches pages (one notch = prev/next note, animated); mouse drag is the direct-manipulation input. Spike B input probe captured 279 wheel + 11 press/10 release drag events with no F-key labels.
- 2026-10-17: Spike A hands-on by user: core features work (focus, decorated markdown, python fence, checkbox, folding, line numbers, wheel page switch, Transparent/Acrylic/Mica). Rough edges: wheel animation hitchy, no checkbox affordance, Hide untested. See docs/spikes/A/USER-FEEDBACK.md.
- 2026-10-17: User feedback after both prototypes: transparency/Mica/Blur effectively broken in both -> DE-PRIORITIZED. Product goal: native-Mac-app (AppKit/Swift) level fit and finish. Spike B smoother with stutter, nicer text; spike A text has parts the user prefers. B tray/hotkey SHELL ERROR to be diagnosed. Wait for both Directors to finish before further user tests. See docs/spikes/*/USER-FEEDBACK.md.
- 2026-10-17: Spike B PARKED by user (user leans Avalonia/A). Documented in docs/spikes/B/PARKED.md. UI demo rule: realistic Avalonia-only renders, no other engines without disclosure; at least 5 AntiNote-suited variants requested (demos/ui-variants/).
- 2026-10-17: Spike A retest: all 4 checks PASS (Shift+Tab, wheel+drag, checkbox hover/click, Hide/reopen by tray and Ctrl+Alt+Shift+A). Animation feel 'a bit weird' but acceptable for a spike. Spike A is now the leading stack; stack decision and real implementation plan are the next step (planned for a fresh Opus chat).
- 2026-10-17: docs/spike-spec.md carousel item consolidated (old 4/4a/4b merged into one item 4: wheel = page switch, drag = 1:1, touchpad optional).
- 2026-10-17: Stack CONFIRMED by user: Avalonia + AvaloniaEdit. UI look undecided; theming via one Look record, pick after real app (M6). Plan: docs/implementation-plan.md.
