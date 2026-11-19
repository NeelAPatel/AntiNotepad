# A26-tab-at-text-start report

1. **Summary:** Stopped with partial implementation: the Findings incorrectly say Alt+Shift+Left/Right is unbound. Window navigation intercepts it before the editor.
2. **Change list:**
   - `Editor/ListEditing.cs:23` | text-start and Tab decision | Plan: data shape and After.
   - `Editor/DocumentCommands.cs:23`, `Editor/PadView.cs:862` | list edits and key mapping | Plan: After and selection rule; arrow routing is unreachable.
   - `UI/HelpOverlay.cs:19` | shortcut help | Plan: mitigation.
   - `SelfTest/Lists.cs:37`, `SelfTest/Contracts.cs:187` | decisions, selected line edits, contract | Plan: test plan and Contracts.
   - `SelfTest/Design.cs:22` | update help text design hash | Plan: help line and selftest gate.
3. **Behavior changes:** Empty-selection list Tab follows the new decision; Ctrl+Tab inserts spaces (Word behavior assumed). Shift+Tab mid-text is swallowed. Selected Tab retains editor handling. The intended Alt+Shift+arrows do not work: window navigation wins.
4. **Out-of-scope files:** `SelfTest/Design.cs` updated because the planned help text changes the design snapshot. Gate also reports four listed files outside scope because the card abbreviates repeated paths.
5. **Plan deviations:** Stopped per brief on incorrect Findings; window key routing requires an amended plan and touches the explicitly excluded PadWindow files. No hand test performed.
6. **Contracts:** `list-tab-text-start` added; its arrow promise is not satisfied through window routing.
7. **Lens review:** Rework. `UI/PadWindow.cs:105` registers tunnel handler; `UI/PadWindow.Keys.cs:66` intercepts Alt+Shift+Left/Right for navigation before `PadView` receives it. Existing selftests do not exercise routing.
8. **Checks run:** Build succeeded with 0 warnings; `SELFTEST PASS` (33 contract rows, 32 pass, 1 manual); `git diff --check` passed. Offline gate failed Scope: five files, including Design.cs and four abbreviated card paths; build, selftest, seam, tokens, format and size passed. Offline audit disabled. Gate build/data and requested check build removed.
9. **Hand checks:** Deferred until routing and scope are resolved; then check Tab at marker, text start and mid-text, Shift+Tab, Ctrl+Tab and Alt+Shift+arrows with all list types and a multiline selection.
