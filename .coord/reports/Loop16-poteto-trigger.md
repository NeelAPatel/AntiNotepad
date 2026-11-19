# Loop16-poteto-trigger Codex report

## Summary
Implemented the offline brief and first-turn hook gate. Live harness behavior remains for the item chat to verify.

## Change list
- `tools/poteto-gate.ps1`: Brief and Stop modes, `-LogOnly`, logging, and subprocess self-tests.

## Behavior
Brief rejects malformed item-chat templates. Stop accepts the Skill call or fenced fallback in the first turn, honors the active guard, and ignores later turns. Errors and unreadable transcripts fail open. Log-only records verdicts without blocking. Point both user-scope hooks at this script, supplying `-Mode Brief` or `-Mode Stop` and initially `-LogOnly`; the log is under the hook's `CLAUDE_PROJECT_DIR/build` (or this script's checkout if unset).

## Out-of-scope files
The item chat owns playbook and template edits, settings wiring, and live checks. The gate generated `.coord/reports/Loop16-poteto-trigger.gate.md`.

## Deviations
Added `-LogOnly` to support the approved rollout. The log records the launch text when observed; a Skill tool call alone is sufficient for the Stop verdict as specified. No live checks were run.

## Checks run
- `pwsh -NoProfile -File tools/poteto-gate.ps1 -SelfTest`: PASS, including malformed input and unreadable transcript fail-open.
- `pwsh -NoProfile -File tools/loop-lint.ps1`: PASS; existing budget warnings for loop.md, lessons.md, and the item card.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop16-poteto-trigger -Offline`: PASS; same budget warnings, NuGet audit disabled for offline build. The script was adjusted during the gate run; its final version passed its independent self-test after that adjustment.
