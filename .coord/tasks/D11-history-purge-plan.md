# D11-history-purge-plan (read-only Codex scan; do not commit, do not edit files)

Role: director (Sol), read-only. Reason: careful reasoning over git history, no edits.
Sandbox: read-only. Your final message is the report (also lands in .coord/logs).

## Goal
Produce an exact, safe plan to remove a narrow set of personal files from ALL history of `main` and `dev`, with no squash and no change to commit count or to any tracked file at the branch tips. Claude will run the scratch test; you only plan.

## Facts (owner decisions)
- History is published as is (about 619 commits). No squash. Only a narrow path purge.
- Candidate paths: `.claude/atlas/` (4 JSON files, added around commit fc5e2ce), `docs/spikes/A/persistence-path.txt`, `docs/spikes/A/persistence-write-log.txt`, `.coord/handoff/A35-bar-insets/`.
- Guards (should never appear in any commit): `.openchamber/`, `.claude/settings.local.json`, `.coord/logs/`.
- Public refs afterwards: `main`, `dev`, tags `v0.7.0` and `v0.7.1`. Other tags and `archive/*` refs are NOT published, so they need no rewrite.
- Inputs: `.coord/research/public-prep/D5s-history-gates-report.md`, `HISTORY-MILESTONES.md`, git history of the repo (read with `git log`, `git rev-list`, `git ls-tree`; never write).

## Do
1. For each candidate path and guard, list every commit (hash, date, subject) that adds, modifies or deletes it, across `main` and `dev` only, and say whether it exists at either tip.
2. Scan `main` and `dev` history for other personal content the D5s report may have missed: names, home paths (`C:\Users\`, the owner's username), emails, machine names, `.claude/settings.local.json`, tokens. Report only hits outside the candidate paths, with commit and path; no secrets values, redact them.
3. Check whether `git filter-repo` is installed (`git filter-repo --version`, python `-m` forms). It is probably not. Compare options for this Windows machine: pip install of git-filter-repo into a scratch environment, versus `git filter-branch --index-filter`. Recommend one with exact commands.
4. Write the exact purge command sequence to run inside a FRESH scratch clone (`git clone --no-local <repo> <scratch>`), including: restricting to `main`, `dev` and the two tags, the path filter, and what to do with the author/committer metadata (unchanged).
5. Write the verification checklist: commit count before and after (same), tree hash at each tip before and after (must be identical if the purged paths are absent at the tips; if present at a tip, say so and what changes), `git log --all -- <path>` returns nothing, repo size before and after, `dotnet build` and `--selftest` of the scratch tip (give commands, do not run builds).
6. State the consequences: every commit hash changes from the first affected commit on; old hashes cited in docs (list the docs that cite hashes, if any) would break; say how many docs.

## Constraints
- Read-only. Never run a command that writes to the repo, `.git`, or any ref. Never push. Never touch the installed app or its data.
- Do not print file contents of personal files; describe them in one line each.

## Report format (final message)
Short sections: Paths and commits (table), Extra hits, Tooling recommendation, Command sequence, Verification checklist, Consequences, Open questions for the owner. Under 120 lines.
