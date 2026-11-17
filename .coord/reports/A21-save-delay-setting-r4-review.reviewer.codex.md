Wrote the [review report](.coord/reports/A21-save-delay-setting-r4.review.md).

**VERDICT: rework**

Proven findings:

- Deleted text returns after clearing a new note during its first queued save.
- Switching focus between split notes misses the immediate save.
- Clock changes can stall label refresh for an hour or longer.

Build and existing selftests pass. Product files unchanged; temporary probes removed.