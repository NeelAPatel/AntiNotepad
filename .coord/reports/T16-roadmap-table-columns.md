# T16-roadmap-table-columns — Codex report

Implemented the expanded Plan table: collapsible remembered groups, release marker suppression, Entered timestamps from one chronological Git patch pass, per-column filters, a saved column picker, and saved resizable widths. ID and Name stay visible. Changes are limited to the two requested implementation files; this report is the requested delivery artifact.

Validation: offline task gate PASS (build, selftest, scope and other checks); roadmap-data.ps1 -Check PASS; extracted JavaScript passes node --check; git diff --check PASS. On this checkout, 145 of 151 data rows have matching committed entry timestamps. Browser hand testing remains for the item chat/user. The pre-existing deletion of .coord/reports/T16-roadmap-table-columns.terra.codex.md was left untouched.
