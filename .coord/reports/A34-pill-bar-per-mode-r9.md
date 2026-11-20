# A34-pill-bar-per-mode-r9

## Change list
- Shared icon buttons dim their face and glyph with a theme token when disabled, suppress disabled hover feedback, and restore enabled appearance. Added an opacity transition capability check.
- A palette-based shadow helper supplies the floating bar, pager, toast, and save-failure card independently.
- Added the requested bar summaries and Kit file header.

## Behavior changes
Disabled icon buttons visibly dim; disabled hover color and wash are suppressed. The save-failure card retains its shadow in Float and Quiet. Enabled icon appearance and pill/toast shadow values are preserved. Visual hand testing remains outstanding.

## Plan deviations
The requested design hash remains unchanged in SelfTest/Design.cs. The design dump changed from 6693fbbd9f7f302e0a6b4ffc2de92499d15515a8cfed556b532f96dadc3a96ee to f08e0c448f3f7a400dd73fe7710bd51e05f6545782c40b3caa6ed5c24466aa79 after these fixes; the pinned check consequently fails. The new opacity check verifies effective visual opacity through control properties, not a pixel capture.

## Gate result
- Offline build: PASS, 0 warnings and 0 errors.
- Selftest: FAIL solely on the pinned design hash; 36 contracts pass, one manual. New bar capability check passes.
- Offline gate: FAIL on the same selftest hash; build, seam, tokens, format, scope, and size pass. Gate retained its build/data folders on failure.
- Cleanup of build/A34-check was blocked by automatic approval policy; the folder remains.
