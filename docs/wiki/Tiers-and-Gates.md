**In short**

Risk decides how much checking an item needs. Every item gets automatic checks and a comparison with the approved plan. More sensitive changes also get independent reviews; visible changes get a hand test (`docs/loop.md`, “Tiers” and “Gates menu”).

# Tiers and Gates

## Pick checks before building

The master sets the tier at Plan; Findings can raise it. When unsure, use the higher tier. A bug without a proven cause is at least Medium. Without reproduction, stop and ask instead of building a guess (`docs/loop.md`, “2. Findings” and “Tiers”).

| Tier | Typical change | Findings | Minimum checks |
|---|---|---|---|
| Low | UI text, theme tokens, docs/tools, Calc rules with selftests, pure moves with LAYERS rows | Master's short scout | Gate; hand test if visible |
| Medium | Editor features, Settings sections, new settings, shared state, unproven bug cause | Codex audit | Gate, fresh Codex review; hand test if visible |
| High | Store/notes, startup/quit, platform window styles, timers/deferred work, saving, memory | Codex audit | Gate, fresh Codex review, design review, hand test always |

These are the defaults in `docs/loop.md`, “Tiers.” The Plan names what each check catches for this item; it can add checks but cannot drop tier defaults. Every bug fix adds a CONTRACTS row and needs reproduction evidence. Design changes need a lab preview approved by the user (`docs/loop.md`, “Gates menu”; `AGENTS.md`, “Rules”).

## What the gate checks

Run `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug>` in the item checkout. It builds and runs headless selftests, separately from opening the dev app (`tools/gate.ps1`; `docs/dev-loop.md`, “Checks”).

| Step | What it catches | Source |
|---|---|---|
| Build | Requires zero warnings and errors | `tools/gate.ps1`, build result handling |
| Selftest | Requires exit 0 and SELFTEST PASS; includes LAYERS dependency rules and CONTRACTS promises | `tools/gate.ps1`, selftest handling; `product/AntiNotepad/SelfTest/Contracts.cs` |
| Seam | Platform calls crossing the intended boundary | `tools/check-platform-seam.ps1` |
| Format | Problems on changed lines in changed product C# files | `tools/gate.ps1`, Format; `.editorconfig` |
| Scope | Changed files outside exact Files entries on the card and round briefs, excluding `.coord/` paths; untracked files count | `tools/gate.ps1`, Get-ChangedFiles and Scope |
| LoopLint | Inconsistent live loop instructions and broken file references | `tools/loop-lint.ps1`; `tools/loop-lint.allow` |
| Size | Warns at size/growth thresholds; defaults 600 lines and 25 lines of growth | `tools/gate.ps1`, Size |

Scope fails on item branches and warns on dev, in CI, or with -LooseScope. `.coord/` paths are excluded from the changed-file scope set. The report records `Head:` and marks tracked changes other than STATUS as dirty; a PASS describes that tested state, not later edits (`tools/gate.ps1`, Get-ChangedFiles, Scope and Write-GateReport). Merge preflight checks that evidence (`tools/merge-preflight.ps1`).

Gate reports are regenerated and not committed; result lines go on the card. The Codex card template uses -Offline to disable NuGet audit for that sandbox build; the manager repeats the gate online. CI is manually triggered and skips Format (`docs/loop.md`, “Rules”; `.coord/templates/item.md`, “For Codex”; `.github/workflows/gate.yml`; `tools/gate.ps1`, Format).

## Judgment after scripts

The Check session compares Request, Plan/Approve, and the report and diff. Each difference is “within plan,” “justified,” or “ask the user.” Missing behavior-change entries, broken Must not change promises, blockers, and major review findings require triage. Side issues become proposals (`.claude/skills/review-task/SKILL.md`, steps 1–4a).

The in-run lens reviewer and fresh Codex review are separate checks. Their proof rule requires a failing check, a probe, or an exact failure path with file:line; speculation stays under Unproven risks (`.agents/skills/lens-review/SKILL.md`, “Proof rule”; `docs/agents.md`, “Briefs”; `.claude/skills/review-task/SKILL.md`, step 3). See [Who Does What](Who-Does-What).

## Sources

- `docs/loop.md` — Findings, Plan, Gates menu, Tiers, Rules.
- `AGENTS.md` — lab-first design and capability selftests.
- `tools/gate.ps1`, `tools/merge-preflight.ps1` — gate evidence and merge checks.
- `tools/check-platform-seam.ps1`, `tools/loop-lint.ps1`, `tools/loop-lint.allow`, `.editorconfig` — checks and exceptions.
- `product/AntiNotepad/SelfTest/Contracts.cs` — cross-feature promises.
- `.claude/skills/review-task/SKILL.md`, `.agents/skills/lens-review/SKILL.md` — review and proof rules.
- `.coord/templates/item.md`, `docs/agents.md`, `docs/dev-loop.md`, `.github/workflows/gate.yml` — sandbox, routing, app separation, manual CI.
