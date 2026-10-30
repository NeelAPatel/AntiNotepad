# Loop2-loop-v3-1-gate-and-audit-hardening-review lens review

## Findings (proven)
1. [major] .claude/skills/next-task/SKILL.md:41 - Audit handoff assumes the director produced the successful report. The wrapper names output using the current attempt's role (`.coord/tools/codex-run.sh:39,45`), and on capacity/rate-limit failure or a reached cap advances director -> worker -> backup (`:32,59-60`). A successful worker therefore writes `<ID-slug>-audit.worker.codex.md` and exits 0 (`:58`), while step 2 copies `<ID-slug>-audit.director.codex.md`. With no director report the copy fails; with a leftover director report it copies stale evidence instead of the successful audit. Proof: exact output-name and failover paths above; no live provider run needed to establish the mismatch. Smallest fix: copy the successful report identified by the wrapper's DONE line (including worker/backup), and stop if that run did not succeed; alternatively give successful runs one stable output path within the six-file scope.
2. [minor] .coord/tools/codex-run.sh:50 - The added unqualified `at capacity` alternative classifies unrelated failures as provider capacity failures. An unsuccessful run logging `write failed: disk is at capacity` or `invalid format capacity parameter` matches, returns 4, and advances the model tier at lines 59-60 instead of preserving the original failure. An in-memory regex probe returned True for both strings and for the intended `Selected model is at capacity`; these use literal substring matching shared by the POSIX ERE and probe expression. Smallest fix: restrict the new alternative to a model/provider capacity diagnostic with word boundaries and identifying context. Keep its existing return-4 chain.

## Behavior changes check
Report's list vs the diff: matches the implemented changes. Two promised workflows have the defects above: role-specific report capture is incompatible with the fixed director copy after failover, and capacity detection also retries unrelated errors. Sandbox validation precedes ledger/report setup (wrapper lines 11-15 versus 20-24), rejects an explicitly empty value, defaults to workspace-write when unset, prints the sandbox, and passes it quoted to codex exec. These wrapper conclusions are static, not execution evidence.

The gate diff changes only the scope switch, the scope-only loose-summary suffix, and Scope processing. Build, selftest, seam, format, changed-file discovery, size, cleanup, exit-0/1 logic and report serialization are unchanged. Scope exceptions still FAIL. The existing `.coord/` exclusion is unchanged and is outside this step's requested changes.

## Scope and plan check
In plan. The requested diff contains exactly the brief's six files: tools/gate.ps1, .coord/tools/codex-run.sh, .coord/templates/findings.md, .claude/skills/next-task/SKILL.md, docs/agents.md and docs/loop.md. No unrelated implementation hunks or unlisted implementation deviations. STATUS/card and other coordination artifacts are outside the supplied implementation diff and were not altered by this review.

Findings template now forbids file writes, returns sections 1-7 and READY in the final message, and describes build probes for the manager. Normal director output agrees with the new copy source; failover does not. The agents sandbox bullet and loop Gates row match the brief's requested wording and exceptions.

## Unproven risks
- Real read-only Codex execution, dry-run sandbox output and invalid-value exit 2 remain unverified here because Git Bash fails before script execution; the implementer explicitly records that limitation.

## Checks run
- Read item card, step brief, implementer report, supplied gate report, review skill and coding rules; inspected every supplied diff hunk and the gate/wrapper consumers.
- Branch readback: chore/Loop2-loop-v3-1. No branch, git-index, product, STATUS or card mutations; only this report written.
- PowerShell parser: 0 errors. Scoped git diff --check: PASS. Scoped diff file list: exactly six planned files. Caller search: no remaining StrictScope references in tools/skills; CI, review-task and merge-task invocation paths inspected.
- In-memory execution of the extracted Scope block, with branch/changed-file/brief reads replaced by fixtures: item stray FAIL; CI stray WARN; LooseScope stray WARN; branch equal to Base WARN; detached HEAD stray FAIL; missing brief FAIL; missing Files section FAIL; no backticked paths FAIL; missing requested rework brief FAIL; malformed round Files/path list FAIL even when base brief allows the changed file; valid rework union PASS. No fixture files or clones created. Real Add-Step inspected separately for the loose-scope suffix. These isolate Scope and do not constitute a full gate run.
- In-memory capacity regex probe: intended provider message matches; both unrelated strings in finding 2 also match. Failover/output-path finding proven by the quoted exact source paths.
- bash -n .coord/tools/codex-run.sh: could not start Bash (CreateFileMapping, Win32 error 5); syntax/execution not claimed as passed. No live Codex run, build or selftest launched. Supplied gate report records PASS, including SELFTEST PASS and CONTRACTS 13 rows (12 pass, 1 manual); that is implementer evidence, not rerun evidence.

VERDICT: rework
