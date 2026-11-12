## Sessions: master, sub-sessions, names (user, 2026-10-29)

- **The picture** (user, 2026-10-30): the user is the CEO; masters are directors, one per area (today three live
  directors: Master 4 for app and design, Loop Hub 2 for `Loop` and `T`, Roadmap Hub 1 for the roadmap; earlier masters
  are closed); sub-sessions are managers who hire Codex for one item or phase and let it go when done. It is a guide,
  not a wall: the user talks to either level directly. Testing an item or changing its scope goes to its sub-session;
  backlog and cross-item matters go to the master. Assume the master always finds out; it may give
  its view through the sub-session but does not override what the user asked there.
- **A master** owns its area's STATUS rows and the roadmap, runs Request to Approve and Merge, checks Files overlap
  before dispatching (`tools/files-overlap.ps1`), and starts sub-sessions. It never runs Codex, builds or hand-test
  launches itself; its own reading is the Low-tier scout.
- **Routing:** directors do not do item work themselves. They ask the user how to route it; the default is a
  Sonnet-or-lower sub-chat per change that runs Codex workers. Sonnet when the user will refine the ask in that chat
  (user, 2026-11-06: "use sonnet"); Haiku when the brief fully specifies the job, such as audits and tests, so the user
  has no need to "travel" with the delegation.
- **Step back after dispatch.** Once a director dispatches, it steps back. User: "Assume that when top level delegates,
  i am traveling WITH that delegation to work together with Sonnet etc. Otherwise expect that it finishes what its
  working on and updates you periodically."
- **Need-to-know updates, after the task, or ask the user.** No in-process updates. A director finishes its task first,
  then decides whether others need an update, or asks the user. User: "Not everything needs to be told to them as ill
  ask them to self update and catch up, unless something im working on really impacts heavily on their domain."
- **Live directors:** Master 4 (app and design, owns STATUS), Loop Hub 2 (`Loop` and `T`), Roadmap Hub 1 (roadmap).
  Earlier masters are closed.
- **Master notes:** each master keeps `.coord/directors/<slug>.md` (gitignored; slug from its name): its role, open
  threads, decisions waiting on the user, linked sub-sessions, and where it left off. It updates the file when a
  decision is made, not later. After a context compaction a `SessionStart` hook (`tools/director-notes.ps1`) prints only the active director's
  notes back into the chat, so the master resumes from its own notes rather than the summary alone.
- **Master name:** `<Role> <N> · LP<vX.Y.Z> · <slug>`, Role the chat's role (Master, Loop Hub, Roadmap Hub), N one
  higher than the last of its role (STATUS "Working rules" names the current one), vX.Y.Z the loop version above
  (written `LPv5.3.0` so it never reads as a `Loop` item ID), the slug the remaining work it was opened for. When a
  merged item bumps the loop version, the merging director updates STATUS and renames all three live director chats to
  the new `LP<vX.Y.Z>` (user, 2026-11-06: "propogate the versions to yourself, master4 and roadmap"). Example: `Master 4 · LPv5.3.0 · design-tokens`.
- **Sub-session per step, well scoped:** each has one item, one step, one worktree, and a brief that names its done
  condition. At most two sub-sessions per item: one to investigate, one to build, test and wrap up (`A55` ran an audit chat,
  then a test chat); a third only on the user's word. The item card is what they share. A sub-session records the user's scope changes on its card in the
  user's words and gets a design approval from the user itself. Name: `<ID> · <step> · <slug>`, for example `A1 · build · list-commands`. Its **role** is shown in its
  status card.
  | Step | Role | When | Model | Does |
  |---|---|---|---|---|
  | `scan` | `scout` | rarely: a Low scout too big for the master | Haiku | read-only findings, no Codex |
  | `audit` | `auditor` | Findings for Medium, High | Haiku | runs the Codex findings audit, leaves it untracked until Approve, reports |
  | `build` | `builder` | after Approve | Sonnet medium | runs Codex, then Check (`/review-task`) and the hand-test handover |
  | `review` | `reviewer` | Check; a fresh one only when the build session stalled | Sonnet medium | `/review-task` |
  | `test` | `tester` | the hand test; a separate one only for a re-test the user asked for | Sonnet medium | launches the build, gives the UI checks, records the result |
  Each role's model, effort and tools are fixed in `.claude/agents/<role>.md` (builder, reviewer, auditor); this table says when to use them.
  A build session moving into Check or Hand test renames itself to that step (`A1 · review · list-commands`,
  `A1 · test · list-commands`) and changes its role, so the sidebar shows where it is; the master renames it if it
  cannot. The Codex role for each run is picked by the job (`docs/agents.md`).
- **Roadmap:** every master uses the same local server page `http://localhost:47811` (`/roadmap`), served live from STATUS
  and Git.
- **Handing over to a new master** (the chat grew long, or the loop version changed): no sub-session mid-step (or each one
  named in STATUS with its state), STATUS "Now" says where every item stands, dev clean and pushed. Retitle the old master
  `Master <N> · Loop v<V> · <slug> (closed)`, stop and archive it. The user opens the new master with:
  > You are `Master <N+1> · LP<vX.Y.Z> · <slug>` for AntiNotepad. Read `AGENTS.md`, the core `docs/loop.md` and the files it names, and
  > `.coord/STATUS.md`. Rename this chat to that
  > name, update STATUS "Working rules" with it, open `/roadmap`, then reply with the status card for the next plan item
  > and wait for my approval. Computer Use only when I say so.

## File inventory
| File | For |
|---|---|
| `AGENTS.md` (loaded by `CLAUDE.md` and by Codex) | project rules; points here |
| `docs/coding.md` | comments, formatting, file and folder rules |
| `docs/glossary.md` | one line per term |
| `.editorconfig` | formatting rules the gate's Format step checks |
| `.coord/templates/item.md` | the item card: Request, Findings, Plan, Approve, Codex brief, Check, Hand test |
| `.coord/templates/findings.md` | brief for the read-only Codex audit (Findings) |
| `.coord/templates/scan.md` | brief for one read-only Haiku backlog scout |
| `.coord/templates/review.md` | brief for the fresh Codex review |
| `.coord/templates/round.md` | brief for a rework round |
| `tools/gate.ps1` | build, selftest (LAYERS, CONTRACTS), seam, tokens, format, scope, loop-lint, size |
| `tools/check-design-tokens.ps1` | inventory and ratchet literal design values |
| `tools/loop-lint.ps1` | read-only consistency check of live loop files and their file references |
| `tools/loop-lint.allow` | path, rule and line-substring exceptions for historical mentions |
| `tools/catch-up.ps1` | rebase an item branch onto local dev, report conflicts and whether to repeat the hand test |
| `tools/files-overlap.ps1` | compare an item's Files with other item checkouts |
| `tools/scan-delta.ps1` | compare a backlog scan SHA and Where files with dev; flag rescans and cards to read |
| `tools/approve.ps1` | check the item cap and overlap, then create the branch and worktree |
| `tools/merge-preflight.ps1` | check the card, gate, branch and dev before Merge, and that every commit builds and passes `--selftest` |
| `product/AntiNotepad/SelfTest/Contracts.cs` | the `CONTRACTS` table of cross-feature promises |
| `.agents/skills/lens-review/SKILL.md` | Codex review checklist (logic, coupling, reliability, plan bounds, proof rule) |
| `.codex/agents/lens-reviewer.toml` | Codex role the director spawns before reporting |
| `.coord/tools/codex-run.sh` | Codex wrapper: roles by job, failover on usage limits, ledger (`docs/agents.md`) |
| `.claude/agents/design-reviewer.md` | Claude design reviewer (Opus, read-only) |
| `.claude/skills/next-task/SKILL.md` | `/next-task [ID-slug]`: Request, Findings, Plan, Approve, start Build |
| `.claude/skills/scan-backlog/SKILL.md` | `/scan-backlog [slug ...]`: snapshot scouts and proposed backlog order |
| `.claude/skills/review-task/SKILL.md` | `/review-task <ID-slug>`: Check (gates, bounds check, verdict), hand over |
| `.claude/skills/merge-task/SKILL.md` | `/merge-task <ID-slug>`: Merge |
| `tools/loop-card.ps1` | `UserPromptSubmit` hook: status-card reminder and the items in flight |
| `.coord/lessons.md` | misses (cause, caught by, should have caught) and per-item reviewer scores; master-owned |
| `.github/workflows/gate.yml` | CI gate, manual trigger |
| `tools/roadmap-data.ps1`, `tools/roadmap-serve.ps1`, `tools/roadmap/index.html`, `tools/roadmap.url`, `.claude/skills/roadmap/SKILL.md` | live roadmap from STATUS and Git (`/roadmap`); snapshot Artifact on request |
| `tools/director-notes.ps1` | `SessionStart` hook after compaction: prints only the active director's `.coord/directors/<slug>.md` (director chats only) |
| `.claude/settings.json` | hooks: status card plus roadmap auto-start (UserPromptSubmit), master notes after compaction (SessionStart); attribution off |
