# Retrospective, 2026-10-17

Written from the repo files, logs and `git log`. It does not rely on chat memory.

## 1. What was achieved

**Verified (user hands-on or a real-process test):**
- **Spike A (Avalonia) works.** The user confirmed focus, decorated Markdown, Python fence colours, checkbox click, folding, line numbers and wheel page-switch ([USER-FEEDBACK A](spikes/A/USER-FEEDBACK.md)). The user's words: "works surprisingly better than I thought".
- **A autosave and relaunch** passed an automated real-process test ([REPORT A](spikes/A/REPORT.md)).
- **B input gate** passed: 279 wheel events and 11 press / 10 release events from the MX Master 3S ([input-gate1.tsv](spikes/B/input-gate1.tsv)).
- **B builds and runs.** The user saw smoother animation and nicer text than A ([USER-FEEDBACK B](spikes/B/USER-FEEDBACK.md)).
- **Decisions are recorded:** B parked, A leading, transparency de-prioritized ([decisions-log.md](decisions-log.md)).

**Claimed only, not verified:**
- **A's feedback fixes** (Shift+Tab, hover cursor, 220 ms page slide) are committed in 422bca8. The REPORT lists them as "awaiting short retest".
- **A's tray and hotkey delivery** was never physically tested. Another copy held the chord.
- **Latency numbers** are single scripted debug-build samples: 3.2 ms show-to-focus, 1882 ms main-to-opened. They are not hotkey-to-paint.
- **B's tray and hotkey failed** (`SHELL ERROR`) and was never diagnosed.
- **B's checkbox, Tab/Shift+Tab, autosave and measurements** are PENDING in its own REPORT. The user said only that "expected features work".
- **The 15/15 and 12/12 smoke checks** cover models and stores, not the UI.

## 2. What could have been done better

- **Ask first.** Input hardware (no touchpad) and the Codex sandbox having no network were found only after Directors had built to the wrong gate. The touchpad finger-lift gate was written into the plan and T-001 before anyone asked what the user owns.
- **Sandbox diagnosis took hours.** `codex exec` failed with `node_repl` os error 32. The `windows.sandbox="unelevated"` fix came from a peer session (per the task brief). Exactly how many hours this cost is not recorded in the repo.
- **"Wait inside a turn" mistake.** Directors were told to finish in one turn, and the same lesson is recorded in [comms-tests.md](comms-tests.md). It was learned the hard way, and the manager had no polling of its own.
- **Quota burn.** Logs total about 44,650 lines. Single resumes used 500k and 330k tokens. The limit hit at ~2 AM mid-B-fix (T-003-resume4.log:1843). A and B ran in parallel, so one fix round of B was the last spend before the user parked it anyway. A cheaper order would have been A first and B only on a concrete failure.
- **Coordination overhead.** `.coord/status-A.md` is 22 KB against a 1.5 KB STATUS.md. Many Director resumes carried large briefs and logs.
- **Commits.** Directors could not write `.git`, so the manager committed everything. Per-task commits were never real.

## 3. What was overlooked

- **Gate definitions moved mid-run** (touchpad, then mouse wheel, then wheel = page switch). Specs 4, 4a and 4b now coexist in [spike-spec.md](spike-spec.md) and contradict each other.
- **Network:** crates and NuGet packages had to be fetched by the manager because the Codex sandbox is offline.
- **Shared STATUS.md:** Directors were told not to touch it, so STATUS was stale between manager updates. Nobody owned it formally.
- **Never tested:**
  - hide and reopen. The user saw no cue, which is a UX gap in itself.
  - the Ctrl+Alt+Space collision (error 1409) was found in A, but B still used that chord and only had the A chord suggested at parking (PARKED.md).
  - B's idle-versus-animating GPU baseline (the user had no baseline for the ~4-5% reading).
  - which text-rendering parts of A the user prefers. The specifics were not collected.
  - release-build performance (everything was measured in Debug).
- **The "Mac AppKit polish" goal** arrived late and was never in the spike spec. The spikes proved capability, not feel.

## 4. Risks still open for the real app

- **Polish ceiling:** Avalonia on Windows is unlikely to match native Mac fit and finish unless there is custom styling work. The UI variants are the test of that.
- **Carousel feel** with a notched wheel is hitchy in A. Direct manipulation applies only to drag, through a grab strip.
- **Editor effort:** the Markdown and Python scanner is a bounded regex transformer. A production parser or grammar path is several days of work.
- **Hotkey and tray** reliability is unproven in both spikes.
- **Speed priority** (the project's top goal) has no valid measurement yet.
- **Maintenance:** AvaloniaEdit's 12.0.0 package dates from 2026-04. B's GPUI snapshot is a vendored fork.

## 5. Recommended next steps (priority order)

1. **Retest A's 4 checks** (end of `.coord/status-A.md`): Shift+Tab, hover, wheel feel, tray/hotkey with the old copy closed.
2. **Review the UI variants** and judge whether Avalonia can reach the polish you want.
3. **Decide the stack** (A leaning), then write the real implementation plan.
4. **Measure a Release build:** hotkey-to-visible-and-focused, hide time, idle RAM.
5. **Resume B only if A fails** the polish or feel review. Use `PARKED.md`.

**Process changes**
- **Intake first:** list hardware, network, quota and OS constraints before writing any gate.
- **One spec, one version:** supersede spec items instead of appending them.
- **Cap quota per run:** set a token budget and one milestone per Director turn. Prefer Sonnet sessions for small fixes.
- **Sequential spikes:** start the second only if the first fails a gate.
- **Fix the commit path early** so every task gets a real commit.
- **Single STATUS owner:** the manager copies from the per-agent status files.

## 6. Decisions the user still owns

- **Final stack:** A, or revisit B or C.
- **Polish bar:** how close to AppKit is "good enough".
- **Whether transparency returns** to the requirements.
- **Hotkey choice:** Ctrl+Alt+Shift+A, or another chord.
- **Wheel behaviour:** one notch = one page, or something smoother.
- **Spend:** more Codex usage, or Claude-only work.
