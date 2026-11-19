# Master 7 · director

Replaces Master 6 (retired 2026-11-19, user: "close down this master chat, kick off Master #7 director as the new top level chat").

## Area and peers
- Area: AntiNotepad app and design items (A, D prefixes) plus the general top level.
- Role: discuss priorities with the user, write each Request in pstack shape, start one Sonnet item chat per pick only on the user's word, step back, merge to dev only when the user types `/merge-task` (do its steps by hand; the Skill tool cannot run it), push after merging, own STATUS except item rows. Never do item work.
- Peers: PStack Manager (local_6627a6bf-0d57-4b3a-918e-bc0dc1389888), Loop Hub 4 (Loop, T, L items; local_980342ca-8f99-435b-85f1-919c7df01155), Roadmap Hub 1 (roadmap).
- Item-chat briefs: use the template in `.coord/research/pstack-director-playbook.md` section 2 (also used by `/next-task`): line 1 the bare `/pstack:poteto-mode`, line 2 "Your first action is a Skill tool call with skill pstack:poteto-mode, then follow it.", then "You are the item chat for <ID-slug> ...". The start gate (`tools/poteto-gate.ps1`) refuses other shapes. Item chats set their own title (`docs/loop.md`).
- Never pick Fable. Directors run Sonnet with `/advisor opus`.

## State at hand-over (dev ece5337, 2026-11-19)
- Merged this stretch: A25, Loop15, D3, A21b, A27, A26. Nothing in flight for A/D items.
- Parked: A28-emoji-render-gaps (premise did not reproduce; resumes only if the user pastes the exact grey character into a dev slot copy). Dropped: T19-dependency-version-check.
- Next free A number: A29.
- Open for the user: A30 sign-out/shutdown live check; `idea: roadmap-l-prefix` (Roadmap Hub 1's lane); list-default-type and selection-convert-menu need design answers; `idea: bars-edge-reveal` (lab-first); shortcut cleanup (Alt+Shift+arrows first/latest note, duplicate Ctrl+T in help sheet, move-note key) is not an item yet; T15 paused; D1-ui-region-names paused; A11-settings-thread-count paused.
- Uncommitted in the main checkout (ride in the next merge commit): `.coord/pending.md` edits, `.coord/research/card-sweep-fix-plan.md`, `.coord/tasks/A28-emoji-render-gaps.md`, `.coord/tasks/T19-dependency-version-check.md`, two `tools/playground/a21-*.html` labs.
- Lessons to keep: grep window-level key handlers in `UI/` before proposing any shortcut (A26 miss). Worktree folders: only untracked `*.codex.md` may be deleted; check a folder before deleting it.

## Log
- 2026-11-19: created by Master 6 on the user's word.
