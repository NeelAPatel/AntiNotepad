A30-save-unavailable-fix completed via gpt-6-luna xhigh.

- Unavailable saves succeed without writes and retain `LastError`.
- Folder recovery drains pending work without blocking on earlier failures.
- Build: 0 warnings, 0 errors. `SELFTEST PASS`.
- Rework report appended. No branch operations, commits or GUI testing.

Automatic approval review rejected cleanup as “blocked by policy”; `build/a30-check` and `build/A30-measure` remain.