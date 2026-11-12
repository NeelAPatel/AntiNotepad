# Loop9-loop-slim: build report

Branch `chore/Loop9-loop-slim` (cut from dev 01b1b06). Every commit built and passed `tools/gate.ps1 -Task Loop9-loop-slim -Offline`. Nothing merged, no STATUS edit outside step d, no other chat started.

## Steps
| Step | Result | Commit | What |
|---|---|---|---|
| a-settings-scope | done | b58689e | `tools/approve.ps1` ignores `.claude/settings.json` drift; rule section in `docs/dev-loop.md` |
| b-ask-rules-trim | done | 3fc7eda | `ask` now only `docs/loop.md`, `AGENTS.md`, `start_session`; no force-push hints in `catch-up.ps1`, `merge-task`, `review-task` |
| c-notes-own-only | partial | 6f87ff4 | hook prints only the active director's notes, only in the main checkout; old notes NOT moved (see Blocked) |
| d-status-diet | partial | 8aafbe3 | "Now" cut to 8 entries; 28.8 KB to `.coord/status-archive.md`; **8 KB target not reached** (see below) |
| e-budgets-cards | done | e06c417 | template, lessons, three skills slimmed; lessons rows to `.coord/lessons-archive.md`; research briefs not moved (see Blocked) |
| f-budgets-loop-docs | partial, `docs/loop.md` blocked | 14e5e76, 6b8ba28 | `docs/decisions.md` locked-choices list; rationale block; slim loop.md written as a proposal file |
| g-budget-guard | done | d1f28e2 | `tools/loop-lint.ps1` WARNs (exit unchanged) over budget: STATUS, loop.md, lessons, current item card |

## Sizes (bytes)
| File | Before | After | Budget |
|---|---|---|---|
| `.coord/STATUS.md` | 82,610 | 54,009 | 8,192 (missed) |
| `.coord/lessons.md` | 11,313 | 2,969 | 3,072 |
| `.coord/templates/item.md` | 5,080 | 3,143 | ~3,000 |
| `.claude/skills/next-task/SKILL.md` | 7,167 | 6,572 | ~5,000 (missed) |
| `.claude/skills/review-task/SKILL.md` | 7,686 | 6,130 | ~5,000 (missed) |
| `.claude/skills/merge-task/SKILL.md` | 4,681 | 4,762 | ~5,000 |
| `docs/loop.md` | 16,384 | 16,384 (untouched) | 8,192; proposal 8,443 |
| `docs/decisions.md` | 11,321 | ~14,100 (list and rationale added) | none |

## Evidence
- Plan section of STATUS: md5 identical before and after (`208818896b17d38a912efebda0903f10`).
- `tools/roadmap-data.ps1` run against a scratch root holding the old and the new STATUS: 121 rows each, rows equal; "now" items 70 to 8. (Running it against the main checkout cannot compare: that STATUS has the director's newer uncommitted edits.)
- `tools/loop-card.ps1` reads STATUS from the main checkout, so it cannot see this branch; the in-flight list comes from the Plan table, which is byte-identical, so the list is unchanged.
- Hook check: `director-notes.ps1` prints 0 lines with a worktree project dir and the one newest notes file (about 5 KB, not 47 KB) with a main-checkout dir.
- Gate offline PASS after every commit; only WARNs are the new budget warnings (STATUS, loop.md, this card 5.9 KB).

## Blocked or skipped
- **`docs/loop.md` edit (step f):** it is on the `ask` list, so it would prompt and nobody could answer. Not edited. The slim version is `.coord/reports/Loop9-loop-slim-loop-md.proposed` (8.4 KB, has the "A sub-chat never starts a chat" line, one Loop version line, Sessions section dropped because `docs/loop-sessions.md` already holds it). To apply: copy it over `docs/loop.md` (director, one command), delete the proposal.
- **Director notes archive and research archive (steps c and e):** both folders are untracked in the main checkout, outside this worktree. Not touched. The hook change already stops old notes printing. To finish: move `master-2.md` and `master-3.md` into `.coord/directors/archive/`, and the Level 4 briefs in `.coord/research/` into `.coord/research/archive/`.
- **STATUS 8 KB target:** the Plan table alone is 42 KB and the card forbids changing its format or rows. Reaching 8 KB needs a follow-up: move done rows' long Scope text out of the Plan (changes roadmap rows, needs a roadmap-data check) and trim Backlog (4 KB).
- **Skill sizes:** next-task and review-task are at 6.6 and 6.1 KB, not 5 KB; the rest is behavior text I did not want to cut unattended.
- **Merge note:** `docs/dev-loop.md` and the merge-task skill got small edits; STATUS and the lessons file will conflict with the director's pending edits, as the card says (patch aside, merge, re-apply).

## Your own settings (suggestions, you apply)
In your user `settings.json`:
```json
{
  "modelSettings": {
    "claude-opus-5-5": { "autoCompactWindow": 200000 },
    "claude-sonnet-5-5": { "autoCompactWindow": 200000 }
  }
}
```
and remove the top-level `"advisorModel"` key.
