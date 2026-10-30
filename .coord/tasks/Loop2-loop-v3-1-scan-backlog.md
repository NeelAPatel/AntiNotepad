# Loop2-loop-v3-1-scan-backlog: step 3.1e, batch backlog scans and a delta refresh at task time

Step brief of `Loop2-loop-v3-1` (read the card `.coord/tasks/Loop2-loop-v3-1.md` first, especially "Round 2 scope
change": the user's way of working this step writes down).

Tier: Low · docs-tools
Role: director (gpt-6.1-sol medium)

## Why this step
The user scans several backlog ideas at once with read-only Haiku chats, ranks them, and later points the item's
session at the old scan. Today that lives in chat transcripts: nothing records which dev commit a scan saw, and nothing
shows what changed since, so a later item either re-reads everything or trusts a stale scan and misses new code that
would help it.

## After
1. **`.coord/templates/scan.md`**: brief for one scout (Haiku, role `scout`, read-only, no Codex). It reads the idea's
   plan row (and any screenshots named there) in the snapshot and writes `.coord/research/<slug>.md` with:
   `Scanned at: <full sha>`, then sections `## Kind` (bug/feature/polish/process and why), `## Now` (what the app does
   today, file:line evidence), `## Where` (one backticked repo path per `- ` line, then symbols), `## Size and tier`
   (guess with the Tiers slug), `## Risk`, `## Links` (other backlog ideas it overlaps, depends on, or would make easier),
   `## Questions` (for the user). Short; evidence over prose; no plan, no fix.
2. **`.claude/skills/scan-backlog/SKILL.md`** (`/scan-backlog [slug ...]`, master): pick the ideas (named, or every
   `idea:` row that is `needs you`/`planned`; ask if more than about 10). Create one snapshot
   `git worktree add --detach .claude/worktrees/scan/<YYYY-MM-DD> dev` and record its sha. Start one linked Haiku
   sub-session per idea, `scan · <slug>`, role `scout`, use_worktree false, working only in the snapshot with absolute
   paths, writing only `.coord/research/<slug>.md` in the main checkout, ending its turn with the file path. Tell the user
   in one line: snapshot path, sha, the scouts. When all are back: one table for the user (idea, kind, tier/size guess,
   Where files, overlaps and links, open questions) and a proposed order; the order goes into STATUS only on the user's
   word. Then `git worktree remove` the snapshot. Research files stay untracked in the main checkout until their item is
   approved.
3. **`tools/scan-delta.ps1 -Slug <slug> [-Base dev]`**: reads `.coord/research/<slug>.md` (`Scanned at:` and the `## Where`
   paths). Prints the commits `<sha>..<Base>` (`--first-parent`, subject per line), then each file they changed marked
   `WHERE` (listed in Where), `NEAR` (same folder as a Where file) or nothing (others are not printed), then the cards to
   read: `.coord/tasks/<ID-slug>.md` for each commit subject that starts with `<ID-slug>:` and exists. Last line
   `DELTA: none | near (<n>) | where (<n> of <m> Where files changed)`, and `RESCAN: yes` when more than half of the Where
   files changed or a Where file was deleted, else `RESCAN: no`. Exit 0; exit 1 on a missing file, a missing sha or a git
   failure. Read-only.
4. **`.claude/skills/next-task/SKILL.md` step 2 (Findings)**, first: if `.coord/research/<slug>.md` exists, run
   `tools/scan-delta.ps1 -Slug <slug>`, read the scan, the delta's cards and its `WHERE`/`NEAR` files, and write the
   card's Findings from them with one line "Since the scan (<short sha>): <unchanged | changed: ... | new code to use: ...>".
   `RESCAN: yes`: a fresh scout or audit as today. A Medium/High audit still runs; its brief gets the scan and delta as
   inputs. Nothing else in the step changes.
5. **`tools/approve.ps1`**: also moves `.coord/research/<slug>.md` (slug = `-Task` without its leading `<ID>-`) into the
   new branch with the card, when it exists, and commits it with the card.
6. **`docs/loop.md`**: a short "Backlog scans" paragraph under "2. Findings" (what `/scan-backlog` does, research files
   with their sha, the delta at task time, rescan rule) and rows for `tools/scan-delta.ps1`,
   `.claude/skills/scan-backlog/SKILL.md` and `.coord/templates/scan.md` in the "Files" table. Nothing else.

## Files
- `.coord/templates/scan.md`
- `.claude/skills/scan-backlog/SKILL.md`
- `tools/scan-delta.ps1`
- `.claude/skills/next-task/SKILL.md`
- `tools/approve.ps1`
- `docs/loop.md`

## Done means
- Probe in a throwaway clone under `build/Loop2-loop-v3-1-probe` (deleted after; never this checkout): a research file
  stamped at an old commit, then commits touching a Where file, a nearby file and an unrelated file, one with an
  `<ID-slug>:` subject and a card: `scan-delta` prints the right marks, the card, `DELTA: where (1 of 2 ...)`,
  `RESCAN: no`; with both Where files changed or one deleted, `RESCAN: yes`; no commits since, `DELTA: none`; missing sha,
  exit 1. `approve.ps1` moves and commits the research file with the card (and still passes without one). List results.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline` passes.

## Constraints
Work in this checkout (`chore/Loop2-loop-v3-1`; no worktree). Never switch branches, merge, commit or push; never edit
`.coord/STATUS.md`, the card, or `product/`. Never launch or kill AntiNotepad. Keep scripts short, in the style of
`tools/files-overlap.ps1` and `docs/coding.md`; reuse the same `## Files`-style path parsing.

Report: `.coord/reports/Loop2-loop-v3-1-scan-backlog.md` with the item template's report sections.
