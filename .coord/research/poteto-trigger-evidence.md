# poteto-mode in started chats: evidence (PStack Manager, 2026-11-19)

`/pstack:poteto-mode` has `disable-model-invocation: true` (pstack 0.15.42, unchanged since install 2026-11-15).
Sweep of every started chat in this project whose opening brief names the command (transcripts, Claude Code 2.1.325):

| Where the brief put the command | Chats | Result |
|---|---|---|
| Alone on its own line, first line under `Task:` | 10 (A21, A22 and 8 more, 11-16 to 11-18) | Skill tool call succeeded ("Launching skill") in all 10 |
| Inside a sentence ("First run `/pstack:poteto-mode`", "Start with ...") | 11 (A25, A21b, T19, D3, A26 x2, A27, A28, Loop Hub 4 x2, discussion chat) | Refused in all 11: "cannot be used with Skill tool due to disable-model-invocation" |
| At line start but with text after it on the same line | 1 (gate test d978f6e8) | Chat never tried; said the skill was not in its list |

Reading: the Skill tool lets a user-only skill run when the opening message holds it as a bare slash-command line, the
way a typed command looks. This is observed behavior, not documented; a Claude Code update could change it.
Two things must both hold: the bare command line (the harness check) and the model choosing to call the skill (it is
hidden from the model's skill list, so the gate test's chat never tried).
Commit 2f334eb (gate) came from a flawed test: its brief had text after the command on the same line, and the model
never attempted the call, so the test showed nothing about the harness.
Not shown: whether "own line anywhere" works or only "first line under Task:"; require the first line (all 10 passes).

Brief template (the `prompt` field of `start_session`; the app shows it under `Task:`):
```
/pstack:poteto-mode
Your first action is a Skill tool call with skill `pstack:poteto-mode`. If it is refused, reply with one line and a
code block holding `/pstack:poteto-mode <ID-slug>: work the brief above, Findings then Plan, stop at Approve`, then stop.
You are the item chat for <ID-slug> ...
```

Proposed enforcement (Loop item for Loop Hub 4, hooks in user scope or settings.local.json so they fire in the director
chats):
1. Director side: PreToolUse hook on `mcp__ccd_session__start_session`. When `prompt` contains "You are the item chat",
   refuse unless its first line is exactly `/pstack:poteto-mode` and the explicit-call line follows.
2. Child side: Stop hook with a `stop_hook_active` guard. In an item chat's first turn, pass only if the transcript shows
   "Launching skill: pstack:poteto-mode" or a fenced block holding the command; otherwise block with the fallback text.
3. Proof before building: one throwaway read-only started chat with the template prints "Launching skill" (user's word
   needed to start it). PASSED 2026-11-19 (session 3316d994): "Launching skill: pstack:poteto-mode", full skill text
   injected, no refusal.
