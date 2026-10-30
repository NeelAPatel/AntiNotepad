**In short**
The loop gives each person, chat and script one job. Claude coordinates the work; Codex implements and checks code; you approve the plan and test the visible result. Scripts carry the routine checks between them, so a chat's confidence alone does not move an item forward. These responsibilities come from `docs/loop.md`, "Phases" and "Sessions", and `docs/agents.md`.

# Who does what

A **sub-chat** is a visible Claude session in the sidebar. A **subagent** is a delegated agent inside a run. A **headless run** is Codex running through the command line without its own interactive chat. The distinction matters: the Claude builder can commit, while the Codex process it launches cannot (`docs/loop.md`, "Sessions"; `docs/agents.md`, "Briefs"). See [Glossary](Glossary).

## You and the main chat

### You

- **Kind/model:** person; no model or effort setting.
- **Started by/given:** you send a request or `/next-task`; the master shows Now → After, Files, tests and gates before asking Approve. Later the tester gives the exact UI checks.
- **Opening line:** no fixed user wording. The request and your decisions are recorded verbatim, with dates.
- **May/may not:** approve, change or drop the plan; decide unresolved scope and judgment questions; hand test; request Merge or a release. A release requires your explicit go.
- **Leaves:** Request, Approve and Hand test evidence on the item card.

Source: `docs/loop.md`, "Phases", "Triggers in practice"; `.claude/skills/next-task/SKILL.md`, steps 1 and 4; `.claude/skills/merge-task/SKILL.md`, opening conditions.

### Master

- **Kind/model:** one main Claude chat; the rules do not fix its model or effort.
- **Started by:** the user's new-master prompt in `docs/loop.md`, "Sessions"; [Getting Started](Getting-Started) quotes it in full. Its opening is: "You are `Master <N+1> · Loop v<V> · <slug>` for AntiNotepad."
- **Given:** `AGENTS.md`, all of `docs/loop.md`, `.coord/STATUS.md`, then the user's request and the item card. `/next-task` is defined in `.claude/skills/next-task/SKILL.md`.
- **May/may not:** owns Request through Approve, STATUS, the roadmap and Merge; does the short Low-tier scout; starts linked sub-chats. It does not launch Codex, build or launch the hand test itself. It cannot silently approve an idea's scope for the user.
- **Leaves:** the approved card, linked sessions, current STATUS, and the merged item plus lessons on dev through `/merge-task`.

Source: `docs/loop.md`, "Sessions"; `.claude/skills/next-task/SKILL.md`, steps 1–5; `.claude/skills/merge-task/SKILL.md`, steps 1–8.

The optional **loop discussion chat** is a separate main chat, with no fixed model. It observes the loop and proposes ideas; it edits no STATUS, merges nothing and runs no item unless the user hands one over. Loop2-loop-v3-1 was that exception (`docs/loop.md`, "Sessions"; `.coord/tasks/Loop2-loop-v3-1.md`, opening and Approve).

## Claude sub-chats

The master uses `start_session` with `use_worktree: false`: the session works in the already selected checkout using absolute paths, rather than creating another worktree. Audit and backlog-scan launch instructions also specify `user_asked` and `wake_when_done`. The master waits for completion notes instead of polling (`.claude/skills/next-task/SKILL.md`, steps 2 and 5; `.claude/skills/scan-backlog/SKILL.md`, step 3).

### Scout

- **Kind/model:** Claude sub-chat, Haiku; no effort is fixed. A small Low scout stays in the master; a separate scout is rare. Backlog scans use one Haiku scout per idea.
- **Started by:** `start_session`, titled `scan · <slug>` for backlog research; item-step names follow `<ID> · scan · <slug>`.
- **Given:** a filled `.coord/templates/scan.md` with snapshot absolute path, full commit SHA, idea row, screenshots and the one report path. Its opening sentence is exactly: "Brief for one Haiku scout (role `scout`), read-only, no Codex."
- **May/may not:** read the detached snapshot; write only the specified research report in the main checkout. No snapshot edits, fix plan, commit or Codex launch.
- **Leaves:** a research Markdown file under .coord/research/, stamped `Scanned at: <full sha>`, and its absolute path as the completion note.

Source: `.coord/templates/scan.md`; `.claude/skills/scan-backlog/SKILL.md`, steps 2–5; `docs/loop.md`, "Sessions". The rare separate Low scout has no distinct fixed opening prompt in those rules; do not infer the backlog template is mandatory for it.

### Auditor

- **Kind/model:** Claude sub-chat, Haiku; no effort is fixed.
- **Started by:** the master's `start_session`, titled `<ID> · audit · <slug>`, for Medium/High Findings.
- **Given:** the master's launch-and-copy instructions in `.claude/skills/next-task/SKILL.md`, step 2, plus the filled findings brief. There is no fixed auditor opening sentence; the skill specifies its payload and actions.
- **Does:** launches `CODEX_SANDBOX=read-only bash .coord/tools/codex-run.sh director <ID-slug>-audit .coord/tasks/<ID-slug>-audit.md` in the background. It copies only the report named by this invocation's successful `DONE via <role>. Report: <path>` line to the item audit report. No DONE line means no copy.
- **May/may not:** no product edits or commits before Approve; no report reuse from an earlier run.
- **Leaves:** the audit report path, its `READY:` line and the wrapper's last lines.

Source: `.claude/skills/next-task/SKILL.md`, step 2. The rule files are inconsistent: `docs/loop.md:208` (Sessions) says the auditor "commits it," while `docs/loop.md:241-242` (Branches) defers the commit to Approve and `.claude/skills/next-task/SKILL.md:37-46` (step 2) says to commit nothing. This wiki follows the skill's step 2: no commits before Approve.

### Builder

- **Kind/model:** Claude sub-chat, Sonnet medium.
- **Started by:** master `start_session` after Approve, titled `<ID> · build · <slug>`.
- **Given:** the item path and the bullet brief in `.claude/skills/next-task/SKILL.md`, step 5. Its first role instruction is exactly: "your role is `builder`; show it on the status card's first line; rename yourself `<ID> · review · <slug>` (role `reviewer`) when Check starts and `<ID> · test · <slug>` (role `tester`) at the hand test;"
- **Does:** reads the card, launches the Codex director in that worktree, then runs `/review-task` when it exits; relaunches permitted rework, commits each round on the item branch and pushes.
- **May/may not:** works only in its assigned path, never edits dev or STATUS, never mints IDs. A docs change of a few lines may be made by the builder itself if its report says so.
- **Leaves:** item-branch commits, reports and the card's Build/Check/Hand test evidence.

Source: `.claude/skills/next-task/SKILL.md`, step 5; `docs/loop.md`, "Rules".

### Reviewer

- **Kind/model:** the same Claude sub-chat renamed `<ID> · review · <slug>`, Sonnet medium. A fresh reviewer chat is used only if the build session stalls.
- **Started by/given:** the completed Codex run triggers `/review-task <ID-slug>`; inputs are the approved card, current round report, planned gates and report summaries. A separate fixed opening prompt is not defined.
- **May/may not:** catches up, gates, commissions planned independent reviews, checks requested vs approved vs done, and triages failures. Never fixes product code in Check. Stops for missing Plan/Approve, an unanswered scope question or exhausted rework allowance.
- **Leaves:** Check result lines, bounds decisions, Misses and Noticed follow-ups on the card; acceptance or a routed failure.

Source: `docs/loop.md`, "Sessions"; `.claude/skills/review-task/SKILL.md`, steps 1–4a.

### Tester

- **Kind/model:** the accepted builder/reviewer chat renamed `<ID> · test · <slug>`, Sonnet medium. A separate test chat is only for a user-requested re-test.
- **Started by/given:** Check acceptance, the Plan's UI checks and any new checks in the report; no separate fixed opening prompt.
- **Does:** runs `tools/dev.ps1` immediately before hand checks, verifies EXE, DATA and log paths, and tells the user the expected dev badge. Records the user's result verbatim and reports to the master.
- **May/may not:** no launch during review or rework; no use of the installed copy. Hand tests take turns, oldest accepted first.
- **Leaves:** the committed Hand test section or a triaged failure. Nonvisible work can omit a hand test; High tier always requires one.

Source: `.claude/skills/review-task/SKILL.md`, step 5; `docs/loop.md`, "Tiers", "Sessions" and "Branches, worktrees and commits"; `docs/dev-loop.md`, "Dev copy".

## Codex runs and agents

### Shared launch and permissions

The Bash script `.coord/tools/codex-run.sh` is a launcher, not an AI agent. Its `attempt` function passes the entire brief file on standard input:

```bash
codex exec -m "$model" -c model_reasoning_effort="$effort" -c 'windows.sandbox="unelevated"' \
  -C "$WORKDIR" --sandbox "$SANDBOX" --output-last-message "$out" "${EXTRA[@]}" - < "$file" > "$log" 2>&1
```

The default sandbox is `workspace-write`; only `workspace-write` and `read-only` are accepted. Findings explicitly selects `read-only`. A fresh review uses the default sandbox but its brief forbids product edits. Codex never commits, switches branches, launches or kills AntiNotepad. Temporary build/selftest work uses the task's build folder and is cleaned up (`.coord/tools/codex-run.sh`, sandbox validation and `attempt`; `docs/agents.md`, "Briefs"; `.coord/templates/review.md`).

### Findings director

- **Kind/model:** headless Codex run, `gpt-6.1-sol` medium, with launcher failover below.
- **Started by:** the auditor's read-only command above.
- **Given/opening:** filled `.coord/templates/findings.md` via stdin. Its role line starts exactly: "Role: director (gpt-6.1-sol medium), read-only. Product `product/AntiNotepad`."
- **May/may not:** reads the master checkout on dev and answers seven evidence questions; cannot write files, build probes that write, switch, merge or commit. The wrapper captures its final message outside the agent's file-writing work.
- **Leaves:** final-message audit ending `READY: yes` or `READY: no - <what is missing>`; the wrapper stores the last message, log and run-counter row.

Source: `.coord/templates/findings.md`; `.claude/skills/next-task/SKILL.md`, step 2; `.coord/tools/codex-run.sh`, `attempt`.

### Build director

- **Kind/model:** headless Codex run, `gpt-6.1-sol` medium, delegating to Luna workers.
- **Started by:** builder runs `bash .coord/tools/codex-run.sh director <ID-slug> .coord/tasks/<ID-slug>.md` from the item worktree.
- **Given/opening:** the whole card on stdin, including its For Codex instructions. The exact template role line starts: "Role: <director (gpt-6.1-sol medium) | worker (gpt-6-luna xhigh)>. Product `product/AntiNotepad`." The filled card selects the role; the template is not an additional hidden prompt.
- **May/may not:** implements only the approved After and Files; stops if Findings proves wrong; never edits STATUS or the card. Before reporting it spawns `lens_reviewer`, fixes proven issues or explains why not.
- **Leaves:** implementation plus the nine-section report requested by `.coord/templates/item.md`; wrapper last message and log.

Source: `.coord/templates/item.md`, "For Codex"; `docs/agents.md`, "Briefs"; `.coord/tools/codex-run.sh`.

### Codex workers

- **Kind/model:** Codex subagents delegated by the director, `gpt-6-luna` xhigh. The wrapper also supports a separate headless `worker` tier with that model.
- **Started by/given:** the director delegates a bounded task; the rules fix neither a per-worker command nor an exact opening prompt. A direct worker run uses `bash .coord/tools/codex-run.sh worker <ID-slug> <brief-file>` and the same stdin mechanism.
- **May/may not:** obey the parent/card scope and Codex restrictions. A worker without spawning performs the same lens checklist itself and says so.
- **Leaves:** delegated results to the director, or the card-format report for a direct worker run.

Source: `docs/agents.md`, Launch table; `.coord/templates/item.md`, "Before you report"; `.coord/tools/codex-run.sh`, `tier` and `attempt`.

### In-run `lens_reviewer`

- **Kind/model:** Codex subagent, `gpt-6.1-sol` medium.
- **Started by/given:** director spawns project role `lens_reviewer` on its diff before reporting. It receives the card, reports and diff as defined by `.agents/skills/lens-review/SKILL.md`, "Inputs".
- **Exact opening:** "You are the lens reviewer for AntiNotepad. Read `.agents/skills/lens-review/SKILL.md` and follow it exactly."
- **May/may not:** no product edits, commits, app launches/kills or `--quit`. May build and run selftest in a temporary review folder to prove a finding, then deletes it. Findings require a failing test/probe or an exact code path; guesses remain Unproven risks.
- **Leaves:** the skill's review format as its final message; no report file unless asked.

Source: `.codex/agents/lens-reviewer.toml`; `.agents/skills/lens-review/SKILL.md`, "Proof rule" and "Output format". The nested-worktree rehearsal proved file visibility with no trust warning, not a live spawn (`.coord/reports/Loop2-loop-v3-1-rehearsal.md`, check 7).

### Fresh reviewer

- **Kind/model:** separate headless Codex run, `gpt-6.1-sol` medium by default; `CODEX_REVIEW_EFFORT` overrides effort. It did not implement this round.
- **Started by:** Claude reviewer runs `bash .coord/tools/codex-run.sh reviewer <round>-review .coord/tasks/<round>-review.md` for Medium/High or a planned extra gate.
- **Given/opening:** filled `.coord/templates/review.md` via stdin: "Role: reviewer (fresh run, read-only for product code). Product `product/AntiNotepad`." Inputs include card/round briefs, implementation and gate reports, committed and uncommitted product diffs.
- **May/may not:** follows the lens checklist; may use temporary proof builds, never edits product code, switches, merges or commits. This is a brief restriction within `workspace-write`, not the audit's `read-only` sandbox.
- **Leaves:** the requested round review Markdown report, ending `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.

Source: `.claude/skills/review-task/SKILL.md`, step 3; `.coord/templates/review.md`; `.coord/tools/codex-run.sh`, `tier`.

### Backup and run counter

The backup is a headless `gpt-5.6-luna` xhigh run with the same brief and sandbox as the attempt it replaces. It has no separate role opening: stdin still contains the original card/audit/review brief. `.coord/tools/codex-run.sh`, `next`, defines director → worker → backup → stop and reviewer → backup → stop.

Failover happens on a reached counter cap or a matching usage/rate-limit/capacity error. Other failures, including the timeout, stop rather than retry. Default counters per date/budget tag are director 20, worker 10, reviewer 10, backup 5. These count wrapper attempts, including failures, rather than tokens or delegated subagent calls. `CODEX_MAX_DIRECTOR`, `CODEX_MAX_WORKER`, `CODEX_MAX_REVIEWER` and `CODEX_MAX_BACKUP` override caps; `CODEX_BUDGET_TAG` changes the bucket. `CODEX_TIMEOUT_MIN` defaults to 45 minutes; `CODEX_DRY=1` prints the plan without a run (`.coord/tools/codex-run.sh`, `tier`, `attempt` and main loop).

The ledger and logs are under the main checkout's .coord/logs/, found through Git's common directory so worktree removal does not lose them. Last-message output goes under the invoking checkout's `.coord/reports/`. The successful role determines the output suffix, so a failover audit must use the DONE line rather than assume a director filename (`.coord/tools/codex-run.sh`, `MAIN`, `LEDGER`, `attempt`; `.claude/skills/next-task/SKILL.md`, step 2).

## Claude design reviewer

- **Kind/model:** Claude subagent, Opus; no effort is fixed. Tools: Read, Grep, Glob, Bash.
- **Started by:** reviewer uses the Agent tool with `subagent_type: design-reviewer` and exactly "Design review of <ID-slug>, latest round <round>." High requires it; Medium can add it.
- **Given:** card/round briefs, implementation report, gate report, Codex review, then diff summary and only necessary hunks.
- **Exact opening:** "You review one finished AntiNotepad task for architecture and design."
- **May/may not:** reviews responsibility, boundaries, coupling, fit and future cost. Bash is only for read-only git and `rg`; never edits, builds, launches or commits. It does not repeat Codex's logic review.
- **Leaves:** under 300 words, `Verdict: accept | accept-with-followup | rework`, proven findings and follow-up proposals.

Source: `.claude/agents/design-reviewer.md`; `.claude/skills/review-task/SKILL.md`, step 3.

## Scripts and hooks

These entries have **no model, reasoning effort or agent opening prompt**. A command supplies parameters, not a conversational payload. The table names the caller, input and result; the named script is the source for each row.

| File / kind | Started by | Given | Allowed work and result |
|---|---|---|---|
| `.coord/tools/codex-run.sh` / Bash launcher | Builder, auditor or reviewer with role, task and brief arguments | Brief file on stdin to Codex; environment overrides | Validates sandbox/name, routes models and failover; writes logs, last message and ledger; never commits |
| `tools/scan-delta.ps1` / PowerShell script | `/next-task` Findings: `pwsh -NoProfile -File tools/scan-delta.ps1 -Slug <slug>` | Research scan SHA and Where paths, dev history | Read-only delta, related cards, WHERE/NEAR labels and RESCAN decision; no files written |
| `tools/approve.ps1` / PowerShell script | Master at Approve: `pwsh -NoProfile -File tools/approve.ps1 -Task <ID-slug> -Prefix <prefix>` | Untracked card, STATUS, worktrees; optional authorized `-NoWorktree` | Checks cap/overlap and checkout; creates branch/worktree and moves/commits planning files. `-DryRun` checks and prints without mutation |
| `tools/files-overlap.ps1` / PowerShell script | Approve calls `pwsh -NoProfile -File tools/files-overlap.ps1 -Task <ID-slug>` | Item Files and cards in other item checkouts | Reads and prints FAIL for shared non-hot paths; WARN only for hot paths |
| `tools/catch-up.ps1` / PowerShell script | Item's session before Check or Merge: `pwsh -NoProfile -File tools/catch-up.ps1 -Task <ID-slug>` | Item card/branch and local dev | Fetches origin, merges local dev, never pushes/rebases; prints CLEAN/UP TO DATE, RETEST, or leaves conflicts open |
| `tools/gate.ps1` / PowerShell script | Reviewer and master: `pwsh -NoProfile -File tools/gate.ps1 -Task <round>` | Card Files, checkout changes, product project | Builds/selftests in isolated task folders; seam, format, scope, lint and size checks; writes disposable gate report with `Head:`; no commit |
| `tools/check-platform-seam.ps1` / PowerShell script | Gate's Seam step | Product source | Checks native-platform boundary; prints violations and exit status |
| `tools/loop-lint.ps1` / PowerShell script | Gate when loop files changed, or `pwsh -NoProfile -File tools/loop-lint.ps1` | Live loop files and `tools/loop-lint.allow` | Read-only consistency, file/link/script-reference checks; PASS/FAIL output |
| `tools/merge-preflight.ps1` / PowerShell script | Master: `pwsh -NoProfile -File tools/merge-preflight.ps1 -Task <ID-slug>` | Committed branch card, checkout state, newest gate, dev ancestry | Read-only refusal reason or PASS; does not perform the merge |
| `tools/dev.ps1` / PowerShell launcher | Accepted tester: `pwsh -NoProfile -File tools/dev.ps1` | Worktree's source and disposable dev data | Builds and launches the separate dev copy, safely hands off the prior dev instance, prints paths; installed copy is excluded |
| `tools/roadmap.ps1` / PowerShell script and hook | `/roadmap`: `pwsh -NoProfile -File tools/roadmap.ps1 -Open`; PostToolUse and Stop: same script with `-Hook` | Main checkout STATUS Plan | Rebuilds local .coord/roadmap.html; `-Open` opens it; hook skips worktrees; no automatic publishing |
| `tools/loop-card.ps1` / UserPromptSubmit hook | `.claude/settings.json`: `pwsh -NoProfile -File "$CLAUDE_PROJECT_DIR/tools/loop-card.ps1"` | Hook stdin (discarded) and live main-checkout STATUS | Adds card reminder and in-flight list to Claude context; warns above 3 past Approve; changes no item files |

`tools/gate.ps1`, `Get-ChangedFiles`, excludes `.coord/` from Scope. Scope is strict on item branches but WARN on dev/base, `-Ci` or `-LooseScope`. `tools/merge-preflight.ps1` checks Request, Plan, Approve (or old Go), Check and Hand test content; it is not a separate natural-language audit of every phase. Both approval and merge checks permit specified STATUS changes, rather than requiring a completely empty working tree (named scripts' check blocks).

Hook wiring is explicit in `.claude/settings.json`: UserPromptSubmit has a 15-second timeout; PostToolUse matches Edit/Write/MultiEdit and Stop also rebuilds the roadmap, with 30-second timeouts. The status hook reminds; it does not enforce reply wording. Approval's cap is the blocking check (`tools/loop-card.ps1`; `tools/approve.ps1`).

## Sources

- `docs/loop.md` — Phases, Tiers, Rules, Sessions, Branches and triggers.
- `docs/agents.md` — launch, models, permissions and brief/report contract.
- `.claude/skills/next-task/SKILL.md` — steps 1–5.
- `.claude/skills/scan-backlog/SKILL.md` — snapshot and scout dispatch.
- `.claude/skills/review-task/SKILL.md` — gates, bounds, triage and handover.
- `.claude/skills/merge-task/SKILL.md` — master merge responsibilities.
- `.coord/templates/item.md`, `.coord/templates/findings.md`, `.coord/templates/review.md`, `.coord/templates/scan.md` — exact payload openings.
- `.codex/agents/lens-reviewer.toml`, `.agents/skills/lens-review/SKILL.md` — in-run reviewer and proof rule.
- `.claude/agents/design-reviewer.md` — design-review role and restrictions.
- `.coord/tools/codex-run.sh` — actual stdin launch, caps, failover and outputs.
- `.claude/settings.json`, `tools/loop-card.ps1`, `tools/roadmap.ps1` — hook wiring and outputs.
- `tools/approve.ps1`, `tools/files-overlap.ps1`, `tools/catch-up.ps1`, `tools/merge-preflight.ps1` — handoff checks.
- `tools/gate.ps1`, `tools/check-platform-seam.ps1`, `tools/loop-lint.ps1`, `tools/loop-lint.allow` — scripted validation.
- `tools/scan-delta.ps1`, `tools/dev.ps1`, `docs/dev-loop.md` — research delta and dev-copy boundary.
- `.coord/tasks/Loop2-loop-v3-1.md`, `.coord/reports/Loop2-loop-v3-1-rehearsal.md` — exception and limits of evidence.
