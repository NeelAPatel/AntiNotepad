# pstack migration plan: what we adopt, word for word

Written 2026-11-15 for Loop Hub 3. Read-only research; nothing below is applied. Proposals have no numbers yet (loop.md: a number is given only on the user's confirmation), so items are `idea:` slugs.

## Summary in plain words

- **The user's picks:** (1) no comments, (2) linting and management procedures, (3) convert pstack to Claude format. The fourth bullet was an accident (the user said so); I do not guess it.
- **Pick 3 is already solved.** pstack ships an official Claude Code copy at `harness/pstack/claude-code/` in `github.com/cursor/plugins`. We do not install it (51 skills cost context every session). We lift a few sentences into our own files.
- **Nine adoptions**, all small. Four are text-only (templates and phase files). Two are new offline lints. One is a new rule table. One is a feature map. One is the comments policy, the only one that reverses an earlier user decision.
- **Linting finding:** our layer-boundary lint already exists (`SelfTest/Layers.cs` has dozens of LAYERS rules, plus `check-platform-seam.ps1` and `check-design-tokens.ps1`). My earlier "add a layer lint" suggestion was smaller than I said. The real gaps are: (a) comments have no lint, (b) the rule "every bug adds a CONTRACTS row" has no lint, (c) repeated mistakes (stale gate head 5 times, detached Codex launches) are fixed by text, not by structure.
- **Budgets:** `docs/loop.md` is already 8,331 bytes (budget 8,192) and `.coord/lessons.md` is 4,821 (budget 3,072). Nothing here adds to either. New text goes into unbudgeted files (phase files, templates, a new `docs/enforcement.md`).
- **User must decide:** see "What the user must decide" at the end. First one: how far no-comments goes.

## Source files used (all `github.com/cursor/plugins/blob/main/harness/pstack/claude-code/...`)
- `skills/no-comments/SKILL.md`, `agents/comment-sicko.md`
- `skills/correct/SKILL.md`, `skills/principle-encode-lessons-in-structure/SKILL.md`
- `skills/principle-prove-it-works/SKILL.md`, `skills/blast-radius/SKILL.md`, `skills/interrogate/SKILL.md`
- `skills/create-verification-skill/SKILL.md` and `references/feature-map-example/README.md`
- `skills/poteto-mode/playbooks/bug-fix.md`

## Adoption table

Format per row: pstack wording, our target, before text, after text, size, risk, undo. Sizes are bytes of the changed file.

### 1. Comments policy (idea: no-comments-policy). NEEDS USER DECISION
- **pstack source** (`skills/no-comments/SKILL.md`, `agents/comment-sicko.md`): "Spawn Comment Sicko. Act on accepted findings." Exceptions list: "Non-obvious behavior forced by an external dependency, platform, vendor, or protocol we cannot reshape. Surprises in our own code are meat." and "Doc comments that define a public API contract." Her slide: "Comments are liabilities".
- **Target:** `.claude/rules/coding.md`, section "## Comments" (lines 11-42, about 1.9 KB). Also `.claude/skills/review-task/phases.md` line 9.
- **Before (coding.md, the rules in full):** "1. File header. Each file opens with 1-3 lines... 2. Summaries. Each class and non-trivial method gets a `///` summary of what it does and why, not how. 3. Inline `//` only for the non-obvious... 7. Pipeline line. A method with several stages that must stay one method opens with its stages in order..." Rules 4-6 (no history, one word per idea, short) stay in spirit.
- **After (replace the whole "## Comments" section with):**

```
## Comments
Code explains itself through names, types and folders. A comment is a last resort for what the code cannot say.

1. **Keep** a comment only for a fact outside our code: a Win32 or Avalonia quirk, or a timing reason the platform forces. Start it with `// platform:` so the gate can tell it from narration.
2. **Delete on touch:** file headers, `///` summaries of our own code, narration, commented-out code, and history (item IDs, dates, "changed from X").
3. **A surprise in our own code is a rename or a split, not a comment.** If a reader would trip, change the name, the type or the structure.
4. **A method with stages** is written as named stage methods called in order (`Lex`, `Parse`, `Format`).
5. **A comment that says "do not change" or "must stay"** is a missing check: add a selftest or a CONTRACTS row, then delete the comment.
```
- **After (phases.md line 9).** Before: "Comments and formatting in the changed files follow `docs/coding.md` (on touch); a gap is a rework line, not a pass." After: "Formatting in the changed files follows `docs/coding.md` (on touch). An added or kept comment without a `// platform:` tag is a rework line, not a pass."
- **Size:** coding.md 5,345 to about 4,400 bytes. phases.md +40 bytes.
- **Risk:** reverses the user's own rule of 2026-10-29 ("user, 2026-10-29" in coding.md). Agents lose the in-file orientation unless names and folders carry it (pstack's bet; our one-job-per-file rule already helps). 746 comment lines exist today (326 `//`, 420 `///` in 94 files).
- **Undo:** revert one commit. Nothing in code changes until a file is touched.

### 2. Comment ratchet lint (idea: comment-ratchet-lint). Depends on row 1
- **pstack source** (`skills/correct/SKILL.md`): "If the pattern is already common, fail only when a change adds more." and "Exceptions go on the offending line with a reason, an expiry date, and a human's approval."
- **Target:** new `tools/check-comments.ps1` and `tools/comments-baseline.tsv`, new gate step `Comments` in `tools/gate.ps1`, same shape as `Tokens` (`check-design-tokens.ps1`: baseline TSV, `-Update`, `-SelfCheck`, prints "TOKENS FAIL: ..." plus a fix line).
- **Before:** no comment check exists.
- **After (rules the script enforces):** count whole-line comments (`^\s*//` and `^\s*///`) per `product/**/*.cs` file, excluding lines starting `// platform:`. Fail when a file's count is higher than its baseline row, or a new file has any. Output ends with: `COMMENTS FAIL: <file> has N comments (baseline M). Delete it, or rename/split so the code says it. Keep only outside facts as "// platform: ...".` When counts fall: `COMMENTS baseline can be lowered: run -Update`.
- **Offline check:** pure text scan, no build, no ports; `-SelfCheck` builds two tiny temp files (one over, one under) like the Tokens self-check.
- **False-positive risk:** low to medium. Commented-out code counts (intended). `//` inside multi-line raw strings could count; there are very few. Trailing comments (`x = 1; // why`) are NOT counted in v1 (string-aware parsing needed, e.g. `"http://"`); this is a known gap.
- **Size:** new files only; AGENTS.md unchanged.
- **Undo:** remove the gate step; the script is inert.

### 3. Finish line and "inconclusive" (idea: finish-line-rule)
- **pstack source** (`playbooks/bug-fix.md` step 4): "The original repro now passes. 'Inconclusive' or wrong-surface is not a pass. Flag it." (`skills/principle-prove-it-works/SKILL.md`): "Verify every task output by checking the real thing directly. Do not infer from proxies, self-reports, or 'it compiles.'"
- **Target:** `.coord/templates/item.md` (Plan list) and `.claude/skills/review-task/phases.md` (section 6).
- **Before (item.md):** `- **Test plan:** <selftest checks and `CONTRACTS` rows; UI: the exact hand checks, or "None (not visible)">`
- **After (add one line directly below it):** `- **Finish line:** <one result you can observe that proves it works, named before Build; if it cannot be observed, the answer is "inconclusive", never "pass">`
- **Before (phases.md, after the bullet "Every difference gets one line..."):** nothing.
- **After (add bullet):** `- **Finish line** (`## Plan`) must be observed, not inferred from "it builds". A check that could not observe it reports "inconclusive", which is not a pass; the item stops for the user.`
- **Size:** each new card +about 190 bytes (cards budget 4 KB). phases.md +230 bytes (3,109 to about 3,340, unbudgeted). `docs/loop.md` untouched.
- **Risk:** low. Slightly longer cards. Undo: revert the two edits (old cards keep working; the lint does not require the line).

### 4. "Safe because" line (idea: safe-because-line). From blast-radius
- **pstack source** (`skills/blast-radius/SKILL.md`): "Find the one or two facts the whole thing depends on and prove them by running code." and the ladder "You said so. Worthless on its own."
- **Target:** `.coord/templates/item.md`, Plan list, below "Must not change".
- **Before:** `- **Must not change:** <what stays as is>`
- **After (add below):** `- **Safe because:** <the one fact the change depends on, and how it was proven (read file:line, ran a selftest). Required for High tier>`
- **Size:** +about 170 bytes per card. **Risk:** low. **Undo:** revert one line.
- Note: our `lens-review` skill already keeps "only findings you can prove" (SKILL.md line 3), so no change to review wording.

### 5. Mistake-class rule table (idea: enforcement-table)
- **pstack source** (`skills/correct/SKILL.md`): "Keep a table in the agent instruction file that pairs each rule with what enforces it... Drop a rule once its mistake can't happen." and (`principle-encode-lessons-in-structure`): "If the fix is structural, only use the structural fix. The instruction is the symptom."
- **Target:** new `docs/enforcement.md`; one line in `AGENTS.md` Rules; one column change in `.coord/lessons.md` template text (no row edits).
- **Before (AGENTS.md):** no table. **After (add one bullet under "## Rules", about 110 bytes):** `- Each rule and what enforces it: docs/enforcement.md. A rule nothing enforces, repeated twice, gets a structural fix (lint, selftest, or script) in the same item.`
- **After (docs/enforcement.md, seed, rows from real evidence):**

```
# Rule table
Rule | Enforced by | Level
No literal radius, font, color, shadow, opacity or motion values | gate Tokens (`tools/check-design-tokens.ps1`) | lint, ratchet
Layers: Editor, Model, Theme and Platform never reference UI; Calc references nothing | selftest `SelfTest/Layers.cs` | test
Win32 only in Platform | gate Seam (`tools/check-platform-seam.ps1`) | lint
No stale phase, ID, version or missing path in loop files | gate LoopLint (`tools/loop-lint.ps1`) | lint
Changed files stay inside the card's Files | gate Scope | check
Formatting on changed lines | gate Format | lint
Gate report matches the tip at merge | `tools/merge-preflight.ps1` | check, late (hit 5 times: A2, A5, A7, T13, A14)
Every bug adds a CONTRACTS row | nothing (review-task Check only) | candidate: row 6
Codex runs as tracked background call, never `&` or nohup | nothing (docs/agents.md text only; hit twice: A16, A17) | candidate: guard
Selftests that open a PadWindow install the Fluent theme and run last | nothing (A9 lesson text) | candidate
```
- **Evidence:** the three "nothing" rows come from `.coord/lessons.md` and `.coord/lessons.md` Scores ("A16/A17 launched Codex detached with & or nohup"; A9 selftest hang). `tools/guard-bash.ps1` has no nohup or detach pattern by grep; Hub 3 should confirm before building a guard.
- **Size:** about 1.4 KB new, unbudgeted; AGENTS.md +110 bytes. **Risk:** low; table can go stale, so each lint's message should name its row. **Undo:** delete file and bullet.

### 6. "Bug needs a CONTRACTS row" warning (idea: fix-needs-contract-lint)
- **pstack source** (`skills/correct/SKILL.md`): "Enforce it with types... If bad code still compiles, add a lint or CI check whose error names the file, type, or function to use instead."
- **Target:** `tools/gate.ps1` new step `Contract`. Rule it enforces today only in prose (docs/loop.md: "Every bug fixed adds a `CONTRACTS` row").
- **Before:** no step. **After:** on a branch named `fix/*` whose diff touches `product/`, WARN (not fail) unless `product/AntiNotepad/SelfTest/Contracts.cs` is in the diff. Message: `CONTRACT WARN: fix/ branch changes product/ but adds no CONTRACTS row (SelfTest/Contracts.cs). Add one, or say on the card why none fits.`
- **Offline check:** `git diff --name-only dev...HEAD` only. **False-positive risk:** medium (a fix with no testable promise); so WARN only, and loop tool bugs are excluded by the `product/` condition. **Undo:** remove the step.

### 7. Feature map for one area (idea: settings-feature-map)
- **pstack source** (`create-verification-skill/references/feature-map-example/README.md`): "Each feature file starts with an H1 title and one paragraph describing the user-visible behavior. It then uses exactly four H2 sections in this order." (`Sub-features`, `How to get to it (user POV)`, `Driving it with <harness>`, `Gotchas`) and "Never drive an instance that was not started by this verification run."
- **Target:** new `docs/features/README.md` and `docs/features/settings.md` (start with Settings, 3-5 features); `tools/loop-lint.ps1` pattern list gains `'docs/features/*.md'` plus rule `feature-shape`.
- **Before:** none. **After (README, adapted, exact):**

```
# AntiNotepad feature map
How to reach and prove each user-visible feature. Read this before driving the dev copy.

## Baseline
- Drive only the dev copy from `tools/dev.ps1` with `ANTINOTEPAD_DATA` set to a temp folder. Never the installed copy; never `--quit` without `ANTINOTEPAD_DATA`.
- Seed notes come from the dev copy's fresh data folder (`tools/dev.ps1 -Fresh`).
- Press the hotkey with `tools/press-hotkey.ps1`; capture with F12 (`docs/dev-loop.md`; captures miss the Acrylic backdrop).

## Features
- [Settings](./settings.md)
```
  Each feature file: H1, one paragraph, then exactly four H2s: `Sub-features`, `How to get to it (user POV)`, `Driving it`, `Gotchas`. Contents are filled by the item chat from the code, not from this plan.
- **New lint (loop-lint `feature-shape`):** each `docs/features/*.md` except README must have the four H2s in order; otherwise `feature-shape: <file> needs ## Sub-features, ## How to get to it (user POV), ## Driving it, ## Gotchas`. Offline text scan.
- **Size:** settings.md about 1.5 KB, README about 0.7 KB. **Risk:** low; maps rot if nobody updates them, so the Check phase should name the map when a feature changes. **Undo:** delete folder and one pattern line.

### 8. Allow-list expiry (idea: lint-allow-expiry). Optional, low priority
- **pstack source** (`skills/correct/SKILL.md`): "Exceptions go on the offending line with a reason, an expiry date, and a human's approval."
- **Target:** `tools/loop-lint.ps1` lines 29-37 parser and `tools/loop-lint.allow`.
- **Before:** each entry is `path|rule|literal text`, preceded by a free `#` comment. **After:** a `# expires: YYYY-MM-DD` comment line directly above an entry makes the lint fail with `ALLOW EXPIRED: <entry>` after that date. Entries without it stay permanent. (A fourth `|` field is NOT used: the literal text may contain `|`.)
- **Risk:** low; a date passing makes the gate fail on an unrelated item. **Undo:** revert one parser edit.

### 9. Fix-naming messages (idea: lint-fix-hints). Optional
- **pstack source** (`skills/correct/SKILL.md`): "a lint or CI check whose error names the file, type, or function to use instead."
- **Target:** `tools/check-platform-seam.ps1` line 237 and `tools/loop-lint.ps1` `Write-Problem` (line 8-14).
- **Before:** seam prints `${relative}:${line}: $($match.Value)` and exits 1; loop-lint prints `path:line: rule: line text`. Tokens already ends with "Use a token from Theme/ or Motion/...".
- **After:** seam adds one closing line: `PLATFORM-SEAM FAIL: Win32 and raw platform types belong in Platform/. Call the wrapper there instead.` loop-lint appends a hint per rule, for example `stale-path` -> `use the current tools/ path`, `missing-path` -> `fix the path or remove the reference`.
- **Risk:** none for behavior; output only. **Undo:** revert.

## Linting section

### What we have today
| Layer | What it checks | Where | Failure style |
|---|---|---|---|
| Build | 0 warnings | gate Build | compiler text |
| Selftest | small capability checks, CONTRACTS rows, LAYERS rules (namespace boundaries, `Calc` references nothing) | `SelfTest/*.cs`, `Layers.cs` | check names |
| Seam | Win32 and raw platform types only in `Platform/` | `tools/check-platform-seam.ps1` | `file:line: match`, no fix line |
| Tokens | design literals, ratchet with baseline TSV | `tools/check-design-tokens.ps1` | names the fix (Theme/ or Motion/) |
| Format | `dotnet format` on changed lines only | gate Format | names the fix |
| Scope | changed files within card's `## Files` | gate Scope | lists files |
| LoopLint | stale paths, phases, IDs, versions, missing path references in loop docs and tools; hot-file budget WARNs | `tools/loop-lint.ps1` | `path:line: rule: text`, no fix line |
| Preflight | gate head equals tip, card sections present | `tools/merge-preflight.ps1` | at merge, late |

Coding rules (`.claude/rules/coding.md`) are mostly prose: comments, one-job-per-file, folder/namespace mirroring, 600-line trigger. Only Tokens, Format and Layers are enforced.

### pstack's lint approach, in her words
- `correct`: "Fix each class at the highest level that works": architecture, then types, then "a lint or CI check whose error names the file, type, or function to use instead", then tests, then "Write docs or agent rules last, only for judgment calls. Nothing fails when an agent skips them."
- `principle-encode-lessons-in-structure`: "Apply when you catch yourself writing the same instruction a second time... Encode the rule as a lint, metadata flag, runtime check, or script instead of more text."
- A class counts "once it has happened twice" (same as our loop.md: "Loop rules change only when `.coord/lessons.md` shows the same miss twice"). Existing patterns are ratcheted: "fail only when a change adds more". "Prove each new check fails on a real past mistake. Run the same command locally and in CI."

### New rules we would add (word for word)
| # | Rule and exact failure message | Checked how, offline | False-positive risk |
|---|---|---|---|
| L-a | `COMMENTS FAIL: <file> has N comments (baseline M). Delete it, or rename/split so the code says it. Keep only outside facts as "// platform: ...".` (row 2) | text scan of `product/**/*.cs`, baseline TSV | low-medium; trailing comments uncounted |
| L-b | `CONTRACT WARN: fix/ branch changes product/ but adds no CONTRACTS row (SelfTest/Contracts.cs). Add one, or say on the card why none fits.` (row 6) | `git diff --name-only dev...HEAD` | medium; WARN only |
| L-c | `feature-shape: <file> needs ## Sub-features, ## How to get to it (user POV), ## Driving it, ## Gotchas` (row 7) | headings scan in loop-lint | low |
| L-d | `ALLOW EXPIRED: <entry>` (row 8) | date compare in loop-lint | low |
| L-e | guard against detached Codex launch: block `codex-run.sh ... &` and `nohup ... codex` in `tools/guard-bash.ps1` with message `Run Codex as a tracked background Bash call (run_in_background: true), bare command; see docs/agents.md.` | regex on the command string; offline | low; needs Hub 3 to read `guard-bash.ps1` first, grep found no such pattern |

Proof step from `correct` ("prove each new check fails on a real past mistake"): L-a selftest uses a temp file with one added comment; L-b uses the A-item branches that fixed bugs; L-e uses the exact A16/A17 launch lines from `.coord/lessons.md`.

### Not a gap (corrected claim)
Layer boundaries are enforced already (Layers.cs, seam). I will not suggest a new "layer lint". The one open step in coding.md ("update the selftest LAYERS rules") is satisfied by namespace-prefix matching, so subfolders inherit their parent's rule.

## Not adopting, and why
- **pstack install and `/poteto-mode` router (23 playbooks, about 51 skills):** costs context every session; our phase files already route. Wording only is lifted (rows 3, 4, 5).
- **autopilot, swarm, shipping, babysit, `watch-pr` (Bun/TypeScript scripts):** built for GitHub PRs and `gh pr merge --squash`; we merge to `dev` by director. Windows and Bun friction.
- **40 `principle-*` skills:** the ones that matter are quoted above; the rest are style that costs tokens.
- **Failing-repro-commit-first** (`bug-fix.md` step 5, "the failing repro lands before the fix in git history"): conflicts with our "tidy commits that each build" and the per-commit selftest check (Loop8 lesson). We use the CONTRACTS row plus Finish line instead.
- **arena and interrogate (multi-adversary worktrees):** our `lens_reviewer` already keeps "only findings you can prove"; four adversaries would multiply Codex usage on a $20 plan.
- **`comment-sicko` agent as an always-on reviewer:** the ratchet lint does the job without a model run. It could be briefed once as a Codex `quick` run for the sweep (decision 2).
- **formal verification, statechart, visual-parity pixel diff, show-me-your-work TSV, `/loop` hill-climb:** no matching need today.
- **The fourth pick:** accidental per the user; nothing to adopt.

## Suggested order
1. **Text-only bundle (one card, Low, docs-tools):** rows 3 and 4. Edits `item.md` and `phases.md`. Fast, no code.
2. **Rule table (one card, Low):** row 5, seeded rows only. No code.
3. **Small lints (one card, Low-Medium):** rows 6, 8, 9 plus L-e if the guard check confirms. Each proven on a past case.
4. **Feature map for Settings (one card, Low):** row 7 with the Settings file filled by reading the code. Pairs with the ffmpeg trial and the three motion items.
5. **Comments:** user decision first, then rewrite (row 1), then ratchet (row 2), then an optional one-time sweep as its own item.

Why this order: 1 and 2 cost nothing and make every later card better; 5 is last because it reverses a user decision and changes 94 files over time.

## What the user must decide
1. **How far no-comments goes** (first, others wait): (a) full, as in row 1, delete on touch; (b) keep file header and `///` summaries, ban only inline narration; (c) keep as is. Recommendation: (a), because the ratchet makes it safe and names plus folders carry meaning; the risk (agents lose orientation) is checked on the first touched files.
2. **One-time comment sweep** of all 746 lines? Cost: one Codex `quick` run per folder, a few hundred thousand tokens in total (estimate, unmeasured). Or only on touch (free, slow).
3. **Bundle or separate:** one card for rows 3+4+5, or three? Recommendation: one text-only bundle.
4. **Expiry dates (row 8) and fix hints (row 9):** do now or skip. Recommendation: skip row 8 for now, do row 9 with row 6.
