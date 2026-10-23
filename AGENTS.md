# Antipad

Windows 11 hotkey-summoned, chrome-free translucent scratchpad: a recreation of AntiNote (its functionality and feel, not a 1:1
copy). Avalonia 12 + AvaloniaEdit 12 on .NET 10, in `product/Antipad`. Speed of show/hide is the priority. US spelling in UI text.
The earlier AntiQuickPad project (spikes A/B, `src/AntiQuickPad`) is retired and kept read-only in `archive/`; nothing in the
product may depend on it.

## Rules
- Local git only. Branches: `main` (tagged releases only, on user say-so) > `dev` (integration) > `feat/<ID-slug>` (one per task,
  off dev, merged back with --no-ff). Commit per completed task. Tags: `antipad-vN`.
- Name items and tasks `<ID>-<short-slug>` everywhere (chat, briefs, commits, reports). Never a bare ID.
- Design changes go lab-first: show them in a spike or the Icon Lab (`Antipad.exe --iconlab`), wait for the user's approval,
  then implement.
- The user's own copy (`build/antipad`, real data) is never typed into, killed or rebuilt without the swap routine in
  `docs/dev-loop.md`. Dev work runs on the separate dev instance.
- Tests are small `--selftest` checks of capability, not full coverage.
- First-party libraries first; others only on a concrete need, and only if maintained and documented.
- Update `.coord/STATUS.md` at every dispatch, milestone and completion. Keep this file minimal; detail goes in linked docs.

## Roles
- Implementation: Codex. Director `gpt-6.1-sol` medium, workers `gpt-6-luna` xhigh. Launch and routing: `docs/agents.md`.
- Manager: the driving Claude session. Writes briefs, gates results (build, selftest, a look in the dev copy), commits (Codex
  cannot write to .git), relays to the user. Keeps its own implementation to tiny gate fixes.

## Read when
- Starting or resuming work: `.coord/STATUS.md`.
- Building, running, swapping the user's copy, dev instance, hotkeys, data folders: `docs/dev-loop.md`.
- Launching or briefing Codex: `docs/agents.md`. Briefs go in `.coord/tasks/<ID-slug>.md`, reports in `.coord/reports/`.
- Why something is the way it is (locked choices): `docs/decisions.md`.
- Code layout: `product/Antipad` — `Model/` (store, settings, notes), `Editor/` (PadView, colorizers, renderers, motion),
  `UI/` (windows, Settings, menu, Kit controls), `Calc/` (math mode), `Theme/` (palettes), `Platform/` (Win32, tray, hotkey).
