# Dev loop vs Claude Code's native features (exploration, 2026-11-07)

Inputs: `docs/loop.md` (35 KB), `docs/dev-loop.md`, `AGENTS.md`, `.claude/` (5 skills, 1 agent, 1 output style, 2 hooks),
`tools/*.ps1`, `.coord/lessons.md`, `.coord/logs/usage.tsv`; official docs at code.claude.com/docs (hooks, permissions,
memory, skills, sub-agents, worktrees, headless, costs, plugins). The docs were read by two helper agents through a fetch
step, so items marked (unverified) need a check in your build before anything depends on them.

## 1. Rules that are prose today and could be enforced by the harness

Principle: a hook or permission rule runs every time, costs zero model tokens and cannot be forgotten. Prose in loop.md is
only followed when the model remembers it. Move every "never / only / before" rule that a script can test.

| Today (prose, memory or reminder) | Native replacement | Notes |
|---|---|---|
| "Only a master merges to dev / moves main" | `PreToolUse` hook on Bash with `if: "Bash(git merge *)"` (and `git push`, `git checkout main`) that exits 2 unless the session is the master checkout or the item card says `hand-tested` | `if` takes one permission rule, no `&&`. A Bash rule misses `git -C . push`, so the hook parses the command itself |
| "Never run AntiNotepad.exe --quit without ANTINOTEPAD_DATA" (memory file) | `PreToolUse` hook that denies any command containing `--quit` or `%LOCALAPPDATA%\AntiNotepad` without the dev env | Turns a memory note into a wall |
| "Sub-sessions never edit STATUS, lessons, loop.md" | `permissions.ask` or `deny` on `Edit(.coord/STATUS.md)` etc., plus a hook for Bash writes | `ask` doubles as your "loop freeze" switch: flip one line instead of a rule paragraph |
| "Gate must pass before Check / Merge" | `Stop` or `TaskCompleted` hook that runs `tools/gate.ps1 -Fast` and blocks finishing with the failing lines | Must check `stop_hook_active` to avoid loops |
| "Never launch dev.ps1 during review or rework" | `PreToolUse` hook reading the card phase | Phase becomes data, see section 3 |
| Per-turn status-card reminder (`loop-card.ps1`) | Already duplicated by the output style. Drop the hook's reminder line; show items in flight in the **status line** | Status line is free of tokens, updates on screen, and can show branch, worktree, context %, cost |
| `director-notes.ps1` after compaction | Keep (`SessionStart` matcher `compact`). Extend to every session: print the card phase and next action for the current branch | Makes sessions disposable |
| Skills' descriptions in context every turn | `disable-model-invocation: true` on next-task, review-task, merge-task, scan-backlog (you type them anyway) | Removes their listing from every turn |
| Skill reads git state with several tool calls | `` !`cmd` `` preprocessing: the output (diff stat, gate summary, card phase) is in the skill before the model sees it | A non-zero exit aborts the skill, which is itself a guard |
| Review in the build session's context | `context: fork` plus `agent:` on review-task: its own window, only the summary returns | Cheaper and cleaner than the builder's long context |
| "Sonnet medium builder, Haiku auditor" prose | Agent files with `model`, `effort`, `tools`, `maxTurns`, `skills`, `omitClaudeMd` | Replaces the role table with config |
| docs/coding.md (5 KB) loaded when relevant | `.claude/rules/coding.md` with `paths: product/**` | Loads only when product files are touched |

Things that do **not** transfer:
- The sandbox is not available on native Windows (WSL2 only), so protection comes from hooks and deny rules, not isolation.
- Bash permission rules are convenience, not a security boundary.
- `@import` does not shrink context; `CLAUDE.md` -> `@AGENTS.md` costs AGENTS.md every turn (small, about 3.6 KB).
- Plugin sub-agents ignore `hooks` and `permissionMode`, so packaging the loop as a plugin loses some enforcement. Keep it
  in `.claude/` for now; git already versions it.
- Native `--worktree` and `isolation: worktree` branch from the remote default branch (main), not dev, unless
  `worktree.baseRef` is `head`. Check how the app-made worktree `claude/dreamy-raman-7c3f1c` was based. Keep
  `tools/approve.ps1` for item worktrees; docs describe single names only, so `feat/A1-x` nesting is unverified.
- Your Codex-in-visible-sub-sessions rule is a desktop-app session feature, not the native sub-agent tool. Both coexist.

## 2. Channels: stable, dev, in-the-moment

You already have the right shape. Name it as three channels and make each one hard to cross.

| Channel | Built from | Where it runs | Data | Promotion |
|---|---|---|---|---|
| Stable | tag on `main` | installed copy | real notes | user runs `install.ps1` in a normal terminal |
| Dev | tip of `dev` | `build/dev` | `build/testdata` | merge of an item |
| Item | worktree branch | worktree `build/dev` | per-checkout test data | hand test, then merge |

Gaps, cheapest first:
1. `install.ps1` should refuse unless HEAD is a tag on main and the gate result for that SHA is green (script, deterministic).
2. The prod-data hook from section 1 makes the "installed copy is never touched" rule a wall.
3. CI today is manual. Run `gate.ps1 -Ci` on push to `dev` and on `main` tags, so the backstop does not depend on a session.
4. Parallel hand tests: one dev copy at a time is the real throttle. A per-worktree instance id and hotkey derived from the
   branch would allow several at once. Medium engineering, optional.
5. "Do not break old features" lives in `CONTRACTS` plus selftest plus per-commit build. Keep that as the spine. Candidate to
   evaluate: Avalonia's headless test support for keyboard and render checks without a desktop (first-party; not verified here).

## 3. Development, review, testing with less AI memory and cost

Observed in your own data:
- `lessons.md`: of about 30 misses, most are `plan` or `env`, and "stale Head" in the merge preflight recurred three times
  (A2, A5, A7). These are script and plan-shape defects, not model weakness. Fix the script and template, not the prose.
- Every master is told to read all of loop.md (about 9k tokens) plus STATUS (419 lines). Sub-sessions repeat that.
- 125 cards and 247 reports exist; the card is already your memory, which is the right idea.

Direction: **scripts own state and transitions, models own judgment.**
1. **Card front matter as the state machine.** Phase, tier, round, files, branch in machine-readable fields. One
   `tools/loop.ps1 <verb>` (`approve`, `check`, `handoff`, `merge`, `can-merge`) does the deterministic steps and prints a
   short summary. Skills shrink to "run the verb, read the summary, decide the exceptions".
2. **Hooks consult the state.** `git merge` is blocked unless `loop.ps1 can-merge <ID>` passes; dev.ps1 is blocked in
   review; the Stop hook runs the gate. The rules cannot drift because they are code.
3. **Split loop.md by audience.** Core (5 KB: phases, tiers, who merges) stays; phase detail moves into the skill that runs the
   phase, loaded only then; branch and merge detail into merge-task. Masters stop reading 35 KB.
4. **Review ladder, cheapest first.** Script gates (zero tokens) -> targeted review skill in a fork with pre-injected facts ->
   Codex lens review only Medium and High -> design review only High. Scores in lessons.md already tell you which rung earns its cost.
5. **Structured verdicts.** For automated review or audit runs: `claude -p --output-format json --json-schema <verdict>
   --max-turns N --no-session-persistence --model haiku|sonnet`. Parse the field, do not read prose. `--bare` skips hooks,
   skills and CLAUDE.md but needs an API key, so it does not fit a subscription (unverified `--max-budget-usd` semantics).
6. **Tiered gate.** `gate.ps1 -Fast` (build, affected selftest area, scope, format) for the Stop hook and mid-build; full
   gate only at Check and Merge.
7. **Disposable sessions.** With the card as truth and a SessionStart hook that prints "phase, next action, files", a fresh
   session per phase costs less than a long one, and subscription cache TTL is 1 hour so warm context is not the main saving.
8. **One config file for the dials you change.** `.coord/loop.config.json`: tier -> default gates, item cap, rework cap,
   role -> model and effort, freeze on/off. Scripts and hooks read it; loop-lint checks that settings and skills agree with
   it. Changing the loop becomes editing one file plus bumping `loopVersion`, with the lint as safety net.

Where the real savings are (estimates, not measured): not reading 35 KB loop.md and STATUS in every session (largest), fewer
tool calls via `!` injection, forked reviews returning summaries, skipping Codex on Low items. Dropping the per-turn hook
text saves little (about 150 tokens a turn) but removes a duplicate instruction.

## 4. Suggested items (unnumbered ideas until you confirm)

| Idea | Risk | Gives |
|---|---|---|
| `native-guards`: hooks plus ask/deny rules for merge, prod data, loop files | Low | rules become walls |
| `skill-diet`: disable-model-invocation, `!` injection, fork review, rules split | Low | fewer tokens, fewer tool calls |
| `status-line`: terminal status line wrapping the user's global one, plus one project line (worktree marker, item, in-flight items); trim `loop-card.ps1` (corrected 2026-11-08: no context % or cost, the global line already has ctx and limits; Desktop does not draw status lines) | Low | no per-turn reminder |
| `loop-split-docs`: loop.md by audience | Low | masters stop reading 35 KB |
| `loop-state-cli`: card front matter plus `loop.ps1` plus can-merge hook | Medium | determinism |
| `ci-on-dev`: gate on push to dev, release check in install.ps1 | Low | backstop |
| `loop-config`: one config file plus lint | Medium | flexible updates |

## Unverified in the docs pass
`additionalContext` placement for PreToolUse; full settings precedence order; `dontAsk` semantics; `worktree.*` value
formats; WorktreeCreate and PreCompact input fields; `--max-budget-usd`; whether `--worktree` accepts `prefix/slug`.
