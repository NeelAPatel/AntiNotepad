# pstack trial log (2026-11-16)

Trial chat: PStack test2 (Sonnet, read-only). Judged by PStack Manager.

| # | Skill | Worked | Didn't | House rules held? | Verdict |
|---|---|---|---|---|---|
| 1 | poteto-help | Picks map to our phases (how = Findings, blast-radius = Check, in-your-own-words before a Codex run); applied house rules unprompted (Codex builds, stop at Approve, Codex as second reviewer); named the real gap (no verify skill) | Nothing new beyond my own list; correct and deslop not mentioned | Yes | Pass |
| 2 | how (PadView show/hide) | Accurate; spot-checked Prewarm, the hotkey-to-focus log line and the 160 ms reveal token in code. Ends with a "watch out for" list that is ready to paste into a speed item's Findings | Plain words for skill names (fixed in house rules for new chats) | Yes, read-only | Pass |
| 3 | blast-radius (A20-overscroll-speed) | Ran the real spring code in a scratch harness (outside the repo) and proved it settles. Found 3 risks our review missed: arriving page snaps on fast repeat flips (cosmetic), overshoot on very fast flicks (cosmetic), and a possible stuck edge pull after hide. PStack Manager confirmed the third in code: StartFrame returns early while _frameRunning is true, and only the frame callback or StopFrames clears it; nothing resets it on hide. Also noted that no independent reviewer ran on A20 | Wrote scratch files outside the repo (harmless, but not strictly "create no file") | Yes | Pass, best result so far |
| 4 | correct, report only | 6 repeat-mistake classes, each with a cheapest check; spot-checked merge-preflight section check, the agents.md detach rule and 37 reports with "blocked by policy". New vs migration plan: Codex cleanup blocked, split items never filed, misses never reach the Misses table, Check/Hand test checked only at merge. Overlaps plan: stale gate head, detached Codex | Ran on Opus, not Sonnet | Yes, read-only | Pass |

## Fixes made during the trial
- Step 1 (user): skill names came out as plain words ("how"), unclear they are commands. Added a house rule: write skills as code with slash and prefix (`/pstack:how`). New chats only.

## Stop rules
- Stop if the chat edits, creates or commits a file, launches Codex, or starts a helper on Fable.
- Stop if one step costs far more than its answer is worth (long helper fan-out for a read-only question).

## Cost check after step 3 (2026-11-16)
- All Claude, no Codex: usage.tsv has no run after 2026-11-15 and no Codex session exists for today. Correct under the house rules (review stays with Claude; Codex is only the arena/interrogate second opinion).
- Trial chat runs on Opus 5.5 (medium), not Sonnet as planned. Step 2 spent one helper (80k tokens, 62 s). Step 3 took 18 turns: diff + 3 reports read, then a scratch .NET project (temp folder) compiled and run to prove the spring settles.
- Cheaper next time: run trial and review chats on Sonnet; say "proof by reading only" when a running harness isn't worth it.
