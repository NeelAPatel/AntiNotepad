# Release speed baseline (M5)

## Headline (interactive desktop run, 2026-10-18, v0.1)

**Budget hotkey-to-visible-and-focused < 100 ms: PASS** (median ~11 ms, p95 ~15 ms, 27 samples per state).

| Metric (ms) | Empty: median / p95 / max | 20 x 200-line notes: median / p95 / max |
|---|---|---|
| Key -> WM_HOTKEY | 1.8 / 2.5 / 2.6 | 1.9 / 2.9 / 3.0 |
| Key -> editor focused (foreground) | 11.0 / 12.5 / 13.0 | 11.1 / 14.6 / 14.9 |
| Key -> first frame callback | 11.1 / 12.6 / 13.2 | 11.2 / 14.8 / 15.0 |
| Key -> visible and focused | 11.1 / 12.6 / 13.2 | 11.2 / 14.8 / 15.0 |
| Esc -> hidden (incl. save) | 6.3 / 7.3 / 8.9 | 18.1 / 23.4 / 25.3 |

| Idle 30 s | Working set MiB | Private MiB | CPU % of one core |
|---|---:|---:|---:|
| Empty, hidden | 136 | 103 | 0.31 |
| Empty, shown | 131 | 98 | 0.47 |
| Heavy, hidden | 149 | 113 | 0.26 |
| Heavy, shown | 144 | 109 | 0.83 |

Cold start (process start -> first frame callback): 984 ms (one observation).

Raw: `docs/perf-data/2026-10-18-desktop.json`. Run by the manager outside the Codex sandbox (SendInput is blocked inside it).
Not covered: physical keyboard transport, compositor/DWM presentation and scanout after the frame callback. Display scale 100%.
The sandbox run below showed ~100% of one core after show; the desktop run shows < 1%, so that was a sandbox/render-backend artifact, not the app.

Rerun (desktop, hands off for ~3 min; PowerShell must wait for the windowed exe):

```powershell
Start-Process .\bin\Release\net10.0\AntiQuickPad.exe -ArgumentList '--measure','--samples','30','--out','.\measure.json' -Wait
et10.0\AntiQuickPad.exe -ArgumentList '--measure','--samples','30','--out','.\measure.json' -Wait
```

## After T-012 refactor (desktop, 2026-10-18)

| Key -> visible and focused (ms) | v0.1 median / p95 | T-012 median / p95 |
|---|---|---|
| Empty | 11.1 / 12.6 | 12.2 / 13.4 |
| 20 x 200-line notes | 11.2 / 14.8 | 11.8 / 13.0 |

Esc -> hidden: 6.6 / 8.1 ms (empty), 17.5 / 19.3 ms (heavy). Idle CPU < 1.1% of one core; working set 132-145 MiB. Cold start 1008 ms. Verdict: no regression (differences within run-to-run noise). Raw: `docs/perf-data/2026-10-18-desktop-T012.json`.

## After T-013 shells (desktop, 2026-10-18)

| Key -> visible and focused (ms), median / p95 | v0.1 Classic | Quiet | Card |
|---|---|---|---|
| Empty | 11.1 / 12.6 | 13.2 / 15.4 | 12.7 / 15.1 |
| 20 x 200-line notes | 11.2 / 14.8 | 13.2 / 17.2 | 13.1 / 17.5 |

Esc -> hidden median / p95: Quiet 5.9 / 7.6 (empty), 17.9 / 19.3 (heavy); Card 6.8 / 8.6, 20.5 / 28.3. Idle CPU <= 1.2% of one core. Working set shown: Quiet 138 / 184 MiB, Card 155 / 203 MiB (empty / heavy; v0.1 was 131 / 144). Cold start ~1030 ms. Verdict: PASS, ~2 ms slower than Classic and ~40-60 MiB more memory in the heavy state; worth watching as motion lands. Raw: `docs/perf-data/2026-10-18-desktop-T013-{quiet,card}.json`.

---

# Sandbox run and full method (T-010 Director)

## Method and rerun

From `src/AntiQuickPad`, quit the ordinary resident gracefully before building:

```powershell
dotnet bin/Release/net10.0/AntiQuickPad.dll --quit
$env:AVALONIA_TELEMETRY_OPTOUT='1'
dotnet build -c Release --no-restore -p:UseSharedCompilation=false
dotnet bin/Release/net10.0/AntiQuickPad.dll --selftest
& .\bin\Release\net10.0\AntiQuickPad.exe --measure --samples 30 --out .\measure-release.json
```

Run the last command in an interactive Windows desktop outside the sandbox to complete input/focus timings. Leave the desktop unlocked, do not type or click during sampling, and leave Ctrl+Alt+F9 available. The run takes about 2.5 minutes. A complete input baseline has 30 total samples, with the first 3 discarded as warmups, leaving up to 27 retained **in each state**. `--samples N` specifies total attempts; minimum spacing is 300 ms after each completed hide. A missing WM_HOTKEY or foreground-focus endpoint stops that state's input loop after the first failed attempt, then continues other metrics. Missing values are JSON null, never zero. Exit 0 means the harness collected its available metrics and shut down cleanly; consult `gaps` before interpreting it as a complete input baseline.

The harness does not acquire ordinary residency. It launches a separate apphost child with `--measure-child --instance AntiQuickPad-measure-<GUID> --data <unique directory>`, sets only the child's `ANTIQUICKPAD_HOTKEY=Ctrl+Alt+F9`, and uses the existing per-user/session singleton scheme under that unique ID. The private child flags must be supplied together; they are intended for harness use. All notes, settings, command files, event streams and logs stay under `bin/Release/net10.0/measure/<GUID>`. The ordinary resident and its data are not used by measurement. Both baseline (one empty note) and heavy (20 notes, exactly 200 Markdown lines each, last note selected) use the same resident process. The heavy fixture is created through NotesSession and bound to the real editor, without synthetic input typing. It is not a navigation or all-notes-rendered benchmark.

The parent calls QueryPerformanceCounter immediately before SendInput; the child's WM_HOTKEY timestamp uses Stopwatch.GetTimestamp (the same Windows QPC clock). Focus, frame and hidden events use QueryPerformanceCounter directly. Raw absolute ticks remain in the child's `events.jsonl`; parent input/Esc ticks and millisecond deltas remain in the output JSON. Events flush synchronously, so instrumentation and log I/O are included in the timings. SendInput sends Ctrl down, Alt down, F9 down, then releases in reverse order; Escape is also sent through SendInput. No internal show call is used for the timed hotkey path.

- **Input to WM_HOTKEY:** injected chord submission to receipt in the actual Win32 hook; excludes physical switch/USB/Bluetooth keyboard delay.
- **Input to focused:** submission to confirmation that the editor has keyboard focus, its Avalonia window is active, and GetForegroundWindow matches its native handle. This avoids counting logical focus in an inactive window.
- **Input to first frame callback:** submission to the first RequestAnimationFrame callback scheduled after Show. This is a rendering opportunity, **not proof of a painted/presented frame**: the callback can precede drawing, compositor submission, DWM presentation and scanout. Those latencies require an external presentation/optical measurement. The combined endpoint is max(focus, frame callback).
- **Esc to hidden:** injection to Hide returning after the production save/cancel path. It includes synchronous note/settings saving, but excludes compositor fade/scanout.
- **Cold start:** parent QPC just before Process.Start to the startup frame callback. This is one process-cold launch, not a reboot/disk-cache-cold launch, and inherits the frame limitation above.
- **Idle:** one-second settling period, then actual elapsed 30-second windows hidden and shown; working set and private bytes read at each window's end. CPU is process TotalProcessorTime delta / wall time, expressed as percent of one logical core (divide by reported processor count for machine-capacity percent). The measurement child has a 50 ms control-file poll timer and a graceful watchdog, so idle CPU includes probe overhead. Control-file show/hide calls are used only to obtain idle states; they are never timing samples or substitutes for blocked keyboard input.

Statistics sort retained non-null samples: minimum, ordinary median (average central pair), nearest-rank p95, maximum, all milliseconds. No percentiles are inferred from failed samples. Idle values and cold start are individual observations, not distributions. No forced GC or process termination is used. Child shutdown follows save/quit; a ten-minute graceful watchdog is a fallback if the harness disappears. Existing artifacts are retained for audit; each rerun uses a new directory.

## Machine and results

Measured 2026-10-18, standalone Release run completed 12:43:25 UTC. Raw artifact: [measure-release.json](perf-data/2026-10-18-sandbox.json). Child audit stream: `src/AntiQuickPad/bin/Release/net10.0/measure/a00a6661081941a9a361ac5c11179c41/events.jsonl`; ordinary child shell log beside it under `logs/`.

Machine: Windows NT 10.0 **26340.9616**, registry DisplayVersion **26H2**; AMD Ryzen 7 7800X3D, **16 logical processors** exposed to the process; .NET **10.0.13**, Avalonia **12.1.4**, AvaloniaEdit **12.0.0**. Window RenderScaling **1.0 (100%)**. Physical display refresh rate, mixed-monitor setup, RAM capacity and GPU/driver were not established; CIM inventory returned Access denied. Scale is the measured child window scale, not a claim about every monitor. QPC frequency **10,000,000 Hz**. This is a sandbox desktop run; normal desktop presentation and foreground interaction were not verified.

| State / 30 s idle window | Actual seconds | Working set (MiB) | Private bytes (MiB) | CPU (% of one core) |
|---|---:|---:|---:|---:|
| One empty note, hidden | 30.012 | 141.79 | 103.68 | 0.208 |
| One empty note, shown | 30.007 | 199.04 | 191.73 | 100.341 |
| 20 x 200-line notes, hidden | 30.014 | 200.30 | 189.80 | 97.819 |
| 20 x 200-line notes, shown | 30.014 | 201.98 | 205.34 | 97.196 |

MiB = 1,048,576 bytes; exact byte counts and elapsed times are in JSON. Cold process start -> startup frame callback: **952.144 ms**, one observation. A prior independent smoke run recorded **981.445 ms** and reproduced the high post-show CPU (98–100% of one core). 100% of one core is approximately 6.25% of this machine's exposed CPU capacity. The high CPU persists in the later hidden window; its cause is **unresolved**, and cannot be assigned to either the sandbox/render backend or the app from this measurement. Recheck outside the sandbox before adopting this as the normal-desktop baseline. The initial hidden window precedes the control-file re-show; the later hidden window follows shown idle and fixture creation, so these are sequential states, not fresh processes.

| Metric (ms) | State | Retained n | Min | Median | p95 | Max |
|---|---|---:|---|---|---|---|
| Input -> WM_HOTKEY | Empty / heavy | 0 / 0 | unavailable | unavailable | unavailable | unavailable |
| Input -> foreground editor focused | Empty / heavy | 0 / 0 | unavailable | unavailable | unavailable | unavailable |
| Input -> first frame callback | Empty / heavy | 0 / 0 | unavailable | unavailable | unavailable | unavailable |
| Input -> max(focus, frame callback) | Empty / heavy | 0 / 0 | unavailable | unavailable | unavailable | unavailable |
| Esc input -> hidden | Empty / heavy | 0 / 0 | unavailable | unavailable | unavailable | unavailable |

**Exact gap:** Ctrl+Alt+F9 registration succeeded (Win32 registration error 0). SendInput returned **0 of 6 events, last-error 5 (Access denied)** on the first warmup attempt in each state. The child never received WM_HOTKEY; no timed focus or show-frame endpoint exists. Escape was not sent because foreground editor focus could not be established; it is unavailable rather than a measured failed Escape injection. Initial startup and control-file shows also produced no foreground-confirmed focus event. Each state stopped input retries at the blocked first warmup while continuing all resource measurements. Requested total count was 30 per state (up to 27 retained); actual retained count was zero. This identifies the API failure without asserting which desktop/UIPI restriction caused it.

**Budget verdict: UNVERIFIED.** The warm hotkey-to-visible-and-focused <100 ms target cannot pass or fail from this sandbox run. The cold-start value measures a different operation. Even a future passing callback/focus baseline will not establish compositor-to-screen latency. Run the command above on the manager's interactive desktop to obtain the missing distributions; preserve the JSON and compare the same machine, scale, build, note fixture and endpoint definitions for M6.

Release build: 0 warnings, 0 errors. Full selftest includes a real `--measure --samples 3 --out <isolated path>` process, checks exit 0, parses JSON and verifies both state objects. It intentionally accepts explicit desktop-input gaps; it does not turn those gaps into a timing pass. The smoke run also spends four full 30-second idle windows, so `--selftest` now takes roughly 2.5 minutes. Existing proof PNGs now stay in the selftest's own directory rather than the prior T-009 report destinations.


