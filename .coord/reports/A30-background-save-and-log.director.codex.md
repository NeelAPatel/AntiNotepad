Implemented **A30-background-save-and-log** with two Luna workers.

- Release build: **0 warnings, 0 errors**
- Selftest: **SELFTEST PASS**
- Autosave caller median: **18.046 → 0.060 ms**
- Few-ms shutdown target remains unmet in the synthetic harness: **10–12 ms** median; needs a live check.

[Full report](.coord/reports/A30-background-save-and-log.md). No commits or real-data access.

Automatic approval review blocked cleanup as “blocked by policy”; both task build folders remain.