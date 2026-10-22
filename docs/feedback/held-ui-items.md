# Held UI items (original rows)

Removed from TRACKER.md on 2026-10-21 and folded into M6b, M6c, M6d, M7 and F52 so the live list stays short. Kept here verbatim for the UI phase brief. Priority/status at removal were as shown.

Triage 2026-10-21 (user approved, after M8c-v1-compact-shell): done in V1: F4, F40 (visual), F50, F44 placement; obsolete: F26, F35, F36, F38, F41; M6c-design-tokens: F42, F43, F48, F51; M6d-motion-system: F5, F21, F22, F23, F33, F45, F47; re-check on V1: F31, F39, F46, F49; M7-settings: F9, F17.

| # | Feedback | Area | Status | Priority | Where / task |
| --- | --- | --- | --- | --- | --- |
| F4 | Chrome-free hover bars (top: drag, Pin, Minimize, Close; bottom) | Window polish | Later (on hold, UI phase) | Deferred | M6b |
| F5 | Show/hide/quit use standard Windows window animations | Window polish | Later (on hold, UI phase) | Deferred | — |
| F9 | Marker dimness as a user setting | Settings | Pending | P2 | M7 |
| F17 | Strike thickness possibly a setting | Settings | Pending | P2 | M7 |
| F21 | Smooth scrolling (animated wheel scroll) | Editor | Later (on hold, UI phase) | Deferred | M6 |
| F22 | Smooth caret movement (animated caret) | Editor | Later (on hold, UI phase) | Deferred | M6 |
| F23 | Smooth animated folding (AvaloniaEdit folds instantly; needs custom work, risky) | Editor | Later (on hold, UI phase) | Deferred | M6 |
| F26 | Quiet footer: "3 of 8" sits slightly higher than the prev/next arrows (manager spotted in renders) | Editor polish | Later (on hold, UI phase) | Deferred | T-015-alignment-fixes |
| F31 | More-menu items not aligned (section labels, check column, item text) | Shell | Later (on hold, UI phase) | Deferred | T-015-alignment-fixes |
| F33 | Motion everywhere: hover/press/drag get lightweight, interruptible animation tied to the input (Mac-app feel), not fire-and-forget | Motion | Later (on hold, UI phase) | Deferred | visual pass, after design-system research |
| F35 | Card footer: "1 of 4" and "All changes saved" not vertically centred with the floating pager | Shell polish | Later (on hold, UI phase) | Deferred | T-015-alignment-fixes |
| F36 | Quiet footer: "All changes saved" sat 6 DIP above the count, arrows and Esc hint (manager spotted in T-015 renders) | Shell polish | Later (on hold, UI phase) | Deferred | T-015-alignment-fixes (manager follow-up) |
| F38 | Quiet eyebrow "PERSONAL / TODAY" is the same on every note (hard-coded), about 9 px, low contrast; Card shows "Notes" (audit UIA-03) | Shell | Later (on hold, UI phase) | Deferred | audit 2026-10-18 |
| F39 | Scroll rail sits about 2 px inside the code panel's right edge (audit UIA-06) | Editor polish | Later (on hold, UI phase) | Deferred | audit 2026-10-18 |
| F40 | Empty note: caret on line 1 but hint headline about 50 px lower, and the headline reads like note text, not a placeholder (audit UIA-08, UIA-09) | Editor | Later (on hold, UI phase) | Deferred | P1 behaviour |
| F41 | Card neighbour peek shows a clipped "Ne"; left peek is blank (audit UIA-11, UIA-12) | Card shell | Later (on hold, UI phase) | Deferred | audit 2026-10-18 |
| F42 | Hit targets under 24 px (pager dots ~5 px, arrows ~8 px glyphs); set a minimum target size token (audit UIA-13) | Accessibility | Later (on hold, UI phase) | Deferred | M6c |
| F43 | Small text and icons below contrast minimums: Quiet dark footer/eyebrow ~3.5:1, Card light footer ~1.9:1, inactive dots and toolbar icons ~2.4:1 (audit UIA-14) | Accessibility | Later (on hold, UI phase) | Deferred | M6c |
| F45 | Slide takes ~280 ms (spec ~220) and only 8% of travel happens in the first 50 ms, so a wheel notch feels late (audit UIA-16, UIA-17) | Carousel | Later (on hold, UI phase) | Deferred | M6d |
| F46 | Slide landing: ~48 ms frame gap as it lands, active-line highlight jumps rows after landing, edge slivers of chevrons/rail before the incoming text (audit UIA-18..20) | Carousel | Later (on hold, UI phase) | Deferred | P1 behaviour |
| F47 | A previous/next notch at the first/last note does nothing visible; needs boundary feedback (audit UIA-21) | Carousel | Later (on hold, UI phase) | Deferred | M6d |
| F48 | Toolbar is icon-only; tooltips exist (Delete note seen) but More is much smaller than New/Close; accessible names unchecked (audit UIA-02) | Accessibility | Later (on hold, UI phase) | Deferred | audit 2026-10-18 |
| F49 | Menu shows two highlights at once (keyboard-focus on New note stays lit while the mouse hovers another row), and the hover row is narrower than the selected row (click-through capture) | Shell | Later (on hold, UI phase) | Deferred | P1 behaviour (focus/hover state model) |
| F50 | Last lines are clipped by the footer / card bottom edge; no bottom padding or overscroll (click-through captures, both shells) | Editor | Later (on hold, UI phase) | Deferred | P1 behaviour |
| F51 | Keyboard focus is barely visible (Tab onto pager/toolbar shows only a faint fill, no ring) (click-through focus captures) | Accessibility | Later (on hold, UI phase) | Deferred | M6c |
