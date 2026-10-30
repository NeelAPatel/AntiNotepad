# Loop v2.2: absorbed into v3.1

**Status: ABSORBED into loop v3.1** (2026-10-30, `Loop2-loop-v3-1`). v2.2 was never activated; v3.1 adopted the selected
steps below. The remaining v2.2 files and `docs/loop-v2.2-plan.md` stay as historical records.

| v2.2 step | v3.1 result |
|---|---|
| Overlap check | 3.1a moved and generalized the check; 3.1b added it to Approve. |
| In-flight item limit | 3.1b enforces the existing cap. |
| Hand-test queue | One rule in 3.1d: one dev copy, oldest accepted item first. |
| Catch-up with dev | 3.1a moved the script; 3.1c added catch-up points and retesting. |
| Conflict triage | One row in 3.1d sends semantic clashes back to Plan. |
| Conflict Codex round | Dropped. |
| Rehearsal | 3.1i. |

| File | What it is |
|---|---|
| `docs/v2.2/README.md` | this page: status, contents, how to activate |
| `docs/v2.2/DISCUSSION.md` | summary of the v2.2 discussion: why, evidence, decisions, rejected options, open questions |
| `docs/v2.2/CHANGELOG.md` | structured change log: every change v2.2 makes, step by step, and its state (all "not applied") |
| `docs/v2.2/ACTIVATION.md` | the exact edits to apply per target file when v2.2 is triggered |
| `docs/loop-v2.2-plan.md` | the full plan (Now / Proposed / cost / touches) |
| `docs/wiki/Loop-v2.2-Plan.md` | plain-words version, published on the GitHub wiki |
| `tools/files-overlap.ps1` | the overlap check, moved here from `tools/v2.2/` in 3.1a and used by `tools/approve.ps1` |
| `tools/catch-up.ps1` | the catch-up script, moved here from `tools/v2.2/` in 3.1a and run by `/review-task` |

This page replaces activation instructions; see `docs/loop.md` for the live v3.1 rules. Other v2.2 files are retained as history.
