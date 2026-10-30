# Loop2-loop-v3-1-preflights-r2: labeled template placeholders

## Summary
Both preflights now reject empty sections and placeholder values following optional bullets and bold labels.
Clone probes and the offline gate passed; probe cleanup was blocked by automatic approval review.

## Change list
| File:lines | What changed | Plan line |
|---|---|---|
| `tools/approve.ps1:21-25,50-52` | Add section helper and use it for Approve | R2 placeholder rule |
| `tools/merge-preflight.ps1:17-21,42-44` | Identical helper, used for each required phase | R2 placeholder rule |

## Behavior changes
Empty sections or lines starting with a whole `<text>` token after optional whitespace, a `-`/`*` bullet and a
`**Label:**` field now fail. Later `<` characters and inline-code paths remain filled. The token must end before
whitespace or end of line; `<prefix>/x` is not a whole placeholder token. No other checks or state changed.

## Out-of-scope files
Only the two scripts changed for this round, plus this requested report and regenerated gate report.
Disposable clone, harness and local bundle were created under `build/`; cleanup remains blocked (see below).
Existing checkout changes, STATUS, item card and product source were left untouched.

## Plan deviations
Cleanup incomplete: automatic approval review rejected the guarded PowerShell `Remove-Item -LiteralPath ... -Recurse
-Force` action as `blocked by policy`. No deletion occurred. Remaining artifacts:
`build/Loop2-loop-v3-1-probe/`, `build/Loop2-loop-v3-1-probe.ps1`, `build/Loop2-loop-v3-1-probe.bundle`.
No results file was created. The clone was made from a local dev bundle, without network access or source Git writes.

## Contracts
None (docs/tools; focused disposable probes, no product changes).

## Lens review
Fresh `lens_reviewer`, using `.agents/skills/lens-review/SKILL.md`: no proven findings or unproven risks;
`VERDICT: accept`. Identical helpers parsed and passed 16 in-memory cases, including real template Plan, CRLF/LF,
labels, token boundaries and inline code. Report was not yet present during review.

## Checks run
- Throwaway clone `build/Loop2-loop-v3-1-probe`; local synthetic cards and gate report with the exact branch SHA:

| Case | Result |
|---|---|
| Real item template Plan verbatim; other required sections filled | Merge FAIL (Plan section missing or unfilled), exit 1 |
| Same card with Plan filled | Merge PASS (ready), exit 0 |
| Real template Approve `<The user's answer...>` | Approve FAIL (Approve section missing or unfilled), exit 1 |
| Approve `- **Answer:** <Pending approval>` | Approve FAIL (Approve section missing or unfilled), exit 1 |
| Filled Approve, actual NoWorktree creation and card commit in clone | Approve PASS (card committed), exit 0 |
| Dev advanced beyond item branch | Merge FAIL (behind dev), exit 1 |

- Actual helper extracted from each script: 13 cases each PASS (empty, whitespace, standalone/bulleted/labeled
  placeholders, placeholder with trailing words, filled text, later `<`, inline code and whole-token boundary).
  Harness initially shadowed `git` with its helper name; renamed that disposable helper and reran successfully.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline`: exit 0, `GATE: PASS`;
  build 0 warnings / 0 errors, `SELFTEST PASS`, CONTRACTS 13 rows (12 pass, 1 manual), seam/format/scope/size/cleanup PASS.
  Expected offline NuGet-audit WARN. Gate cleanup covers its own build/data folders, not the blocked clone cleanup.
- `git diff --check`: exit 0. Source branch remains `chore/Loop2-loop-v3-1`; no source commit, push, switch or merge.

## Hand checks for the user
None (not visible).
