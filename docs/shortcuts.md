# Keyboard shortcuts

The configurable table lives in `product/AntiNotepad/UI/Shortcuts.cs`. The Settings → Shortcuts page records in-window keys, clears a binding, resets one action, or resets the full table. A blank binding has no key; the action remains available to bind later. Help toggles with F1 or its assigned key. Checkbox or timer has no default key, and remains bindable. Recording uses exact modifiers. If another action in the same scope owns the key, Settings asks before moving it.

<!-- shortcuts:table:start -->
| Action | Section | Scope | Default |
|---|---|---|---|
| New note | Notes | Pad | Ctrl+N |
| Next note | Notes | Pad | Alt+Right |
| Previous note | Notes | Pad | Alt+Left |
| Next note (alias) | Notes | Pad | Unassigned (bindable) |
| Previous note (alias) | Notes | Pad | Unassigned (bindable) |
| First note | Notes | Pad | Unassigned (bindable) |
| Latest note | Notes | Pad | Unassigned (bindable) |
| Move note earlier | Notes | Pad | Unassigned (bindable) |
| Move note later | Notes | Pad | Unassigned (bindable) |
| Delete note | Notes | Pad | Unassigned (bindable) |
| Search notes | Search | Pad | Ctrl+F |
| Search notes (P alias) | Search | Pad | Unassigned (bindable) |
| Search notes (K alias) | Search | Pad | Unassigned (bindable) |
| Settings | Window | Pad | Unassigned (bindable) |
| Help | Window | Pad | F1 |
| Open slot 1–9 | Slots | Pad | Unassigned (bindable) |
| Keep note in slot 1–9 | Slots | Pad | Unassigned (bindable) |
| Back to the stack | Slots | Pad | Unassigned (bindable) |
| Swap stack and slots | Slots | Pad | Unassigned (bindable) |
| Split view | Slots | Pad | Ctrl+\ |
| Keep on top | Window | Pad | Ctrl+T |
| Text size up | Writing | Pad | Ctrl++ |
| Text size down | Writing | Pad | Ctrl+- |
| Copy the whole note | Writing | Pad | Ctrl+Shift+C |
| Read text from a screen snip | Writing | Pad | Ctrl+Shift+O |
| Checkbox or timer | Writing | Pad | Unassigned (bindable) |
| Fold at caret | Writing | Pad | Ctrl+Shift+[ |
| Unfold at caret | Writing | Pad | Ctrl+Shift+] |
| Quit | Window | Pad | Ctrl+Shift+Q |
| Grid view (in search) | Search | Search | Unassigned (bindable) |
| Trash (in search) | Search | Search | Unassigned (bindable) |
<!-- shortcuts:table:end -->

`Open slot 1–9` and `Keep note in slot 1–9` each use one recorded digit as a family: the same modifier chord works with 1 through 9 on the number row or numpad. The global show/hide and optional new-note hotkeys remain in their existing Settings rows and are not part of this table.

## Fixed keys

- Esc closes an overlay or hides the pad.
- Enter continues a list or accepts a completion; Tab and Shift+Tab indent list items at text start.
- Ctrl+[ and Ctrl+] change list level; Ctrl+V pastes; Ctrl+Z and Ctrl+Y undo and redo.
- Shift+Backspace deletes a character; Shift+Delete cuts; Shift+Insert pastes; Ctrl+Insert copies; Ctrl+Shift+Z redoes.
- Ctrl+Home scrolls to the top of a note; Ctrl+click opens a link.
- The global show/hide hotkey is a separate setting. Development copies reserve Ctrl+Alt+Shift+1–9 and Ctrl+Alt+Shift+F1–F9; Ctrl+Alt+Shift+S remains the default global show/hide chord.

Recording refuses text-entry and caret keys, Windows keys, fixed editor keys, the development chords above, and the active global hotkeys. A shortcut is matched only when its modifiers match exactly. Search actions apply only while Search is open.
