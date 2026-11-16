# pstack setup record (2026-11-15)

pstack 0.15.42 (cursor/plugins, MIT) installed for Claude Code at **local scope** (this checkout's `.claude/settings.local.json`; not committed). Codex needs no install: it follows AGENTS.md and the repo docs. Update by hand: `claude plugin update pstack@cursor-plugins`; turn off between uses: `claude plugin disable pstack@cursor-plugins`. Cost: about 4.7k tokens always-on per session; `/pstack:poteto-mode` about 7.4k when invoked.

## User-scope files (outside the repo, load in every project)
| File | Owner | Holds |
|---|---|---|
| `~/.claude/rules/pstack-models.md` | setup-pstack (re-run overwrites it) | budget small; all 13 roles `inherit-parent` (helpers run on the chat's model) |
| `~/.claude/rules/pstack-house-rules.md` | user (setup never touches it) | overrides below |
| `~/.claude/skills/deslop/SKILL.md` | user | cursor-team-kit `deslop` adapted: diffs against the item's base branch; report-only where Codex implements |

## House rules (summary)
- Builder: where AGENTS.md names an implementer (here Codex), it writes all product code; pstack delegate/swarm/hillclimb/arena-for-code mean "brief and launch Codex". Claude plans, designs, reviews, writes prose and runs live-app checks.
- Repo merge and branch rules override pstack's Opening a PR, Babysit and Shipping (no PRs; items merge to dev with `--no-ff`).
- Tiny fixes (a few lines, one or two files, no shared timing or state) go to a Haiku subagent when cheaper than a Codex run; unsure means Codex.
- Second opinion (arena judge, second interrogate reviewer): Codex review pass, not a second Claude model.
- Fable: never for helpers or panels; only the user picks it.
- `defaults: 2` is a format marker, not a helper count; panels run one helper per entry (architect still at least two).
- Approval: stop at the item card's `## Approve`; "just do it" covers only read-only work and reversible work inside an approved plan.
- Failing repro and fix land in one commit (every commit passes checks); the report shows fail then pass.
- Writing: replies keep the AntiNotepad output style layout with pstack sentence rules; briefs, commits and docs use pstack rules. "pstack voice" switches to pure pstack; "house style" switches back.

## Checks done
- Dry run in a fresh chat: poteto-mode planned Codex builds, no PR, stopped at Approve, dev copy only. Pass.
- Fresh chat listed both rule files correctly. Pass.

## Next
- Trial skills read-only (poteto-help, how, blast-radius on A20-overscroll-speed, correct report-only), then one Low-tier item in poteto-mode.
- Adoption of loop changes: `pstack-migration-plan.md`, decisions in `pstack-agreements.md`.
- Later items: verification skill (dev copy only, after T15-ffmpeg-frame-capture-trial); trim `.claude/output-styles/antinotepad.md` for better adherence.

## Confirmed by the user (2026-11-16)
- Directors run on Sonnet with the advisor on Opus by default; sub-chats run on Sonnet or Haiku; Fable only on the user's word.
- The advisor is the only escalation path; pstack roles stay `inherit-parent`.
- Switching a chat to Opus mid-way breaks nothing; its helpers and new sub-chats inherit Opus, so switch back after.
- Offload split "Claude decides, Codex digs" adopted, target 50-60% of all work on Codex (read-only, reviews and tests included). House rules updated. Needs a Loop Hub 4 item: read-only Codex scout role, brief template carrying a pstack skill's text, short report format, usage measure.
- Trial results: `pstack-trial-log.md` (all 4 steps passed).
- Mirror copies with history: `.coord/research/pstack-user-files/` (refresh after any change to the live files).
