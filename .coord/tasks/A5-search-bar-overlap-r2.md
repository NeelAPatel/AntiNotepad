# A5-search-bar-overlap-r2: fix two format problems on changed lines

Rework round of `A5-search-bar-overlap` (`docs/loop.md`). Read the item card `.coord/tasks/A5-search-bar-overlap.md` first: its Plan, Files,
Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Medium, same as the card (bug-unproven-cause)
Role: director, because the same run owner as round 1 (`.coord/tools/codex-run.sh director`)

## Why this round
Triage cause: `code`. Round 1 gate (online) failed only on Format:
- `product/AntiNotepad/UI/PadWindow.Layout.cs:101 WHITESPACE` and `:102 WHITESPACE` (the `AddOverlayDragBand` object initializer: two properties share a line, `.editorconfig` wants one per line).
Everything else passed (build 0 warnings, selftest PASS, seam, tokens, scope). Lens review of round 1: accept.

## Change to the plan
None.

## Files
Files added for this round only (already in the card, listed so the gate reads a list):
- `product/AntiNotepad/UI/PadWindow.Layout.cs`

## Done means
- The `AddOverlayDragBand` initializer is formatted so `dotnet format --verify-no-changes --include` reports nothing for the changed lines; no behavior change.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A5-search-bar-overlap-r2 -Offline` passes (Size WARN on Layout is accepted).
- Do not spawn lens review again; the change is whitespace only. Do not delete build folders (policy blocks it); leave them.

Report: `.coord/reports/A5-search-bar-overlap-r2.md`, in the card's report format (short; "None" where true).
