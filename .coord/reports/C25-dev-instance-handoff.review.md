# C25-dev-instance-handoff lens review

## Findings (proven)
None.

## Behavior changes check
Report's list vs the diff: matches.

- `tools/dev.ps1:256-277,373-375`: both helper and normal launch use `New-DevStartInfo`, which asserts absolute, nonempty DATA and explicitly sets DATA, DEV, hotkey and shots. The helper receives exactly `--quit`. `Program.cs:41-49` selects the dev singleton before handling quit; this route does not target the installed identity.
- `tools/dev.ps1:215-253`: the only termination path uses the published dev owner, pins its process handle, verifies exact canonical repo/worktree image and common Git directory, excludes the installed directory, rejects reparse ancestors, and rechecks start time and ownership before killing that process object. Local PID records never select a termination target. Launch-verification failure performs no termination.
- `tools/dev.ps1:153-181,272-297,412-439`: probes release acquired/abandoned mutex ownership in finally; helper success requires vacancy. Nonzero helper exit permits one verified force-close followed by a second successful helper/vacancy round. The shared SID/session launcher mutex covers staging through recording, has bounded acquisition, and releases in finally. The brief's explicit later authorization of disposable-dev force-close supersedes the audit's original no-force-close recommendation; the audit's identity and path safeguards remain implemented.
- `tools/dev.ps1:420-434`: guarded helper output is built separately before shutdown; local record removal, Fresh and canonical replacement occur only after shutdown/vacancy succeeds. Existing Fresh deletion remains contained to local testdata and does not follow the notes pointer. Output children and relevant managed-path ancestors are checked before build.
- `tools/dev.ps1:380-409`: startup polls owner publication without claiming the product mutex, tolerates missing/zero publication, then verifies survival, exact image, occupied mutex and matching owner before writing the local record last. Failures are reported without an ambiguous cleanup kill.
- Changed internal contracts have no external callers: `Invoke-DevBuild` is called for staged and canonical output; `Get-ProcessIdentity` serves verified owner shutdown and launch verification; `New-DevStartInfo` serves helper and normal launch; `Request-DevQuit` calls `Stop-DevOwner` only after helper failure. The old `Stop-TrackedDev` discovery/termination contract is fully replaced. Repository search found no other consumers of `dev.pid`.
- `tools/dev.sh` retains its existing argument validation, `--fresh` translation and `exec pwsh` exit-status forwarding; only its comment changes. All four documentation/skill edits consistently put launch after gate/review acceptance and before user checks, with badge/EXE/DATA/log verification and failure reporting.

## Scope check
In scope. All six changed implementation files serve the brief; no product, selftest, hotkey or per-checkout data-location changes. This review writes only this report; STATUS remains with the manager under the review's explicit file restriction.

## Unproven risks
- Static review cannot establish live cross-checkout/concurrent handoff, hung-resident recovery, save persistence or installed-process preservation; run the report's disposable-data hand checks.
- Manual/older launchers do not share the launcher lock; vacancy and startup verification detect observed conflicts but cannot prevent every later competing launch, as documented.

## Checks run
- Read the lens-review skill, STATUS, brief, accepted audit, implementer report and existing gate report; inspected every diff hunk, complete launcher/wrapper and product singleton/startup callers.
- `pwsh -NoProfile -Command` with `Parser::ParseFile` on `tools/dev.ps1`: `POWERSHELL PARSE PASS`; launcher not executed.
- `git diff --check`: PASS. Branch: `feat/C25-dev-instance-handoff`.
- `bash -n tools/dev.sh`: UNVERIFIED; Git Bash terminated before parsing with `CreateFileMapping ... Win32 error 5` in the sandbox. Rerun outside the sandbox.
- Existing gate report records `GATE: PASS`, build with 0 warnings/errors, `SELFTEST PASS`, and seam/scope/size/cleanup PASS; read as supplied evidence, not rerun by this reviewer.
- No AntiNotepad launch, quit, kill, build, selftest or GUI interaction performed in this review.

VERDICT: accept
