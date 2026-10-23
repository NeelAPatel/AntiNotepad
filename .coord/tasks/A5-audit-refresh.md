# A5-audit-refresh

You are the director (gpt-6.1-sol medium); delegate reading to gpt-6-luna xhigh subagents (one per area: `Editor/`, `UI/`,
`Model/`+`Calc/`+`Theme/`+`Platform/`+root .cs, and `tools/`+`.coord/`+`docs/`+repo files), then merge and verify.
Repo: this folder, branch `feat/A5-audit-refresh` (off dev). Never switch branch, merge or commit. Read-only audit: **fix nothing**.
Read first: `AGENTS.md`, `.coord/STATUS.md`, `docs/decisions.md`, and the previous audit `docs/audit/A1-codebase-audit.md`.

## What changed since A1
Seven A1 findings were fixed and merged: A1-18, A1-21, A1-22, A1-23, A1-24, A1-25, A1-26 (commits from
`git log --oneline ce7588e..HEAD`, diff with `git diff ce7588e HEAD -- product tools .coord/tools docs/agents.md`).

## Goal
A full re-audit of the current tree that produces **verified data**; the manager writes the final document from it.
1. **Re-check every A1 finding (27)** against the current code: status `Fixed`, `Open` or `Changed` (still there but
   different), with its **current** `path:line` and a short quote that is visible there now. For the 7 fixed ones, confirm
   the fix is real and complete, and say if it introduced anything new.
2. **Fresh sweep** of every tracked file for anything A1 missed, using the same five lenses: code debt, dependencies,
   inefficiency (render/hot-path allocations, idle timers, per-keystroke full scans, UI-thread I/O), correctness (unhandled
   exceptions, races, data-loss paths in store/migration/install scripts), repo hygiene. New findings get ids
   `A5-<n>-<slug>`. Don't re-report an A1 finding under a new id. Quality over count: each one needs a location and a quote.
3. **Plain-language line per finding**: one sentence a non-programmer understands (what the user would notice or risk),
   in addition to the technical description. No jargon in that sentence.

## Measured evidence (already collected by the manager; quote, do not re-run)
`.coord/scratch/A5/`: `build-release.txt` (0 warnings), `analyzers-dedup.txt` (50 warnings: CA1859 x18, CA1305 x17,
CA1310 x8, CA1822 x3, CA1862, CA1852, CA1720, CA1001), `packages.txt` (no top-level updates, none vulnerable or
deprecated), `selftest.txt` (PASS), `files.txt` (91 tracked files = the checklist).

## Write scope (only this file)
`.coord/reports/A5-audit-refresh.md`, with exactly these sections:

1. `## Findings`: one block per finding (all 27 A1 plus any new A5), most severe first:
   ```
   ### <id>
   - status: Open | Fixed | Changed
   - severity: High | Medium | Low   (keep A1's level unless the evidence says otherwise; say why if you change it)
   - area: Your notes | Crashes | Speed and memory | Install and tools | Repo housekeeping
   - where: path:line (current)
   - quote: `short code quote visible at that line`
   - plain: one non-technical sentence
   - technical: 1-3 sentences: mechanism and trigger
   - fix: one or two sentences
   - effort: S | M | L
   - auto-fix safe: yes | no — reason in a few words
   - decision needed: none | the question the user must answer, in one line
   ```
2. `## Fix verification`: for each of the 7 fixed findings, one line on what you checked.
3. `## Checklist`: a table of all 91 files from `files.txt`, in that order: `path | finding ids or - | note (max 10 words)`.
4. `## Not covered`: what you couldn't check and why.

## Done means
- Only that one file is written. Nothing else changes. No build, no app launch.
- 91 checklist rows. Every id in the checklist appears in Findings, and the other way round.
