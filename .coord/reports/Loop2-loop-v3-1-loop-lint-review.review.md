# Loop2-loop-v3-1-loop-lint-review lens review

## Findings (proven)
1. [major] tools/loop-lint.ps1:65 - Valid relative Markdown links with escaped punctuation fail `missing-path`, blocking the gate on valid documentation. For example, `[x](docs/agents\.md)` in AGENTS.md resolves to the existing docs/agents.md, and `[x](docs/dev\-loop.md)` resolves to docs/dev-loop.md. The destination regex at line 61 accepts these escapes, but line 65 removes backslashes only before backslash, parentheses and space. Consequently line 67 tests a different filesystem path and line 72 reports a problem. An in-memory probe of the script's rule body, with only file enumeration and ReadAllLines inputs substituted, reported both as missing and exited 1; the unescaped equivalents passed. Smallest fix: decode Markdown backslash escapes for ASCII punctuation before resolving the destination, preserving the subsequent URI decoding; check these two existing-file examples alongside the existing parentheses cases.
2. [minor] tools/loop-lint.ps1:17 - A file supplied as `-Root` produces a false success instead of an input error. Directly running `pwsh -NoProfile -File tools/loop-lint.ps1 -Root tools/loop-lint.ps1` returned exit 0 and `LOOP-LINT: PASS (0 files)`. Resolve-Path accepts a file; enumeration at lines 24-25 silently discards the resulting errors, so the success path at lines 76-77 runs without checking anything. Smallest fix: require Root to be a filesystem directory before enumeration and throw through the existing exit-2 handler otherwise.

## Behavior changes check
Report's list vs the diff: matches the intended additions, with the Markdown resolution and invalid-Root exceptions above. The normal checkout returns `LOOP-LINT: PASS (37 files)`, exit 0.

The only new caller is gate.ps1's LoopLint block. It maps lint exit 0 to PASS and any other exit to FAIL, catches invocation failures, retains the first 40 output lines and runs immediately after Scope. There are no changed existing signatures or product callers, persistent state, deferred work, resource ownership or layering changes.

## Scope and plan check
In plan. The scoped tracked diff changes only docs/loop.md and the new LoopLint block in tools/gate.ps1; the untracked implementation files are tools/loop-lint.ps1 and tools/loop-lint.allow. Nothing else in gate.ps1 changed.

The one-line branch-example correction lists the supported prefixes and preserves the example's meaning. The other documentation edits are the requested gate-menu and Files-table additions. No history sentence was edited. All six allow entries have reason comments and identify actual historical attribution or the dormant proposal: docs/loop.md's history paragraph and tools/dev.sh's implementation attribution. The card's own Files edit and unrelated existing checkout changes were excluded from this review. No phases, roles, tiers, session names, status card or product files changed.

The live inventory patterns match the brief and enumerate 37 files here. They do not recurse into docs/v2.2, docs/wiki, loop-v2.2-plan, tasks, reports, STATUS or lessons. Gate matching covers the same live path patterns, including deleted paths. Scope's existing Get-ChangedFiles removes all .coord paths (gate.ps1:231); the new step deliberately inherits that exclusion, as required by the brief's same-set condition and disclosed by the implementer.

## Unproven risks
- None beyond the proven findings.

## Checks run
- Read the item card, step brief, implementer report, supplied gate report, review skill, coding rules, surrounding changed code and gate caller/helper paths. Reviewed every scoped hunk and both untracked implementation files.
- Ran the actual checkout lint: `LOOP-LINT: PASS (37 files)`, exit 0. Ran the actual invalid-Root case: `LOOP-LINT: PASS (0 files)`, exit 0 (finding 2).
- Read-only in-memory rule probes substituted inventory and text inputs while retaining the script's matching, exception, reporting and exit logic. One planted line for each of stale-path, stale-phase, stale-id, stale-version, feat-only and backticked missing-path yielded the six correct diagnostics and exit 1. Historical Go exemptions, alternative prefixes and loop v3.1 passed with exit 0.
- Allow probes: exact path/rule/substring silenced only its matching file; identical content in another live file remained a hit; different rule casing did not suppress. Missing-field and empty-field entries returned exit 2. No fixture files were written.
- Markdown probes: absent destinations with double-quoted, single-quoted and parenthesized titles failed with exit 1; existing destination plus anchor, external URL, anchor-only link and empty destination passed. Escaped punctuation in existing destinations failed with exit 1 (finding 1).
- Executed the actual gate LoopLint block extracted in memory, stubbing only process capture/output helpers and Add-Step: product-only and excluded-path changes SKIP; CI with no changes runs; a deleted live skill with exit 1 and AGENTS.md with exit 2 FAIL; changed docs/loop.md with exit 0 PASS. Each running case retained exactly 40 of 50 output lines. Checked the unchanged changed-file collector includes tracked, staged, untracked and deleted paths before its existing .coord exclusion.
- `git diff --check -- tools docs .claude .coord/templates .agents .codex AGENTS.md`: passed. The supplied full gate report records Build, Selftest, Seam, Scope and LoopLint PASS; no full gate/build was rerun during this scripts/docs-only read-only review.
- Only this requested report was written. No branch changes, commits, application launches, STATUS edits or product edits.

VERDICT: rework
