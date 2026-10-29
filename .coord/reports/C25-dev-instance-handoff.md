## Summary
Confirmed cause: every custom-data run shares `AntiNotepad-dev`, but the previous launcher only stopped its checkout-local PID, so a foreign dev primary received Show and the new build exited. The launcher now serializes cross-checkout shutdown/build/launch and verifies published ownership before recording success. Product code is unchanged; live checks remain for the manager.

## Change list
file:line-range | what changed | which goal line it serves
---|---|---
`tools/dev.ps1:16` | separate helper output | Goal 2 staging
`tools/dev.ps1:146-297` | singleton probes, path/repository validation, graceful quit and verified owner force-close, explicit child env | Goals 1-3
`tools/dev.ps1:299-310` | build to supplied guarded destination | Goal 2 staging and canonical build order
`tools/dev.ps1:368-410` | owner/image/survival verification, record last, printed EXE, no ambiguous launch-failure kill | Goals 1-3
`tools/dev.ps1:412-441` | bounded shared launcher mutex and ordered transaction with finally release | Goal 2 serialization and Fresh ordering
`tools/dev.sh:2-3` | describe shared handoff; preserve argument/exit forwarding | Goal 1 parity
`docs/dev-loop.md:63-92` | handoff promise, bounds, force-close safety, record handling and accepted-round launch | Goals 1-4
`docs/loop.md:26,61,77-79` | sub-session opens verified worktree build after acceptance | Goal 4
`.claude/skills/next-task/SKILL.md:47-49` | implementation sub-session launch timing | Goal 4
`.claude/skills/review-task/SKILL.md:47-52` | accepted-round launch, checks and failure handling | Goal 4

## Behavior changes
- One per-user/session `Local\AntiNotepad-dev-<SID SHA256 prefix>-Launcher` mutex covers staging through record; waits at most 120 seconds and releases in finally, including abandoned-lock recovery. Dev singleton probes also handle abandonment and release probe ownership.
- Builds helper Debug dev identity in `build/dev-handoff-helper` before requesting exact `--quit`; canonical `build/dev` is built only after confirmed vacancy. Every app child receives explicit DEV=1, absolute nonempty checkout DATA, unchanged hotkey and shots path.
- Graceful client normally returns within about 5 seconds. Nonzero exit invokes verified singleton-owner force-close (not local PID records); only same-repository canonical main/worktree dev images qualify, with process handle/start time and owner rechecks. Waits 5 seconds for forced exit, requires vacancy, then runs a second helper requiring exit 0 and vacancy; no repeated force-close loop. An occupied mutex after exit 0 fails. No name enumeration/kill.
- If the helper itself remains unresponsive after 8 seconds, fail without replacement, requiring its exit before retry: a delayed Quit is ambiguous, and the brief prohibits killing staging images. Normal resident graceful timeout returns nonzero and uses the force-close path. Force-close may lose disposable dev edits.
- Reject reparse ancestors on managed paths, redirected build children, installed images and foreign/unverified roots. Canonical install path is explicitly excluded. No install, registry, hotkey or per-checkout DATA changes.
- Local legacy/stale/mismatched records no longer select a process; remove them after shutdown, write new JSON only after owner/image/liveness validation. Startup publication polls Owner without acquiring the product mutex and tolerates missing/zero publication, with a 5-second bound plus 750 ms survival wait; success now prints EXE. Launch failure leaves ambiguous processes for inspection instead of killing them.
- Fresh cleanup implementation is unchanged; it runs after shutdown, only on local testdata, never follows the notes pointer. Helper output persists under build; all bookkeeping remains local. No new shared disk record.
- Implementation sub-session now starts and verifies the dev badge, EXE, DATA and log after gate/review acceptance, immediately before hand checks; never during review/rework. Codex does not launch it.

## Out-of-scope files
- `.coord/reports/C25-dev-instance-handoff.md`: report explicitly required by this brief.
- `.coord/reports/C25-dev-instance-handoff.gate.md`: generated required offline gate artifact (not committed).
No other implementation files changed. Existing untracked audit director output was left untouched. STATUS remains with the manager per `docs/loop.md` branch rule.

## Contracts
None of the product CONTRACTS rows changed. The one-dev-copy/current-checkout launcher promise is documented in `docs/dev-loop.md`.

## Lens review
VERDICT: accept; no outstanding findings. Fixed both proven major findings: (1) polling vacancy during startup could acquire the mutex before the child and make it exit as a secondary; now poll Owner without claiming the product mutex; (2) the map can briefly contain zero before PID publication; startup polling now tolerates zero/missing publication. Reviewer accepted the final diff, including the second helper exit-0 check.

## Checks run
- PowerShell Parser::ParseFile: POWERSHELL PARSE PASS (no execution of launcher).
- `bash -n tools/dev.sh`: could not parse: Git Bash failed at runtime startup, `CreateFileMapping ... Win32 error 5` in sandbox. Manager must rerun outside sandbox.
- `pwsh -NoProfile -File tools/gate.ps1 -Task C25-dev-instance-handoff -Offline`: GATE: PASS; Build: PASS, exit 0, 0 warnings/errors; Selftest: PASS, SELFTEST PASS; Seam/Scope/Size/Cleanup: PASS. Offline WARN: NuGet audit disabled for this build only.
- `git diff --check`: passed. Final PowerShell parse and offline gate repeated after script fixes, with the same passing results.
- No AntiNotepad launch, quit, kill or GUI performed by Codex; only gate selftest. Live behavior unverified.

## Hand checks for the user
1. Manager completes fresh Codex and Claude design reviews first. Use disposable dev notes; inspect A/B `build/testdata/location.json` before typing so neither points at real notes. Record installed PID/start time/image without interacting with its UI.
2. From checkout A, run `pwsh -NoProfile -File tools/dev.ps1`; verify dev badge, printed EXE/DATA, log under A DATA and singleton owner equals its recorded PID. Type a disposable edit.
3. From worktree B, run the same command. Verify A exits and edit persisted; B is the sole dev primary, its PID/image is B `build/dev/AntiNotepad.exe`, badge is dev and DATA/log are B. Repeat B-to-A; run twice in the same checkout, confirming newest build and one primary each time. Include paths with spaces.
4. Exit dev through its own UI, remove local record, launch again; test stale, bare numeric and mismatched records and a manually launched dev copy without a record. No unrelated process may be stopped.
5. With the updated launcher in both checkouts, run A and B launchers concurrently, then two from the same checkout. Verify serialization and final owner/path belongs to the last launcher. Repeat via `bash tools/dev.sh` and `--fresh`; rerun `bash -n tools/dev.sh` outside sandbox.
6. With disposable dev state, test a blocked/hung UI or save failure: graceful timeout should return nonzero, warn and force-close only the verified canonical dev owner, then launch this build. Check ambiguous/foreign dev image fails clearly before Fresh/replacement. Test crashed/abandoned mutex recovery and competing manual launch failure.
7. Test Fresh from B while A runs: only B testdata cleared after A exits; A data persists. Confirm installed PID/start time/image and real-data state unchanged in every case. End with accepted task worktree's dev copy open; verify badge/EXE/DATA/log before handing over checks.

## Manager verdict
Tier High. Gate: PASS (build 0 warnings/0 errors, selftest PASS, seam/scope/size/cleanup PASS). Codex review: `VERDICT: accept`, no proven findings. Design review: accept-with-followup. Followups (proposals only, not numbered): pin the dev.ps1 mutex-name/owner-map copy of `SingleInstance.cs` with a cross-reference comment and a gate check; comment the three-levels-up checkout-root assumption; optionally skip the helper build when the dev mutex is already vacant. `bash -n tools/dev.sh` PASS (run by the manager).
