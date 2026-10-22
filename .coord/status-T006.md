# T-006 checkpoints

- Started: read AGENTS, coordination state, task, feedback items 1-5 and T-005 report. Scope: source plus this status and T-006 report; no git/network/shared STATUS changes.
- Implementing items 1-5 and focused selftest extensions.

- Item 1 implemented: heading scope ends before two blank lines outside fences; equal/higher heading boundary retained. Smoke cases added, including nested independent fence.
- Item 2 implemented: strike now changes decoration only (preserves bold/italic typeface), font-metric placement and 0.1 em thickness (1.7 DIP default). Visual approval pending.
- Item 3 implemented: Enter continuation/empty exit for bullets, tasks, numbered lists; indent/newline preserved, single undo operation. Smoke cases added.
- Item 4 implemented: dedicated Look.MarkdownMarker #7E8B9B (was shared Muted #9BA9B8). Visual approval pending.
- Item 5 implemented: Quit start/closed finish, failed-save reason, show/focus and visible error status; real failure/success paths covered by selftest.
- Validation starting.

- First Release build passed (0 warnings/errors), standard output unlocked. First selftest reached all editor/carousel/focus checks but failed in the new log-reading assertion: read handle needed FileShare.ReadWrite while logger is open. Corrected the assertion handle; rerunning.

- Final validation: requested standard Release build exit 0, 0 warnings/errors; --selftest exit 0. No alternate output needed.
- Focus samples: 4.159 / 1.368 / 1.409 ms, all <100 ms (scripted internal measurement).
- Completed: report written to .coord/reports/T-006.md. User visual review remains for strike glyphs/thickness/centering and marker dimness. No blocker.
