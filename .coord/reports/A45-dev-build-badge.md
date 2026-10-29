## Summary
A45-dev-build-badge stamps repo Debug builds with their branch and build time and displays a compact badge that expands on hover. Missing stamps keep plain dev; Release builds carry neither new metadata key. Live UI checks remain for the user; no app was launched or stopped.

## Change list
product/AntiNotepad/AntiNotepad.csproj:30-33 | conditional branch/time AssemblyMetadata | goal 1
product/AntiNotepad/AppInfo.cs:2-3,28-43 | nullable stamp accessors and pure short/full formatter | goals 1,2,4
product/AntiNotepad/UI/PadWindow.Layout.cs:27,153,203-240 | hover hit testing, dev chip text, glyph selection and tooltip | goals 2-5
tools/dev.ps1:313 | pass current worktree branch as build property | goal 1
product/AntiNotepad/SelfTest.cs:2396-2417 | formatting and metadata absence checks | Contracts
product/AntiNotepad/SelfTest/Contracts.cs:22-27 | dev-badge-build-stamp contract | Contracts

## Behavior changes
- dev.ps1 adds AntiNotepadBranch from git branch --show-current to its existing Debug build invocation, including the helper build; launcher lifecycle unchanged.
- AntiNotepadDev=true Debug assemblies get AntiNotepadBranch and AntiNotepadBuildTime (round-trip timestamp with offset). Release and ordinary builds have no new keys. AppInfo.BuildBranch and BuildTime return null when absent/empty or unparseable.
- Dev chip defaults to glyph + leading task ID (optional letter suffix), or first 12 branch characters, plus local build time in US h:mm tt. Hover shows dev/<branch>@yyyy-MM-dd h:mm tt with feat/ removed; exit restores compact text. dev itself stays glyph + dev, with dated tooltip.
- Tooltip always supplies the full branch/date/time. Cascadia Mono/Code supplies U+E0A0 when installed with that glyph; otherwise U+2387 is used. Existing chip styling and runtime dev visibility are retained; badges accept pointer hit testing for hover.
- Missing branch or time keeps plain dev. CONTRACTS now reports 10 rows, 9 pass, 1 manual.

## Out-of-scope files
.coord/reports/A45-dev-build-badge.md: required task report.
.coord/reports/A45-dev-build-badge.gate.md: generated required gate evidence (ignored).

## Contracts
Added dev-badge-build-stamp: pure formatting for feature, optional letter, dev, other and missing stamps; non-dev metadata absence. Hover, tooltip, glyph rendering and release badge visibility require hand checks.

## Lens review
lens_reviewer verdict: accept; no proven findings. Reviewed all six files, callers and gate evidence. Unproven risks assigned to hand checks: narrow-window expansion, glyph/fallback rendering and hover while docked chrome fades/wakes.

## Checks run
Stamped Debug: Build succeeded, 0 warnings, 0 errors; SELFTEST PASS.
Release (branch property supplied): Build succeeded, 0 warnings, 0 errors; SELFTEST PASS, including absent stamp contract.
Offline gate: GATE: PASS; Build PASS (0 warnings/errors), Selftest PASS, Seam PASS, Scope PASS, Size PASS, Cleanup PASS. Offline mode disables NuGet audit.
Task check output remains: automatic tool approval rejected deletion of build/A45-dev-build-badge-check twice with "blocked by policy", including after the exact absolute target was verified. No escalation is available. Gate removed its own output successfully.

## Hand checks for the user
In the worktree dev copy, confirm glyph + A45 + build time; hover expands to dev/A45-dev-build-badge with date/time, tooltip agrees, exit contracts. Check narrow window, all bar styles, palette changes and hover while docked chrome fades/wakes; verify glyph appearance and fallback on a machine without Cascadia. A dev build without branch remains plain dev; dev branch shows glyph + dev with dated tooltip. Release has no badge. No app launch/kill/quit was performed by Codex.

## Manager verdict
Tier Low; gate PASS (build 0 warnings, selftest PASS, seam/scope/size/cleanup PASS). No further review for Low. Accepted; hand test pending.
