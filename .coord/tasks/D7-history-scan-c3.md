# D7-history-scan-c3: classify commits 209-312 of the dev history

Role: quick, read-and-grep inventory over git history (read-only sandbox).

## Request
The owner is preparing this repo (Windows scratchpad app AntiNotepad, Avalonia 12 + .NET 10, product in product/AntiNotepad) to go public. History is 619 commits on dev (main is the same tip). Non-product history (roadmap UI/UX, dev-loop/agent process) may be squashed. Findings only: change nothing, write no files, run no builds, no ports, no app.

## Your chunk
Commits numbered 209 to 312 in oldest-first order of first-parent-and-all history:
`git log --reverse --format='%h %ad %s' --date=short dev | sed -n '209,312p'`
For path info use `git show --stat --format= <hash>` or `git log --reverse --name-only --format='@@%h %ad %s' --date=short dev | ...`; sample, do not read full diffs. Merge commits: classify by the merged item.

## Classes (one per commit)
- product: app code, features, UX in product/AntiNotepad
- docs: README, docs/ not about the loop
- process: dev loop and agent process (.claude, .codex, .coord, docs/loop*, docs/agents, tools for the loop)
- roadmap: tools/roadmap or T-prefix roadmap items (roadmap UI/UX)
- spike: AntiQuickPad spikes, retired experiments, lab pages
- other: say what

## Report (put it in your FINAL message; keep under about 120 lines)
1. Counts per class for this chunk, plus date span and the first and last hash.
2. Per-day or per-item-group summary (item ID-slug groups, how many commits each, class) as a compact table; for roadmap and process runs, name the contiguous hash ranges (first..last hash) so they can be squashed.
3. Notable commits (hash, date, one line) where something real appears: first working hotkey show/hide, first persisted notes, calculator, search, themes, settings, tray, installer, tags (v0.1, v0.5, v0.7.x), the rename AntiQuickPad to AntiNotepad, the 2026-10-29 history rewrite. Only list those that fall in your range; say "none" if none.
4. Merge commits in range (count) and whether they are --no-ff item bubbles.
5. Anything odd: commits mixing product and process files, junk or fix-up commits that could be folded.
Be specific with hashes; do not guess. If a class is uncertain say so.

Done means: the report is in your final message.
