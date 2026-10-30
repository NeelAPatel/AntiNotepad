# Loop2-loop-v3-1-fix-loop-scripts

## Summary
Moved overlap and catch-up into tools/ and enabled v3.1 item branches and main-checkout operation.
The requested clone probes and offline gate passed; no product changes.

## Change list
| File:lines | Change | Plan |
|---|---|---|
| tools/files-overlap.ps1:1-30 | v3.1 header, shared prefix list, complete hot-file defaults | After 1, 2, 3, 5 |
| tools/files-overlap.ps1:33-59 | Scan every matching item checkout, including main; warn for hot-file patterns | After 3 |
| tools/catch-up.ps1:1-33 | v3.1 header, prefixes, hot files; allow only STATUS tracked changes and ignore untracked files | After 1, 2, 4, 5 |
| tools/catch-up.ps1:35-52 | Capture Git refusal messages, preserve CLEAN and open CONFLICT results; mark hot patterns | After 4 |
| tools/v2.2/files-overlap.ps1, tools/v2.2/catch-up.ps1 | Removed old locations | After 1 |

## Behavior changes
- Overlap accepts feat/fix/refactor/docs/chore item branches, excludes the requested task, and reads each checkout's own card. Shared plain files still exit 1; hot files warn only, with wildcard matches for selftest parts and skill files.
- Catch-up requires the matching task on one of those prefixes. Tracked changes outside STATUS refuse; untracked files do not block. Dirty STATUS is preserved when Git can merge; Git's own refusal is printed with its message and exits 1.
- Fetch failures now refuse rather than attempting a merge against stale refs. A failed merge without unmerged paths refuses instead of reporting a zero-file conflict. True conflicts remain open, marked with the same hot patterns, exit 2. No rebase or push.
- Existing HotFiles override and Base parameters remain available.

## Out-of-scope files
This requested report and the generated ignored .coord/reports/Loop2-loop-v3-1.gate.md. Temporary fixture files were confined to build/Loop2-loop-v3-1-probe and deleted after probing; the localhost daemon was stopped. The empty tools/v2.2 folder was removed.
Existing STATUS edits, history-preview.md and the step brief were left untouched.

## Plan deviations
Local-path Git clone/fetch hit the Windows sandbox's sh.exe CreateFileMapping access-denied error. The fixture clone and fetch used a localhost-only Git daemon serving the throwaway seed instead. No external remote was contacted by the probes.

## Contracts
None added or changed (no product code). Script capability checks below exercise the requested behavior.

## Lens review
Fresh lens_reviewer verdict: accept; no proven findings or unproven risks. The reviewer independently ran overlap, checked all five branch prefixes plus wrong-task/dev/extra-suffix rejection, checked diff whitespace, and reviewed the gate and probe evidence. No fixes required.

## Checks run
- Real checkout: files-overlap.ps1 -Task Loop2-loop-v3-1 => FILES-OVERLAP: PASS (0 other items, 0 hot warnings), exit 0.
- Throwaway seed and clone under build/Loop2-loop-v3-1-probe, with paths containing spaces:
  - fix/T9-a in main and chore/T9-b in linked checkout: shared plain.txt FAIL, all six hot examples WARN, exit 1.
  - Reverse scan from linked checkout finds main item: same FAIL/WARN, exit 1.
  - Hot-only overlap: PASS, six warnings, exit 0.
  - UP TO DATE with dirty STATUS and an untracked file: exit 0.
  - Wrong task and unsupported dev branch: REFUSED, exit 1 each.
  - Dirty tracked plain.txt: REFUSED, exit 1.
  - Incoming incoming.txt: CLEAN, incoming path listed, dirty STATUS retained, untracked file allowed, exit 0.
  - Incoming STATUS colliding with dirty STATUS: REFUSED with Git's overwritten-by-merge message, exit 1; no MERGE_HEAD.
  - Conflicting changes in Contracts.cs, SelfTest.cs, SelfTest/Probe.cs, docs/loop.md, docs/glossary.md and .claude/skills/probe/SKILL.md: CONFLICT, all six marked [hot], exit 2; MERGE_HEAD exists in clone.
- gate.ps1 -Task Loop2-loop-v3-1 -Offline => GATE: PASS; build 0 warnings/0 errors, SELFTEST PASS, CONTRACTS 13 rows (12 pass, 1 manual), seam/format/scope/size/cleanup PASS. Offline NuGet audit warning expected.
- git diff --check: PASS.

## Hand checks
None (not visible).
