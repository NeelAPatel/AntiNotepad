**In short**

A55-split-view-clip ran on loop v2 in era one, before the current approval and merge checks. Its card and reports still exist, so it remains useful as a historical example of a proven cause, a limited fix and a real hand test. Original history is preserved by tag `archive/full-history-2026-10-29`; use [Worked Example: Loop2](Worked-Example-Loop2) for v3.1 evidence.

# Worked Example: A55-split-view-clip

The era and version are recorded in `docs/loop.md`, "IDs and numbering" and "Sessions". The archive tag is recorded in `.coord/STATUS.md`, "Where things are". The historical merge hashes below belong to that archived history, not the rewritten dev sequence.

## Request and findings

The user reported on 2026-10-28: "in split view, scrolling text overlaps across the divider; the divider should crop the visual of each pane." The card preserves that request, the screenshots and the later scope answers: `.coord/tasks/A55-split-view-clip.md`, Request.

The item began as Medium tier because the cause of an editor rendering bug was not yet proved. Its audit traced the problem to moving panes with no stationary clipping parent. The card then records the user's screenshots confirming the visible overlap. This distinguishes a code-path explanation from a live automated reproduction: `.coord/tasks/A55-split-view-clip.md`, Findings; `.coord/reports/A55-split-view-clip-audit.md`, reproduction analysis.

## Plan and permission

The plan put each pane inside a stationary clipping host. Swipe and arrival motion, the divider, focus, undo and top blur stayed outside the change. It named four allowed product files, a structural contract and six screen checks. Source: `.coord/tasks/A55-split-view-clip.md`, Plan, Files and Contracts.

The user approved on 2026-10-29: "Go". This phase was called Go in loop v2; the current phase is Approve. Sources: `.coord/tasks/A55-split-view-clip.md`, Go; `docs/loop.md`, Phases and Sessions.

## Build and Check

The card's "For Codex" section assigns a Sol medium director delegating to a Luna xhigh worker. It forbids commits, branch changes, STATUS edits and app launches, and requires a lens review before the director reports. The implementation report describes the stationary hosts and their production-backed selftest: `.coord/reports/A55-split-view-clip.md`, Change list, Contracts and Lens review.

The recorded online gate passed with no build warnings, SELFTEST PASS, 11 contract rows (10 pass and one manual), no layering violations and all four changed files allowed. The fresh review found no proven defects and accepted. A 30-line selftest growth exceeded the 25-line advisory and was justified as one focused check. Sources: `.coord/tasks/A55-split-view-clip.md`, Check; `.coord/reports/A55-split-view-clip.review.md`, Findings and verdict.

The contract checked the structure. Actual pixel containment, caret/selection clipping and top blur remained hand checks. Calling that structural pass proof of the visible result would overstate the evidence: `.coord/tasks/A55-split-view-clip.md`, Contracts and Check.

## Hand test and historical merge

The card records the dev-copy paths and a clean log, but explicitly says the helper did not read the on-screen badge because Computer Use was off. The user's result on checks 0-5 was, on 2026-10-29, "all passed". A floating-scrollbar request in that same message became a separate idea. Source: `.coord/tasks/A55-split-view-clip.md`, Hand test.

In the archived history, `2a7a482` is "Merge A55-split-view-clip: clip each split pane at the divider (stationary clip hosts), CONTRACTS split-pane-clipping"; `ddb6ce7` is the following status commit. In the rewritten dev history, `6639e2c` is "A55-split-view-clip: build - clip each split pane at the divider". These are historical records, not the current squash-merge instructions. The live rule is one squashed item commit with STATUS included: `docs/loop.md`, "Branches, worktrees and commits".

## What this example teaches

The card separates the visible request, proven cause, allowed change, automated structural proof and user's screen result. A new request remains separate even when it arrives during a hand test. Those boundaries are visible in `.coord/tasks/A55-split-view-clip.md`, rather than inferred from a successful build alone.

## Sources

- `.coord/tasks/A55-split-view-clip.md` — complete historical item card, bounds check and user's hand-test words.
- `.coord/reports/A55-split-view-clip-audit.md` — findings and code-path reproduction.
- `.coord/reports/A55-split-view-clip.md` — implementation, contracts and checks.
- `.coord/reports/A55-split-view-clip.review.md` — fresh review and verdict.
- `docs/loop.md` — current phases, eras, version history and merge rule.
- `.coord/STATUS.md` — archive tag and history rewrite, "Where things are".
- Git history: `git log dev --oneline --grep=A55-split-view-clip` and `git log archive/full-history-2026-10-29 --oneline --grep=A55-split-view-clip` — rewritten and original subjects.
