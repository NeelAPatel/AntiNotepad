## Summary
Each PadView now sits inside a stationary, zero-inset Border host in its existing split column. Split view clips moving content at each host's bounds; single view disables left-host clipping and hides the right host with the right pane.

## Change list
file:line-range | what changed | which Plan line it serves
--- | --- | ---
product/AntiNotepad/UI/PadWindow.Layout.cs:32 | stores the two pane hosts | After: stationary clipping hosts
product/AntiNotepad/UI/PadWindow.Layout.cs:68-83 | production host construction and split-state application | After; right host follows split; no host padding/background
product/AntiNotepad/UI/PadWindow.Layout.cs:112-113 | builds panes through the shared layout helpers | After; production layout exercised by selftest
product/AntiNotepad/UI/PadWindow.Navigation.cs:31 | applies host state when binding panes | After: split visibility stays synchronized
product/AntiNotepad/SelfTest.cs:299-328 | structural clipping regression check | Test plan, code
product/AntiNotepad/SelfTest/Contracts.cs:75-80 | adds split-pane-clipping | Contracts

## Behavior changes
- New per-window left/right host references retain stationary Borders around the existing PadViews; transforms still target the PadViews.
- New internal AddPaneHosts constructs the three-column child hierarchy and returns its hosts. ApplyPaneSplit sets the existing column definitions and pane/divider visibility, additionally synchronizing right-host visibility and enabling left-host clipping only in split view. BuildLayout and BindPanes call these helpers.
- Split panes now crop text, caret, selection and decorations at their column bounds. The right host hides in single view; the left host remains unclipped there.
- No public method signatures, motion timings, distances, fades, settings defaults, note bindings or focus targets changed. Top blur and MotionPreview source remain untouched.

## Out-of-scope files
This required report, the fresh .coord/reports/A55-split-view-clip.lens.md review, and the generated .coord/reports/A55-split-view-clip.gate.md are task metadata outside the four product paths. The pre-existing untracked A55-split-view-clip-audit.director.codex.md was left untouched.

## Plan deviations
Implementation matches the Plan. Pixel containment remains a manual check; structural containment is tested through production layout helpers. Required check-build deletion was attempted with an exact resolved-path check and a reparse-point guard, but automatic approval review rejected it with "blocked by policy"; build/A55-split-view-clip-check remains for manager cleanup. Gate-owned build/data cleanup passed.

## Contracts
Added split-pane-clipping with structural checks and a manual pixel-containment check. Existing split-divider-above-pager is unchanged.

## Lens review
Fresh lens_reviewer: VERDICT: accept; no proven findings. Behavior list matches the diff; host/transform coupling, split visibility, focus and blur consumers remain consistent. Full checklist and relevant callers reviewed; git diff --check passed. Review: .coord/reports/A55-split-view-clip.lens.md. Pixel containment and blur appearance remain for the planned hand checks.

## Checks run
- Director build to build/A55-split-view-clip-check, AVALONIA_TELEMETRY_OPTOUT=1, NuGetAudit=false: 0 warnings, 0 errors.
- Check build --selftest: SELFTEST PASS; CONTRACTS: 11 rows, 10 pass, 1 manual. Both split-pane-clipping and split-divider-above-pager structural checks pass. Pixel containment remains manual.
- pwsh -NoProfile -File tools/gate.ps1 -Task A55-split-view-clip -Offline: GATE: PASS; build 0 warnings/errors, selftest exit 0, LAYERS 0 refs/violations, PLATFORM-SEAM 0 violations, scope 4 allowed files, cleanup PASS.
- Gate warnings: NuGet audit disabled for offline build; SelfTest.cs 2950 -> 2980 lines (+30, exceeding the advisory +25 threshold). The focused structural regression check accounts for the growth. Manager online gate remains required.
- git diff --check: PASS. Check-build cleanup rejected by automatic approval review (blocked by policy); folder remains. No app launched or killed.

## Hand checks for the user
Not performed; no app launched or killed. In the dev copy:
0. Repeat screenshots 1 and 2 with the same notes; both sides should match screenshot 3's crop.
1. Pull the right pane toward the divider using touchpad and Shift+wheel, then let it spring back; repeat for the left pane toward the right. No content crosses the divider.
2. Commit a swipe in each pane and change notes with the keyboard; arrival motion stays the same and crops at the divider.
3. Pull with a caret and selection near the inner edge; both stay cropped.
4. Scroll up/down in each pane; nothing crosses.
5. Toggle split off/on and resize; check right pane, focus, divider, pager and revealed top blur.
