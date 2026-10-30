# Loop2-loop-v3-1-loop-lint: step 3.1g, a cheap check that the loop's own files agree with themselves

Step brief of `Loop2-loop-v3-1` (read the card `.coord/tasks/Loop2-loop-v3-1.md` first).

Tier: Low · docs-tools
Role: director (gpt-6.1-sol medium)

## Why this step
The v3 rename (`feat/` to `<prefix>/`, Go to Approve, retired `C`/`N`/`P`/`S` IDs) silently broke both dormant v2.2
scripts, and nothing noticed until a manual read. Skills name scripts and docs by path; a renamed or missing one is only
found when a session trips on it.

## After
1. **`tools/loop-lint.ps1 [-Root <repo>]`**, read-only, zero model tokens. **Live loop files** only: `AGENTS.md`,
   `docs/loop.md`, `docs/glossary.md`, `docs/agents.md`, `docs/coding.md`, `docs/dev-loop.md`,
   `.claude/skills/*/SKILL.md`, `.claude/agents/*.md`, `.coord/templates/*.md`, `.agents/skills/*/SKILL.md`,
   `.codex/agents/*.toml`, `tools/*.ps1`, `tools/*.sh`, `.coord/tools/*.sh`. Never `docs/v2.2/`, `docs/wiki/`,
   `docs/loop-v2.2-plan.md`, `.coord/tasks/`, `.coord/reports/`, STATUS or lessons (history may say old things).
   Rules, each reported as `path:line: <rule>: <text>`:
   - `stale-path`: `tools/v2.2/`.
   - `stale-phase`: the Go phase (`## Go`, `● Go`, `Plan > Go`, `"Go"`, `past Go`) unless the line also says `era-one` or
     `before loop v3`.
   - `stale-id`: retired era-one IDs used as live names: `\b(C|N|P)[0-9]+[a-z]?-[a-z]` or `\bS[0-9]+-status\b`.
   - `stale-version`: `loop v1`, `loop v2`, `v2.1`, `v2.2` (case-insensitive) used as the current loop.
   - `feat-only`: a branch pattern or example that hard-codes `feat/` without the other prefixes on the same line
     (e.g. `refs/heads/feat/`, `"feat/$`, `feat/<ID-slug>` alone).
   - `missing-path`: a backticked repo path in a live file that starts with `tools/`, `docs/`, `.claude/`,
     `.coord/templates/`, `.coord/tools/`, `.agents/` or `.codex/`, ends in a file extension, contains no `<`, `*` or `{`,
     and does not exist. Also relative Markdown links `](...)` in live `.md` files that do not resolve.
   - Exceptions live in **`tools/loop-lint.allow`**: one `path|rule|substring` per line (`#` comments), each with a reason
     comment above it. An exception matches when path, rule and a substring of the reported line all match.
   Last line `LOOP-LINT: PASS (<n> files)` or `LOOP-LINT: FAIL (<n> problems)`; exit 0/1; exit 2 on a script error.
2. **Current repo passes.** Run it on this branch. For each hit: fix it when the file is in the card's `## Files` and the
   fix is a wording or path correction of one line (list each fix in the report); a true history mention (a sentence
   about what an older version did) goes into `tools/loop-lint.allow` with a reason; anything else is listed in the report
   for the manager, not fixed.
3. **`tools/gate.ps1`:** a `LoopLint` step after Scope. It runs the script when any changed file (same changed-file set as
   Scope) is a live loop file or `AGENTS.md`; otherwise SKIP ("no loop files changed"). In `-Ci` it always runs. FAIL when
   the script exits non-zero; its problem lines go into the step's lines (first 40).
4. **`docs/loop.md`:** the gate row in "Gates menu" and the `tools/gate.ps1` row in "Files" mention `loop-lint`; a "Files"
   row for `tools/loop-lint.ps1` and `tools/loop-lint.allow`. Nothing else.

## Files
- `tools/loop-lint.ps1`
- `tools/loop-lint.allow`
- `tools/gate.ps1`
- `docs/loop.md`
- (one-line fixes only, per After 2) any other file listed in the card's `## Files`

## Done means
- `pwsh -NoProfile -File tools/loop-lint.ps1` prints `LOOP-LINT: PASS` on this branch.
- Probe in a throwaway clone under `build/Loop2-loop-v3-1-probe` (deleted after; never this checkout): one planted hit
  per rule is reported with the right rule name and exit 1; an allow line silences exactly its hit; a planted hit in
  `docs/wiki/` or `.coord/tasks/` is ignored.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline` passes with `LoopLint: PASS`.

## Constraints
Work in this checkout (`chore/Loop2-loop-v3-1`; no worktree). Never switch branches, merge, commit or push; never edit
`.coord/STATUS.md`, the card, or `product/`. Never launch or kill AntiNotepad. Keep the script short and readable, in the
style of `tools/files-overlap.ps1` and `docs/coding.md`.

Report: `.coord/reports/Loop2-loop-v3-1-loop-lint.md` with the item template's report sections, plus the list of hits and
what was done with each.
