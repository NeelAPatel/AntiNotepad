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
- Start one item chat per pick (Sonnet). Its opening brief is the Request plus:
  `/pstack:poteto-mode` + "Follow the house rules and docs/loop.md. Stop at Approve. Relay the finished summary to <director> when the user asks."
- Then the director steps back.

## 3. In the item chat (user travels here)
| Phase | Who digs | User's part | Useful skills |
|---|---|---|---|
| Findings | Codex scout once the scout role exists; until then Claude | Read the summary, not the trace | `/pstack:how`, `/pstack:why` |
| Plan | Claude | Discuss; ask for 2 shapes if it crosses a boundary | `/pstack:architect`, `/pstack:in-your-own-words` before approving |
| Lab (visible items) | Claude builds the lab page | Approve the lab | prototype step |
| Approve | n/a | Say approve, change or drop | the only "go" signal |
| Build | Codex | Wait | n/a |
| Check | Codex runs blast-radius, review and deslop; Claude judges | Read the verdict | `/pstack:blast-radius`, `deslop` |
| Hand test | Claude for live checks | Test in the dev copy | n/a |

- Hopping between A, B and C: in each chat, `/pstack:ketchup` gives the catch-up before you continue.
- Steering mid-task: principle names ("apply prove it works", "subtract before you add"), or "new task" to switch playbooks.

## 4. Return and merge (director chat)
- Item chat relays: what changed, proof (fail then pass), risks left, gate result.
- Director runs `/merge-task <ID-slug>` on the user's "merge it".

## Not yet in place
- Codex scout role and report template (Loop Hub 4 item). Until then, Findings and reviews run on Claude Sonnet.
- Verification skill that drives the dev copy (later item, after T15-ffmpeg-frame-capture-trial).
