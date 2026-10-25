Implemented A26-allow-windows-shutdown: session end saves once without prompts and permits both resident windows to close. Normal close behavior remains.

Release build: **0 warnings, 0 errors**. Selftests: **SELFTEST PASS**.

[Report and manager verification probe](.coord/reports/A26-allow-windows-shutdown.md). Live shutdown timing remains unverified; stalled storage has no hard deadline.

Automatic approval review blocked cleanup as “blocked by policy”; `build/a26-check` remains.