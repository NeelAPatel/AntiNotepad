# C25-dev-instance-handoff-audit

## Cause

Confirmed. `Program.cs:41-49` selects `AntiNotepad-dev` for **any nonempty** `ANTINOTEPAD_DATA`; neither checkout path, compile-time dev identity nor `ANTINOTEPAD_DEV` selects the instance. `Platform/SingleInstance.cs:62-70` scopes mutex/Show/Quit/Owner objects to the current Windows session and user. A secondary launch signals Show and returns 0. `tools/dev.ps1:145-237` only knows this checkout's JSON PID record, validates image and start time, then **kills**, without saving. Its launch check can record a secondary before it exits. `tools/dev.sh` delegates to PowerShell; it has no separate PID implementation.

Graceful `--quit` with explicit nonempty dev DATA reaches the resident across checkout paths, including custom-data runs without a dev badge, in the same user/session. It cannot promise to close every resident. `RequestQuit` returns 0 for absent/free/abandoned mutex, missing owner process, or observed process exit; 1 for timeout or caught error. It sends Quit, never Show, and already waits for process exit within a shared ~5-second budget (including owner publication). Zero alone does not prove ongoing vacancy or successful persistence. Owner metadata contains only PID, with a narrow stale/PID-reuse race.

The listener queues `PadWindow.Quit` on the UI thread, latched through startup. Quit flushes editor state and synchronously saves/drains Store, then flushes Store/log and shuts down. Forced save bypasses the normal 50 ms coalescing delay; disk/UI work has no fixed completion bound. Save failure opens Retry/Quit anyway and leaves the app running; unavailable Store can return success without writing. Five seconds is the client's timeout, not a save guarantee; the resident may finish later.

## Files and tier

The six listed implementation files suffice: `tools/dev.ps1`, `tools/dev.sh`, `docs/dev-loop.md`, `docs/loop.md`, `.claude/skills/next-task/SKILL.md`, `.claude/skills/review-task/SKILL.md`. Shell changes may be only comments because it already delegates. No product/selftest change is necessary. Keep launch outside headless Codex: the manager launches after gate **and review acceptance**, immediately before handing over checks; avoid opening an unreviewed build during review/rework.

Correct Medium to **High**: `docs/loop.md` explicitly assigns startup and quit to High, even though this implementation is tooling. Require gate, fresh Codex review, Claude design review and user hand test.

## Installed-copy safety

| Route to installed copy or real state | Guard |
|---|---|
| Missing/empty DATA on quit or normal launch selects installed identity, migrations and data | Set absolute checkout DATA explicitly on every child process, including helper; assert nonempty before spawning. DEV/build branding alone offers no isolation. Preserve explicit dev hotkey/shots. |
| PID fallback, reused/stale records or name-wide termination stops installed process | Prefer no force-kill fallback. Never enumerate/kill by name. Any retained termination requires exact checkout-owned image and start-time verification, not a `build/dev` suffix alone; fail closed on ambiguity/access errors. Audit launch-failure cleanup too. |
| Build/copy/Fresh follows links or deletes real data | Preserve cleanup containment/reparse guards; reject redirected output/helper/PID paths and inspect relevant ancestors. Never invoke install/uninstall or write canonical install paths, shortcuts or Run keys. |
| A dev home's `location.json` points to real notes after an in-app move | Dev DATA isolates identity/app home, not necessarily notes. Verify disposable pointers before hand tests; do not edit real notes or follow the pointer for Fresh deletion. Graceful quit saves the old resident's existing notes destination. |
| Automatic hand-test action targets installed UI/hotkey | Manager uses the dev hotkey and verifies dev badge, executable and data/log paths before interaction; no installed-copy interaction. |

## Risks

- **Build ordering:** building fresh helper into canonical `build/dev` before quitting can fail on the running same-checkout exe/DLL locks. Stage helper output separately; quit before rebuilding/replacing canonical output. Fresh must occur only after shutdown is confirmed.
- **Concurrent launchers:** serialize the whole build/quit/Fresh/launch/record transaction with a distinct per-user/session launcher mutex shared across worktrees (not a per-checkout lock). Bound lock acquisition; release in finally. Different checkout `build/` directories are not coordination state.
- **Quit/relaunch gap:** verify the actual dev mutex is vacant after quit, releasing any probe ownership, and handle abandonment. A manual/old launcher can still race. Short survival plus image path is a heuristic: compare the existing Owner PID with the launched PID as well, then recheck liveness before reporting success. No permanent singleton guarantee against later competing launches.
- **Hung/UI-blocked/slow save:** nonzero helper exit or timeout aborts before cleanup/replacement/launch; retain actionable diagnostics. No automatic escalation to kill; the original pending Quit can complete after timeout.
- **Records/parity:** remove reliance on foreign-checkout PID files for discovery; define stale/legacy local record handling without silently killing anything. If shared storage is introduced, resolve linked-worktree common-dir paths absolutely and validate links; it adds complexity and still misses manual launches. Keep bash as the forwarding wrapper, including arguments and exit status.

## Hand checks

Manager/user only, with disposable dev notes and pointers; installed app may remain running untouched.

1. A running with a recent unsaved edit: launch from B (different worktree/build). Verify A exits, its edit persists, B owns the singleton, and B's image/data/log paths match B. Repeat B-to-A and twice within one checkout, including spaces in paths.
2. No resident, no PID record; stale, legacy and mismatched local records; manually launched dev without a PID record. Verify documented handling and no unrelated process termination.
3. Launch two scripts concurrently from different worktrees, then the same checkout. Verify serialization, truthful success/failure and final owner/path; repeat through `bash tools/dev.sh` and `--fresh`.
4. Disposable save failure, slow/hung resident, startup-time Quit and competing manual launch: expect bounded failure where appropriate, no force kill or premature Fresh, clear recovery instructions. Verify abandoned-mutex recovery after a disposable crash.
5. Fresh clears only launching checkout testdata after shutdown, preserving another checkout's data. Confirm installed PID/start time/image and existing real-data state remain unchanged without interacting with its UI. Parse PowerShell and bash; manager runs gate/reviews. These static checks cannot prove live handoff.

## Recommendation on handling

Keep one task, raise to High, amend the brief before implementation. Use existing graceful protocol plus a launcher mutex: stage a fresh helper separately, call exact `--quit` with explicit dev environment, require success and vacancy, then Fresh if requested, canonical build, launch, verify owner/image/liveness and write local bookkeeping. Abort on failures. Drop the proposed shared-PID/kill fallback: it cannot cure a hung save safely and introduces a second ownership protocol. Product behavior, hotkey and per-checkout data remain in scope only for verification.

Audit evidence: source reads only; no app/process control, build, gate or live checks performed. Branch confirmed `feat/C25-dev-instance-handoff`. Only this report was written; STATUS is left for the manager under the audit's read-only rule.
