# My Claude Code hooks

I use these hooks to keep my development loop visible and guard a few actions. They are optional for contributors. They run only when `ANTINOTEPAD_LOOP` is exactly `1`; otherwise each exits successfully without output or side effects. The two script self-tests work without the switch.

## Turn my loop on

- **Where:** your user-level Claude settings file, `%USERPROFILE%\.claude\settings.json` on Windows (`~/.claude/settings.json` elsewhere). It is not part of this repo.
  - *User-level means it applies to every project you open. Other projects ignore this variable.*
- **What to add:** a top-level `env` block. If you already have one, add the line inside it.

```json
{
  "env": {
    "ANTINOTEPAD_LOOP": "1"
  }
}
```

- **Restart Claude Code** after the edit.
  - *Settings load at startup, so running chats keep the old value.*
- **Just for one shell instead:**

```powershell
$env:ANTINOTEPAD_LOOP = '1'
```

- **Check it worked:** start a chat in this repo. On each prompt you should see the in-flight items from `.coord/STATUS.md`, and the status line should show `loop: ...`.
  - *If you see neither, the variable did not reach Claude Code; recheck the file location and restart.*

## What else my loop needs

The switch only turns on the hooks. To run the full loop the way I do, you also need these.

- **Tools installed:** Claude Code, the Codex CLI on your path, PowerShell 7 and the .NET 10 SDK.
  - *Codex does the building; Claude briefs it and checks the result.*
- **The pstack plugin** for Claude Code, which provides `/pstack:poteto-mode`.
  - *Item chats open with it. Without it, skip my gate hook (it is not in this repo; it lives in my user settings).*
- **This repo's own pieces, already in place:** the `AntiNotepad` output style, the skills in `.claude/skills` and the agents in `.claude/agents`.

### The base prompts

Where I start things, in order. Details are in [the playbook](playbook.md) and [the loop](loop.md).

| Step | Type | Where | What happens |
|---|---|---|---|
| 1 | Read `AGENTS.md`, `.coord/STATUS.md` and `docs/loop.md` | Any new chat | Orients the chat; the hooks do the in-flight summary for you |
| 2 | `/next-task` (or describe the change) | A director chat in the main checkout | Writes the Request and starts one item chat |
| 3 | Answer "Approve?" | The item chat | Creates the branch and worktree, then builds with Codex |
| 4 | `/review-task <ID-slug>` | The item chat | Checks the result against the plan and hands you a hand test |
| 5 | Say pass or fail in your own words | The item chat | Records it on the card |
| 6 | `/merge-task <ID-slug>` | The director chat | Merges to `dev` and cleans up |
| Any time | `/roadmap` | Any chat | Opens the live overview at `http://localhost:47811` |

- An item chat's first prompt opens with `/pstack:poteto-mode` on line 1 and an explicit Skill-tool call on line 2. My gate hook checks for that; the exact template is in the `/next-task` skill (`.claude/skills/next-task/`).
- Never type into or stop an installed copy of the app while testing. Use `tools/dev.ps1` for a separate dev copy.

| Event | Script | Reads when on | Writes or starts when on | Prints when on | Switch off |
|---|---|---|---|---|---|
| PreToolUse (Bash) | `tools/guard-bash.ps1` | Hook JSON, command and cwd, `tools/guard.mode`, environment | Appends matching commands, cwd and verdicts (or errors) to `build/guard.log`; there is no automatic retention limit | Nothing normally; a block reason on stderr in block mode | Silent exit 0 |
| UserPromptSubmit | `tools/loop-card.ps1` | Git metadata, main checkout `.coord/STATUS.md`, roadmap health | May start the hidden roadmap server if it is not running | In-flight items and an item-cap warning when applicable | Silent exit 0; no server |
| UserPromptSubmit | `tools/title-hint.ps1` | Hook JSON, Git metadata, pending and STATUS rows, item cards, usage ledger and logs | Nothing | Title hints for matching items | Silent exit 0 |
| SessionStart (startup) | `tools/worktree-check.ps1` | Hook JSON and Git worktree, branch and base metadata | May append diagnostics to main checkout `.coord/logs/worktree-check.log` | Nothing | Silent exit 0 |
| SessionStart (compact) | `tools/director-notes.ps1` | Hook input, project and director environment, Git metadata, director notes | Nothing | Selected director notes in a main checkout, when available | Silent exit 0 |
| SessionStart (startup or resume) | `tools/session-where.ps1` | Project environment, Git branch, item card and STATUS | Nothing | Current item, phase, next action and card when available | Silent exit 0 |
| Status line | `tools/status-line.ps1` | Status JSON, environment, Git metadata, STATUS, optional user status-line script | May run Node with the user's status-line script | Its output and applicable loop context | Silent exit 0 |

The guard uses `log` mode unless `tools/guard.mode` says `block`. Logs can contain command text, so review and remove them as you see fit. The roadmap hook contacts a local health endpoint before it may start the server. Director notes printed on compaction enter the session context.

## Other project settings

The tracked Claude settings also select the `AntiNotepad` `outputStyle`, set empty commit and PR `attribution` and disable session URLs, set `worktree.baseRef` to `head`, and ask before edits to `docs/loop.md` and `AGENTS.md`. These settings apply independently of the hook switch.

For personal choices, add settings to `.claude/settings.local.json` (create it if needed); for example, `{"outputStyle":"default","worktree":{"baseRef":"dev"}}` selects a different style and worktree base locally. You can also set local `attribution` values and add local permission rules there. Permission rules from settings are combined, so adding an allow rule does not remove a project `ask` rule; the project prompts still require a decision. Consult Claude Code's settings documentation for supported values and precedence.

The `.codex` and `.agents` directories contain optional tooling and guidance; contributors do not need to use them or reproduce my loop.
