# pstack user files (mirror)

Read-only copies of the live files pstack obeys. The live files are in the user folder (`~/.claude/...`, same subpaths) and load in every chat; edit those, then refresh these copies and commit, so the history stays here.

- `rules/pstack-house-rules.md`: user overrides (setup never touches it)
- `rules/pstack-models.md`: setup-pstack output (a re-run overwrites it)
- `skills/deslop/SKILL.md`: deslop adapted from cursor-team-kit (MIT)

Restore: copy a file back to the same subpath under `~/.claude/`, then start a new chat.
