# Loop5-reply-style: replies the user can read cold, with every decision explained

Item card (`docs/loop.md`), fast lane (side task, `docs/loop.md` "Rules").

Tier: Low · docs-tools (rules: `docs/loop.md`)
Branch: `docs/Loop5-reply-style` in `.claude/worktrees/docs/Loop5-reply-style` (created at Approve)

## Request
User, 2026-11-02, in Loop Hub 2, after reading Master 2 and Master 3 replies:
- "Couple things ive noticed: reading exhaustion; assumption that im keeping track when im not. What else are you able
  to see for possible improvements/efficiencies"
- "whats cheaper? a claude mod or adjusting output? Also i do prefer it when claude is using its different output
  capabilities. visualizations, table renders w/ consistent data, bullet point hierarchy, bold, underline, etc.
  Basically im noticing that often times i cant understand the output because its made internal decisions i wouldn't be
  aware of when sessions request my action"
- "Yes, open Loop5"

## Findings
- **Kind and why:** process; every session follows one reply rule, and that rule drives the problems the user sees.
- **Now:**
  - `docs/loop.md:109-131` puts the full card (6 lines plus the phase strip) on **every** reply about project work,
    whether or not anything changed.
  - `tools/loop-card.ps1:30` repeats that rule every turn, in every session.
  - No rule covers the body below the card. Reading Master 2 and Master 3 (2026-11-02) turned up:
    - internal detail the user doesn't need: hashes, `file:line`, warning codes, token counts, gate step names;
    - names used with no reminder of what they are ("your grouping work", "control-groups + fluent-study", "the two
      plugins", "numbered");
    - two decisions packed into one "Waiting on you";
    - full card replies to peer FYIs the user doesn't need;
    - decisions the session already took are never shown, so the user's "go" lacks context.
- **Where:** `docs/loop.md` "Status card", `docs/glossary.md:23`, `tools/loop-card.ps1:30` and
  `docs/wiki/Getting-Started.md:82`. The skills only say "opens with the status card" and follow the definition, so
  they stay as they are.
- **Mod or output rule:** the output rule is cheaper. It is text, not code. Shorter replies also cost fewer tokens.
  The roadmap portal already does a live panel's job (user chose this, 2026-11-02).
- **Risk:**
  - An output style replaces part of the system prompt. It needs `keep-coding-instructions: true`, or builders lose
    their coding defaults.
  - Too much compression could drop facts a builder or reviewer needs. Card files and reports keep full detail; only
    replies to the user change.
- **Source:** master scout (Loop Hub 2: transcripts of Master 2 and Master 3, last 25 messages each).

## Plan
- **After:**
  1. **A project output style** `.claude/output-styles/antinotepad.md`, set by `outputStyle` in
     `.claude/settings.json`, so every session in the repo gets it. Contents:
     - **Plain words first.** No hashes, paths, warning codes or counts unless the user asks or they change the
       decision.
     - **Glosses.** Each item, step or tool name gets a 3-6 word gloss the first time it appears in a reply.
     - **Formatting set.** Tables with fixed columns for status or comparisons, so the same data sits in the same
       place. A bullet hierarchy: main point, details nested. Bold on 2-3 key words. 🟢🟡🔴 markers (Markdown has no
       underline). Visuals (diagram or chart) for plans, overviews and before/after, not for routine replies.
     - **The decision block** whenever a session asks the user to act, with exactly one decision per reply:
       - **Your call:** the question
       - **Background:** one or two lines, no assumed memory
       - **Already decided without you:** a bullet list, each line saying who decided it and when
       - **Options:** a table of If you say | What happens | Undo?
       - Buttons in the next-step box.
     - **"Since you last looked"** line when the user returns after a gap or a peer-driven turn.
     - **Peer messages.** An FYI from another session needs no reply to the user unless it needs them; one line at most.
  2. **A light card** in `docs/loop.md`:
     - **Default:** one line, `>> ID-slug · ● phase · what changed`.
     - **Full card** (tier, phase strip, goal, now) only when the phase changes, at the first reply in a session, or
       with a decision block.
     - "Waiting on you" becomes the decision block, or "nothing".
  3. **Hook and docs follow the new card.** `tools/loop-card.ps1` says the same in one shorter line, and
     `docs/glossary.md` and `docs/wiki/Getting-Started.md` match the new card.
- **Expected time:** about 1 hour, request to merge.
- **Must not change:**
  - item cards, reports, briefs for Codex, STATUS format;
  - the hook's in-flight list, its cap warning and the roadmap auto-start;
  - the user's global `~/.claude/CLAUDE.md`.
- **Scope questions:** None. Sessions that are already running pick the style up when they restart, or the master
  sets it on them (`set_session_output_style`) after merge.
- **Test plan, code:**
  - `tools/loop-card.ps1` runs and prints the new line plus the in-flight list;
  - the gate's loop-lint passes, so file references stay consistent;
  - the output style file has valid frontmatter (`name`, `description`, `keep-coding-instructions: true`).
- **Test plan, UI (hand test):** the user reads replies from two sessions that start after the merge, one master and
  one builder. Pass means a reply that asks for action can be understood with no other context.
- **Gates:** `tools/gate.ps1 -Task Loop5-reply-style`: scope, loop-lint, format.
- **Needs from you:** the go at Approve, then the hand-test read.

## Files
- `.claude/output-styles/antinotepad.md`
- `.claude/settings.json`
- `docs/loop.md`
- `docs/glossary.md`
- `docs/wiki/Getting-Started.md`
- `tools/loop-card.ps1`

## Contracts
None.

## Approve
User, 2026-11-02: "Approve".

## For Codex
Not used: fast lane, a few doc lines and one hook string; a Claude builder sub-session writes them and says so in its
report.

## Check
Builder (Claude, fast lane, no Codex), 2026-11-02.
- **Gate:** PASS. Seam, Format (no C#), Scope (6 files, all allowed), LoopLint (39 files), Size, Cleanup.
- **Hook:** `tools/loop-card.ps1` prints the new ASCII reminder line, then the in-flight list; cap warning and roadmap start untouched.
- **Style file:** 57 lines; frontmatter has `name`, `description`, `keep-coding-instructions: true`.
- **Bounds check:** request, plan and done match. Only the six listed files changed; `outputStyle` was the only key added to settings.
- **Misses:** none. The style's `Status card` section points at `docs/loop.md` rather than repeating it.

## Hand test
After merge, by plan: the user reads a reply from a session started after the merge (one master, one builder).
