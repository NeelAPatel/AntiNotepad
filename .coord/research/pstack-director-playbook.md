# Director playbook with pstack (Master 6, 2026-11-16)

Same loop, same roles: the user picks, the director dispatches and merges, one item chat per item does Findings to Hand test. pstack changes how each phase is prompted and who does the digging.

## Models
- Director: Sonnet, advisor Opus. Item chats: Sonnet (Haiku for obvious, repeatable asks). Fable only on the user's word.
- pstack helpers follow the chat's model. Switching a chat to Opus for a hard discussion is fine; switch back after.

## 1. Pick (director chat)
- `/roadmap`, then discuss priorities. The user picks A, B, C.
- For each pick, the director writes the Request in pstack prompt shape:
  - **Goal:** the outcome, one sentence, no "how".
  - **Done check:** pass or fail, observable ("glide runs once, no jump, 10 times in a row").
  - **Proof:** what evidence the item chat hands back (selftest output, frame sheet, log line).
  - **What we know:** repro, when it started, related items. No theory of the cause.
  - **Constraints:** tier, lab first if visible, dev copy only, one Codex run unless the plan says more.

## 2. Dispatch (director chat)
- Start one item chat per pick (Sonnet). Its `prompt` is exactly this template (the app shows it under `Task:`). Line 1 is the bare command, nothing else on it; line 2 is the explicit Skill-call line (tested 2026-11-19, `.coord/research/poteto-trigger-evidence.md`: command alone on line 1 loaded 10 of 10, inside a sentence refused 11 of 11):
  ```
  /pstack:poteto-mode
  Your first action is a Skill tool call with skill `pstack:poteto-mode`. If it is refused, reply with one line and a code block holding `/pstack:poteto-mode <ID-slug>: work the brief above, Findings then Plan, stop at Approve`, then stop.
  You are the item chat for <ID-slug> ... <the Request, "Follow the house rules and docs/loop.md. Stop at Approve. Relay the finished summary to <director> when the user asks.">
  ```
- Enforced by `tools/poteto-gate.ps1`: a PreToolUse hook on `start_session` refuses a prompt containing "You are the item chat" that breaks the template; a Stop hook blocks an item chat once if its first turn shows neither the skill launch nor a fenced command. The paste line in the template is the only fallback.
- Then the director steps back.

## 3. In the item chat (user travels here)
| Phase | Who digs | User's part | Useful skills |
|---|---|---|---|
| Findings | Claude for about 5 files or fewer; a read-only Codex `quick` run for bigger inventories (see below) | Read the summary, not the trace | `/pstack:how`, `/pstack:why` |
| Plan | Claude | Discuss; ask for 2 shapes if it crosses a boundary | `/pstack:architect`, `/pstack:in-your-own-words` before approving |
| Lab (visible items) | Claude briefs; Codex `quick` or `terra` writes the page; Claude serves and checks it (see below) | Approve the lab | prototype step |
| Approve | n/a | Say approve, change or drop | the only "go" signal |
| Build | Codex | Wait | n/a |
| Check | Codex runs blast-radius, review and deslop; Claude judges | Read the verdict | `/pstack:blast-radius`, `deslop` |
| Hand test | Claude for live checks | Test in the dev copy | n/a |

- Hopping between A, B and C: in each chat, `/pstack:ketchup` gives the catch-up before you continue.
- Steering mid-task: principle names ("apply prove it works", "subtract before you add"), or "new task" to switch playbooks.

## 4. Return and merge (director chat)
- Item chat relays: what changed, proof (fail then pass), risks left, gate result.
- Director runs `/merge-task <ID-slug>` on the user's "merge it".

### Labs and inventories go to Codex (Loop20-codex-labs-and-scout)
- **Lab page:** Claude writes a short lab brief in the item card. A new page or about 150+ lines goes to a Codex run (`quick` for a variant of an existing lab, `terra` for a new page) that writes the HTML in the item worktree under `tools/playground/`. A few small tweaks stay with Claude. Claude then serves it (`tools/playground-serve.ps1`), checks it in the browser pane and shows it to the user. The brief's "Done means" lists offline checks only (file exists, opens without script errors by reading it); never port, server or browser checks.
- **Inventory:** a read-and-grep sweep over more than about 5 files (every key handler, every use of a setting) goes to a Codex `quick` run with `CODEX_SANDBOX=read-only`. It reports at `.coord/reports/<ID-slug>.md` with three sections: answer, evidence (file:line), open questions. Claude judges the report. Reading about 5 files or fewer stays with Claude.
- **Keep with Claude:** inline `show_widget` visuals, serving and browser-pane checks, talk with the user, decisions, and card and `.coord/pending.md` edits.
- **Lab brief example** (A31-list-default-type style):
  ```
  Role: terra, a new lab page across a few states.
  Write tools/playground/a31-list-default-lab.html (one self-contained file, no network). Show the list-type default
  menu in three states: off, bullet, numbered. Match tokens in docs/decisions.md. Goal: the user can pick one option.
  Done means: the file exists, is under 400 lines, and has no external URLs. Do not serve or open it.
  Report: .coord/reports/A31-list-default-type.md, sections What I built, Options shown, Open questions.
  ```

## Not yet in place
- Verification skill that drives the dev copy (later item, after T15-ffmpeg-frame-capture-trial).
- Findings and reviews not covered above still run on Claude Sonnet.
