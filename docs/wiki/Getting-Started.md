**In short**

Start in one master chat and describe the change you want in ordinary words.
The master gathers evidence and asks you to approve a plan before a builder starts.
For visible work, you try the accepted build in the dev copy, report the result, and tell the master to merge it
(`docs/loop.md`, "Triggers in practice").

# Getting started

## What you need

Use the Claude desktop app with this repository available: the loop's question cards, linked sub-sessions, and next-step
box are Claude app features (`docs/loop.md`, "Widgets" and "Sessions"; `docs/dev-loop.md`, "Install, upgrade and uninstall").
The files show these command-line tools:

| Tool | Evidence of its use |
|---|---|
| Codex CLI | `.coord/tools/codex-run.sh`, `attempt()`, calls `codex exec` |
| Git | `tools/approve.ps1`, `tools/catch-up.ps1`, and `tools/merge-preflight.ps1` |
| PowerShell 7 (`pwsh`) | `docs/loop.md`, "Triggers in practice," runs the gate with `pwsh` |
| .NET 10 SDK | `product/AntiNotepad/AntiNotepad.csproj` targets .NET 10; `.github/workflows/gate.yml` installs SDK 10.0.x |
| Bash and its wrapper utilities | `.coord/tools/codex-run.sh` uses Bash, `timeout`, `awk`, and `grep` |

These are the tools the repository uses, not an installer checklist or a claim that this wiki verified your machine.

## Open the master chat

Keep one master at a time. When replacing one, finish or name its active sub-sessions in STATUS, make dev clean and
pushed, and close/archive the old master first (`docs/loop.md`, "Sessions").
Use the handover prompt from that section, replacing its name placeholders:

> You are `Master <N+1> · Loop v<V> · <slug>` for AntiNotepad. Read `AGENTS.md`, `docs/loop.md` (all of it, it is the
> law for how you run items and start Claude and Codex sub-sessions) and `.coord/STATUS.md`. Rename this chat to that
> name, update STATUS "Working rules" with it, open `/roadmap`, then reply with the status card for the next plan item
> and wait for my approval. Computer Use only when I say so.

The current master name is recorded in `.coord/STATUS.md`, "Working rules right now"; the naming pattern and version
rule are in `docs/loop.md`, "Sessions."

## Your first item

1. **Say what you want.** Describe what happens now and what you want instead. You can also type `/next-task` or say
   "next." The master copies your request into an item card. An unnumbered idea gets a number only after you confirm
   its scope (`.claude/skills/next-task/SKILL.md`, step 1).
2. **Answer scope questions.** The master checks what dev does today and explains the evidence. A bug needs a proven
   reproduction; an unclear cause raises the tier, which determines the checks. If the request already matches dev,
   the master stops to ask (`.claude/skills/next-task/SKILL.md`, step 2).
3. **Approve, change, or drop.** Read Now → After, the Files list, the code/UI test plan, and what each gate catches.
   Your answer is recorded verbatim. Approval runs a preflight before creating the branch and worktree
   (`.claude/skills/next-task/SKILL.md`, steps 3–4; `tools/approve.ps1`).
4. **Wait for Build and Check.** The master starts a Sonnet medium builder sub-chat. It launches Codex, then runs the
   gate and required reviews. Rework is handled there; scope decisions come back to you
   (`.claude/skills/next-task/SKILL.md`, step 5; `.claude/skills/review-task/SKILL.md`, steps 1–4a).
5. **Hand test in the dev copy.** After acceptance, the tester launches the worktree's dev build, verifies its EXE,
   DATA, and log paths, and tells you the badge and checks to expect. Try those checks and report what passed or failed.
   Your words go on the card. Visible items need this; High tier always does
   (`.claude/skills/review-task/SKILL.md`, step 5; `docs/loop.md`, "Tiers").
6. **Say "merge it" to the master.** It preflights, squashes the item onto dev, gates again, records lessons, commits,
   pushes, and cleans up. A preflight failure stops that route for resolution
   (`.claude/skills/merge-task/SKILL.md`, steps 1–8).

Do not use your installed copy for these checks. The installed copy contains real data; the dev copy has a distinct
instance and initially disposable data (`docs/dev-loop.md`, "Installed and dev copies").

## Commands you can type in chat

| Command | What it does | Instruction file |
|---|---|---|
| `/next-task [ID-slug]` | Request through Approve, then starts Build | `.claude/skills/next-task/SKILL.md` |
| `/scan-backlog [slug ...]` | Snapshot scouts for ideas, then proposes an order for your choice | `.claude/skills/scan-backlog/SKILL.md` |
| `/review-task <ID-slug>` | Check, bounds review, failure routing, hand-test handover | `.claude/skills/review-task/SKILL.md` |
| `/merge-task <ID-slug>` | Merge an accepted and appropriately hand-tested item | `.claude/skills/merge-task/SKILL.md` |
| `/roadmap` | Opens the live project overview at http://localhost:47811 ([Roadmap Portal](Roadmap-Portal)) | `.claude/skills/roadmap/SKILL.md` |

Normally the builder runs `/review-task` automatically after Codex exits. You do not have to launch Codex by hand,
commit the branch, or interpret and repair raw gate output yourself: the builder/reviewer and master own those jobs.
They ask you when the cause is a plan or judgment decision (`docs/loop.md`, "Triggers in practice";
`.claude/skills/review-task/SKILL.md`, step 4a; `.claude/skills/merge-task/SKILL.md`).

## What you see

Each substantive item reply starts with a status card showing tier, goal, current phase, and what is waiting on you.
Question cards carry decisions; the next-step box closes replies that did work. A sidebar sub-chat changes from build
to review to test as the same session moves along. A quiet relay with no change may be just one line
(`docs/loop.md`, "Status card," "Widgets," and "Sessions").

| Chat-and-respond habit | The loop's replacement |
|---|---|
| Keep agreement in chat history | An item card holds Request, Plan, Approve, and results |
| Ask for code immediately | Findings and approval precede Build |
| Ask whether it seems finished | Planned gates and a bounds check precede acceptance |
| Try whichever app is open | Tester identifies the dev build and gives exact checks |
| Ask the same chat to do every job | Named sub-chats and Codex runs get bounded payloads |

These replacements are the responsibilities in `docs/loop.md`, "Phases," "Check," and "Sessions";
they do not guarantee that a check catches every defect.

## Sources

- `docs/loop.md`: Phases; Tiers; Status card; Widgets; Sessions; Triggers in practice.
- `.claude/skills/next-task/SKILL.md`, `.claude/skills/review-task/SKILL.md`, `.claude/skills/merge-task/SKILL.md`.
- `.claude/skills/scan-backlog/SKILL.md`, `.claude/skills/roadmap/SKILL.md`.
- `docs/dev-loop.md`: Installed and dev copies; Dev copy.
- `.coord/STATUS.md`: current master; `tools/approve.ps1`: approval preflight.
- `.coord/tools/codex-run.sh`, `tools/gate.ps1`, `tools/catch-up.ps1`, `tools/merge-preflight.ps1`: tools used.
- `product/AntiNotepad/AntiNotepad.csproj`, `.github/workflows/gate.yml`: .NET target and CI SDK.
