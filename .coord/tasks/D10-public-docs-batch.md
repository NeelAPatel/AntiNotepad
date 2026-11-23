# D10-public-docs-batch (Codex build in this worktree; do not commit)

Role: terra, writing and reasoning across drafts. Sandbox workspace-write.
Inputs: `.coord/research/public-prep/D6-public-docs-report.md` (drafts of README, About, contributing, wiki list, license analysis), `D5-public-audit-report.md`, `T21-release-kit-report.md`, `PUBLIC-TREE-REVIEW.md`, `HISTORY-MILESTONES.md`, `docs/*`, `product/AntiNotepad` (verify features from code and selftests; never copy claims from old docs unverified).

## Owner decisions (fixed)
- MIT license. Public branches main and dev. First release is a portable zip: download, unzip, run the .exe. Unsigned.
- The project has TWO parts: (1) the notepad, a hotkey-summoned translucent scratchpad for Windows 11 (Avalonia 12 + AvaloniaEdit, .NET 10); (2) the learning growth: AI-led development, agentic orchestration, dev loops, pstack. The README presents both; part 2 points to a new `docs/building-with-ai.md` and the wiki.
- Credit for JFung of Antinote.io is PARKED: do NOT put it in README or anywhere. The owner will add it later from `.coord/research/public-prep/CREDITS-DRAFT.md`. You may leave a single HTML comment `<!-- credits: see maintainer notes -->` in README where it would go.
- AI-led disclosure belongs in README: absolutely AI-led, with scrutiny (the owner reviews, tests by hand, approves each plan); a personal learning project for AI models, agentic development and orchestration.
- Contribution stance: ideas welcome; atomic fixes and patches welcome; UI enhancements and ideas are the owner's hardest area, so thoughtful proposals are especially welcome; strong AI-leveraged testing ideas are appreciated. The repo's dev loop is owner-run; contributors open issues or small PRs against `dev`.
- Do not touch: `AGENTS.md`, product code, `.coord/research/**` except the one new file named below (another run owns path scrubbing and removing research).

## Write these files (in the worktree root)
1. `README.md` (replace). Hero line, what it is, features (verified from code), install (portable zip: "Download the latest zip from Releases, unzip, run AntiNotepad.exe"; say it will exist from the first release; unsigned warning), usage and default hotkeys (read from code), build from source (read `docs/dev-loop.md`, `tools/dev.ps1`), the two-part framing, AI-led disclosure, link to the wiki `https://github.com/NeelAPatel/AntiNotepad/wiki`, contributing link, license. Mark screenshot places with `<!-- screenshot: ... -->`. Do not invent screenshots or badges that cannot exist.
2. `CONTRIBUTING.md` from the draft, adapted to the decisions above: how to build, run `--selftest`, branch from `dev`, what a good atomic fix looks like, how to propose UI ideas, what AI-leveraged testing ideas look like (3 concrete examples).
3. `LICENSE` (MIT). Copyright line: `Copyright (c) 2026 NeelAPatel`; flag in your report that the owner must confirm the legal name.
4. `THIRD-PARTY-NOTICES.md`: Avalonia, AvaloniaEdit (vendored, MIT, Eli Arbel; point to `third_party/AvaloniaEdit/LICENSE`), Fluent UI System Icons (MIT, Microsoft; point to `product/AntiNotepad/Assets/Fonts/FluentSystemIcons-LICENSE.txt`), any NuGet packages from the csproj files with their licenses. Mark each "verified from file" or "verify before release".
5. `docs/building-with-ai.md`: an index and narrative for part 2. Sections: why this exists, the loop in one page (read `docs/loop.md`, `docs/agents.md`, `docs/playbook.md`), loop eras with dates (read `.coord/lessons.md`, `.coord/status-archive.md`), milestone moments (use the table in `HISTORY-MILESTONES.md` but cite only commit subjects and dates, never old hashes), where to look (`.coord/tasks` cards, `.coord/reports`, `.claude/`, `.codex/`, `.agents/`, `tools/roadmap`), lessons learned (from lessons.md), and an honest "what went wrong" paragraph. Links must be relative paths that exist after the scrub. Do NOT link into `.coord/research/` at all.
6. `.coord/research/public-prep/D10-about-and-wiki.md`: the GitHub About description (max 350 chars), 8-12 topics, and the wiki page list with a 3-line outline per page (do not write the wiki pages).

## Done means (offline)
- Every relative link in the new files resolves to a file that exists (check with a script or grep).
- README claims about features and hotkeys are each traced to a file in your final message.
- No home paths, repo paths, claude.ai artifact URLs, or Antinote credit text in the new files except the single HTML comment above.
- Final message: files written, assumptions, items the owner must confirm.
