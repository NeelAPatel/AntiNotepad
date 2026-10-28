A41b-clipboard-policy is implemented on `reorg/A41b-clipboard-policy`. The new [ClipboardCapturePolicy.cs](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/product/AntiNotepad/Editor/ClipboardCapturePolicy.cs>) owns the raw dedupe state and formatting. The window keeps the specified capture order, and `product/AntiNotepad/UI` has no remaining `_ownClipboard` or `_lastCaptured` references.

The Debug build succeeded with **0 warnings and 0 errors**. `--selftest` passed, including `CLIPBOARD policy 12 cases` and all 12 new layer rules at 0 references. The dispatcher-post count stayed at **25**. No commit was made.

The [A41b report](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.coord/reports/A41b-clipboard-policy.md>) records the changed files, verification output, and deviation. The requested build-output cleanup remains unfinished: automatic command review rejected `Remove-Item -Recurse -Force` as “blocked by policy,” so `build/A41b-codex-check` remains. The hand check is also pending; no live app was launched. Pre-existing untracked reports were preserved.

