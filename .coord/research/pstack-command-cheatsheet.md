# Command cheat sheet (pstack + AntiNotepad loop, 2026-11-16)

Top layer = director chats (Master 6, Loop Hub 4, Roadmap Hub 1). Item chat = one sub-chat per item.

## Top layer
| Command | When |
|---|---|
| `/advisor opus` | Once, right after the chat starts |
| `/roadmap` | Choosing what to work on |
| `/scan-backlog` | The idea list grew; rank the next batch |
| `/next-task` | Request phase: write the Request, start the item chat |
| `/pstack:sharpen` | Before dispatch, to tighten a vague Request |
| `/pstack:interrogate` | Torn between priorities or directions; second opinion is Codex |
| `/pstack:ketchup` | Back after a break or a pile of sub-chat notes |
| `/merge-task <ID-slug>` | After you say "merge it" |

## Item chat
| Command | Phase | When |
|---|---|---|
| `/pstack:poteto-mode` | all | Already in the opening brief; retype only if it acts plain after a compact |
| `/pstack:how` | Findings | How does X work now, where would a change go |
| `/pstack:why` | Findings | Why is it shaped this way; regression history |
| `/pstack:teach` | any | You want to understand an area yourself |
| `/pstack:architect` | Plan | Change crosses a function boundary (poteto-mode calls it itself) |
| `/pstack:in-your-own-words` | Plan | Right before you answer Approve |
| `/pstack:tdd` | Build | Bug with a cheap selftest path |
| `/pstack:show-me-your-work` | Build/Check | A long run; you want the evidence, not a claim |
| `/deslop` | before Check | Clean-up findings for Codex's next brief |
| `/review-task <ID-slug>` | Check | Build finished |
| `/pstack:blast-radius` | Check | Motion, timing, shared state or anything you don't trust |
| `/pstack:ketchup` | any | Hopping back in from another item |

## Either layer, occasionally
| Command | When |
|---|---|
| `/pstack:poteto-help` | "Which skill for X?" |
| `/pstack:correct` + "report only" | Every ~10 items (Loop Hub 4 dispatches it) |
| `/model sonnet` / `/model opus` | Opus for a hard discussion, back to Sonnet after |
| `/compact` | Chat is long and slowing; the item card keeps the state |

## Phrases, not commands
- "approve" / "change" / "drop": the only go signal.
- "Scope up: add X. Re-plan and show me." / "Scope down: drop X."
- "This is its own item. Write a one-line Request for Master 6."
- "one Codex run", "proof by reading only", "don't change any code yet".
- "apply prove it works", "subtract before you add", "new task".
- "pstack voice" / "house style".

## Don't use (for now)
- `/pstack:setup-pstack` (only to change models; house rules survive it).
- `/pstack:swarm`, `/pstack:arena` (only for a contested design, judged by Codex).
- `/pstack:create-verification-skill` (later item, dev copy only), `/pstack:create-formal-verification-skill`, `/pstack:statechart`.
- `/pstack:no-comments` (comments policy undecided), `/pstack:reflect`, `/pstack:automate-me`.
- `/pstack:make-bot-ui`, `/pstack:typescript-best-practices` (not our stack).
