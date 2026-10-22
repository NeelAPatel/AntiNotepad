# Product checklist: core and product functions (UI-agnostic)

Built 2026-10-21 from `docs/feedback/TRACKER.md`, `docs/implementation-plan.md`, `.coord/STATUS.md`. The tracker stays the live list; this is a dated snapshot (2026-10-21) of it and the seed for M9's function contract table. Superseded as the function list by `docs/function-contract.md` (M9-function-contract-separation part a, 2026-10-21); kept as the dated verdict snapshot.

Legend: `[x]` built and your verdict is pass · `[~]` built, your verdict has open issues · `[ ]` not built.
"Your view" = what you have said or confirmed so far.

## Summon and dismiss (the speed promise)
- [x] Global hotkey toggles the pad (Ctrl+Alt+Shift+A) · M1 · your view: PASS
- [x] Show lands focus in the editor · M1 · your view: PASS
- [x] Esc / hotkey / Close = hide, not quit · F2 · PASS
- [x] Speed: 11 ms median, 15 ms p95 vs the 100 ms budget · M5 (scripted; no physical hotkey-to-paint number) · not yet your judgement of feel
- [ ] Hotkey configurable by the user · M7-settings
- [ ] Start with Windows · M7-settings

## Window and tray
- [x] Tray icon + menu, Quit from tray · M1 · your view: works well (confirmed 2026-10-21)
- [x] Single instance · M1 · PASS
- [x] Always-on-top off by default, Pin toggle · F1 · PASS
- [x] Window geometry restored on relaunch · M3-M4 notes · PASS
- [~] Translucent background (Mica/Acrylic) · F25-translucency-live-look · probe built, you have not judged the live look (P0)
- [ ] Windows show/hide/quit animations · F5 · wanted, not built
- [ ] Chrome-free hover bars · F4 · deferred to overhaul

## Notes and storage
- [x] Notes saved as plain .md files, autosave, atomic writes · M2 · works
- [x] Restore last note and caret on relaunch · M2 · works (you confirmed 2026-10-21, "for now")
- [x] New note, delete note · M4 · works
- [ ] Undo for delete · F44-delete-placement-and-undo · needs your call
- [ ] Data folder choice · M7-settings
- [ ] Search / find across notes · never specced (open question for you)

## Navigation between notes (carousel)
- [x] Wheel notch = one note, ~220 ms slide · M4 · PASS, but feel is under review
- [x] Drag the grab strip, snap, n-of-count indicator · M4 · PASS; F15 you may remove drag
- [~] Slide polish: late start, landing hitch, boundary feedback · F45-slide-timing, F46-slide-landing-hitch-and-jump (P0), F47-boundary-feedback
- [ ] Wheel threshold by scroll distance · F14-wheel-threshold · needs your spec

## Editor
- [x] Markdown styling, dimmed markers, per-level heading colours · F8, F27 · PASS
- [x] Python fenced code colouring · M3 · PASS
- [x] Checkbox widgets, click to toggle · M3, F12 · PASS
- [x] List continuation on Enter; Tab/Shift+Tab indent · F10, F11 · PASS
- [x] Folding (heading rule), fold markers, hit targets · F6, F24, F29 · PASS
- [x] Strike rendering, indent guides, active guide, active line · F7, F13, F19, F20 · PASS
- [x] Monospaced font throughout · F28 · PASS
- [~] Empty note: caret vs hint mismatch · F40-empty-note-caret-vs-hint (P0)
- [~] Last lines clipped at the bottom · F50-last-lines-clipped (P0)
- [ ] Smooth scroll, smooth caret, animated folding · F21, F22, F23 · wanted
- [ ] Marker dimness / strike thickness settings · F9, F17 · M7-settings

## Quality and accessibility
- [~] Menu double-highlight (focus + hover) · F49-menu-focus-hover-double-highlight (P0)
- [ ] Visible keyboard focus ring · F51-focus-visible
- [ ] Contrast and minimum target size · F43-contrast, F42-target-size
- [ ] Accessible names on icon-only buttons · F48

## Dev workflow (not product, but blocks you)
- [x] `--quit` flag so rebuilds do not need a manual quit · F18 · done
- [ ] Self-tests must not pop windows over your pad · F37-hidden-selftest-windows (P0)

## Rough completeness
Core loop (summon, type, notes saved, switch notes, dismiss) is built and passing your checks. What is missing is mostly settings (M7), motion/polish, and the P0 behaviour bugs above. Nothing found yet for search, export, or sync; confirm whether you want any.
