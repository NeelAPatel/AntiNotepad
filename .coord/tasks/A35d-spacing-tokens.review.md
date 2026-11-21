# A35d-spacing-tokens: fresh lens review (read-only)

Role: reviewer. This is the Medium-tier Codex review of the finished product diff.

Review the diff `git diff 3b6ecb5 3d3d93f -- product tools`. Read `AGENTS.md`, `.claude/rules/coding.md`, the item card
`.coord/tasks/A35d-spacing-tokens.md`, the snap list `.coord/reports/A35d-spacing-tokens-snaps.md`, and the step reports
`.coord/reports/A35d-spacing-tokens-build-1.md`, `-build-2.md` and `-build-3.md`.

Find, with evidence (`file:line` and why it is wrong):
1. **Value drift.** A literal replaced by a token whose value differs, outside snap rows 1-19. A derive that gives a
   different number than before.
2. **Wrong snaps.** A snap row applied to the wrong site, or with the wrong value, or a row that is missing.
3. **Lost logic.** A35a-data-safety and A35b-icon-registry logic that this branch changed beyond values. Run
   `python -I .coord/tools/a35-survival.py 3d3d93f` and judge each line it lists.
4. **The Fluent corner setup.** The app and both selftest setups get the same resources. Nothing runs twice in the app.
   No control that sets its own `Shape` radius is overridden.
5. **The scanner.** Fixtures are real hit and no-hit cases. No category got blind to real literals, for example a
   nonzero value next to a zero.
6. **The coding standard on touched files:** file headers, summaries, no history in comments, token names that read well.

Do not edit any file. Report `.coord/reports/A35d-spacing-tokens-review.md`: verdict (pass, or fixes needed), then
findings ranked by severity, each with evidence and a one-line fix. Write "None" for a section with no finding.
