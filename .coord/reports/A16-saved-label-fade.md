# A16-saved-label-fade report

1. **Summary:** Implemented directional saved-label motion. Hand verification and a passing strict gate remain outstanding.
2. **Change list:** Retained the label across chrome refreshes, connected note navigation direction, and added a direction-offset selftest.
3. **Behavior changes:** Changed note labels exit and enter in page direction using existing Windows motion tokens; edits, equal text, and reduced motion do not start a directional transition. Interrupted transitions retain one label.
4. **Out-of-scope files:** None under the card's `SelfTest/*` wildcard. The gate treats `SelfTest/Run.cs` as outside scope because it matches paths literally.
5. **Plan deviations:** Strict gate failed on that wildcard mismatch. The task card was left unchanged as instructed.
6. **Contracts:** None new; selftest reports 18 rows, 17 pass and 1 manual.
7. **Lens review:** Implementer self-review against lens checklist; no proven product findings. Independent review not performed. Live animation and rapid interruption remain unverified.
8. **Checks run:** Offline build: 0 warnings, 0 errors. `--selftest`: SELFTEST PASS. Offline gate: build, selftest, seam, tokens and format pass; scope fails on literal wildcard matching; size warning for Layout.cs. Temporary build and gate folders removed.
9. **Hand checks:** Pending user: next/previous direction, rapid switching without ghost text, equal labels, typing, and reduced motion.
