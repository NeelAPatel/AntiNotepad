# A34-pill-bar-per-mode-r13

## Change list
- Animate the existing Plain face brush and render the highlighted glyph over the normal glyph without adding visual nodes.
- Add settled-state checks for reduced motion, disabled buttons and replacement of the face background.

## Behavior changes
- Pointer washes use the Settings enter and exit durations and curves; press is immediate and release fades. Interruptions start at the current opacity, and disabling clears the wash.
- Highlighted icons fade on pointer or keyboard focus. Settings segment and toggle handlers replace the Plain brush; the filled Settings face continues to cover it.

## Plan deviations
- The self-test uses unattached controls, for which Anim completes immediately. It does not verify intermediate frames or exercise actual Settings segment and toggle instances; those checks remain outstanding. Live appearance and timing were not tested.
- Automatic sandbox review rejected the recursive removal of `build/A34-check`; the folder remains. The rejection did not state a more specific reason.
- STATUS was not changed: the loop assigns its item row to the item chat and says branches never edit STATUS.

## Gate result
- Build: 0 warnings, 0 errors. Self-test: PASS; design hash unchanged at `f08e0c448f3f7a400dd73fe7710bd51e05f6545782c40b3caa6ed5c24466aa79`.
- `tools/gate.ps1 -Task A34-pill-bar-per-mode -Offline`: PASS; offline NuGet audit disabled by the gate. Gate build and data folders were removed by the gate.
