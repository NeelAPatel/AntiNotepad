**In short**

A failure goes to the person or tool able to resolve it. The session already checking the item names the cause, so diagnosis needs no extra agent. The card keeps a lesson, and the master records it at Merge (`docs/loop.md`, “Triage, lessons, scores”).

# Triage and Lessons

## One cause, one route

Every gate failure, rework verdict, and failed hand check is triaged in the Check session. These routes come from `.claude/skills/review-task/SKILL.md`, step 4a.

| Cause | Meaning | Route |
|---|---|---|
| code | Implementation does not match Plan | Codex round brief with failing lines or proven findings |
| test | Selftest, contract, or probe is wrong | Codex round limited to test files |
| plan | Findings or Plan were wrong | Stop; master re-plans and asks for approval again |
| env | Tooling, cache, sandbox, or flaky run | Repeat gate once; then manager-owned one-line gate fix or tell master |
| conflict | Catch-up stopped on conflicts | Item session resolves on its branch, preserves both plans, commits merge, repeats gate |
| judgment | Only the user can decide | Ask that one question with the status card |

A conflict that changes an assumption of the approved Plan is a semantic clash: abort the catch-up merge and route as plan (`.claude/skills/review-task/SKILL.md`, step 4a). See [Parallel and Branches](Parallel-and-Branches).

## Keep rework bounded

Rework uses the same branch and a brief from `.coord/templates/round.md`. The gate uses the round name, such as `<ID-slug>-r2`, and combines Files lists from the base card and round briefs. Reviews read base and round reports. Only two rework rounds, -r2 and -r3, are allowed before -r4 goes to the user (`docs/loop.md`, “Rules”; `.claude/skills/review-task/SKILL.md`, steps 3–4a).

## Learn from misses

The card's Misses entry records where the failure was found, its cause, what caught it, what should have caught it earlier, and a one-line lesson. At Merge, the master copies it into `.coord/lessons.md` and adds a Scores row (`.claude/skills/merge-task/SKILL.md`, step 5; `.coord/lessons.md`, “Misses” and “Scores”).

| Recorded example | Lesson | Evidence |
|---|---|---|
| A53-split-divider-height | Check what dev already does before building | `.coord/lessons.md`, Misses, 2026-10-29 |
| A54-link-hover-click | Reproduce first; ask scope questions at Plan | `.coord/lessons.md`, Misses, 2026-10-29 |
| Loop2-loop-v3-1 | Review found seven proven problems across five steps | `.coord/lessons.md`, Scores, 2026-10-30; [worked example](Worked-Example-Loop2) |

When the same cause and “should have caught” pair occurs twice without a rule change, the master proposes an unnumbered idea naming the rule or checklist to change. It does not rewrite rules at Merge. The freeze was lifted; loop rules change after a repeated miss or at the user's request (`docs/loop.md`, “Rules” and “Triage, lessons, scores”).

Scores help assess whether reviews find useful problems. Empty reviews followed by escaped bugs point to a checklist change rather than automatically adding reviewers (`docs/loop.md`, “Triage, lessons, scores”).

## Sources

- `docs/loop.md` — Rules; Triage, lessons, scores.
- `.claude/skills/review-task/SKILL.md` — steps 3–5, conflict and semantic-clash routes.
- `.coord/templates/round.md` — bounded rework brief.
- `.claude/skills/merge-task/SKILL.md` — step 5, master-owned lessons.
- `.coord/lessons.md` — actual misses and scores.
