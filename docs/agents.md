# Codex agents

## Launch
Headless, in the background; the process exit wakes the manager:

```bash
.coord/tools/codex-run.sh <director|worker|backup> <ID-slug> .coord/tasks/<ID-slug>.md
```

| Tier | Model / effort | Run counter per day |
|---|---|---|
| director | gpt-6.1-sol medium (delegates to gpt-6-luna xhigh subagents) | 20 |
| worker | gpt-6-luna xhigh | 10 |
| backup | gpt-5.6-luna xhigh | 5 |
| reviewer | gpt-6.1-sol `CODEX_REVIEW_EFFORT` (medium); fresh lens review, see `docs/loop.md` | 10 |

- The wrapper adds the only working sandbox route on this machine (`-c 'windows.sandbox="unelevated"'` with
  `--sandbox`, never full access). Audits run `CODEX_SANDBOX=read-only`; builds and reviews keep the default
  `workspace-write`.
- Failover on a reached cap or a usage/rate-limit error: director -> worker -> backup -> stop; reviewer -> backup -> stop. Other failures are not retried.
- The per-day figures are the wrapper's run counter (`.coord/logs/usage.tsv`); Codex's real usage limits are its 5-hour and
  weekly limits. Override with `CODEX_MAX_DIRECTOR|WORKER|BACKUP`; `CODEX_TIMEOUT_MIN` (45) caps one run; `CODEX_DRY=1`
  prints the plan only.
- Output: log `.coord/logs/<ID-slug>.<role>.log`, last message `.coord/reports/<ID-slug>.<role>.codex.md`; the brief asks for a
  short report at `.coord/reports/<ID-slug>.md`.

## Briefs
- One item card per item in `.coord/tasks/<ID-slug>.md`, from `.coord/templates/item.md`: the user's request, findings,
  the approved plan, the exact files in scope (`## Files`, read by `tools/gate.ps1`), contracts, the user's go, and a
  "For Codex" section (role, constraints, "Done means", the fixed report sections). Codex reads the whole card, never edits it.
- Findings audits use `.coord/templates/findings.md`; rework rounds `.coord/templates/round.md` (`<ID-slug>-rN.md`).
- Directors spawn the project role `lens_reviewer` (`.codex/agents/lens-reviewer.toml`) before reporting. Codex loads
  project `.codex/` only for trusted folders; a worktree outside the repo folder may need trusting once.
- Codex cannot write to `.git` in its sandbox: it never commits; the manager reviews and commits.
- Codex cannot see the screen: the manager does the visual check in the dev copy.
- Codex never launches or kills AntiNotepad; it builds into `build/<task>-check`, runs `--selftest` there, then deletes that folder.
- Builds need `AVALONIA_TELEMETRY_OPTOUT=1` inside the sandbox.

## Reporting to the user
When launching, tell the user in one line: working folder, brief file, log and report paths.
