# A15-notes-edge-pills report

1. **Summary:** Implemented the approved stack-edge notices and Win11 toast motion.
2. **Change list:** Added `EdgeNotice` and four selftest cases; updated toast animation and retired the old toast duration tokens. Toast animations cancel their predecessor to keep repeated notices visible.
3. **Behavior changes:** The first and empty newest stack edges show their requested brief notices with the existing bump; creating a note by navigation shows “New Note Created”. All toasts enter with the Win11 rise and exit with the Win11 curve. Hold intervals remain unchanged.
4. **Out-of-scope files:** None. This report and the gate report are generated task artifacts.
5. **Plan deviations:** The dedicated check build directory remains: automatic command policy rejected its removal. The initial build attempted NuGet audit access and was stopped; the successful build disabled the audit for offline use.
6. **Contracts:** No new contract rows; the four notice cases are covered by selftest.
7. **Lens review:** No independent lens review was performed. Implementer inspected the diff and toast interruption paths; visual behavior remains unverified.
8. **Checks run:** Dedicated build: 0 warnings, 0 errors; `--selftest`: SELFTEST PASS; offline gate: PASS (build, selftest, seam, tokens, format, scope and size); `git diff --check`: clean.
9. **Hand checks:** Not run as instructed. In a dev copy, check swipes at both ends, repeated overscroll, note creation, toast motion and reduced motion; verify Alt+Up/Down end bump remains unchanged.
