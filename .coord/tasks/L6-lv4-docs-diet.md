# L6-lv4-docs-diet: shrink what every master and session reads, without losing a rule

Item card (`docs/loop.md`). Level 4 item 6 of 7 (shared brief: `.coord/research/lv4-shared-brief.md`).

Tier: Low · docs-tools (rules: `docs/loop.md`)
Branch: `chore/L6-lv4-docs-diet` in `.claude/worktrees/chore/L6-lv4-docs-diet` (created at Approve)

## Request
Loop Hub 2 brief, 2026-11-08: split `docs/loop.md` (about 35 KB) by audience into a core of about 5 KB for masters, with phase
detail in the skill that runs the phase and branch/merge detail in merge-task; move `docs/coding.md` to
`.claude/rules/coding.md` with a `paths:` filter for `product/**`; trim `AGENTS.md`, fix "one squashed commit per item" to the
merge-commit rule, add the `L` prefix, record the numbering restart; bump the loop version with a changelog; measure bytes a
master reads at start, before and after.

## Findings
- **Kind and why:** process. Every master is told to read all of `docs/loop.md` (35,413 B) plus `AGENTS.md` (3,665 B) at start.
- **Now (measured 2026-11-08, bytes):** AGENTS.md 3,665 · docs/loop.md 35,413 · docs/coding.md 5,307 · glossary 4,932 ·
  agents 4,263 · dev-loop 11,661 · skills: next-task 7,010, review-task 7,118, merge-task 4,627, scan-backlog 2,562, roadmap 899.
  Master start read today = AGENTS.md + loop.md = 39,078 B (`CLAUDE.md` loads AGENTS.md every turn).
- **Where:** `docs/loop.md`; `docs/coding.md` (referenced by skills, templates, glossary, loop.md); `AGENTS.md`;
  `tools/loop-lint.ps1` (file list, no section counting); `tools/loop-card.ps1` and `tools/catch-up.ps1` (version string);
  `tools/catch-up.ps1` and `tools/files-overlap.ps1` (HotFiles list); `docs/glossary.md`; `.claude/skills/*`.
- **Risk:** a rule dropped in the split. Mitigation: the old-section to new-home table below is checked line by line at Check.
  Second risk: overlap with L1-L5 on `.claude/skills/*/SKILL.md` and `.claude/settings.json` (rebase at Build).
- **Source:** master scout (reading the files above).

### Mapping, old section to new home (planned)
| Old `docs/loop.md` section | New home |
|---|---|
| Intro, Phases table, paused | stays in core |
| 2 Findings: what they must answer; backlog scans | `.claude/skills/next-task/phases.md` (read by `/next-task`) |
| 3 Plan: what the user approves; Gates menu | `.claude/skills/next-task/phases.md` (gates menu also one line in core) |
| 6 Check: bounds check; Triage, lessons, scores | `.claude/skills/review-task/phases.md` |
| Tiers | stays in core |
| Status card | stays in core |
| Widgets | `.claude/output-styles/antinotepad.md` is the reply style; the widget list moves to `next-task/phases.md` (2 lines stay in core) |
| Rules (rework, gate reports, Codex sub-sessions, ownership, overrides, proposals, merging, freeze, fast lane, who builds, evidence) | core keeps the short ones (who merges, ownership, fast lane, freeze, overrides); rework counting to `review-task/phases.md` |
| IDs and numbering | core (with `L` prefix and the numbering restart) |
| Sessions: picture, master duties, routing, step back, need-to-know, live directors, notes, name | core keeps picture, master duties, routing in 8 lines; rest to `docs/loop-sessions.md` |
| Sessions: sub-session step table, handover prompt | `docs/loop-sessions.md` (next-task links it when it starts sub-sessions) |
| Loop version history paragraph | `docs/decisions.md` ("Loop version history"); core keeps the current version and bump rule |
| Branches, worktrees, commits; catching up | `.claude/skills/merge-task/branches.md`; core keeps the 4-line rule (branch per item, worktree, merge commit, only masters merge) |
| Triggers in practice | core (3 lines) |
| Files table | `docs/loop-sessions.md`... exact home decided at build; core links it |

## Plan
- **After:** (a) `docs/loop.md` about 5 KB; skills carry their phase detail in a reference file beside `SKILL.md`, loaded only
  when that skill runs; (b) `.claude/rules/coding.md` with `paths: product/**` and a short pointer left at `docs/coding.md`
  (Codex does not read `.claude/rules`, so the pointer matters); (c) `AGENTS.md` trimmed, "one squashed commit per item"
  replaced by "tidy commits plus one `--no-ff` merge commit", `L` prefix and the Level 4 numbering restart added;
  (d) loop version bumped (proposed `v5.3.0`, minor: new structure, no phase change) in `docs/loop.md`, `docs/glossary.md`,
  `tools/loop-card.ps1`, `tools/catch-up.ps1`, changelog line in `docs/decisions.md`; Loop Hub 2 updates STATUS and renames
  the three directors at merge (`merge-task` step 8); (e) before/after bytes table in the report.
- **Expected time:** about 1.5 h, request to merge (docs-only, one Luna run plus my mapping check). Not over an hour of hands-on work;
  the clock depends on waiting for L1-L5 to merge first.
- **Must not change:** any rule's meaning; app code; gate scripts except `loop-lint.ps1` file list and the version strings;
  user-global settings.
- **Scope questions:** (1) version number `v5.3.0` or `v5.2.1`; (2) detail in `.claude/skills/<name>/phases.md` files
  (my default) versus inside `SKILL.md`.
- **Test plan, code:** none (docs). `tools/loop-lint.ps1` extended to lint `.claude/skills/*/*.md`, `.claude/rules/*.md`,
  `docs/loop-*.md`.
- **Test plan, UI:** None (not visible).
- **Gates:** `tools/gate.ps1` (build, loop-lint finds broken file references, scope). Check: mapping table verified line by line;
  sizes measured with `wc -c`; `claude` memory view (`/memory`) shows what loads in a product-free session, if available.
- **Needs from you:** the two scope questions above.

## Files
- `docs/loop.md`
- `docs/loop-sessions.md`
- `docs/coding.md`
- `docs/decisions.md`
- `docs/glossary.md`
- `AGENTS.md`
- `.claude/rules/coding.md`
- `.claude/skills/next-task/SKILL.md`
- `.claude/skills/next-task/phases.md`
- `.claude/skills/review-task/SKILL.md`
- `.claude/skills/review-task/phases.md`
- `.claude/skills/merge-task/SKILL.md`
- `.claude/skills/merge-task/branches.md`
- `.claude/skills/scan-backlog/SKILL.md`
- `.coord/templates/item.md`
- `.coord/templates/findings.md`
- `tools/loop-lint.ps1`
- `tools/catch-up.ps1`

## Contracts
None.

## Approve
User, 2026-11-08: "* 5.3.0 * reference files ... plan is approved with your recommendations, start the worktree, branch and build this out. Once built, debrief me and wait for additional corrections or green light to prep for merging."

## For Codex
Role: quick (Luna medium), because it is a verbatim docs move with a written mapping. Product `product/AntiNotepad` is untouched. The branch is
already checked out: never switch, merge or commit. Read `AGENTS.md`, `docs/loop.md`, `docs/coding.md`, `tools/loop-lint.ps1`, then this card.

Build exactly this. **Move text verbatim (no rewording, no rule dropped).** Only headings, links and the short core summaries are new.
1. `docs/loop.md` becomes the core, 5-6 KB. Keep: intro, "Phases" table and `paused`, "Tiers", "Status card", a trimmed "Rules"
   (who merges, section ownership, fast lane, `Loop`/`D` short cards, who builds, rule overrides, proposals into STATUS, freeze, evidence,
   Codex in visible sub-sessions, release markers, lab-first, roadmap URL), "IDs and numbering", a short "Sessions" (the picture, master
   duties, routing, step back, loop version line and the bump rule), a 4-line "Branches" rule (branch `<prefix>/<ID-slug>` per item at
   Approve, worktree by default, tidy commits plus one `--no-ff` merge commit, only a master merges), "Triggers in practice", and a
   "Where the detail lives" table pointing at the files below. Keep the `**Loop version: v5.3.0**` wording and the `LP<vX.Y.Z>` rule.
2. Create `.claude/skills/next-task/phases.md` from loop.md: "2. Findings", "Backlog scans", "3. Plan", "Gates menu", "Widgets".
   Create `.claude/skills/review-task/phases.md`: "6. Check (bounds check)", "Triage, lessons, scores" and the rework/round rule and
   gate-reports rule from "Rules". Create `.claude/skills/merge-task/branches.md`: "Branches, worktrees and commits" and "Catching up with dev".
   Each starts with one line saying which skill reads it. Add one line to each `SKILL.md` ("Phase detail: `<path>`"), and fix every
   `docs/loop.md "<Section>"` reference in the four `SKILL.md` files, `.coord/templates/item.md` and `findings.md` that now points at a moved section.
3. Create `docs/loop-sessions.md`: the rest of "Sessions" (sub-session step table, naming, notes, master name, handover and the resume
   prompt, edited to say "read the core `docs/loop.md` and the files it names") and the "Files" table (update it with every new file).
4. Move the loop version history paragraph into `docs/decisions.md` under a new "Loop version history" heading and add a v5.3.0 line
   (2026-11-08, `L6-lv4-docs-diet`: loop.md split by audience, coding rules path-scoped, `L` prefix). Do not add decisions.md to loop-lint.
5. Version v5.3.0 in `docs/loop.md`, `docs/glossary.md`, `tools/loop-card.ps1`, `tools/catch-up.ps1`; update glossary's ID-prefix line with `L`.
6. `docs/coding.md` text moves to `.claude/rules/coding.md` unchanged, with this frontmatter first:
   ```
   ---
   paths:
     - "product/**"
   ---
   ```
   `docs/coding.md` becomes a 3-line pointer to it (Codex does not read `.claude/rules`, so the pointer says "read `.claude/rules/coding.md`").
7. `AGENTS.md`: trim to about 2.8 KB without dropping a rule (shorten wording, point to docs instead of repeating). Replace "one squashed
   commit per item" with "tidy commits plus one `--no-ff` merge commit per item". Name items rule: add prefix `L` (Level 4 and 5 loop
   automation, lowercase slugs such as `L6-lv4-docs-diet`) beside A, Loop, D, T. In `docs/loop.md` "IDs and numbering" record: `L` items
   restart numbering at `L1` on 2026-11-08 (L1-L7 Level 4, L8-L15 Level 5) and slugs are lowercase.
8. `tools/loop-lint.ps1`: add patterns `.claude/skills/*/*.md`, `.claude/rules/*.md`, `docs/loop-*.md`; keep the rest. It does not count sections.

Constraints: no drive-by edits; US spelling; keep `loop-lint` passing; only the Files listed in the card change. Never edit this card or STATUS.
Inside the sandbox run `pwsh -NoProfile -File tools/loop-lint.ps1` and `pwsh -NoProfile -File tools/gate.ps1 -Task L6-lv4-docs-diet -Offline`.

Done means (offline checks only):
- `loop-lint` PASS; gate PASS offline.
- `wc -c docs/loop.md AGENTS.md` are about 5-6 KB and 2.8 KB; every old loop.md `##`/`###` heading is found in exactly one new place.
- Report `.coord/reports/L6-lv4-docs-diet.md` with the usual sections plus: a table old section -> new file/heading, and a table of bytes
  before/after (before: AGENTS.md 3665, loop.md 35413, coding.md 5307).

Report: `.coord/reports/L6-lv4-docs-diet.md`, short, sections: Summary; Change list; Behavior changes; Out-of-scope files; Plan deviations;
Contracts; Lens review (say "none, docs only; checked the mapping myself"); Checks run; Hand checks for the user ("None (not visible)").

## Set aside (user, 2026-11-08: "L5 should finish up, merge, then picked up by L6")
`tools/loop-card.ps1` left L6's Files so L5 can merge first. Patch to reapply after L5 merges: change the version text `loop v5.2.0` to `loop v5.3.0` in the reminder line (and the header comment if L5 kept it). Then re-add the file to Files, rerun the gate. After L5 merged (7fad1b4) `tools/loop-card.ps1` no longer carries a version string, so there is nothing to reapply; the file stays out of Files.

## Check
- Gate (online, after rebase onto dev 069afbc): PASS; loop-lint PASS (51 files, extended to skill reference files, rules, loop-*.md).
- Bounds check: round 1 compressed core wording (rules reworded or dropped); caught by a sentence-level text diff against the old loop.md; round 2 restored the old text verbatim. Remaining diff: intro item-card sentence (restored by me), version history lines (added by me to decisions.md), handover prompt (edited on purpose).
- Within plan: all 16 changed files are on the Files list. Justified: core is about 15.7 KB, not 5 KB, because "no rule lost" outranks the size target.
- Verified: byte counts, lint, gate, rebase over L3/L4 (L3 role-table line moved to docs/loop-sessions.md).
- Assumed (not tested): `.claude/rules/coding.md` with `paths: product/**` loads for subagents and worktrees; docs say it loads on Read/Write/Edit of matching files.
- Misses: Codex paraphrased despite "verbatim" (cause: plan/brief too loose on 5 KB target; caught by: manager diff).

## Hand test
No UI (docs only; dev badge not applicable). User, 2026-11-08: "Green light" to merge.
