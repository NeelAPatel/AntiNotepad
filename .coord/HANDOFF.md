# HANDOFF (overwritten at every pause or manager swap)

Read AGENTS.md, then .coord/STATUS.md, docs/retrospective-2026-10-17.md, docs/decisions-log.md.

State (2026-10-17 evening): spike A (Avalonia + AvaloniaEdit) validated by the user: core features plus all 4 retest checks pass. Spike B (GPUI) parked (docs/spikes/B/PARKED.md). 6 UI variants rendered in Avalonia (demos/ui-variants/REPORT.md, out/*.png): Ink, Paper, Terminal, Slate, Midnight, Carousel.

Open for the next session (suggested: fresh Opus 5.5 medium chat):
1. Confirm stack = Avalonia + AvaloniaEdit (user leans A; ask once).
2. Choose a UI direction from the variants; list the known Avalonia limits (no leading control, code-panel inner padding, rounded transparent window, fold margin styling).
3. Write the real implementation plan: carousel (wheel = page switch, drag = direct manipulation), notes model/persistence, markdown + fenced-code colorizing, checkbox widgets, tray/hotkey (Ctrl+Alt+Space collides on this machine), Release-build speed measurement, theming via one Look record. Milestones, small smoke checks only, local git, AGENTS.md pointers.
4. Dispatch: Codex first (Directors gpt-6.1-sol medium via app session + codex queue, or codex exec with per-run `-c 'windows.sandbox="unelevated"'`; Luna gpt-6-luna high workers). Codex sandbox has NO network: manager runs restore/fetch, Directors build offline. Claude helpers as visible sessions. Directors must finish all milestones in one turn.

User rules: no full-access Codex; transparency de-prioritized; goal = native-Mac-app fit and finish; wheel notches (MX Master 3S, no touchpad); no non-Avalonia mockups presented as our UI; Claude usage conserved.

Resume prompt: "You are resuming AntiQuickPad. Read AGENTS.md, .coord/STATUS.md, .coord/HANDOFF.md and docs/retrospective-2026-10-17.md. Then confirm the stack with the user and write the implementation plan."
