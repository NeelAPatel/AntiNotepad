# A27-recovered-folder-always-open

1. **Summary:** Implemented; not complete because the selftest and gate fail.
2. **Change list:**
   - `product/AntiNotepad/Platform/NotesLocation.cs:44` | guarded recovery-folder creation | After.
   - `product/AntiNotepad/UI/SettingsWindow.cs:607` | ensure when building Storage; always return Open | After.
   - `product/AntiNotepad/SelfTest/Notes.cs:15` | creation, idempotence, missing-parent checks | Test plan.
   - `product/AntiNotepad/SelfTest/Contracts.cs:25` | ensure contract and manual check | Contracts.
3. **Behavior changes:** Storage creates the recovery folder when its parent exists; Open is always shown. Existing count and unavailable handling remain.
4. **Out-of-scope files:** No protected files changed. Contracts.cs was explicitly requested under Contracts but is absent from Files; the gate flags it.
5. **Plan deviations:** Design snapshot needs investigation/update outside the listed Files. The execution policy rejected the requested check-build folder deletion; it remains under `build/A27-recovered-folder-always-open-check`.
6. **Contracts:** 33 rows, 32 pass, 1 manual; new ensure contract passed.
7. **Lens review:** Accept; no proven findings. Reviewer noted a possible concurrent parent-removal race.
8. **Checks run:** Specified build passed with 0 warnings. Selftest failed: design hash actual `24fb1170782ffea6df032804aec4fe4efbd8e4d282fecfd7394c7dde9df3a43d`, expected `7c2c76482fc04e6e4e3c933b72e6d5e42774f6a702844eb41cbf66db181ce607`. Offline gate failed Selftest and Scope; Build, Seam, Tokens, Format and Size passed. Gate retained its build/data folders on failure. See `.coord/reports/A27-recovered-folder-always-open.gate.md`.
9. **Hand checks for the user:** After the failing checks are resolved, use the slot dev copy: Settings > Storage shows Open beside Empty; clicking it opens the recovered folder.
