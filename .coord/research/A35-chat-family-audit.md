# A35-ux-pass chat family audit (read-only, 2026-11-21)

Scope: item chat `A35-ux-pass` and every chat under it. Method: app session list (incl. archived), raw transcripts in `C:\Users\Neel\.claude\projects\...` (mapped by first timestamp and content), `git branch -a`, worktrees, `.coord/logs/usage.tsv`, `docs/loop.md`, `merge-task/SKILL.md`. Times are UTC (the Codex ledger is UTC-5). **[C]** = confirmed in a transcript or file, **[G]** = my guess.

**Note from the user (2026-11-21):** A35 was designed as an umbrella item: too big for one chat, so its sub-items (A35a-g) merge into A35, and A35 merges to dev once. Read every "deviation" below with that in mind: merging children into `docs/A35-ux-pass` is the design. The real gap is that the loop has no umbrella-item concept: no parent row in STATUS or pending (so no title hint, no roadmap fold from `T18-roadmap-parent-rows`), no rule on who merges children into the umbrella, and no umbrella merge-to-dev step on the card. Fix 1 in section 8 should become "umbrella item = STATUS parent row plus child rows created at dispatch, children listed on the umbrella card".

## 1. Chats in the family

| Chat | Session / transcript | State | Model | Start to end | What it did |
|---|---|---|---|---|---|
| Master 7 (parent) | a54d0459 / 470b253b | live, idle | Sonnet | 11-20 20:22 to 11-21 08:58 | Wrote the A35 card (untracked), started the item chat, retry after a usage-limit death, took 3 backlog ideas |
| A35-ux-pass (item chat) | b2d0adc9 / 84337b7b | live, idle | Opus 5.5 (first turns Sonnet) | 01:33 to 17:16 | Stages 0-3, started 5 children (8 start calls), merged 4 of them into `docs/A35-ux-pass`; 2 compactions (03:44, 09:45) |
| A35a-data-safety | 57a521b3 / 4bbab206 | live, "Merged A35a ... done" | Opus (some Sonnet) | 03:48 to 07:44 | data-loss fixes; 2 build rounds, merged 07:43 |
| A35b-icon-registry | 7fbd9a80 / 130ab031 | live, "[Merged] ... Merge" | Opus | 03:49 to 07:47 | icon registry; 3 rounds, merged 07:39 |
| A35g-labels-tooltips | c94d767f / a3f83394 | live, idle, title never changed | Opus | 03:58 to 16:28 | labels and tooltips; r2; merged 16:28; 1 compaction (10:07) |
| A35d-spacing-tokens | 73528c35 / de0b5aee | live, idle, title never changed | Opus | 03:58 to 16:50 | spacing tokens; 25 Codex launches; merged 16:49; 1 compaction (08:52) |
| A35e/f targets+contrast | 7e964ef1 / fd2f2833 | live, running its first lab | Sonnet | 17:13 to 17:16 | newest child, 37 tool calls so far |
| Proposal cards task_8b0ecc9d, task_81da845a | none | dismissed by the item chat [C] | n/a | 16:50 to 17:13 | two stale A35e/f cards (wrong parent id; then "Sonnet version"); third card was started |
| Codex runs (43) | ledger rows, no chats | all exit=0 except 2 | terra 28, luna worker 7, sol reviewer 4, luna quick 4 | 11-20 22:12 to 11-21 12:19 (local) | scans, builds, labs, reviews; about 20,800 s wall time |
| Old "Era0 ... A35 bar insets" | 14692480 | archived | n/a | Oct 25 | an older, different A35 item; same number, easy to confuse |

No chat for **A35c**, **A35f** (folded into A35e/f) exists. No archived or deleted chat in the family was found: `list_sessions` with archived shows only the rows above, and no orphan transcript mentions A35a-g outside these seven files.

### Sub-items A35a-A35g: what became of each [C]
| Item | Branch / worktree | Fate |
|---|---|---|
| A35a | `fix/A35a-data-safety`, worktree present | merged into `docs/A35-ux-pass` (fa2b84c) 07:43; branch and worktree kept |
| A35b | `feat/A35b-icon-registry`, worktree present | merged (f1e714c) 07:39; kept |
| A35c design contract | none; no card, no chat | **never started**. The user said "move design contract at the end" (item row 2126); the item chat planned it last. It is the gate before merging to dev |
| A35d | `refactor/A35d-spacing-tokens` | merged (1191e0f) 16:49; kept |
| A35e | `fix/A35e-targets-keyboard` at the merge base | chat started 17:13, branch cut, no commits yet |
| A35f | none | folded into the A35e/f chat |
| A35g | `fix/A35g-labels-tooltips` | merged 16:28; kept |

Why the user cannot find them: STATUS (last updated 2026-11-16) says "In flight: none" and has no A35 row; the A35 card is **untracked** in the main checkout (`git status`: `?? .coord/tasks/A35-ux-pass.md`), and the roadmap reads only STATUS and pending. The five children do exist in the app list, but they sit as separate top-level rows, two of them still show their start title.

## 2. Complaint 1: titles stopped changing

**Finding [C]:** the item chat's last rename was 03:30:09 (`⁉️ A35-ux-pass ◆ Findings (stage 4 pick)`). It never renamed again in 13 more hours, through two compactions, four merges and six children. Title history (custom-title rows): stage 0, stage 0 gate, Codexing stage 1, Codexing stage 3, stage 4 pick, then nothing. The first compaction was at 03:44, 14 minutes later.

Children: A35a (10 renames) and A35b (8) kept their titles current. **A35g, A35d and A35e/f never called the rename tool at all (0 calls)**, so they still carry the start title (`A35g-labels-tooltips: labels, tooltips, hints`). Their briefs never mention titles [C: grep of the start prompts]; A35a and A35b renamed anyway [G: they read `docs/loop.md` carefully; why A35g/A35d did not is not shown].

**Evidence about the hook:**
- `tools/title-hint.ps1` output in every family chat is the same five lines, none about A35: `title-hint A11-settings-thread-count ... T15-ffmpeg-frame-capture-trial` [C, hook_success rows at 84337b7b rows 14, 108, 1982, 2252].
- The script builds hints only from rows in `.coord/STATUS.md` "Plan" and `.coord/pending.md`; **A35 is in neither** (git history of both files has no A35-ux-pass row) [C]. The second hook, `loop-card.ps1`, prints "In flight per STATUS: none." on every turn [C].
- `docs/loop.md:57` ties the title to the STATUS row: "set or clear their own title in the same step as their STATUS row". The item brief said the opposite: "Do not merge or edit `.coord/STATUS.md`; Master 7 merges", and the compaction summary repeated it ("Do not edit STATUS, don't touch pending.md") [C].
- The hook fires only on a user-role turn, and only 4 of about 45 user-role turns in the item chat carried it (the typed ones and one wake-up) [C count; why the rest skipped it is unknown].
- Compaction is **not** the main cause. The rename tool was still available (it is in the post-compaction tool list), the chat just never used it; and the same silence is in A35g/A35d before their compactions [C]. What compaction did lose is the habit: the summary keeps "title is set to X" as a past fact, not a standing duty [C, summaries at rows 1750 and 3144].
- The `[Merged]` rename does not apply to the item chat (A35 has not merged to dev). A35a ended as "Merged A35a-data-safety ◆ done", A35b as "[Merged] A35b-icon-registry ◆ Merge": the two use different forms [C].

**Most likely cause:** an item with no STATUS/pending row is invisible to the hook, so the only thing driving titles is the chat's own memory of a rule that lives in a 91-line doc. The family was started with "don't edit STATUS" in the brief, which removes the row the rule hangs on. [G for the ranking; the missing row and the hint output are C.]

**Fixes**
| Fix | Size |
|---|---|
| Brief template and `docs/loop.md`: an item chat gets a STATUS/pending row at start (the director adds it, one line) before the first turn; `tools/poteto-gate.ps1` refuses an item brief without it | S (1 doc line, 10 lines of gate) |
| `title-hint.ps1`: also accept an item whose card exists in `.coord/tasks/` or a worktree even without a row, and say "no row for <cwd item>; set title from card" | S-M (30 lines + selftest) |
| `title-hint.ps1`/Stop hook: when the current chat title (from the session file) differs from the computed title, say so every turn, not only on user turns | M |
| Child briefs: one line "set your title per docs/loop.md, rename when you merge" | XS |
| Compaction summary reminder: `director-notes.ps1` (SessionStart compact) also prints "rename rule: ..." | S |

## 3. Complaint 2: merging and thinking deviations

Rules used: `docs/loop.md:67` ("only a director merges to dev or moves main; the item chat suggests the merge"), `merge-task/SKILL.md` steps 0-9, `AGENTS.md`.

| # | Deviation | Evidence | Verdict |
|---|---|---|---|
| 1 | The item chat merged four children itself (4 x `git merge --no-ff` into `docs/A35-ux-pass`), from its own worktree, not the master checkout | rows 2426, 2631, 3225, 3375 | **User-authorized**: "treat THIS branch as a temporary dev so anything you kick off from here merges back here" (user, 03:47). Not written on the card: the card's Approve section still says stage 4 pending |
| 2 | `/merge-task` and `tools/merge-preflight.ps1` never ran; the gate was `gate.ps1 -Offline` in the background | no preflight call in any of the six transcripts; gate calls at rows 2445, 2644, 3303, 3426 | Preflight builds every commit; not done, so a broken middle commit would not be caught [G on risk] |
| 3 | No STATUS, archive or `lessons.md` edit on any merge; A35 has no STATUS row at all | STATUS "Updated 2026-11-16"; compaction summary "Do not edit STATUS" | Brief-caused (Master 7 forbade it), but nothing replaced it, so 6 live branches are invisible |
| 4 | Commit shape: step 0 (commit review) was done for A35a, A35b, A35g; A35d merged with 12 commits including `hand test`, `re-check after rebase`, `re-pin the design hash`; A35g carries `gate pass, paused for hand test`, `hand test pass, ready to merge` | `git log docs/A35-ux-pass` | Breaks "no status-only commits" / "tidy commits that each build" |
| 5 | Merge order changed mid-flight: A35a first, then A35b, then A35b first (05:23), four messages and a rebase for each | rows 2041, 2289-2290 | Cost, not a rule break |
| 6 | Worktrees and branches kept after merge | item message "keep the worktree and branch for now" | Reasonable (integration branch), but SKILL step 7 says remove |
| 7 | User fear "A -> B -> A silent overwrite" answered with a new tool `.coord/tools/a35-survival.py` (46 lines) committed on the item branch and imposed on every child | rows 2836-2941 | Good fix, correct size; it is a one-off outside `tools/`, not in the gate [G: worth promoting] |
| 8 | A35e/f proposed three times (wrong parent id in card 1; card 2 replaced for Sonnet); cards 1-2 were dismissed correctly | rows 3488-3560, dismiss at 3504, 3545 | One avoidable retry |
| 9 | The item brief waived "item chats never start other chats", turning it into a director of five children; `AGENTS.md`, memory (`codex-runs-in-subsessions`: max two chats per item) and the director playbook all say otherwise | brief ("waived for this item only"); user at 03:11 said "make subchats on your own accord" | User-authorized, but it created the three-level family behind complaints 1 and 4 |

On "thinking things through": the item chat's reasoning was careful on content (verified each merged tree equals the hand-tested tip, ran survival, read each of 12 flagged lines). The gaps were process, not analysis: no preflight, no tracking row, no card update recording the temp-dev override.

**Fixes (ranked in section 8):** `merge-task` gets a "merge into an integration branch" variant (preflight with `-Base <branch>`, commit review, no STATUS) | M. `tools/merge-preflight.ps1 -Base` | S if not present. Promote `a35-survival.py` to `tools/` and into the gate as an optional step | S. Card template: an "Overrides" line the item chat must write when the user changes the merge target | XS.

## 4. Complaint 3: playbooks mostly not used

| Chat | poteto-mode | docs/loop.md | docs/agents.md | docs/playbook.md | pstack-director-playbook | review/merge/next-task skill files | other pstack skills |
|---|---|---|---|---|---|---|---|
| item | first action 01:33 | read once 01:33 | 03:09 | **never** | **never** | merge-task/branches.md once, 05:23 | none |
| A35a | 03:48 | 03:49 | 03:55 | never | never | review-task SKILL, merge-task/branches.md | none |
| A35b | 03:49 | 03:49 | 03:49 | never | never | review-task SKILL, merge-task/branches.md | none |
| A35g | 03:58 | 03:59 | 07:50 | never | never | none | none |
| A35d | 03:58 | 03:59 | 04:07 | never | 04:08 (once) | none | none |
| A35e/f | 17:13 | not read | read | never | 17:19 | merge-task/branches.md, next-task/phases.md | none |

- `Skill` tool calls: exactly one per chat (poteto-mode). Nobody called `/pstack:ketchup` or any other pstack skill [C].
- **After compaction**: poteto-mode was **re-injected by the harness** (an `invoked_skills` attachment holds its full text, rows 1756 and 3150), so the house rule's "reload after compaction" happens automatically. What is lost is the docs, which are not re-read; the summaries restore only files the chat read (card, reports) [C].
- The item brief named `AGENTS.md`, `docs/loop.md`, three design docs and the A29 reports; it did not name `docs/playbook.md`, the director playbook or `loop-sessions.md` [C]. The children's briefs name only the card, reports and poteto-mode.
- The item chat did follow the Loop20 line in `docs/agents.md:39-41` in practice (terra for labs and scans), so its use of the *rule* is better than its reading record suggests.

**Cause [G]:** the loop is spread across five docs and two skills, and the only thing forced at start is poteto-mode (the PreToolUse brief gate). Nothing forces or reminds the others, and compaction summaries do not re-list them.

**Fixes:** (a) a short "start card" printed by the SessionStart (startup, resume, compact) hook: role, 8 rules, titles, merge, Codex split, with file names, instead of expecting reads | S-M. (b) extend `poteto-gate.ps1` brief template to name `docs/playbook.md` and the phase skill for the item's phase | XS. (c) `/pstack:ketchup` in the item brief at hand-over | XS.

## 5. Complaint 4: parent/child crosstalk

Counts [C, from `SendMessage` calls and cross-session receipts]:
- Item chat to children: **18** messages. Children to item: **10**. A35d to Master 7: 1. Master 7 to item: 1. Total about **30** in 15 hours.
- **19 wake-up events** (`side-sessions-event`) landed in the item chat, each a full turn on a ~200k-token context. The chat answered most of them with a "waiting" status; 14 `wrap_up` calls.

My classification of the 28 item-child messages:
| Class | Count | Examples |
|---|---|---|
| Needed | 12 | 3 "ready to merge" reports (A35a, A35g, A35d); 3 "merged, close your dev copy" confirmations; merge-order change (2); commit-review request (1); new survival rule (2); retry from Master 7 (1) |
| Avoidable | 16 | 3 acks ("Ack merge order", "Ack toast idea", "Ack welcome-note"); 6 "A35b/A35a landed, rebase" FYIs to chats that were not waiting (A35g and A35d each got two; one line at their own merge-prep would do); 4 idea relays child to item to Master 7 (toast countdown, welcome note, note-stats, 2 backlog items) that could go straight to `pending.md` or Master 7; 3 duplicates caused by the 03:55 vs 05:23 order change |

Rules broken [C]: memory `top-level-updates-need-to-know` ("no FYIs mid-task") and `delegate-and-step-back`. Nothing in `docs/loop.md` forbids ack messages; the wake-up cost is a harness feature.

**Cause:** a long-lived coordinator chat (item) that merges serially plus children that each ask "ready?" and wait. [G on the first: with branch-order dependency, some chatter was unavoidable.]

**Fixes:** (a) rule line: "no acks; no 'landed' FYIs; a child rebases when it is ready to merge and reports once" | XS. (b) one shared file `.coord/tasks/A35-board.md` (merge queue, order, what landed) children read instead of being messaged | S. (c) children write ideas straight to Master 7 or a shared ideas file | XS. (d) item chat stays in a `wrap_up`-free state while children run (no status reply per wake-up) | rule XS.

## 6. Complaint 5: Codex share

**Measured [C]**
- Codex runs for the family (ledger, 11-20 22:12 to 11-21 12:19): **43** (terra 28, luna worker 7, sol reviewer 4, luna quick 4), about **20,800 s** wall. Two luna lab runs for A35d failed (exit=1, 2,893 s and 2,891 s, run in parallel); nine terra `lab3` runs redid them.
- Product code: `git diff dev docs/A35-ux-pass -- product/` is **+1,354 / -387 lines**. Claude wrote **9 lines of product code** by Edit/Write across all six chats and ran 17 shell commands that touched product paths (sed/patch-style; mostly merge/rebase fixes) [C].
- Claude written lines, by Edit/Write: item 870 report lines + 266 card lines; A35a 121 card; A35b 90 report + 185 card; A35g 26 + 64; A35d 58 report + 493 card + 63 other; A35e/f 88 card. About 2,400 lines, none product.
- Claude cost: output tokens item 363k, A35d 291k, A35b 174k, Master 7 105k, A35a 88k, A35g 85k, A35e/f 46k (about 1.15M total); cache reads 99.8M (item) and 106M (A35d) each. **No Codex token figure exists** (the ledger has seconds only), so a true 50/50 token split cannot be computed [C].

**Reading:** by lines of product code Codex did about 99%. The user's "about 70% Claude" is real if measured by tokens or turns, and it comes from orchestration, not from authoring. Two chats (item, A35d) carry roughly 100M cache-read tokens each: every wake-up, every status reply and every Codex-result relay re-reads a huge context. [G on the exact split.]

| Work | Who did it | Fair? |
|---|---|---|
| Stage 0 web research (19 WebSearch), UX principle choice, merge arbitration, survival design, design-contract planning, hand-test pairing | Claude | Fair (reasoning, no network in Codex's sandbox) |
| Reading and condensing 4 Codex scans into the findings report (870 report lines) | Claude | **Should be Codex-assisted**: mechanical merge of four `s3a-d` reports; could be a `quick` run that writes the table, Claude reviews |
| Item cards: 753 lines across chats (card + snap list + build briefs) | Claude | Briefs are Claude's job by rule; the snap list and string table (A35d, A35g) are tables Codex could draft |
| Rebases, commit review, fixups, gate runs, background polls (Bash 107/71/102/57/160 per chat) | Claude | Mechanical; a `quick` Codex run (or a script) could do rebase plus survival plus gate as one command |
| Hand-test screenshots through the browser pane (A35b 13, A35d 7 batches) | Claude | Fair (needs the running app) |
| Labs: A35d pad and settings labs | Codex first (luna, failed twice), then terra | Matches Loop20, but luna failure cost 48 min of wall time; no fallback role in the playbook row |
| Icon contact sheet render | Codex terra scan, Claude rendered | Matches Loop20 |

**Loop20 rows vs reality [C]:** `docs/agents.md:39-41` ("lab HTML: terra new, quick variant; inventories over 5 files: quick read-only") was followed for the labs and the scans. It does not cover (a) merging scan reports into one findings table, (b) writing cards, snap lists and string tables, (c) routine git mechanics.

**Fixes:** extend the Loop20 rows with "report consolidation, tables, snap lists and string tables go to Codex `quick`; Claude reviews" | S. A `tools/step-handback.ps1` doing rebase, survival, gate and a one-line report so the child runs one command instead of 8-10 Bash calls | M. A usage line in the ledger reading Codex tokens, so the split can be measured | M [G on feasibility].

## 7. Other findings
- The item chat's first turn "died on a usage limit before doing anything" (Master 7's retry message, 01:35); it ran on Sonnet for six messages, then the user switched it to Opus by `/model` [C].
- STATUS and the roadmap are stale for this family: "Updated 2026-11-16", A35 absent, six live branches, one untracked card [C].
- The A35 card in the main checkout is untracked and a second copy lives in the worktree; the hook's `Get-CardPhase` could use either, but only for rows that exist [C].

## 8. Ranked fixes
| Rank | Fix | Complaints | Size | Why first |
|---|---|---|---|---|
| 1 | Item chat always gets a STATUS/pending row at start; gate refuses a brief without one; card gets an "Overrides" line | 1, 2, 3 | S | Root of the blind hook and the invisible A35a-c |
| 2 | Start card via SessionStart (startup, resume, compact): role, title rule, merge rule, playbook list | 1, 3 | S-M | Survives compaction; no reading needed |
| 3 | Title hint reads the card when no row exists; warns every turn when the real title differs | 1 | S-M | Covers children and wake-ups |
| 4 | "No acks, no landed-FYIs, one report per child" rule plus a shared board file | 4 | XS-S | Removes about half the messages and wake-ups |
| 5 | Integration-branch merge variant of `merge-task` (preflight `-Base`, commit review, no STATUS) | 2 | M | Today it is improvised |
| 6 | Loop20 extension: report consolidation, tables, string tables, git mechanics to Codex `quick` | 5 | S | Cuts Claude turns |
| 7 | `step-handback` script (rebase + survival + gate) | 2, 5 | M | Saves 8-10 calls per merge |
| 8 | Promote `a35-survival.py` to `tools/` and into the gate | 2 | S | Good idea, currently buried |
| 9 | Codex token column in the ledger | 5 | M | Makes the 50/50 goal measurable |

## 9. Unverified
- Why 3 of 5 children never renamed while A35a/A35b did.
- Why 41 of about 45 user-role turns in the item chat carried no title hook output.
- Claude-versus-Codex split by tokens (Codex tokens are not recorded).
- Whether other items that did rename (A37, T20) had STATUS rows at the time; I did not check their transcripts.
