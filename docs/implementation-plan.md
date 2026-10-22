# Implementation plan (v1, 2026-10-17)

Stack confirmed by user: **Avalonia + AvaloniaEdit** (spike B parked, C unused). UI direction: undecided; ship one default `Look` and choose after seeing the real app. Transparency stays de-prioritized.

## Principles
- Speed of show/hide is the top goal: pre-created window kept alive, show/hide only toggles visibility. Measure in Release.
- Start from spike A code (`spikes/A`), moved into `src/AntiQuickPad/` and cleaned. Spikes stay as reference, untouched.
- Smoke checks only (models, stores, a few scripted UI probes). Disposable.
- One `Look` record drives all theming (palette, fonts, gutter, checkbox style, code panel).
- Local git, one commit per milestone. Manager commits (Directors cannot write `.git`).
- Codex sandbox is offline: manager runs `dotnet restore` before dispatch.

## Known Avalonia limits (accepted)
No line-height control; no inner padding in code panels; true rounded/transparent window flaky; fold margin only restylable by brush. Design around these, do not fight them.

## Milestones

| # | Milestone | Done when |
|---|---|---|
| M0 | **Project skeleton**: `src/AntiQuickPad` (net, Avalonia 12.1.4, AvaloniaEdit 12.0.0), `Look` record + one default look, `RuntimePaths`, solution, restore done offline-ready | `dotnet build -c Release` clean; empty window opens |
| M1 | **Shell and hotkey**: borderless always-on-top window, tray icon, single instance, global hotkey (default **Ctrl+Alt+Shift+A**, configurable; Ctrl+Alt+Space collides here), show = focus editor, hide = Esc/hotkey, reopen cue | Scripted + manual: hotkey toggles, focus lands in editor |
| M2 | **Notes model and persistence**: note list, current index, autosave (debounced + on hide/exit), atomic writes, plain `.md` files in a data folder, restore last note and caret on relaunch | Smoke check: create/edit/relaunch round trip |
| M3 | **Editor**: Markdown decoration (headings, bold/italic/strike with dimmed markers), fenced-code colouring (Python first, language from fence or first-line `python:`), `- [ ]`/`- [x]` checkbox widgets with click toggle and hover cursor, Tab/Shift+Tab indent, folding, line numbers per Look | Fixture file renders; checkbox toggles text |
| M4 | **Carousel**: wheel notch = prev/next note with ~220 ms slide, drag = 1:1 direct manipulation with snap, new/delete note, page indicator | Wheel and drag verified by user on MX Master 3S |
| M5 | **Release measurement**: hotkey-to-visible-and-focused, hide time, idle RAM, idle CPU, in Release; record in `docs/perf.md` | Numbers recorded; compare against a speed budget agreed with user (proposed: show < 100 ms) |
| M6 | **Look pass**: implement 2 to 3 variants from `demos/ui-variants` (candidates Ink, Paper, Slate) as `Look` entries, runtime switch; user picks | User chooses default look |
| M6b | **Hover bars** (user request 2026-10-17): top and bottom bars hidden until the mouse enters that edge; top bar drags the window and has Pin (always-on-top toggle, default off), Minimize and Close (= hide, like Esc); bottom bar content TBD. Styled from the `Look` record | User checks reveal, drag, and buttons |
| M7 | **Settings and polish**: hotkey setting, font size, data folder, start with Windows, about/version | Manual checklist |

M0 to M2 can start immediately; M3 and M4 depend on M0 only and can run in parallel after it (different files). M5 needs M1 to M4. M6 needs M3.

## Dispatch (per AGENTS.md roles)
- Directors (Codex `gpt-6.1-sol` medium) own a milestone each and finish it in one turn; Luna workers (`gpt-6-luna` high) do bounded subtasks (colorizer, checkbox generator, carousel animation).
- Task briefs in `.coord/tasks/<id>.md`, reports in `.coord/reports/<id>.md`. STATUS.md updated by the manager at each dispatch and milestone.
- Folder ownership: `src/` is a new folder; one writer at a time per subfolder (`src/AntiQuickPad/Editor`, `/Shell`, `/Notes`, `/Carousel`). AGENTS.md rule list to be extended with these.
- Claude reviews each report (Sonnet junior reviewer; Opus final review when available).

## Risks and mitigations
- **Hotkey/tray unproven in Release**: M1 is first and tested before editor work.
- **Carousel feel**: M4 isolates animation; tune duration/easing after user test.
- **Polish ceiling**: M6 decides; if the bar is not met, record it and stop rather than reopen B.
- **Quota**: one milestone per Director turn, token budget noted in each brief.

## Open decisions for the user
- Speed budget for M5 (proposed < 100 ms show-to-focus).
- Final hotkey chord.
- Wheel behaviour: one notch = one page (current) or smoother.
- Whether transparency returns later.
