# T1-roadmap-overview-server-r2: server swallows requests

Rework round of `T1-roadmap-overview-server` (step T1a). Read the card `.coord/tasks/T1-roadmap-overview.md` and the step
brief `.coord/tasks/T1-roadmap-overview-server.md` first; they still apply unless this file says otherwise.

Tier: Low · docs-tools
Role: worker

## Why this round
The manager ran the server outside the sandbox (port 47899): `-Background` starts and binds, but every `/health` and
`/data` request then hangs until the client times out. Cause, in `tools/roadmap-serve.ps1` main loop: each iteration calls
`$listener.GetContextAsync()` and waits 1000 ms; on timeout it `continue`s and the next iteration starts a new
`GetContextAsync()`, orphaning the earlier pending task, which then swallows the next request. Because `-Stop` and the
one-instance check use `/health`, they fail too.

## Change to the plan
None. Fix: keep one pending `GetContextAsync()` task across iterations and start a new one only after the previous task
completed and its context was handled (the idle-timeout check keeps working on the 1 s wait). Review the rest of the
server for the same kind of problem (a wait that can lose a request, a response that is never closed).

## Files
No new files; the same three as the step brief, plus the report.

## Done means
- The loop no longer orphans pending tasks (visible in the code).
- Your sandbox cannot start `HttpListener` ("handle is invalid"); do not try to probe the server and do not use port
  47899 (a hung instance of the old code is still bound there). The manager runs the live probe afterwards.
- `-Check` still prints `ROADMAP DATA OK`; `pwsh -NoProfile -File tools/gate.ps1 -Task T1-roadmap-overview -Offline` passes.

Report: `.coord/reports/T1-roadmap-overview-server-r2.md`, short: the change and the checks.
