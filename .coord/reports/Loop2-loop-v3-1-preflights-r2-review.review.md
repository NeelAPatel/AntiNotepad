# Loop2-loop-v3-1-preflights-r2-review lens review

## Findings (proven)
1. [minor] `tools/merge-preflight.ps1:20` (identical expression at `tools/approve.ps1:24`) - The shared placeholder rule still accepts the real template's multiline Check placeholder (`.coord/templates/item.md:78-80`). Its `<` starts line 78 and closing `>` ends line 80; `[^>\r\n]+` prevents matching across those lines. Extracting and executing the actual helper returned `False` for that section. This does not currently produce a merge false PASS: the separate Check guard rejects the template's `ask the user` line. Smallest fix: recognize a whole placeholder token spanning lines while retaining the start-of-line/optional-label and closing-token boundaries; probe the real Check section through the helper as well as the phase loop.

## Behavior changes check
Report's list vs the code: matches for single-line placeholder tokens, empty sections, optional bullets/bold labels, later `<` characters and inline code. The blanket claim about placeholder values needs the multiline qualification in finding 1.

Both helper definitions are identical. Callers are approve's Approve check (`tools/approve.ps1:50`) and merge's five-phase loop (`tools/merge-preflight.ps1:42`). Missing-section checks and the subsequent Check guard remain intact. No new shared state, timing, cleanup, product dependencies or exception paths are introduced by the helper. No additional behavioral change was found in the surrounding code.

## Scope and plan check
In plan: the helper and its two call sites serve the R2 placeholder rule. No product changes or unrelated implementation changes were identified. Both scripts are untracked, so there is no Git R2 baseline from which to independently prove that every other byte stayed unchanged; this review compares current code with the briefs, prior finding and implementer report.

The actual Loop2-loop-v3-1 card has filled Request, Plan, Approve and Hand test sections, all accepted by the helper. Its Check is genuinely unfinished (`<Filled per step and at the end.>`) and correctly rejected. That refusal is expected, not a false FAIL.

## Unproven risks
- None beyond the minor helper gap above; the real template Check is rejected by the complete validation loop.

## Checks run
- Read the skill, STATUS, item card, round/step briefs, prior review, implementer report, template, coding rules and loop context; read both scripts in full.
- Parsed both scripts with the PowerShell AST parser: no parse errors; extracted helper definitions compare identically.
- In-memory helper probes: 20 explicit cases passed for empty/whitespace, standalone/bulleted/labeled placeholders, LF/CRLF, trailing words, filled text, later `<`, inline code and `<prefix>/x` token boundaries. All seven real Plan placeholder lines rejected individually.
- Real template helper results: Request/Plan/Approve/Hand test rejected; multiline Check accepted (finding 1). Actual card helper results: only unfinished Check rejected.
- Executed the actual extracted merge phase-validation loop, retaining each real template phase in turn and filling the other four: Request/Plan/Approve/Hand test failed as unfilled; Check failed with `open ask the user line`. No false PASS on these real template cards.
- Same actual loop with all five phases filled, including later `<` and inline-code paths: PASS.
- `git diff --check`: exit 0 (STATUS line-ending warning only); branch remains `chore/Loop2-loop-v3-1`.
- Existing gate report says `GATE: PASS`, Head `ac450d2ca41f41c2095a0b7b624c8635658dc9d8-dirty`. This is existing evidence; no build/gate or mutating clone probes rerun. All review probes ran in memory; only this report was written.

VERDICT: accept
