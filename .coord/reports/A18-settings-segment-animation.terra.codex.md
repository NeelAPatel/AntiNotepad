Implemented the sliding highlight and hover fade in [SettingsParts.cs](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/feat/A18-settings-segment-animation/product/AntiNotepad/UI/SettingsParts.cs>), with a measured-bounds self-test and updated design snapshot.

The build has **zero warnings** and `SELFTEST PASS`. The offline gate **did not pass**: its scope check flags the already committed `tools/playground/a18-segment-lab.html`, which is referenced by the item but absent from its Files list. Format, tokens, and platform seam checks passed.

The behavior still needs a hand test. In particular, settings that immediately rebuild their rows initialize the replacement highlight at the chosen option, so their glide is cut short. The requested cleanup of the check and failed-gate build folders also remains outstanding.

Automatic approval review rejected the recursive cleanup command. The tool reported only that it was blocked by policy; it did not provide a more specific reason.