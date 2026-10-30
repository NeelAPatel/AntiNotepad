# Loop2-loop-v3-1-loop-lint

## Summary
Added a read-only live-loop consistency lint and a gate step immediately after Scope. The checkout passes lint and the offline gate; isolated clone probes cover all six rules and narrow exceptions.

## Change list
| Block | Change | Plan |
|---|---|---|
| tools/loop-lint.ps1:1-82 | Live file inventory, six rules, literal exceptions, exit 0/1/2 | After 1 |
| tools/loop-lint.allow:1-13 | Historical reference exceptions with reasons | After 2 |
| tools/gate.ps1:517-534 | LoopLint after Scope; same changed-file set; always in CI; first 40 output lines | After 3 |
| docs/loop.md:62 | Gate menu includes loop-lint | After 4 |
| docs/loop.md:224 | Branch example lists the other supported prefixes | After 2 |
| docs/loop.md:274-276 | Gate Files row and two lint Files rows | After 4 |

## Behavior changes
The new script reads only the named live files, reports path:line:rule diagnostics, returns 1 on unsuppressed problems and 2 on script errors. Exceptions match exact case-sensitive paths/rules and literal substrings. Old version numbers inside directory paths are handled by stale-path rather than stale-version. Relative Markdown file links resolve from their document; anchors and external links are ignored.

The gate now fails on nonzero lint exit when Scope's changed-file set contains a live-loop path, including deletions; CI always runs it. Otherwise it records SKIP with "no loop files changed". Existing Scope excludes .coord paths, and this step deliberately uses that same set.

## Out-of-scope files
None. The gate regenerated its normal .coord/reports/Loop2-loop-v3-1.gate.md artifact. Existing STATUS/card edits were left untouched.

## Plan deviations
Direct local git clone failed with Git sh.exe CreateFileMapping error 5 in the sandbox. Created a local HEAD bundle and cloned that into the exact requested build/Loop2-loop-v3-1-probe path instead, then overlaid the four working lint/gate/doc files. No branch, commit, merge, push or main-checkout git write was performed.

## Contracts
None (no product change).

## Lens review
Fresh lens_reviewer verdict: accept, no remaining findings or unproven risks. It proved two issues: Markdown links with single-quoted titles were skipped and valid filenames with parentheses failed; bracket characters in -Root caused a false PASS with zero files. Fixed link parsing to support balanced/escaped parentheses and quoted titles, and escaped the root before wildcard enumeration. Reviewer rechecked all cases successfully.

## Checks run
- Repo lint: LOOP-LINT: PASS (37 files), exit 0.
- Isolated clone: planted exactly one hit per rule; all six named correctly, exit 1. Exact allow entry removed only its hit; identical text in a different path still failed. Hits under docs/wiki and .coord/tasks were ignored. Relative link resolution, valid link with anchor, external link, historical Go exemption and malformed allow exit 2 also passed.
- Offline gate: Build exit 0, 0 warnings/errors; SELFTEST PASS; CONTRACTS 13 rows, 12 pass, 1 manual; seam PASS; Scope PASS; LoopLint PASS; GATE PASS. NuGet audit disabled for this build only.
- Extracted gate step probes with a stubbed process result: SKIP for product-only/wiki changes; PASS in CI with no changes; PASS for live skill changes; FAIL on exits 1/2, including a deleted live path; first 40 lines retained. All passed. These exercise the actual gate block without repeating build/selftest for each conditional case.
- Reviewer probes: missing link with single-quoted title gives missing-path/exit 1; existing filename with parentheses passes/exit 0; bracket-containing root reports a planted stale-phase/exit 1.
- Final full offline gate after review fixes: GATE PASS, LoopLint PASS (37 files), build 0 warnings/errors, SELFTEST PASS. git diff --check passed for edited tracked implementation files.
- Deleted the isolated clone, review probe folder, local bundle and temporary probe harnesses under build/. Recursive cleanup was blocked by policy; the gate's existing Remove-BuildEntry helper safely removed only these verified build children without following reparse points. PROBE CLEANUP: PASS.

## Hand checks for the user
None (not visible).

## Hits and disposition
| Original hit | Disposition |
|---|---|
| docs/loop.md:182 stale-id | Allow: previous loop attribution to C26-item-card-loop |
| docs/loop.md:182 stale-version | Allow: previous loop version in history |
| docs/loop.md:183 stale-id | Allow: history names C28-loop-cheap-checks and C13-dev-loop-kit |
| docs/loop.md:183 stale-version | Allow: sentence describes dormant older proposal |
| docs/loop.md:184 stale-path | Allow: history locates dormant scripts |
| docs/loop.md:224 feat-only | One-line correction adds fix/, refactor/, docs/, chore/ to branch example |
| tools/dev.sh:2 stale-id | Allow: historical implementation attribution, not a current item name |

Initial broad version matching also reported docs/loop.md:184 for directory names; restricted version matching to exclude slash-prefixed directory versions, leaving its stale-path check intact. No unresolved hits for the manager.
