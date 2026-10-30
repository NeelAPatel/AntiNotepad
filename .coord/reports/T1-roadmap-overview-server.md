# T1-roadmap-overview-server

## Summary

Added the era configuration, a read-only JSON builder, and a localhost server with caching, ETags, repeat-start handling, and stop support. The data check and requested gate pass; the live server probe could not get past `HttpListener.Start()` in this execution environment.

## Change list

| File and lines | Change | Plan line served |
|---|---|---|
| `tools/roadmap/eras.json:1-4` | Defines Era 0 and Era 1 boundaries. | Era configuration |
| `tools/roadmap-data.ps1:9-108` | Resolves the main checkout from Git, wraps read-only Git commands, records source warnings, and extracts STATUS sections. | Main-checkout working-copy data |
| `tools/roadmap-data.ps1:109-362` | Builds Plan rows, Now items, card details, and usage entries. | STATUS, task-card, and usage sources |
| `tools/roadmap-data.ps1:363-769` | Assigns eras and builds work log, worktrees, branches, tags, lanes, and graph commits. | Read-only Git data and graph |
| `tools/roadmap-data.ps1:770-904` | Assembles the sample-shaped JSON with `eras` and `warnings`; implements `-Check` and stdout output. | JSON output and data check |
| `tools/roadmap-serve.ps1:1-120` | Loads the builder once and defines localhost health, input fingerprint, and response helpers. | In-process server and cache inputs |
| `tools/roadmap-serve.ps1:121-247` | Implements localhost checks, one-instance detection, `-Open`, `-Stop`, and hidden `-Background` startup. | Server lifecycle |
| `tools/roadmap-serve.ps1:248-390` | Implements the listener, routes, two-second changed-input cache, ETags, idle exit, and start/stop logging. | Local server behavior |

## Behavior changes

- `roadmap-data.ps1` now emits one JSON document by default. `-Root` reads the selected checkout's working files; without it, Git's common directory resolves the main checkout, including when invoked from this worktree. `-Check` verifies the sample keys and Plan row count, then prints counts or exits 1 with a reason.
- The JSON preserves the sample's keys and shapes and adds `eras` and `warnings`. The era file is read from the selected root when present, with the script's working-copy file as a fallback while this step is not yet merged into the main checkout.
- `roadmap-serve.ps1` adds GET routes for `/`, `/data`, and `/health`; unknown paths return 404 and other methods return 405. It reuses builder functions in-process, caches until an input changes and at least two seconds have elapsed, and supports matching ETags, repeat starts, `-Open`, `-Stop`, hidden `-Background`, and a 12-hour idle exit.
- The localhost client disables system proxies, and requests are additionally restricted to loopback endpoints.

## Out-of-scope files

None. The temporary clone, bundle, and probe script under `build/` were removed after the probe attempt.

## Plan deviations

- The server runtime probe could not complete in this sandbox. A standalone `HttpListener.Start()` probe throws `The handle is invalid`; `-Background` on port 47899 consequently exits 1 with `Roadmap server could not bind port 47899.` No listener was available for `/health`, `/data`, repeat-start, cache refresh, ETag/304, or live `-Stop` checks.
- Git for Windows could not clone directly from the checkout here (`sh.exe` failed `CreateFileMapping` with Win32 error 5). The probe used a local bundle clone at `build/T1-probe`; it passed the clone and missing-source checks before the listener limitation, and all temporary files were deleted.
- The 904-line data builder exceeds the coding standard's 600-line review trigger. It remains one job: each parser and Git reader feeds the same JSON document, and the approved Files list provides no shared helper file to split into. No behavior was split across new files.

## Contracts

None. This step changes tooling only and adds no product contract.

## Lens review

- **Findings:** None proven in source review.
- **Behavior changes check:** The report lists all new CLI and server behavior.
- **Scope and plan check:** In plan; only the three step files and this requested report remain. The temporary probe artifacts were removed.
- **Unproven runtime risks:** `HttpListener` binding, route responses, the two-second cache, ETag/304, simultaneous starts, status refresh, and live stop remain unverified because the sandbox rejected listener startup.
- **Checks run:** PowerShell parser passed for both scripts; `roadmap-data.ps1 -Check` passed; the missing-source builder probe passed; gate passed. The server probe reached the `HttpListener` startup limitation described above.

## Checks run

- PowerShell AST parse: `tools/roadmap-data.ps1` and `tools/roadmap-serve.ps1` both parsed successfully.
- `pwsh -NoProfile -File tools/roadmap-data.ps1 -Check`: `ROADMAP DATA OK`; rows 70, groups 8, log 169, lanes 5, worktrees 4, tags 34, cards 37, warnings 0.
- Independent count from the main checkout working copy: 70 Plan item rows and 8 bold groups; `git branch` listed 5 local branches; `git worktree list` listed 4 worktrees.
- Era check: 156 dev commits classified Era 0 and 13 Era 1; the boundary commit `8d18f72` is Era 0. The hash appears only in `tools/roadmap/eras.json`.
- Missing-source probe against an empty `-Root`: builder exited normally and returned 4 warnings for STATUS, task cards, usage, and Git.
- Server probe: local bundle clone passed; missing-source checks passed. `HttpListener.Start()` failed with `The handle is invalid`; HTTP routes, ETag/304, concurrent/repeat starts, changed-STATUS refresh, and live stop were blocked and not verified.
- `pwsh -NoProfile -File tools/gate.ps1 -Task T1-roadmap-overview -Offline`: `GATE: PASS`. Build passed with 0 warnings/errors; selftest passed; seam, format, scope, loop lint, size, and cleanup passed. Offline NuGet audit was a warning only.

## Hand checks for the user

None (not visible). To complete the live server checks, rerun the disposable-clone probe from a normal Windows PowerShell host where `HttpListener` can start; this sandbox returned the bind failure above.
