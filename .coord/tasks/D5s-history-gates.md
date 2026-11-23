# D5s-history-gates (read-only Codex scout)

Role: quick, read-and-grep scan, sandbox read-only. No edits, no network, no app, no ports.
Context: the owner will publish branches main and dev of this repo. Before that, scan the FULL history of both for sensitive content. Report only; the final message is the report.

## Task
1. Run `git log --all` is NOT wanted; scan only commits reachable from `main` and `dev` (`git rev-list main dev`). Skip archive/* tags.
2. If `gitleaks` or `trufflehog` exists on PATH, run it offline on that range. Otherwise use `git log -p` / `git grep` over every revision (e.g. `git grep -I -nE PATTERN $(git rev-list main dev)` in batches) for: API keys and tokens (ghp_, github_pat_, sk-, AKIA, xox, Bearer, private key blocks), passwords or connection strings, `.env`-like files, email addresses, phone numbers, IP addresses, absolute user paths (%USERPROFILE%, <repo root>), the Claude artifact URLs, private chat/session ids, and any large binary or screenshot that looks personal.
3. Also list files in history that are not in the current tree but look personal (screenshots of the desktop, exports, backups, chat transcripts).

## Report sections
1. Summary (5 lines). Verdict: any real secret found? yes/no.
2. Findings table: severity (secret / personal / cosmetic) | what | commit hash | path:line | still in current tip? | suggested targeted removal.
3. Flagged .coord files for the owner to skim (max 25): path, why flagged.
4. Tool used and its coverage limits (what this scan cannot prove).
5. Questions for the owner (max 3).
Mark every claim verified or guess. Done means: report in the final message; nothing modified.
