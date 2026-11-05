# A4a-token-scan: count hard-coded design values and stop the count from rising

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build; Codex reads the whole card and
follows "For Codex". Codex never edits this file; Check and Hand test are filled by the sub-session afterwards.

Tier: Low · docs-tools (gate tooling only; no product code)
Branch: `feat/A4a-token-scan` in `.claude/worktrees/feat/A4a-token-scan` (created at Approve)
Idea: A4-design-tokens (step 1 of 6: token-scan, type-tokens, shape-tokens, motion-tokens, paint-tokens, timer-names)

## Request
User, 2026-11-01, on the design-tokens proposal: "i think it might be better to do it in parts.. do the obvious ones
first and slowly go into the fuzzier tokens?" and then: "make a branch and start with token scan only".
STATUS row scope (Master 3's proposal, shown to the user): "Fresh inventory of hard-coded design values (refreshes A59's
stale TSV) plus a gate ratchet: per file and category the count may not rise (spacing included)".
Earlier answers (2026-11-01): spacing and thickness out of tokenizing but on the ratchet; token-scan stays a
design-tokens step (not a Loop Hub 2 T item).

## Findings
- **Kind and why:** process/tooling. About 280 visual values plus about 240 spacing values are hard-coded across 20
  product files (A59 inventory, `.coord/reports/A59-design-ownership-audit-inventory.tsv`, 642 rows at a79cc0a). Before
  the later steps name them, nothing stops new literals from being added, and A59's line numbers are stale (A1-A3 splits,
  A42c/d and A52-A55 changed the files since).
- **Now:** no check counts design literals. `tools/gate.ps1` has steps Offline, Build, Selftest, Seam, Format, Scope,
  loop-lint, Size (`tools/gate.ps1:269-582`). `tools/check-platform-seam.ps1` already has a C# masker
  (`Mask-CSharpCode`, lines 1-200) that blanks comments **and** strings; this check needs comments blanked but strings
  kept (font stacks and `#RRGGBB` colors live in strings).
- **Where values live today (rough counts from a regex pass):** `UI/SettingsParts.cs`, `UI/SettingsWindow.cs`,
  `UI/PadWindow.Layout.cs`, `UI/SearchOverlay.cs`, `UI/PadMenu.cs`, `UI/Kit.cs`, `Editor/PadView.cs`,
  `Editor/Renderers.cs`, `Editor/Colorizers.cs`, `Editor/Motion.cs`, `UI/NoteSwipe.cs`, `UI/PadWindow*.cs`.
  `Theme/Palette.cs` and `Theme/ChromeTokens.cs` are the token homes and are excluded.
- **Where (change):** a new script and baseline under `tools/`, one new gate step, two doc lines. No product file changes.
- **Risk:** (1) false positives that block other directors' items on dev (the ratchet applies to every item, A items of
  Master 2 too): counts are per file and category, and the fail message says how to fix it. (2) Moving code between files
  (a split) moves counts: the check compares totals per category as well, and a file's count may rise when another
  file's count falls by at least as much in the same category. (3) Regexes that miss values: acceptable, the inventory is a
  ratchet and a guide, not proof of the look (that is the later design-dump step).
- **Source:** master scout (Master 3), plus A59 report sections 5 and 7.
- **Narrowed from the STATUS row:** the design-dump selftest (applied control properties, byte-identical to dev) is not
  in this step. It touches product selftest code and constructs real controls (hidden-window risk, F37 lesson), so it
  becomes its own proposed step before type-tokens. This step stays tooling only.

## Plan
- **After:**
  - `tools/check-design-tokens.ps1` scans `product/AntiNotepad/**/*.cs` (not `bin/`, `obj/`, `Theme/`, `SelfTest/`,
    `Calc/`), blanks comments (strings kept), and counts literal design values per file in these categories:
    `radius`, `font-family`, `font-size`, `font-weight`, `color`, `shadow`, `opacity`, `tween`, `timer`, `spacing`.
    A use of a named token (`ChromeTokens.X`, a const or field name) is not a literal and is not counted; `0` and `1`
    as opacity endpoints are not counted.
  - Modes: default **check** (compare with the baseline, exit 1 on a rise); `-Update` (rewrite the baseline from the
    tree); `-Inventory <path>` (one TSV row per hit: category, file, line, matched text, trimmed code); `-SelfCheck`
    (built-in synthetic fixtures: a value in a comment is ignored, a font in a string counts, an interpolated hole counts,
    a named token does not count, a rise fails, a move between files that keeps the category total passes).
  - Baseline `tools/design-tokens-baseline.tsv`: `file<TAB>category<TAB>count`, sorted, generated from dev.
  - Check rule: FAIL when a category total rises, or a file's count rises and the category total did not fall by at
    least as much. Counts below the baseline pass with a note ("baseline can be lowered: run -Update"). The FAIL message
    names file, category, old and new count and says: "use a token from Theme/ or Motion/, or raise the baseline with
    -Update and give the reason on the item card".
  - `tools/gate.ps1`: a `Tokens` step after `Seam`, same pattern (`Invoke-CapturedProcess`, PASS/FAIL, key lines).
    Runs in every mode including `-Ci` and `-Offline`.
  - First inventory written to `.coord/reports/A4a-token-scan-inventory.tsv`, with a per-category, per-file summary
    in the report.
  - Docs: `docs/loop.md` gate menu row and Files table name the `tokens` step and the script;
    `docs/coding.md` gets one line under a short "Design values" heading: new radius, font, color, shadow, opacity,
    motion and timer values go in `Theme/` or `Motion/` tokens; the gate's Tokens step counts literals.
- **Expected time:** about 45-75 min request to merge (similar tooling runs in `.coord/logs/usage.tsv`: Terra or Luna
  medium 3-10 min per run; plus gate, review, merge). No hand test (not visible).
- **Must not change:** any product file; the other gate steps' behavior and output; `check-platform-seam.ps1`.
- **Scope questions:** None open (spacing on the ratchet, owner here: answered 2026-11-01).
- **Test plan, code:** `pwsh -NoProfile -File tools/check-design-tokens.ps1 -SelfCheck` passes; the default check passes
  on the branch with the new baseline; a scratch edit adding `CornerRadius = new CornerRadius(5)` to a product file
  makes the check fail (reverted after, noted in the report).
- **Test plan, UI:** None (not visible).
- **Gates:** `tools/gate.ps1 -Task A4a-token-scan` (build, selftest, seam, format, scope, loop-lint for the doc edits,
  size, and the new Tokens step running on itself). Low tier: no Codex review or design review.
- **Needs from you:** Nothing. Master 3 tells Master 2 and Loop Hub 2 before merge that the gate gains a Tokens step.

## Files
Only these files may change (exact paths; `tools/gate.ps1` reads this list). Any other file needs a line under
"Out-of-scope files" in the report.
- `tools/check-design-tokens.ps1`
- `tools/design-tokens-baseline.tsv`
- `tools/gate.ps1`
- `docs/loop.md`
- `docs/coding.md`
- `.coord/reports/A4a-token-scan-inventory.tsv`

## Contracts
None: no product behavior changes. The check's own fixtures live in `-SelfCheck`.

## Approve
User, 2026-11-01: "make a branch and start with token scan only".

## For Codex
Role: terra (`docs/agents.md`), because it is a normal tooling feature across a few files that needs care with C#
lexing and the gate's step pattern, not hard shared state. The branch is already checked out: never switch, merge or
commit (you cannot write to `.git`). Read `AGENTS.md`, `docs/coding.md`, then the files above and
`tools/check-platform-seam.ps1` (masker to learn from; do not change it). Line numbers are from the current tree;
re-read before editing.

Constraints:
- Build exactly the Plan's "After" for the Files above. No product code changes, no drive-by refactors.
- Comments in the new script follow `docs/coding.md` (file header, short summaries, no item IDs or dates in code).
- Keep the categories' regexes in one table at the top of the script, each with a one-line comment of what it matches.
- If the Findings turn out wrong (for example the gate cannot host another step cleanly), stop and report instead.
- Build into `build/A4a-token-scan-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that
  folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A4a-token-scan -Offline`; the
  manager re-runs it online.
- Never launch or kill AntiNotepad. Never run `--quit`. Never edit `.coord/STATUS.md` or this card.

Done means (offline checks only: build, selftest, gate, script checks, files; never ports, servers, browser or screen):
- `tools/check-design-tokens.ps1 -SelfCheck` passes all fixtures listed in the Plan.
- The default check exits 0 on the branch; the baseline equals `-Update` output on the tree.
- A temporary added literal makes the check exit 1 with the Plan's message (reverted; say so in the report).
- `.coord/reports/A4a-token-scan-inventory.tsv` exists; the report gives counts per category and the top 10 files.
- The gate shows a `Tokens` step and passes; build 0 warnings, `SELFTEST PASS`.

Before you report (director only): spawn the `lens_reviewer` agent on your diff (it reads
`.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why not. Workers without spawning: run the same
checklist yourself and say so.

Report: `.coord/reports/A4a-token-scan.md`, short, with exactly these sections:
1. **Summary**: two or three sentences.
2. **Change list**: one row per changed block: `file:line-range | what changed | which Plan line it serves`.
3. **Behavior changes**: every changed public method result or meaning, new or changed shared state, timing, default, or
   setting effect. Write "None" only if true; the review checks this against the diff.
4. **Out-of-scope files**: file and why, or "None".
5. **Plan deviations**: anything done differently from the Plan, and why, or "None".
6. **Contracts**: rows added or changed, or "None".
7. **Lens review**: findings and what you did with each.
8. **Checks run**: build, selftest, gate result lines.
9. **Hand checks for the user**: the Plan's UI checks, plus anything new, or "None (not visible)".

## Check
Rounds: r1 (draft, scanner counted text inside strings, many forms missed) > r2 (string-aware masker, wider forms) > r3
(brush opacity and byte alpha, asked by Master 3 on 59599a8). Gate re-run online on the r3 tree.

- **Gate (online, `tools/gate.ps1 -Task A4a-token-scan`):** `GATE: PASS`. Build 0 warnings 0 errors; `SELFTEST PASS`;
  Seam PASS; Tokens PASS (`TOKENS PASS`); Format PASS (no product C# changed); Scope PASS (5 changed files, all allowed);
  LoopLint PASS; Size PASS; Cleanup PASS.
- **By hand:** `-SelfCheck` = `TOKENS SELFCHECK PASS`; default check exit 0; `-Update` leaves the baseline byte-identical;
  `git diff --stat dev...HEAD -- product` empty (no product file changed); scratch `new CornerRadius(5)` probe failed the
  check with file, category and counts (Codex ran it, reverted).
- **Bounds check:** Request, Plan and done agree: script with modes check / -Update / -Inventory / -SelfCheck, baseline TSV,
  `Tokens` gate step after Seam, docs lines, inventory TSV. Extra files: the report and the r2/r3 briefs under `.coord/tasks/`
  (none in product). Behavior changes: none to product; gate gains one step. Ask the user: none.
- **Misses, round 1:** (1) patterns ran over string contents, so text in a string could raise a count; (2) positional/enum
  forms missed (FontWeight.X, Color.Parse, shadow args, tween helpers), only 169 hits; (3) a duplicate Codex launch
  (my mistake) made two runs collide, so round 1 was discarded and rerun. r2 fixed 1-2; r3 added brush opacity and byte
  alpha (opacity 4 -> 45, new alpha 7).
- **Known limits (in the report):** arbitrary positional font sizes outside `Text`/`GlyphIcon`, target-typed font constructors
  and general drawing geometry are not counted; the inventory is a ratchet and guide, not proof of the look.
- **Verdict:** accepted.

## Hand test
None (not visible).
