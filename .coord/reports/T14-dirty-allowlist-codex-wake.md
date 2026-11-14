# T14-dirty-allowlist-codex-wake report

## Step a: dirty allowlist
- Regex in `tools/approve.ps1`, `catch-up.ps1`, `gate.ps1`, `merge-preflight.ps1` is now
  `^.. (\.coord/(STATUS|pending|lessons|status-archive)\.md|\.claude/settings\.json)$`.
- `docs/dev-loop.md` gained one line (Other tools) naming those files as allowed dirty.
- Test: the new regex matches ` M`/`??` lines for those four files and rejects ` M product/x.cs`. The approve dry run was not
  run against the real main checkout (it would touch the director's tree); the one-line regex swap is identical in all four scripts.

## Step b: why Codex runs were not followed
Evidence (item-chat transcripts, launch Bash calls):
| Item | Launch call | Result |
|---|---|---|
| A16 | `run_in_background: true` **and** `( bash codex-run.sh ... > /dev/null 2>&1 & )` | tool call ends in ~2 s (subshell detaches Codex); completion notice fires at once, not at Codex exit |
| A17 | foreground call, `( nohup bash codex-run.sh ... > /dev/null 2>&1 & )` | same: detached; chat ended its turn with "when Codex finishes I run Check"; only the user's "check on codex" woke it |
| A18 | `run_in_background: true`, bare `codex-run.sh terra ...` (also -r2) | correct form; the call stays open until exit |

Cause: `docs/agents.md` said "in the background; the process exit wakes the item chat" without saying how. Chats backgrounded
with shell `&`/`nohup`, which detaches Codex from the tool call, so no exit notice exists. Only the harness's own
`run_in_background` makes exit wake the chat. (`codex-run.sh` itself is fine: it runs in the foreground and exits with Codex's code.)

Fix applied (text only): `docs/agents.md` "Launch" now says: one Bash call, `run_in_background: true`, bare command; never
`&`, `nohup`, `( )` or `> /dev/null`. The script already writes its own log and ledger, so redirecting is unneeded.
Not verified live (would need a Codex run); A18's form is the evidence it works.

Update (user goal: the launching chat owns the run and resumes at exit without a user turn): one rule line added to `docs/agents.md` Launch, and the Codex launch bullets in `.claude/skills/next-task/SKILL.md` (build) and `review-task/SKILL.md` (Codex review) now say tracked background call, end the turn, resume on the exit notice. The audit launch in next-task already said background and no poll.
