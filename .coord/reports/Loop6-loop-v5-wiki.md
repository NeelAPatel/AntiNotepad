# Loop6-loop-v5-wiki

## Summary
Loop version and wiki refreshed to v5.0.0.

## Change list
- Defined major, minor and chat-output patch increments; added the version to the hook and glossary.
- Updated wiki guidance for roles, fast lane, paused items, gate evidence, format baseline, masters and reply style.
- Copied the original flow diagram to the v5 flow and updated the named lanes, bubbles and routes. The small branch diagrams remain applicable and the historical flow remains linked from history.

## Plan deviations
- None to the approved file scope. The sidebar and small diagrams needed no changes.

## Checks run
- Hook with an isolated temporary root: starts `AntiNotepad loop v5.0.0:`; isolated root avoids starting or probing the roadmap server.
- New SVG parsed as XML; version search found earlier versions only in historical material.
- `git diff --check`: pass.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop6-loop-v5-wiki -Offline`: PASS, including scope, format and loop-lint; selftest has one manual contract.

## Hand checks
- Builder: open the v5 flow at full scale and check text, clipping and routes. This offline run did not open a browser or inspect the screen.
- User: review the diagram. Wiki publication waits until after Merge and the user's go.
