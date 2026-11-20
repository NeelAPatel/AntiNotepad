# A34-pill-bar-per-mode-r10

## Change list
- Animated shared icon-button enabled opacity with the existing pill fade token, cancellation, reduced-motion support and immediate initial state.
- Held docked chrome awake over the bar and top row, restarting sleep on pointer exit; preserved immediate typing fade.
- Changed left slot navigation to wrap and skip the right split slot, retained last-slot swap, and updated shortcut help and table labels.
- Added bar interaction checks and regression contracts, including a routed swap-button click.

## Behavior changes
- Wheel, swipe and previous/next shortcuts cycle all nine slots, including empty ones. Show stack, swap and the current dot still return to the stack.
- Clicking swap while hovering keeps both chrome areas visible; moving away starts the normal sleep timer. The fade and pointer behavior still need a hand test in the dev copy.

## Plan deviations
- Added `UI/PadWindow.cs` for the typing exception and `SelfTest/Contracts.cs` for the required bug contracts to the card's Files list. The hand-test rework supersedes the original slot-arrow decision.
- Design dump hash remains `f08e0c448f3f7a400dd73fe7710bd51e05f6545782c40b3caa6ed5c24466aa79`; `SelfTest/Design.cs` was not edited.
- Automatic approval review rejected recursive deletion of `build/A34-check` as blocked by policy; the folder remains for manager cleanup.

## Gate result
- Offline check build: 0 warnings, 0 errors; NuGet audit disabled for the offline build.
- `build/A34-check/AntiNotepad.exe --selftest`: PASS; contracts 39 pass, 1 manual.
- `tools/gate.ps1 -Task A34-pill-bar-per-mode -Offline`: PASS, including Build, Selftest, Seam, Tokens, Format, Scope and Size.
