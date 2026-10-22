**M8a-quiet-variant-renders stopped after two capture attempts.** GDI `BitBlt` from the screen DC returned Windows error 5 (access denied), so the glass check and variant renders could not run. I reverted the renderer and dispatch source edits and wrote the required failure marker at [.coord/scratch/M8a/failed](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.coord/scratch/M8a/failed>). The requested report and render files were not produced.

Before the render gate, the Release build passed with 0 warnings and 0 errors, and `--selftest` passed, including Quiet/Card conformance, boundary, and pixel proofs.

