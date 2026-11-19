# Playbook: the loop as a habit

One page. Rules and reasons are in `docs/loop.md`; this is only what you do. Words: an **item** is one change; a
**director** is a top-level chat (Master 4 for app work, Loop Hub 2 for `Loop`, `L`, `T`); the **item chat** is the one chat
for an item (it does everything but merge); the **card** is the item's file `.coord/tasks/<ID-slug>.md`; a **hook** is a small script Claude Code runs on
its own before or after an action.

## The same five steps for every item
| Step | You do | Then |
|---|---|---|
| 1 | Type `/next-task` (or describe the change) in the right director chat | Director writes the Request and starts the item chat; open that chat, it is the one you talk to |
| 2 | In the item chat: read the plan; answer "Approve?" | Item chat runs `approve.ps1` (branch and worktree) and builds with Codex |
| 3 | Stay in the item chat while it checks (`/review-task`) | It hands you a hand test, or "None" for non-visible items |
| 4 | Do the hand test; say **pass** or **fail** (your own words) | Item chat records it on the card and says it is ready |
| 5 | Tell the director `/merge-task <ID-slug>` | Director merges to dev, re-checks, pushes, cleans up |

## Pick the director
| Item | Example | Start `/next-task` in | What differs |
|---|---|---|---|
| **A** (app) | `A4-design-tokens` | Master 4 (app director) | Step 4 is a real hand test: the item chat opens the item's dev copy (never your installed copy) and gives you UI checks |
| **T** (tooling) | `T11-roadmap-hide-rows` | Loop Hub 2 | Hand test is usually "open the page or run the script, say if it works" |
| **L** (Level 4/5 loop config) | `L7-lv4-playbook` | Loop Hub 2 | Hand test is reading the file or trying the command; hooks start in log-only mode, so nothing can lock you out |

## What runs for you, and what you still do
| Part | Runs by itself | You still do |
|---|---|---|
| **Guard hook** (L1, `tools/guard-bash.ps1`) | Before every Bash command it checks: merge/switch to main or dev/push to main or dev from an item worktree; rebase or force-push in the main dev checkout; anything touching the installed copy (`--quit`, `Programs\AntiNotepad`) without `ANTINOTEPAD_DATA`. Logs each hit to `build/guard.log` (per checkout) | Read the log now and then. It only logs today (`tools/guard.mode` says `log`); you decide when to switch it to `block` |
| **Poteto gate** (Loop16, `tools/poteto-gate.ps1`) | Refuses a director start of an item chat whose prompt does not open with `/pstack:poteto-mode` plus the Skill-call line; blocks an item chat once if its first turn neither launched the skill nor showed the command in a code block. Logs to `build/poteto-gate.log` | If blocked, copy the command it shows. Log-only until you say block |
| **Ask rules** (L1, `.claude/settings.json`) | Claude Code asks you before editing `.coord/STATUS.md`, `.coord/lessons.md` or `docs/loop.md` | Say yes when it is the director doing a planned edit; no otherwise |
| **Worktree check** (L2, `tools/worktree-check.ps1`) | At session start, logs a worktree whose path or branch is not `<prefix>/<ID-slug>` or that was cut from main, to `.coord/logs/worktree-check.log` | Create worktrees only through `approve.ps1`. The native setting `worktree.baseRef` is `head` |
| **Agents** (L3, `.claude/agents`) | `builder`, `reviewer` (cannot edit files), `auditor` (read-only, Haiku) have fixed model, effort and tool limits | Nothing; directors pick them |
| **Typed-only skills** (L4) | `/next-task`, `/merge-task`, `/scan-backlog` no longer start from plain words ("merge X" does nothing) and no longer cost context each turn. They pre-load git facts | **Type the slash command yourself** |
| **Status line + start-up note** (L5) | The bottom of the chat shows `loop: [WT] branch, item, in flight`. At session start the chat prints the item's phase and next step | Glance at it; it replaces asking "where are we" |
| **Slimmer docs** (L6) | Directors read about 18 KB at start instead of 39 KB; phase detail loads inside the skill that needs it; coding rules load only when product files are touched | Nothing |

Still entirely yours: approving the plan, hand test pass/fail, "release" at a marker, installing to your real copy
(`tools/install.ps1` in a normal terminal), and telling a director when to merge.

## Slash commands
| Command | Who types it | Where | When |
|---|---|---|---|
| `/next-task [ID-slug]` | you | director chat | start an item or take the next planned one |
| `/review-task <ID-slug>` | the item chat (you may) | item chat | after Codex finishes a build or rework round |
| `/merge-task <ID-slug>` | you, to the director | director chat | after a passed hand test |
| `/roadmap` | you | any chat | open the plan page `http://localhost:47811` |
| `/scan-backlog [slug ...]` | you | director chat | rank several ideas before choosing |

## A hook or rule stopped something
| What you see | Why | Do | Turn it off |
|---|---|---|---|
| A Bash command refused with a short reason such as "Merging in an item worktree is blocked" (only after the guard is switched to `block`) | The command was run in the wrong place for the loop | Hand the step to the director chat, which owns merges, or run it from the main dev checkout | Put `log` in `tools/guard.mode` |
| "Installed-copy command requires ANTINOTEPAD_DATA" | The command mentions `--quit` or the installed folder | Use `tools/dev.ps1`, which sets the dev data folder; never touch the installed copy | Same one-word change |
| Claude Code asks "edit STATUS.md?" | Ask rule on loop files | Answer yes only for a planned edit | Delete that line under `permissions.ask` in `.claude/settings.json` |
| `approve.ps1` says `FAIL (cap)` | 3 items are already building | Wait for a merge, or tell the director you accept a one-off `-Cap 4` | n/a |
| Typed "merge X" and nothing started | Skills are typed-only now | Type `/merge-task <ID-slug>` | Remove `disable-model-invocation` from that skill |

If a hook itself crashes, it logs and lets the command through; it never freezes your session.

## Results so far (measured in the L1-L6 reports)
| Measure | Before | After | Source |
|---|---|---|---|
| Skill descriptions in every turn | 1,395 B | 150 B | L4 report |
| Director start reading (AGENTS.md + loop.md) | 39,078 B | 18,225 B (about 53% less) | L6 reports, byte check on merged files |
| Per-turn hook text | 487 chars | 279 chars | L5 report |
| Guard hook policy self-tests | none | all pass on PowerShell 7 and 5.1; log and block modes both exercised | L1 report |
| Status line cost | n/a | about 358 ms per run (target was 300) | L5 report |

Not yet measured: how many real mistakes the guard stopped (it only logs so far), and the auditor agent's real model and
tools (L3 could not run one without a login). Both are fair to state as open when showing this off.

## Checked against the real setup
Dry run on the merged `dev` (2026-11-08): every command, file and setting named above was matched to its file. The
guard message wording and the `log`/`block` file come from `tools/guard-bash.ps1` and `tools/guard.mode`; agent limits
from `.claude/agents/*.md`; skill flags from `.claude/skills/*/SKILL.md`.
