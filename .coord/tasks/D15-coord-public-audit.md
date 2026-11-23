# D15-coord-public-audit (read-only Codex scan; do not edit, do not commit)

Role: terra, read-and-judge across a large folder. Sandbox: read-only. Your final message is the report.
Work in this worktree (it holds the latest public-prep state). Do NOT read anything under `.coord/research/`, and do not read `.coord/logs/`.

## Goal
Assess everything in `.coord/` (709 tracked files; also the untracked `.coord/directors/` as a category only) and recommend what belongs on **dev** and on **main** going forward, now that the repo is going public.

## Facts and decisions (fixed)
- Repo goes public as is: history is NOT rewritten. Public refs: `main` and `dev`. The project has two parts: the notepad, and the learning growth (AI-led dev loop, pstack). The loop's evidence is part of what I want to show.
- Current rule (AGENTS.md): `main` is releases only, fast-forwarded from `dev` on my go and tagged semver; `dev` integrates items. So today `main` and `dev` have the same tree at release time. Part of your job: say whether that should stay, or whether `main` should be leaner than `dev`, and what that would cost (a release commit that removes things breaks fast-forward; say how the repo would have to work).
- Already decided: `.coord/research/` goes (a stub README stays because tools write there); `.coord/logs/` is gitignored and stays untracked everywhere; `.coord/directors/` is gitignored and local-only.
- Tools and skills read and write many `.coord` paths (STATUS.md, pending.md, tasks, reports, templates, lessons, roadmap data, `codex-run.sh`). Anything you recommend removing must be checked against `tools/`, `.claude/`, `.codex/`, `.agents/` and `docs/` for references; report which would break.

## Do
1. Per top-level item in `.coord/` (STATUS.md, pending.md, lessons.md, lessons-archive.md, status-archive.md, roadmap-prefs.json, roadmap.html, tasks/, reports/, templates/, tools/, research/, directors/, logs/): purpose, size, tracked count, who reads or writes it (cite the tool or doc), and a recommendation: keep on dev and main / keep on dev only / archive / untrack and ignore / delete.
2. `tasks/` (300 cards) and `reports/` (374 files): classify by prefix and age (A, Loop, T, C, L, D, P, N). What would a visitor learn from them, which are noise, which have personal content (home paths, your names, chat or session IDs, usernames; count per file group, do not print values). Recommend: keep all, keep a curated showcase set plus the rest on dev only, or move old ones into a single archive folder. Give the curated showcase list (at most 12 cards with reasons) that best shows the loop.
3. Tracked generated or state files that should not be tracked (for example `roadmap.html`, `roadmap-prefs.json`): list with evidence.
4. `STATUS.md`, `pending.md`: what a public reader sees on day one; any wording that assumes a private repo, specific chats or directors by name that should be made neutral.
5. The main-versus-dev question: give one recommended model with the exact mechanics (which paths differ, how a release is cut, what the release workflow sees, what contributors see), plus one fallback.
6. A removal plan: ordered commits (by area) to implement your recommendation, each with a risk note.

## Report format (final message, under 150 lines)
Short sections: Summary and recommendation (5 lines), Per-folder table, Tasks and reports analysis, Showcase list, Files to untrack, Public-reader view of STATUS and pending, Main vs dev model, Removal plan, Open questions for me. No file contents dumped; describe. Redact values.
