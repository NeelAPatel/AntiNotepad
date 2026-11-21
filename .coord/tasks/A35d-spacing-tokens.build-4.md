# A35d-spacing-tokens build, step 4

Role: quick. This step edits one doc.

Read `.coord/tasks/A35d-spacing-tokens.build.md` in full. Do **step 4 only**, then write its report.

Steps 1 to 3 are committed, so read the final token values from `product/AntiNotepad/Theme/` and do not copy them from
older notes. `Space`, the stroke tokens, the size tokens and the named exceptions all exist now. This step needs no
build. Run the offline gate only: `pwsh -NoProfile -File tools/gate.ps1 -Task A35d-spacing-tokens -Base 3b6ecb5 -Offline`.
