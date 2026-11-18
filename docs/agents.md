# Codex agents

## Launch
**Rule: The chat that launches Codex owns that run: launch as a tracked background Bash call (`run_in_background: true`, bare command), end the turn, and resume on the exit notice; never detach, never ask the user to check.**
Headless. Run it as one Bash tool call with `run_in_background: true` and the bare command below; the completion notice
of that call wakes the item chat when Codex exits. Never add `&`, `nohup`, `( ... )` or `> /dev/null`: that detaches Codex
from the call, which then reports done in seconds, so nothing wakes you and the run goes unwatched:

```bash
.coord/tools/codex-run.sh <role> <ID-slug> .coord/tasks/<ID-slug>.md
```

## Pick the role by the job
Claude session roles (builder, reviewer, auditor) are config files in `.claude/agents/`; this table covers Codex roles only.
Every brief names its role with a one-line reason ("Role: quick, a two-line rename"). There is no default role, and
xhigh is never the default. When unsure, start with the cheaper role.

| Role | Model / effort | Use for |
|---|---|---|
| `quick` | gpt-6-luna medium | small, surgical changes: a rename, a guard, a few lines in known places, docs |
| `terra` | gpt-6.1-terra medium | normal features and fixes that need reasoning across a few files; faster output than Luna |
| `worker` | gpt-6-luna xhigh | hard work: unclear cause, shared state, timing, many files |
| `director` | gpt-6.1-sol medium (delegates to Luna subagents) | High-tier items that split into parallel parts |
| `reviewer` | gpt-6.1-sol `CODEX_REVIEW_EFFORT` (medium) | fresh lens review (`docs/loop.md`) |
| `backup` | gpt-5.6-luna xhigh | failover only |

- `CODEX_EFFORT=<low|medium|high|xhigh>` overrides one run's effort. `.coord/logs/usage.tsv` records each run's seconds;
  compare roles there before changing a default (user, 2026-10-30: "We'll have to test this").
- **No run caps.** Only Codex's own 5-hour and weekly usage limits stop runs (user, 2026-10-30). On a usage, rate-limit
  or capacity error the wrapper fails over: quick, worker, reviewer -> backup; terra, director -> worker. Other failures
  are not retried. `CODEX_TIMEOUT_MIN` (45) stops one run; `CODEX_DRY=1` prints the plan only.
- The wrapper adds the only working sandbox route on this machine (`-c 'windows.sandbox="unelevated"'` with
  `--sandbox`, never full access). Audits run `CODEX_SANDBOX=read-only`; builds and reviews keep the default
  `workspace-write`.
- Output: log `.coord/logs/<ID-slug>.<role>.log`, last message `.coord/logs/<ID-slug>.<role>.last.md` in the main
  checkout (ignored; not part of the item's commits). The brief asks for a short report at
  `.coord/reports/<ID-slug>.md`. Existing tracked `.codex.md` reports stay as history.

## What Codex can and cannot check
The sandbox cannot bind ports, open a browser, see the screen or launch AntiNotepad. **A brief's "Done means" lists only
checks Codex can run offline:** build, `--selftest`, `tools/gate.ps1 -Offline`, a script's `-Check`, file contents and
diffs. Live servers, ports, browser pages, the dev copy and anything on screen are checked by the Claude session after
the run (T1-roadmap-overview: briefs asked for live server probes; every run hit "could not bind port" and kept working
around it).
- **Stop on a repeated environment error.** If a run's log shows the same sandbox or environment failure twice, the
  Claude session stops the run and tells the user, instead of waiting it out.
- **Progress checks need evidence.** "Is it still running?" is answered with two samples about 30 seconds apart (process
  CPU time, log size and last write, the last commands, `git status`, the report file, repeats of the latest error), then
  progressing, stuck since when, or finished.

## Briefs
- One item card per item in `.coord/tasks/<ID-slug>.md`, from `.coord/templates/item.md`: the user's request, findings,
  the approved plan, the exact files in scope (`## Files`, read by `tools/gate.ps1`), contracts, the user's go, and a
  "For Codex" section (role and reason, constraints, "Done means", the fixed report sections). Codex reads the whole
  card, never edits it.
- Findings audits use `.coord/templates/findings.md`; rework rounds `.coord/templates/round.md` (`<ID-slug>-rN.md`),
  written by the item chat.
- Directors spawn the project role `lens_reviewer` (`.codex/agents/lens-reviewer.toml`) before reporting. Codex loads
  project `.codex/` only for trusted folders; a worktree outside the repo folder may need trusting once.
- Codex cannot write to `.git` in its sandbox: it never commits; the Claude session reviews and commits.
- Codex never launches or kills AntiNotepad; it builds into `build/<task>-check`, runs `--selftest` there, then deletes that folder.
- Builds need `AVALONIA_TELEMETRY_OPTOUT=1` inside the sandbox.

## Reporting to the user
When launching, tell the user in one line: working folder, brief file, log and report paths.
