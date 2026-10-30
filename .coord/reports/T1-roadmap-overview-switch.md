# T1-roadmap-overview-switch

## Summary

The roadmap now opens from the localhost server, the prompt hook starts it asynchronously when available, and the old rebuild hook and script are removed. Static snapshots inline the current roadmap data and bypass polling; the approved page layout is unchanged.

The hook’s start and already-running paths were exercised with a local TCP health stub. The real roadmap server could not be validated in this sandbox because `HttpListener.Start()` failed with “The handle is invalid.”

## Change list

- `tools/loop-card.ps1:35-69` | Added a 300 ms health request and hidden, detached server start with failures swallowed | Plan After 1.
- `.claude/settings.json:1-22` | Removed only the PostToolUse and Stop roadmap hooks | Plan After 2.
- `tools/roadmap.ps1:1-129` | Deleted the old page builder | Plan After 3.
- `tools/roadmap-data.ps1:5-8,884-910` | Added `-Snapshot` to inline current data in the specified file | Plan After 4.
- `tools/roadmap/index.html:220-222,627-631` | Added the snapshot data slot and startup path that skips polling and shows `snapshot <updated>` | Plan After 4.
- `.claude/skills/roadmap/SKILL.md:3-16` | Switched `/roadmap` and phone copy instructions to the server and snapshot | Plan After 5.
- `README.md:1-19` | Added the requested app sentence and Roadmap section | Plan After 6.
- `AGENTS.md:29`, `docs/loop.md:162-164,214-216,305-306` | Updated only the roadmap references | Plan After 7.

## Behavior changes

`UserPromptSubmit` now checks `http://localhost:47811/health` for the roadmap app and starts `$Root/tools/roadmap-serve.ps1` hidden without waiting when it is unavailable. The hook still exits successfully and prints only its existing status-card text. A snapshot page renders `window.ROADMAP_DATA` directly and does not call `pollData()`.

## Out-of-scope files

- `.gitignore` and `docs/wiki/Who-Does-What.md` still contain live legacy roadmap references; neither is in this step’s Files list.
- `.coord/STATUS.md`, the task cards, and the sample JSON also contain historical references. STATUS and the card/references were explicitly excluded from editing.

## Plan deviations

No implementation deviation. The host-level real-server lifecycle check remains unverified because this sandbox rejected `HttpListener.Start()`.

## Contracts

None.

## Lens review

Worker checklist run manually: checked hook output and failure handling, snapshot script escaping and injection order, and the page’s conditional polling path. No code issue found. No separate lens-review agent was used for this worker run.

## Checks run

- `tools/loop-lint.ps1`: PASS (38 files).
- `tools/gate.ps1 -Task T1-roadmap-overview -Offline`: PASS; build 0 warnings and errors, selftest PASS, seam PASS, scope PASS (11 files), loop-lint PASS, size PASS.
- `.claude/settings.json`: parses with `ConvertFrom-Json`; no `roadmap.ps1` reference remains.
- `tools/roadmap-data.ps1 -Snapshot build/T1-snapshot.html`: wrote inline data; code inspection confirmed the `DATA` branch skips `pollData()`; the snapshot file was deleted.
- Hook output: the stopped and running runs printed identical status-card lines. The first stub-backed start took 1.12 s; three already-running calls took 0.85 s, 0.81 s, and 0.80 s. One stub PID/start record remained, then the stub was stopped.
- The actual server could not bind: an isolated `HttpListener.Start()` on port 47891 threw `System.Net.HttpListenerException (6): The handle is invalid`. The server reported no instance on 47891 or 47811 after the attempts.
- `git grep -n -I roadmap.ps1`: legacy matches remain in `.gitignore`, `docs/wiki/Who-Does-What.md`, STATUS, task/reference files, and reports. This grep condition is not met within the approved file scope.

## Hand checks for the user

No UI hand check was specified for this switch. Re-run the real loop-card server-start check on the host after the main checkout contains the T1a server files; the approved roadmap UI was checked during T1b.
