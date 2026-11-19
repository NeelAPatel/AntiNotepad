# A26-tab-at-text-start-r2 report

1. **Summary:** Swapped list-level shortcuts to Ctrl+[ / Ctrl+] and removed their window-level note navigation bindings.
2. **Change list:**
   - `Editor/PadView.cs:862` | map plain Ctrl+brackets to selected list-line edits | rework key swap.
   - `UI/PadWindow.Keys.cs:89` | retain Ctrl+PageUp/PageDown navigation, release brackets to editor | rework routing.
   - `UI/HelpOverlay.cs:16` | update Notes and Writing shortcuts | rework help.
   - `SelfTest/Contracts.cs:187`, `SelfTest/Lists.cs:197` | update contract, hand check and plain-selection description | rework test plan.
   - `docs/decisions.md:25` | record bracket shortcut decision | rework scope.
   - Round 1 changes in `Editor/ListEditing.cs`, `Editor/DocumentCommands.cs` and `SelfTest/Design.cs` remain; the design hash already matches the resulting snapshot.
3. **Behavior changes:** Ctrl+] indents and Ctrl+[ outdents list lines at any caret position, including list lines touched by a selection, then renumbers. A selection containing no list lines leaves the key unhandled. Tab rules are unchanged from round 1; Ctrl+Shift+brackets retain folding. Alt+arrows and Ctrl+PageUp/PageDown retain note navigation.
4. **Out-of-scope files:** None; round 1 files remain within the card's Files list.
5. **Plan deviations:** No window-routing selftest: the handler is an instance method coupled to PadWindow state, and the existing pure checks do not exercise routed events. The first build attempted NuGet vulnerability lookup before being stopped; the successful rerun disabled audit. The requested check build directory remains: sandbox policy rejected the explicit recursive cleanup command. Gate's own task build and data were cleaned.
6. **Contracts:** `list-tab-text-start` updated. Existing list tests exercise Tab decisions and selected-line editing; key routing remains a hand check.
7. **Lens review:** No independent lens review performed; the card requires one worker and no fan-out. Source inspection confirms the window switch no longer handles brackets and the editor checks exact Ctrl modifiers; routed behavior remains unverified.
8. **Checks run:** Offline build succeeded with 0 warnings; `SELFTEST PASS` (33 contracts, 32 pass, 1 manual); offline gate PASS, including scope and format. No app UI launched.
9. **Hand checks:** In the dev slot, check Tab at marker, text start and mid-text, Shift+Tab and Ctrl+Tab; test Ctrl+[ / Ctrl+] on bullets, checkboxes, numbers, mixed multiline selections and plain text. Check Ctrl+Shift+brackets fold and Alt+arrows / Ctrl+PageUp/PageDown navigate.
