# A35d-spacing-tokens build, step 3

Role: worker. This step makes visible snaps across many files and adds a shared Fluent setup.

Read `.coord/tasks/A35d-spacing-tokens.build.md` in full. Do **step 3 only**, then write its report.

Context:
- Steps 1 and 2 are committed. The branch is rebased onto A35a-data-safety and A35b-icon-registry. Keep their logic
  intact: change only the values the snap rows name.
- Before your edits, run `--design-dump build/A35d-before.txt` from your check build. After your edits, run
  `--design-dump build/A35d-after.txt` and diff the two files. In the report, map every changed dump line to a snap
  row; a changed line with no row is a bug, so fix it.
- The approved After values are in the snap list's "After" column, and you can see them in the lab
  (`tools/playground/a35d-lab/`, where `core.js` lists every row's before and after values).
- Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A35d-spacing-tokens -Base 3b6ecb5 -Offline`.
- Section 7 of the report lists the exact hand checks: the screen, what to look at, and what it should look like.
