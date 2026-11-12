# AntiNotepad

Windows 11 hotkey-summoned translucent scratchpad; Avalonia 12 + AvaloniaEdit 12 on .NET 10, in product/AntiNotepad. Speed of show/hide is the priority; US spelling in UI. AntiQuickPad spikes are retired and only in git history.

## Rules
- Private GitHub NeelAPatel/AntiNotepad (origin); manager pushes after commits. main is releases only, fast-forwarded on user go and tagged semver; dev integrates each item as tidy commits plus one --no-ff merge commit per item; item branches are prefix/ID-slug, created at Approve in .claude/worktrees/prefix/ID-slug. Details: docs/loop.md and /merge-task.
- Name items ID-short-slug everywhere, never bare IDs. Prefixes: A app, Loop loop/agents, D docs, T non-AI tooling, L Level 4/5 loop automation. Steps A1a; rework -r2. See docs/loop.md.
- Product code follows .claude/rules/coding.md; Codex reads docs/coding.md, which points there.
- Visible design changes are lab-first; user approves the lab before implementation.
- Never type into or stop the installed copy (%LOCALAPPDATA%/Programs/AntiNotepad, real data). Upgrade only on request with tools/install.ps1; dev uses tools/dev.ps1 or tools/dev.sh (docs/dev-loop.md).
- Tests are small --selftest capability checks. Prefer first-party libraries; add maintained, documented dependencies only for a concrete need.
- Update .coord/STATUS.md at dispatch, milestone and completion; keep it minimal and link details.

## Roles and startup
- Codex implements; roles and launch: docs/agents.md. Codex checks run offline only.
- One chat per item: it does Findings, Plan, Approve, build, check and hand test, and the user answers it. It never starts other chats or Codex-run chats; Codex runs in it. A second chat only if it sat idle over an hour with a large context, or on the user's word.
- Masters are area directors: write the Request, start the item chat, own STATUS (except an item's own row) and alone merge to dev/main. The item chat owns the item's phases, briefs/checks Codex and commits on its branch; Codex cannot write to .git.
- Start/resume: .coord/STATUS.md, docs/loop.md, then /roadmap (http://localhost:47811). Phase steps: skill files named by docs/loop.md; build/install: docs/dev-loop.md; terms: docs/glossary.md; locked choices: docs/decisions.md. Habit guide (steps, slash commands, blocked hooks): docs/playbook.md.
- Product layout: product/AntiNotepad uses Model/ (store/settings/notes; Search/ has search types and Store.Search.cs), Editor/ (PadView, colorizers, renderers, motion), UI/ (windows, Settings, menu, Kit), Calc/ (Calculator.cs and Units, Dates, Values, Lexer, Parser, SelfTest parts), Theme/ (palettes), Platform/ (Win32, tray, hotkey), SelfTest/ (checks by area).
