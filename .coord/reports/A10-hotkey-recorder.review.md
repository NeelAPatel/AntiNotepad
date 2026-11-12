# A10-hotkey-recorder lens review

## Findings (proven)
None.

## Behavior changes check
Report's list vs the diff: matches across the round-1 report and latest r3 report. Round 1 describes the cumulative recording behavior and per-control state; the latest report explicitly limits its "None" to r3 and retains the r2 Escape-routing fix.

- Both Settings callers (`UI/SettingsWindow.cs:499-505`) now use HotkeyControl, whose sole caller constructs HotkeyRecorder (`:541-542`). Their registration callbacks, success-only settings writes, and optional empty-to-null conversion are preserved. ISettingsHost, PadWindow forwarding, HotkeyRegistrar rollback, GlobalHotkey parsing, settings fields, defaults and dependencies are unchanged.
- GestureText is called by RecordKey and the focused selftest; IsCommand likewise. All 66 main keys produce parser-compatible text with modifiers in Ctrl/Alt/Shift/Win order. Modifier-only and unsupported keys produce no gesture. Unmodified Esc/Enter are commands; modified forms record normally.
- New state is confined to each recorder. Begin resets the candidate and disables Save; supported capture enables it; unsupported keys leave it intact; releases retain it. Successful Apply or cancellation resets recording and candidate. Failed Apply retains the session and error hint. Optional Clear passes empty text to the unchanged callback.
- IsRecordingSource is called only by the Settings tunnel guard and its selftest. It defers Escape for an active recorder host or descendant, while idle/outside sources retain the existing Settings command. The recorder then handles bare Escape as Cancel and modified Escape as a candidate.
- Deferred Begin focus is guarded by recording state; the deferred LostFocus check cancels only when focus is outside the recorder, allowing internal action-button focus. Handlers belong to the control; no static capture state, timers, native calls or upward layer dependencies were introduced.

## Scope and plan check
In plan. Reviewed every product hunk against After and Must not change: the extracted recorder, two row substitutions and old-editor removal, r2 tunnel guard, key/guard selftests and corresponding CONTRACTS rows all serve approved requirements. The extra Escape contract is expressly covered by Rework r2. No unrelated product hunks or unlisted product deviations found. The latest report documents its folder-mode formatting workaround; it changes no runtime behavior. Registration, stored text format, defaults and ISettingsHost signatures remain unchanged.

## Unproven risks
- Actual focus transfers between host/buttons, window deactivation, and focus after replacing content still require the planned hand test; the probe only raises synthetic events without a window.
- Windows delivery of reserved/Win combinations and real registration refusal remain hand-test territory; parser and synthetic-event success do not prove OS delivery or registration.

## Checks run
- Read the item card including Approve and Rework r2/r3, both requested implementer reports, GATE: PASS report, complete `git diff dev...HEAD -- product`, changed-file context and registration/settings/button callers. Initial git status was clean.
- Offline product build: `AVALONIA_TELEMETRY_OPTOUT=1; dotnet build product/AntiNotepad -c Debug --no-restore -o build/A10-hotkey-recorder-review/app` with IntermediateOutputPath inside the review folder and NuGetAudit=false: Build succeeded; 0 warnings, 0 errors.
- Review build `AntiNotepad.exe --selftest`: CONTRACTS: 17 rows, 16 pass, 1 manual; SELFTEST PASS. Existing registrar checks also pass, including refusal rollback and blank new-note disable.
- Temporary reflection probe against that build, without opening a window: waiting Save disabled; canonical capture; release retention; unsupported-key retention; failed Save retains recording and error; successful Enter; modified Esc; bare Esc cancellation without apply; modifier-only Save disabled; Cancel; optional Clear; synthetic LostFocus plus dispatcher drain cancellation. Final result: PROBE PASS (12 checks). Initial probe harness issues (native Skia lookup and incorrect synthetic focus-event argument type) were corrected within the review folder; they were not product failures.
- `git diff --check`: pass. No app UI, ports, servers, browser or screen checks run. Temporary build/probe folder removed after checks; product files unchanged.

VERDICT: accept
