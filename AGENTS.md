# AntiNotepad

Windows 11 hotkey-summoned, chrome-free translucent scratchpad: a recreation of AntiNote (its functionality and feel, not a 1:1
copy). Avalonia 12 + AvaloniaEdit 12 on .NET 10, in `product/AntiNotepad`. Speed of show/hide is the priority. US spelling in UI text.
The earlier AntiQuickPad project (spikes A/B, `src/AntiQuickPad`) is retired and removed; it lives only in git history.

## Rules
- Remote: private GitHub repo `NeelAPatel/AntiNotepad` (origin); the manager pushes after commits. Branches: `main`
  (releases only, fast-forwarded on the user's say-so, semver tags `v0.7.0` / `-rcN`) > `dev` (integration: one squashed
  commit per item, carrying its STATUS edits) > `<prefix>/<ID-slug>` (`feat/`, `fix/`, `refactor/`, `docs/`, `chore/`;
  one per item, created at Approve in `.claude/worktrees/<prefix>/<ID-slug>`). Details: `docs/loop.md`.
- Name items `<ID>-<short-slug>` everywhere (chat, briefs, commits, reports), never a bare ID. Prefixes: `A` app, `Loop`
  dev loop and agents, `D` docs, `T` non-AI tooling; steps `A1a`, rework `-r2` (`docs/loop.md`, "IDs and numbering").
- Code follows `docs/coding.md` (comments, formatting, file and folder rules), applied to every file an item touches.
- Design changes go lab-first: show them in a spike or lab build first, wait for the user's approval, then
  implement.
- The user's installed copy (`%LOCALAPPDATA%\Programs\AntiNotepad`, real data) is never typed into or stopped during dev work.
  Upgrade only on user request with `tools/install.ps1`; see `docs/dev-loop.md`. Dev work uses `tools/dev.ps1` or `tools/dev.sh`.
- Tests are small `--selftest` checks of capability, not full coverage.
- First-party libraries first; others only on a concrete need, and only if maintained and documented.
- Update `.coord/STATUS.md` at every dispatch, milestone and completion. Keep this file minimal; detail goes in linked docs.

## Roles
- Implementation: Codex. Director `gpt-6.1-sol` medium, workers `gpt-6-luna` xhigh. Launch and routing: `docs/agents.md`.
- Manager: the driving Claude session. Writes briefs, gates results (build, selftest, a look in the dev copy), commits (Codex
  cannot write to .git), relays to the user. Keeps its own implementation to tiny gate fixes.

## Read when
- Starting or resuming work: `.coord/STATUS.md`; the plan: `/roadmap` (local page `.coord/roadmap.html`, self-updating).
- Building, installing/upgrading the user's copy, dev instance, hotkeys, data folders: `docs/dev-loop.md`.
- The per-item loop (item card, phases Request > Findings > Plan > Approve > Build > Check > Hand test > Merge, gates,
  status card on every reply, sessions and roles): `docs/loop.md`. Terms: `docs/glossary.md`.
- Launching or briefing Codex: `docs/agents.md`. Briefs go in `.coord/tasks/<ID-slug>.md`, reports in `.coord/reports/`.
- Why something is the way it is (locked choices): `docs/decisions.md`.
- Code layout: `product/AntiNotepad` — `Model/` (store, settings, notes), `Editor/` (PadView, colorizers, renderers, motion),
  `UI/` (windows, Settings, menu, Kit controls), `Calc/` (math mode), `Theme/` (palettes), `Platform/` (Win32, tray, hotkey).
