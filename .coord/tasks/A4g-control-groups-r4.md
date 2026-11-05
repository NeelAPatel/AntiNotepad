# A4g-control-groups-r4: narrow finish of the control map

Rework round of `A4g-control-groups` (`docs/loop.md`). Read the card `.coord/tasks/A4g-control-groups.md` and `-r3.md` first.
Part 1 only, report only, no product file changes. Approved by the user 2026-11-03: "Narrow round 4". Do not expand scope
to new sites; work only from rows already in the map and the source files they cite.

Tier: Medium
Role: terra, same as earlier rounds

## Why this round
Round 3 left the map partial. Four narrow fixes, nothing else:
1. Replace each placeholder "**Disagreements:** See the instance values..." line with a concrete list built from that
   group's own table (value X in file:method vs value Y in file:method). Delete the old prose paragraphs under each table
   once their facts are in the table or the list.
2. Fold the "Additional inspected instances" rows into their groups (preview tiles -> a new "Preview tile" group, or Card;
   palette chips and accent dots -> a new "Swatch" group; ResultRenderer chip -> Badge) and delete that section.
3. Rewrite "Proposed group tokens" to ONE row per group, naming the shared properties (radius, font, fill, border,
   padding). No per-instance variants such as Field.SearchRadius / PathRadius. Add a column "Instances disagree?" marking
   where a group token would change a rendered value (those wait for design-questions, not Part 2).
4. Title: remove "(draft)". Make "Gaps" an honest, short list of what is genuinely not covered. Keep the ChromeTokens alias
   table as is (Check verified it).

## Change to the plan
None.

## Files
- `.coord/reports/A4g-control-groups-map.md`

## Done means
- All four fixes done; no product file changed.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A4g-control-groups-r4 -Base feat/A4-design-tokens -Offline` passes.

Report: `.coord/reports/A4g-control-groups-r4.md`, in the card's report format. Write the map as far as you got before reporting.
