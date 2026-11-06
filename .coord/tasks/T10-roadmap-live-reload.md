# T10-roadmap-live-reload: the running roadmap server picks up changes to its own scripts without a restart

Item card (`docs/loop.md`), fast lane (side task, `docs/loop.md` "Rules").

Tier: Low · docs-tools (side tool, does not ship), roadmap portal
Branch: `chore/T10-roadmap-live-reload`

## Request
User to Master 4, 2026-11-06, relayed to Roadmap Hub 1: "anytime status is updated, the localhost roadmap should still
be auto updating without any ai intervention periodically, i.e every 3 or 5s". Master 4: a STATUS edit showed up on its
own within seconds, but after T9-roadmap-step-rows changed `tools/roadmap-data.ps1` the running server kept serving the
old script until it was restarted by hand. Both STATUS edits and roadmap tool changes must reach the page with no
restart or AI step.

## Findings
- **Kind and why:** bug; roadmap tool changes do not reach the live page until a manual restart.
- **Now:**
  - The page polls `/data` every 3 s (`tools/roadmap/index.html` `pollData`), so STATUS edits already arrive.
  - `tools/roadmap-serve.ps1` dot-sources `roadmap-data.ps1` once at start (line 14), so `Get-RoadmapData` stays the
    old code for the life of the process.
  - `Get-RoadmapFingerprint` covers STATUS, `usage.tsv`, `eras.json` and the cards in `.coord/tasks`, not the tool
    scripts, so a script change does not even trigger a rebuild.
  - `index.html` is read from disk on each `/` request, so page changes already arrive on the next reload (the page
    itself does not reload on its own).
  - A change to `roadmap-serve.ps1` itself needs a new process.
- **Where:** `tools/roadmap-serve.ps1`; maybe `tools/roadmap/index.html` (see Plan).
- **Risk:** low; worst case is a server that exits on a bad script edit and is not running until the next restart.
- **Source:** Roadmap Hub 1 read the server, 2026-11-06.

## Plan
- **After:**
  - The fingerprint also covers `tools/roadmap-data.ps1` (its length and write time). When it changes, the server
    dot-sources it again before rebuilding, so the new `Get-RoadmapData` is used. If the re-load throws, keep serving
    the last good data and log the error (`Write-RoadmapServerLog`); retry on the next change.
  - When `tools/roadmap-serve.ps1` itself changes (checked in the idle loop, about once a second), the server starts a
    fresh copy of itself with the same arguments (minus `-Open`) and exits, so the port frees and the new copy binds it.
    Make sure the new copy can bind (close the listener first, or let the new copy retry the bind for a few seconds).
  - The page: when `/data` reports a new page version (for example a hash or write time of `index.html` sent in the
    data or a header), the page reloads itself once, so page edits also arrive with no manual refresh.
  - Nothing else changes: poll period stays 3 s, data rules and layout unchanged.
- **Expected time:** about 30 minutes, request to merge.
- **Must not change:** the data JSON shape (beyond one added page-version field, if used), the port, the 12-hour idle
  exit, localhost-only access, `/health` and `/stop`.
- **Scope questions:** None.
- **Test plan, code:** start the server in the background from the worktree on a spare port (`-Port 47819`), touch
  `roadmap-data.ps1` (add then remove a harmless comment, or change a summary field in a temp copy) and see `/data`
  change within about 5 s with no restart; touch `roadmap-serve.ps1` and see `/health` answer with a new pid within
  about 5 s; stop it with `/stop`. Never stop or restart the user's server on port 47811 during the test.
- **Test plan, UI:** the user edits STATUS and watches the page update; Roadmap Hub 1 checks the live page after merge.
- **Gates:** `tools/gate.ps1 -Task T10-roadmap-live-reload`: scope, loop-lint.
- **Needs from you:** Nothing.

## Files
- `tools/roadmap-serve.ps1`
- `tools/roadmap/index.html`

## Contracts
None.

## Approve
User to Roadmap Hub 1, 2026-11-06: "Haiku sub-chat + Codex" (routing for this fix), then "worktree this task too so i
can do some parallel work".

## For Codex
Luna medium (surgical change to one script plus a small page hook). Codex cannot bind ports in its sandbox, so the
server test above is run by the builder sub-chat, not by Codex (`docs/agents.md`).

Implementation notes (from the builder, 2026-11-06):
- **Data script reload:** `Get-RoadmapFingerprint` also stats `tools/roadmap-data.ps1` (path `Join-Path $PSScriptRoot
  'roadmap-data.ps1'`, same `path|length|ticks` form). Keep a `$dataScriptStamp` (length + ticks) captured at start. In
  the `/data` branch, before `Get-RoadmapData`, if the stamp changed: re-dot-source `$dataScript -RequestedRoot $Root`
  inside try/catch, update the stamp, and on error `Write-RoadmapServerLog` and keep the previous functions and cache.
- **Serve script self-restart:** in the idle branch (`-not $pending.Wait(1000)`), compare a `$serveStamp` (length +
  ticks of `$PSCommandPath` or `Join-Path $PSScriptRoot 'roadmap-serve.ps1'`) captured at start. When it changes: log
  it, call `$listener.Stop()`, start a new copy with `Start-Process` (same executable as `(Get-Process -Id $PID).Path`,
  args `-NoProfile -File "<script>" -Port <Port>` plus `-Root "<Root>"` if given, `-WindowStyle Hidden`), set
  `$stopping = $true` and leave the loop. The new copy must be able to bind: in the initial bind, retry
  `$listener.Start()` (fresh `HttpListener` each try, about 20 tries, 250 ms apart) before falling back to the existing
  health check, so the old listener closing first does not lose the port.
- **Page reload hook:** `/data` (200 and 304 responses) sends header `X-Roadmap-Page: <length>-<ticks>` of
  `tools/roadmap/index.html`, computed per request. Add an optional `-PageStamp` to `Send-RoadmapResponse` for this. In
  `tools/roadmap/index.html` `pollData`, read that header before the 304 early return; store the first value seen; if a
  later value differs, call `location.reload()` once. No JSON shape change.

## Done means (Codex, offline only)
- `tools/roadmap-serve.ps1` and `tools/roadmap/index.html` are the only changed files (`git status`).
- Both `.ps1` files parse with zero errors (`[System.Management.Automation.Language.Parser]::ParseFile`).
- `tools/roadmap-data.ps1` runs once with no error (no edits to it).
- Nothing starts a server, binds a port, or touches port 47811 or the installed AntiNotepad.
- `/health`, `/stop`, poll period (3 s), port default and the 12-hour idle exit are unchanged.

## Report
Short report at `.coord/reports/T10-roadmap-live-reload.md`: files changed, what each hunk does, the offline checks
run and their results, anything not done.

## Check
- Codex (quick, Luna medium, offline): both `.ps1` parse with zero errors; `roadmap-data.ps1` ran once with no error;
  only `tools/roadmap-serve.ps1` and `tools/roadmap/index.html` changed. Report: `.coord/reports/T10-roadmap-live-reload.md`.
- Gate: `tools/gate.ps1 -Task T10-roadmap-live-reload -Offline` → **PASS** (scope 2 files, loop-lint, selftest,
  contracts 12 pass / 1 manual, seam, tokens). Plain gate without `-Offline` fails only on the NU1900 NuGet audit
  (no network in the sandbox); `-Offline` is the documented route for that.
- Server test on port 47819 (builder, worktree server; port 47811 untouched):
  - `roadmap-data.ps1` edit (temporary probe field): `/data` showed it after about 5 s, and gone after restore
    (about 3 s). The file was restored byte-for-byte (`git diff` clean).
  - `roadmap-serve.ps1` touched: `/health` pid changed 7356 → 28896 without a manual restart. The first attempt
    looked failed because a polling loop kept the idle check from running; a quiet wait showed the restart.
  - `/data` 200 carries `X-Roadmap-Page`; `/stop` on 47819 worked; 47811 still answered its own pid.
  - Page script: `node --check` on the inline script passes.
- Not exercised: the browser reload when `index.html` changes (no live page under test). The user's hand test covers it.

## Hand test
- Not recorded check by check. User to Roadmap Hub 1, 2026-11-06, with the 47819 test server up: "merge it". Roadmap Hub 1
  checks the live page on 47811 after the one-time restart.
