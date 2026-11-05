# A4e-motion-tokens

## Summary
Centralized 14 animation durations in MotionTokens, preserving each original value. The 13 scanned tween literals now use named tokens; the caret glide also uses its own token.

## Change list
| File | Literals swapped (count) | Tokens used |
| --- | ---: | --- |
| product/AntiNotepad/Motion/MotionTokens.cs | 0 | Defines all 14 duration tokens |
| product/AntiNotepad/Editor/PadView.cs | 1 | NoteArrive |
| product/AntiNotepad/Editor/Motion.cs | 1 | CaretGlide |
| product/AntiNotepad/UI/NoteSwipe.cs | 1 | SwipeSpringBack |
| product/AntiNotepad/UI/PadWindow.cs | 3 | WindowSlide, ContentReveal, WindowFade |
| product/AntiNotepad/UI/PadWindow.Keys.cs | 1 | OverlayIn |
| product/AntiNotepad/UI/PadWindow.Layout.cs | 6 | BarsReveal, PillFade, ChromeShift, ToastIn, ToastOut, Nudge |
| product/AntiNotepad/UI/SettingsWindow.cs | 1 | SettingsPage |
| tools/design-tokens-baseline.tsv | 0 | Refreshed scan baseline |

## Behavior changes
None.

## Out-of-scope files
None.

## Plan deviations
None.

## Contracts
None.

## Lens review
Worker performed the lens checklist locally. No proven findings: every changed duration maps to the same double value; no method contracts, shared mutable state, ordering, cleanup, failure paths, or layer dependencies changed. No unproven risks identified. Verdict: accept.

## Checks run
- Check build: succeeded, 0 warnings, 0 errors; check build folder deleted.
- Selftest: `DESIGN dump 8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b 16160`; `SELFTEST PASS`.
- Offline gate: Build PASS, Selftest PASS, Seam PASS, Tokens PASS, Format PASS, Scope PASS, Size PASS, Cleanup PASS; LoopLint SKIP; Offline WARN (NuGet audit disabled); `GATE: PASS`.
- Tokens inventory: tween 13 before, 0 after; no baseline category rose. CRLF verified in all changed files.
- Initial gate scope failed on three temporary inventory/output files; removed them and reran successfully.

## Hand checks for the user
In a dev build, summon and hide the pad (slide and fade), switch notes (arrive), swipe and let go (spring-back), move the pointer to the top and bottom edge (bars and pill), trigger a toast, open help (overlay), switch Settings pages, and move the caret with glide on. Confirm nothing feels different.
