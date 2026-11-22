# A35-ux-pass stage 3: rules shared by every per-screen brief

Goal (user, via `.coord/tasks/A35-ux-pass.md`): one whole-app UX audit backed by real principles, reporting "no issue"
too. Stage 3 lists per-screen findings the user will pick from. You read code only; the item chat checks screens live.

- Role: terra. Sandbox read-only, offline. Never launch, build or kill AntiNotepad, never edit or create files, never
  commit. Your **final message is the report**.
- Principles: `.coord/research/ux-principles.md`. Cite rows by ID (N1 to N10, S1, S2, K1, U1, U2, DN1 to DN4, F1, T1, T2, H1,
  P1, D1, V1 to V6, G1, G2, R1, L1, C1 to C6, KB1 to KB4, W1). Severity scale and house rules (targets: under 24 px 🔴,
  24 to 31 🟡 when frequent, 32 passes; icon-only buttons need a tooltip with the key and an automation name) are in
  that file. **Motion is out of scope** (no timing or easing findings).
- Already covered, do not repeat: tokens and spacing (`.coord/reports/A35-ux-pass-s1.md`), icons
  (`.coord/reports/A35-ux-pass-s2.md`). Cite them by ID (I-1 etc.) if relevant.
- Evidence is file:line. Measure hit boxes from code (Width/Height/Padding of the clickable element, not the glyph).
  Tooltips: `ToolTip.SetTip`/`ToolTip.Tip`; automation names: `AutomationProperties.Name`. Keys: `UI/Shortcuts.cs`.
- **Cover every interactive control on your screens.** Keep rows short. A control with no problem gets one "no issue"
  line in the checklist table. Finish the whole list rather than going deep on a few; if you must stop, say exactly which
  controls are not covered.

## Report format (final message)
```
# A35-ux-pass-s3<x>: <screens>
## Summary            (counts by severity; top 3 findings in one line each)
## Controls checklist (table: screen | control | file:line | hit box | tooltip+key | automation name | keyboard path | result)
## Findings           (table: ID | screen | finding | principle IDs | evidence | sev | fix idea)
## No issue           (principles checked and passed, one line each)
## Not covered
## Checks run
READY: yes | no - <what is missing>
```
Finding IDs: your letter plus a number (B-1, B-2).
