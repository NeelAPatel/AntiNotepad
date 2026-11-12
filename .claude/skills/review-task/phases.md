Read by /review-task for Check and failure handling.

### 6. Check: plan vs done (bounds check)
After the gates, the reviewer compares three things and writes `## Check` on the card:
- **Requested** (`## Request`), **approved** (`## Plan`, `## Approve`), **done** (the report's Change list, Behavior
  changes and Out-of-scope files, and `git diff --stat dev...HEAD`).
- Every difference gets one line: **within plan**, **justified** (why, and why the user does not need to decide), or
  **ask the user** (it changes behavior or scope the user did not approve). Ask-the-user items stop the item until answered.
- Comments and formatting in the changed files follow `docs/coding.md` (on touch); a gap is a rework line, not a pass.
- Side bugs noticed on the way go under "Noticed" as proposed follow-ups, never fixed in passing.
- A scope change from the user mid-item (A54 r3) is recorded on the card with the user's words and goes through Plan again
  for that round.

### Triage, lessons, scores
Strictness is cheap by design: scripts first (zero model tokens), model judgment only on exceptions.
- **Triage on every failure** (gate fail, review rework, hand-test fail), done by the Check session that already has the
  context, no extra agent: one cause, `code`, `test`, `plan`, `env`, `conflict` or `judgment`, routed by the table in
  `/review-task` step 4a. Only `code` and `test` go back to Codex; `plan` and `judgment` go to the user, who answers this chat.
- **Lessons:** each failure leaves one "Misses" line on the card; the director copies them into `.coord/lessons.md` at Merge
  with one Scores row per item (what each check found). Master-owned on dev, like STATUS.
- **Self-improvement, user-approved:** when the same cause and "should have caught" pair appears twice without a rule
  change, the director proposes an unnumbered `idea:` row naming the rule or bot file to change. Rules change only through
  an item the user approves.
- **Reviewer scores:** the Scores table shows whether each review earns its run. Empty reviews on items that later had
  misses mean that review's checklist needs a change (an `idea:` row), not more reviewers.
- **Cost:** a green item adds no model calls (format and the ledger are scripts); a failure adds a few lines in a
  session already open; the retro is one row written at Merge.

- A failed gate, a `rework` verdict or a user scope change goes back to Build as `<ID-slug>-r2`, `-r3` (STATUS `rework rN`),
  with the round's brief listing what changed and why, written by the item chat (it saw the failure).
  Counting: the first build is round 1 (no suffix), so two rework rounds (`-r2`, `-r3`) run without asking; a fourth
  round (`-r4`) needs the user's go. A round's gate and review run under the round's name (`-Task <ID-slug>-rN`): the gate allows the Files of the base card and every round's brief, and the
  reviews read that round's report plus the base report.

- Gate reports (`.coord/reports/*.gate.md`) are regenerated on every run and not committed; the result lines go into the
  card's `## Check`.
